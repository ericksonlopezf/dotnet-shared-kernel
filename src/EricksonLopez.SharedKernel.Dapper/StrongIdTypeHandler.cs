// Copyright © Erickson Lopez. MIT License.
using System;
using System.Data;
using Dapper;
using EricksonLopez.DomainPrimitives;

namespace EricksonLopez.SharedKernel.Dapper;

/// <summary>
/// Represents a Dapper type handler for mapping strongly-typed domain identifiers to and from database parameters.
/// </summary>
/// <typeparam name="TSelf">The strongly-typed identifier type.</typeparam>
/// <typeparam name="TValue">The underlying database primitive value type.</typeparam>
public sealed class StrongIdTypeHandler<TSelf, TValue>
    : SqlMapper.TypeHandler<TSelf>
    where TSelf : notnull, IStrongId<TSelf, TValue>
    where TValue : notnull, IEquatable<TValue>
{
    /// <summary>
    /// Sets the database parameter value from the specified strongly-typed identifier.
    /// </summary>
    /// <param name="parameter">The database parameter to configure.</param>
    /// <param name="value">
    /// The strongly-typed identifier whose underlying value is applied to the parameter.
    /// When <see langword="null"/> or when the underlying value is <see langword="null"/>,
    /// sets the parameter value to <see cref="DBNull.Value"/>.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="parameter"/> is <see langword="null"/>.</exception>
    public override void SetValue(
        IDbDataParameter parameter,
        TSelf? value)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        parameter.Value = value is null || value.Value is null
            ? DBNull.Value
            : value.Value;
    }

    /// <summary>
    /// Converts a raw database value to a <typeparamref name="TSelf"/> strongly-typed identifier.
    /// </summary>
    /// <param name="value">The raw database value to convert.</param>
    /// <returns>A <typeparamref name="TSelf"/> instance constructed from the database value.</returns>
    /// <exception cref="System.Data.DataException">
    /// <paramref name="value"/> is <see langword="null"/> or <see cref="DBNull"/>,
    /// <paramref name="value"/> is not assignable to <typeparamref name="TValue"/>,
    /// or the primitive value is not valid for <typeparamref name="TSelf"/>.
    /// </exception>
    public override TSelf Parse(object value)
    {
        if (value is null || value is DBNull)
        {
            throw new DataException(
                $"Cannot map a null database value to the non-nullable " +
                $"strong identifier '{typeof(TSelf).FullName}'.");
        }

        if (value is not TValue primitive)
        {
            throw new DataException(
                $"Database type '{value.GetType().FullName}' is incompatible " +
                $"with strong identifier '{typeof(TSelf).FullName}', " +
                $"which requires '{typeof(TValue).FullName}'.");
        }

        try
        {
            return TSelf.Create(primitive);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException)
        {
            throw new DataException(
                $"The database value is invalid for strong identifier " +
                $"'{typeof(TSelf).FullName}'.",
                ex);
        }
    }
}
