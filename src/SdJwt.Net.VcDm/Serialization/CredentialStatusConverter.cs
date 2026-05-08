using System.Text.Json;
using System.Text.Json.Serialization;
using SdJwt.Net.VcDm.Models;

namespace SdJwt.Net.VcDm.Serialization;

/// <summary>
/// Deserializes <see cref="CredentialStatus"/> objects by reading the <c>type</c>
/// discriminator first, then mapping to the correct subclass:
/// <c>BitstringStatusListEntry</c>, <c>StatusList2021Entry</c> (legacy),
/// or <see cref="UnknownCredentialStatus"/> for unrecognized types.
/// </summary>
public sealed class CredentialStatusConverter : JsonConverter<CredentialStatus>
{
    /// <inheritdoc/>
    public override CredentialStatus? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of object for credentialStatus.");

        // Buffer the full object so we can read type first
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var type = root.TryGetProperty("type", out var typeProp)
            ? typeProp.GetString()
            : null;

        return type switch
        {
            "BitstringStatusListEntry" => JsonSerializer.Deserialize<BitstringStatusListEntry>(root.GetRawText(), options),
            "StatusList2021Entry" =>
#pragma warning disable CS0618
                JsonSerializer.Deserialize<StatusList2021Entry>(root.GetRawText(), options),
#pragma warning restore CS0618
            _ => DeserializeUnknown(root, type ?? "unknown", options)
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CredentialStatus value, JsonSerializerOptions options)
    {
        // Serialize via the concrete runtime type so the abstract [JsonConverter] attribute
        // on CredentialStatus is bypassed and the subclass properties are written directly.
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

    private static UnknownCredentialStatus DeserializeUnknown(
        JsonElement root, string type, JsonSerializerOptions options)
    {
        var status = new UnknownCredentialStatus(type);
        var extra = new Dictionary<string, object>();

        foreach (var prop in root.EnumerateObject())
        {
            if (prop.Name is "id" or "type") continue;
            extra[prop.Name] = JsonSerializer.Deserialize<object>(prop.Value.GetRawText(), options)!;
        }

        if (root.TryGetProperty("id", out var idProp))
            status.Id = idProp.GetString();

        status.AdditionalProperties = extra.Count > 0 ? extra : null;
        return status;
    }
}
