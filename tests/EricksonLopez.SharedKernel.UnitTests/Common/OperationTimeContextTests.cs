// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using Xunit;

namespace EricksonLopez.SharedKernel.UnitTests.Common;

public sealed class OperationTimeContextTests
{
    [Fact]
    public void Current_WithoutScope_ShouldBeNull()
    {
        OperationTimeContext.Current.Should().BeNull();
    }

    [Fact]
    public void CurrentOrUtcNow_WithoutScope_ShouldReturnRecentUtcTime()
    {
        var before = DateTimeOffset.UtcNow;
        var actual = OperationTimeContext.CurrentOrUtcNow();
        var after = DateTimeOffset.UtcNow;

        actual.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void BeginScope_ShouldSetCurrentTimestamp_AndRevertOnDispose()
    {
        var expected = new DateTimeOffset(2026, 8, 31, 12, 0, 0, TimeSpan.Zero);

        using (OperationTimeContext.BeginScope(expected))
        {
            OperationTimeContext.Current.Should().Be(expected);
            OperationTimeContext.CurrentOrUtcNow().Should().Be(expected);
        }

        OperationTimeContext.Current.Should().BeNull();
    }

    [Fact]
    public void BeginScope_NestedScopes_ShouldRestoreParentTimestamp()
    {
        var parentTime = new DateTimeOffset(2026, 8, 31, 10, 0, 0, TimeSpan.Zero);
        var childTime = new DateTimeOffset(2026, 8, 31, 11, 0, 0, TimeSpan.Zero);

        using (OperationTimeContext.BeginScope(parentTime))
        {
            OperationTimeContext.Current.Should().Be(parentTime);

            using (OperationTimeContext.BeginScope(childTime))
            {
                OperationTimeContext.Current.Should().Be(childTime);
            }

            OperationTimeContext.Current.Should().Be(parentTime);
        }

        OperationTimeContext.Current.Should().BeNull();
    }

    [Fact]
    public void Dispose_MultipleTimes_ShouldBeIdempotent()
    {
        var expected = new DateTimeOffset(2026, 8, 31, 12, 0, 0, TimeSpan.Zero);
        var scope = OperationTimeContext.BeginScope(expected);

        OperationTimeContext.Current.Should().Be(expected);

        scope.Dispose();
        scope.Dispose();

        OperationTimeContext.Current.Should().BeNull();
    }

    [Fact]
    public void BeginScope_WithDefaultTimestamp_ThrowsArgumentException()
    {
        var act = () => OperationTimeContext.BeginScope(default);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("timestamp");
    }

    [Fact]
    public void Reset_ClearsAmbientStack()
    {
        var timestamp = new DateTimeOffset(2026, 8, 31, 12, 0, 0, TimeSpan.Zero);
        using (OperationTimeContext.BeginScope(timestamp))
        {
            OperationTimeContext.Current.Should().Be(timestamp);
            OperationTimeContext.Reset();
            OperationTimeContext.Current.Should().BeNull();
        }
    }

    [Fact]
    public void BeginScope_OutOfOrderDisposal_OuterBeforeInner_MaintainsInnerUntilDisposed()
    {
        var outerTime = new DateTimeOffset(2026, 8, 31, 10, 0, 0, TimeSpan.Zero);
        var innerTime = new DateTimeOffset(2026, 8, 31, 11, 0, 0, TimeSpan.Zero);

        var outerScope = OperationTimeContext.BeginScope(outerTime);
        var innerScope = OperationTimeContext.BeginScope(innerTime);

        OperationTimeContext.Current.Should().Be(innerTime);

        // Disposing outer while inner is still active (out of order)
        outerScope.Dispose();

        // Inner should still be the active scope
        OperationTimeContext.Current.Should().Be(innerTime);

        // Now disposing inner should leave stack empty because outer was already filtered out
        innerScope.Dispose();
        OperationTimeContext.Current.Should().BeNull();
    }

    [Fact]
    public void BeginScope_OutOfOrderDisposal_MiddleScope_PreservesRemainingScopes()
    {
        var time1 = new DateTimeOffset(2026, 8, 31, 10, 0, 0, TimeSpan.Zero);
        var time2 = new DateTimeOffset(2026, 8, 31, 11, 0, 0, TimeSpan.Zero);
        var time3 = new DateTimeOffset(2026, 8, 31, 12, 0, 0, TimeSpan.Zero);

        var scope1 = OperationTimeContext.BeginScope(time1);
        var scope2 = OperationTimeContext.BeginScope(time2);
        var scope3 = OperationTimeContext.BeginScope(time3);

        OperationTimeContext.Current.Should().Be(time3);

        // Dispose middle scope (scope2)
        scope2.Dispose();
        scope2.Dispose(); // Idempotent when out of order

        // Active scope is still scope3
        OperationTimeContext.Current.Should().Be(time3);

        // Dispose top scope (scope3) -> stack should revert directly to scope1
        scope3.Dispose();
        OperationTimeContext.Current.Should().Be(time1);

        // Dispose bottom scope (scope1) -> stack should be empty
        scope1.Dispose();
        OperationTimeContext.Current.Should().BeNull();
    }
}
