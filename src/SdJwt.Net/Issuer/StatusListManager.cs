using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

        var payload = new JwtPayload
        {
            { "iss", issuer },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { "sub", encodedList }, // The bitstring is the subject of the JWT
            { "status_list", new { bits = 1, len = statusBits.Length } }
        };

        if (jwtId != null)
        {
            payload.Add("jti", jwtId);
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        // 1. Create the SecurityTokenDescriptor. This is the modern, recommended way to describe a token.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // 2. Set the subject (payload) of the token.
            Subject = new ClaimsIdentity(payload.Claims),

            // 3. Set the signing credentials. The handler will use this to set the 'alg' header.
            SigningCredentials = new SigningCredentials(_signingKey, _signingAlgorithm),

            // 4. Add our custom header claims here. This avoids all conflicts.
            AdditionalHeaderClaims = new Dictionary<string, object>
            {
                { "typ", "statuslist+jwt" }
            }
        };

        // 5. Create and sign the token using the descriptor.
        // The handler will correctly merge the 'alg' from SigningCredentials
        // with the 'typ' from the Header property.
        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

        // 6. Write the token to its string representation.
        return tokenHandler.WriteToken(token);
    }
}