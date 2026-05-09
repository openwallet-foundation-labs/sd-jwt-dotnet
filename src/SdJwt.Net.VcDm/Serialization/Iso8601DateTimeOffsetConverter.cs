using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SdJwt.Net.VcDm.Serialization;

/// <summary>
/// Serializes <see cref="Nullable{DateTimeOffset}"/> as an ISO 8601 string
/// (e.g., "2024-01-15T09:00:00Z") as required by W3C VCDM 2.0 for
/// <c>validFrom</c>, <c>validUntil</c>, and the deprecated <c>issuanceDate</c> / <c>expirationDate</c>.
/// </summary>
public sealed class Iso8601DateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
{
    private const string Format = "yyyy-MM-ddTHH:mm:sszzz";

    /// <inheritdoc/>
    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        var str = reader.GetString();
        if (str is null)
            return null;
        return DateTimeOffset.Parse(str, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Value.ToUniversalTime().ToString(Format, CultureInfo.InvariantCulture)
                .Replace("+00:00", "Z"));
    }
}
