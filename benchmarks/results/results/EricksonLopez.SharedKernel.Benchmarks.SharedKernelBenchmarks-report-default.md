
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
INTEL XEON PLATINUM 8573C 3.00GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4


 Method                                | Job       | Runtime   | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
-------------------------------------- |---------- |---------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
 EntityEquality_SameId                 | .NET 10.0 | .NET 10.0 |  2.399 ns | 0.0105 ns | 0.0098 ns |     ? |       ? |      - |         - |           ? |
 EntityEquality_SameId                 | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityEquality_SameId                 | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 EntityEquality_DifferentId            | .NET 10.0 | .NET 10.0 |  2.409 ns | 0.0240 ns | 0.0213 ns |     ? |       ? |      - |         - |           ? |
 EntityEquality_DifferentId            | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityEquality_DifferentId            | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 EntityGetHashCode                     | .NET 10.0 | .NET 10.0 |  4.607 ns | 0.0230 ns | 0.0215 ns |     ? |       ? |      - |         - |           ? |
 EntityGetHashCode                     | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityGetHashCode                     | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 AggregateDrainDomainEvents_NoEvents   | .NET 10.0 | .NET 10.0 |  6.222 ns | 0.0621 ns | 0.0580 ns |     ? |       ? |      - |         - |           ? |
 AggregateDrainDomainEvents_NoEvents   | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateDrainDomainEvents_NoEvents   | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 AggregateDrainDomainEvents_WithEvents | .NET 10.0 | .NET 10.0 | 64.833 ns | 0.3028 ns | 0.2684 ns |     ? |       ? | 0.0023 |     192 B |           ? |
 AggregateDrainDomainEvents_WithEvents | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateDrainDomainEvents_WithEvents | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 AggregateRaiseDomainEvent_FirstTime   | .NET 10.0 | .NET 10.0 | 39.426 ns | 0.4979 ns | 0.4414 ns |     ? |       ? | 0.0016 |     136 B |           ? |
 AggregateRaiseDomainEvent_FirstTime   | .NET 8.0  | .NET 8.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateRaiseDomainEvent_FirstTime   | .NET 9.0  | .NET 9.0  |        NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |           |           |           |       |         |        |           |             |
 AggregateRaiseDomainEvent_Subsequent  | .NET 10.0 | .NET 10.0 | 46.275 ns | 0.1969 ns | 0.1745 ns |     ? |       ? | 0.0016 |     136 B |           ? |
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
