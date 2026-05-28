using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using breaking_worse.Input;
using breaking_worse.Input.Enums;
using breaking_worse.input.KeyTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace breaking_worse.State.CustomJsonConverters;

public class KeyConverter : JsonConverter<IKey>
{
    public override IKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonObject = JsonDocument.ParseValue(ref reader).RootElement;
        var typeProperty = jsonObject.GetProperty("Type").GetString(); 
        return typeProperty switch
        {
            "KeyboardKey" => new KeyboardKey(jsonObject.GetProperty("Key").Deserialize<Keys>(options)),
            "GamePadButton" => new GamePadButton(jsonObject.GetProperty("Button").Deserialize<Buttons>(options), jsonObject.GetProperty("PlayerIndex").Deserialize<PlayerIndex>(options)),
            "MouseButton" => new MouseButton(jsonObject.GetProperty("Button").Deserialize<MouseButtons>(options)),
            _ => throw new NotSupportedException($"Type '{typeProperty}' is not supported")
        };
    }

    public override void Write(Utf8JsonWriter writer, IKey value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Type", value.GetType().Name);
        foreach (var property in value.GetType().GetProperties())
        {
            writer.WritePropertyName(property.Name);
            JsonSerializer.Serialize(writer, property.GetValue(value), property.PropertyType, options);
        }
        writer.WriteEndObject();
    }
}
