// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;

namespace EricksonLopez.SharedKernel;

/// <summary>
/// Represents an abstract base record for immutable domain events.
/// </summary>
/// <remarks>
/// Instances are immutable and assigned a time-ordered UUIDv7 identifier (<see cref="EventId"/>) upon creation.
/// </remarks>
public abstract record DomainEvent : IDomainEvent
{
    /// <summary>
    /// Gets the unique, time-ordered identifier of the domain event.
    /// </summary>
    public EventId Id { get; }

    /// <summary>
    /// Gets the UTC timestamp at which the domain event occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; }

    /// <summary>
    /// Gets the underlying GUID value of the domain event identifier.
    /// </summary>
    public Guid EventId => Id.Value;

    /// <summary>
    /// Gets the UTC occurrence timestamp as a backward-compatibility alias for <see cref="OccurredAt"/>.
    /// </summary>
    public DateTimeOffset OccurredOn => OccurredAt;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEvent"/> class with a new time-ordered identifier and the current UTC timestamp.
    /// </summary>
    protected DomainEvent()
    {
        Id = EricksonLopez.Events.Identifiers.EventId.New();
        OccurredAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEvent"/> class with the specified identifier and occurrence timestamp.
    /// </summary>
    /// <param name="id">The unique identifier of the domain event.</param>
    /// <param name="occurredAt">The date and time in UTC when the domain event occurred.</param>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty, or <paramref name="occurredAt"/> is equal to <see langword="default"/></exception>
    protected DomainEvent(
        EventId id,
        DateTimeOffset occurredAt)
    {
        if (id.IsEmpty)
        {
            throw new ArgumentException(
                "Domain event identifier cannot be empty.",
                nameof(id));
        }

        if (occurredAt == default)
        {
            throw new ArgumentException(
                "Domain event timestamp cannot be default.",
                nameof(occurredAt));
        }

        Id = id;
        OccurredAt = occurredAt;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEvent"/> class from raw identifier and timestamp values for rehydration scenarios.
    /// </summary>
    /// <param name="eventId">The raw unique identifier of the domain event.</param>
    /// <param name="occurredOn">The date and time in UTC when the domain event occurred.</param>
    /// <exception cref="ArgumentException"><paramref name="eventId"/> is equal to <see cref="Guid.Empty"/>, or <paramref name="occurredOn"/> is equal to <see langword="default"/></exception>
    protected DomainEvent(
        Guid eventId,
        DateTimeOffset occurredOn)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException(
                "Domain event identifier cannot be empty.",
                nameof(eventId));
        }

        if (occurredOn == default)
        {
            throw new ArgumentException(
                "Domain event timestamp cannot be default.",
                nameof(occurredOn));
        }

        Id = new EventId(eventId);
        OccurredAt = occurredOn;
    }
}
