```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.4 LTS (Noble Numbat)
AMD EPYC 7763 2.81GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.400
  [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3


```
| Method                                      | Job       | Runtime   | Mean      | Error     | StdDev    | Ratio | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------------- |---------- |---------- |----------:|----------:|----------:|------:|--------:|-----:|-------:|----------:|------------:|
| Ardalis_AggregateHydration_ZeroEvents       | .NET 10.0 | .NET 10.0 | 15.286 ns | 0.3570 ns | 0.3164 ns |     ? |       ? |    2 | 0.0043 |      72 B |           ? |
| EricksonLopez_AggregateHydration_ZeroEvents | .NET 10.0 | .NET 10.0 |  8.008 ns | 0.2216 ns | 0.4771 ns |     ? |       ? |    1 | 0.0024 |      40 B |           ? |
| Ardalis_DrainEvents_WithEvents              | .NET 10.0 | .NET 10.0 | 73.802 ns | 0.6845 ns | 0.6068 ns |     ? |       ? |    4 | 0.0119 |     200 B |           ? |
| EricksonLopez_DrainEvents_WithEvents        | .NET 10.0 | .NET 10.0 | 28.390 ns | 0.5705 ns | 0.5603 ns |     ? |       ? |    3 | 0.0067 |     112 B |           ? |
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
