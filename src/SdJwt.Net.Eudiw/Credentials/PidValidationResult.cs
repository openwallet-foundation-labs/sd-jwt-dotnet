namespace SdJwt.Net.Eudiw.Credentials;

/// <summary>
/// Result of PID credential validation.
/// </summary>
public class PidValidationResult
{
    /// <summary>
    /// Indicates whether validation succeeded.
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// List of validation errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful PID validation result.</returns>
    public static PidValidationResult Success()
    {
        return new PidValidationResult { IsValid = true };
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <returns>A failed PID validation result.</returns>
    public static PidValidationResult Failure(params string[] errors)
    {
        return new PidValidationResult { IsValid = false, Errors = errors };
    }
}
