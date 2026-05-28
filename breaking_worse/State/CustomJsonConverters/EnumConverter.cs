using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace breaking_worse.State.CustomJsonConverters;

public class EnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var enumString = reader.GetString();
        if (string.IsNullOrEmpty(enumString) || !Enum.TryParse(enumString, out T result))
        {
            throw new JsonException($"Unable to convert \"{enumString}\" to Enum \"{typeof(T)}\".");
        }
        return result;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
