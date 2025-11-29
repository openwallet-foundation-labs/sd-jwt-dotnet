using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Oid4Vci.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace SdJwt.Net.Oid4Vci.Client;

/// <summary>
/// Exception thrown when proof building fails.
/// </summary>
public class ProofBuildException : Exception
{
    public ProofBuildException(string message) : base(message) { }
    public ProofBuildException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Utility class for building proof of possession JWTs for OID4VCI credential requests.
/// </summary>
public static class ProofBuilder
{
    /// <summary>
    /// Creates a proof of possession JWT for OID4VCI credential requests.
    /// </summary>
    /// <param name="walletPrivateKey">The wallet's private key for signing</param>
    /// <param name="issuerUrl">The credential issuer URL</param>
    /// <param name="cNonce">The challenge nonce from the issuer</param>
    /// <param name="clientId">Optional client identifier</param>
    /// <returns>A signed JWT proof string</returns>
    /// <exception cref="ProofBuildException">Thrown when proof creation fails</exception>
    public static string CreateProof(
        SecurityKey walletPrivateKey,
        string issuerUrl,
        string cNonce,
        string? clientId = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(walletPrivateKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(issuerUrl, nameof(issuerUrl));
        ArgumentException.ThrowIfNullOrWhiteSpace(cNonce, nameof(cNonce));
#else
        if (walletPrivateKey == null)
            throw new ArgumentNullException(nameof(walletPrivateKey));
        if (string.IsNullOrWhiteSpace(issuerUrl))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(issuerUrl));
        if (string.IsNullOrWhiteSpace(cNonce))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(cNonce));
#endif

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Determine the algorithm based on the key type
            var algorithm = GetAlgorithmFromKey(walletPrivateKey);
            
            // Create JWT header
            var header = new JwtHeader(
                signingCredentials: new SigningCredentials(walletPrivateKey, algorithm))
            {
                ["typ"] = Oid4VciConstants.ProofJwtType
            };

            // Add JWK to header for public key verification
            var jwk = JsonWebKeyConverter.ConvertFromSecurityKey(walletPrivateKey);
            if (jwk != null)
            {
                // Remove private key components for security
                jwk.D = null;
                jwk.DP = null;
                jwk.DQ = null;
                jwk.P = null;
                jwk.Q = null;
                jwk.QI = null;
                
                // Convert JWK to dictionary manually to avoid serialization issues
                var jwkDict = CreateJwkDictionary(jwk);
                header["jwk"] = jwkDict;
            }

            // Create JWT payload
            var payload = new JwtPayload();
            
            // Add issuer if client ID is provided
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                payload["iss"] = clientId;
            }
            
            payload["aud"] = issuerUrl;
            payload["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            payload["nonce"] = cNonce;

            // Create and sign the JWT
            var token = new JwtSecurityToken(header, payload);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            throw new ProofBuildException("Failed to create proof of possession JWT", ex);
        }
    }

    /// <summary>
    /// Creates a proof of possession JWT with explicit algorithm specification.
    /// </summary>
    /// <param name="walletPrivateKey">The wallet's private key for signing</param>
    /// <param name="algorithm">The signing algorithm to use</param>
    /// <param name="issuerUrl">The credential issuer URL</param>
    /// <param name="cNonce">The challenge nonce from the issuer</param>
    /// <param name="clientId">Optional client identifier</param>
    /// <returns>A signed JWT proof string</returns>
    /// <exception cref="ProofBuildException">Thrown when proof creation fails</exception>
    public static string CreateProof(
        SecurityKey walletPrivateKey,
        string algorithm,
        string issuerUrl,
        string cNonce,
        string? clientId = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(walletPrivateKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(algorithm, nameof(algorithm));
        ArgumentException.ThrowIfNullOrWhiteSpace(issuerUrl, nameof(issuerUrl));
        ArgumentException.ThrowIfNullOrWhiteSpace(cNonce, nameof(cNonce));
#else
        if (walletPrivateKey == null)
            throw new ArgumentNullException(nameof(walletPrivateKey));
        if (string.IsNullOrWhiteSpace(algorithm))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(algorithm));
        if (string.IsNullOrWhiteSpace(issuerUrl))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(issuerUrl));
        if (string.IsNullOrWhiteSpace(cNonce))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(cNonce));
#endif

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Create JWT header with specified algorithm
            var header = new JwtHeader(
                signingCredentials: new SigningCredentials(walletPrivateKey, algorithm))
            {
                ["typ"] = Oid4VciConstants.ProofJwtType
            };

            // Add JWK to header for public key verification
            var jwk = JsonWebKeyConverter.ConvertFromSecurityKey(walletPrivateKey);
            if (jwk != null)
            {
                // Remove private key components for security
                jwk.D = null;
                jwk.DP = null;
                jwk.DQ = null;
                jwk.P = null;
                jwk.Q = null;
                jwk.QI = null;
                
                // Convert JWK to dictionary manually to avoid serialization issues
                var jwkDict = CreateJwkDictionary(jwk);
                header["jwk"] = jwkDict;
            }

            // Create JWT payload
            var payload = new JwtPayload();
            
            // Add issuer if client ID is provided
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                payload["iss"] = clientId;
            }
            
            payload["aud"] = issuerUrl;
            payload["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            payload["nonce"] = cNonce;

            // Create and sign the JWT
            var token = new JwtSecurityToken(header, payload);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            throw new ProofBuildException("Failed to create proof of possession JWT", ex);
        }
    }

    private static Dictionary<string, object> CreateJwkDictionary(JsonWebKey jwk)
    {
        var dict = new Dictionary<string, object>
        {
            ["kty"] = jwk.Kty
        };

        if (!string.IsNullOrEmpty(jwk.Use))
            dict["use"] = jwk.Use;

        if (jwk.KeyOps?.Count > 0)
            dict["key_ops"] = jwk.KeyOps.ToArray();

        if (!string.IsNullOrEmpty(jwk.Alg))
            dict["alg"] = jwk.Alg;

        if (!string.IsNullOrEmpty(jwk.Kid))
            dict["kid"] = jwk.Kid;

        // EC key parameters
        if (!string.IsNullOrEmpty(jwk.Crv))
            dict["crv"] = jwk.Crv;
        if (!string.IsNullOrEmpty(jwk.X))
            dict["x"] = jwk.X;
        if (!string.IsNullOrEmpty(jwk.Y))
            dict["y"] = jwk.Y;

        // RSA key parameters
        if (!string.IsNullOrEmpty(jwk.N))
            dict["n"] = jwk.N;
        if (!string.IsNullOrEmpty(jwk.E))
            dict["e"] = jwk.E;

        // Symmetric key parameters
        if (!string.IsNullOrEmpty(jwk.K))
            dict["k"] = jwk.K;

        return dict;
    }

    private static string GetAlgorithmFromKey(SecurityKey key)
    {
        return key switch
        {
            ECDsaSecurityKey _ => SecurityAlgorithms.EcdsaSha256,
            RsaSecurityKey _ => SecurityAlgorithms.RsaSha256,
            SymmetricSecurityKey _ => SecurityAlgorithms.HmacSha256,
            _ => throw new ProofBuildException($"Unsupported key type: {key.GetType().Name}")
        };
    }
}