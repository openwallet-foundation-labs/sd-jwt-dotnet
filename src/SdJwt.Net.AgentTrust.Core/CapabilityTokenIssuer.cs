using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Mints SD-JWT capability tokens.
/// </summary>
public class CapabilityTokenIssuer
{
    private readonly SdIssuer _sdIssuer;
    private readonly INonceStore _nonceStore;
    private readonly ILogger<CapabilityTokenIssuer> _logger;

    /// <summary>
    /// Initializes a new issuer.
    /// </summary>
    public CapabilityTokenIssuer(
        SecurityKey signingKey,
        string signingAlgorithm,
        INonceStore nonceStore,
        ILogger<CapabilityTokenIssuer>? logger = null)
    {
        if (signingKey == null)
        {
            throw new ArgumentNullException(nameof(signingKey));
        }

        if (string.IsNullOrWhiteSpace(signingAlgorithm))
        {
            throw new ArgumentException("Signing algorithm is required.", nameof(signingAlgorithm));
        }

        _sdIssuer = new SdIssuer(signingKey, signingAlgorithm);
        _nonceStore = nonceStore ?? throw new ArgumentNullException(nameof(nonceStore));
        _logger = logger ?? NullLogger<CapabilityTokenIssuer>.Instance;
    }

    /// <summary>
    /// Mints a capability token.
    /// </summary>
    public CapabilityTokenResult Mint(CapabilityTokenOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.Issuer) ||
            string.IsNullOrWhiteSpace(options.Audience) ||
            string.IsNullOrWhiteSpace(options.Capability.Tool) ||
            string.IsNullOrWhiteSpace(options.Capability.Action) ||
            string.IsNullOrWhiteSpace(options.Context.CorrelationId))
        {
            throw new ArgumentException("Missing capability token options.", nameof(options));
        }

        var now = DateTimeOffset.UtcNow;
        var tokenId = Guid.NewGuid().ToString("N");
        var expiresAt = now.Add(options.Lifetime <= TimeSpan.Zero ? TimeSpan.FromSeconds(60) : options.Lifetime);

        var claims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = options.Issuer,
            [JwtRegisteredClaimNames.Aud] = options.Audience,
            [JwtRegisteredClaimNames.Iat] = now.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = expiresAt.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Jti] = tokenId,
            ["cap"] = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(options.Capability))!,
            ["ctx"] = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(options.Context))!
        };

        var issuance = _sdIssuer.Issue(claims, new SdIssuanceOptions()).Issuance;
        _logger.LogDebug("Minted capability token {TokenId} for {Audience}", tokenId, options.Audience);

        return new CapabilityTokenResult
        {
            Token = issuance,
            TokenId = tokenId,
            ExpiresAt = expiresAt
        };
    }
}

