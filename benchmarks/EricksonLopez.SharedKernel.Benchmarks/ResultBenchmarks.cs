using BenchmarkDotNet.Attributes;
using EricksonLopez.SharedKernel.Results;

namespace EricksonLopez.SharedKernel.Benchmarks;

/// <summary>
/// Benchmarks for the Result pattern — factory cost, monadic chain throughput,
/// and allocation profile of the Combine operators.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class ResultBenchmarks
{
    private static readonly Error TestError = Error.Validation("Bench.Error", "Benchmark error");
    private static readonly Result<int> Success1 = Result.Success(1);
    private static readonly Result<int> Success2 = Result.Success(2);
    private static readonly Result<int> Success3 = Result.Success(3);
    private static readonly Result<int> Failure1 = Result.Failure<int>(TestError);

    // ── Factory ──────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true, Description = "Result.Success() — cached singleton")]
    public Result SuccessNonGeneric() => Result.Success();

    [Benchmark(Description = "Result<T>.Success(value)")]
    public Result<int> SuccessWithValue() => Result.Success(42);

    [Benchmark(Description = "Result<T>.Failure(error)")]
    public Result<int> FailureWithError() => Result.Failure<int>(TestError);

    // ── Monadic chain ────────────────────────────────────────────────────────

    [Benchmark(Description = "Map — 5-step chain (all success)")]
    public Result<int> MapChain()
        => Result.Success(0)
            .Map(x => x + 1)
            .Map(x => x + 1)
            .Map(x => x + 1)
            .Map(x => x + 1)
            .Map(x => x + 1);

    [Benchmark(Description = "Bind — 5-step chain (all success)")]
    public Result<int> BindChain()
        => Result.Success(0)
            .Bind(x => Result.Success(x + 1))
            .Bind(x => Result.Success(x + 1))
            .Bind(x => Result.Success(x + 1))
            .Bind(x => Result.Success(x + 1))
            .Bind(x => Result.Success(x + 1));

    [Benchmark(Description = "Map chain — short-circuits at step 2")]
    public Result<int> MapChainShortCircuit()
        => Result.Success(1)
            .Ensure(x => x < 0, TestError)  // fails here
            .Map(x => x + 1)
            .Map(x => x + 1)
            .Map(x => x + 1)
            .Map(x => x + 1);

    [Benchmark(Description = "Tap + Map + Bind full pipeline")]
    public Result<string> FullPipeline()
        => Result.Success(10)
            .Ensure(x => x > 0, TestError)
            .Map(x => x * 2)
            .Tap(_ => { })
            .Bind(x => Result.Success(x.ToString()));

    // ── Combine ──────────────────────────────────────────────────────────────

    [Benchmark(Description = "Combine — 3 success (non-generic)")]
    public Result CombineAllSuccess() => Result.Combine(Success1, Success2, Success3);

    [Benchmark(Description = "Combine<T> — 3 success (typed, returns IReadOnlyList)")]
    public Result<IReadOnlyList<int>> CombineGenericAllSuccess()
        => Result.Combine<int>(Success1, Success2, Success3);

    [Benchmark(Description = "Combine<T1,T2> — 2-tuple success")]
    public Result<(int, int)> CombineTuple2() => Result.Combine(Success1, Success2);

    [Benchmark(Description = "Combine — 1 failure triggers compound error path")]
    public Result CombineWithOneFailure() => Result.Combine(Success1, Failure1, Success3);

    // ── Error operations ─────────────────────────────────────────────────────

    [Benchmark(Description = "Error.HasInnerErrors (no alloc — null check)")]
    public bool ErrorHasInnerErrors() => TestError.HasInnerErrors;

    [Benchmark(Description = "Error.Equals — structural equality")]
    public bool ErrorEquals() => TestError.Equals(TestError);

    [Benchmark(Description = "Error.GetHashCode — hash computation")]
    public int ErrorGetHashCode() => TestError.GetHashCode();
}
