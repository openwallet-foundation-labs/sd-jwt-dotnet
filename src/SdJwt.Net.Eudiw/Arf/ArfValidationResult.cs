namespace SdJwt.Net.Eudiw.Arf;

/// <summary>
/// Result of ARF profile validation.
/// </summary>
public class ArfValidationResult
{
    /// <summary>
    /// Indicates whether the validation succeeded.
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// The identified credential type, if validation succeeded.
    /// </summary>
    public ArfCredentialType? CredentialType
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
    /// List of missing mandatory claims, if applicable.
    /// </summary>
    public IReadOnlyList<string> MissingClaims { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Creates a valid result with the identified credential type.
    /// </summary>
    /// <param name="credentialType">The credential type.</param>
    /// <returns>A valid ARF validation result.</returns>
    public static ArfValidationResult Valid(ArfCredentialType credentialType)
    {
        return new ArfValidationResult
        {
            IsValid = true,
            CredentialType = credentialType
        };
    }

    /// <summary>
    /// Creates an invalid result with an error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="missingClaims">Optional list of missing claims.</param>
    /// <returns>An invalid ARF validation result.</returns>
    public static ArfValidationResult Invalid(string error, IEnumerable<string>? missingClaims = null)
    {
        return new ArfValidationResult
        {
            IsValid = false,
            Error = error,
            MissingClaims = missingClaims?.ToArray() ?? Array.Empty<string>()
        };
    }
}
