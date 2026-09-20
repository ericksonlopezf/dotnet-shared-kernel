// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.SharedKernel.Persistence;

using System;

/// <summary>
/// Marks a property as an audit field. The Unit of Work will automatically populate this field during inserts/updates.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class AuditAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditAttribute"/> class.
    /// </summary>
    /// <param name="fieldType">The type of audit data represented by this property.</param>
    public AuditAttribute(AuditFieldType fieldType)
    {
        FieldType = fieldType;
    }

    /// <summary>
    /// Gets the audit data type represented by this property.
    /// </summary>
    public AuditFieldType FieldType { get; }
}
