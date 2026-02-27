using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Oid4Vci.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace SdJwt.Net.Oid4Vci.Issuer;

/// <summary>
/// Exception thrown when proof validation fails.
/// </summary>
public class ProofValidationException : Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProofValidationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ProofValidationException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProofValidationException"/> class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ProofValidationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Result of proof validation containing the extracted public key and claims.
/// </summary>
public class ProofValidationResult {
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProofValidationResult"/> class.
        /// </summary>
        /// <param name="publicKey">The public key extracted from the proof JWT.</param>
        /// <param name="jwtToken">The JWT claims from the validated proof.</param>
        /// <param name="clientId">The client ID from the proof, if present.</param>
        /// <param name="audience">The audience from the proof.</param>
        /// <param name="nonce">The nonce from the proof.</param>
        /// <param name="issuedAt">The issued at time from the proof.</param>
        public ProofValidationResult(SecurityKey publicKey, JwtSecurityToken jwtToken, string? clientId, string audience, string nonce, DateTime issuedAt) {
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
public static class CNonceValidator {
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
            TimeSpan? clockSkew = null) {
                return ValidateProof(jwtString, expectedCNonce, expectedIssuerUrl, null, clockSkew);
        }

        /// <summary>
        /// Validates a proof of possession JWT with optional key resolver support for <c>kid</c>.
        /// </summary>
        /// <param name="jwtString">The proof JWT string.</param>
        /// <param name="expectedCNonce">The expected nonce value.</param>
        /// <param name="expectedIssuerUrl">The expected audience (issuer URL).</param>
        /// <param name="keyResolver">Optional key resolver used when the JWT header contains <c>kid</c> but no <c>jwk</c>.</param>
        /// <param name="clockSkew">Optional clock skew tolerance (default: 5 minutes).</param>
        /// <returns>Proof validation result containing the public key and claims.</returns>
        /// <exception cref="ProofValidationException">Thrown when proof validation fails.</exception>
        public static ProofValidationResult ValidateProof(
            string jwtString,
            string expectedCNonce,
            string expectedIssuerUrl,
            Func<string, SecurityKey?>? keyResolver,
            TimeSpan? clockSkew = null) {
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

                try {
                        var tokenHandler = new JwtSecurityTokenHandler();

                        // First, read the token without validation to extract the header and claims
                        var token = tokenHandler.ReadJwtToken(jwtString);

                        // Validate the header type
                        if (token.Header.Typ != Oid4VciConstants.ProofJwtType) {
                                throw new ProofValidationException($"Invalid JWT type. Expected '{Oid4VciConstants.ProofJwtType}', got '{token.Header.Typ}'");
                        }

                        // Extract the public key from the header
                        var publicKey = ExtractPublicKeyFromHeader(token.Header, keyResolver);

                        // Validate the audience
                        var audience = token.Claims.FirstOrDefault(c => c.Type == "aud")?.Value;
                        if (string.IsNullOrEmpty(audience)) {
                                throw new ProofValidationException("Missing 'aud' claim in proof JWT");
                        }

                        if (!string.Equals(audience, expectedIssuerUrl, StringComparison.OrdinalIgnoreCase)) {
                                throw new ProofValidationException($"Invalid audience. Expected '{expectedIssuerUrl}', got '{audience}'");
                        }

                        // Validate the nonce
                        var nonce = token.Claims.FirstOrDefault(c => c.Type == "nonce")?.Value;
                        if (string.IsNullOrEmpty(nonce)) {
                                throw new ProofValidationException("Missing 'nonce' claim in proof JWT");
                        }

                        if (nonce != expectedCNonce) {
                                throw new ProofValidationException($"Invalid nonce. Expected '{expectedCNonce}', got '{nonce}'");
                        }

                        // Validate the issued at time
                        var iatClaim = token.Claims.FirstOrDefault(c => c.Type == "iat")?.Value;
                        if (string.IsNullOrEmpty(iatClaim) || !long.TryParse(iatClaim, out var iatUnixTime)) {
                                throw new ProofValidationException("Missing or invalid 'iat' claim in proof JWT");
                        }

                        var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iatUnixTime).DateTime;
                        var now = DateTime.UtcNow;

                        if (issuedAt > now.Add(tolerance)) {
                                throw new ProofValidationException($"JWT issued in the future. Issued at: {issuedAt}, Current time: {now}");
                        }

                        if (issuedAt < now.Subtract(tolerance)) {
                                throw new ProofValidationException($"JWT too old. Issued at: {issuedAt}, Current time: {now}");
                        }

                        // Extract client ID if present
                        var clientId = token.Claims.FirstOrDefault(c => c.Type == "iss")?.Value;

                        // Now validate the signature
                        var validationParameters = new TokenValidationParameters {
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
                catch (ProofValidationException) {
                        throw;
                }
                catch (Exception ex) {
                        throw new ProofValidationException("Failed to validate proof JWT", ex);
                }
        }

        /// <summary>
        /// Generates a secure nonce for use in credential requests.
        /// </summary>
        /// <param name="length">The length of the nonce (default: 32 characters)</param>
        /// <returns>A cryptographically secure random nonce</returns>
        public static string GenerateNonce(int length = 32) {
                if (length <= 0)
                        throw new ArgumentException("Length must be positive", nameof(length));

                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var nonceChars = new char[length];
                using var rng = RandomNumberGenerator.Create();
                var buffer = new byte[4];
                var max = (uint)chars.Length;
                var upperBound = uint.MaxValue - (uint.MaxValue % max);

                for (var i = 0; i < length; i++) {
                        uint value;
                        do {
                                rng.GetBytes(buffer);
                                value = BitConverter.ToUInt32(buffer, 0);
                        } while (value >= upperBound);

                        nonceChars[i] = chars[(int)(value % max)];
                }

                return new string(nonceChars);
        }

        private static SecurityKey ExtractPublicKeyFromHeader(JwtHeader header, Func<string, SecurityKey?>? keyResolver) {
                // Try to get JWK from header
                if (header.TryGetValue("jwk", out var jwkObj)) {
                        try {
                                string jwkJson;

                                if (jwkObj is Dictionary<string, object> jwkDict) {
                                        // Convert dictionary back to JSON
                                        jwkJson = JsonSerializer.Serialize(jwkDict);
                                }
                                else if (jwkObj is JsonElement jwkElement) {
                                        jwkJson = jwkElement.GetRawText();
                                }
                                else if (jwkObj is string jwkString) {
                                        jwkJson = jwkString;
                                }
                                else {
                                        throw new ProofValidationException($"Unsupported JWK format in header: {jwkObj.GetType()}");
                                }

                                var jwk = new JsonWebKey(jwkJson);
                                return jwk;
                        }
                        catch (Exception ex) {
                                throw new ProofValidationException("Failed to extract public key from JWK in header", ex);
                        }
                }

                // Try to get X.509 certificate chain from header
                if (header.TryGetValue("x5c", out var x5cObj)) {
                        try {
                                var certBase64 = ExtractFirstX5cEntry(x5cObj);
                                var certRaw = Convert.FromBase64String(certBase64);
#pragma warning disable SYSLIB0057
                                var cert = new X509Certificate2(certRaw);
#pragma warning restore SYSLIB0057
                                return new X509SecurityKey(cert);
                        }
                        catch (Exception ex) {
                                throw new ProofValidationException("Failed to extract public key from x5c in header", ex);
                        }
                }

                // Try to get key ID from header
                if (!string.IsNullOrEmpty(header.Kid)) {
                        if (keyResolver != null) {
                                var resolvedKey = keyResolver(header.Kid);
                                if (resolvedKey != null) {
                                        return resolvedKey;
                                }
                        }

                        throw new ProofValidationException(
                            $"JWT contains 'kid' header but key resolution failed for '{header.Kid}'. " +
                            "Provide a keyResolver callback or include 'jwk'/'x5c' header.");
                }

                throw new ProofValidationException("No public key found in JWT header. One of 'jwk', 'x5c', or resolvable 'kid' is required");
        }

        private static string ExtractFirstX5cEntry(object x5cObj) {
                if (x5cObj is string singleString) {
                        return singleString;
                }

                if (x5cObj is JsonElement element) {
                        if (element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0) {
                                var first = element[0];
                                if (first.ValueKind == JsonValueKind.String) {
                                        return first.GetString() ?? throw new InvalidOperationException("x5c first entry is null.");
                                }
                        }
                        else if (element.ValueKind == JsonValueKind.String) {
                                return element.GetString() ?? throw new InvalidOperationException("x5c string is null.");
                        }
                }

                if (x5cObj is IEnumerable<object> enumerable) {
                        foreach (var item in enumerable) {
                                if (item is string s && !string.IsNullOrWhiteSpace(s)) {
                                        return s;
                                }
                        }
                }

                throw new InvalidOperationException("x5c header must contain at least one Base64 DER certificate.");
        }
}
