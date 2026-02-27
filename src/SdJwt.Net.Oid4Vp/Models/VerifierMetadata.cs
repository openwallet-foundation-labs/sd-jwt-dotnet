using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models;

/// <summary>
/// Represents verifier metadata as defined in OID4VP 1.0 Section 11.
/// This object is used by verifiers to advertise their supported credential formats
/// and encryption preferences to wallets.
/// </summary>
public class VerifierMetadata
{
    /// <summary>
    /// Gets or sets the VP formats supported by the verifier.
    /// REQUIRED. A map of format identifiers (e.g., <c>"vc+sd-jwt"</c>) to format-specific
    /// algorithm requirements.
    /// </summary>
    [JsonPropertyName("vp_formats_supported")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Dictionary<string, object>? VpFormatsSupported
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the JWE algorithm the verifier expects for encrypted Authorization Responses.
    /// OPTIONAL. Must be a valid JWA identifier (e.g., <c>"ECDH-ES"</c>).
    /// </summary>
    [JsonPropertyName("authorization_encrypted_response_alg")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? AuthorizationEncryptedResponseAlg
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the JWE content encryption algorithm expected for encrypted Authorization Responses.
    /// OPTIONAL. Must be a valid JWA identifier (e.g., <c>"A256GCM"</c>).
    /// </summary>
    [JsonPropertyName("authorization_encrypted_response_enc")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? AuthorizationEncryptedResponseEnc
    {
        get; set;
    }

    /// <summary>
    /// Creates a <see cref="VerifierMetadata"/> instance suitable for verifiers that accept
    /// SD-JWT VC credentials in <c>vc+sd-jwt</c> format.
    /// </summary>
    /// <returns>A pre-configured <see cref="VerifierMetadata"/> for SD-JWT VC.</returns>
    public static VerifierMetadata CreateForSdJwtVc()
    {
        return new VerifierMetadata
        {
            VpFormatsSupported = new Dictionary<string, object>
            {
                ["vc+sd-jwt"] = new Dictionary<string, object>
                {
                    ["sd-jwt_alg_values"] = new[] { "ES256", "ES384", "PS256" },
                    ["kb-jwt_alg_values"] = new[] { "ES256", "ES384", "PS256" }
                }
            }
        };
    }
}
