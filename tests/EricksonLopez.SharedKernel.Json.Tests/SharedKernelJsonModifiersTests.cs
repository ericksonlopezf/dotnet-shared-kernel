// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.Json;
using Xunit;

namespace EricksonLopez.SharedKernel.Json.Tests;

public sealed record SampleOrderCreatedEvent(string CustomerName, decimal Amount) : DomainEvent;

public class SharedKernelJsonModifiersTests
{
    private static readonly JsonSerializerOptions EventOptions = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { SharedKernelJsonModifiers.IgnoreLegacyDomainEventAliases }
        }
    };

    [Fact]
    public void IgnoreLegacyDomainEventAliases_NullTypeInfo_ThrowsArgumentNullException()
    {
        var act = () => SharedKernelJsonModifiers.IgnoreLegacyDomainEventAliases(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("typeInfo");
    }

    [Fact]
    public void IgnoreLegacyDomainEventAliases_WhenDomainEventSerialized_ExcludesEventIdAndOccurredOn()
    {
        var evt = new SampleOrderCreatedEvent("John Doe", 99.95m);
        var json = JsonSerializer.Serialize(evt, EventOptions);

        json.Should().Contain("\"Id\":");
        json.Should().Contain("\"OccurredAt\":");
        json.Should().Contain("\"CustomerName\":\"John Doe\"");
        json.Should().Contain("\"Amount\":99.95");
        json.Should().NotContain("\"EventId\":");
        json.Should().NotContain("\"OccurredOn\":");
    }

    [Fact]
    public void IgnoreLegacyDomainEventAliases_WhenNonDomainEventSerialized_LeavesPropertiesIntact()
    {
        var obj = new { EventId = Guid.NewGuid(), OccurredOn = DateTimeOffset.UtcNow, Name = "Test" };
        var json = JsonSerializer.Serialize(obj, EventOptions);

        json.Should().Contain("\"EventId\":");
        json.Should().Contain("\"OccurredOn\":");
        json.Should().Contain("\"Name\":\"Test\"");
    }
}
