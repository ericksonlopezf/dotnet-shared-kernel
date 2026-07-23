namespace EricksonLopez.SharedKernel.Domain;

/// <summary>
/// Base class for Value Objects — types defined by their structural equality.
/// </summary>
/// <remarks>
/// <para>
/// A value object has no identity — two value objects are equal if all their
/// components are equal. Value objects should be immutable.
/// </para>
/// <para>
/// Override <see cref="GetEqualityComponents"/> to define which properties
/// participate in equality. This approach uses <c>IEnumerable&lt;object?&gt;</c>,
/// which causes boxing for value-type components (int, decimal, etc.).
/// </para>
/// <para>
/// <b>Performance note:</b> For Value Objects on hot paths, override
/// <see cref="Equals(ValueObject?)"/> and <see cref="GetHashCode"/> directly
/// to avoid boxing allocations:
/// </para>
/// <code>
/// public sealed class Money : ValueObject
/// {
///     public decimal Amount { get; }
///     public string Currency { get; }
///
///     public Money(decimal amount, string currency)
///     {
///         Amount = amount;
///         Currency = currency;
///     }
///
///     // Standard approach (simple, but boxes value types):
///     protected override IEnumerable&lt;object?&gt; GetEqualityComponents()
///     {
///         yield return Amount;
///         yield return Currency;
///     }
///
///     // Optional: override for zero-boxing equality on hot paths:
///     // public override bool Equals(ValueObject? other)
///     //     =&gt; other is Money m &amp;&amp; Amount == m.Amount &amp;&amp; Currency == m.Currency;
///     // public override int GetHashCode()
///     //     =&gt; HashCode.Combine(Amount, Currency);
/// }
/// </code>
/// </remarks>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Returns the components used for structural equality comparison.
    /// </summary>
    /// <remarks>
    /// Yield all fields/properties that define this Value Object's identity.
    /// Note: Value types (int, decimal, etc.) will be boxed when yielded.
    /// For performance-critical scenarios, override <see cref="Equals(ValueObject?)"/>
    /// and <see cref="GetHashCode"/> directly instead.
    /// </remarks>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// Determines whether this value object is equal to another.
    /// </summary>
    /// <remarks>
    /// The default implementation compares all components from
    /// <see cref="GetEqualityComponents"/> using <see cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})"/>.
    /// Override this method for zero-boxing equality on hot paths.
    /// </remarks>
    public virtual bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj)
        => obj is ValueObject other && Equals(other);

    public override int GetHashCode()
        => GetEqualityComponents()
            .Aggregate(0, (hash, component) => HashCode.Combine(hash, component));

    public static bool operator ==(ValueObject? left, ValueObject? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !(left == right);
}
