```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.69GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4


```
| Method                                   | Job       | Runtime   | Mean      | Error     | StdDev    | Ratio | RatioSD | Rank | Allocated | Alloc Ratio |
|----------------------------------------- |---------- |---------- |----------:|----------:|----------:|------:|--------:|-----:|----------:|------------:|
| Ardalis_EntityEquality_SameId            | .NET 10.0 | .NET 10.0 | 0.1973 ns | 0.0021 ns | 0.0017 ns |     ? |       ? |    1 |         - |           ? |
| EricksonLopez_EntityEquality_SameId      | .NET 10.0 | .NET 10.0 | 2.7339 ns | 0.0061 ns | 0.0048 ns |     ? |       ? |    2 |         - |           ? |
| Ardalis_EntityEquality_DifferentId       | .NET 10.0 | .NET 10.0 | 0.1912 ns | 0.0020 ns | 0.0017 ns |     ? |       ? |    1 |         - |           ? |
| EricksonLopez_EntityEquality_DifferentId | .NET 10.0 | .NET 10.0 | 2.7359 ns | 0.0033 ns | 0.0030 ns |     ? |       ? |    2 |         - |           ? |
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
