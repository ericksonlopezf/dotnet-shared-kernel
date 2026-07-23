using System;
using System.Linq.Expressions;

namespace EricksonLopez.SharedKernel.Specifications;

/// <summary>
/// Defines the contract for a specification — an encapsulated, reusable business rule
/// expressed as a predicate over a type.
/// </summary>
/// <typeparam name="T">The type the specification applies to.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// The expression tree representing this specification's predicate.
    /// Can be composed into LINQ-to-SQL queries.
    /// </summary>
    Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Evaluates the specification against a concrete instance in memory.
    /// </summary>
    bool IsSatisfiedBy(T candidate);
}

