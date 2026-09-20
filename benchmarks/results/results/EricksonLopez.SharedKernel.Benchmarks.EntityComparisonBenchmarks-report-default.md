
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.49GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4


 Method                                   | Job       | Runtime   | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Rank | Allocated | Alloc Ratio |
----------------------------------------- |---------- |---------- |----------:|----------:|----------:|----------:|------:|--------:|-----:|----------:|------------:|
 Ardalis_EntityEquality_SameId            | .NET 10.0 | .NET 10.0 | 0.0024 ns | 0.0032 ns | 0.0028 ns | 0.0023 ns |     ? |       ? |    1 |         - |           ? |
 EricksonLopez_EntityEquality_SameId      | .NET 10.0 | .NET 10.0 | 0.0442 ns | 0.0163 ns | 0.0153 ns | 0.0392 ns |     ? |       ? |    2 |         - |           ? |
 Ardalis_EntityEquality_DifferentId       | .NET 10.0 | .NET 10.0 | 0.0408 ns | 0.0065 ns | 0.0051 ns | 0.0390 ns |     ? |       ? |    2 |         - |           ? |
 EricksonLopez_EntityEquality_DifferentId | .NET 10.0 | .NET 10.0 | 0.0024 ns | 0.0032 ns | 0.0027 ns | 0.0017 ns |     ? |       ? |    1 |         - |           ? |
 Ardalis_EntityEquality_SameId            | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
 EricksonLopez_EntityEquality_SameId      | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
 Ardalis_EntityEquality_DifferentId       | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
 EricksonLopez_EntityEquality_DifferentId | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
 Ardalis_EntityEquality_SameId            | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
 EricksonLopez_EntityEquality_SameId      | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
 Ardalis_EntityEquality_DifferentId       | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |
 EricksonLopez_EntityEquality_DifferentId | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |        NA |     ? |       ? |    ? |        NA |           ? |

Benchmarks with issues:
  EntityComparisonBenchmarks.Ardalis_EntityEquality_SameId: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EntityComparisonBenchmarks.EricksonLopez_EntityEquality_SameId: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EntityComparisonBenchmarks.Ardalis_EntityEquality_DifferentId: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EntityComparisonBenchmarks.EricksonLopez_EntityEquality_DifferentId: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EntityComparisonBenchmarks.Ardalis_EntityEquality_SameId: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EntityComparisonBenchmarks.EricksonLopez_EntityEquality_SameId: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EntityComparisonBenchmarks.Ardalis_EntityEquality_DifferentId: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EntityComparisonBenchmarks.EricksonLopez_EntityEquality_DifferentId: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
