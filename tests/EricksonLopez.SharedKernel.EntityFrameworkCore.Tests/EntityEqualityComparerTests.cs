// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.EntityFrameworkCore;
using Xunit;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore.Tests;

public sealed class EntityEqualityComparerTests
{
    private sealed class SampleEntity(Guid id) : Entity<Guid>(id);

    private sealed class OtherEntity(Guid id) : Entity<Guid>(id);

    private class BaseOrder(Guid id) : Entity<Guid>(id);

    private sealed class BaseOrderProxy : BaseOrder
    {
        public BaseOrderProxy(Guid id) : base(id) { }
    }

    [Fact]
    public void Create_ReturnsNewComparerInstance()
    {
        var comparer = EntityEqualityComparer.Create<Guid>();
        comparer.Should().NotBeNull();
        comparer.Should().BeOfType<EntityEqualityComparer<Guid>>();
    }

    [Fact]
    public void Equals_WithBothNull_ReturnsTrue()
    {
        var comparer = EntityEqualityComparer.Create<Guid>();
        comparer.Equals(null, null).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithSameReference_ReturnsTrue()
    {
        var comparer = EntityEqualityComparer.Create<Guid>();
        var entity = new SampleEntity(Guid.NewGuid());
        comparer.Equals(entity, entity).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithOneNull_ReturnsFalse()
    {
        var comparer = EntityEqualityComparer.Create<Guid>();
        var entity = new SampleEntity(Guid.NewGuid());
        comparer.Equals(entity, null).Should().BeFalse();
        comparer.Equals(null, entity).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameTypeAndSameId_ReturnsTrue()
    {
        var comparer = EntityEqualityComparer.Create<Guid>();
        var id = Guid.NewGuid();
        var e1 = new SampleEntity(id);
        var e2 = new SampleEntity(id);

        comparer.Equals(e1, e2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithSameTypeAndDifferentId_ReturnsFalse()
    {
        var comparer = EntityEqualityComparer.Create<Guid>();
        var e1 = new SampleEntity(Guid.NewGuid());
        var e2 = new SampleEntity(Guid.NewGuid());

        comparer.Equals(e1, e2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentTypesAndSameId_ReturnsFalse()
    {
        var comparer = EntityEqualityComparer.Create<Guid>();
        var id = Guid.NewGuid();
        var e1 = new SampleEntity(id);
        var e2 = new OtherEntity(id);

        comparer.Equals(e1, e2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithProxyAndTargetType_UnwrapsAndComparesTrue()
    {
        var comparer = EntityEqualityComparer.Create<Guid>();
        var id = Guid.NewGuid();
        var baseOrder = new BaseOrder(id);
        var proxyOrder = new BaseOrderProxy(id);

        comparer.Equals(baseOrder, proxyOrder).Should().BeTrue();
        comparer.Equals(proxyOrder, baseOrder).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithValidEntity_ReturnsConsistentHashCode()
    {
        var comparer = EntityEqualityComparer.Create<Guid>();
        var id = Guid.NewGuid();
        var e1 = new SampleEntity(id);
        var e2 = new SampleEntity(id);

        comparer.GetHashCode(e1).Should().Be(comparer.GetHashCode(e2));
    }

    [Fact]
    public void GetHashCode_WithNull_ThrowsArgumentNullException()
    {
        var comparer = EntityEqualityComparer.Create<Guid>();
        var act = () => comparer.GetHashCode(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetUnproxiedType_WithNull_ThrowsArgumentNullException()
    {
        var act = () => EntityEqualityComparer.GetUnproxiedType(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetUnproxiedType_WithRegularType_ReturnsSameType()
    {
        EntityEqualityComparer.GetUnproxiedType(typeof(SampleEntity))
            .Should().Be<SampleEntity>();
    }

    [Fact]
    public void GetUnproxiedType_WithProxyEndingName_ReturnsBaseType()
    {
        EntityEqualityComparer.GetUnproxiedType(typeof(BaseOrderProxy))
            .Should().Be<BaseOrder>();
    }

    [Fact]
    public void GetUnproxiedType_WithCastleProxiesNamespace_ReturnsBaseType()
    {
        EntityEqualityComparer.GetUnproxiedType(typeof(global::Castle.Proxies.CastleOrderEntity))
            .Should().Be<Entity<Guid>>();
    }

    [Fact]
    public void GetUnproxiedType_WithProxyEndingName_WhoseBaseTypeIsObject_ReturnsSameType()
    {
        EntityEqualityComparer.GetUnproxiedType(typeof(SimpleObjectProxy))
            .Should().Be<SimpleObjectProxy>();
    }
}

public class SimpleObjectProxy { }
