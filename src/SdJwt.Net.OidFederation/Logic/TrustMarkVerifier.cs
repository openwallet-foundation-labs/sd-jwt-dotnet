using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.OidFederation.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace SdJwt.Net.OidFederation.Logic;

/// <summary>
/// Verifies the signature and claims of a Trust Mark JWT (<c>typ=trust-mark+jwt</c>) per
/// OpenID Federation 1.0 §7.1.
/// </summary>
/// <remarks>
/// A Trust Mark MUST be signed by its Trust Mark Issuer — an entity cannot be trusted on the
/// basis of a Trust Mark it merely placed in its own (self-signed) entity configuration.
/// This verifier resolves the issuer's signing key and cryptographically verifies the embedded
/// JWT, in contrast to <see cref="TrustMark.IsValid"/> which only checks syntactic/temporal fields.
/// </remarks>
public sealed class TrustMarkVerifier
{
    private readonly Func<string, string?, CancellationToken, Task<SecurityKey?>> _issuerKeyResolver;
    private readonly TimeSpan _clockSkew;

    private static readonly string[] AllowedAlgorithms =
    [
        SecurityAlgorithms.EcdsaSha256, SecurityAlgorithms.EcdsaSha384, SecurityAlgorithms.EcdsaSha512,
        SecurityAlgorithms.RsaSsaPssSha256, SecurityAlgorithms.RsaSsaPssSha384, SecurityAlgorithms.RsaSsaPssSha512,
        SecurityAlgorithms.RsaSha256, SecurityAlgorithms.RsaSha384, SecurityAlgorithms.RsaSha512
    ];

    /// <summary>
    /// Creates a verifier that resolves a Trust Mark Issuer's signing key via the supplied callback
    /// (for example, by resolving the issuer's trust chain to a Trust Anchor).
    /// </summary>
    /// <param name="issuerKeyResolver">Resolves a signing key given the issuer identifier and optional key id.</param>
    /// <param name="clockSkew">Allowed clock skew for lifetime validation. Defaults to 5 minutes.</param>
    public TrustMarkVerifier(
        Func<string, string?, CancellationToken, Task<SecurityKey?>> issuerKeyResolver,
        TimeSpan? clockSkew = null)
    {
        _issuerKeyResolver = issuerKeyResolver ?? throw new ArgumentNullException(nameof(issuerKeyResolver));
        _clockSkew = clockSkew ?? TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Creates a verifier backed by a fixed set of trusted Trust Mark Issuer keys, keyed by issuer identifier.
    /// </summary>
    /// <param name="issuerKeys">Map of issuer identifier to its trusted signing key.</param>
    /// <param name="clockSkew">Allowed clock skew for lifetime validation. Defaults to 5 minutes.</param>
    public static TrustMarkVerifier FromTrustedKeys(
        IReadOnlyDictionary<string, SecurityKey> issuerKeys,
        TimeSpan? clockSkew = null)
    {
        if (issuerKeys == null)
        {
            throw new ArgumentNullException(nameof(issuerKeys));
        }

        return new TrustMarkVerifier(
            (issuer, _, _) => Task.FromResult(
                issuerKeys.TryGetValue(issuer, out var key) ? key : null),
            clockSkew);
    }

    /// <summary>
    /// Verifies the signed Trust Mark JWT carried in <see cref="TrustMark.TrustMarkValue"/>.
    /// </summary>
    /// <param name="trustMark">The Trust Mark whose <c>trust_mark</c> JWT is to be verified.</param>
    /// <param name="expectedSubject">When supplied, the JWT <c>sub</c> must equal this entity identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The verification result.</returns>
    public async Task<TrustMarkVerificationResult> VerifyAsync(
        TrustMark trustMark,
        string? expectedSubject = null,
        CancellationToken cancellationToken = default)
    {
        if (trustMark == null)
        {
            throw new ArgumentNullException(nameof(trustMark));
        }

        var jwt = trustMark.TrustMarkValue;
        if (string.IsNullOrWhiteSpace(jwt) || jwt!.Count(c => c == '.') != 2)
        {
            return TrustMarkVerificationResult.Failure(
                "Trust mark does not carry a signed JWT.", OidFederationConstants.ErrorCodes.TrustMarkValidationFailed);
        }

        JwtSecurityToken parsed;
        string? issuer;
        string? subject;
        string? trustMarkType;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            parsed = handler.ReadJwtToken(jwt);

            if (!string.Equals(parsed.Header.Typ, OidFederationConstants.JwtHeaders.TrustMarkType, StringComparison.Ordinal))
            {
                return TrustMarkVerificationResult.Failure(
                    $"Trust mark JWT typ must be '{OidFederationConstants.JwtHeaders.TrustMarkType}'.",
                    OidFederationConstants.ErrorCodes.TrustMarkValidationFailed);
            }

            issuer = GetClaim(parsed, "iss");
            subject = GetClaim(parsed, "sub");
            // The spec uses 'trust_mark_type'; older drafts/this library's model use 'id'.
            trustMarkType = GetClaim(parsed, "trust_mark_type") ?? GetClaim(parsed, "id");

            if (string.IsNullOrEmpty(issuer))
            {
                return Fail("Trust mark JWT is missing the iss claim.");
            }

            if (string.IsNullOrEmpty(subject))
            {
                return Fail("Trust mark JWT is missing the sub claim.");
            }

            if (string.IsNullOrEmpty(trustMarkType))
            {
                return Fail("Trust mark JWT is missing the trust_mark_type/id claim.");
            }

            if (GetClaim(parsed, "iat") == null)
            {
                return Fail("Trust mark JWT is missing the iat claim.");
            }
        }
        catch (Exception ex) when (ex is ArgumentException or JsonException or FormatException)
        {
            return Fail($"Trust mark JWT could not be parsed: {ex.Message}");
        }

        // The issuer's signing key must be resolvable and trusted.
        var key = await _issuerKeyResolver(issuer!, parsed.Header.Kid, cancellationToken).ConfigureAwait(false);
        if (key == null)
        {
            return Fail($"Could not resolve a trusted signing key for trust mark issuer '{issuer}'.");
        }

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                RequireExpirationTime = false,
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidAlgorithms = AllowedAlgorithms,
                ClockSkew = _clockSkew
            };

            new JwtSecurityTokenHandler().ValidateToken(jwt, validationParameters, out _);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return Fail($"Trust mark JWT signature/lifetime validation failed: {ex.Message}");
        }

        // Cross-check the verified claims against the enclosing TrustMark metadata.
        if (!string.IsNullOrWhiteSpace(trustMark.Id) &&
            !string.Equals(trustMark.Id, trustMarkType, StringComparison.Ordinal))
        {
            return Fail("Trust mark JWT trust_mark_type does not match the declared trust mark id.");
        }

        if (!string.IsNullOrWhiteSpace(trustMark.Issuer) &&
            !string.Equals(trustMark.Issuer, issuer, StringComparison.Ordinal))
        {
            return Fail("Trust mark JWT iss does not match the declared trust mark issuer.");
        }

        if (!string.IsNullOrEmpty(expectedSubject) &&
            !string.Equals(expectedSubject, subject, StringComparison.Ordinal))
        {
            return Fail("Trust mark JWT sub does not match the expected subject.");
        }

        return TrustMarkVerificationResult.Success(trustMarkType, issuer, subject);
    }

    private static string? GetClaim(JwtSecurityToken jwt, string name)
        => jwt.Payload.TryGetValue(name, out var value) ? value?.ToString() : null;

    private static TrustMarkVerificationResult Fail(string error)
        => TrustMarkVerificationResult.Failure(error, OidFederationConstants.ErrorCodes.TrustMarkValidationFailed);
}
