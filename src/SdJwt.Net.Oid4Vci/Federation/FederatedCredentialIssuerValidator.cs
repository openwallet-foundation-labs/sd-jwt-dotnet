using SdJwt.Net.Oid4Vci.Models;
using SdJwt.Net.OidFederation.Logic;
using System.Text.Json;

namespace SdJwt.Net.Oid4Vci.Federation;

/// <summary>
/// Validates an OID4VCI Credential Issuer using a resolved OpenID Federation trust chain.
/// </summary>
public static class FederatedCredentialIssuerValidator
{
    private const string CredentialIssuerProtocol = "openid_credential_issuer";

    /// <summary>
    /// Validates that the resolved trust chain represents a trusted OID4VCI Credential Issuer.
    /// </summary>
    /// <param name="trustChain">The resolved OpenID Federation trust chain.</param>
    /// <param name="options">Optional validation requirements.</param>
    /// <returns>The validation result and resolved Credential Issuer metadata.</returns>
    public static FederatedCredentialIssuerValidationResult ValidateTrustChain(
        TrustChainResult trustChain,
        FederatedCredentialIssuerValidationOptions? options = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(trustChain);
#else
        if (trustChain == null)
            throw new ArgumentNullException(nameof(trustChain));
#endif

        options ??= new FederatedCredentialIssuerValidationOptions();

        if (!trustChain.IsValid)
        {
            return FederatedCredentialIssuerValidationResult.Untrusted(
                trustChain.ErrorMessage ?? "OpenID Federation trust chain is invalid.");
        }

        if (!IsAllowedTrustAnchor(trustChain.TrustAnchor, options.AllowedTrustAnchors))
        {
            return FederatedCredentialIssuerValidationResult.Untrusted(
                "The trust chain terminates at a trust anchor that is not allowed.");
        }

        foreach (var requiredTrustMark in options.RequiredTrustMarks ?? Array.Empty<string>())
        {
            if (!trustChain.HasTrustMark(requiredTrustMark))
            {
                return FederatedCredentialIssuerValidationResult.Untrusted(
                    $"Required trust mark '{requiredTrustMark}' was not present.");
            }
        }

        var rawMetadata = trustChain.GetEffectiveMetadata(CredentialIssuerProtocol);
        if (rawMetadata == null)
        {
            return FederatedCredentialIssuerValidationResult.Untrusted(
                "Resolved metadata does not contain openid_credential_issuer metadata.");
        }

        var metadata = ConvertMetadata<CredentialIssuerMetadata>(rawMetadata);
        if (metadata == null)
        {
            return FederatedCredentialIssuerValidationResult.Untrusted(
                "Resolved openid_credential_issuer metadata could not be parsed.");
        }

        if (!string.IsNullOrWhiteSpace(options.ExpectedCredentialIssuer) &&
            !string.Equals(metadata.CredentialIssuer, options.ExpectedCredentialIssuer, StringComparison.Ordinal))
        {
            return FederatedCredentialIssuerValidationResult.Untrusted(
                "Resolved credential issuer metadata does not match the expected credential issuer.");
        }

        if (string.IsNullOrWhiteSpace(metadata.CredentialIssuer))
        {
            return FederatedCredentialIssuerValidationResult.Untrusted(
                "Resolved credential issuer metadata is missing credential_issuer.");
        }

        return FederatedCredentialIssuerValidationResult.Trusted(
            trustChain.TrustAnchor,
            metadata,
            trustChain);
    }

    private static bool IsAllowedTrustAnchor(string? trustAnchor, string[]? allowedTrustAnchors)
    {
        return allowedTrustAnchors == null ||
            allowedTrustAnchors.Length == 0 ||
            (trustAnchor != null && allowedTrustAnchors.Contains(trustAnchor, StringComparer.Ordinal));
    }

    private static T? ConvertMetadata<T>(object metadata)
    {
        if (metadata is T typed)
        {
            return typed;
        }

        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(metadata));
    }
}

/// <summary>
/// Options for validating an OID4VCI Credential Issuer trust chain.
/// </summary>
public sealed class FederatedCredentialIssuerValidationOptions
{
    /// <summary>
    /// Gets or sets the expected <c>credential_issuer</c> identifier.
    /// </summary>
    public string? ExpectedCredentialIssuer { get; set; }

    /// <summary>
    /// Gets or sets the trust anchors allowed for this issuer.
    /// </summary>
    public string[]? AllowedTrustAnchors { get; set; }

    /// <summary>
    /// Gets or sets trust marks required on the resolved entity.
    /// </summary>
    public string[]? RequiredTrustMarks { get; set; }
}

/// <summary>
/// Result of validating a federated OID4VCI Credential Issuer.
/// </summary>
public sealed class FederatedCredentialIssuerValidationResult
{
    private FederatedCredentialIssuerValidationResult()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the issuer is trusted.
    /// </summary>
    public bool IsTrusted { get; private set; }

    /// <summary>
    /// Gets the validation error when the issuer is not trusted.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Gets the trust anchor that anchors the issuer.
    /// </summary>
    public string? TrustAnchor { get; private set; }

    /// <summary>
    /// Gets the resolved OID4VCI Credential Issuer metadata.
    /// </summary>
    public CredentialIssuerMetadata? Metadata { get; private set; }

    /// <summary>
    /// Gets the underlying OpenID Federation trust chain result.
    /// </summary>
    public TrustChainResult? TrustChain { get; private set; }

    /// <summary>
    /// Creates a trusted validation result.
    /// </summary>
    /// <param name="trustAnchor">The selected trust anchor.</param>
    /// <param name="metadata">The resolved Credential Issuer metadata.</param>
    /// <param name="trustChain">The underlying trust chain result.</param>
    /// <returns>A trusted validation result.</returns>
    public static FederatedCredentialIssuerValidationResult Trusted(
        string? trustAnchor,
        CredentialIssuerMetadata metadata,
        TrustChainResult trustChain)
    {
        return new FederatedCredentialIssuerValidationResult
        {
            IsTrusted = true,
            TrustAnchor = trustAnchor,
            Metadata = metadata,
            TrustChain = trustChain
        };
    }

    /// <summary>
    /// Creates an untrusted validation result.
    /// </summary>
    /// <param name="errorMessage">The validation error.</param>
    /// <returns>An untrusted validation result.</returns>
    public static FederatedCredentialIssuerValidationResult Untrusted(string errorMessage)
    {
        return new FederatedCredentialIssuerValidationResult
        {
            IsTrusted = false,
            ErrorMessage = errorMessage
        };
    }
}
