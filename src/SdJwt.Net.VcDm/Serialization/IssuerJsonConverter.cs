using System.Text.Json;
using System.Text.Json.Serialization;
using SdJwt.Net.VcDm.Models;

namespace SdJwt.Net.VcDm.Serialization;

/// <summary>
/// Handles polymorphic serialization of <see cref="Issuer"/>:
/// reads both a plain URL string and an object <c>{ "id": "...", "name": "..." }</c>.
/// Always writes a plain string when <see cref="Issuer.IsSimpleUrl"/> is true,
/// otherwise writes the full object.
/// </summary>
public sealed class IssuerJsonConverter : JsonConverter<Issuer>
{
    /// <inheritdoc/>
    public override Issuer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var url = reader.GetString()
                ?? throw new JsonException("Issuer string value must not be null.");
            return new Issuer(url);
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            string? id = null;
            string? name = null;
            string? description = null;
            var extra = new Dictionary<string, object>();

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName) continue;
                var propName = reader.GetString()!;
                reader.Read();

                switch (propName)
                {
                    case "id":
                        id = reader.GetString();
                        break;
                    case "name":
                        name = reader.GetString();
                        break;
                    case "description":
                        description = reader.GetString();
                        break;
                    default:
                        extra[propName] = ReadValue(ref reader, options);
                        break;
                }
            }

            if (id is null)
                throw new JsonException("Issuer object must contain an 'id' property.");

            return new Issuer(id)
            {
                Name = name,
                Description = description,
                AdditionalProperties = extra.Count > 0 ? extra : null
            };
        }

        throw new JsonException($"Unexpected token type {reader.TokenType} for Issuer.");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Issuer value, JsonSerializerOptions options)
    {
        if (value.IsSimpleUrl)
        {
            writer.WriteStringValue(value.Id);
            return;
        }

        writer.WriteStartObject();
        writer.WriteString("id", value.Id);
        if (value.Name is not null) writer.WriteString("name", value.Name);
        if (value.Description is not null) writer.WriteString("description", value.Description);
        if (value.AdditionalProperties is not null)
        {
            foreach (var (k, v) in value.AdditionalProperties)
            {
                writer.WritePropertyName(k);
                JsonSerializer.Serialize(writer, v, options);
            }
        }
        writer.WriteEndObject();
    }

    private static object ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<object>(ref reader, options) ?? throw new JsonException("Null value.");
}
