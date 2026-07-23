using BenchmarkDotNet.Attributes;
using EricksonLopez.SharedKernel.Specifications;

namespace EricksonLopez.SharedKernel.Benchmarks;

// ─── Test doubles ─────────────────────────────────────────────────────────────

internal sealed record User(string Name, bool IsActive, decimal Score);

internal sealed class ActiveUserSpec : Specification<User>
{
    public override System.Linq.Expressions.Expression<Func<User, bool>> ToExpression()
        => u => u.IsActive;
}

internal sealed class HighScoreSpec(decimal threshold) : Specification<User>
{
    public override System.Linq.Expressions.Expression<Func<User, bool>> ToExpression()
        => u => u.Score >= threshold;
}

/// <summary>
/// NativeAOT-safe spec that overrides Evaluate directly, bypassing Expression.Compile().
/// </summary>
internal sealed class NativeAotActiveUserSpec : Specification<User>
{
    protected override bool Evaluate(User candidate) => candidate.IsActive;

    public override System.Linq.Expressions.Expression<Func<User, bool>> ToExpression()
        => throw new InvalidOperationException("Should not be called — Evaluate overrides the path");
}

// ─── Benchmarks ───────────────────────────────────────────────────────────────

/// <summary>
/// Benchmarks the Specification pattern: compiled expression caching,
/// NativeAOT override path, composite operators, and collection filtering.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class SpecificationBenchmarks
{
    private readonly User _activeHighScore = new("Alice", true, 950m);
    private readonly User[] _dataset;

    private readonly ActiveUserSpec _activeSpec = new();
    private readonly NativeAotActiveUserSpec _nativeAotSpec = new();
    private readonly Specification<User> _andSpec;
    private readonly Specification<User> _orSpec;
    private readonly Specification<User> _deepSpec;

    public SpecificationBenchmarks()
    {
        _dataset = Enumerable.Range(0, 1000)
            .Select(i => new User($"User{i}", i % 2 == 0, i * 10m))
            .ToArray();

        _andSpec = new ActiveUserSpec() & new HighScoreSpec(500m);
        _orSpec = new ActiveUserSpec() | new HighScoreSpec(500m);
        _deepSpec = ((new ActiveUserSpec() & new HighScoreSpec(100m))
                    & !new HighScoreSpec(5m))
                    | !new ActiveUserSpec();

        // Warmup — prime the lazy compiled expression cache
        _activeSpec.IsSatisfiedBy(_activeHighScore);
        _andSpec.IsSatisfiedBy(_activeHighScore);
        _orSpec.IsSatisfiedBy(_activeHighScore);
        _deepSpec.IsSatisfiedBy(_activeHighScore);
    }

    // ── Single spec ──────────────────────────────────────────────────────────

    [Benchmark(Baseline = true, Description = "ActiveSpec — cached compiled expression")]
    public bool SingleSpec_CompiledCached()
        => _activeSpec.IsSatisfiedBy(_activeHighScore);

    [Benchmark(Description = "ActiveSpec — NativeAOT direct Evaluate override")]
    public bool SingleSpec_NativeAotOverride()
        => _nativeAotSpec.IsSatisfiedBy(_activeHighScore);

    [Benchmark(Description = "ActiveSpec — cold compile (Expression.Compile on each call)")]
    public bool SingleSpec_ColdCompile()
    {
        var compiled = new ActiveUserSpec().ToExpression().Compile();
        return compiled(_activeHighScore);
    }

    // ── Composite specs ──────────────────────────────────────────────────────

    [Benchmark(Description = "AndSpec — 2 conditions (cached)")]
    public bool AndSpec_TwoConditions()
        => _andSpec.IsSatisfiedBy(_activeHighScore);

    [Benchmark(Description = "OrSpec — 2 conditions (cached)")]
    public bool OrSpec_TwoConditions()
        => _orSpec.IsSatisfiedBy(_activeHighScore);

    [Benchmark(Description = "Deep chain — 5-level composition (cached)")]
    public bool DeepChain_FiveLevels()
        => _deepSpec.IsSatisfiedBy(_activeHighScore);

    // ── Collection filtering ──────────────────────────────────────────────────

    [Benchmark(Description = "Filter 1,000 items with single spec")]
    public int FilterDataset_SingleSpec()
        => _dataset.Count(_activeSpec.IsSatisfiedBy);

    [Benchmark(Description = "Filter 1,000 items with AndSpec")]
    public int FilterDataset_AndSpec()
        => _dataset.Count(_andSpec.IsSatisfiedBy);
}
