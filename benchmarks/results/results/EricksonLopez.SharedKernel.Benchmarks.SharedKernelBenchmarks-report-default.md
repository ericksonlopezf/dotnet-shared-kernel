
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


 Method                                | Job       | Runtime   | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
-------------------------------------- |---------- |---------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
 EntityEquality_SameId                 | .NET 10.0 | .NET 10.0 |  3.617 ns | 0.0136 ns | 0.0113 ns |     ? |       ? |      - |         - |           ? |
 EntityEquality_SameId                 | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityEquality_SameId                 | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 EntityEquality_DifferentId            | .NET 10.0 | .NET 10.0 |  3.447 ns | 0.0076 ns | 0.0063 ns |     ? |       ? |      - |         - |           ? |
 EntityEquality_DifferentId            | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityEquality_DifferentId            | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 EntityGetHashCode                     | .NET 10.0 | .NET 10.0 |  7.476 ns | 0.1350 ns | 0.1263 ns |     ? |       ? |      - |         - |           ? |
 EntityGetHashCode                     | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityGetHashCode                     | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 AggregateDrainDomainEvents_NoEvents   | .NET 10.0 | .NET 10.0 |  2.072 ns | 0.0156 ns | 0.0130 ns |     ? |       ? |      - |         - |           ? |
 AggregateDrainDomainEvents_NoEvents   | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateDrainDomainEvents_NoEvents   | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 AggregateDrainDomainEvents_WithEvents | .NET 10.0 | .NET 10.0 | 54.831 ns | 0.9717 ns | 0.9090 ns |     ? |       ? | 0.0114 |     192 B |           ? |
 AggregateDrainDomainEvents_WithEvents | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateDrainDomainEvents_WithEvents | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 AggregateRaiseDomainEvent_FirstTime   | .NET 10.0 | .NET 10.0 | 31.213 ns | 0.3471 ns | 0.3077 ns |     ? |       ? | 0.0081 |     136 B |           ? |
 AggregateRaiseDomainEvent_FirstTime   | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateRaiseDomainEvent_FirstTime   | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 AggregateRaiseDomainEvent_Subsequent  | .NET 10.0 | .NET 10.0 | 43.587 ns | 0.5304 ns | 0.4701 ns |     ? |       ? | 0.0081 |     136 B |           ? |
 AggregateRaiseDomainEvent_Subsequent  | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateRaiseDomainEvent_Subsequent  | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |

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
