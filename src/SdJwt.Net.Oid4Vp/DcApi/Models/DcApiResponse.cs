using System.Text.Json.Serialization;
using SdJwt.Net.Oid4Vp.Models;

namespace SdJwt.Net.Oid4Vp.DcApi.Models;

/// <summary>
/// Represents a response from the Digital Credentials API.
/// </summary>
public class DcApiResponse
{
    /// <summary>
    /// Protocol identifier from the response.
    /// </summary>
    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = string.Empty;

    /// <summary>
    /// Browser origin of the wallet that provided the response.
    /// </summary>
    [JsonPropertyName("origin")]
    public string Origin { get; set; } = string.Empty;

    /// <summary>
    /// The VP token containing the presentation.
    /// </summary>
    [JsonPropertyName("vp_token")]
    public string VpToken { get; set; } = string.Empty;

    /// <summary>
    /// Presentation submission describing the structure of the VP token.
    /// </summary>
    [JsonPropertyName("presentation_submission")]
    public PresentationSubmission? PresentationSubmission
    {
        get; set;
    }

    /// <summary>
    /// Nonce echoed from the request.
    /// </summary>
    [JsonPropertyName("nonce")]
    public string? Nonce
    {
        get; set;
    }

    /// <summary>
    /// Timestamp when the presentation was created.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset? IssuedAt
    {
        get; set;
    }
}

/// <summary>
/// Validation options for DC API responses.
/// </summary>
public class DcApiValidationOptions
{
    /// <summary>
    /// Expected browser origin for origin validation.
    /// Must match the client_id when using web-origin scheme.
    /// </summary>
    public string ExpectedOrigin { get; set; } = string.Empty;

    /// <summary>
    /// Expected nonce value for replay protection.
    /// </summary>
    public string ExpectedNonce { get; set; } = string.Empty;

    /// <summary>
    /// Maximum age allowed for the presentation.
    /// </summary>
    public TimeSpan MaxAge { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// JWK for decrypting dc_api.jwt responses.
    /// Required when expecting encrypted responses.
    /// </summary>
    public object? DecryptionKey
    {
        get; set;
    }

    /// <summary>
    /// Whether to validate the browser origin strictly.
    /// </summary>
    public bool ValidateOrigin { get; set; } = true;

    /// <summary>
    /// Clock skew tolerance for time-based validations.
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);
}

/// <summary>
/// Represents a verified credential from a DC API response.
/// </summary>
public class VerifiedCredential
{
    /// <summary>
    /// The credential type (e.g., "IdentityCredential").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The verified claims from the credential.
    /// </summary>
    public IDictionary<string, object> Claims { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// The issuer of the credential.
    /// </summary>
    public string? Issuer
    {
        get; set;
    }

    /// <summary>
    /// The subject of the credential.
    /// </summary>
    public string? Subject
    {
        get; set;
    }
}

/// <summary>
/// Result of DC API response validation.
/// </summary>
public class DcApiValidationResult
{
    /// <summary>
    /// Indicates whether the response is valid.
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// Error message if validation failed.
    /// </summary>
    public string? Error
    {
        get; set;
    }

    /// <summary>
    /// Error code if validation failed.
    /// </summary>
    public string? ErrorCode
    {
        get; set;
    }

    /// <summary>
    /// The verified credentials from the response.
    /// </summary>
    public IReadOnlyList<VerifiedCredential> VerifiedCredentials { get; set; } = Array.Empty<VerifiedCredential>();

    /// <summary>
    /// The presentation submission describing the credentials.
    /// </summary>
    public PresentationSubmission? PresentationSubmission
    {
        get; set;
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="credentials">The verified credentials.</param>
    /// <param name="submission">The presentation submission.</param>
    /// <returns>A successful validation result.</returns>
    public static DcApiValidationResult Success(
        IReadOnlyList<VerifiedCredential> credentials,
        PresentationSubmission? submission = null)
    {
        return new DcApiValidationResult
        {
            IsValid = true,
            VerifiedCredentials = credentials,
            PresentationSubmission = submission
        };
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="error">Error message.</param>
    /// <param name="errorCode">Error code.</param>
    /// <returns>A failed validation result.</returns>
    public static DcApiValidationResult Failure(string error, string errorCode)
    {
        return new DcApiValidationResult
        {
            IsValid = false,
            Error = error,
            ErrorCode = errorCode,
            VerifiedCredentials = Array.Empty<VerifiedCredential>()
        };
    }
}
