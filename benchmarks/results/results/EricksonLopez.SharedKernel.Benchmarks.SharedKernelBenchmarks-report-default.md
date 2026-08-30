
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.4 LTS (Noble Numbat)
AMD EPYC 7763 2.96GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.400
  [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3


 Method                                | Job       | Runtime   | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
-------------------------------------- |---------- |---------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
 EntityEquality_SameId                 | .NET 10.0 | .NET 10.0 |  0.4255 ns | 0.0049 ns | 0.0044 ns |     ? |       ? |      - |         - |           ? |
 EntityEquality_SameId                 | .NET 8.0  | .NET 8.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityEquality_SameId                 | .NET 9.0  | .NET 9.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |            |           |           |       |         |        |           |             |
 EntityEquality_DifferentId            | .NET 10.0 | .NET 10.0 |  0.4961 ns | 0.0080 ns | 0.0075 ns |     ? |       ? |      - |         - |           ? |
 EntityEquality_DifferentId            | .NET 8.0  | .NET 8.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityEquality_DifferentId            | .NET 9.0  | .NET 9.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |            |           |           |       |         |        |           |             |
 EntityGetHashCode                     | .NET 10.0 | .NET 10.0 |  7.5049 ns | 0.0210 ns | 0.0197 ns |     ? |       ? |      - |         - |           ? |
 EntityGetHashCode                     | .NET 8.0  | .NET 8.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 EntityGetHashCode                     | .NET 9.0  | .NET 9.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |            |           |           |       |         |        |           |             |
 AggregateDrainDomainEvents_NoEvents   | .NET 10.0 | .NET 10.0 |  1.0623 ns | 0.0031 ns | 0.0028 ns |     ? |       ? |      - |         - |           ? |
 AggregateDrainDomainEvents_NoEvents   | .NET 8.0  | .NET 8.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateDrainDomainEvents_NoEvents   | .NET 9.0  | .NET 9.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |            |           |           |       |         |        |           |             |
 AggregateDrainDomainEvents_WithEvents | .NET 10.0 | .NET 10.0 | 26.8573 ns | 0.0825 ns | 0.0772 ns |     ? |       ? | 0.0067 |     112 B |           ? |
 AggregateDrainDomainEvents_WithEvents | .NET 8.0  | .NET 8.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
 AggregateDrainDomainEvents_WithEvents | .NET 9.0  | .NET 9.0  |         NA |        NA |        NA |     ? |       ? |     NA |        NA |           ? |
                                       |           |           |            |           |           |       |         |        |           |             |
 AggregateRaiseDomainEvent_FirstTime   | .NET 10.0 | .NET 10.0 | 28.5869 ns | 0.2678 ns | 0.2236 ns |     ? |       ? | 0.0076 |     128 B |           ? |
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
