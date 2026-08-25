// Copyright © Erickson Lopez. MIT License.

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using EricksonLopez.DomainPrimitives;
using EricksonLopez.SharedKernel.Json;
using EricksonLopez.SharedKernel.Json.Tests.Fakes;
using EricksonLopez.SharedKernel.TestingUtilities.Fakes;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace EricksonLopez.SharedKernel.Json.Tests;

/// <summary>
/// Exhaustive adversarial, edge-case, and fuzzing test suite for StrongId JSON converters.
/// Verifies resilience against corrupted tokens, malformed payloads, boundary overflows, and unexpected AST types.
/// </summary>
public sealed class StrongIdJsonFuzzingAndAdversarialTests
{
    private readonly JsonSerializerOptions _options;

    public StrongIdJsonFuzzingAndAdversarialTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _options.Converters.Add(new StrongIdJsonConverterFactory());
    }

    #region Adversarial Token Type Injections

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("[1, 2, 3]")]
    [InlineData("{\"Nested\": 123}")]
    [InlineData("123.456")]
    public void Deserialize_GuidStrongId_WithIncompatibleJsonTokens_ThrowsJsonException(string invalidTokenJson)
    {
        var act = () => JsonSerializer.Deserialize<OrderId>(invalidTokenJson, _options);

        act.Should().Throw<JsonException>(
            because: $"JSON token '{invalidTokenJson}' cannot be mapped to a Guid StrongId.");
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("\"NotANumber\"")]
    [InlineData("\"12345\"")] // String quotes when number is expected
    [InlineData("3.14159")]   // Floating point when int expected
    [InlineData("1e10")]      // Exponential notation
    public void Deserialize_IntStrongId_WithIncompatibleJsonTokens_ThrowsJsonException(string invalidTokenJson)
    {
        var act = () => JsonSerializer.Deserialize<Quantity>(invalidTokenJson, _options);

        act.Should().Throw<JsonException>(
            because: $"JSON token '{invalidTokenJson}' cannot be mapped to an integer StrongId.");
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("12345")] // Raw unquoted number when string is expected
    [InlineData("[ \"element\" ]")]
    public void Deserialize_StringStrongId_WithIncompatibleJsonTokens_ThrowsJsonException(string invalidTokenJson)
    {
        var act = () => JsonSerializer.Deserialize<ProductCode>(invalidTokenJson, _options);

        act.Should().Throw<JsonException>(
            because: $"JSON token '{invalidTokenJson}' cannot be mapped to a string StrongId without string tokens.");
    }

    #endregion

    #region Boundary Overflows and Malformed Representations

    [Fact]
    public void Deserialize_IntStrongId_WhenNumberOverflowsInt32_ThrowsJsonException()
    {
        // 9999999999999999999 exceeds Int32.MaxValue
        const string overflowJson = "9999999999999999999";

        var act = () => JsonSerializer.Deserialize<Quantity>(overflowJson, _options);

        act.Should().Throw<JsonException>();
    }

    [Theory]
    [InlineData("\"not-a-valid-guid-format\"")]
    [InlineData("\"00000000-0000-0000-0000\"")]
    [InlineData("\"12345678-1234-1234-1234-123456789abcde\"")] // Too long
    [InlineData("\"\"")] // Empty string
    [InlineData("\"   \"")] // Whitespace only
    public void Deserialize_GuidStrongId_WithMalformedGuidStrings_ThrowsJsonException(string malformedGuidJson)
    {
        var act = () => JsonSerializer.Deserialize<OrderId>(malformedGuidJson, _options);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_TruncatedStream_ThrowsJsonException()
    {
        // Incomplete JSON string token
        var truncatedBytes = Encoding.UTF8.GetBytes("\"01a02b94-b582-7001-871e-");

        var act = () =>
        {
            var reader = new Utf8JsonReader(truncatedBytes);
            reader.Read();
            var converter = new StrongIdJsonConverter<OrderId, Guid>();
            converter.Read(ref reader, typeof(OrderId), _options);
        };

        act.Should().Throw<JsonException>();
    }

    #endregion

    #region Complex DTO Adversarial Testing

    private sealed class AdversarialOrderDto
    {
        public OrderId? PrimaryId { get; set; }
        public ProductCode? Sku { get; set; }
        public Quantity? Amount { get; set; }
        public OrderId[]? AdditionalIds { get; set; }
    }

    [Fact]
    public void Deserialize_ComplexDto_WithCorruptedArrayItem_ThrowsJsonException()
    {
        var json = $$"""
        {
            "PrimaryId": "{{Guid.NewGuid()}}",
            "Sku": "SKU-VALID-1",
            "Amount": 10,
            "AdditionalIds": [
                "{{Guid.NewGuid()}}",
                "INVALID-GUID-ITEM",
                "{{Guid.NewGuid()}}"
            ]
        }
        """;

        var act = () => JsonSerializer.Deserialize<AdversarialOrderDto>(json, _options);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Roundtrip_ComplexDto_WithNullOptionalStrongIds_PreservesStructure()
    {
        var dto = new AdversarialOrderDto
        {
            PrimaryId = null,
            Sku = null,
            Amount = null,
            AdditionalIds = null
        };

        var json = JsonSerializer.Serialize(dto, _options);
        var deserialized = JsonSerializer.Deserialize<AdversarialOrderDto>(json, _options);

        deserialized.Should().NotBeNull();
        deserialized!.PrimaryId.Should().BeNull();
        deserialized.Sku.Should().BeNull();
        deserialized.Amount.Should().BeNull();
        deserialized.AdditionalIds.Should().BeNull();
    }

    [Fact]
    public void Roundtrip_ComplexDto_WithPopulatedArrayOfStrongIds_PreservesAllElements()
    {
        var id1 = OrderId.Create(Guid.NewGuid());
        var id2 = OrderId.Create(Guid.NewGuid());
        var id3 = OrderId.Create(Guid.NewGuid());

        var dto = new AdversarialOrderDto
        {
            PrimaryId = id1,
            Sku = ProductCode.Create("PROD-ARRAY-TEST"),
            Amount = Quantity.Create(99),
            AdditionalIds = [id1, id2, id3]
        };

        var json = JsonSerializer.Serialize(dto, _options);
        var deserialized = JsonSerializer.Deserialize<AdversarialOrderDto>(json, _options);

        deserialized.Should().NotBeNull();
        deserialized!.PrimaryId.Should().Be(id1);
        deserialized.AdditionalIds.Should().HaveCount(3);
        deserialized.AdditionalIds![0].Should().Be(id1);
        deserialized.AdditionalIds[1].Should().Be(id2);
        deserialized.AdditionalIds[2].Should().Be(id3);
    }

    #endregion

    #region FsCheck Adversarial Property-Based Tests

    [Property]
    public Property Fuzz_RandomStringPayloads_NeverCrashUncaught(NonNull<string> randomPayload)
    {
        var raw = randomPayload.Get;
        var json = JsonSerializer.Serialize(raw); // Ensures it is a valid JSON string literal

        try
        {
            _ = JsonSerializer.Deserialize<ProductCode>(json, _options);
        }
        catch (JsonException)
        {
            // Expected for invalid domain invariants (e.g. whitespace, empty, synthetic format error)
        }

        // Must either successfully deserialize or throw a controlled JsonException without uncaught crashes
        return true.ToProperty();
    }

    [Property]
    public Property Fuzz_RandomIntegerPayloads_NeverCrashUncaught(int randomInt)
    {
        var json = randomInt.ToString(System.Globalization.CultureInfo.InvariantCulture);

        try
        {
            _ = JsonSerializer.Deserialize<Quantity>(json, _options);
        }
        catch (JsonException)
        {
            // Expected for domain negative value validation
        }

        return true.ToProperty();
    }

    #endregion
}
