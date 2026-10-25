```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3


```
| Method                                   | Job       | Runtime   | Mean      | Error     | StdDev    | Ratio | RatioSD | Rank | Allocated | Alloc Ratio |
|----------------------------------------- |---------- |---------- |----------:|----------:|----------:|------:|--------:|-----:|----------:|------------:|
| Ardalis_EntityEquality_SameId            | .NET 10.0 | .NET 10.0 | 0.4977 ns | 0.0060 ns | 0.0054 ns |     ? |       ? |    1 |         - |           ? |
| EricksonLopez_EntityEquality_SameId      | .NET 10.0 | .NET 10.0 | 3.4539 ns | 0.0062 ns | 0.0055 ns |     ? |       ? |    2 |         - |           ? |
| Ardalis_EntityEquality_DifferentId       | .NET 10.0 | .NET 10.0 | 0.5127 ns | 0.0139 ns | 0.0123 ns |     ? |       ? |    1 |         - |           ? |
| EricksonLopez_EntityEquality_DifferentId | .NET 10.0 | .NET 10.0 | 3.5461 ns | 0.0141 ns | 0.0125 ns |     ? |       ? |    3 |         - |           ? |
| Ardalis_EntityEquality_SameId            | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
| EricksonLopez_EntityEquality_SameId      | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
| Ardalis_EntityEquality_DifferentId       | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
| EricksonLopez_EntityEquality_DifferentId | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
| Ardalis_EntityEquality_SameId            | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
| EricksonLopez_EntityEquality_SameId      | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
| Ardalis_EntityEquality_DifferentId       | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
| EricksonLopez_EntityEquality_DifferentId | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |

Benchmarks with issues:
  EntityComparisonBenchmarks.Ardalis_EntityEquality_SameId: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EntityComparisonBenchmarks.EricksonLopez_EntityEquality_SameId: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EntityComparisonBenchmarks.Ardalis_EntityEquality_DifferentId: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EntityComparisonBenchmarks.EricksonLopez_EntityEquality_DifferentId: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EntityComparisonBenchmarks.Ardalis_EntityEquality_SameId: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EntityComparisonBenchmarks.EricksonLopez_EntityEquality_SameId: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EntityComparisonBenchmarks.Ardalis_EntityEquality_DifferentId: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EntityComparisonBenchmarks.EricksonLopez_EntityEquality_DifferentId: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
