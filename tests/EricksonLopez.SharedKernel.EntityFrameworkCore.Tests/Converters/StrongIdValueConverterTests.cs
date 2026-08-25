// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Converters;

using AwesomeAssertions;
using EricksonLopez.SharedKernel.EntityFrameworkCore;
using EricksonLopez.SharedKernel.TestingUtilities.Fakes;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

public class StrongIdValueConverterTests
{
    [Fact]
    public void StrongIdValueConverter_DefaultConstructor_ConvertsBetweenStrongIdAndPrimitive()
    {
        var converter = new StrongIdValueConverter<CustomerId, Guid>();
        var guid = Guid.NewGuid();
        var customerId = CustomerId.Create(guid);

        var toProvider = converter.ConvertToProvider(customerId);
        toProvider.Should().Be(guid);

        var fromProvider = converter.ConvertFromProvider(guid);
        fromProvider.Should().Be(customerId);
    }

    [Fact]
    public void StrongIdValueConverter_WithMappingHints_InitializesCorrectly()
    {
        var hints = new ConverterMappingHints(size: 36);
        var converter = new StrongIdValueConverter<CustomerId, Guid>(hints);

        converter.MappingHints.Should().NotBeNull();
        converter.MappingHints!.Size.Should().Be(36);

        var guid = Guid.NewGuid();
        var customerId = CustomerId.Create(guid);
        converter.ConvertToProvider(customerId).Should().Be(guid);
        converter.ConvertFromProvider(guid).Should().Be(customerId);
    }

    [Fact]
    public void StrongIdValueConverter_WithNullMappingHints_InitializesCorrectly()
    {
        var converter = new StrongIdValueConverter<CustomerId, Guid>((ConverterMappingHints?)null);

        converter.MappingHints.Should().BeNull();

        var guid = Guid.NewGuid();
        var customerId = CustomerId.Create(guid);
        converter.ConvertToProvider(customerId).Should().Be(guid);
        converter.ConvertFromProvider(guid).Should().Be(customerId);
    }

    [Fact]
    public void StrongIdValueConverter_WithCustomFactory_UsesFactoryDelegate()
    {
        Func<long, LongOrderId> factory = val => new LongOrderId(val * 10);
        var converter = new StrongIdValueConverter<LongOrderId, long>(factory);

        var orderId = new LongOrderId(50);
        converter.ConvertToProvider(orderId).Should().Be(50L);
        var created = (LongOrderId)converter.ConvertFromProvider(5L)!;
        created.Value.Should().Be(50L);
    }

    [Fact]
    public void StrongIdValueConverter_WithCustomFactoryAndMappingHints_InitializesBothCorrectly()
    {
        var hints = new ConverterMappingHints(size: 20);
        Func<long, LongOrderId> factory = val => new LongOrderId(val * 2);
        var converter = new StrongIdValueConverter<LongOrderId, long>(factory, hints);

        converter.MappingHints.Should().NotBeNull();
        converter.MappingHints!.Size.Should().Be(20);

        var orderId = new LongOrderId(40);
        converter.ConvertToProvider(orderId).Should().Be(40L);
        var created = (LongOrderId)converter.ConvertFromProvider(20L)!;
        created.Value.Should().Be(40L);
    }

    [Fact]
    public void StrongIdValueConverter_WithNullFactory_ThrowsArgumentNullException()
    {
        var act = () => new StrongIdValueConverter<LongOrderId, long>((Func<long, LongOrderId>)null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("factory");
    }

    [Fact]
    public void StrongIdValueConverter_WithNonStandardValueType_ConvertsDateOnlyCorrectly()
    {
        var hints = new ConverterMappingHints(size: 10, unicode: false);
        var converter = new StrongIdValueConverter<DateOnlyId, DateOnly>(hints);

        converter.MappingHints.Should().NotBeNull();
        converter.MappingHints!.Size.Should().Be(10);
        converter.MappingHints.IsUnicode.Should().BeFalse();

        var date = new DateOnly(2026, 8, 17);
        var dateId = DateOnlyId.Create(date);

        var toProvider = converter.ConvertToProvider(dateId);
        toProvider.Should().Be(date);

        var fromProvider = converter.ConvertFromProvider(date);
        fromProvider.Should().Be(dateId);
    }

    [Fact]
    public void StrongIdValueConverter_WithStringValueType_ConvertsStringCorrectly()
    {
        var converter = new StrongIdValueConverter<ProductCode, string>();
        var code = ProductCode.Create("SKU-98765");

        var toProvider = converter.ConvertToProvider(code);
        toProvider.Should().Be("SKU-98765");

        var fromProvider = converter.ConvertFromProvider("SKU-98765");
        fromProvider.Should().Be(code);
    }

    [Fact]
    public void StrongIdValueConverter_ExpressionTrees_CompileAndExecuteCorrectly()
    {
        var converter = new StrongIdValueConverter<CustomerId, Guid>();
        var guid = Guid.NewGuid();
        var customerId = CustomerId.Create(guid);

        var toProviderFunc = converter.ConvertToProviderExpression.Compile();
        toProviderFunc(customerId).Should().Be(guid);

        var fromProviderFunc = converter.ConvertFromProviderExpression.Compile();
        fromProviderFunc(guid).Should().Be(customerId);
    }

    [Fact]
    public void StrongIdValueConverter_WithNullProviderValue_ReturnsNull()
    {
        var converter = new StrongIdValueConverter<ProductCode, string>();
        
        // EF Core ValueConverters automatically handle nulls bypassing the conversion logic, 
        // but we verify the base conversion method returns null safely.
        var fromProvider = converter.ConvertFromProvider(null);
        fromProvider.Should().BeNull();

        var toProvider = converter.ConvertToProvider(null);
        toProvider.Should().BeNull();
    }

    [Fact]
    public void StrongIdValueConverter_WithDefaultProviderValue_DelegatesToFactory()
    {
        // Tests what happens when EF Core reads a default value (e.g. 0, Guid.Empty) from the database.
        // It should simply delegate to the factory. If the factory throws (e.g. domain validation), that is correct.
        var converter = new StrongIdValueConverter<CustomerId, Guid>();
        
        var act = () => converter.ConvertFromProvider(Guid.Empty);
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    #region Property-Based Tests (FsCheck)

    [Property]
    public Property StrongIdValueConverter_Guid_RoundtripPreservesValue(Guid idValue)
    {
        // Discard invalid domain generator values (Guid.Empty) via FsCheck precondition filtering
        if (idValue == Guid.Empty)
            return false.When(false);

        var converter = new StrongIdValueConverter<CustomerId, Guid>();
        var customerId = CustomerId.Create(idValue);

        var providerValue = (Guid)converter.ConvertToProvider(customerId)!;
        var restored = (CustomerId)converter.ConvertFromProvider(providerValue)!;

        return (providerValue == idValue && restored.Equals(customerId) && restored.Value == idValue).When(idValue != Guid.Empty);
    }

    [Property]
    public Property StrongIdValueConverter_Int_RoundtripPreservesValue(PositiveInt positiveInt)
    {
        var converter = new StrongIdValueConverter<DepartmentId, int>();
        var deptId = DepartmentId.Create(positiveInt.Get);

        var providerValue = (int)converter.ConvertToProvider(deptId)!;
        var restored = (DepartmentId)converter.ConvertFromProvider(providerValue)!;

        return (providerValue == positiveInt.Get && restored.Equals(deptId) && restored.Value == positiveInt.Get).When(positiveInt.Get >= 0);
    }

    [Property]
    public Property StrongIdValueConverter_String_RoundtripPreservesValue(NonNull<string> nonNullString)
    {
        var raw = nonNullString.Get;
        // Discard whitespace and synthetic error token strings via FsCheck precondition filtering
        if (string.IsNullOrWhiteSpace(raw) || raw == "FORMAT_ERR" || raw == "FORMAT_ERROR")
            return false.When(false);

        var converter = new StrongIdValueConverter<ProductCode, string>();
        var productCode = ProductCode.Create(raw);

        var providerValue = (string)converter.ConvertToProvider(productCode)!;
        var restored = (ProductCode)converter.ConvertFromProvider(providerValue)!;

        return (providerValue == raw && restored.Equals(productCode) && restored.Value == raw).ToProperty();
    }

    #endregion
}


