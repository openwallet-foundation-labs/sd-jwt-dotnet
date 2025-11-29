using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Oid4Vci.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace SdJwt.Net.Oid4Vci.Issuer;

/// <summary>
/// Exception thrown when proof validation fails.
/// </summary>
public class ProofValidationException : Exception
{
    public ProofValidationException(string message) : base(message) { }
    public ProofValidationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Result of proof validation containing the extracted public key and claims.
/// </summary>
public class ProofValidationResult
{
    /// <summary>
    /// Gets the public key extracted from the proof JWT.
    /// </summary>
    public SecurityKey PublicKey { get; }

    /// <summary>
    /// Gets the JWT claims from the validated proof.
    /// </summary>
    public JwtSecurityToken JwtToken { get; }

    /// <summary>
    /// Gets the client ID from the proof, if present.
    /// </summary>
    public string? ClientId { get; }

    /// <summary>
    /// Gets the audience from the proof.
    /// </summary>
    public string Audience { get; }

    /// <summary>
    /// Gets the nonce from the proof.
    /// </summary>
    public string Nonce { get; }

    /// <summary>
    /// Gets the issued at time from the proof.
    /// </summary>
    public DateTime IssuedAt { get; }

    public ProofValidationResult(SecurityKey publicKey, JwtSecurityToken jwtToken, string? clientId, string audience, string nonce, DateTime issuedAt)
    {
        PublicKey = publicKey;
        JwtToken = jwtToken;
        ClientId = clientId;
        Audience = audience;
        Nonce = nonce;
        IssuedAt = issuedAt;
    }
}

/// <summary>
/// Utility class for validating nonces and proof JWTs in OID4VCI flows.
/// </summary>
public static class CNonceValidator
{
    /// <summary>
    /// Validates a proof of possession JWT.
    /// </summary>
    /// <param name="jwtString">The proof JWT string</param>
    /// <param name="expectedCNonce">The expected nonce value</param>
    /// <param name="expectedIssuerUrl">The expected audience (issuer URL)</param>
    /// <param name="clockSkew">Optional clock skew tolerance (default: 5 minutes)</param>
    /// <returns>Proof validation result containing the public key and claims</returns>
    /// <exception cref="ProofValidationException">Thrown when proof validation fails</exception>
    public static ProofValidationResult ValidateProof(
        string jwtString,
        string expectedCNonce,
        string expectedIssuerUrl,
        TimeSpan? clockSkew = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtString, nameof(jwtString));
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedCNonce, nameof(expectedCNonce));
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedIssuerUrl, nameof(expectedIssuerUrl));
#else
        if (string.IsNullOrWhiteSpace(jwtString))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(jwtString));
        if (string.IsNullOrWhiteSpace(expectedCNonce))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(expectedCNonce));
        if (string.IsNullOrWhiteSpace(expectedIssuerUrl))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(expectedIssuerUrl));
#endif

        var tolerance = clockSkew ?? TimeSpan.FromMinutes(5);

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // First, read the token without validation to extract the header and claims
            var token = tokenHandler.ReadJwtToken(jwtString);

            // Validate the header type
            if (token.Header.Typ != Oid4VciConstants.ProofJwtType)
            {
                throw new ProofValidationException($"Invalid JWT type. Expected '{Oid4VciConstants.ProofJwtType}', got '{token.Header.Typ}'");
            }

            // Extract the public key from the header
            var publicKey = ExtractPublicKeyFromHeader(token.Header);

            // Validate the audience
            var audience = token.Claims.FirstOrDefault(c => c.Type == "aud")?.Value;
            if (string.IsNullOrEmpty(audience))
            {
                throw new ProofValidationException("Missing 'aud' claim in proof JWT");
            }

            if (!string.Equals(audience, expectedIssuerUrl, StringComparison.OrdinalIgnoreCase))
            {
                throw new ProofValidationException($"Invalid audience. Expected '{expectedIssuerUrl}', got '{audience}'");
            }

            // Validate the nonce
            var nonce = token.Claims.FirstOrDefault(c => c.Type == "nonce")?.Value;
            if (string.IsNullOrEmpty(nonce))
            {
                throw new ProofValidationException("Missing 'nonce' claim in proof JWT");
            }

            if (nonce != expectedCNonce)
            {
                throw new ProofValidationException($"Invalid nonce. Expected '{expectedCNonce}', got '{nonce}'");
            }

            // Validate the issued at time
            var iatClaim = token.Claims.FirstOrDefault(c => c.Type == "iat")?.Value;
            if (string.IsNullOrEmpty(iatClaim) || !long.TryParse(iatClaim, out var iatUnixTime))
            {
                throw new ProofValidationException("Missing or invalid 'iat' claim in proof JWT");
            }

            var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iatUnixTime).DateTime;
            var now = DateTime.UtcNow;

            if (issuedAt > now.Add(tolerance))
            {
                throw new ProofValidationException($"JWT issued in the future. Issued at: {issuedAt}, Current time: {now}");
            }

            if (issuedAt < now.Subtract(tolerance))
            {
                throw new ProofValidationException($"JWT too old. Issued at: {issuedAt}, Current time: {now}");
            }

            // Extract client ID if present
            var clientId = token.Claims.FirstOrDefault(c => c.Type == "iss")?.Value;

            // Now validate the signature
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false, // We don't validate issuer in proof JWTs
                ValidateAudience = false, // We already validated audience manually
                ValidateLifetime = false, // We already validated timing manually
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = publicKey,
                ClockSkew = tolerance
            };

            tokenHandler.ValidateToken(jwtString, validationParameters, out var validatedToken);

            return new ProofValidationResult(
                publicKey,
                (JwtSecurityToken)validatedToken,
                clientId,
                audience,
                nonce,
                issuedAt);
        }
        catch (ProofValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ProofValidationException("Failed to validate proof JWT", ex);
        }
    }

    /// <summary>
    /// Generates a secure nonce for use in credential requests.
    /// </summary>
    /// <param name="length">The length of the nonce (default: 32 characters)</param>
    /// <returns>A cryptographically secure random nonce</returns>
    public static string GenerateNonce(int length = 32)
    {
        if (length <= 0)
            throw new ArgumentException("Length must be positive", nameof(length));

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static SecurityKey ExtractPublicKeyFromHeader(JwtHeader header)
    {
        // Try to get JWK from header
        if (header.TryGetValue("jwk", out var jwkObj))
        {
            try
            {
                string jwkJson;
                
                if (jwkObj is Dictionary<string, object> jwkDict)
                {
                    // Convert dictionary back to JSON
                    jwkJson = JsonSerializer.Serialize(jwkDict);
                }
                else if (jwkObj is JsonElement jwkElement)
                {
                    jwkJson = jwkElement.GetRawText();
                }
                else if (jwkObj is string jwkString)
                {
                    jwkJson = jwkString;
                }
                else
                {
                    throw new ProofValidationException($"Unsupported JWK format in header: {jwkObj.GetType()}");
                }

                var jwk = new JsonWebKey(jwkJson);
                return jwk;
            }
            catch (Exception ex)
            {
                throw new ProofValidationException("Failed to extract public key from JWK in header", ex);
            }
        }

        // Try to get key ID from header
        if (!string.IsNullOrEmpty(header.Kid))
        {
            throw new ProofValidationException(
                $"JWT contains 'kid' header but no 'jwk'. Key resolution by ID is not supported. " +
                $"Please include the public key in the 'jwk' header. Kid: {header.Kid}");
        }

        throw new ProofValidationException("No public key found in JWT header. Either 'jwk' or 'kid' header is required");
    }
}