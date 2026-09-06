
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.4 LTS (Noble Numbat)
AMD EPYC 7763 2.81GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.400
  [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3


 Method                                   | Job       | Runtime   | Mean      | Error     | StdDev    | Ratio | RatioSD | Rank | Allocated | Alloc Ratio |
----------------------------------------- |---------- |---------- |----------:|----------:|----------:|------:|--------:|-----:|----------:|------------:|
 Ardalis_EntityEquality_SameId            | .NET 10.0 | .NET 10.0 | 0.5150 ns | 0.0051 ns | 0.0048 ns |     ? |       ? |    1 |         - |           ? |
 EricksonLopez_EntityEquality_SameId      | .NET 10.0 | .NET 10.0 | 0.8105 ns | 0.0235 ns | 0.0220 ns |     ? |       ? |    2 |         - |           ? |
 Ardalis_EntityEquality_DifferentId       | .NET 10.0 | .NET 10.0 | 0.8478 ns | 0.0051 ns | 0.0048 ns |     ? |       ? |    2 |         - |           ? |
 EricksonLopez_EntityEquality_DifferentId | .NET 10.0 | .NET 10.0 | 0.5048 ns | 0.0083 ns | 0.0073 ns |     ? |       ? |    1 |         - |           ? |
 Ardalis_EntityEquality_SameId            | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
 EricksonLopez_EntityEquality_SameId      | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
 Ardalis_EntityEquality_DifferentId       | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
 EricksonLopez_EntityEquality_DifferentId | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
 Ardalis_EntityEquality_SameId            | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
 EricksonLopez_EntityEquality_SameId      | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
 Ardalis_EntityEquality_DifferentId       | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
 EricksonLopez_EntityEquality_DifferentId | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |

Benchmarks with issues:
  EntityComparisonBenchmarks.Ardalis_EntityEquality_SameId: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EntityComparisonBenchmarks.EricksonLopez_EntityEquality_SameId: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EntityComparisonBenchmarks.Ardalis_EntityEquality_DifferentId: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EntityComparisonBenchmarks.EricksonLopez_EntityEquality_DifferentId: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EntityComparisonBenchmarks.Ardalis_EntityEquality_SameId: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EntityComparisonBenchmarks.EricksonLopez_EntityEquality_SameId: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EntityComparisonBenchmarks.Ardalis_EntityEquality_DifferentId: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EntityComparisonBenchmarks.EricksonLopez_EntityEquality_DifferentId: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
