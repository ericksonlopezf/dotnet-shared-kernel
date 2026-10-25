
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3


 Method                                      | Job       | Runtime   | Mean     | Error    | StdDev   | Ratio | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
-------------------------------------------- |---------- |---------- |---------:|---------:|---------:|------:|--------:|-----:|-------:|----------:|------------:|
 Ardalis_AggregateHydration_ZeroEvents       | .NET 10.0 | .NET 10.0 | 16.54 ns | 0.342 ns | 0.320 ns |     ? |       ? |    2 | 0.0043 |      72 B |           ? |
 EricksonLopez_AggregateHydration_ZeroEvents | .NET 10.0 | .NET 10.0 | 14.81 ns | 0.534 ns | 1.573 ns |     ? |       ? |    1 | 0.0029 |      48 B |           ? |
 Ardalis_DrainEvents_WithEvents              | .NET 10.0 | .NET 10.0 | 79.21 ns | 0.971 ns | 0.861 ns |     ? |       ? |    4 | 0.0119 |     200 B |           ? |
 EricksonLopez_DrainEvents_WithEvents        | .NET 10.0 | .NET 10.0 | 57.20 ns | 0.363 ns | 0.322 ns |     ? |       ? |    3 | 0.0114 |     192 B |           ? |
 Ardalis_AggregateHydration_ZeroEvents       | .NET 8.0  | .NET 8.0  |       NA |       NA |       NA |     ? |       ? |    ? |     NA |        NA |           ? |
 EricksonLopez_AggregateHydration_ZeroEvents | .NET 8.0  | .NET 8.0  |       NA |       NA |       NA |     ? |       ? |    ? |     NA |        NA |           ? |
 Ardalis_DrainEvents_WithEvents              | .NET 8.0  | .NET 8.0  |       NA |       NA |       NA |     ? |       ? |    ? |     NA |        NA |           ? |
 EricksonLopez_DrainEvents_WithEvents        | .NET 8.0  | .NET 8.0  |       NA |       NA |       NA |     ? |       ? |    ? |     NA |        NA |           ? |
 Ardalis_AggregateHydration_ZeroEvents       | .NET 9.0  | .NET 9.0  |       NA |       NA |       NA |     ? |       ? |    ? |     NA |        NA |           ? |
 EricksonLopez_AggregateHydration_ZeroEvents | .NET 9.0  | .NET 9.0  |       NA |       NA |       NA |     ? |       ? |    ? |     NA |        NA |           ? |
 Ardalis_DrainEvents_WithEvents              | .NET 9.0  | .NET 9.0  |       NA |       NA |       NA |     ? |       ? |    ? |     NA |        NA |           ? |
 EricksonLopez_DrainEvents_WithEvents        | .NET 9.0  | .NET 9.0  |       NA |       NA |       NA |     ? |       ? |    ? |     NA |        NA |           ? |

Benchmarks with issues:
  AggregateHydrationBenchmarks.Ardalis_AggregateHydration_ZeroEvents: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  AggregateHydrationBenchmarks.EricksonLopez_AggregateHydration_ZeroEvents: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  AggregateHydrationBenchmarks.Ardalis_DrainEvents_WithEvents: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  AggregateHydrationBenchmarks.EricksonLopez_DrainEvents_WithEvents: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  AggregateHydrationBenchmarks.Ardalis_AggregateHydration_ZeroEvents: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  AggregateHydrationBenchmarks.EricksonLopez_AggregateHydration_ZeroEvents: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  AggregateHydrationBenchmarks.Ardalis_DrainEvents_WithEvents: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  AggregateHydrationBenchmarks.EricksonLopez_DrainEvents_WithEvents: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
