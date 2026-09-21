// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.SharedKernel.Persistence;

using System;

/// <summary>
/// Specifies the database table that a class is mapped to.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class TableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TableAttribute"/> class with the specified table name.
    /// </summary>
    /// <param name="name">The name of the database table.</param>
    public TableAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the name of the table.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the schema of the table.
    /// </summary>
    public string? Schema { get; set; }
}
