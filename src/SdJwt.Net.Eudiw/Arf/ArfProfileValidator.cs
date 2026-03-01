namespace SdJwt.Net.Eudiw.Arf;

/// <summary>
/// Validates credentials against EU Architecture Reference Framework (ARF) requirements.
/// </summary>
public class ArfProfileValidator
{
    private static readonly HashSet<string> SupportedAlgorithms = new(
        EudiwConstants.Algorithms.SupportedAlgorithms,
        StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> EuMemberStates = new(
        EudiwConstants.MemberStates.All,
        StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Validates a cryptographic algorithm against ARF requirements.
    /// </summary>
    /// <param name="algorithm">The algorithm to validate.</param>
    /// <returns>True if the algorithm is supported per ARF.</returns>
    public bool ValidateAlgorithm(string? algorithm)
    {
        if (string.IsNullOrWhiteSpace(algorithm))
        {
            return false;
        }

        return SupportedAlgorithms.Contains(algorithm);
    }

    /// <summary>
    /// Validates a credential type/DocType against ARF definitions.
    /// </summary>
    /// <param name="docTypeOrVct">The document type or verifiable credential type.</param>
    /// <returns>ARF validation result with identified credential type.</returns>
    public ArfValidationResult ValidateCredentialType(string? docTypeOrVct)
    {
        if (string.IsNullOrWhiteSpace(docTypeOrVct))
        {
            return ArfValidationResult.Invalid("Credential type cannot be empty");
        }

        // Check mdoc DocTypes
        if (docTypeOrVct == EudiwConstants.Pid.DocType)
        {
            return ArfValidationResult.Valid(ArfCredentialType.Pid);
        }

        if (docTypeOrVct == EudiwConstants.Mdl.DocType)
        {
            return ArfValidationResult.Valid(ArfCredentialType.Mdl);
        }

        // Check SD-JWT VC types
        if (docTypeOrVct.StartsWith(EudiwConstants.Qeaa.VctPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return ArfValidationResult.Valid(ArfCredentialType.Qeaa);
        }

        if (docTypeOrVct.StartsWith(EudiwConstants.Eaa.VctPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return ArfValidationResult.Valid(ArfCredentialType.Eaa);
        }

        return ArfValidationResult.Invalid($"Unknown credential type: {docTypeOrVct}");
    }

    /// <summary>
    /// Validates PID mandatory claims per ARF requirements.
    /// </summary>
    /// <param name="claims">The claims dictionary.</param>
    /// <returns>ARF validation result.</returns>
    public ArfValidationResult ValidatePidClaims(IDictionary<string, object>? claims)
    {
        if (claims == null)
        {
            return ArfValidationResult.Invalid("Claims cannot be null");
        }

        var missingClaims = new List<string>();
        foreach (var mandatoryClaim in EudiwConstants.Pid.MandatoryClaims)
        {
            if (!claims.ContainsKey(mandatoryClaim))
            {
                missingClaims.Add(mandatoryClaim);
            }
        }

        if (missingClaims.Count > 0)
        {
            return ArfValidationResult.Invalid(
                $"Missing mandatory PID claims: {string.Join(", ", missingClaims)}",
                missingClaims);
        }

        return ArfValidationResult.Valid(ArfCredentialType.Pid);
    }

    /// <summary>
    /// Validates credential validity period.
    /// </summary>
    /// <param name="issuanceDate">The issuance date.</param>
    /// <param name="expiryDate">The expiry date.</param>
    /// <returns>ARF validation result.</returns>
    public ArfValidationResult ValidateValidityPeriod(DateTimeOffset issuanceDate, DateTimeOffset expiryDate)
    {
        var now = DateTimeOffset.UtcNow;

        if (issuanceDate > now)
        {
            return ArfValidationResult.Invalid("Credential has future issuance date - not yet valid");
        }

        if (expiryDate < now)
        {
            return ArfValidationResult.Invalid("Credential has expired");
        }

        if (expiryDate <= issuanceDate)
        {
            return ArfValidationResult.Invalid("Expiry date must be after issuance date");
        }

        return ArfValidationResult.Valid(ArfCredentialType.Pid);
    }

    /// <summary>
    /// Validates if a country code is a valid EU member state.
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 country code.</param>
    /// <returns>True if the country is an EU member state.</returns>
    public bool ValidateMemberState(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return false;
        }

        return EuMemberStates.Contains(countryCode);
    }
}
