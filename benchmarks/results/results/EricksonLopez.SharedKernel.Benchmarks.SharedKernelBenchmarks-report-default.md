
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.4 LTS (Noble Numbat)
AMD EPYC 7763 2.81GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.400
  [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3


 Method                                | Job       | Runtime   | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
-------------------------------------- |---------- |---------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
 EntityEquality_SameId                 | .NET 10.0 | .NET 10.0 |  0.4911 ns | 0.0086 ns | 0.0081 ns |     ? |       ? |      - |         - |           ? |
 EntityEquality_SameId                 | .NET 8.0  | .NET 8.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityEquality_SameId                 | .NET 9.0  | .NET 9.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |            |           |           |       |         |        |           |             |
 EntityEquality_DifferentId            | .NET 10.0 | .NET 10.0 |  0.5074 ns | 0.0073 ns | 0.0064 ns |     ? |       ? |      - |         - |           ? |
 EntityEquality_DifferentId            | .NET 8.0  | .NET 8.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityEquality_DifferentId            | .NET 9.0  | .NET 9.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |            |           |           |       |         |        |           |             |
 EntityGetHashCode                     | .NET 10.0 | .NET 10.0 |  7.5011 ns | 0.0334 ns | 0.0312 ns |     ? |       ? |      - |         - |           ? |
 EntityGetHashCode                     | .NET 8.0  | .NET 8.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityGetHashCode                     | .NET 9.0  | .NET 9.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |            |           |           |       |         |        |           |             |
 AggregateDrainDomainEvents_NoEvents   | .NET 10.0 | .NET 10.0 |  1.0634 ns | 0.0055 ns | 0.0046 ns |     ? |       ? |      - |         - |           ? |
 AggregateDrainDomainEvents_NoEvents   | .NET 8.0  | .NET 8.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateDrainDomainEvents_NoEvents   | .NET 9.0  | .NET 9.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |            |           |           |       |         |        |           |             |
 AggregateDrainDomainEvents_WithEvents | .NET 10.0 | .NET 10.0 | 27.8608 ns | 0.3015 ns | 0.2821 ns |     ? |       ? | 0.0067 |     112 B |           ? |
 AggregateDrainDomainEvents_WithEvents | .NET 8.0  | .NET 8.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateDrainDomainEvents_WithEvents | .NET 9.0  | .NET 9.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |            |           |           |       |         |        |           |             |
 AggregateRaiseDomainEvent_FirstTime   | .NET 10.0 | .NET 10.0 | 31.0483 ns | 0.4167 ns | 0.3694 ns |     ? |       ? | 0.0076 |     128 B |           ? |
 AggregateRaiseDomainEvent_FirstTime   | .NET 8.0  | .NET 8.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateRaiseDomainEvent_FirstTime   | .NET 9.0  | .NET 9.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |            |           |           |       |         |        |           |             |
 AggregateRaiseDomainEvent_Subsequent  | .NET 10.0 | .NET 10.0 |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateRaiseDomainEvent_Subsequent  | .NET 8.0  | .NET 8.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateRaiseDomainEvent_Subsequent  | .NET 9.0  | .NET 9.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |

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
  SharedKernelBenchmarks.AggregateRaiseDomainEvent_Subsequent: .NET 10.0(Runtime=.NET 10.0, Toolchain=net10.0)
  SharedKernelBenchmarks.AggregateRaiseDomainEvent_Subsequent: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  SharedKernelBenchmarks.AggregateRaiseDomainEvent_Subsequent: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
