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

    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };
}