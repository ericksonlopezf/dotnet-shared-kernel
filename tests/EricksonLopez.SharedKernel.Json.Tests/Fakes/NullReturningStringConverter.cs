// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.SharedKernel.Json.Tests.Fakes;

using System.Text.Json;
using System.Text.Json.Serialization;

public class NullReturningStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => null;
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) => writer.WriteStringValue(value);
}


