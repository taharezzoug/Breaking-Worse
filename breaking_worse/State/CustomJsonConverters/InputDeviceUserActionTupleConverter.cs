using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using breaking_worse.Input.Enums;

namespace breaking_worse.State.CustomJsonConverters;

public class InputDeviceUserActionTupleConverter : JsonConverter<(InputDevice, UserAction)>
{
    public override (InputDevice, UserAction) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, (InputDevice, UserAction) value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override (InputDevice, UserAction) ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var keyString = reader.GetString();
        var parts = keyString.Split(',');
        var InputDevice = (InputDevice)Enum.Parse(typeof(InputDevice), parts[0]);
        var userAction = (UserAction)Enum.Parse(typeof(UserAction), parts[1]);
        return (InputDevice, userAction);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, (InputDevice, UserAction) value, JsonSerializerOptions options)
    {
        writer.WritePropertyName($"{value.Item1},{value.Item2}");
    }
}
