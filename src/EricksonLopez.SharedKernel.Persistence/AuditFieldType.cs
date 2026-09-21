// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.SharedKernel.Persistence;

/// <summary>
/// Defines the type of audit data a property represents.
/// </summary>
public enum AuditFieldType
{
    /// <summary>
    /// Timestamp when the record was created.
    /// </summary>
    CreatedAt,

    /// <summary>
    /// Identifier of the user or system that created the record.
    /// </summary>
    CreatedBy,

    /// <summary>
    /// Timestamp when the record was last modified.
    /// </summary>
    UpdatedAt,

    /// <summary>
    /// Identifier of the user or system that last modified the record.
    /// </summary>
    UpdatedBy,

    /// <summary>
    /// Concurrency row version or revision token.
    /// </summary>
    Version
}
