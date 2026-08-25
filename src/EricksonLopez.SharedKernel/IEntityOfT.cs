// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.SharedKernel;

/// <summary>
/// Defines the contract for domain entities with a strongly-typed identifier.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier type of the entity.</typeparam>
public interface IEntity<TId> : IEntity
    where TId : notnull, IEquatable<TId>
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    TId Id { get; }
}
