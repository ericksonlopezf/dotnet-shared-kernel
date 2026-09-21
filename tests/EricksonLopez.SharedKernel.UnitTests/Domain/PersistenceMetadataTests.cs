// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.SharedKernel.UnitTests.Domain;

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using EricksonLopez.SharedKernel.Persistence;
using Xunit;

#pragma warning disable CS0618 // Tested for backward compatibility

public sealed class PersistenceMetadataTests
{
    [Fact]
    public void TableAttribute_ShouldStoreNameAndSchema()
    {
        // Act
        var attr = new TableAttribute("orders")
        {
            Schema = "sales"
        };

        // Assert
        attr.Name.Should().Be("orders");
        attr.Schema.Should().Be("sales");
    }

    [Fact]
    public void ColumnAttribute_ShouldStoreProperties()
    {
        // Act
        var attr = new ColumnAttribute("order_id")
        {
            IsPrimaryKey = true,
            IsDatabaseGenerated = true
        };

        // Assert
        attr.Name.Should().Be("order_id");
        attr.IsPrimaryKey.Should().BeTrue();
        attr.IsDatabaseGenerated.Should().BeTrue();
    }

    [Theory]
    [InlineData(AuditFieldType.CreatedAt)]
    [InlineData(AuditFieldType.CreatedBy)]
    [InlineData(AuditFieldType.UpdatedAt)]
    [InlineData(AuditFieldType.UpdatedBy)]
    public void AuditAttribute_ShouldStoreFieldType(AuditFieldType fieldType)
    {
        // Act
        var attr = new AuditAttribute(fieldType);

        // Assert
        attr.FieldType.Should().Be(fieldType);
    }

    [Fact]
    public void TenantScopedAttribute_ShouldInstantiate()
    {
        // Act
        var attr = new TenantScopedAttribute();

        // Assert
        attr.Should().NotBeNull();
    }

    [Fact]
    public void PropertyMetadata_ShouldCorrectlyDelegateAccessors()
    {
        // Arrange
        var testObj = new TestEntity { Total = 100m };
        var prop = new PropertyMetadata(
            ClrName: nameof(TestEntity.Total),
            ClrType: typeof(decimal),
            ColumnName: "total",
            DatabaseType: "numeric(18,2)",
            IsKey: false,
            IsNullable: false,
            IsAuditColumn: false,
            IsSoftDeleteColumn: false,
            IsTenantColumn: false,
            IsConcurrencyToken: false,
            Getter: obj => ((TestEntity)obj).Total,
            Setter: (obj, val) => ((TestEntity)obj).Total = (decimal)val!
        );

        // Act & Assert
        prop.Getter(testObj).Should().Be(100m);
        prop.Setter!(testObj, 250m);
        testObj.Total.Should().Be(250m);
        prop.ColumnName.Should().Be("total");
    }

    [Fact]
    public void PropertyMetadata_ShouldProvideZeroBoxingAccessorsWhenConfigured()
    {
        // Arrange
        var testObj = new TestEntity { Id = Guid.NewGuid(), Total = 500m };
        Func<TestEntity, decimal> typedGetter = e => e.Total;
        Action<TestEntity, decimal> typedSetter = (e, v) => e.Total = v;

        var prop = new PropertyMetadata(
            ClrName: nameof(TestEntity.Total),
            ClrType: typeof(decimal),
            ColumnName: "total",
            DatabaseType: "numeric(18,2)",
            IsKey: false,
            IsNullable: false,
            IsAuditColumn: false,
            IsSoftDeleteColumn: false,
            IsTenantColumn: false,
            IsConcurrencyToken: false,
            Getter: obj => ((TestEntity)obj).Total,
            Setter: (obj, val) => ((TestEntity)obj).Total = (decimal)val!)
        {
            StronglyTypedGetter = typedGetter,
            StronglyTypedSetter = typedSetter
        };

        // Act & Assert
        prop.GetValue<TestEntity, decimal>(testObj).Should().Be(500m);
        prop.SetValue(testObj, 750m);
        testObj.Total.Should().Be(750m);
        prop.GetValue<TestEntity, decimal>(testObj).Should().Be(750m);
    }

    [Fact]
    public void EntityMetadata_ShouldIndexColumnsAndProvideFastLookup()
    {
        // Arrange
        var idProp = new PropertyMetadata(
            "Id", typeof(Guid), "id", "uuid", true, false, false, false, false, false,
            o => ((TestEntity)o).Id, (o, v) => ((TestEntity)o).Id = (Guid)v!);

        var totalProp = new PropertyMetadata(
            "Total", typeof(decimal), "total", "numeric", false, false, false, false, false, false,
            o => ((TestEntity)o).Total, (o, v) => ((TestEntity)o).Total = (decimal)v!);

        var metadata = new EntityMetadata
        {
            ClrType = typeof(TestEntity),
            TableName = "test_entities",
            Schema = "public",
            Columns = [idProp, totalProp],
            PrimaryKeys = [idProp]
        };

        // Act & Assert
        metadata.GetColumn("Id").Should().BeSameAs(idProp);
        metadata.TryGetColumn("Total", out var found).Should().BeTrue();
        found.Should().BeSameAs(totalProp);

        metadata.TryGetColumn("NonExistent", out var notFound).Should().BeFalse();
        notFound.Should().BeNull();

        Action act = () => metadata.GetColumn("NonExistent");
        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void EntityMetadata_ShouldStoreSpecializedColumns()
    {
        var tenantProp = new PropertyMetadata(
            "TenantId", typeof(Guid), "tenant_id", "uuid", false, false, false, false, true, false,
            o => Guid.Empty, (o, v) => { });

        var softDeleteProp = new PropertyMetadata(
            "IsDeleted", typeof(bool), "is_deleted", "boolean", false, false, false, true, false, false,
            o => false, (o, v) => { });

        var concurrencyProp = new PropertyMetadata(
            "RowVersion", typeof(byte[]), "row_version", "bytea", false, false, false, false, false, true,
            o => Array.Empty<byte>(), (o, v) => { });

        var metadata = new EntityMetadata
        {
            ClrType = typeof(TestEntity),
            TableName = "test_entities",
            TenantColumn = tenantProp,
            SoftDeleteColumn = softDeleteProp,
            ConcurrencyToken = concurrencyProp
        };

        metadata.TenantColumn.Should().BeSameAs(tenantProp);
        metadata.SoftDeleteColumn.Should().BeSameAs(softDeleteProp);
        metadata.ConcurrencyToken.Should().BeSameAs(concurrencyProp);
    }

    [Fact]
    public void EntityMetadata_ColumnIndex_IsThreadSafeUnderConcurrency()
    {
        var idProp = new PropertyMetadata(
            "Id", typeof(Guid), "id", "uuid", true, false, false, false, false, false,
            o => ((TestEntity)o).Id, (o, v) => ((TestEntity)o).Id = (Guid)v!);

        var metadata = new EntityMetadata
        {
            ClrType = typeof(TestEntity),
            Columns = [idProp]
        };

        System.Threading.Tasks.Parallel.For(0, 50, _ =>
        {
            metadata.GetColumn("Id").Should().BeSameAs(idProp);
        });
    }

    [Fact]
    public void PropertyMetadata_FallbackAccessors_ShouldUseUntypedDelegates()
    {
        var testObj = new TestEntity { Id = Guid.NewGuid(), Total = 100m };
        var readOnlyProp = new PropertyMetadata(
            ClrName: nameof(TestEntity.Total),
            ClrType: typeof(decimal),
            ColumnName: "total",
            DatabaseType: "numeric(18,2)",
            IsKey: false,
            IsNullable: false,
            IsAuditColumn: false,
            IsSoftDeleteColumn: false,
            IsTenantColumn: false,
            IsConcurrencyToken: false,
            Getter: o => ((TestEntity)o).Total,
            Setter: null);

        readOnlyProp.GetValue<TestEntity, decimal>(testObj).Should().Be(100m);
        // Setter is null, SetValue should be a safe no-op
        readOnlyProp.SetValue(testObj, 200m);
        testObj.Total.Should().Be(100m);

        var nullGetterProp = new PropertyMetadata(
            ClrName: "NullProp",
            ClrType: typeof(string),
            ColumnName: "null_prop",
            DatabaseType: "text",
            IsKey: false,
            IsNullable: true,
            IsAuditColumn: false,
            IsSoftDeleteColumn: false,
            IsTenantColumn: false,
            IsConcurrencyToken: false,
            Getter: _ => null,
            Setter: (o, v) => ((TestEntity)o).Total = 0m);

        nullGetterProp.GetValue<TestEntity, string>(testObj).Should().BeNull();
        nullGetterProp.SetValue(testObj, "new_val");
        testObj.Total.Should().Be(0m);

        var nullValueTypeProp = new PropertyMetadata(
            ClrName: "NullInt",
            ClrType: typeof(int),
            ColumnName: "null_int",
            DatabaseType: "integer",
            IsKey: false,
            IsNullable: true,
            IsAuditColumn: false,
            IsSoftDeleteColumn: false,
            IsTenantColumn: false,
            IsConcurrencyToken: false,
            Getter: _ => null,
            Setter: null);

        // When val is null, it must return default(int) without throwing cast exception
        nullValueTypeProp.GetValue<TestEntity, int>(testObj).Should().Be(0);
        nullValueTypeProp.GetValue<TestEntity, Guid>(testObj).Should().Be(Guid.Empty);
    }

    [Fact]
    public void EntityMetadata_DefaultsAndExceptionMessage_ShouldBeVerified()
    {
        var meta = new EntityMetadata
        {
            ClrType = typeof(TestEntity)
        };

        meta.TableName.Should().Be(string.Empty);
        meta.Schema.Should().Be("public");

        var act = () => meta.GetColumn("MissingProp");
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("Column 'MissingProp' not found on entity 'TestEntity'.");
    }

    private sealed class TestEntity
    {
        public Guid Id { get; set; }
        public decimal Total { get; set; }
    }
}
