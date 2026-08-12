using BenchmarkDotNet.Running;

namespace EricksonLopez.SharedKernel.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<SharedKernelBenchmarks>();
    }
}
