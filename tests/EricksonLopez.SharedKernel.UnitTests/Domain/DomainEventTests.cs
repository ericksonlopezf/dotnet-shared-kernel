// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.UnitTests.Common;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace EricksonLopez.SharedKernel.UnitTests.Domain;

public class DomainEventTests
{
    private sealed record UserCreatedEvent(string UserName) : DomainEvent;
    private sealed record OtherUserEvent(string UserName) : DomainEvent;

    private sealed record RehydratedUserEvent : DomainEvent
    {
        public string UserName { get; }

        public RehydratedUserEvent(Guid eventId, DateTimeOffset occurredOn, string userName)
            : base(eventId, occurredOn)
        {
            UserName = userName;
        }

        public RehydratedUserEvent(EventId id, DateTimeOffset occurredAt, string userName)
            : base(id, occurredAt)
        {
            UserName = userName;
        }
    }

    private sealed class FakeDispatcher : IDomainEventDispatcher
    {
        public IReadOnlyList<IDomainEvent>? LastDispatchedEvents { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }

        public ValueTask DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            LastDispatchedEvents = domainEvents;
            LastCancellationToken = cancellationToken;
            return ValueTask.CompletedTask;
        }
    }

    #region Default Constructor

    [Fact]
    public void DefaultConstructor_GeneratesValidIdAndUtcTimestamp()
    {
        var before = DateTimeOffset.UtcNow;
        var @event = new UserCreatedEvent(TestValues.Strings.UserName);
        var after = DateTimeOffset.UtcNow;

        @event.Id.IsEmpty.Should().BeFalse();
        @event.EventId.Should().NotBe(Guid.Empty);
        @event.EventId.Should().Be(@event.Id.Value);

        @event.OccurredAt.Offset.Should().Be(TimeSpan.Zero);
        @event.OccurredAt.Should().BeOnOrAfter(before);
        @event.OccurredAt.Should().BeOnOrBefore(after);

        @event.OccurredOn.Should().Be(@event.OccurredAt);

#if NET9_0_OR_GREATER
        @event.EventId.Version.Should().Be(7, because: "DomainEvent EventId must be a sequential UUIDv7 identifier in .NET 9+.");
#endif
    }

    #endregion

    #region EventId Constructor & Validations

    [Fact]
    public void EventIdConstructor_WithValidParameters_SetsPropertiesCorrectly()
    {
        var rawGuid = Guid.NewGuid();
        var eventId = new EventId(rawGuid);
        var customDate = new DateTimeOffset(2026, 6, 15, 10, 30, 0, TimeSpan.Zero);

        var @event = new RehydratedUserEvent(eventId, customDate, TestValues.Strings.UserName);

        @event.Id.Should().Be(eventId);
        @event.EventId.Should().Be(rawGuid);
        @event.OccurredAt.Should().Be(customDate);
        @event.OccurredOn.Should().Be(customDate);
        @event.UserName.Should().Be(TestValues.Strings.UserName);
    }

    [Fact]
    public void EventIdConstructor_WithEmptyEventId_ThrowsArgumentException()
    {
        var emptyId = EventId.Empty;
        var act = () => new RehydratedUserEvent(emptyId, DateTimeOffset.UtcNow, TestValues.Strings.UserName);

        var ex = act.Should().Throw<ArgumentException>()
            .WithParameterName("id")
            .Which;

        ex.Message.Should().Contain("Domain event identifier cannot be empty.");
    }

    [Fact]
    public void EventIdConstructor_WithDefaultTimestamp_ThrowsArgumentException()
    {
        var eventId = EventId.New();
        var act = () => new RehydratedUserEvent(eventId, default, TestValues.Strings.UserName);

        var ex = act.Should().Throw<ArgumentException>()
            .WithParameterName("occurredAt")
            .Which;

        ex.Message.Should().Contain("Domain event timestamp cannot be default.");
    }

    [Fact]
    public void EventIdConstructor_WithBoundaryTimestamp_AllowsCreation()
    {
        var eventId = EventId.New();
        var boundaryTime = DateTimeOffset.MinValue.AddTicks(1);
        
        var @event = new RehydratedUserEvent(eventId, boundaryTime, TestValues.Strings.UserName);

        @event.OccurredAt.Should().Be(boundaryTime);
    }


    #endregion

    #region Guid Rehydration Constructor & Validations

    [Fact]
    public void GuidConstructor_SetsExplicitEventIdAndOccurredOn()
    {
        var customId = Guid.NewGuid();
        var customDate = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        var @event = new RehydratedUserEvent(customId, customDate, TestValues.Strings.UserName);

        @event.EventId.Should().Be(customId);
        @event.Id.Value.Should().Be(customId);
        @event.OccurredOn.Should().Be(customDate);
        @event.OccurredAt.Should().Be(customDate);
        @event.UserName.Should().Be(TestValues.Strings.UserName);
    }

    [Fact]
    public void GuidConstructor_WithEmptyGuid_ThrowsArgumentException()
    {
        var act = () => new RehydratedUserEvent(Guid.Empty, DateTimeOffset.UtcNow, TestValues.Strings.UserName);

        var ex = act.Should().Throw<ArgumentException>()
            .WithParameterName("eventId")
            .Which;

        ex.Message.Should().Contain("Domain event identifier cannot be empty.");
    }

    [Fact]
    public void GuidConstructor_WithDefaultTimestamp_ThrowsArgumentException()
    {
        var act = () => new RehydratedUserEvent(Guid.NewGuid(), default, TestValues.Strings.UserName);

        var ex = act.Should().Throw<ArgumentException>()
            .WithParameterName("occurredOn")
            .Which;

        ex.Message.Should().Contain("Domain event timestamp cannot be default.");
    }

    [Fact]
    public void GuidConstructor_WithBoundaryTimestamp_AllowsCreation()
    {
        var boundaryTime = DateTimeOffset.MinValue.AddTicks(1);
        var @event = new RehydratedUserEvent(Guid.NewGuid(), boundaryTime, TestValues.Strings.UserName);

        @event.OccurredAt.Should().Be(boundaryTime);
    }


    #endregion

    #region Equality & HashCode

    [Fact]
    public void Equals_WithSameValuesAndType_ReturnsTrue()
    {
        var eventId = Guid.NewGuid();
        var occurredOn = DateTimeOffset.UtcNow;

        var event1 = new RehydratedUserEvent(eventId, occurredOn, TestValues.Strings.UserName);
        var event2 = new RehydratedUserEvent(eventId, occurredOn, TestValues.Strings.UserName);

        event1.Equals(event2).Should().BeTrue();
        (event1 == event2).Should().BeTrue();
        (event1 != event2).Should().BeFalse();
        event1.GetHashCode().Should().Be(event2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentEventId_ReturnsFalse()
    {
        var occurredOn = DateTimeOffset.UtcNow;

        var event1 = new RehydratedUserEvent(Guid.NewGuid(), occurredOn, TestValues.Strings.UserName);
        var event2 = new RehydratedUserEvent(Guid.NewGuid(), occurredOn, TestValues.Strings.UserName);

        event1.Equals(event2).Should().BeFalse();
        (event1 == event2).Should().BeFalse();
        (event1 != event2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        var eventId = Guid.NewGuid();
        var occurredOn = DateTimeOffset.UtcNow;

        var event1 = new RehydratedUserEvent(eventId, occurredOn, TestValues.Strings.UserName);
        var event2 = new OtherUserEvent(TestValues.Strings.UserName);

        event1.Equals(event2).Should().BeFalse();
        (event1 == event2).Should().BeFalse();
    }

    [Fact]
    public void ToString_IncludesRecordTypeNameAndProperties()
    {
        var @event = new UserCreatedEvent(TestValues.Strings.UserName);
        var stringRep = @event.ToString();

        stringRep.Should().Contain("UserCreatedEvent");
        stringRep.Should().Contain(TestValues.Strings.UserName);
    }

    #endregion

    #region Interface Contracts & Dispatcher

    [Fact]
    public void IDomainEvent_PolymorphicAccess_ExposesPropertiesCorrectly()
    {
        var rawGuid = Guid.NewGuid();
        var date = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var @event = new RehydratedUserEvent(rawGuid, date, "test-user");

        IDomainEvent iEvent = @event;
        iEvent.Id.Value.Should().Be(rawGuid);
        iEvent.OccurredAt.Should().Be(date);
    }

    [Fact]
    public async Task IDomainEventDispatcher_Contract_DispatchesBatchAsync()
    {
        var dispatcher = new FakeDispatcher();
        var events = new IDomainEvent[]
        {
            new UserCreatedEvent("Alice"),
            new UserCreatedEvent("Bob")
        };

        using var cts = new CancellationTokenSource();
        await dispatcher.DispatchAsync(events, cts.Token);

        dispatcher.LastDispatchedEvents.Should().BeSameAs(events);
        dispatcher.LastCancellationToken.Should().Be(cts.Token);
    }

    #endregion

    #region Property-Based Testing (FsCheck)

    [Property]
    public Property RehydratedEvent_WithValidGuidAndDate_PreservesIdentity(NonNull<string> name)
    {
        var guid = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;

        var @event = new RehydratedUserEvent(guid, date, name.Get);

        return (@event.EventId == guid &&
                @event.Id.Value == guid &&
                @event.OccurredOn == date &&
                @event.OccurredAt == date &&
                @event.UserName == name.Get).ToProperty();
    }

    #endregion
}

