using System.Collections.Immutable;

namespace Owf.Sd.Jwt;
/// <summary>
/// A class that contains constants used in the SD-JWT specification.
/// </summary>
public static class Constants
{
    /// <summary>
    /// The default hash algorithm used in SD-JWT.
    /// </summary>
    public const string DEFAULT_HASH_ALGORITHM = "sha-256";

    /// <summary>
    /// The "_sd" key reserved by the SD-JWT specification.
    /// </summary>
    public const string KEY_SD = "_sd";

    /// <summary>
    /// The "_sd_alg" key reserved by the SD-JWT specification.
    /// </summary>
    public const string KEY_SD_ALG = "_sd_alg";

    /// <summary>
    /// The "_sd_jwt" key reserved by the SD-JWT specification.
    /// </summary>
    public const string KEY_SD_JWT = "_sd_jwt";

    /// <summary>
    /// The "..." key reserved by the SD-JWT specification.
    /// </summary>
    public const string KEY_THREE_DOTS = "...";

    /// <summary>
    /// Keys reserved by the SD-JWT specification.
    /// </summary>
    public static readonly ImmutableHashSet<string> RESERVED_KEYS = ImmutableHashSet.Create
    (
        KEY_SD,
        KEY_SD_ALG,
        KEY_SD_JWT,
        KEY_THREE_DOTS
    );

    /// <summary>
    /// Claims that are not selectively-disclosable according to the SD-JWT specification.
    /// </summary>
    public static readonly ImmutableHashSet<string> RETAINED_CLAIMS = ImmutableHashSet.Create
    (
        "iss",
        "iat",
        "nbf",
        "exp",
        "cnf",
        "type",
        "status"
    );
}