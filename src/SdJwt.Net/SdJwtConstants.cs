using System.Text.Json;
using System.Text.Json.Serialization;

namespace SdJwt.Net;

/// <summary>
/// Defines constants used throughout the SD-JWT library.
/// </summary>
public static class SdJwtConstants
{
    public const string SdJwtTypeName = "sd+jwt";
    public const string DisclosureSeparator = "~";
    public const string DefaultHashAlgorithm = "sha-256";
    public const string SdAlgorithmClaim = "_sd_alg";
    public const string SdClaim = "_sd";
    public const string CnfClaim = "cnf";
    public const string JwkClaim = "jwk";
    public const string KbJwtHeaderType = "kb+jwt";
    public const string SdHashClaim = "sd_hash";

    /// <summary>
    /// Media type for SD-JWT content (RFC 9901 Section 11.2.1)
    /// </summary>
    public const string SdJwtMediaType = "application/sd-jwt";

    /// <summary>
    /// Media type for JWS JSON Serialized SD-JWT content (RFC 9901 Section 11.2.2)
    /// </summary>
    public const string SdJwtJsonMediaType = "application/sd-jwt+json";

    /// <summary>
    /// Media type for Key Binding JWT content (RFC 9901 Section 11.2.3)
    /// </summary>
    public const string KeyBindingJwtMediaType = "application/kb+jwt";

    /// <summary>
    /// Structured syntax suffix for SD-JWT (RFC 9901 Section 11.3)
    /// </summary>
    public const string SdJwtSuffix = "+sd-jwt";

    // SD-JWT VC Constants (draft-ietf-oauth-sd-jwt-vc-13)
    /// <summary>
    /// Type value for SD-JWT VC header (draft-ietf-oauth-sd-jwt-vc-13)
    /// </summary>
    public const string SdJwtVcTypeName = "dc+sd-jwt";

    /// <summary>
    /// Legacy type value for SD-JWT VC header (for transition period)
    /// Verifiers should accept both dc+sd-jwt and vc+sd-jwt
    /// </summary>
    public const string SdJwtVcLegacyTypeName = "vc+sd-jwt";

    /// <summary>
    /// Media type for SD-JWT VC content (draft-ietf-oauth-sd-jwt-vc-13)
    /// </summary>
    public const string SdJwtVcMediaType = "application/dc+sd-jwt";

    /// <summary>
    /// Verifiable credential type claim
    /// </summary>
    public const string VctClaim = "vct";

    /// <summary>
    /// Verifiable credential type integrity claim
    /// </summary>
    public const string VctIntegrityClaim = "vct#integrity";

    // Status List Constants (draft-ietf-oauth-status-list-13)
    /// <summary>
    /// Type value for Status List Token header
    /// </summary>
    public const string StatusListJwtTypeName = "statuslist+jwt";

    /// <summary>
    /// Media type for Status List Token in JWT format
    /// </summary>
    public const string StatusListJwtMediaType = "application/statuslist+jwt";

    /// <summary>
    /// Media type for Status List Token in CWT format
    /// </summary>
    public const string StatusListCwtMediaType = "application/statuslist+cwt";

    /// <summary>
    /// Status claim name
    /// </summary>
    public const string StatusClaim = "status";

    /// <summary>
    /// Status list claim name within status_list token payload
    /// </summary>
    public const string StatusListClaim = "status_list";

    /// <summary>
    /// Time to live claim name
    /// </summary>
    public const string TtlClaim = "ttl";

    /// <summary>
    /// Well-known URI for JWT VC Issuer Metadata
    /// </summary>
    public const string JwtVcIssuerWellKnownUri = "/.well-known/jwt-vc-issuer";

    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };
}