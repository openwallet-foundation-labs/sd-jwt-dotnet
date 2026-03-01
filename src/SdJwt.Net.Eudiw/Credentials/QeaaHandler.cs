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

/// <summary>
/// Handles QEAA (Qualified Electronic Attestation of Attributes) credential processing.
/// </summary>
public class QeaaHandler
{
    private static readonly string[] MandatoryClaims = new[] { "iss", "iat", "exp", "vct" };

    /// <summary>
    /// Gets whether QEAA requires a qualified issuer.
    /// </summary>
    public bool RequiresQualifiedIssuer => true;

    /// <summary>
    /// Determines if a VCT indicates a QEAA credential.
    /// </summary>
    /// <param name="vct">The verifiable credential type.</param>
    /// <returns>True if the VCT indicates a QEAA credential.</returns>
    public bool IsQeaaCredential(string? vct)
    {
        if (string.IsNullOrWhiteSpace(vct))
        {
            return false;
        }

        return vct.StartsWith(EudiwConstants.Qeaa.VctPrefix, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates QEAA credential claims.
    /// </summary>
    /// <param name="claims">The QEAA claims.</param>
    /// <returns>Validation result.</returns>
    public QeaaValidationResult Validate(IDictionary<string, object>? claims)
    {
        if (claims == null)
        {
            return QeaaValidationResult.Failure("Claims cannot be null");
        }

        var errors = new List<string>();

        // Check mandatory claims
        foreach (var mandatory in MandatoryClaims)
        {
            if (!claims.ContainsKey(mandatory))
            {
                errors.Add($"Missing mandatory claim: {mandatory}");
            }
        }

        // Check expiry
        if (claims.TryGetValue("exp", out var exp))
        {
            long expTimestamp;
            if (exp is long l)
            {
                expTimestamp = l;
            }
            else if (long.TryParse(exp?.ToString(), out var parsed))
            {
                expTimestamp = parsed;
            }
            else
            {
                errors.Add("Invalid exp claim format");
                return QeaaValidationResult.Failure(errors.ToArray());
            }

            var expiry = DateTimeOffset.FromUnixTimeSeconds(expTimestamp);
            if (expiry < DateTimeOffset.UtcNow)
            {
                errors.Add("QEAA credential has expired");
            }
        }

        return errors.Count > 0
            ? QeaaValidationResult.Failure(errors.ToArray())
            : QeaaValidationResult.Success();
    }

    /// <summary>
    /// Extracts the credential category from a QEAA VCT.
    /// </summary>
    /// <param name="vct">The verifiable credential type.</param>
    /// <returns>The credential category (e.g., "diploma", "professional").</returns>
    public string? ExtractCredentialCategory(string? vct)
    {
        if (string.IsNullOrWhiteSpace(vct) || !IsQeaaCredential(vct))
        {
            return null;
        }

        // VCT format: urn:eu:europa:ec:eudi:qeaa:{category}:{subcategory}
        var suffix = vct.Substring(EudiwConstants.Qeaa.VctPrefix.Length);
        var parts = suffix.Split(':');
        return parts.Length > 0 ? parts[0] : null;
    }
}
