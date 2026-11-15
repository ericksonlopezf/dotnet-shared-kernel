```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V45 4.33GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4


```
| Method                                      | Job       | Runtime   | Mean      | Error     | StdDev    | Ratio | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------------- |---------- |---------- |----------:|----------:|----------:|------:|--------:|-----:|-------:|----------:|------------:|
| Ardalis_AggregateHydration_ZeroEvents       | .NET 10.0 | .NET 10.0 |  8.736 ns | 0.2143 ns | 0.3336 ns |     ? |       ? |    2 | 0.0043 |      72 B |           ? |
| EricksonLopez_AggregateHydration_ZeroEvents | .NET 10.0 | .NET 10.0 |  3.834 ns | 0.1168 ns | 0.2277 ns |     ? |       ? |    1 | 0.0029 |      48 B |           ? |
| Ardalis_DrainEvents_WithEvents              | .NET 10.0 | .NET 10.0 | 42.713 ns | 0.6665 ns | 0.8429 ns |     ? |       ? |    4 | 0.0119 |     200 B |           ? |
| EricksonLopez_DrainEvents_WithEvents        | .NET 10.0 | .NET 10.0 | 33.101 ns | 0.6047 ns | 0.6471 ns |     ? |       ? |    3 | 0.0114 |     192 B |           ? |
| Ardalis_AggregateHydration_ZeroEvents       | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |
| EricksonLopez_AggregateHydration_ZeroEvents | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |
| Ardalis_DrainEvents_WithEvents              | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |
| EricksonLopez_DrainEvents_WithEvents        | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |
| Ardalis_AggregateHydration_ZeroEvents       | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |
| EricksonLopez_AggregateHydration_ZeroEvents | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |
| Ardalis_DrainEvents_WithEvents              | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |
| EricksonLopez_DrainEvents_WithEvents        | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |

Benchmarks with issues:
  AggregateHydrationBenchmarks.Ardalis_AggregateHydration_ZeroEvents: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  AggregateHydrationBenchmarks.EricksonLopez_AggregateHydration_ZeroEvents: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  AggregateHydrationBenchmarks.Ardalis_DrainEvents_WithEvents: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  AggregateHydrationBenchmarks.EricksonLopez_DrainEvents_WithEvents: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  AggregateHydrationBenchmarks.Ardalis_AggregateHydration_ZeroEvents: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  AggregateHydrationBenchmarks.EricksonLopez_AggregateHydration_ZeroEvents: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  AggregateHydrationBenchmarks.Ardalis_DrainEvents_WithEvents: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  AggregateHydrationBenchmarks.EricksonLopez_DrainEvents_WithEvents: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
