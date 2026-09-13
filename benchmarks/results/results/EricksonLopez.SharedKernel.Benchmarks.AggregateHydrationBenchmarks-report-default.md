
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


 Method                                      | Job       | Runtime   | Mean      | Error     | StdDev    | Ratio | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
-------------------------------------------- |---------- |---------- |----------:|----------:|----------:|------:|--------:|-----:|-------:|----------:|------------:|
 Ardalis_AggregateHydration_ZeroEvents       | .NET 10.0 | .NET 10.0 | 14.811 ns | 0.3417 ns | 0.3196 ns |     ? |       ? |    2 | 0.0043 |      72 B |           ? |
 EricksonLopez_AggregateHydration_ZeroEvents | .NET 10.0 | .NET 10.0 |  8.087 ns | 0.1984 ns | 0.2037 ns |     ? |       ? |    1 | 0.0024 |      40 B |           ? |
 Ardalis_DrainEvents_WithEvents              | .NET 10.0 | .NET 10.0 | 78.055 ns | 0.6520 ns | 0.5780 ns |     ? |       ? |    4 | 0.0119 |     200 B |           ? |
 EricksonLopez_DrainEvents_WithEvents        | .NET 10.0 | .NET 10.0 | 29.309 ns | 0.5951 ns | 0.7084 ns |     ? |       ? |    3 | 0.0067 |     112 B |           ? |
 Ardalis_AggregateHydration_ZeroEvents       | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |
 EricksonLopez_AggregateHydration_ZeroEvents | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |
 Ardalis_DrainEvents_WithEvents              | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |
 EricksonLopez_DrainEvents_WithEvents        | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |
 Ardalis_AggregateHydration_ZeroEvents       | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |
 EricksonLopez_AggregateHydration_ZeroEvents | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |
 Ardalis_DrainEvents_WithEvents              | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |
 EricksonLopez_DrainEvents_WithEvents        | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |     NA |        NA |           ? |

Benchmarks with issues:
  AggregateHydrationBenchmarks.Ardalis_AggregateHydration_ZeroEvents: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  AggregateHydrationBenchmarks.EricksonLopez_AggregateHydration_ZeroEvents: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  AggregateHydrationBenchmarks.Ardalis_DrainEvents_WithEvents: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  AggregateHydrationBenchmarks.EricksonLopez_DrainEvents_WithEvents: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  AggregateHydrationBenchmarks.Ardalis_AggregateHydration_ZeroEvents: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  AggregateHydrationBenchmarks.EricksonLopez_AggregateHydration_ZeroEvents: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  AggregateHydrationBenchmarks.Ardalis_DrainEvents_WithEvents: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  AggregateHydrationBenchmarks.EricksonLopez_DrainEvents_WithEvents: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
