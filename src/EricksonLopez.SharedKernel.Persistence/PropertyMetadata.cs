// Copyright © Erickson Lopez. MIT License.
namespace EricksonLopez.SharedKernel.Persistence;

using System;

using System.Runtime.CompilerServices;

/// <summary>
/// Represents compile-time extracted metadata for a single property of a domain entity.
/// Provides statically typed delegate accessors (Getter and Setter) to eliminate runtime reflection.
/// </summary>
/// <param name="ClrName">The CLR name of the property.</param>
/// <param name="ClrType">The CLR type of the property.</param>
/// <param name="ColumnName">The mapped database column name.</param>
/// <param name="DatabaseType">The database data type descriptor.</param>
/// <param name="IsKey">True if this column is part of the primary key.</param>
/// <param name="IsNullable">True if this property accepts null values.</param>
/// <param name="IsAuditColumn">True if this property is decorated with an audit marker.</param>
/// <param name="IsSoftDeleteColumn">True if this property manages soft deletion state.</param>
/// <param name="IsTenantColumn">True if this property holds the tenant identifier.</param>
/// <param name="IsConcurrencyToken">True if this property acts as an optimistic concurrency token.</param>
/// <param name="Getter">Compiled delegate to retrieve the property value without reflection.</param>
/// <param name="Setter">Compiled delegate to set the property value without reflection, or null if read-only.</param>
public sealed record PropertyMetadata(
    string ClrName,
    Type ClrType,
    string ColumnName,
    string DatabaseType,
    bool IsKey,
    bool IsNullable,
    bool IsAuditColumn,
    bool IsSoftDeleteColumn,
    bool IsTenantColumn,
    bool IsConcurrencyToken,
    Func<object, object?> Getter,
    Action<object, object?>? Setter)
{
    /// <summary>
    /// Gets the optional strongly typed getter delegate (e.g. Func&lt;TEntity, TProperty&gt;).
    /// </summary>
    public Delegate? StronglyTypedGetter { get; init; }

    /// <summary>
    /// Gets the optional strongly typed setter delegate (e.g. Action&lt;TEntity, TProperty&gt;).
    /// </summary>
    public Delegate? StronglyTypedSetter { get; init; }

    /// <summary>
    /// Retrieves the property value without boxing allocations when strongly-typed delegate is available.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TProperty GetValue<TEntity, TProperty>(TEntity entity)
    {
        if (StronglyTypedGetter is Func<TEntity, TProperty> typedGetter)
        {
            return typedGetter(entity);
        }

        var val = Getter(entity!);
        return val is null ? default! : (TProperty)val;
    }

    /// <summary>
    /// Assigns the property value without boxing allocations when strongly-typed delegate is available.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetValue<TEntity, TProperty>(TEntity entity, TProperty value)
    {
        if (StronglyTypedSetter is Action<TEntity, TProperty> typedSetter)
        {
            typedSetter(entity, value);
            return;
        }

        Setter?.Invoke(entity!, value);
    }
}
