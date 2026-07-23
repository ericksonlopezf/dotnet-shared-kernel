using EricksonLopez.SharedKernel.Specifications;

namespace EricksonLopez.SharedKernel.UnitTests.Specifications;

// ─── Test doubles ─────────────────────────────────────────────────────────────

internal sealed record Product(string Name, decimal Price, bool IsActive);

internal sealed class ActiveProductSpec : Specification<Product>
{
    public override System.Linq.Expressions.Expression<Func<Product, bool>> ToExpression()
        => p => p.IsActive;
}

internal sealed class AffordableProductSpec(decimal maxPrice) : Specification<Product>
{
    public override System.Linq.Expressions.Expression<Func<Product, bool>> ToExpression()
        => p => p.Price <= maxPrice;
}

internal sealed class ShortCircuitSpec : Specification<Product>
{
    public bool WasEvaluated { get; private set; }

    public override System.Linq.Expressions.Expression<Func<Product, bool>> ToExpression()
    {
        return p => RecordEvaluation();
    }

    private bool RecordEvaluation()
    {
        WasEvaluated = true;
        return true;
    }
}

internal sealed class NativeAotSpec : Specification<Product>
{
    public bool EvaluateWasCalled { get; private set; }

    protected override bool Evaluate(Product entity)
    {
        EvaluateWasCalled = true;
        return entity.IsActive;
    }

    public override System.Linq.Expressions.Expression<Func<Product, bool>> ToExpression()
        => throw new InvalidOperationException("ToExpression should not be called when Evaluate is overridden");
}

// ─── Tests ────────────────────────────────────────────────────────────────────

public sealed class SpecificationTests
{
    private static readonly Product ActiveCheap = new(TestValues.Domain.ProductName, 10m, true);
    private static readonly Product ActiveExpensive = new(TestValues.Domain.AlternativeProductName, 500m, true);
    private static readonly Product InactiveCheap = new(TestValues.Strings.Sample, 5m, false);

    [Fact]
    public void SingleSpec_IsSatisfiedBy_ShouldReturnCorrectResult()
    {
        // Arrange
        var spec = new ActiveProductSpec();

        // Act & Assert
        spec.IsSatisfiedBy(ActiveCheap).Should().BeTrue();
        spec.IsSatisfiedBy(InactiveCheap).Should().BeFalse();
    }

    [Fact]
    public void AndSpec_ShouldRequireBothConditions()
    {
        // Arrange
        var spec = new ActiveProductSpec().And(new AffordableProductSpec(100m));

        // Act & Assert
        spec.IsSatisfiedBy(ActiveCheap).Should().BeTrue();
        spec.IsSatisfiedBy(ActiveExpensive).Should().BeFalse();
        spec.IsSatisfiedBy(InactiveCheap).Should().BeFalse();
    }

    [Fact]
    public void OrSpec_ShouldRequireEitherCondition()
    {
        // Arrange
        var spec = new ActiveProductSpec().Or(new AffordableProductSpec(100m));

        // Act & Assert
        spec.IsSatisfiedBy(ActiveCheap).Should().BeTrue();
        spec.IsSatisfiedBy(ActiveExpensive).Should().BeTrue();
        spec.IsSatisfiedBy(InactiveCheap).Should().BeTrue();
    }

    [Fact]
    public void NotSpec_ShouldNegate()
    {
        // Arrange
        var spec = new ActiveProductSpec().Not();

        // Act & Assert
        spec.IsSatisfiedBy(ActiveCheap).Should().BeFalse();
        spec.IsSatisfiedBy(InactiveCheap).Should().BeTrue();
    }

    [Fact]
    public void OperatorAnd_ShouldWorkLikeAndMethod()
    {
        // Arrange
        var spec = new ActiveProductSpec() & new AffordableProductSpec(100m);

        // Act & Assert
        spec.IsSatisfiedBy(ActiveCheap).Should().BeTrue();
        spec.IsSatisfiedBy(ActiveExpensive).Should().BeFalse();
    }

    [Fact]
    public void CompiledExpression_ShouldBeCached_AndProduceSameResult()
    {
        // Arrange
        var spec = new ActiveProductSpec();

        // Act
        var first = spec.IsSatisfiedBy(ActiveCheap);
        var second = spec.IsSatisfiedBy(ActiveCheap);

        // Assert
        first.Should().Be(second).And.BeTrue();
    }

    [Fact]
    public void SpecAsLinqFilter_ShouldWorkOnCollection()
    {
        // Arrange
        var products = new[] { ActiveCheap, ActiveExpensive, InactiveCheap };
        var spec = new ActiveProductSpec();

        // Act
        var filtered = products.Where(spec.IsSatisfiedBy).ToList();

        // Assert
        filtered.Should().HaveCount(2).And.NotContain(InactiveCheap);
    }

    [Fact]
    public void OperatorOr_ShouldWorkLikeOrMethod()
    {
        // Arrange
        var spec = new ActiveProductSpec() | new AffordableProductSpec(100m);

        // Act & Assert
        spec.IsSatisfiedBy(ActiveCheap).Should().BeTrue();
        spec.IsSatisfiedBy(ActiveExpensive).Should().BeTrue();
        spec.IsSatisfiedBy(InactiveCheap).Should().BeTrue();
    }

    [Fact]
    public void OperatorNot_ShouldWorkLikeNotMethod()
    {
        // Arrange
        var spec = !new ActiveProductSpec();

        // Act & Assert
        spec.IsSatisfiedBy(ActiveCheap).Should().BeFalse();
        spec.IsSatisfiedBy(InactiveCheap).Should().BeTrue();
    }

    [Fact]
    public void CompositeSpec_ToExpression_ShouldCompileAndEvaluateCorrectly()
    {
        // Arrange
        var andSpec = new ActiveProductSpec() & new AffordableProductSpec(100m);
        var orSpec = new ActiveProductSpec() | new AffordableProductSpec(100m);
        var notSpec = !new ActiveProductSpec();

        // Act
        var andFunc = andSpec.ToExpression().Compile();
        var orFunc = orSpec.ToExpression().Compile();
        var notFunc = notSpec.ToExpression().Compile();

        // Assert
        andFunc(ActiveCheap).Should().BeTrue();
        andFunc(ActiveExpensive).Should().BeFalse();

        orFunc(ActiveCheap).Should().BeTrue();
        orFunc(InactiveCheap).Should().BeTrue();

        notFunc(ActiveCheap).Should().BeFalse();
        notFunc(InactiveCheap).Should().BeTrue();
    }

    [Fact]
    public void NativeAotSafeSpec_Evaluate_ShouldNotCompileExpression()
    {
        // Arrange
        var spec = new NativeAotSpec();
        
        // Act
        var result = spec.IsSatisfiedBy(ActiveCheap);

        // Assert
        result.Should().BeTrue();
        spec.EvaluateWasCalled.Should().BeTrue();
    }

    [Fact]
    public void CompiledExpression_IsThreadSafe()
    {
        // Arrange
        var spec = new ActiveProductSpec();
        const int iterations = 10_000;
        var successes = 0;

        // Act
        Parallel.For(0, iterations, _ =>
        {
            if (spec.IsSatisfiedBy(ActiveCheap))
            {
                Interlocked.Increment(ref successes);
            }
        });

        // Assert
        successes.Should().Be(iterations, "lazy initialization of CompiledExpression must be thread-safe");
    }

    [Fact]
    public void AndSpec_ShortCircuit_RightSideNotEvaluated_WhenLeftFalse()
    {
        // Arrange
        var rightSide = new ShortCircuitSpec();
        var andSpec = new ActiveProductSpec().Not().And(rightSide);

        // Act
        var result = andSpec.IsSatisfiedBy(ActiveCheap);

        // Assert
        result.Should().BeFalse();
        rightSide.WasEvaluated.Should().BeFalse("AndSpec should short-circuit if left side is false");
    }

    [Fact]
    public void OrSpec_ShortCircuit_RightSideNotEvaluated_WhenLeftTrue()
    {
        // Arrange
        var rightSide = new ShortCircuitSpec();
        var orSpec = new ActiveProductSpec().Or(rightSide);

        // Act — ActiveCheap.IsActive == true, left side satisfies immediately
        var result = orSpec.IsSatisfiedBy(ActiveCheap);

        // Assert
        result.Should().BeTrue();
        rightSide.WasEvaluated.Should().BeFalse("OrSpec must short-circuit when left side is already true");
    }

    [Fact]
    public void TernaryComposition_AAndB_Or_C_ShouldEvaluateCorrectly()
    {
        // Arrange — (Active AND Affordable(100)) OR Affordable(600)
        var active = new ActiveProductSpec();
        var affordable100 = new AffordableProductSpec(100m);
        var affordable600 = new AffordableProductSpec(600m);
        var spec = (active & affordable100) | affordable600;

        // Act & Assert
        spec.IsSatisfiedBy(ActiveCheap).Should().BeTrue("active AND cheap satisfies left branch");
        spec.IsSatisfiedBy(ActiveExpensive).Should().BeTrue("active but expensive — rescued by affordable600");
        spec.IsSatisfiedBy(InactiveCheap).Should().BeTrue("inactive but cheap (<=600)");

        var inactiveExpensive = new Product("Luxury", 999m, false);
        spec.IsSatisfiedBy(inactiveExpensive).Should().BeFalse("inactive AND over 600 satisfies neither branch");
    }

    [Fact]
    public void NotOverCompositeSpec_ShouldNegateCorrectly()
    {
        // Arrange — !(Active AND Affordable(100))
        var spec = !(new ActiveProductSpec() & new AffordableProductSpec(100m));

        // Act & Assert
        spec.IsSatisfiedBy(ActiveCheap).Should().BeFalse("active AND cheap — negated to false");
        spec.IsSatisfiedBy(ActiveExpensive).Should().BeTrue("active AND expensive — negated to true");
        spec.IsSatisfiedBy(InactiveCheap).Should().BeTrue("inactive — left conjunct fails, negated to true");
    }

    [Fact]
    public void CompiledExpression_Cache_IsReusedAcrossMultipleCalls()
    {
        // Arrange — each IsSatisfiedBy call goes through Evaluate -> cached func
        var spec = new ActiveProductSpec();

        // Act — three calls to ensure the cache path is exercised
        var r1 = spec.IsSatisfiedBy(ActiveCheap);
        var r2 = spec.IsSatisfiedBy(ActiveCheap);
        var r3 = spec.IsSatisfiedBy(InactiveCheap);

        // Assert — results must remain correct regardless of caching
        r1.Should().BeTrue();
        r2.Should().BeTrue();
        r3.Should().BeFalse();
    }

    [Fact]
    public void DeepChainSpec_FiveLevels_ShouldEvaluateCorrectly()
    {
        // Arrange — 5-level composition: ((Active AND <=1000) AND NOT <=5) OR NOT Active
        // Simplifies to: (Active AND price > 5 AND price <= 1000) OR NOT Active
        Specification<Product> spec =
            ((new ActiveProductSpec() & new AffordableProductSpec(1000m))
             & !new AffordableProductSpec(5m))
            | !new ActiveProductSpec();

        // Active, price = 10 -> satisfies (active & <=1000 & >5)
        spec.IsSatisfiedBy(ActiveCheap).Should().BeTrue("price=10 satisfies the active+affordable branch");

        // Inactive -> satisfies the NOT Active branch
        spec.IsSatisfiedBy(InactiveCheap).Should().BeTrue("inactive satisfies the right OR branch");

        // Active, price = 3 -> fails >5 constraint; is also active (not !Active)
        var activeTooChap = new Product("X", 3m, true);
        spec.IsSatisfiedBy(activeTooChap).Should().BeFalse("price=3 fails the >5 constraint and is active");
    }
}
