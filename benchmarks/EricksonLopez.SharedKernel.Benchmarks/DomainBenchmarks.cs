using BenchmarkDotNet.Attributes;
using EricksonLopez.SharedKernel.Domain;

namespace EricksonLopez.SharedKernel.Benchmarks;

// ─── Test doubles (internal — file-scoped not allowed in public member signatures) ──

internal sealed record OrderPlaced(Guid OrderId) : IDomainEvent;
internal sealed record OrderShipped(Guid OrderId) : IDomainEvent;

internal sealed class BenchOrder : AggregateRoot<Guid>
{
    public static BenchOrder Create(Guid id)
    {
        var order = new BenchOrder { Id = id };
        order.RaiseDomainEvent(new OrderPlaced(id));
        return order;
    }

    public void Ship() => RaiseDomainEvent(new OrderShipped(Id));
}

internal sealed class BenchMoney : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public BenchMoney(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

internal sealed class BenchMoneyOptimized : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public BenchMoneyOptimized(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override bool Equals(ValueObject? other)
        => other is BenchMoneyOptimized m && Amount == m.Amount && Currency == m.Currency;

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
}

internal sealed class BenchEntity : Entity<Guid>
{
    public static BenchEntity Create(Guid id) => new() { Id = id };
}

// ─── Benchmarks ───────────────────────────────────────────────────────────────

/// <summary>
/// Benchmarks for Domain layer primitives:
/// <see cref="AggregateRoot{TId}"/>, <see cref="Entity{TId}"/>, and <see cref="ValueObject"/>.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class DomainBenchmarks
{
    private static readonly Guid Id1 = Guid.NewGuid();
    private static readonly Guid Id2 = Guid.NewGuid();

    private readonly BenchOrder _order = BenchOrder.Create(Guid.NewGuid());
    private readonly BenchEntity _entity1 = BenchEntity.Create(Id1);
    private readonly BenchEntity _entity2 = BenchEntity.Create(Id1); // same ID
    private readonly BenchEntity _entity3 = BenchEntity.Create(Id2); // different ID

    private readonly BenchMoney _money1 = new(100.5m, "USD");
    private readonly BenchMoney _money2 = new(100.5m, "USD");
    private readonly BenchMoneyOptimized _optimized1 = new(100.5m, "USD");
    private readonly BenchMoneyOptimized _optimized2 = new(100.5m, "USD");

    // ── AggregateRoot ─────────────────────────────────────────────────────────

    [Benchmark(Baseline = true, Description = "AggregateRoot.RaiseDomainEvent")]
    public void AggregateRoot_RaiseDomainEvent() => _order.Ship();

    [Benchmark(Description = "AggregateRoot.DomainEvents (ToArray snapshot)")]
    public IReadOnlyList<IDomainEvent> AggregateRoot_GetDomainEvents() => _order.DomainEvents;

    [Benchmark(Description = "AggregateRoot.ClearDomainEvents")]
    public void AggregateRoot_ClearDomainEvents() => _order.ClearDomainEvents();

    // ── Entity ────────────────────────────────────────────────────────────────

    [Benchmark(Description = "Entity.Equals — same ID (fast path)")]
    public bool Entity_Equals_SameId() => _entity1.Equals(_entity2);

    [Benchmark(Description = "Entity.Equals — different ID")]
    public bool Entity_Equals_DifferentId() => _entity1.Equals(_entity3);

    [Benchmark(Description = "Entity.GetHashCode")]
    public int Entity_GetHashCode() => _entity1.GetHashCode();

    // ── ValueObject ───────────────────────────────────────────────────────────

    [Benchmark(Description = "ValueObject.Equals — default (boxes via GetEqualityComponents)")]
    public bool ValueObject_Equals_Default() => _money1.Equals(_money2);

    [Benchmark(Description = "ValueObject.GetHashCode — default (boxes components)")]
    public int ValueObject_GetHashCode_Default() => _money1.GetHashCode();

    [Benchmark(Description = "ValueObject.Equals — optimized override (zero boxing)")]
    public bool ValueObject_Equals_Optimized() => _optimized1.Equals(_optimized2);

    [Benchmark(Description = "ValueObject.GetHashCode — optimized override (HashCode.Combine)")]
    public int ValueObject_GetHashCode_Optimized() => _optimized1.GetHashCode();
}
