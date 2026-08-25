// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using EricksonLopez.SharedKernel;

namespace EricksonLopez.SharedKernel.Benchmarks;

/// <summary>
/// Comparative benchmarks measuring entity equality comparison performance
/// against simulated competitive architecture patterns (Ardalis.SharedKernel v5.0.0).
/// </summary>
[MemoryDiagnoser]
[RankColumn]
public class EntityComparisonBenchmarks
{
    private readonly Guid _id1 = Guid.NewGuid();
    private readonly Guid _id2 = Guid.NewGuid();

    private readonly BenchmarkEntity _elEntity1;
    private readonly BenchmarkEntity _elEntitySame;
    private readonly BenchmarkEntity _elEntityDiff;

    private readonly ArdalisSimulatedEntity _ardalisEntity1;
    private readonly ArdalisSimulatedEntity _ardalisEntitySame;
    private readonly ArdalisSimulatedEntity _ardalisEntityDiff;

    public EntityComparisonBenchmarks()
    {
        _elEntity1 = new BenchmarkEntity(_id1);
        _elEntitySame = new BenchmarkEntity(_id1);
        _elEntityDiff = new BenchmarkEntity(_id2);

        _ardalisEntity1 = new ArdalisSimulatedEntity { Id = _id1 };
        _ardalisEntitySame = new ArdalisSimulatedEntity { Id = _id1 };
        _ardalisEntityDiff = new ArdalisSimulatedEntity { Id = _id2 };
    }

    [Benchmark(Baseline = true)]
    public bool Ardalis_EntityEquality_SameId()
    {
        return _ardalisEntity1.Equals(_ardalisEntitySame);
    }

    [Benchmark]
    public bool EricksonLopez_EntityEquality_SameId()
    {
        return _elEntity1.Equals(_elEntitySame);
    }

    [Benchmark]
    public bool Ardalis_EntityEquality_DifferentId()
    {
        return _ardalisEntity1.Equals(_ardalisEntityDiff);
    }

    [Benchmark]
    public bool EricksonLopez_EntityEquality_DifferentId()
    {
        return _elEntity1.Equals(_elEntityDiff);
    }
}

/// <summary>
/// Comparative benchmarks measuring aggregate root allocation during hydration and event draining.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
public class AggregateHydrationBenchmarks
{
    private readonly Guid _id1 = Guid.NewGuid();
    private readonly BenchmarkDomainEvent _domainEvent = new();

    [Benchmark(Baseline = true)]
    public ArdalisSimulatedAggregateRoot Ardalis_AggregateHydration_ZeroEvents()
    {
        // Eagerly allocates List<DomainEvent> in constructor
        return new ArdalisSimulatedAggregateRoot { Id = _id1 };
    }

    [Benchmark]
    public BenchmarkAggregate EricksonLopez_AggregateHydration_ZeroEvents()
    {
        // Zero-allocation: lazy event buffer initialized to null
        return new BenchmarkAggregate(_id1);
    }

    [Benchmark]
    public int Ardalis_DrainEvents_WithEvents()
    {
        var agg = new ArdalisSimulatedAggregateRoot { Id = _id1 };
        agg.RegisterDomainEvent(new ArdalisSimulatedDomainEvent());
        var copy = new List<ArdalisSimulatedDomainEvent>(agg.DomainEvents);
        agg.ClearDomainEvents();
        return copy.Count;
    }

    [Benchmark]
    public int EricksonLopez_DrainEvents_WithEvents()
    {
        var agg = new BenchmarkAggregate(_id1);
        agg.RecordEvent(_domainEvent);
        return agg.DrainDomainEvents().Count;
    }
}

#region Ardalis Simulated Baseline Types

public class ArdalisSimulatedEntity : IEquatable<ArdalisSimulatedEntity>
{
    public Guid Id { get; set; }

    public bool Equals(ArdalisSimulatedEntity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj) => Equals(obj as ArdalisSimulatedEntity);

    public override int GetHashCode() => Id.GetHashCode();
}

public class ArdalisSimulatedDomainEvent
{
    public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
}

public class ArdalisSimulatedAggregateRoot : ArdalisSimulatedEntity
{
    // Ardalis pattern: eager List allocation on every instance
    private readonly List<ArdalisSimulatedDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<ArdalisSimulatedDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void RegisterDomainEvent(ArdalisSimulatedDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

#endregion
