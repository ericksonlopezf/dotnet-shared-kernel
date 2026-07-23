using System.Text.Json;

namespace EricksonLopez.SharedKernel.UnitTests.Results;

/// <summary>
/// Documents System.Text.Json serialization behavior for domain types.
/// These tests deliberately avoid adding a JsonConverter to the production library.
/// Per the project design principle: serialization infrastructure belongs in the
/// application/presentation layer, not in the SharedKernel domain core.
/// </summary>
public sealed class ResultSerializationTests
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // ── Error serialization ───────────────────────────────────────────────────

    [Fact]
    public void Error_CanBeSerializedToJson_UsingDefaultSerializer()
    {
        // Arrange
        var error = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act — serialization must not throw (public properties are accessible)
        var act = () => JsonSerializer.Serialize(error, DefaultOptions);

        // Assert
        act.Should().NotThrow("Error exposes public properties that STJ can read");
    }

    [Fact]
    public void Error_Serialized_ContainsExpectedFields()
    {
        // Arrange
        var error = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act
        var json = JsonSerializer.Serialize(error, DefaultOptions);
        using var doc = JsonDocument.Parse(json);

        // Assert — the public contract is preserved in the serialized form
        doc.RootElement.GetProperty("code").GetString()
            .Should().Be(TestValues.Strings.ErrorCode);
        doc.RootElement.GetProperty("description").GetString()
            .Should().Be(TestValues.Strings.ErrorMessage);
        doc.RootElement.GetProperty("type").GetInt32()
            .Should().Be((int)ErrorType.Validation);
    }

    [Fact]
    public void Error_WithInnerErrors_Serialized_ContainsInnerErrorsArray()
    {
        // Arrange
        var inner = Error.Validation(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.AlternativeErrorMessage);
        var compound = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, inner);

        // Act
        var json = JsonSerializer.Serialize(compound, DefaultOptions);
        using var doc = JsonDocument.Parse(json);

        // Assert
        var innerErrors = doc.RootElement.GetProperty("innerErrors");
        innerErrors.GetArrayLength().Should().Be(1);
        innerErrors[0].GetProperty("code").GetString()
            .Should().Be(TestValues.Strings.AlternativeErrorCode);
    }

    [Fact]
    public void Error_CannotBeDeserialized_WithoutCustomConverter_DocumentsLimitation()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var json = JsonSerializer.Serialize(error, DefaultOptions);

        // Act — deserialization WILL fail because Error has a private constructor
        // This test DOCUMENTS the limitation: a custom JsonConverter<Error> is required
        // for round-trip deserialization. That converter must live in the infrastructure
        // or presentation layer, NOT in EricksonLopez.SharedKernel.
        var act = () => JsonSerializer.Deserialize<Error>(json, DefaultOptions);

        // Assert — document the expected failure mode
        act.Should().Throw<Exception>(
            "Error has a private constructor — STJ cannot instantiate it without a custom converter. " +
            "If round-trip JSON is needed, provide a JsonConverter<Error> in the infrastructure layer.");
    }

    // ── ErrorType enum serialization ──────────────────────────────────────────

    [Fact]
    public void ErrorType_CanBeSerializedToJson_AsInteger()
    {
        // Arrange
        var type = ErrorType.NotFound;

        // Act
        var json = JsonSerializer.Serialize(type, DefaultOptions);

        // Assert
        json.Should().Be(((int)ErrorType.NotFound).ToString());
    }
}
