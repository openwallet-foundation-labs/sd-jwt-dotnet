using System.Text.Json;
using System.Text.Json.Serialization;

namespace SdJwt.Net.VcDm.Serialization;

/// <summary>
/// Provides pre-configured <see cref="JsonSerializerOptions"/> for W3C VCDM 2.0 serialization.
/// </summary>
public static class VcDmSerializerOptions
{
    private static readonly JsonSerializerOptions _default = BuildDefault();

    /// <summary>
    /// Gets the default options: camelCase property names, write-indented off,
    /// null values omitted, all VCDM converters registered.
    /// </summary>
    public static JsonSerializerOptions Default => _default;

    private static JsonSerializerOptions BuildDefault()
    {
        var opts = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = null, // VCDM uses explicit [JsonPropertyName] on all fields
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        opts.Converters.Add(new IssuerJsonConverter());
        opts.Converters.Add(new CredentialStatusConverter());
        opts.Converters.Add(new Iso8601DateTimeOffsetConverter());
        opts.Converters.Add(new SingleOrArrayConverter<SdJwt.Net.VcDm.Models.CredentialSubject>());
        opts.Converters.Add(new SingleOrArrayConverter<SdJwt.Net.VcDm.Models.CredentialStatus>());
        opts.Converters.Add(new SingleOrArrayConverter<SdJwt.Net.VcDm.Models.CredentialSchema>());
        opts.Converters.Add(new SingleOrArrayConverter<SdJwt.Net.VcDm.Models.TermsOfUse>());
        opts.Converters.Add(new SingleOrArrayConverter<SdJwt.Net.VcDm.Models.Evidence>());
        opts.Converters.Add(new SingleOrArrayConverter<SdJwt.Net.VcDm.Models.RefreshService>());
        opts.Converters.Add(new SingleOrArrayConverter<SdJwt.Net.VcDm.Models.DataIntegrityProof>());
        opts.Converters.Add(new SingleOrArrayConverter<object>());

        return opts;
    }
}
