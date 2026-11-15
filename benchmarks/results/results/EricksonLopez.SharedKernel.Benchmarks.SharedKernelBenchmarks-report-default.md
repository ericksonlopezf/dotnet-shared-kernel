
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V45 4.33GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4


 Method                                | Job       | Runtime   | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
-------------------------------------- |---------- |---------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
 EntityEquality_SameId                 | .NET 10.0 | .NET 10.0 |  2.104 ns | 0.0591 ns | 0.0553 ns |     ? |       ? |      - |         - |           ? |
 EntityEquality_SameId                 | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityEquality_SameId                 | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 EntityEquality_DifferentId            | .NET 10.0 | .NET 10.0 |  1.885 ns | 0.0592 ns | 0.0495 ns |     ? |       ? |      - |         - |           ? |
 EntityEquality_DifferentId            | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityEquality_DifferentId            | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 EntityGetHashCode                     | .NET 10.0 | .NET 10.0 |  3.236 ns | 0.0793 ns | 0.1186 ns |     ? |       ? |      - |         - |           ? |
 EntityGetHashCode                     | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityGetHashCode                     | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 AggregateDrainDomainEvents_NoEvents   | .NET 10.0 | .NET 10.0 |  5.035 ns | 0.1185 ns | 0.1411 ns |     ? |       ? |      - |         - |           ? |
 AggregateDrainDomainEvents_NoEvents   | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateDrainDomainEvents_NoEvents   | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 AggregateDrainDomainEvents_WithEvents | .NET 10.0 | .NET 10.0 | 31.994 ns | 0.5937 ns | 0.5263 ns |     ? |       ? | 0.0114 |     192 B |           ? |
 AggregateDrainDomainEvents_WithEvents | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateDrainDomainEvents_WithEvents | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 AggregateRaiseDomainEvent_FirstTime   | .NET 10.0 | .NET 10.0 | 18.313 ns | 0.2697 ns | 0.2106 ns |     ? |       ? | 0.0081 |     136 B |           ? |
 AggregateRaiseDomainEvent_FirstTime   | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateRaiseDomainEvent_FirstTime   | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 AggregateRaiseDomainEvent_Subsequent  | .NET 10.0 | .NET 10.0 | 22.039 ns | 0.4827 ns | 0.9639 ns |     ? |       ? | 0.0081 |     136 B |           ? |
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
