namespace EricksonLopez.SharedKernel.Domain;

/// <summary>
/// Base class for all domain entities.
/// </summary>
/// <remarks>
/// An entity is defined by its identity (Id), not its attributes.
/// Two entities are equal if and only if they share the same Id and the same concrete type.
/// This class manages the collection of domain events raised by the entity.
/// </remarks>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// The unique identifier of this entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Read-only view of domain events raised by this entity since the last dispatch.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event. The event will be dispatched by the infrastructure layer
    /// (e.g., after SaveChanges in the Unit of Work).
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all pending domain events. Should be called by the infrastructure layer
    /// after dispatching events.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    // ─── Equality ───────────────────────────────────────────────────────────────

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        var other = (Entity<TId>)obj;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);
}
