// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.DomainPrimitives;

namespace EricksonLopez.SharedKernel.Json;

/// <summary>
/// Represents a JSON converter for strongly-typed domain identifiers that serializes and deserializes values using their underlying primitive type.
/// </summary>
/// <typeparam name="TSelf">The strongly-typed identifier type.</typeparam>
/// <typeparam name="TValue">The underlying primitive value type.</typeparam>
[RequiresDynamicCode("JSON converter for generic strong IDs requires dynamic code for arbitrary underlying types.")]
[RequiresUnreferencedCode("JSON converter for generic strong IDs requires reflection for arbitrary underlying types.")]
public sealed class StrongIdJsonConverter<TSelf, TValue>
    : JsonConverter<TSelf>
    where TSelf : notnull, IStrongId<TSelf, TValue>
    where TValue : notnull, IEquatable<TValue>
{
    /// <summary>
    /// Reads and converts a JSON value to a <typeparamref name="TSelf"/> instance
    /// using the underlying primitive type <typeparamref name="TValue"/>.
    /// </summary>
    /// <param name="reader">The reader to read the JSON value from.</param>
    /// <param name="typeToConvert">The target type to convert to.</param>
    /// <param name="options">The serializer options to use during deserialization.</param>
    /// <returns>The deserialized <typeparamref name="TSelf"/> instance.</returns>
    /// <exception cref="System.Text.Json.JsonException">
    /// The JSON token is <see langword="null"/>, the underlying primitive is <see langword="null"/>,
    /// or the primitive value is not valid for <typeparamref name="TSelf"/>.
    /// </exception>
    public override TSelf Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            throw new JsonException(
                $"Null is not a valid value for strong identifier " +
                $"'{typeof(TSelf).FullName}'.");
        }

        var primitive = JsonSerializer.Deserialize<TValue>(ref reader, options);

        if (primitive is null)
        {
            throw new JsonException(
                $"The underlying primitive value for strong identifier " +
                $"'{typeof(TSelf).FullName}' cannot be null.");
        }

        try
        {
            return TSelf.Create(primitive);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException)
        {
            throw new JsonException(
                $"The JSON value is invalid for strong identifier " +
                $"'{typeof(TSelf).FullName}'.",
                ex);
        }
    }

    /// <summary>
    /// Writes a <typeparamref name="TSelf"/> value as its underlying <typeparamref name="TValue"/> primitive to the JSON writer.
    /// </summary>
    /// <param name="writer">The writer to write the JSON value to.</param>
    /// <param name="value">The strongly-typed identifier value to serialize.</param>
    /// <param name="options">The serializer options to use during serialization.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
    public override void Write(
        Utf8JsonWriter writer,
        TSelf value,
        JsonSerializerOptions options)
    {
        // Stryker disable once Statement: JsonSerializer.Serialize also throws ArgumentNullException for null writer
        ArgumentNullException.ThrowIfNull(writer);

        JsonSerializer.Serialize(
            writer,
            value.Value,
            options);
    }
}
