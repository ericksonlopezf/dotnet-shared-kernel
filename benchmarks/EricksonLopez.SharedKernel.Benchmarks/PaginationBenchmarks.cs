using BenchmarkDotNet.Attributes;
using EricksonLopez.SharedKernel.Pagination;

namespace EricksonLopez.SharedKernel.Benchmarks;

/// <summary>
/// Benchmarks for <see cref="PagedList{T}"/> and <see cref="PaginationParameters"/>.
/// Focus: array copy cost in Create, Map projection overhead, Skip computation.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class PaginationBenchmarks
{
    private readonly List<int> _items10 = Enumerable.Range(1, 10).ToList();
    private readonly List<int> _items100 = Enumerable.Range(1, 100).ToList();
    private readonly PaginationParameters _page1Of10 = PaginationParameters.Of(1, 10);
    private readonly PaginationParameters _page5Of100 = PaginationParameters.Of(5, 100);
    private readonly PagedList<int> _pagedList10;
    private readonly PagedList<int> _pagedList100;

    public PaginationBenchmarks()
    {
        _pagedList10  = PagedList<int>.Create(_items10,  1000, _page1Of10);
        _pagedList100 = PagedList<int>.Create(_items100, 1000, _page5Of100);
    }

    // ── PagedList.Create ─────────────────────────────────────────────────────

    [Benchmark(Baseline = true, Description = "PagedList.Create — 10 items")]
    public PagedList<int> Create_10Items()
        => PagedList<int>.Create(_items10, 1000, _page1Of10);

    [Benchmark(Description = "PagedList.Create — 100 items")]
    public PagedList<int> Create_100Items()
        => PagedList<int>.Create(_items100, 1000, _page5Of100);

    // ── PagedList.Map ─────────────────────────────────────────────────────────

    [Benchmark(Description = "PagedList<int>.Map — 10 items to string")]
    public PagedList<string> Map_10ItemsToString()
        => _pagedList10.Map(x => x.ToString());

    [Benchmark(Description = "PagedList<int>.Map — 100 items to string")]
    public PagedList<string> Map_100ItemsToString()
        => _pagedList100.Map(x => x.ToString());

    // ── PaginationParameters ──────────────────────────────────────────────────

    [Benchmark(Description = "PaginationParameters.Of — construction + clamp")]
    public PaginationParameters ParametersOf()
        => PaginationParameters.Of(3, 25);

    [Benchmark(Description = "PaginationParameters.Skip — computed property")]
    public int ParametersSkip()
        => _page5Of100.Skip;
}
