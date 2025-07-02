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

           // 1. Create a list of claims for the payload.
        var claims = new List<Claim>
        {
            // The bitstring is the subject of the JWT.
            new(JwtRegisteredClaimNames.Sub, encodedList),
            
            // Add the custom status_list object as a JSON claim.
            new("status_list",
                      $"{{\"bits\": 1, \"len\": {statusBits.Length}}}",
                      JsonClaimValueTypes.Json)
        };

        if (jwtId != null)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, jwtId));
        }

        // 2. Create the SecurityTokenDescriptor using its direct properties.
        // This is the most reliable way to ensure standard claims are included.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // Set the standard 'iss' and 'iat' claims directly.
            Issuer = issuer,
            IssuedAt = DateTime.UtcNow,

            // Set the payload claims.
            Subject = new ClaimsIdentity(claims),

            // Set the signing credentials.
            SigningCredentials = new SigningCredentials(_signingKey, _signingAlgorithm),

            // Set the custom header claim.
            AdditionalHeaderClaims = new Dictionary<string, object>
            {
                { JwtHeaderParameterNames.Typ, "statuslist+jwt" }
            }
        };

        // 3. Create and sign the token.
        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}