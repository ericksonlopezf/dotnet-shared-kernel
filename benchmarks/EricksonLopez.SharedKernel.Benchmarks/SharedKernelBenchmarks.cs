using BenchmarkDotNet.Attributes;
using System;

namespace EricksonLopez.SharedKernel.Benchmarks;

[MemoryDiagnoser]
public class SharedKernelBenchmarks
{
    private readonly Guid _guid = Guid.NewGuid();
    private readonly BenchmarkEntity _entity1;
    private readonly BenchmarkEntity _entity2;
    private readonly BenchmarkAggregate _aggregate;

    public SharedKernelBenchmarks()
    {
        _entity1 = new BenchmarkEntity(_guid);
        _entity2 = new BenchmarkEntity(_guid);
        _aggregate = new BenchmarkAggregate(_guid);
    }

    [Benchmark]
    public bool EntityEquality()
    {
        return _entity1.Equals(_entity2);
    }

    [Benchmark]
    public int EntityGetHashCode()
    {
        return _entity1.GetHashCode();
    }

    [Benchmark]
    public int AggregateDomainEventsAccessNoEvents()
    {
        return _aggregate.DomainEvents.Count;
    }
}

public sealed class BenchmarkEntity : Entity<Guid>
{
    public BenchmarkEntity(Guid id)
    {
        Id = id;
    }
}

public sealed class BenchmarkAggregate : AggregateRoot<Guid>
{
    public BenchmarkAggregate(Guid id)
    {
        Id = id;
    }
}
