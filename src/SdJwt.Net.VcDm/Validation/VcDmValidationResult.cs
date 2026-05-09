namespace SdJwt.Net.VcDm.Validation;

/// <summary>
/// Result of a W3C VCDM 2.0 structural validation.
/// </summary>
public sealed class VcDmValidationResult
{
    private VcDmValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    /// <summary>True when all required fields pass validation.</summary>
    public bool IsValid
    {
        get;
    }

    /// <summary>Human-readable error messages when <see cref="IsValid"/> is false.</summary>
    public IReadOnlyList<string> Errors
    {
        get;
    }

    internal static VcDmValidationResult Success() =>
        new(true, Array.Empty<string>());

    internal static VcDmValidationResult Failure(IEnumerable<string> errors) =>
        new(false, errors.ToList().AsReadOnly());
}
