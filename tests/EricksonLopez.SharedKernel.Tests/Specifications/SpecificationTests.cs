using EricksonLopez.SharedKernel.Specifications;
using AwesomeAssertions;

namespace EricksonLopez.SharedKernel.Tests.Specifications;

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

// ─── Tests ────────────────────────────────────────────────────────────────────

public sealed class SpecificationTests
{
    private static readonly Product ActiveCheap = new("Widget", 10m, true);
    private static readonly Product ActiveExpensive = new("Gadget", 500m, true);
    private static readonly Product InactiveCheap = new("Doohickey", 5m, false);

    [Fact]
    public void SingleSpec_IsSatisfiedBy_ShouldReturnCorrectResult()
    {
        var spec = new ActiveProductSpec();

        spec.IsSatisfiedBy(ActiveCheap).Should().BeTrue();
        spec.IsSatisfiedBy(InactiveCheap).Should().BeFalse();
    }

    [Fact]
    public void AndSpec_ShouldRequireBothConditions()
    {
        var spec = new ActiveProductSpec().And(new AffordableProductSpec(100m));

        spec.IsSatisfiedBy(ActiveCheap).Should().BeTrue();
        spec.IsSatisfiedBy(ActiveExpensive).Should().BeFalse();
        spec.IsSatisfiedBy(InactiveCheap).Should().BeFalse();
    }

    [Fact]
    public void OrSpec_ShouldRequireEitherCondition()
    {
        var spec = new ActiveProductSpec().Or(new AffordableProductSpec(100m));

        spec.IsSatisfiedBy(ActiveCheap).Should().BeTrue();
        spec.IsSatisfiedBy(ActiveExpensive).Should().BeTrue();
        spec.IsSatisfiedBy(InactiveCheap).Should().BeTrue();
    }

    [Fact]
    public void NotSpec_ShouldNegate()
    {
        var spec = new ActiveProductSpec().Not();

        spec.IsSatisfiedBy(ActiveCheap).Should().BeFalse();
        spec.IsSatisfiedBy(InactiveCheap).Should().BeTrue();
    }

    [Fact]
    public void OperatorAnd_ShouldWorkLikeAndMethod()
    {
        var spec = new ActiveProductSpec() & new AffordableProductSpec(100m);

        spec.IsSatisfiedBy(ActiveCheap).Should().BeTrue();
        spec.IsSatisfiedBy(ActiveExpensive).Should().BeFalse();
    }

    [Fact]
    public void CompiledExpression_ShouldBeCached_AndProduceSameResult()
    {
        var spec = new ActiveProductSpec();

        var first = spec.IsSatisfiedBy(ActiveCheap);
        var second = spec.IsSatisfiedBy(ActiveCheap);

        first.Should().Be(second).And.BeTrue();
    }

    [Fact]
    public void SpecAsLinqFilter_ShouldWorkOnCollection()
    {
        var products = new[] { ActiveCheap, ActiveExpensive, InactiveCheap };
        var spec = new ActiveProductSpec();

        var filtered = products.Where(spec.IsSatisfiedBy).ToList();

        filtered.Should().HaveCount(2).And.NotContain(InactiveCheap);
    }

    [Fact]
    public void OperatorOr_ShouldWorkLikeOrMethod()
    {
        var spec = new ActiveProductSpec() | new AffordableProductSpec(100m);

        spec.IsSatisfiedBy(ActiveCheap).Should().BeTrue();
        spec.IsSatisfiedBy(ActiveExpensive).Should().BeTrue();
        spec.IsSatisfiedBy(InactiveCheap).Should().BeTrue();
    }

    [Fact]
    public void OperatorNot_ShouldWorkLikeNotMethod()
    {
        var spec = !new ActiveProductSpec();

        spec.IsSatisfiedBy(ActiveCheap).Should().BeFalse();
        spec.IsSatisfiedBy(InactiveCheap).Should().BeTrue();
    }

    [Fact]
    public void CompositeSpec_ToExpression_ShouldCompileAndEvaluateCorrectly()
    {
        var andSpec = new ActiveProductSpec() & new AffordableProductSpec(100m);
        var andFunc = andSpec.ToExpression().Compile();
        andFunc(ActiveCheap).Should().BeTrue();
        andFunc(ActiveExpensive).Should().BeFalse();

        var orSpec = new ActiveProductSpec() | new AffordableProductSpec(100m);
        var orFunc = orSpec.ToExpression().Compile();
        orFunc(ActiveCheap).Should().BeTrue();
        orFunc(InactiveCheap).Should().BeTrue();

        var notSpec = !new ActiveProductSpec();
        var notFunc = notSpec.ToExpression().Compile();
        notFunc(ActiveCheap).Should().BeFalse();
        notFunc(InactiveCheap).Should().BeTrue();
    }
}
