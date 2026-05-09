using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Models.Dcql;
using SdJwt.Net.OidFederation.Logic;
using System.Text.Json;

namespace SdJwt.Net.Oid4Vp.Federation;

/// <summary>
/// Validates OID4VP verifier metadata using a resolved OpenID Federation trust chain.
/// </summary>
public static class FederatedVerifierValidator
{
    /// <summary>
    /// Client identifier scheme value used when <c>client_id</c> is an OpenID Federation Entity Identifier.
    /// </summary>
    public const string OpenIdFederationClientIdScheme = "openid_federation";

    private const string VerifierProtocol = "openid_relying_party_verifier";
    private const string RelyingPartyProtocol = "openid_relying_party";

    /// <summary>
    /// Validates that an authorization request's verifier is trusted by OpenID Federation.
    /// </summary>
    /// <param name="request">The OID4VP Authorization Request.</param>
    /// <param name="trustChain">The resolved OpenID Federation trust chain for the verifier.</param>
    /// <param name="options">Optional validation requirements.</param>
    /// <returns>The validation result and resolved verifier metadata.</returns>
    public static FederatedVerifierValidationResult ValidateTrustChain(
        AuthorizationRequest request,
        TrustChainResult trustChain,
        FederatedVerifierValidationOptions? options = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(trustChain);
#else
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (trustChain == null)
            throw new ArgumentNullException(nameof(trustChain));
#endif

        options ??= new FederatedVerifierValidationOptions();

        if (!trustChain.IsValid)
        {
            return FederatedVerifierValidationResult.Untrusted(
                trustChain.ErrorMessage ?? "OpenID Federation trust chain is invalid.");
        }

        if (!string.IsNullOrWhiteSpace(request.ClientIdScheme) &&
            !string.Equals(request.ClientIdScheme, OpenIdFederationClientIdScheme, StringComparison.Ordinal))
        {
            return FederatedVerifierValidationResult.Untrusted(
                "client_id_scheme is not openid_federation.");
        }

        if (!string.Equals(trustChain.EntityConfiguration?.Subject, request.ClientId, StringComparison.Ordinal))
        {
            return FederatedVerifierValidationResult.Untrusted(
                "The resolved federation subject does not match the authorization request client_id.");
        }

        if (!IsAllowedTrustAnchor(trustChain.TrustAnchor, options.AllowedTrustAnchors))
        {
            return FederatedVerifierValidationResult.Untrusted(
                "The trust chain terminates at a trust anchor that is not allowed.");
        }

        foreach (var requiredTrustMark in options.RequiredTrustMarks ?? Array.Empty<string>())
        {
            if (!trustChain.HasTrustMark(requiredTrustMark))
            {
                return FederatedVerifierValidationResult.Untrusted(
                    $"Required trust mark '{requiredTrustMark}' was not present.");
            }
        }

        var rawMetadata = trustChain.GetEffectiveMetadata(VerifierProtocol)
            ?? trustChain.GetEffectiveMetadata(RelyingPartyProtocol);
        if (rawMetadata == null)
        {
            return FederatedVerifierValidationResult.Untrusted(
                "Resolved metadata does not contain OID4VP verifier metadata.");
        }

        var metadata = ConvertMetadata<VerifierMetadata>(rawMetadata);
        if (metadata == null)
        {
            return FederatedVerifierValidationResult.Untrusted(
                "Resolved OID4VP verifier metadata could not be parsed.");
        }

        return FederatedVerifierValidationResult.Trusted(
            trustChain.TrustAnchor,
            metadata,
            trustChain);
    }

    /// <summary>
    /// Checks whether a resolved federation chain satisfies DCQL <c>openid_federation</c> trusted authorities.
    /// </summary>
    /// <param name="trustChain">The resolved trust chain for the credential issuer or verifier.</param>
    /// <param name="trustedAuthorities">The DCQL trusted authority constraints.</param>
    /// <returns>True when no OpenID Federation authorities are requested or one matches the chain.</returns>
    public static bool SatisfiesOpenIdFederationAuthorities(
        TrustChainResult trustChain,
        IEnumerable<TrustedAuthority>? trustedAuthorities)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(trustChain);
#else
        if (trustChain == null)
            throw new ArgumentNullException(nameof(trustChain));
#endif

        var openIdFederationAuthorities = trustedAuthorities?
            .Where(a => string.Equals(a.Type, OpenIdFederationClientIdScheme, StringComparison.Ordinal))
            .ToArray() ?? Array.Empty<TrustedAuthority>();

        if (openIdFederationAuthorities.Length == 0)
        {
            return true;
        }

        if (!trustChain.IsValid)
        {
            return false;
        }

        var chainEntities = trustChain.GetTrustChainEntities();
        foreach (var authority in openIdFederationAuthorities)
        {
            foreach (var value in authority.Values)
            {
                if (string.Equals(trustChain.TrustAnchor, value, StringComparison.Ordinal) ||
                    chainEntities.Contains(value, StringComparer.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
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
/// Options for validating an OID4VP verifier trust chain.
/// </summary>
public sealed class FederatedVerifierValidationOptions
{
    /// <summary>
    /// Gets or sets the trust anchors allowed for this verifier.
    /// </summary>
    public string[]? AllowedTrustAnchors
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets trust marks required on the resolved verifier.
    /// </summary>
    public string[]? RequiredTrustMarks
    {
        get; set;
    }
}

/// <summary>
/// Result of validating a federated OID4VP verifier.
/// </summary>
public sealed class FederatedVerifierValidationResult
{
    private FederatedVerifierValidationResult()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the verifier is trusted.
    /// </summary>
    public bool IsTrusted
    {
        get; private set;
    }

    /// <summary>
    /// Gets the validation error when the verifier is not trusted.
    /// </summary>
    public string? ErrorMessage
    {
        get; private set;
    }

    /// <summary>
    /// Gets the trust anchor that anchors the verifier.
    /// </summary>
    public string? TrustAnchor
    {
        get; private set;
    }

    /// <summary>
    /// Gets the resolved verifier metadata.
    /// </summary>
    public VerifierMetadata? Metadata
    {
        get; private set;
    }

    /// <summary>
    /// Gets the underlying OpenID Federation trust chain result.
    /// </summary>
    public TrustChainResult? TrustChain
    {
        get; private set;
    }

    /// <summary>
    /// Creates a trusted validation result.
    /// </summary>
    /// <param name="trustAnchor">The selected trust anchor.</param>
    /// <param name="metadata">The resolved verifier metadata.</param>
    /// <param name="trustChain">The underlying trust chain result.</param>
    /// <returns>A trusted validation result.</returns>
    public static FederatedVerifierValidationResult Trusted(
        string? trustAnchor,
        VerifierMetadata metadata,
        TrustChainResult trustChain)
    {
        return new FederatedVerifierValidationResult
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
    public static FederatedVerifierValidationResult Untrusted(string errorMessage)
    {
        return new FederatedVerifierValidationResult
        {
            IsTrusted = false,
            ErrorMessage = errorMessage
        };
    }
}
