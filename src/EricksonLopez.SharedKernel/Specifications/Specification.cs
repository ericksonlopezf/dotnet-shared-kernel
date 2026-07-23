using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EricksonLopez.SharedKernel.Specifications;

/// <summary>
/// Base class for specifications. Provides composition operators (And, Or, Not)
/// and NativeAOT-safe in-memory evaluation.
/// </summary>
/// <remarks>
/// <para>
/// Specifications have two evaluation paths:
/// <list type="bullet">
///   <item><see cref="ToExpression"/> — Returns an expression tree for LINQ-to-SQL
///   translation (EF Core, Dapper). Never compiled at runtime.</item>
///   <item><see cref="IsSatisfiedBy"/> — Evaluates in-memory via <see cref="Evaluate"/>.
///   Override <see cref="Evaluate"/> for NativeAOT-compatible evaluation without
///   <c>Expression.Compile()</c>.</item>
/// </list>
/// </para>
/// <para>
/// <b>NativeAOT:</b> The default <see cref="Evaluate"/> fallback uses
/// <c>Expression.Compile()</c>, which requires the JIT. For NativeAOT scenarios,
/// override <see cref="Evaluate"/> in your leaf specifications. Composite
/// specifications (And, Or, Not) are already NativeAOT-safe — they delegate
/// to their children's <see cref="IsSatisfiedBy"/> without compiling.
/// </para>
/// <para>
/// <b>Example:</b>
/// <code>
/// public sealed class ActiveSpec : Specification&lt;Product&gt;
/// {
///     public override Expression&lt;Func&lt;Product, bool&gt;&gt; ToExpression()
///         =&gt; p =&gt; p.IsActive;
///
///     // Optional: NativeAOT-safe override
///     protected override bool Evaluate(Product candidate)
///         =&gt; candidate.IsActive;
/// }
/// </code>
/// </para>
/// </remarks>
/// <typeparam name="T">The type the specification applies to.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    private Func<T, bool>? _compiledExpression;

    /// <inheritdoc/>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <inheritdoc/>
    public bool IsSatisfiedBy(T candidate) => Evaluate(candidate);

    /// <summary>
    /// Evaluates the specification against a candidate in memory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default implementation compiles the expression tree from
    /// <see cref="ToExpression"/> and caches the result. This requires
    /// the JIT and is <b>not compatible with NativeAOT</b>.
    /// </para>
    /// <para>
    /// Override this method for NativeAOT-safe evaluation. Composite
    /// specifications (And, Or, Not) already override this to delegate
    /// to their children without compiling.
    /// </para>
    /// </remarks>
    /// <param name="candidate">The instance to evaluate against.</param>
    /// <returns><c>true</c> if the candidate satisfies the specification.</returns>
    protected virtual bool Evaluate(T candidate)
    {
        // Fallback: compiles the expression tree for in-memory evaluation.
        // For NativeAOT, override this method to avoid Expression.Compile().
#pragma warning disable IL3050 // RequiresDynamicCode — virtual method; consumers override for AOT
        _compiledExpression ??= ToExpression().Compile();
#pragma warning restore IL3050
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

// ─── Composite specifications (NativeAOT-safe) ───────────────────────────────

/// <remarks>
/// Evaluates children directly via <see cref="Specification{T}.IsSatisfiedBy"/>
/// — no <c>Expression.Compile()</c> needed. NativeAOT-safe.
/// </remarks>
internal sealed class AndSpecification<T>(Specification<T> left, Specification<T> right)
    : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = left.ToExpression();
        var rightExpr = right.ToExpression();

        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(leftExpr.Parameters[0], parameter);
        var leftBody = leftVisitor.Visit(leftExpr.Body);

        var rightVisitor = new ReplaceExpressionVisitor(rightExpr.Parameters[0], parameter);
        var rightBody = rightVisitor.Visit(rightExpr.Body);

        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(leftBody, rightBody), parameter);
    }

    /// <summary>NativeAOT-safe: delegates to children without compiling.</summary>
    protected override bool Evaluate(T candidate)
        => left.IsSatisfiedBy(candidate) && right.IsSatisfiedBy(candidate);
}

/// <inheritdoc cref="AndSpecification{T}"/>
internal sealed class OrSpecification<T>(Specification<T> left, Specification<T> right)
    : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = left.ToExpression();
        var rightExpr = right.ToExpression();

        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(leftExpr.Parameters[0], parameter);
        var leftBody = leftVisitor.Visit(leftExpr.Body);

        var rightVisitor = new ReplaceExpressionVisitor(rightExpr.Parameters[0], parameter);
        var rightBody = rightVisitor.Visit(rightExpr.Body);

        return Expression.Lambda<Func<T, bool>>(
            Expression.OrElse(leftBody, rightBody), parameter);
    }

    /// <summary>NativeAOT-safe: delegates to children without compiling.</summary>
    protected override bool Evaluate(T candidate)
        => left.IsSatisfiedBy(candidate) || right.IsSatisfiedBy(candidate);
}

/// <inheritdoc cref="AndSpecification{T}"/>
internal sealed class NotSpecification<T>(Specification<T> inner) : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var innerExpr = inner.ToExpression();
        var body = Expression.Not(innerExpr.Body);
        return Expression.Lambda<Func<T, bool>>(body, innerExpr.Parameters[0]);
    }

    /// <summary>NativeAOT-safe: delegates to child without compiling.</summary>
    protected override bool Evaluate(T candidate)
        => !inner.IsSatisfiedBy(candidate);
}

internal sealed class ReplaceExpressionVisitor(Expression oldValue, Expression newValue) : ExpressionVisitor
{
    public override Expression Visit(Expression? node)
    {
        if (node == oldValue)
            return newValue;

        return base.Visit(node)!;
    }
}
