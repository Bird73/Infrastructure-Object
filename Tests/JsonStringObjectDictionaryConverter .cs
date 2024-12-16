namespace Birdsoft.Utilities.LogManager.Tests;

using System.Text.Json;
using System.Text.Json.Serialization;

public class JsonStringObjectDictionaryConverter : JsonConverter<Dictionary<string, object>?>
{
    public override Dictionary<string, object>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var dictionary = new Dictionary<string, object>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                var key = reader.GetString();
                reader.Read();
                var value = JsonSerializer.Deserialize<object>(ref reader, options);
                if (key != null && value != null)
                {
                    dictionary[key] = value;
                }
            }
            return dictionary;
        }
        return null;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, object>? value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value != null)
        {
            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key);
                JsonSerializer.Serialize(writer, kvp.Value, options);
            }
        }
        writer.WriteEndObject();
    }
}
