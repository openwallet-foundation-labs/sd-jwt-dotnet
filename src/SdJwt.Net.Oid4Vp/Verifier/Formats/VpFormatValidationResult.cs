namespace SdJwt.Net.Oid4Vp.Verifier.Formats;

/// <summary>
/// Result returned by an OID4VP format-specific presentation validator.
/// </summary>
public sealed class VpFormatValidationResult
{
    /// <summary>
    /// Gets or sets whether the presentation is valid.
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the validation error, if any.
    /// </summary>
    public string? Error
    {
        get; set;
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static VpFormatValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="error">The validation error.</param>
    /// <returns>A failed result.</returns>
    public static VpFormatValidationResult Failed(string error) => new()
    {
        IsValid = false,
        Error = error
    };
}
