// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.DomainPrimitives;

namespace EricksonLopez.SharedKernel.Json;

/// <summary>
/// Provides a JSON converter factory that creates converter instances for types implementing <see cref="IStrongId{TSelf, TValue}"/>.
/// </summary>
[RequiresDynamicCode(
    "The converter factory creates closed generic converter types at runtime.")]
[RequiresUnreferencedCode(
    "The converter factory discovers strongly typed identifiers through reflection.")]
public sealed class StrongIdJsonConverterFactory
    : JsonConverterFactory
{
    private static readonly ConcurrentDictionary<Type, JsonConverter>
        ConverterCache = new();

    /// <summary>
    /// Determines whether this factory can produce a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to evaluate for converter support.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="typeToConvert"/> implements
    /// <see cref="IStrongId{TSelf, TValue}"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeToConvert"/> is <see langword="null"/>.</exception>
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        return GetStrongIdInterface(typeToConvert) is not null;
    }

    /// <summary>
    /// Creates a <see cref="StrongIdJsonConverter{TSelf, TValue}"/> for the specified strongly-typed identifier type.
    /// </summary>
    /// <remarks>
    /// Converter instances are cached per type; repeated calls for the same <paramref name="typeToConvert"/> return the same converter.
    /// </remarks>
    /// <param name="typeToConvert">The strongly-typed identifier type for which to create a converter.</param>
    /// <param name="options">The serializer options passed to the created converter.</param>
    /// <returns>A <see cref="JsonConverter"/> for <paramref name="typeToConvert"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="typeToConvert"/> or <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="typeToConvert"/> does not implement <see cref="IStrongId{TSelf, TValue}"/>.
    /// </exception>
    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        return ConverterCache.GetOrAdd(
            typeToConvert,
            static targetType =>
            {
                var strongIdInterface =
                    GetStrongIdInterface(targetType)
                    ?? throw new InvalidOperationException(
                        $"Type '{targetType.FullName}' does not implement " +
                        $"'{typeof(IStrongId<,>).FullName}'.");

                var genericArguments =
                    strongIdInterface.GetGenericArguments();

                var converterType =
                    typeof(StrongIdJsonConverter<,>)
                        .MakeGenericType(genericArguments);

                return (JsonConverter)
                    Activator.CreateInstance(converterType)!;
            });
    }

    private static Type? GetStrongIdInterface(Type type)
    {
        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType &&
                iface.GetGenericTypeDefinition() ==
                typeof(IStrongId<,>))
            {
                return iface;
            }
        }

        return null;
    }
}
