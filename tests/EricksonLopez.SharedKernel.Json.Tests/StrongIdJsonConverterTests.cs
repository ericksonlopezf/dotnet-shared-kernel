// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;

namespace EricksonLopez.SharedKernel.Json.Tests;

using System.IO;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using EricksonLopez.DomainPrimitives;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.Json;
using EricksonLopez.SharedKernel.Json.Tests.Fakes;
using EricksonLopez.SharedKernel.TestingUtilities.Fakes;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

public class StrongIdJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public StrongIdJsonConverterTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new StrongIdJsonConverterFactory());
    }

    #region Direct StrongIdJsonConverter Tests

    [Fact]
    public void Converter_Write_SerializesUnderlyingPrimitiveValue()
    {
        var converter = new StrongIdJsonConverter<OrderId, Guid>();
        var id = OrderId.Create(Guid.NewGuid());

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        converter.Write(writer, id, new JsonSerializerOptions());
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());
        json.Should().Be($"\"{id.Value}\"");
    }

    [Fact]
    public void Converter_Write_WhenWriterIsNull_ThrowsArgumentNullException()
    {
        var converter = new StrongIdJsonConverter<OrderId, Guid>();
        var id = OrderId.Create(Guid.NewGuid());

        var act = () => converter.Write(null!, id, new JsonSerializerOptions());

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("writer");
    }

    [Fact]
    public void Converter_Write_WithVariousPrimitiveStrongIds_SerializesCorrectly()
    {
        // Int-backed
        var intConverter = new StrongIdJsonConverter<DepartmentId, int>();
        var deptId = DepartmentId.Create(42);
        using (var stream = new MemoryStream())
        using (var writer = new Utf8JsonWriter(stream))
        {
            intConverter.Write(writer, deptId, new JsonSerializerOptions());
            writer.Flush();
            Encoding.UTF8.GetString(stream.ToArray()).Should().Be("42");
        }

        // String-backed
        var strConverter = new StrongIdJsonConverter<ProductCode, string>();
        var prodCode = ProductCode.Create("SKU-999");
        using (var stream = new MemoryStream())
        using (var writer = new Utf8JsonWriter(stream))
        {
            strConverter.Write(writer, prodCode, new JsonSerializerOptions());
            writer.Flush();
            Encoding.UTF8.GetString(stream.ToArray()).Should().Be("\"SKU-999\"");
        }
    }

    [Fact]
    public void Converter_Read_ValidPrimitive_ReturnsStrongId()
    {
        var guid = Guid.NewGuid();
        var json = $"\"{guid}\"";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();

        var converter = new StrongIdJsonConverter<OrderId, Guid>();
        var result = converter.Read(ref reader, typeof(OrderId), new JsonSerializerOptions());

        result.Value.Should().Be(guid);
    }

    [Fact]
    public void Converter_Read_NullToken_ThrowsJsonException()
    {
        var converter = new StrongIdJsonConverter<OrderId, Guid>();
        var json = "null";

        var act = () =>
        {
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read();
            converter.Read(ref reader, typeof(OrderId), new JsonSerializerOptions());
        };

        act.Should().Throw<JsonException>()
            .WithMessage($"*Null is not a valid value for strong identifier '{typeof(OrderId).FullName}'.*");
    }

    [Fact]
    public void Converter_Read_WhenCreateThrowsArgumentException_ThrowsJsonException()
    {
        var converter = new StrongIdJsonConverter<OrderId, Guid>();
        var json = $"\"{Guid.Empty}\"";

        var act = () =>
        {
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read();
            converter.Read(ref reader, typeof(OrderId), new JsonSerializerOptions());
        };

        var ex = act.Should().Throw<JsonException>()
            .WithMessage($"*The JSON value is invalid for strong identifier '{typeof(OrderId).FullName}'.*")
            .Which;

        ex.InnerException.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public void Converter_Read_WhenCreateThrowsFormatException_ThrowsJsonException()
    {
        var converter = new StrongIdJsonConverter<ProductCode, string>();
        var json = "\"FORMAT_ERROR\"";

        var act = () =>
        {
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read();
            converter.Read(ref reader, typeof(ProductCode), new JsonSerializerOptions());
        };

        var ex = act.Should().Throw<JsonException>()
            .WithMessage($"*The JSON value is invalid for strong identifier '{typeof(ProductCode).FullName}'.*")
            .Which;

        ex.InnerException.Should().BeOfType<FormatException>();
    }

    [Fact]
    public void Converter_Read_WhenCreateThrowsOverflowException_ThrowsJsonException()
    {
        var converter = new StrongIdJsonConverter<NumericRangeId, int>();
        var json = "999";

        var act = () =>
        {
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read();
            converter.Read(ref reader, typeof(NumericRangeId), new JsonSerializerOptions());
        };

        var ex = act.Should().Throw<JsonException>()
            .WithMessage($"*The JSON value is invalid for strong identifier '{typeof(NumericRangeId).FullName}'.*")
            .Which;

        ex.InnerException.Should().BeOfType<OverflowException>();
    }

    [Fact]
    public void Converter_Read_WhenPrimitiveDeserializesAsNull_ThrowsJsonException()
    {
        var converter = new StrongIdJsonConverter<ProductCode, string>();
        var options = new JsonSerializerOptions();
        options.Converters.Add(new NullReturningStringConverter());

        var json = "\"SOME_STRING\"";

        var act = () =>
        {
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read();
            converter.Read(ref reader, typeof(ProductCode), options);
        };

        act.Should().Throw<JsonException>()
            .WithMessage($"*The underlying primitive value for strong identifier '{typeof(ProductCode).FullName}' cannot be null.*");
    }

    #endregion

    #region StrongIdJsonConverterFactory Tests

    [Fact]
    public void Factory_CanConvert_WithNullType_ThrowsArgumentNullException()
    {
        var factory = new StrongIdJsonConverterFactory();

        var act = () => factory.CanConvert(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("typeToConvert");
    }

    [Fact]
    public void Factory_CanConvert_WithStrongIdType_ReturnsTrue()
    {
        var factory = new StrongIdJsonConverterFactory();

        factory.CanConvert(typeof(OrderId)).Should().BeTrue();
        factory.CanConvert(typeof(ProductCode)).Should().BeTrue();
        factory.CanConvert(typeof(Quantity)).Should().BeTrue();
    }

    [Fact]
    public void Factory_CanConvert_WithNonStrongIdType_ReturnsFalse()
    {
        var factory = new StrongIdJsonConverterFactory();

        factory.CanConvert(typeof(NonStrongIdType)).Should().BeFalse();
        factory.CanConvert(typeof(string)).Should().BeFalse();
        factory.CanConvert(typeof(int)).Should().BeFalse();
    }

    [Fact]
    public void Factory_CreateConverter_WithNullType_ThrowsArgumentNullException()
    {
        var factory = new StrongIdJsonConverterFactory();

        var act = () => factory.CreateConverter(null!, new JsonSerializerOptions());

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("typeToConvert");
    }

    [Fact]
    public void Factory_CreateConverter_WithNullOptions_ThrowsArgumentNullException()
    {
        var factory = new StrongIdJsonConverterFactory();

        var act = () => factory.CreateConverter(typeof(OrderId), null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Factory_CreateConverter_WithNonStrongIdType_ThrowsInvalidOperationException()
    {
        var factory = new StrongIdJsonConverterFactory();

        var act = () => factory.CreateConverter(typeof(NonStrongIdType), new JsonSerializerOptions());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*Type '{typeof(NonStrongIdType).FullName}' does not implement '{typeof(IStrongId<,>).FullName}'.*");
    }

    [Fact]
    public void Factory_CreateConverter_ReturnsCachedConverterInstance()
    {
        var factory = new StrongIdJsonConverterFactory();
        var options = new JsonSerializerOptions();

        var converter1 = factory.CreateConverter(typeof(OrderId), options);
        var converter2 = factory.CreateConverter(typeof(OrderId), options);

        converter1.Should().NotBeNull();
        converter1.Should().BeOfType<StrongIdJsonConverter<OrderId, Guid>>();
        converter1.Should().BeSameAs(converter2);
    }

    #endregion

    #region Full JsonSerializer Integration Tests

    [Fact]
    public void JsonSerializer_SerializesAndDeserializes_ComplexDtoWithStrongIds()
    {
        var dto = new OrderDto
        {
            Id = OrderId.Create(Guid.NewGuid()),
            Code = ProductCode.Create("SKU-1234"),
            Quantity = Quantity.Create(42)
        };

        var json = JsonSerializer.Serialize(dto, _options);
        json.Should().Contain(dto.Id.Value.ToString());
        json.Should().Contain("SKU-1234");
        json.Should().Contain("42");

        var deserialized = JsonSerializer.Deserialize<OrderDto>(json, _options);
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(dto.Id);
        deserialized.Code.Should().Be(dto.Code);
        deserialized.Quantity.Should().Be(dto.Quantity);
    }

    [Fact]
    public void JsonSerializer_DeserializingNullStrongIdProperty_ThrowsJsonException()
    {
        var json = "{\"Id\":null,\"Code\":\"SKU-1234\",\"Quantity\":42}";

        var act = () => JsonSerializer.Deserialize<OrderDto>(json, _options);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void JsonSerializer_DeserializingInvalidStrongIdValue_ThrowsJsonException()
    {
        var json = $"{{\"Id\":\"{Guid.NewGuid()}\",\"Code\":\"\",\"Quantity\":42}}";

        var act = () => JsonSerializer.Deserialize<OrderDto>(json, _options);

        act.Should().Throw<JsonException>();
    }

    #endregion

    #region Property-Based Tests (FsCheck)

    [Property]
    public Property JsonRoundtrip_PreservesGuidStrongId(Guid idValue)
    {
        // Discard invalid domain generator values (Guid.Empty) via FsCheck precondition filtering
        if (idValue == Guid.Empty)
            return false.When(false);

        var id = OrderId.Create(idValue);
        var json = JsonSerializer.Serialize(id, _options);
        var deserialized = JsonSerializer.Deserialize<OrderId>(json, _options);

        return (deserialized == id && deserialized.Value == idValue).When(idValue != Guid.Empty);
    }

    [Property]
    public Property JsonRoundtrip_PreservesIntStrongId(PositiveInt positiveInt)
    {
        var id = Quantity.Create(positiveInt.Get);
        var json = JsonSerializer.Serialize(id, _options);
        var deserialized = JsonSerializer.Deserialize<Quantity>(json, _options);

        return (deserialized == id && deserialized.Value == positiveInt.Get).When(positiveInt.Get >= 0);
    }

    [Property]
    public Property JsonRoundtrip_PreservesStringStrongId(NonNull<string> nonNullString)
    {
        var raw = nonNullString.Get;
        // Discard whitespace and synthetic error token strings via FsCheck precondition filtering
        if (string.IsNullOrWhiteSpace(raw) || raw == "FORMAT_ERR" || raw == "FORMAT_ERROR")
            return false.When(false);

        var id = ProductCode.Create(raw);
        var json = JsonSerializer.Serialize(id, _options);
        var deserialized = JsonSerializer.Deserialize<ProductCode>(json, _options);

        return (deserialized == id && deserialized.Value == raw).ToProperty();
    }

    #endregion
}




