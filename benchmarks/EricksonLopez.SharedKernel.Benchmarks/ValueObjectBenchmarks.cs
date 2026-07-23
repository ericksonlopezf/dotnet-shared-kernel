using BenchmarkDotNet.Attributes;
using EricksonLopez.SharedKernel.Domain;

namespace EricksonLopez.SharedKernel.Benchmarks;

[MemoryDiagnoser]
public class ValueObjectBenchmarks
{
    private readonly DefaultValueObject _default1 = new(100.5m, "USD");
    private readonly DefaultValueObject _default2 = new(100.5m, "USD");

    private readonly OptimizedValueObject _optimized1 = new(100.5m, "USD");
    private readonly OptimizedValueObject _optimized2 = new(100.5m, "USD");

    [Benchmark(Baseline = true, Description = "Default ValueObject (Boxes decimals)")]
    public bool DefaultEquals() => _default1.Equals(_default2);

    [Benchmark(Description = "Default ValueObject GetHashCode (Boxes decimals)")]
    public int DefaultGetHashCode() => _default1.GetHashCode();

    [Benchmark(Description = "Optimized ValueObject (Zero boxing)")]
    public bool OptimizedEquals() => _optimized1.Equals(_optimized2);

    [Benchmark(Description = "Optimized ValueObject GetHashCode (Zero boxing)")]
    public int OptimizedGetHashCode() => _optimized1.GetHashCode();
}

internal sealed class DefaultValueObject : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public DefaultValueObject(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

internal sealed class OptimizedValueObject : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public OptimizedValueObject(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override bool Equals(ValueObject? other)
        => other is OptimizedValueObject m && Amount == m.Amount && Currency == m.Currency;

    public override int GetHashCode()
        => HashCode.Combine(Amount, Currency);
}
