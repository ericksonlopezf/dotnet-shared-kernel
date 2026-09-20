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
}
