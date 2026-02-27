using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SdJwt.Net.Vc.Metadata;

/// <summary>
/// Validates integrity metadata strings as defined by W3C Subresource Integrity.
/// </summary>
public static class IntegrityMetadataValidator {
        /// <summary>
        /// Validates an integrity metadata value against UTF-8 encoded content.
        /// </summary>
        /// <param name="content">The content to validate.</param>
        /// <param name="integrityMetadata">Integrity metadata (e.g., <c>sha-256-...</c>).</param>
        /// <returns><see langword="true"/> when at least one digest matches; otherwise <see langword="false"/>.</returns>
        public static bool Validate(string content, string integrityMetadata) {
                if (content == null) {
                        throw new ArgumentNullException(nameof(content));
                }
                if (string.IsNullOrWhiteSpace(integrityMetadata)) {
                        throw new ArgumentException("Value cannot be null or whitespace.", nameof(integrityMetadata));
                }

                return Validate(Encoding.UTF8.GetBytes(content), integrityMetadata);
        }

        /// <summary>
        /// Validates an integrity metadata value against binary content.
        /// </summary>
        /// <param name="content">The content to validate.</param>
        /// <param name="integrityMetadata">Integrity metadata (e.g., <c>sha-256-...</c>).</param>
        /// <returns><see langword="true"/> when at least one digest matches; otherwise <see langword="false"/>.</returns>
        public static bool Validate(byte[] content, string integrityMetadata) {
                if (content == null) {
                        throw new ArgumentNullException(nameof(content));
                }
                if (string.IsNullOrWhiteSpace(integrityMetadata)) {
                        throw new ArgumentException("Value cannot be null or whitespace.", nameof(integrityMetadata));
                }

                foreach (var token in integrityMetadata.Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
                        var (algorithmToken, hashBase64) = ParseToken(token);
                        if (algorithmToken == null || hashBase64 == null) {
                                continue;
                        }

                        var expectedHash = DecodeBase64(hashBase64);
                        if (expectedHash == null) {
                                continue;
                        }

                        var computedHash = ComputeHash(content, algorithmToken);
                        if (computedHash == null || computedHash.Length != expectedHash.Length) {
                                continue;
                        }

                        if (CryptographicOperations.FixedTimeEquals(computedHash, expectedHash)) {
                                return true;
                        }
                }

                return false;
        }

        private static (string? AlgorithmToken, string? HashBase64) ParseToken(string token) {
                if (token.StartsWith("sha-256-", StringComparison.OrdinalIgnoreCase)) {
                        return ("sha-256", token.Substring("sha-256-".Length));
                }

                if (token.StartsWith("sha-384-", StringComparison.OrdinalIgnoreCase)) {
                        return ("sha-384", token.Substring("sha-384-".Length));
                }

                if (token.StartsWith("sha-512-", StringComparison.OrdinalIgnoreCase)) {
                        return ("sha-512", token.Substring("sha-512-".Length));
                }

                if (token.StartsWith("sha256-", StringComparison.OrdinalIgnoreCase)) {
                        return ("sha256", token.Substring("sha256-".Length));
                }

                if (token.StartsWith("sha384-", StringComparison.OrdinalIgnoreCase)) {
                        return ("sha384", token.Substring("sha384-".Length));
                }

                if (token.StartsWith("sha512-", StringComparison.OrdinalIgnoreCase)) {
                        return ("sha512", token.Substring("sha512-".Length));
                }

                return (null, null);
        }

        private static byte[]? ComputeHash(byte[] content, string algorithmToken) {
                return algorithmToken.ToLowerInvariant() switch
                {
                    "sha256" or "sha-256" => Compute(SHA256.Create(), content),
                    "sha384" or "sha-384" => Compute(SHA384.Create(), content),
                    "sha512" or "sha-512" => Compute(SHA512.Create(), content),
                    _ => null
                };
        }

        private static byte[] Compute(HashAlgorithm algorithm, byte[] content) {
                using (algorithm) {
                        return algorithm.ComputeHash(content);
                }
        }

        private static byte[]? DecodeBase64(string value) {
                try {
                        return Convert.FromBase64String(value);
                }
                catch {
                        return null;
                }
        }
}
