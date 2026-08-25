// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore;

using EricksonLopez.DomainPrimitives;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
/// Represents an Entity Framework Core value converter for strongly-typed domain identifiers implementing <see cref="IStrongId{TSelf, TValue}"/>.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
/// <typeparam name="TValue">The underlying primitive value type.</typeparam>
public class StrongIdValueConverter<TId, TValue> : ValueConverter<TId, TValue>
    where TId : notnull, IStrongId<TId, TValue>
    where TValue : notnull, IEquatable<TValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StrongIdValueConverter{TId, TValue}"/> class using the default factory and no mapping hints.
    /// </summary>
    public StrongIdValueConverter()
        : this((ConverterMappingHints?)null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StrongIdValueConverter{TId, TValue}"/> class using the static factory method <see cref="IStrongId{TSelf, TValue}.Create"/> and optional mapping hints.
    /// </summary>
    /// <param name="mappingHints">The optional converter mapping hints.</param>
    public StrongIdValueConverter(ConverterMappingHints? mappingHints)
        : this(TId.Create, mappingHints)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StrongIdValueConverter{TId, TValue}"/> class using the specified factory delegate and optional mapping hints.
    /// </summary>
    /// <param name="factory">The factory delegate used to construct an instance of <typeparamref name="TId"/> from its underlying <typeparamref name="TValue"/>.</param>
    /// <param name="mappingHints">The optional converter mapping hints.</param>
    /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <see langword="null"/></exception>
    public StrongIdValueConverter(Func<TValue, TId> factory, ConverterMappingHints? mappingHints = null)
        : base(
            id => id.Value,
            value => factory(value),
            mappingHints)
    {
        ArgumentNullException.ThrowIfNull(factory);
    }
}


