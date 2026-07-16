using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EricksonLopez.SharedKernel.Specifications;

BenchmarkRunner.Run<SpecificationBenchmarks>();

/// <summary>
/// Benchmarks compiled expression caching in Specification.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class SpecificationBenchmarks
{
    private readonly ActiveItemSpec _spec = new();
    private readonly BenchmarkItem[] _items;

    public SpecificationBenchmarks()
        => _items = Enumerable.Range(0, 1000)
            .Select(i => new BenchmarkItem(i % 2 == 0))
            .ToArray();

    [Benchmark(Baseline = true, Description = "Direct lambda")]
    public int DirectLambda()
        => _items.Count(x => x.IsActive);

    [Benchmark(Description = "Spec.IsSatisfiedBy (cached)")]
    public int SpecIsSatisfiedBy()
        => _items.Count(_spec.IsSatisfiedBy);

    [Benchmark(Description = "Spec.ToExpression().Compile() (no cache)")]
    public int SpecToExpressionCompile()
    {
        var compiled = _spec.ToExpression().Compile();
        return _items.Count(compiled);
    }
}

internal sealed record BenchmarkItem(bool IsActive);

internal sealed class ActiveItemSpec : Specification<BenchmarkItem>
{
    public override System.Linq.Expressions.Expression<Func<BenchmarkItem, bool>> ToExpression()
        => x => x.IsActive;
}
