using System.Linq.Expressions;

namespace EricksonLopez.SharedKernel.Specifications;

/// <summary>
/// Base class for specifications. Provides composition operators (And, Or, Not)
/// and in-memory evaluation via compiled expression.
/// </summary>
/// <typeparam name="T">The type the specification applies to.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    private Func<T, bool>? _compiledExpression;

    /// <inheritdoc/>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <inheritdoc/>
    public bool IsSatisfiedBy(T candidate)
    {
        _compiledExpression ??= ToExpression().Compile();
        return _compiledExpression(candidate);
    }

    // ─── Composition ─────────────────────────────────────────────────────────

    /// <summary>
    /// Combines this specification with another using a logical AND.
    /// </summary>
    public Specification<T> And(Specification<T> other)
        => new AndSpecification<T>(this, other);

    /// <summary>
    /// Combines this specification with another using a logical OR.
    /// </summary>
    public Specification<T> Or(Specification<T> other)
        => new OrSpecification<T>(this, other);

    /// <summary>
    /// Negates this specification.
    /// </summary>
    public Specification<T> Not()
        => new NotSpecification<T>(this);

    // ─── Operators ────────────────────────────────────────────────────────────

    public static Specification<T> operator &(Specification<T> left, Specification<T> right)
        => left.And(right);

    public static Specification<T> operator |(Specification<T> left, Specification<T> right)
        => left.Or(right);

    public static Specification<T> operator !(Specification<T> spec)
        => spec.Not();
}

// ─── Composite specifications ─────────────────────────────────────────────────

internal sealed class AndSpecification<T>(Specification<T> left, Specification<T> right)
    : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = left.ToExpression();
        var rightExpr = right.ToExpression();

        // Reuse the same parameter to avoid "parameter not in scope" errors
        var parameter = leftExpr.Parameters[0];
        var body = Expression.AndAlso(
            leftExpr.Body,
            Expression.Invoke(rightExpr, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

internal sealed class OrSpecification<T>(Specification<T> left, Specification<T> right)
    : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = left.ToExpression();
        var rightExpr = right.ToExpression();

        var parameter = leftExpr.Parameters[0];
        var body = Expression.OrElse(
            leftExpr.Body,
            Expression.Invoke(rightExpr, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

internal sealed class NotSpecification<T>(Specification<T> inner) : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var innerExpr = inner.ToExpression();
        var body = Expression.Not(innerExpr.Body);
        return Expression.Lambda<Func<T, bool>>(body, innerExpr.Parameters[0]);
    }
}
