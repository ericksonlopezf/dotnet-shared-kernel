// Copyright © Erickson Lopez. MIT License.
using System;
using BenchmarkDotNet.Attributes;
using EricksonLopez.SharedKernel;

namespace EricksonLopez.SharedKernel.Benchmarks;

[MemoryDiagnoser]
public class SharedKernelBenchmarks
{
    private readonly Guid _guid1 = Guid.NewGuid();
    private readonly Guid _guid2 = Guid.NewGuid();
    private readonly BenchmarkEntity _entity1;
    private readonly BenchmarkEntity _entitySameId;
    private readonly BenchmarkEntity _entityDifferentId;
    private readonly BenchmarkAggregate _aggregateNoEvents;
    private readonly BenchmarkAggregate _aggregateWithEvents;
    private readonly BenchmarkDomainEvent _sampleEvent = new();

    public SharedKernelBenchmarks()
    {
        _entity1 = new BenchmarkEntity(_guid1);
        _entitySameId = new BenchmarkEntity(_guid1);
        _entityDifferentId = new BenchmarkEntity(_guid2);
        _aggregateNoEvents = new BenchmarkAggregate(_guid1);
        _aggregateWithEvents = new BenchmarkAggregate(_guid1);
        _aggregateWithEvents.RecordEvent(_sampleEvent);
    }

    [Benchmark]
    public bool EntityEquality_SameId()
    {
        return _entity1.Equals(_entitySameId);
    }

    [Benchmark]
    public bool EntityEquality_DifferentId()
    {
        return _entity1.Equals(_entityDifferentId);
    }

    [Benchmark]
    public int EntityGetHashCode()
    {
        return _entity1.GetHashCode();
    }

    [Benchmark]
    public int AggregateDrainDomainEvents_NoEvents()
    {
        return _aggregateNoEvents.DrainDomainEvents().Count;
    }

    [Benchmark]
    public int AggregateDrainDomainEvents_WithEvents()
    {
        var aggregate = new BenchmarkAggregate(_guid1);
        aggregate.RecordEvent(_sampleEvent);
        return aggregate.DrainDomainEvents().Count;
    }

    [Benchmark]
    public BenchmarkAggregate AggregateRaiseDomainEvent_FirstTime()
    {
        var aggregate = new BenchmarkAggregate(_guid1);
        aggregate.RecordEvent(_sampleEvent);
        return aggregate;
    }

    [Benchmark]
    public void AggregateRaiseDomainEvent_Subsequent()
    {
        _aggregateWithEvents.RecordEvent(_sampleEvent);
    }
}

public sealed record BenchmarkDomainEvent : DomainEvent;

public sealed class BenchmarkEntity : Entity<Guid>
{
    public BenchmarkEntity(Guid id) : base(id)
    {
    }
}

public sealed class BenchmarkAggregate : AggregateRoot<Guid>
{
    public BenchmarkAggregate(Guid id) : base(id)
    {
    }

    public void RecordEvent(DomainEvent domainEvent)
    {
        RaiseDomainEvent(domainEvent);
    }
}
