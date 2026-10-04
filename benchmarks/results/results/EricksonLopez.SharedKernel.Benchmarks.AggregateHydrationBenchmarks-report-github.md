```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 3.69GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4


```
| Method                                      | Job       | Runtime   | Mean      | Error     | StdDev    | Ratio | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------------- |---------- |---------- |----------:|----------:|----------:|------:|--------:|-----:|-------:|----------:|------------:|
| Ardalis_AggregateHydration_ZeroEvents       | .NET 10.0 | .NET 10.0 | 11.760 ns | 0.1089 ns | 0.0909 ns |     ? |       ? |    2 | 0.0043 |      72 B |           ? |
| EricksonLopez_AggregateHydration_ZeroEvents | .NET 10.0 | .NET 10.0 |  5.505 ns | 0.1629 ns | 0.1524 ns |     ? |       ? |    1 | 0.0029 |      48 B |           ? |
| Ardalis_DrainEvents_WithEvents              | .NET 10.0 | .NET 10.0 | 57.076 ns | 0.1740 ns | 0.1542 ns |     ? |       ? |    4 | 0.0119 |     200 B |           ? |
| EricksonLopez_DrainEvents_WithEvents        | .NET 10.0 | .NET 10.0 | 41.983 ns | 0.1817 ns | 0.1611 ns |     ? |       ? |    3 | 0.0114 |     192 B |           ? |
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
