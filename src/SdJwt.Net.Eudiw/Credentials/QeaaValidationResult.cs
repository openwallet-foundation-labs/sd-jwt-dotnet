namespace SdJwt.Net.Eudiw.Credentials;

/// <summary>
/// Result of QEAA credential validation.
/// </summary>
public class QeaaValidationResult
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
    /// <returns>A successful QEAA validation result.</returns>
    public static QeaaValidationResult Success()
    {
        return new QeaaValidationResult { IsValid = true };
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <returns>A failed QEAA validation result.</returns>
    public static QeaaValidationResult Failure(params string[] errors)
    {
        return new QeaaValidationResult { IsValid = false, Errors = errors };
    }
}
