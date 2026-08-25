// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.DomainPrimitives;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.TestingUtilities.Fakes;
using EricksonLopez.SharedKernel.UnitTests.Common;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace EricksonLopez.SharedKernel.UnitTests.Domain;

public class StrongIdTests
{
    private sealed class Order : AggregateRoot<OrderId>
    {
        public Order(OrderId id) : base(id)
        {
        }

        public void Place()
        {
            RaiseDomainEvent(new OrderPlacedEvent(Id));
        }
    }

    private sealed class Product : Entity<ProductCode>
    {
        public Product(ProductCode id) : base(id)
        {
        }
    }

    private sealed record OrderPlacedEvent(OrderId OrderId) : DomainEvent;

    [Fact]
    public void StrongId_UnderlyingValue_ShouldBeAccessible()
    {
        var rawGuid = Guid.NewGuid();
        var orderId = new OrderId(rawGuid);

        orderId.Value.Should().Be(rawGuid);
    }

    [Fact]
    public void StrongId_StaticCreate_ShouldConstructValidInstance()
    {
        var rawGuid = Guid.NewGuid();
        var orderId = OrderId.Create(rawGuid);

        orderId.Value.Should().Be(rawGuid);
    }

    [Fact]
    public void StrongId_GenericFactoryMethod_ConstructsCorrectType()
    {
        var rawGuid = Guid.NewGuid();
        var orderId = InstantiateStrongId<OrderId, Guid>(rawGuid);

        orderId.Value.Should().Be(rawGuid);
    }

    private static TSelf InstantiateStrongId<TSelf, TValue>(TValue value)
        where TSelf : notnull, IStrongId<TSelf, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        return TSelf.Create(value);
    }

    [Fact]
    public void StrongId_WithStringValue_ShouldBeAccessible()
    {
        var productCode = new ProductCode(TestValues.Strings.ProductCode);

        productCode.Value.Should().Be(TestValues.Strings.ProductCode);
    }

    [Fact]
    public void StrongId_WithLongValue_ShouldBeAccessible()
    {
        var seqId = new SequenceId(TestValues.Numbers.SequenceId);

        seqId.Value.Should().Be(TestValues.Numbers.SequenceId);
    }

    [Fact]
    public void StrongId_WithIntValue_ShouldBeAccessible()
    {
        var deptId = new DepartmentId(TestValues.Numbers.Positive);

        deptId.Value.Should().Be(TestValues.Numbers.Positive);
    }

    [Fact]
    public void StrongId_Equals_WithSameValue_ReturnsTrue()
    {
        var rawGuid = Guid.NewGuid();
        var id1 = new OrderId(rawGuid);
        var id2 = new OrderId(rawGuid);

        id1.Equals(id2).Should().BeTrue();
        id1.Equals((object)id2).Should().BeTrue();
        (id1 == id2).Should().BeTrue();
        (id1 != id2).Should().BeFalse();
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    public void StrongId_Equals_WithDifferentValue_ReturnsFalse()
    {
        var id1 = new OrderId(Guid.NewGuid());
        var id2 = new OrderId(Guid.NewGuid());

        id1.Equals(id2).Should().BeFalse();
        id1.Equals((object)id2).Should().BeFalse();
        (id1 == id2).Should().BeFalse();
        (id1 != id2).Should().BeTrue();
    }

    [Fact]
    public void StrongId_UsedAsEntityIdentifier_WorksCorrectly()
    {
        var orderId = new OrderId(Guid.NewGuid());
        var order = new Order(orderId);

        order.Id.Should().Be(orderId);

        order.Place();
        var events = order.DrainDomainEvents();
        events.Should().ContainSingle()
            .Which.Should().BeOfType<OrderPlacedEvent>()
            .Which.OrderId.Should().Be(orderId);
    }

    [Fact]
    public void StrongId_UsedWithStringEntity_WorksCorrectly()
    {
        var code = new ProductCode(TestValues.Strings.AlternativeProductCode);
        var product = new Product(code);

        product.Id.Should().Be(code);
        product.Id.Value.Should().Be(TestValues.Strings.AlternativeProductCode);
    }

    [Fact]
    public void StrongId_DifferentTypesWithSameValue_AreNotEqual()
    {
        var rawGuid = Guid.NewGuid();
        var orderId = new OrderId(rawGuid);
        var customerId = new CustomerId(rawGuid);

        orderId.Value.Should().Be(customerId.Value);
        orderId.Equals((object)customerId).Should().BeFalse();
    }

    #region TryCreate Tests

    [Fact]
    public void TryCreate_WithValidGuid_ReturnsTrueAndSetsResult()
    {
        var rawGuid = Guid.NewGuid();

        var success = OrderId.TryCreate(rawGuid, out var result, out var validationError);

        success.Should().BeTrue();
        result.Value.Should().Be(rawGuid);
        validationError.IsError.Should().BeFalse();
    }

    [Fact]
    public void TryCreate_WithEmptyGuid_ReturnsFalseAndSetsValidationError()
    {
        var success = OrderId.TryCreate(Guid.Empty, out var result, out var validationError);

        success.Should().BeFalse();
        result.IsDefault.Should().BeTrue();
        validationError.IsError.Should().BeTrue();
        validationError.Code.Should().Be("EMPTY");
        validationError.Message.Should().Be("OrderId cannot be empty.");
    }

    [Fact]
    public void TryCreate_WithValidString_ReturnsTrueAndSetsResult()
    {
        const string raw = "SKU-9999";

        var success = ProductCode.TryCreate(raw, out var result, out var validationError);

        success.Should().BeTrue();
        result.Value.Should().Be(raw);
        validationError.IsError.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryCreate_WithNullOrWhitespaceString_ReturnsFalseAndSetsValidationError(string? invalidValue)
    {
        var success = ProductCode.TryCreate(invalidValue!, out var result, out var validationError);

        success.Should().BeFalse();
        result.IsDefault.Should().BeTrue();
        validationError.IsError.Should().BeTrue();
        validationError.Code.Should().Be("EMPTY");
        validationError.Message.Should().Be("ProductCode cannot be empty.");
    }

    [Fact]
    public void TryCreate_WithValidInt_ReturnsTrueAndSetsResult()
    {
        const int raw = 10;

        var success = DepartmentId.TryCreate(raw, out var result, out var validationError);

        success.Should().BeTrue();
        result.Value.Should().Be(raw);
        validationError.IsError.Should().BeFalse();
    }

    [Fact]
    public void TryCreate_WithNegativeInt_ReturnsFalseAndSetsValidationError()
    {
        const int negative = -5;

        var success = DepartmentId.TryCreate(negative, out var result, out var validationError);

        success.Should().BeFalse();
        result.IsDefault.Should().BeTrue();
        validationError.IsError.Should().BeTrue();
        validationError.Code.Should().Be("NEGATIVE");
        validationError.Message.Should().Be("DepartmentId cannot be negative.");
    }

    [Fact]
    public void TryCreate_WithValidRangeInt_ReturnsTrueAndSetsResult()
    {
        const int raw = 50;

        var success = NumericRangeId.TryCreate(raw, out var result, out var validationError);

        success.Should().BeTrue();
        result.Value.Should().Be(raw);
        validationError.IsError.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void TryCreate_WithOutOfRangeInt_ReturnsFalseAndSetsValidationError(int invalidValue)
    {
        var success = NumericRangeId.TryCreate(invalidValue, out var result, out var validationError);

        success.Should().BeFalse();
        result.IsDefault.Should().BeTrue();
        validationError.IsError.Should().BeTrue();
        validationError.Code.Should().Be("RANGE");
        validationError.Message.Should().Be("Value is outside permissible range [1, 100].");
    }

    [Fact]
    public void TryCreate_WithValidDateOnly_ReturnsTrueAndSetsResult()
    {
        var date = new DateOnly(2026, 8, 20);

        var success = DateOnlyId.TryCreate(date, out var result, out var validationError);

        success.Should().BeTrue();
        result.Value.Should().Be(date);
        validationError.IsError.Should().BeFalse();
    }

    [Fact]
    public void TryCreate_WithDefaultDateOnly_ReturnsFalseAndSetsValidationError()
    {
        var success = DateOnlyId.TryCreate(default, out var result, out var validationError);

        success.Should().BeFalse();
        result.IsDefault.Should().BeTrue();
        validationError.IsError.Should().BeTrue();
        validationError.Code.Should().Be("DEFAULT");
        validationError.Message.Should().Be("DateOnlyId cannot be default.");
    }

    [Fact]
    public void TryCreate_WithLongValue_ReturnsTrueAndSetsResult()
    {
        const long raw = 9876543210L;

        var success = LongOrderId.TryCreate(raw, out var result, out var validationError);

        success.Should().BeTrue();
        result.Value.Should().Be(raw);
        validationError.IsError.Should().BeFalse();
    }

    #endregion

    #region Property-Based Tests (FsCheck)

    [Property]
    public Property StrongId_Guid_RoundtripPreservesValue(Guid idValue)
    {
        // Discard invalid domain generator values (Guid.Empty) via FsCheck precondition filtering
        if (idValue == Guid.Empty)
            return false.When(false);

        var strongId = OrderId.Create(idValue);
        return (strongId.Value == idValue && OrderId.Create(strongId.Value).Equals(strongId)).When(idValue != Guid.Empty);
    }

    [Property]
    public Property StrongId_Int_RoundtripPreservesValue(int rawValue)
    {
        int value = Math.Abs(rawValue);
        var strongId = DepartmentId.Create(value);
        return (strongId.Value == value && DepartmentId.Create(strongId.Value).Equals(strongId)).When(value >= 0);
    }

    [Property]
    public Property StrongId_Long_RoundtripPreservesValue(long value)
    {
        var strongId = SequenceId.Create(value);
        return (strongId.Value == value && SequenceId.Create(strongId.Value).Equals(strongId)).ToProperty();
    }

    [Property]
    public Property StrongId_String_RoundtripPreservesValue(NonNull<string> nonNullString)
    {
        var raw = nonNullString.Get;
        // Discard whitespace and synthetic error token strings via FsCheck precondition filtering
        if (string.IsNullOrWhiteSpace(raw) || raw == "FORMAT_ERR" || raw == "FORMAT_ERROR")
            return false.When(false);

        var strongId = ProductCode.Create(raw);
        return (strongId.Value == raw && ProductCode.Create(strongId.Value).Equals(strongId)).ToProperty();
    }

    #endregion
}
