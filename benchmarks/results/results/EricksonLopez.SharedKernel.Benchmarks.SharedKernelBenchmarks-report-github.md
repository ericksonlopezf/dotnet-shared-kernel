```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


```
| Method                                | Job       | Runtime   | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------- |---------- |---------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| EntityEquality_SameId                 | .NET 10.0 | .NET 10.0 |  3.524 ns | 0.0031 ns | 0.0026 ns |     ? |       ? |      - |         - |           ? |
| EntityEquality_SameId                 | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
| EntityEquality_SameId                 | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
|                                       |           |           |           |           |           |       |         |        |           |             |
| EntityEquality_DifferentId            | .NET 10.0 | .NET 10.0 |  3.307 ns | 0.0052 ns | 0.0046 ns |     ? |       ? |      - |         - |           ? |
| EntityEquality_DifferentId            | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
| EntityEquality_DifferentId            | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
|                                       |           |           |           |           |           |       |         |        |           |             |
| EntityGetHashCode                     | .NET 10.0 | .NET 10.0 |  7.481 ns | 0.1353 ns | 0.1130 ns |     ? |       ? |      - |         - |           ? |
| EntityGetHashCode                     | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
| EntityGetHashCode                     | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
|                                       |           |           |           |           |           |       |         |        |           |             |
| AggregateDrainDomainEvents_NoEvents   | .NET 10.0 | .NET 10.0 |  2.055 ns | 0.0060 ns | 0.0056 ns |     ? |       ? |      - |         - |           ? |
| AggregateDrainDomainEvents_NoEvents   | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
| AggregateDrainDomainEvents_NoEvents   | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
|                                       |           |           |           |           |           |       |         |        |           |             |
| AggregateDrainDomainEvents_WithEvents | .NET 10.0 | .NET 10.0 | 51.534 ns | 0.7373 ns | 0.6896 ns |     ? |       ? | 0.0114 |     192 B |           ? |
| AggregateDrainDomainEvents_WithEvents | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
| AggregateDrainDomainEvents_WithEvents | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
|                                       |           |           |           |           |           |       |         |        |           |             |
| AggregateRaiseDomainEvent_FirstTime   | .NET 10.0 | .NET 10.0 | 30.471 ns | 0.6637 ns | 0.8860 ns |     ? |       ? | 0.0081 |     136 B |           ? |
| AggregateRaiseDomainEvent_FirstTime   | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
| AggregateRaiseDomainEvent_FirstTime   | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
|                                       |           |           |           |           |           |       |         |        |           |             |
| AggregateRaiseDomainEvent_Subsequent  | .NET 10.0 | .NET 10.0 | 35.528 ns | 0.7650 ns | 0.7156 ns |     ? |       ? | 0.0081 |     136 B |           ? |
| AggregateRaiseDomainEvent_Subsequent  | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
| AggregateRaiseDomainEvent_Subsequent  | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |

Benchmarks with issues:
  SharedKernelBenchmarks.EntityEquality_SameId: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  SharedKernelBenchmarks.EntityEquality_SameId: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  SharedKernelBenchmarks.EntityEquality_DifferentId: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  SharedKernelBenchmarks.EntityEquality_DifferentId: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  SharedKernelBenchmarks.EntityGetHashCode: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  SharedKernelBenchmarks.EntityGetHashCode: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  SharedKernelBenchmarks.AggregateDrainDomainEvents_NoEvents: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  SharedKernelBenchmarks.AggregateDrainDomainEvents_NoEvents: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  SharedKernelBenchmarks.AggregateDrainDomainEvents_WithEvents: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  SharedKernelBenchmarks.AggregateDrainDomainEvents_WithEvents: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  SharedKernelBenchmarks.AggregateRaiseDomainEvent_FirstTime: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  SharedKernelBenchmarks.AggregateRaiseDomainEvent_FirstTime: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  SharedKernelBenchmarks.AggregateRaiseDomainEvent_Subsequent: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  SharedKernelBenchmarks.AggregateRaiseDomainEvent_Subsequent: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
