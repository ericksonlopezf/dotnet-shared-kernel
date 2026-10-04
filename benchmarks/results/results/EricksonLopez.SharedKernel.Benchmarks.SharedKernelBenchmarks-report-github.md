```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V74 3.69GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4


```
| Method                                | Job       | Runtime   | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------- |---------- |---------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| EntityEquality_SameId                 | .NET 10.0 | .NET 10.0 |  2.975 ns | 0.0079 ns | 0.0062 ns |     ? |       ? |      - |         - |           ? |
| EntityEquality_SameId                 | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
| EntityEquality_SameId                 | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
|                                       |           |           |           |           |           |       |         |        |           |             |
| EntityEquality_DifferentId            | .NET 10.0 | .NET 10.0 |  2.686 ns | 0.0053 ns | 0.0050 ns |     ? |       ? |      - |         - |           ? |
| EntityEquality_DifferentId            | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
| EntityEquality_DifferentId            | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
|                                       |           |           |           |           |           |       |         |        |           |             |
| EntityGetHashCode                     | .NET 10.0 | .NET 10.0 |  5.189 ns | 0.0135 ns | 0.0120 ns |     ? |       ? |      - |         - |           ? |
| EntityGetHashCode                     | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
| EntityGetHashCode                     | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
|                                       |           |           |           |           |           |       |         |        |           |             |
| AggregateDrainDomainEvents_NoEvents   | .NET 10.0 | .NET 10.0 |  1.380 ns | 0.0022 ns | 0.0018 ns |     ? |       ? |      - |         - |           ? |
| AggregateDrainDomainEvents_NoEvents   | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
| AggregateDrainDomainEvents_NoEvents   | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
|                                       |           |           |           |           |           |       |         |        |           |             |
| AggregateDrainDomainEvents_WithEvents | .NET 10.0 | .NET 10.0 | 41.285 ns | 0.4542 ns | 0.4248 ns |     ? |       ? | 0.0114 |     192 B |           ? |
| AggregateDrainDomainEvents_WithEvents | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
| AggregateDrainDomainEvents_WithEvents | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
|                                       |           |           |           |           |           |       |         |        |           |             |
| AggregateRaiseDomainEvent_FirstTime   | .NET 10.0 | .NET 10.0 | 23.868 ns | 0.5107 ns | 0.4777 ns |     ? |       ? | 0.0081 |     136 B |           ? |
| AggregateRaiseDomainEvent_FirstTime   | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
| AggregateRaiseDomainEvent_FirstTime   | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
|                                       |           |           |           |           |           |       |         |        |           |             |
| AggregateRaiseDomainEvent_Subsequent  | .NET 10.0 | .NET 10.0 | 30.303 ns | 0.3573 ns | 0.3342 ns |     ? |       ? | 0.0081 |     136 B |           ? |
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
