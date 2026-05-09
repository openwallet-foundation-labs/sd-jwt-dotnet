using System.Text.Json;
using System.Text.Json.Serialization;

namespace SdJwt.Net.VcDm.Serialization;

/// <summary>
/// Handles W3C VCDM properties that may appear as either a single object or an array.
/// Reads both forms and normalizes to an array. Always writes an array.
/// </summary>
public sealed class SingleOrArrayConverter<T> : JsonConverter<T[]>
{
    /// <inheritdoc/>
    public override T[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Read each element individually to avoid re-entering this T[] converter.
            var list = new List<T>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                list.Add(JsonSerializer.Deserialize<T>(ref reader, options)!);
            return list.ToArray();
        }

        // Single object — wrap in array
        var single = JsonSerializer.Deserialize<T>(ref reader, options);
        return single is null ? null : [single];
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, T[] value, JsonSerializerOptions options)
    {
        // Write each element individually to avoid re-entering this T[] converter.
        writer.WriteStartArray();
        foreach (var item in value)
            JsonSerializer.Serialize(writer, item, typeof(T), options);
        writer.WriteEndArray();
    }
}
