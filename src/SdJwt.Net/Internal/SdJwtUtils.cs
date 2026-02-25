using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SdJwt.Net.Internal;

/// <summary>  
/// Utility functions for SD-JWT processing. This class is not part of the public API.  
/// </summary>  
public static class SdJwtUtils {
        /// <summary>
        /// Set of cryptographically weak hash algorithms that should not be used for SD-JWT.
        /// These are blocked for security reasons even if they exist in the framework.
        /// </summary>
        private static readonly HashSet<string> BlockedAlgorithms = new(StringComparer.OrdinalIgnoreCase)
        {
        "MD5", "SHA-1", "SHA1" // Blocked weak algorithms
    };

        /// <summary>
        /// Set of RFC 9901 approved hash algorithms for SD-JWT.
        /// Currently focusing on the SHA-2 family as required by the specification.
        /// </summary>
        private static readonly HashSet<string> ApprovedAlgorithms = new(StringComparer.OrdinalIgnoreCase)
        {
        "SHA-256", "SHA-384", "SHA-512"
    };

        /// <summary>  
        /// Creates a cryptographically secure salt value, encoded as Base64Url.  
        /// </summary>  
        /// <returns>A Base64Url encoded salt string.</returns>  
        public static string GenerateSalt() {
                var saltBytes = new byte[16];
#if NET6_0_OR_GREATER
                RandomNumberGenerator.Fill(saltBytes);
#else
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
#endif
                return Base64UrlEncoder.Encode(saltBytes);
        }

        /// <summary>
        /// Validates that a parameter is not null or whitespace (compatibility helper).
        /// </summary>
        private static void ThrowIfNullOrWhiteSpace(string? value, string paramName) {
                if (string.IsNullOrWhiteSpace(value)) {
#if NET6_0_OR_GREATER
                        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
#else
            if (value is null)
                throw new ArgumentNullException(paramName);
            
            throw new ArgumentException("Value cannot be empty or whitespace.", paramName);
#endif
                }
        }

        /// <summary>  
        /// Maps an IANA-registered hash algorithm name to the .NET <see cref="HashAlgorithmName"/>.
        /// Only allows cryptographically secure algorithms approved for SD-JWT use.
        /// </summary>  
        private static HashAlgorithmName GetHashAlgorithmName(string hashAlgorithm) {
                ThrowIfNullOrWhiteSpace(hashAlgorithm, nameof(hashAlgorithm));

                var normalizedAlgorithm = hashAlgorithm.ToUpperInvariant();

                // Block weak algorithms explicitly
                if (BlockedAlgorithms.Contains(normalizedAlgorithm)) {
                        throw new NotSupportedException($"Hash algorithm '{hashAlgorithm}' is cryptographically weak and not supported for SD-JWT. Use SHA-256 or stronger.");
                }

                // Only allow approved algorithms
                if (!ApprovedAlgorithms.Contains(normalizedAlgorithm)) {
                        throw new NotSupportedException($"Hash algorithm '{hashAlgorithm}' is not approved for SD-JWT use. Supported algorithms: {string.Join(", ", ApprovedAlgorithms)}");
                }

                return normalizedAlgorithm switch {
                        "SHA-256" => HashAlgorithmName.SHA256,
                        "SHA-384" => HashAlgorithmName.SHA384,
                        "SHA-512" => HashAlgorithmName.SHA512,
                        _ => throw new NotSupportedException($"Hash algorithm '{hashAlgorithm}' mapping is not implemented.")
                };
        }

        /// <summary>
        /// Computes hash using the appropriate method for the target framework.
        /// </summary>
        private static byte[] ComputeHash(HashAlgorithmName algorithmName, byte[] data) {
#if NET6_0_OR_GREATER
                // Use static methods for better performance on modern .NET
                return algorithmName.Name switch {
                        nameof(HashAlgorithmName.SHA256) => SHA256.HashData(data),
                        nameof(HashAlgorithmName.SHA384) => SHA384.HashData(data),
                        nameof(HashAlgorithmName.SHA512) => SHA512.HashData(data),
                        _ => throw new NotSupportedException($"Hash algorithm '{algorithmName.Name}' is not supported.")
                };
#else
        // Use traditional Create() pattern for .NET Standard 2.1 compatibility
        using HashAlgorithm hashAlgorithm = algorithmName.Name switch
        {
            nameof(HashAlgorithmName.SHA256) => SHA256.Create(),
            nameof(HashAlgorithmName.SHA384) => SHA384.Create(),
            nameof(HashAlgorithmName.SHA512) => SHA512.Create(),
            _ => throw new NotSupportedException($"Hash algorithm '{algorithmName.Name}' is not supported.")
        };
        
        return hashAlgorithm.ComputeHash(data);
#endif
        }

        /// <summary>  
        /// Calculates the digest of a disclosure according to the specified hash algorithm.
        /// Uses approved cryptographically secure algorithms only.
        /// </summary>  
        /// <param name="hashAlgorithm">The IANA name of the hash algorithm (e.g., "SHA-256").</param>  
        /// <param name="encodedDisclosure">The Base64Url encoded disclosure string.</param>  
        /// <returns>A Base64Url encoded digest string.</returns>  
        public static string CreateDigest(string hashAlgorithm, string encodedDisclosure) {
                ThrowIfNullOrWhiteSpace(hashAlgorithm, nameof(hashAlgorithm));
                ThrowIfNullOrWhiteSpace(encodedDisclosure, nameof(encodedDisclosure));

                var algorithmName = GetHashAlgorithmName(hashAlgorithm);
                var disclosureBytes = Encoding.ASCII.GetBytes(encodedDisclosure);

                var digestBytes = ComputeHash(algorithmName, disclosureBytes);
                return Base64UrlEncoder.Encode(digestBytes);
        }

        /// <summary>
        /// Validates that the given hash algorithm is approved for SD-JWT use.
        /// </summary>
        /// <param name="hashAlgorithm">The hash algorithm to validate.</param>
        /// <returns>True if the algorithm is approved, false otherwise.</returns>
        public static bool IsApprovedHashAlgorithm(string? hashAlgorithm) {
                if (string.IsNullOrWhiteSpace(hashAlgorithm))
                        return false;

                return ApprovedAlgorithms.Contains(hashAlgorithm);
        }

        /// <summary>  
        /// A helper method to convert a <see cref="JsonElement"/> to a primitive .NET type,  
        /// a dictionary, or a list.  
        /// </summary>  
        public static object ConvertJsonElement(JsonElement element) {
                return element.ValueKind switch {
                        JsonValueKind.String => element.GetString()!,
                        JsonValueKind.Number => element.GetDecimal(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null!,
                        JsonValueKind.Object => element.Deserialize<Dictionary<string, object>>(SdJwtConstants.DefaultJsonSerializerOptions)!,
                        JsonValueKind.Array => element.Deserialize<List<object>>(SdJwtConstants.DefaultJsonSerializerOptions)!,
                        _ => throw new InvalidOperationException($"Unsupported JsonValueKind: {element.ValueKind}")
                };
        }
}