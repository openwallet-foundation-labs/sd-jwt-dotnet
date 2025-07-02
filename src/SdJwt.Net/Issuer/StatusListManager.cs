using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace SdJwt.Net.Issuer;

/// <summary>
/// Manages the creation and signing of a Status List Credential, which is used for
/// Verifiable Credential revocation checking.
/// </summary>
public class StatusListManager
{
    private readonly SecurityKey _signingKey;
    private readonly string _signingAlgorithm;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusListManager"/> class.
    /// </summary>
    /// <param name="signingKey">The security key used to sign the Status List JWT.</param>
    /// <param name="signingAlgorithm">The JWT signing algorithm.</param>
    public StatusListManager(SecurityKey signingKey, string signingAlgorithm)
    {
        if (string.IsNullOrWhiteSpace(signingAlgorithm)) { throw new ArgumentException("Value cannot be null or whitespace.", nameof(signingAlgorithm)); }

        _signingKey = signingKey ?? throw new ArgumentNullException(nameof(signingKey));
        _signingAlgorithm = signingAlgorithm;
    }

    /// <summary>
    /// Creates a signed JWT representing a Status List Credential.
    /// </summary>
    /// <param name="issuer">The issuer of the status list (e.g., "https://issuer.example.com").</param>
    /// <param name="statusBits">A BitArray where a value of 'true' at an index indicates that the corresponding credential is revoked.</param>
    /// <param name="jwtId">An optional JWT ID (jti) for the status list credential.</param>
    /// <returns>A signed JWT string for the Status List Credential.</returns>
    public string CreateStatusListCredential(string issuer, BitArray statusBits, string? jwtId = null)
    {
        if (string.IsNullOrWhiteSpace(issuer)) { throw new ArgumentException("Value cannot be null or whitespace.", nameof(issuer)); }
        if (statusBits == null) { throw new ArgumentNullException(nameof(statusBits)); }

        var listBytes = new byte[(statusBits.Length + 7) / 8];
        statusBits.CopyTo(listBytes, 0);
        var encodedList = Base64UrlEncoder.Encode(listBytes);

        var tokenHandler = new JwtSecurityTokenHandler();

        // 1. Create the header from the signing credentials.
        var header = new JwtHeader(new SigningCredentials(_signingKey, _signingAlgorithm))
        {
            // 2. Overwrite the default 'typ' header with our specific one.
            //    Using the indexer avoids the "duplicate key" error.
            ["typ"] = "statuslist+jwt"
        };

        // 3. Create the list of claims for the payload.
        var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, encodedList),
        new("status_list",
            $"{{\"bits\": 1, \"len\": {statusBits.Length}}}",
            JsonClaimValueTypes.Json)
    };

        if (jwtId != null)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, jwtId));
        }

        // 4. Create the payload.
        var payload = new JwtPayload(
            issuer: issuer,
            audience: null,
            claims: claims,
            notBefore: null,
            expires: null,
            issuedAt: DateTime.UtcNow
        );

        // 5. Create the JWT from the custom header and payload.
        var token = new JwtSecurityToken(header, payload);

        // 6. Use the handler to serialize the token.
        return tokenHandler.WriteToken(token);
    }
}