// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using AwesomeAssertions;
using EricksonLopez.DomainPrimitives;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.UnitTests.Common;
using Xunit;

namespace EricksonLopez.SharedKernel.UnitTests.Domain;

public class EntityTests
{
    private class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) : base(id)
        {
        }
    }

    private class OtherTestEntity : Entity<Guid>
    {
        public OtherTestEntity(Guid id) : base(id)
        {
        }
    }

    private class IntEntity : Entity<int>
    {
        public IntEntity(int id) : base(id)
        {
        }
    }

    private class StringEntity : Entity<string>
    {
        public StringEntity(string id) : base(id)
        {
        }
    }

    private readonly record struct StronglyTypedId(Guid Value) : IStrongId<StronglyTypedId, Guid>
    {
        public static string PrimitiveName => nameof(StronglyTypedId);
        public bool IsDefault => Value == Guid.Empty;
        public static StronglyTypedId Empty => new(Guid.Empty);
        public static StronglyTypedId Create() => new(Guid.NewGuid());
        public static StronglyTypedId Create(Guid value) => new(value);
        public static bool TryCreate(Guid value, out StronglyTypedId result, out global::EricksonLopez.DomainPrimitives.Validation.PrimitiveError validationError)
        {
            result = new(value);
            validationError = default;
            return true;
        }
    }

    private class StronglyTypedEntity : Entity<StronglyTypedId>
    {
        public StronglyTypedEntity(StronglyTypedId id) : base(id)
        {
        }
    }

    private class DerivedTestEntity : TestEntity
    {
        public DerivedTestEntity(Guid id) : base(id) { }
    }

    #region Constructor & Guard Validation

    [Fact]
    public void Constructor_WithValidId_SetsIdProperty()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);

        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Constructor_WithDefaultGuid_ThrowsArgumentException()
    {
        var act = () => new TestEntity(Guid.Empty);

        var ex = act.Should().Throw<ArgumentException>()
            .WithParameterName("id")
            .Which;

        ex.Message.Should().Contain("Entity identity cannot be default.");
    }

    [Fact]
    public void Constructor_WithDefaultInt_ThrowsArgumentException()
    {
        var act = () => new IntEntity(0);

        var ex = act.Should().Throw<ArgumentException>()
            .WithParameterName("id")
            .Which;

        ex.Message.Should().Contain("Entity identity cannot be default.");
    }

    [Fact]
    public void Constructor_WithNullReference_ThrowsArgumentException()
    {
        var act = () => new StringEntity(null!);

        var ex = act.Should().Throw<ArgumentException>()
            .WithParameterName("id")
            .Which;

        ex.Message.Should().Contain("Entity identity cannot be default.");
    }

    [Fact]
    public void Constructor_WithDefaultStronglyTypedId_ThrowsArgumentException()
    {
        var act = () => new StronglyTypedEntity(default);

        var ex = act.Should().Throw<ArgumentException>()
            .WithParameterName("id")
            .Which;

        ex.Message.Should().Contain("Entity identity cannot be default.");
    }

    [Fact]
    public void Constructor_WithEmptyString_AllowsInstanceCreation()
    {
        // Architectural Invariant:
        // Entity<TId> guard verifies that identity != default(TId). For raw primitive `string`, default is `null`.
        // Therefore, non-null empty strings are permitted at the base generic Entity level.
        // Domain-specific business constraints (e.g. non-empty, non-whitespace, format rules) must be encapsulated
        // in strongly-typed identifiers (IStrongId<TSelf, string>), not in the raw generic Entity base class.
        var emptyStringEntity = new StringEntity(string.Empty);

        emptyStringEntity.Id.Should().Be(string.Empty);
    }

    #endregion

    #region Equality & HashCode

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
        (entity != entity).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentId_ReturnsFalse()
    {
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        entity1.Equals(entity2).Should().BeFalse();
        entity1.Equals((object)entity2).Should().BeFalse();
        (entity1 == entity2).Should().BeFalse();
        (entity1 != entity2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var entity = new TestEntity(Guid.NewGuid());
        Entity<Guid>? nullEntity = null;
        object? nullObj = null;

        entity.Equals(nullEntity).Should().BeFalse();
        entity.Equals(nullObj).Should().BeFalse();
        (entity == null).Should().BeFalse();
        (null == entity).Should().BeFalse();
        (entity != null).Should().BeTrue();
        (null != entity).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new OtherTestEntity(id);

        entity1.Equals(entity2).Should().BeFalse();
        entity1.Equals((object)entity2).Should().BeFalse();
        (entity1.GetHashCode() == entity2.GetHashCode()).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentObjectType_ReturnsFalse()
    {
        var entity = new TestEntity(Guid.NewGuid());
        entity.Equals(new object()).Should().BeFalse();
        entity.Equals("string object").Should().BeFalse();
    }

    [Fact]
    public void Equals_DerivedType_ReturnsFalse_BecauseDomainDoesNotUnwrapProxies()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);
        var derivedEntity = new DerivedTestEntity(id);

        entity.Equals(derivedEntity).Should().BeFalse(
            because: "a derived type is a different concrete type — GetType() != GetType() of base");
        derivedEntity.Equals(entity).Should().BeFalse();
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
    public void Equals_WithOneNullOperand_ReturnsFalse()
    {
        TestEntity? entity1 = new TestEntity(Guid.NewGuid());
        TestEntity? entity2 = null;

        (entity1 == entity2).Should().BeFalse();
        (entity1 != entity2).Should().BeTrue();
        (entity2 == entity1).Should().BeFalse();
        (entity2 != entity1).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithStronglyTypedId_WorksCorrectly()
    {
        var rawId = Guid.NewGuid();
        var id = new StronglyTypedId(rawId);
        var entity1 = new StronglyTypedEntity(id);
        var entity2 = new StronglyTypedEntity(id);
        var entity3 = new StronglyTypedEntity(new StronglyTypedId(Guid.NewGuid()));

        entity1.Equals(entity2).Should().BeTrue();
        (entity1 == entity2).Should().BeTrue();
        (entity1 != entity2).Should().BeFalse();

        entity1.Equals(entity3).Should().BeFalse();
        (entity1 == entity3).Should().BeFalse();
        (entity1 != entity3).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithStringEntity_WorksCorrectly()
    {
        var entity1 = new StringEntity(TestValues.Strings.EntityIdString);
        var entity2 = new StringEntity(TestValues.Strings.EntityIdString);
        var entity3 = new StringEntity(TestValues.Strings.AlternativeEntityIdString);

        entity1.Equals(entity2).Should().BeTrue();
        (entity1 == entity2).Should().BeTrue();
        (entity1 != entity2).Should().BeFalse();

        entity1.Equals(entity3).Should().BeFalse();
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
    public void Collection_HashSet_ShouldContainEntity_WhenAdded()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        var set = new HashSet<TestEntity> { entity1 };

        set.Contains(entity2).Should().BeTrue();
    }

    #endregion

    #region Mathematical Equality Properties (Symmetry & Transitivity)

    [Fact]
    public void Equals_WhenSymmetric_MaintainsMathematicalSymmetry()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        entity1.Equals(entity2).Should().Be(entity2.Equals(entity1));

        var entity3 = new TestEntity(Guid.NewGuid());
        entity1.Equals(entity3).Should().Be(entity3.Equals(entity1));
    }

    [Fact]
    public void Equals_WhenTransitive_MaintainsMathematicalTransitivity()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);
        var entity3 = new TestEntity(id);

        entity1.Equals(entity2).Should().BeTrue();
        entity2.Equals(entity3).Should().BeTrue();
        entity1.Equals(entity3).Should().BeTrue();
    }

    #endregion

    #region Interface Contracts

    [Fact]
    public void IEntity_PolymorphicAccess_ExposesIdCorrectly()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);

        IEntity<Guid> ientity = entity;
        ientity.Id.Should().Be(id);
    }

    #endregion
}
