// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.SharedKernel.SourceGenerators.Tests;

using System;
using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

public sealed class MetadataSourceGeneratorTests
{
    [Fact]
    public void Generator_WhenNoEntitiesPresent_EmitsNoSource()
    {
        const string source = @"
namespace TestApp
{
    public class PlainPoco
    {
        public int Id { get; set; }
    }
}";
        var runResult = RunGenerator(source);

        var generatedTree = runResult.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.EndsWith("GeneratedMetadataRegistry.g.cs", StringComparison.Ordinal));

        generatedTree.Should().BeNull();
    }

    [Fact]
    public void Generator_WhenEntitySubclass_EmitsMetadataRegistry()
    {
        const string source = @"
using System;
using EricksonLopez.SharedKernel.Persistence;

namespace TestApp
{
    public class Order : EricksonLopez.SharedKernel.Entity
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public decimal Total { get; set; }
    }
}";
        var runResult = RunGenerator(source);

        var generatedTree = runResult.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.EndsWith("GeneratedMetadataRegistry.g.cs", StringComparison.Ordinal));

        generatedTree.Should().NotBeNull();
        var code = generatedTree!.ToString();

        code.Should().Contain("namespace EricksonLopez.SharedKernel.Persistence.Generated;");
        code.Should().Contain("public static class GeneratedMetadataRegistry");
        code.Should().Contain("TableName = \"Order\"");
        code.Should().Contain("Schema = \"public\"");
        code.Should().Contain("ColumnName: \"Id\"");
        code.Should().Contain("IsKey: true");
        code.Should().Contain("IsTenantColumn: true");
        code.Should().Contain("PrimaryKeys = columns_TestApp_Order.Where(c => c.IsKey).ToList()");
        code.Should().Contain("TenantColumn = columns_TestApp_Order.FirstOrDefault(c => c.IsTenantColumn)");
    }

    [Fact]
    public void Generator_WithSoftDeleteAndConcurrencyToken_EmitsSpecializedColumnsAndDefensiveSetters()
    {
        const string source = @"
using System;
using EricksonLopez.SharedKernel.Persistence;

namespace TestApp
{
    public class Product : EricksonLopez.SharedKernel.Entity
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public byte[] RowVersion { get; set; }
        public string Name { get; set; }
    }
}";
        var runResult = RunGenerator(source);

        var generatedTree = runResult.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.EndsWith("GeneratedMetadataRegistry.g.cs", StringComparison.Ordinal));

        generatedTree.Should().NotBeNull();
        var code = generatedTree!.ToString();

        code.Should().Contain("IsSoftDeleteColumn: true");
        code.Should().Contain("IsConcurrencyToken: true");
        code.Should().Contain("SoftDeleteColumn = columns_TestApp_Product.FirstOrDefault(c => c.IsSoftDeleteColumn)");
        code.Should().Contain("ConcurrencyToken = columns_TestApp_Product.FirstOrDefault(c => c.IsConcurrencyToken)");
        code.Should().Contain("throw new global::System.ArgumentNullException");
    }

    private static GeneratorDriverRunResult RunGenerator(string source)
    {
        const string persistenceAttributesSource = @"
namespace EricksonLopez.SharedKernel.Persistence
{
    using System;

    public enum AuditFieldType { CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, Version }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class TableAttribute : Attribute
    {
        public TableAttribute(string name) => Name = name;
        public string Name { get; }
        public string? Schema { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string name) => Name = name;
        public string Name { get; }
        public bool IsPrimaryKey { get; set; }
        public bool IsDatabaseGenerated { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AuditAttribute : Attribute
    {
        public AuditAttribute(AuditFieldType fieldType) => FieldType = fieldType;
        public AuditFieldType FieldType { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class TenantScopedAttribute : Attribute {}

    public sealed record PropertyMetadata(
        string ClrName, Type ClrType, string ColumnName, string DatabaseType,
        bool IsKey, bool IsNullable, bool IsAuditColumn, bool IsSoftDeleteColumn,
        bool IsTenantColumn, bool IsConcurrencyToken,
        Func<object, object?> Getter, Action<object, object?>? Setter);

    public sealed record EntityMetadata
    {
        public Type ClrType { get; init; } = null!;
        public string TableName { get; init; } = string.Empty;
        public string Schema { get; init; } = string.Empty;
        public System.Collections.Generic.IReadOnlyList<PropertyMetadata> Columns { get; init; } = [];
        public System.Collections.Generic.IReadOnlyList<PropertyMetadata> PrimaryKeys { get; init; } = [];
        public PropertyMetadata? TenantColumn { get; init; }
        public PropertyMetadata? SoftDeleteColumn { get; init; }
        public PropertyMetadata? ConcurrencyToken { get; init; }
    }
}
namespace EricksonLopez.SharedKernel
{
    public abstract class Entity {}
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var attrTree = CSharpSyntaxTree.ParseText(persistenceAttributesSource);
        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(DateTimeOffset).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Guid).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            new[] { syntaxTree, attrTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new MetadataSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);
        diagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);

        return driver.GetRunResult();
    }
}
