using System.Text.Json;
using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models.Dcql.Formats;

/// <summary>
/// Polymorphic JSON converter for <see cref="IDcqlMeta"/> that uses duck-typing on the
/// present JSON keys to select the concrete type:
/// <c>vct_values</c> → <see cref="SdJwtVcMeta"/>,
/// <c>doctype_value</c> → <see cref="MsoMdocMeta"/>,
/// <c>type_values</c> → <see cref="W3cVcMeta"/>.
/// </summary>
public class DcqlMetaConverter : JsonConverter<IDcqlMeta>
{
    /// <inheritdoc/>
    public override IDcqlMeta? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.TryGetProperty("vct_values", out _))
            return JsonSerializer.Deserialize<SdJwtVcMeta>(root.GetRawText(), options);

        if (root.TryGetProperty("doctype_value", out _))
            return JsonSerializer.Deserialize<MsoMdocMeta>(root.GetRawText(), options);

        if (root.TryGetProperty("type_values", out _))
            return JsonSerializer.Deserialize<W3cVcMeta>(root.GetRawText(), options);

        // Unknown meta format — return empty SdJwtVcMeta as fallback
        return new SdJwtVcMeta();
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IDcqlMeta value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
