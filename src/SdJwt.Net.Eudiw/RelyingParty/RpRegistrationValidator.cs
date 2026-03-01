namespace SdJwt.Net.Eudiw.RelyingParty;

/// <summary>
/// Validates Relying Party registrations per EUDIW requirements.
/// </summary>
public class RpRegistrationValidator
{
    private static readonly HashSet<string> ValidResponseTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "vp_token",
        "vp_token id_token"
    };

    private const string EuTrustFramework = "eu.eudiw.trust";

    /// <summary>
    /// Validates an RP registration.
    /// </summary>
    /// <param name="registration">The registration to validate.</param>
    /// <param name="allowLocalhost">Whether to allow localhost URIs (for development).</param>
    /// <returns>Validation result.</returns>
    public RpValidationResult Validate(RpRegistration? registration, bool allowLocalhost = false)
    {
        if (registration == null)
        {
            return RpValidationResult.Failure("Registration cannot be null");
        }

        // Validate client_id
        if (string.IsNullOrWhiteSpace(registration.ClientId))
        {
            return RpValidationResult.Failure("client_id is required");
        }

        // Validate organization name
        if (string.IsNullOrWhiteSpace(registration.OrganizationName))
        {
            return RpValidationResult.Failure("organization name is required");
        }

        // Validate redirect URIs
        if (registration.RedirectUris == null || registration.RedirectUris.Length == 0)
        {
            return RpValidationResult.Failure("At least one redirect URI is required");
        }

        foreach (var uri in registration.RedirectUris)
        {
            var uriValidation = ValidateRedirectUri(uri, allowLocalhost);
            if (!uriValidation.IsValid)
            {
                return uriValidation;
            }
        }

        // Validate response types
        if (registration.ResponseTypes != null && registration.ResponseTypes.Length > 0)
        {
            foreach (var responseType in registration.ResponseTypes)
            {
                if (!ValidResponseTypes.Contains(responseType))
                {
                    return RpValidationResult.Failure(
                        $"Invalid response_type '{responseType}'. EUDIW only supports: {string.Join(", ", ValidResponseTypes)}");
                }
            }
        }

        // Check trust framework (warning if unknown)
        var warnings = new List<string>();
        if (!string.IsNullOrEmpty(registration.TrustFramework) &&
            !registration.TrustFramework.Equals(EuTrustFramework, StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add($"Unknown trust framework '{registration.TrustFramework}'. Expected '{EuTrustFramework}'.");
        }

        return warnings.Count > 0
            ? RpValidationResult.SuccessWithWarnings(warnings.ToArray())
            : RpValidationResult.Success();
    }

    private static RpValidationResult ValidateRedirectUri(string uri, bool allowLocalhost)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return RpValidationResult.Failure("Redirect URI cannot be empty");
        }

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri))
        {
            return RpValidationResult.Failure($"Invalid redirect URI format: {uri}");
        }

        // Check if localhost is allowed for development
        if (allowLocalhost && IsLocalhost(parsedUri))
        {
            return RpValidationResult.Success();
        }

        // Production URIs must use HTTPS
        if (parsedUri.Scheme != Uri.UriSchemeHttps)
        {
            return RpValidationResult.Failure($"Redirect URI must use HTTPS: {uri}");
        }

        return RpValidationResult.Success();
    }

    private static bool IsLocalhost(Uri uri)
    {
        return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
               uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
               uri.Host.Equals("::1", StringComparison.OrdinalIgnoreCase);
    }
}
