using System;
using EricksonLopez.SharedKernel;
using AwesomeAssertions;
using Xunit;

namespace EricksonLopez.SharedKernel.UnitTests.Domain
{

public class EntityTests
{
    internal class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id)
        {
            Id = id;
        }
    }

    private class OtherTestEntity : Entity<Guid>
    {
        public OtherTestEntity(Guid id)
        {
            Id = id;
        }
    }

    [Fact]
    public void Equals_WithSameIdAndType_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        entity1.Equals(entity2).Should().BeTrue();
        entity1.Equals((object)entity2).Should().BeTrue();
        (entity1 == entity2).Should().BeTrue();
        (entity1 != entity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameReference_ReturnsTrue()
    {
        var entity = new TestEntity(Guid.NewGuid());

        entity.Equals(entity).Should().BeTrue();
        entity.Equals((object)entity).Should().BeTrue();
        (entity == entity).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentId_ReturnsFalse()
    {
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        entity1.Equals(entity2).Should().BeFalse();
        (entity1 == entity2).Should().BeFalse();
        (entity1 != entity2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var entity = new TestEntity(Guid.NewGuid());

        entity.Equals(null).Should().BeFalse();
        (entity == null).Should().BeFalse();
        (null == entity).Should().BeFalse();
        (entity != null).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new OtherTestEntity(id);

        entity1.Equals(entity2).Should().BeFalse();
        entity1.Equals((object)entity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentObjectType_ReturnsFalse()
    {
        var entity = new TestEntity(Guid.NewGuid());
        entity.Equals(new object()).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameIdAndType_ReturnsSameValue()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentTypeButSameId_ReturnsDifferentValue()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new OtherTestEntity(id);

        entity1.GetHashCode().Should().NotBe(entity2.GetHashCode());
    }

    [Fact]
    public void Equals_WithNullOperands_ReturnsTrue()
    {
        TestEntity? entity1 = null;
        TestEntity? entity2 = null;
        
        (entity1 == entity2).Should().BeTrue();
        (entity1 != entity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_TransientEntities_ReturnsFalse()
    {
        var entity1 = new TestEntity(default);
        var entity2 = new TestEntity(default);

        entity1.Equals(entity2).Should().BeFalse();
        (entity1 == entity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithOneTransientEntity_ReturnsFalse()
    {
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(default);

        entity1.Equals(entity2).Should().BeFalse();
        entity2.Equals(entity1).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_TransientEntities_ReturnsDifferentValues()
    {
        var entity1 = new TestEntity(default);
        var entity2 = new TestEntity(default);

        entity1.GetHashCode().Should().NotBe(entity2.GetHashCode());
    }

    [Fact]
    public void Collection_HashSet_ShouldContainEntity_WhenAdded()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id); // Same ID and Type

        var set = new System.Collections.Generic.HashSet<TestEntity> { entity1 };

        set.Contains(entity2).Should().BeTrue();
    }

    private readonly record struct StronglyTypedId(Guid Value);
    
    private class StronglyTypedEntity : Entity<StronglyTypedId>
    {
        public StronglyTypedEntity(StronglyTypedId id)
        {
            Id = id;
        }
    }

    [Fact]
    public void Equals_WithStronglyTypedId_WorksCorrectly()
    {
        var id = new StronglyTypedId(Guid.NewGuid());
        var entity1 = new StronglyTypedEntity(id);
        var entity2 = new StronglyTypedEntity(id);

        entity1.Equals(entity2).Should().BeTrue();
        (entity1 == entity2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DerivedType_ReturnsFalse_BecauseDomainDoesNotUnwrapProxies()
    {
        // IMPORTANT: With GetType() (no proxy-unwrapping), a subclass IS a different type.
        // This is the correct DDD behavior — proxy-aware equality is an infrastructure concern.
        // If consumers use Castle DynamicProxy (EF Core lazy loading), they should handle
        // equality in their infrastructure configuration, not in the domain layer.
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);
        var derivedEntity = new DerivedTestEntity(id);

        entity.Equals(derivedEntity).Should().BeFalse(
            because: "a derived type is a different concrete type — GetType() != GetType() of base");
        derivedEntity.Equals(entity).Should().BeFalse();
    }

    private class DerivedTestEntity : TestEntity
    {
        public DerivedTestEntity(Guid id) : base(id) { }
    }
}

}

