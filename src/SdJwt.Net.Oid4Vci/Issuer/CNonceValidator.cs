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
/// Options controlling proof JWT validation behavior.
/// </summary>
public sealed class ProofValidationOptions {
        /// <summary>
        /// Gets or sets an optional key resolver used when the JWT header contains <c>kid</c>.
        /// </summary>
        public Func<string, SecurityKey?>? KeyResolver { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <c>x5c</c> certificate chains are validated against trusted roots.
        /// </summary>
        public bool ValidateX5cChain { get; set; }

        /// <summary>
        /// Gets or sets trusted root certificates used for <c>x5c</c> chain anchoring.
        /// </summary>
        public IReadOnlyCollection<X509Certificate2> TrustedRootCertificates { get; set; } = Array.Empty<X509Certificate2>();

        /// <summary>
        /// Gets or sets a value indicating whether key attestation is required in JWT headers.
        /// </summary>
        public bool RequireKeyAttestation { get; set; }

        /// <summary>
        /// Gets or sets an optional attestation validator callback.
        /// </summary>
        public Func<string, JwtSecurityToken, JwtHeader, bool>? KeyAttestationValidator { get; set; }

        /// <summary>
        /// Gets or sets header names checked for key attestation payloads.
        /// </summary>
        public IReadOnlyCollection<string> AttestationHeaderNames { get; set; } =
            new[] { "key_attestation", "attestation", "wallet_attestation" };
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
                return ValidateProof(jwtString, expectedCNonce, expectedIssuerUrl, new ProofValidationOptions(), clockSkew);
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
                var options = new ProofValidationOptions {
                        KeyResolver = keyResolver
                };
                return ValidateProof(jwtString, expectedCNonce, expectedIssuerUrl, options, clockSkew);
        }

        /// <summary>
        /// Validates a proof of possession JWT with explicit validation options.
        /// </summary>
        /// <param name="jwtString">The proof JWT string.</param>
        /// <param name="expectedCNonce">The expected nonce value.</param>
        /// <param name="expectedIssuerUrl">The expected audience (issuer URL).</param>
        /// <param name="options">Proof validation options.</param>
        /// <param name="clockSkew">Optional clock skew tolerance (default: 5 minutes).</param>
        /// <returns>Proof validation result containing the public key and claims.</returns>
        /// <exception cref="ProofValidationException">Thrown when proof validation fails.</exception>
        public static ProofValidationResult ValidateProof(
            string jwtString,
            string expectedCNonce,
            string expectedIssuerUrl,
            ProofValidationOptions options,
            TimeSpan? clockSkew = null) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(jwtString, nameof(jwtString));
                ArgumentException.ThrowIfNullOrWhiteSpace(expectedCNonce, nameof(expectedCNonce));
                ArgumentException.ThrowIfNullOrWhiteSpace(expectedIssuerUrl, nameof(expectedIssuerUrl));
                ArgumentNullException.ThrowIfNull(options);
#else
        if (string.IsNullOrWhiteSpace(jwtString))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(jwtString));
        if (string.IsNullOrWhiteSpace(expectedCNonce))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(expectedCNonce));
        if (string.IsNullOrWhiteSpace(expectedIssuerUrl))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(expectedIssuerUrl));
        if (options == null)
            throw new ArgumentNullException(nameof(options));
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
                        var keyMaterial = ExtractPublicKeyFromHeader(token.Header, options.KeyResolver);

                        if (options.ValidateX5cChain) {
                                ValidateX5cCertificateChain(keyMaterial.X5cChain, options.TrustedRootCertificates);
                        }

                        ValidateAttestation(token, options);

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
                                IssuerSigningKey = keyMaterial.PublicKey,
                                ClockSkew = tolerance
                        };

                        tokenHandler.ValidateToken(jwtString, validationParameters, out var validatedToken);

                        return new ProofValidationResult(
                            keyMaterial.PublicKey,
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

        private static void ValidateAttestation(JwtSecurityToken token, ProofValidationOptions options) {
                string? attestation = null;
                foreach (var headerName in options.AttestationHeaderNames) {
                        if (token.Header.TryGetValue(headerName, out var value) &&
                            TryExtractString(value, out var extracted) &&
                            !string.IsNullOrWhiteSpace(extracted)) {
                                attestation = extracted;
                                break;
                        }
                }

                if (options.RequireKeyAttestation && string.IsNullOrWhiteSpace(attestation)) {
                        throw new ProofValidationException("Key attestation is required but missing from proof JWT header.");
                }

                if (options.KeyAttestationValidator != null) {
                        if (string.IsNullOrWhiteSpace(attestation)) {
                                throw new ProofValidationException("Key attestation validator configured but no attestation was provided.");
                        }

                        if (!options.KeyAttestationValidator(attestation!, token, token.Header)) {
                                throw new ProofValidationException("Key attestation validation failed.");
                        }
                }
        }

        private static void ValidateX5cCertificateChain(
            IReadOnlyList<X509Certificate2>? chainCertificates,
            IReadOnlyCollection<X509Certificate2> trustedRoots) {
                if (chainCertificates == null || chainCertificates.Count == 0) {
                        throw new ProofValidationException("x5c certificate chain validation requested but no x5c certificates were provided.");
                }

                if (trustedRoots == null || trustedRoots.Count == 0) {
                        throw new ProofValidationException("x5c certificate chain validation requires at least one trusted root certificate.");
                }

                var trustedThumbprints = new HashSet<string>(
                    trustedRoots
                        .Where(c => !string.IsNullOrWhiteSpace(c.Thumbprint))
                        .Select(c => c.Thumbprint!),
                    StringComparer.OrdinalIgnoreCase);

                var leaf = chainCertificates[0];
                if (chainCertificates.Count == 1 &&
                    IsSelfSigned(leaf) &&
                    !string.IsNullOrWhiteSpace(leaf.Thumbprint) &&
                    trustedThumbprints.Contains(leaf.Thumbprint)) {
                        return;
                }

                using var chain = new X509Chain();
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                foreach (var certificate in chainCertificates.Skip(1)) {
                        chain.ChainPolicy.ExtraStore.Add(certificate);
                }
                foreach (var root in trustedRoots) {
                        chain.ChainPolicy.ExtraStore.Add(root);
                }

                var built = chain.Build(leaf);
                if (!built) {
                        var nonRootStatuses = chain.ChainStatus
                            .Select(s => s.Status)
                            .Where(status => status != X509ChainStatusFlags.UntrustedRoot)
                            .ToArray();

                        if (nonRootStatuses.Length > 0) {
                                throw new ProofValidationException("x5c certificate chain validation failed.");
                        }
                }

                var chainRootThumbprint = chain.ChainElements.Count > 0
                    ? chain.ChainElements[chain.ChainElements.Count - 1].Certificate.Thumbprint
                    : null;
                var anchoredToTrustedRoot =
                    !string.IsNullOrWhiteSpace(chainRootThumbprint) &&
                    trustedThumbprints.Contains(chainRootThumbprint);

                if (!anchoredToTrustedRoot &&
                    !string.IsNullOrWhiteSpace(leaf.Thumbprint) &&
                    trustedThumbprints.Contains(leaf.Thumbprint)) {
                        anchoredToTrustedRoot = true;
                }

                if (!anchoredToTrustedRoot) {
                        throw new ProofValidationException("x5c certificate chain is not anchored to a configured trusted root.");
                }
        }

        private static bool IsSelfSigned(X509Certificate2 certificate) {
                return string.Equals(certificate.SubjectName.Name, certificate.IssuerName.Name, StringComparison.Ordinal);
        }

        private static HeaderKeyMaterial ExtractPublicKeyFromHeader(JwtHeader header, Func<string, SecurityKey?>? keyResolver) {
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
                                return new HeaderKeyMaterial(jwk, null);
                        }
                        catch (Exception ex) {
                                throw new ProofValidationException("Failed to extract public key from JWK in header", ex);
                        }
                }

                // Try to get X.509 certificate chain from header
                if (header.TryGetValue("x5c", out var x5cObj)) {
                        try {
                                var chain = ExtractX5cChain(x5cObj);
                                return new HeaderKeyMaterial(new X509SecurityKey(chain[0]), chain);
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
                                        return new HeaderKeyMaterial(resolvedKey, null);
                                }
                        }

                        throw new ProofValidationException(
                            $"JWT contains 'kid' header but key resolution failed for '{header.Kid}'. " +
                            "Provide a keyResolver callback or include 'jwk'/'x5c' header.");
                }

                throw new ProofValidationException("No public key found in JWT header. One of 'jwk', 'x5c', or resolvable 'kid' is required");
        }

        private static IReadOnlyList<X509Certificate2> ExtractX5cChain(object x5cObj) {
                var encodedEntries = new List<string>();

                if (x5cObj is JsonElement element) {
                        if (element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0) {
                                foreach (var item in element.EnumerateArray()) {
                                        if (item.ValueKind == JsonValueKind.String) {
                                                var value = item.GetString();
                                                if (!string.IsNullOrWhiteSpace(value)) {
                                                        encodedEntries.Add(value!);
                                                }
                                        }
                                }
                        }
                        else if (element.ValueKind == JsonValueKind.String) {
                                var value = element.GetString();
                                if (!string.IsNullOrWhiteSpace(value)) {
                                        encodedEntries.Add(value!);
                                }
                        }
                }
                else if (x5cObj is string singleString && !string.IsNullOrWhiteSpace(singleString)) {
                        encodedEntries.Add(singleString);
                }

                if (x5cObj is IEnumerable<object> enumerable) {
                        foreach (var item in enumerable) {
                                if (item is string s && !string.IsNullOrWhiteSpace(s)) {
                                        encodedEntries.Add(s);
                                }
                        }
                }

                if (encodedEntries.Count == 0) {
                        throw new InvalidOperationException("x5c header must contain at least one Base64 DER certificate.");
                }

                var certificates = new List<X509Certificate2>(encodedEntries.Count);
                foreach (var entry in encodedEntries) {
                        var certRaw = Convert.FromBase64String(entry);
#pragma warning disable SYSLIB0057
                        certificates.Add(new X509Certificate2(certRaw));
#pragma warning restore SYSLIB0057
                }

                return certificates;
        }

        private static bool TryExtractString(object value, out string? extracted) {
                extracted = null;

                switch (value) {
                        case string text:
                                extracted = text;
                                return true;
                        case JsonElement element when element.ValueKind == JsonValueKind.String:
                                extracted = element.GetString();
                                return true;
                        default:
                                return false;
                }
        }

        private sealed class HeaderKeyMaterial {
                public HeaderKeyMaterial(SecurityKey publicKey, IReadOnlyList<X509Certificate2>? x5cChain) {
                        PublicKey = publicKey;
                        X5cChain = x5cChain;
                }

                public SecurityKey PublicKey { get; }

                public IReadOnlyList<X509Certificate2>? X5cChain { get; }
        }
}
