// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using AwesomeAssertions;
using NetArchTest.Rules;
using Xunit;

namespace EricksonLopez.SharedKernel.ArchitectureTests;

/// <summary>
/// Architectural purity tests for the SharedKernel NuGet package.
/// Validates zero third-party dependencies, contract abstraction, XML documentation, and namespace isolation.
/// </summary>
public sealed class SharedKernelPurityTests
{
    private static readonly Assembly KernelAssembly = typeof(Entity<>).Assembly;
    private const string SharedKernelNamespace = "EricksonLopez.SharedKernel";

    [Fact]
    public void SharedKernel_MustHaveZeroTransitiveThirdPartyDependencies()
    {
        // SharedKernel must only reference the .NET BCL (System.*, Microsoft.Extensions.Primitives, netstandard, mscorlib)
        var referencedAssemblies = KernelAssembly.GetReferencedAssemblies();

        var nonSystemAssemblies = referencedAssemblies
            .Where(a => !a.Name!.StartsWith("System", StringComparison.OrdinalIgnoreCase) &&
                        !a.Name.StartsWith("Microsoft.Extensions.Primitives", StringComparison.OrdinalIgnoreCase) &&
                        !a.Name.StartsWith("EricksonLopez.Events", StringComparison.OrdinalIgnoreCase) &&
                        !a.Name.Equals("netstandard", StringComparison.OrdinalIgnoreCase) &&
                        !a.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))
            .Select(a => a.Name)
            .ToArray();

        nonSystemAssemblies.Should().BeEmpty(
            because: $"the SharedKernel NuGet package must not pull in third-party libraries: {string.Join(", ", nonSystemAssemblies)}");
    }

    [Fact]
    public void SharedKernel_MustContainOnlyAbstractTypesOrBuildingBlocks_NoConcreteEntities()
    {
        // A SharedKernel must never contain instantiable business domain entities
        var result = Types.InAssembly(KernelAssembly)
            .That().Inherit(typeof(Entity<>))
            .Should().BeAbstract()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"the SharedKernel must contain only abstract building blocks and no concrete domain entities: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void SharedKernel_AllPublicTypes_MustHaveExplicitXmlDocumentation()
    {
        var xmlDocumentationPath = Path.ChangeExtension(KernelAssembly.Location, ".xml");
        File.Exists(xmlDocumentationPath).Should().BeTrue(
            because: "XML documentation file must be generated alongside the assembly (CS1591 enforced via <TreatWarningsAsErrors>true</TreatWarningsAsErrors>).");

        var doc = XDocument.Load(xmlDocumentationPath);
        var memberDocElements = doc.Descendants("member")
            .Where(m => !string.IsNullOrWhiteSpace(m.Element("summary")?.Value))
            .Select(m => m.Attribute("name")?.Value)
            .Where(name => name is not null)
            .ToHashSet();

        var exportedTypes = KernelAssembly.GetExportedTypes()
            .Where(t => t.IsPublic && !t.IsNested);

        var undocumentedTypes = new List<string>();

        foreach (var type in exportedTypes)
        {
            var typeDocName = $"T:{type.FullName?.Replace('+', '.')}";
            if (!memberDocElements.Contains(typeDocName))
            {
                undocumentedTypes.Add(type.FullName ?? type.Name);
            }
        }

        undocumentedTypes.Should().BeEmpty(
            because: $"all public types in the SharedKernel must have explicit XML documentation for NuGet consumers: {string.Join(", ", undocumentedTypes)}");
    }

    [Fact]
    public void SharedKernel_AllExportedTypes_MustResideInSharedKernelNamespace()
    {
        var exportedTypes = KernelAssembly.GetExportedTypes();

        foreach (var type in exportedTypes)
        {
            type.Namespace.Should().Be(SharedKernelNamespace,
                because: $"exported public type '{type.FullName}' must reside directly in the '{SharedKernelNamespace}' root namespace.");
        }
    }

    [Fact]
    public void SourceCode_AllStrykerSuppressions_MustIncludeTechnicalJustificationComment()
    {
        // Enforces ADR-029: Every `// Stryker disable` directive in src/ must include an inline technical explanation
        var repoRoot = FindRepositoryRoot();
        repoRoot.Should().NotBeNull(because: "Repository root containing Directory.Build.props must be locatable.");

        var srcDir = Path.Combine(repoRoot!, "src");
        Directory.Exists(srcDir).Should().BeTrue();

        var csFiles = Directory.GetFiles(srcDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar) &&
                        !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar));

        var invalidSuppressions = new List<string>();

        foreach (var file in csFiles)
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Contains("// Stryker disable", StringComparison.OrdinalIgnoreCase))
                {
                    // ADR-029 requires a colon followed by a technical justification comment
                    var parts = line.Split(':');
                    if (parts.Length < 2 || parts[1].Trim().Length < 5)
                    {
                        var relativePath = Path.GetRelativePath(repoRoot!, file);
                        invalidSuppressions.Add($"{relativePath}:L{i + 1} -> '{line.Trim()}'");
                    }
                }
            }
        }

        invalidSuppressions.Should().BeEmpty(
            because: $"all Stryker mutation suppressions must be documented with an inline technical justification per ADR-029: {string.Join("; ", invalidSuppressions)}");
    }

    private static string? FindRepositoryRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "EricksonLopez.SharedKernel.slnx")) ||
                File.Exists(Path.Combine(dir, "Directory.Build.props")))
            {
                return dir;
            }

            var parent = Directory.GetParent(dir);
            dir = parent?.FullName;
        }

        return null;
    }
}

