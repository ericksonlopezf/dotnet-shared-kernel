// Copyright © Erickson Lopez. MIT License.
using EricksonLopez.SharedKernel.UnitTests.Common;
using System;
using System.Reflection;
using AwesomeAssertions;
using EricksonLopez.DomainPrimitives;
using EricksonLopez.SharedKernel;
using Xunit;

namespace EricksonLopez.SharedKernel.UnitTests.Domain;

public class ValueObjectTests
{
    private sealed record Address(string Street, string City, string PostalCode) : ValueObject;
    private sealed record OtherValueObject(string Street, string City, string PostalCode) : ValueObject;
    private sealed record Money(decimal Amount, string Currency) : ValueObject;

    [ValueObject]
    private readonly record struct DecoratedMoney(decimal Amount, string Currency);

    #region Equality & HashCode

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        var addr1 = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);
        var addr2 = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);

        addr1.Equals(addr2).Should().BeTrue();
        addr1.Equals((object)addr2).Should().BeTrue();
        (addr1 == addr2).Should().BeTrue();
        (addr1 != addr2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        var addr1 = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);
        var addr2 = new Address(TestValues.Strings.AlternativeStreet, TestValues.Strings.City, TestValues.Strings.PostalCode);

        addr1.Equals(addr2).Should().BeFalse();
        addr1.Equals((object)addr2).Should().BeFalse();
        (addr1 == addr2).Should().BeFalse();
        (addr1 != addr2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        var addr = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);
        var other = new OtherValueObject(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);

        addr.Equals(other).Should().BeFalse();
        addr.Equals((object)other).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var addr = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);
        Address? nullAddr = null;
        object? nullObj = null;

        addr.Equals(nullAddr).Should().BeFalse();
        addr.Equals(nullObj).Should().BeFalse();
        (addr == null).Should().BeFalse();
        (null == addr).Should().BeFalse();
        (addr != null).Should().BeTrue();
        (null != addr).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ReturnsSameHashCode()
    {
        var addr1 = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);
        var addr2 = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);

        addr1.GetHashCode().Should().Be(addr2.GetHashCode());
    }

    [Fact]
    public void WithExpression_CreatesNewInstanceWithModifiedAttribute()
    {
        var addr1 = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);
        var addr2 = addr1 with { City = TestValues.Strings.AlternativeCity };

        addr2.Street.Should().Be(TestValues.Strings.Street);
        addr2.City.Should().Be(TestValues.Strings.AlternativeCity);
        addr2.PostalCode.Should().Be(TestValues.Strings.PostalCode);
        addr1.City.Should().Be(TestValues.Strings.City);
    }

    [Fact]
    public void Money_MultiAttribute_EqualityWorksCorrectly()
    {
        var m1 = new Money(TestValues.Numbers.Hundred, TestValues.Strings.UsdCurrency);
        var m2 = new Money(TestValues.Numbers.Hundred, TestValues.Strings.UsdCurrency);
        var m3 = new Money(TestValues.Numbers.Hundred, TestValues.Strings.EurCurrency);
        var m4 = new Money(TestValues.Numbers.TwoHundred, TestValues.Strings.UsdCurrency);

        m1.Should().Be(m2);
        m1.Should().NotBe(m3);
        m1.Should().NotBe(m4);
    }

    [Fact]
    public void ToString_IncludesRecordTypeNameAndAttributes()
    {
        var addr = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);
        var str = addr.ToString();

        str.Should().Contain("Address");
        str.Should().Contain(TestValues.Strings.Street);
        str.Should().Contain(TestValues.Strings.City);
        str.Should().Contain(TestValues.Strings.PostalCode);
    }

    #endregion

    #region Mathematical Equality Properties

    [Fact]
    public void Equals_WhenReflexive_MaintainsMathematicalIdentity()
    {
        var a = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);

        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenSymmetric_MaintainsMathematicalSymmetry()
    {
        var a = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);
        var b = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);

        a.Equals(b).Should().Be(b.Equals(a));
    }

    [Fact]
    public void Equals_WhenTransitive_MaintainsMathematicalTransitivity()
    {
        var a = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);
        var b = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);
        var c = new Address(TestValues.Strings.Street, TestValues.Strings.City, TestValues.Strings.PostalCode);

        a.Equals(b).Should().BeTrue();
        b.Equals(c).Should().BeTrue();
        a.Equals(c).Should().BeTrue();
    }

    #endregion

    #region ValueObjectAttribute Tests

    [Fact]
    public void ValueObjectAttribute_CanBeInstantiated()
    {
        var attr = new ValueObjectAttribute();

        attr.Should().NotBeNull();
        attr.Should().BeOfType<ValueObjectAttribute>();
    }

    [Fact]
    public void ValueObjectAttribute_AttributeUsageMetadata_IsConfiguredCorrectly()
    {
        var usageAttr = typeof(ValueObjectAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        usageAttr.Should().NotBeNull();
        usageAttr!.ValidOn.Should().Be(AttributeTargets.Struct);
        usageAttr.Inherited.Should().BeFalse();
    }

    [Fact]
    public void ValueObjectAttribute_CanDecorateStruct()
    {
        var structAttr = typeof(DecoratedMoney).GetCustomAttribute<ValueObjectAttribute>();

        structAttr.Should().NotBeNull();
    }

    #endregion
}
