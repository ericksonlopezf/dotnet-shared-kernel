namespace EricksonLopez.SharedKernel.Domain;

/// <summary>
/// Marker interface for domain events.
/// Implement this interface on any class that represents a domain event.
/// Domain events are raised by entities to signal that something meaningful happened in the domain.
/// </summary>
public interface IDomainEvent;
