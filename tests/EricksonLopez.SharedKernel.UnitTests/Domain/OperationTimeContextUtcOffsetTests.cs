// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.SharedKernel;
using Xunit;

namespace EricksonLopez.SharedKernel.UnitTests.Domain;

public sealed record SampleTimeEvent : DomainEvent;

public class OperationTimeContextUtcOffsetTests
{
    [Fact]
    public void BeginScope_WithNonUtcOffset_NormalizesToUtcOffset()
    {
        var nonUtc = new DateTimeOffset(2026, 1, 1, 15, 0, 0, TimeSpan.FromHours(-5));

        using (OperationTimeContext.BeginScope(nonUtc))
        {
            var current = OperationTimeContext.Current;
            current.Should().NotBeNull();
            current!.Value.Offset.Should().Be(TimeSpan.Zero);
            current.Value.Hour.Should().Be(20);

            var @event = new SampleTimeEvent();
            @event.OccurredAt.Offset.Should().Be(TimeSpan.Zero);
            @event.OccurredAt.Hour.Should().Be(20);
        }
    }
}
