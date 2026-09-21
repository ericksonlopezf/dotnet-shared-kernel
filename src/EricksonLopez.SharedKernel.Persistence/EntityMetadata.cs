// Copyright © Erickson Lopez. MIT License.
namespace EricksonLopez.SharedKernel.Persistence;

using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Represents compile-time extracted schema and persistence metadata for a domain entity.
/// Contains pre-computed column indices, primary keys, and specialized structural markers (tenant, audit, concurrency).
/// </summary>
public sealed record EntityMetadata
{
    /// <summary>
    /// Gets the CLR type of the domain entity.
    /// </summary>
    public required Type ClrType { get; init; }

    /// <summary>
    /// Gets the target database table name.
    /// </summary>
    public string TableName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the database schema name (defaults to "public").
    /// </summary>
    public string Schema { get; init; } = "public";

    /// <summary>
    /// Gets the complete list of mapped columns for this entity.
    /// </summary>
    public IReadOnlyList<PropertyMetadata> Columns { get; init; } = [];

    /// <summary>
    /// Gets the list of primary key columns for this entity.
    /// </summary>
    public IReadOnlyList<PropertyMetadata> PrimaryKeys { get; init; } = [];

    /// <summary>
    /// Gets the tenant column metadata, or null if the entity is not tenant-scoped.
    /// </summary>
    public PropertyMetadata? TenantColumn { get; init; }

    /// <summary>
    /// Gets the soft delete column metadata, or null if soft deletion is not enabled.
    /// </summary>
    public PropertyMetadata? SoftDeleteColumn { get; init; }

    /// <summary>
    /// Gets the concurrency token column metadata, or null if optimistic concurrency is not configured.
    /// </summary>
    public PropertyMetadata? ConcurrencyToken { get; init; }

    private Dictionary<string, PropertyMetadata>? _columnIndex;

    // REM-006: Dedicated lock object required by the thread-safe EnsureInitialized overload.
    // The single-argument overload allows duplicate factory invocations under concurrent access.
    private object? _columnIndexLock;

    private Dictionary<string, PropertyMetadata> ColumnIndex =>
        LazyInitializer.EnsureInitialized(ref _columnIndex, ref _columnIndexLock, () =>
        {
            var dict = new Dictionary<string, PropertyMetadata>(Columns.Count, StringComparer.Ordinal);
            foreach (var col in Columns)
            {
                dict[col.ClrName] = col;
            }
            return dict;
        });


    /// <summary>
    /// Retrieves the property metadata for the given CLR property name.
    /// </summary>
    /// <param name="clrName">The name of the CLR property.</param>
    /// <returns>The associated <see cref="PropertyMetadata"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no column matches the property name.</exception>
    public PropertyMetadata GetColumn(string clrName)
    {
        if (ColumnIndex.TryGetValue(clrName, out var meta))
            return meta;
        throw new KeyNotFoundException($"Column '{clrName}' not found on entity '{ClrType.Name}'.");
    }

    /// <summary>
    /// Attempts to retrieve the property metadata for the given CLR property name.
    /// </summary>
    /// <param name="clrName">The name of the CLR property.</param>
    /// <param name="meta">When this method returns, contains the metadata if found; otherwise, null.</param>
    /// <returns>True if the column metadata was found; otherwise, false.</returns>
    public bool TryGetColumn(string clrName, out PropertyMetadata? meta) =>
        ColumnIndex.TryGetValue(clrName, out meta);
}
