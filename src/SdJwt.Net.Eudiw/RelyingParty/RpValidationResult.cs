namespace SdJwt.Net.Eudiw.RelyingParty;

/// <summary>
/// Result of RP registration validation.
/// </summary>
public class RpValidationResult
{
    /// <summary>
    /// Indicates whether validation succeeded.
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
    /// Validation warnings (non-blocking issues).
    /// </summary>
    public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful RP validation result.</returns>
    public static RpValidationResult Success()
    {
        return new RpValidationResult { IsValid = true };
    }

    /// <summary>
    /// Creates a successful validation result with warnings.
    /// </summary>
    /// <param name="warnings">The warnings.</param>
    /// <returns>A successful RP validation result with warnings.</returns>
    public static RpValidationResult SuccessWithWarnings(params string[] warnings)
    {
        return new RpValidationResult { IsValid = true, Warnings = warnings };
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed RP validation result.</returns>
    public static RpValidationResult Failure(string error)
    {
        return new RpValidationResult { IsValid = false, Error = error };
    }
}
