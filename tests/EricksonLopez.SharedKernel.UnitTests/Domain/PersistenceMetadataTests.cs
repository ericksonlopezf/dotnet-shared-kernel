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

    [Fact]
    public void AuditAttribute_ShouldStoreFieldType()
    {
        // Act
        var attr = new AuditAttribute(AuditFieldType.CreatedAt);

        // Assert
        attr.FieldType.Should().Be(AuditFieldType.CreatedAt);
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

    private sealed class TestEntity
    {
        public Guid Id { get; set; }
        public decimal Total { get; set; }
    }
}
