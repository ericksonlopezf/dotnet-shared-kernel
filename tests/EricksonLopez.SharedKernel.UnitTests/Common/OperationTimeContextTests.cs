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
}
