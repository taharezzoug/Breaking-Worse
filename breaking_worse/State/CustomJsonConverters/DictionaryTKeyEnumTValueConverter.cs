using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace breaking_worse.State.CustomJsonConverters;

public class DictionaryTKeyEnumTValueConverter : JsonConverterFactory
{
    
    /// <summary>
    /// From the C# Documentation: enables JSON Serialization of Enum as Key in Dictionary
    /// </summary>
    
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }
        
        if (typeToConvert.GetGenericTypeDefinition() != typeof(Dictionary<,>))
        {
            return false;
        }

        return typeToConvert.GetGenericArguments()[0].IsEnum;
    }

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        Type[] typeArguments = type.GetGenericArguments();
        Type keyType = typeArguments[0];
        Type valueType = typeArguments[1];
        
        JsonConverter converter = (JsonConverter)Activator.CreateInstance(typeof(DictionaryEnumConverterInner<,>).MakeGenericType(keyType, valueType),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: [options],
            culture: null);
        return converter;
    }

    private class DictionaryEnumConverterInner<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>>
        where TKey : struct, Enum
    {
        private readonly JsonConverter<TValue> _valueConverter;
        private readonly Type _keyType;
        private readonly Type _valueType;

        public DictionaryEnumConverterInner(JsonSerializerOptions options)
        {
            _valueConverter = (JsonConverter<TValue>)options.GetConverter(typeof(TValue));
            _keyType = typeof(TKey);
            _valueType = typeof(TValue);
        }

        public override Dictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();
            var dictionary = new Dictionary<TKey, TValue>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject) return dictionary;

                if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();

                string propertyName = reader.GetString();

                if (!Enum.TryParse(propertyName, false, out TKey key) &&
                    !Enum.TryParse(propertyName, true, out key))
                {
                    throw new JsonException($"Unable To Convert: {propertyName} to Enum ");
                }

                reader.Read();
                TValue value = _valueConverter.Read(ref reader, _keyType, options)!;

                dictionary.Add(key, value);
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<TKey, TValue> dictionary,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach ((TKey key, TValue value) in dictionary)
            {
                string propertyName = key.ToString();
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);
                _valueConverter.Write(writer, value, options);
            }

            writer.WriteEndObject();
        }
    }
}