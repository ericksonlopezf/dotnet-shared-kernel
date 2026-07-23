namespace EricksonLopez.SharedKernel.Domain;

/// <summary>
/// Marker interface for domain events.
/// </summary>
/// <remarks>
/// <para>
/// Implement this interface on any record or class that represents something
/// meaningful that happened in the domain.
/// </para>
/// <para>
/// Domain events are raised by <see cref="AggregateRoot{TId}"/> and dispatched
/// by the infrastructure layer (e.g., after persisting changes via Unit of Work).
/// </para>
/// <para>
/// <b>Design note:</b> This interface is intentionally empty. Infrastructure
/// metadata (event ID, timestamp, correlation ID) belongs in the messaging
/// infrastructure (e.g., Outbox message envelope), not in the domain event itself.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed record OrderCreated(Guid OrderId, decimal Total) : IDomainEvent;
/// </code>
/// </example>
public interface IDomainEvent;
