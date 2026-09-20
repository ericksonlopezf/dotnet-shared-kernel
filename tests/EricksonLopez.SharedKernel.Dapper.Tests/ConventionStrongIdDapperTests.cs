// Copyright © Erickson Lopez. MIT License.
using System;
using System.Reflection;
using AwesomeAssertions;
using Dapper;
using EricksonLopez.SharedKernel.Dapper;
using EricksonLopez.SharedKernel.Dapper.Tests.Fakes;
using Xunit;

#pragma warning disable CS0618
namespace EricksonLopez.SharedKernel.Dapper.Tests;

/// <summary>
/// Verifies convention-based Dapper type handler registration for strongly-typed domain identifiers.
/// </summary>
[Collection("DapperRegistryTests")]
public class ConventionStrongIdDapperTests
{
    [Fact]
    public void RegisterByConvention_WithNullAssembly_ThrowsArgumentNullException()
    {
        var act = () => DapperStrongIdRegistry.RegisterByConvention(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("assembly");
    }

    [Fact]
    public void RegisterByConvention_WithCustomFilter_RegistersMatchingTypes()
    {
        var assembly = typeof(FakeConventionUserId).Assembly;

        // Register with filter selecting FakeConventionUserId
        DapperStrongIdRegistry.RegisterByConvention(assembly, type => type == typeof(FakeConventionUserId));

        var expectedGuid = Guid.NewGuid();
        SqlMapper.LookupDbType(typeof(FakeConventionUserId), "col", false, out var handler);

        handler.Should().NotBeNull();
        handler.Should().BeOfType<ConventionStrongIdTypeHandler<FakeConventionUserId>>();

        var param = new FakeDbDataParameter();
        handler!.SetValue(param, new FakeConventionUserId(expectedGuid));
        param.Value.Should().Be(expectedGuid);

        var parsed = handler.Parse(typeof(FakeConventionUserId), expectedGuid);
        parsed.Should().BeOfType<FakeConventionUserId>();
        ((FakeConventionUserId)parsed).Value.Should().Be(expectedGuid);
    }

    [Fact]
    public void RegisterByConvention_WithoutFilter_AppliesDefaultNamingConvention()
    {
        var assembly = typeof(FakeConventionUserId).Assembly;

        // Register with default naming convention (*Id struct with Value property)
        DapperStrongIdRegistry.RegisterByConvention(assembly);

        SqlMapper.LookupDbType(typeof(FakeConventionUserId), "col", false, out var handler);
        handler.Should().NotBeNull();
    }

    public readonly record struct TestGuidId(Guid Value);
    public readonly record struct TestStringId(string Value);
    public readonly record struct TestIntId(int Value);
    public readonly record struct TestLongId(long Value);
    public readonly record struct TestShortId(short Value);
    public readonly record struct TestByteId(byte Value);
    public readonly record struct TestBoolId(bool Value);
    public readonly record struct TestDateTimeId(DateTime Value);
    public readonly record struct TestDateTimeOffsetId(DateTimeOffset Value);
    public readonly record struct TestDateOnlyId(DateOnly Value);
    public readonly record struct TestTimeOnlyId(TimeOnly Value);
    public readonly record struct TestDecimalId(decimal Value);
    public readonly record struct TestDoubleId(double Value);
    public readonly record struct TestFloatId(float Value);
    public readonly record struct TestCustomObjId(object Value);
    public class TestClassStringId { public string? Value { get; set; } public TestClassStringId(string? value) { Value = value; } }

    private static ConventionStrongIdTypeHandler<T> CreateHandler<T>()
    {
        var prop = typeof(T).GetProperty("Value")!;
        var ctor = typeof(T).GetConstructor([prop.PropertyType])!;
        return new ConventionStrongIdTypeHandler<T>(prop, ctor);
    }

    [Fact]
    public void Constructor_WithNullPropertyOrConstructor_ThrowsArgumentNullException()
    {
        var prop = typeof(TestGuidId).GetProperty("Value")!;
        var ctor = typeof(TestGuidId).GetConstructor([typeof(Guid)])!;

        var act1 = () => new ConventionStrongIdTypeHandler<TestGuidId>(null!, ctor);
        act1.Should().Throw<ArgumentNullException>().WithParameterName("valueProp");

        var act2 = () => new ConventionStrongIdTypeHandler<TestGuidId>(prop, null!);
        act2.Should().Throw<ArgumentNullException>().WithParameterName("constructor");
    }

    [Fact]
    public void SetValue_WithNullParameter_ThrowsArgumentNullException()
    {
        var handler = CreateHandler<TestGuidId>();
        var act = () => handler.SetValue(null!, new TestGuidId(Guid.NewGuid()));
        act.Should().Throw<ArgumentNullException>().WithParameterName("parameter");
    }

    [Fact]
    public void SetValue_WithNullInstanceOrNullInner_SetsDBNull()
    {
        var handler = CreateHandler<TestClassStringId>();
        var param = new FakeDbDataParameter();

        handler.SetValue(param, null);
        param.Value.Should().Be(DBNull.Value);

        handler.SetValue(param, new TestClassStringId(null));
        param.Value.Should().Be(DBNull.Value);
    }

    [Fact]
    public void SetValue_InfersCorrectDbTypes()
    {
        void VerifyDbType<T>(T instance, System.Data.DbType? expectedDbType)
        {
            var handler = CreateHandler<T>();
            var param = new FakeDbDataParameter();
            handler.SetValue(param, instance);
            if (expectedDbType.HasValue)
            {
                param.DbType.Should().Be(expectedDbType.Value);
            }
        }

        VerifyDbType(new TestGuidId(Guid.NewGuid()), System.Data.DbType.Guid);
        VerifyDbType(new TestStringId("test"), System.Data.DbType.String);
        VerifyDbType(new TestIntId(42), System.Data.DbType.Int32);
        VerifyDbType(new TestLongId(42L), System.Data.DbType.Int64);
        VerifyDbType(new TestShortId((short)42), System.Data.DbType.Int16);
        VerifyDbType(new TestByteId((byte)42), System.Data.DbType.Byte);
        VerifyDbType(new TestBoolId(true), System.Data.DbType.Boolean);
        VerifyDbType(new TestDateTimeId(DateTime.UtcNow), System.Data.DbType.DateTime2);
        VerifyDbType(new TestDateTimeOffsetId(DateTimeOffset.UtcNow), System.Data.DbType.DateTimeOffset);
        VerifyDbType(new TestDateOnlyId(new DateOnly(2026, 9, 20)), System.Data.DbType.Date);
        VerifyDbType(new TestTimeOnlyId(new TimeOnly(12, 0)), System.Data.DbType.Time);
        VerifyDbType(new TestDecimalId(10.5m), System.Data.DbType.Decimal);
        VerifyDbType(new TestDoubleId(10.5), System.Data.DbType.Double);
        VerifyDbType(new TestFloatId(10.5f), System.Data.DbType.Single);
        VerifyDbType(new TestCustomObjId(new object()), null);
    }

    [Fact]
    public void Parse_WithNullOrDBNull_ThrowsDataException()
    {
        var handler = CreateHandler<TestGuidId>();
        var act1 = () => handler.Parse(null!);
        act1.Should().Throw<System.Data.DataException>();

        var act2 = () => handler.Parse(DBNull.Value);
        act2.Should().Throw<System.Data.DataException>();
    }

    [Fact]
    public void Parse_WithGuidAndString_HandlesBoth()
    {
        var handler = CreateHandler<TestGuidId>();
        var guid = Guid.NewGuid();

        handler.Parse(guid).Value.Should().Be(guid);
        handler.Parse(guid.ToString()).Value.Should().Be(guid);

        var act = () => handler.Parse("not-a-valid-guid");
        act.Should().Throw<System.Data.DataException>();
    }

    [Fact]
    public void Parse_WithConvertibleAndIncompatibleTypes()
    {
        var handler = CreateHandler<TestIntId>();

        handler.Parse(42).Value.Should().Be(42);
        handler.Parse("42").Value.Should().Be(42);
        handler.Parse(42L).Value.Should().Be(42);

        var act = () => handler.Parse("not-a-number");
        act.Should().Throw<System.Data.DataException>();
    }
}
