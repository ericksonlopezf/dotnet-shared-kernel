
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.63GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3


 Method                                   | Job       | Runtime   | Mean      | Error     | StdDev    | Ratio | RatioSD | Rank | Allocated | Alloc Ratio |
----------------------------------------- |---------- |---------- |----------:|----------:|----------:|------:|--------:|-----:|----------:|------------:|
 Ardalis_EntityEquality_SameId            | .NET 10.0 | .NET 10.0 | 0.5126 ns | 0.0073 ns | 0.0068 ns |     ? |       ? |    2 |         - |           ? |
 EricksonLopez_EntityEquality_SameId      | .NET 10.0 | .NET 10.0 | 3.4615 ns | 0.0040 ns | 0.0036 ns |     ? |       ? |    3 |         - |           ? |
 Ardalis_EntityEquality_DifferentId       | .NET 10.0 | .NET 10.0 | 0.3819 ns | 0.0059 ns | 0.0052 ns |     ? |       ? |    1 |         - |           ? |
 EricksonLopez_EntityEquality_DifferentId | .NET 10.0 | .NET 10.0 | 3.4620 ns | 0.0053 ns | 0.0044 ns |     ? |       ? |    3 |         - |           ? |
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
