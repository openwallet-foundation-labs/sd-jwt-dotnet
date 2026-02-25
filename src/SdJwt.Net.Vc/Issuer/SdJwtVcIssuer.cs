using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Vc.Models;
using System.IdentityModel.Tokens.Jwt;

namespace SdJwt.Net.Vc.Issuer;

/// <summary>
/// A specialized issuer for creating SD-JWT-based Verifiable Credentials (SD-JWT VCs),
/// compliant with draft-ietf-oauth-sd-jwt-vc-13 specification.
/// Note: This specification does not utilize the W3C Verifiable Credentials Data Model.
/// </summary>
/// <param name="signingKey">The security key to sign the SD-JWT.</param>
/// <param name="signingAlgorithm">The JWT signing algorithm (e.g., "ES256", "EdDSA").</param>
/// <param name="hashAlgorithm">The hashing algorithm for disclosures (e.g., "sha-256").</param>
/// <param name="logger">An optional logger for diagnostics.</param>
public class SdJwtVcIssuer(
    SecurityKey signingKey,
    string signingAlgorithm,
    string hashAlgorithm = SdJwtConstants.DefaultHashAlgorithm,
    ILogger<SdIssuer>? logger = null) {
        private readonly SdIssuer _sdIssuer = new(signingKey, signingAlgorithm, hashAlgorithm, logger);

        /// <summary>
        /// Issues a new SD-JWT VC according to draft-ietf-oauth-sd-jwt-vc-13.
        /// </summary>
        /// <param name="vctIdentifier">The verifiable credential type identifier (vct claim).</param>
        /// <param name="payload">The SD-JWT VC payload containing claims.</param>
        /// <param name="options">Options for defining which claims are selectively disclosable.</param>
        /// <param name="holderPublicKey">Optional Holder public key for key binding.</param>
        /// <returns>An <see cref="IssuerOutput"/> containing the full SD-JWT VC.</returns>
        public IssuerOutput Issue(
            string vctIdentifier,
            SdJwtVcPayload payload,
            SdIssuanceOptions options,
            Microsoft.IdentityModel.Tokens.JsonWebKey? holderPublicKey = null) {
                if (string.IsNullOrWhiteSpace(vctIdentifier))
                        throw new ArgumentException("VCT identifier cannot be null or empty", nameof(vctIdentifier));
                if (payload == null)
                        throw new ArgumentNullException(nameof(payload));
                if (options == null)
                        throw new ArgumentNullException(nameof(options));

                // Validate payload according to draft-13
                ValidateSdJwtVcPayload(payload, vctIdentifier);

                // Create JWT payload with required structure
                var jwtPayload = new JwtPayload();

                // Set required vct claim (cannot be selectively disclosed)
                jwtPayload["vct"] = vctIdentifier;

                // Set vct#integrity if provided
                if (!string.IsNullOrEmpty(payload.VctIntegrity))
                        jwtPayload["vct#integrity"] = payload.VctIntegrity;

                // Add standard JWT claims that cannot be selectively disclosed
                if (!string.IsNullOrEmpty(payload.Issuer))
                        jwtPayload[JwtRegisteredClaimNames.Iss] = payload.Issuer;

                if (payload.NotBefore.HasValue)
                        jwtPayload[JwtRegisteredClaimNames.Nbf] = payload.NotBefore.Value;

                if (payload.ExpiresAt.HasValue)
                        jwtPayload[JwtRegisteredClaimNames.Exp] = payload.ExpiresAt.Value;

                if (payload.Confirmation != null)
                        jwtPayload["cnf"] = payload.Confirmation;

                if (payload.Status != null)
                        jwtPayload["status"] = payload.Status;

                // Add claims that can be selectively disclosed
                if (!string.IsNullOrEmpty(payload.Subject))
                        jwtPayload[JwtRegisteredClaimNames.Sub] = payload.Subject;

                if (payload.IssuedAt.HasValue)
                        jwtPayload[JwtRegisteredClaimNames.Iat] = payload.IssuedAt.Value;

                // Add any additional data
                if (payload.AdditionalData != null) {
                        foreach (var kvp in payload.AdditionalData) {
                                jwtPayload[kvp.Key] = kvp.Value;
                        }
                }

                return _sdIssuer.Issue(jwtPayload, options, holderPublicKey, SdJwtConstants.SdJwtVcTypeName);
        }

        /// <summary>
        /// Issues a new SD-JWT VC with simplified parameters.
        /// </summary>
        /// <param name="vctIdentifier">The verifiable credential type identifier.</param>
        /// <param name="issuer">The issuer identifier (URI).</param>
        /// <param name="subject">The subject identifier (URI).</param>
        /// <param name="claims">The claims to include in the credential.</param>
        /// <param name="options">Disclosure options.</param>
        /// <param name="validFrom">Optional not-before time.</param>
        /// <param name="validUntil">Optional expiration time.</param>
        /// <param name="holderPublicKey">Optional holder public key for key binding.</param>
        /// <param name="statusReference">Optional status information.</param>
        /// <param name="vctIntegrity">Optional VCT integrity hash.</param>
        /// <returns>An <see cref="IssuerOutput"/> containing the full SD-JWT VC.</returns>
        public IssuerOutput IssueSimple(
            string vctIdentifier,
            string issuer,
            string subject,
            Dictionary<string, object> claims,
            SdIssuanceOptions options,
            DateTime? validFrom = null,
            DateTime? validUntil = null,
            Microsoft.IdentityModel.Tokens.JsonWebKey? holderPublicKey = null,
            object? statusReference = null,
            string? vctIntegrity = null) {
                if (string.IsNullOrWhiteSpace(issuer))
                        throw new ArgumentException("Issuer cannot be null or empty", nameof(issuer));
                if (string.IsNullOrWhiteSpace(subject))
                        throw new ArgumentException("Subject cannot be null or empty", nameof(subject));
                if (claims == null || claims.Count == 0)
                        throw new ArgumentException("Claims cannot be null or empty", nameof(claims));

                var payload = new SdJwtVcPayload {
                        VctIntegrity = vctIntegrity,
                        Issuer = issuer,
                        Subject = subject,
                        IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        NotBefore = validFrom.HasValue ? ((DateTimeOffset)validFrom.Value).ToUnixTimeSeconds() : null,
                        ExpiresAt = validUntil.HasValue ? ((DateTimeOffset)validUntil.Value).ToUnixTimeSeconds() : null,
                        Status = statusReference,
                        AdditionalData = claims
                };

                return Issue(vctIdentifier, payload, options, holderPublicKey);
        }

        /// <summary>
        /// Creates a key binding configuration for the credential.
        /// </summary>
        /// <param name="holderPublicKey">The holder's public key.</param>
        /// <returns>Confirmation method object for the cnf claim.</returns>
        public ConfirmationMethod CreateKeyBindingConfiguration(Microsoft.IdentityModel.Tokens.JsonWebKey holderPublicKey) {
                if (holderPublicKey == null)
                        throw new ArgumentNullException(nameof(holderPublicKey));

                return new ConfirmationMethod {
                        Jwk = holderPublicKey
                };
        }

        /// <summary>
        /// Creates a status reference for the credential using the Status List mechanism.
        /// This method requires the StatusList package to be referenced.
        /// </summary>
        /// <param name="statusListUri">The URI of the Status List Token.</param>
        /// <param name="index">The index of this credential in the status list.</param>
        /// <returns>Status claim object for the status claim.</returns>
        public object CreateStatusReference(string statusListUri, int index) {
                if (string.IsNullOrWhiteSpace(statusListUri))
                        throw new ArgumentException("Status list URI cannot be null or empty", nameof(statusListUri));
                if (index < 0)
                        throw new ArgumentException("Index must be non-negative", nameof(index));

                // Return a simple object structure that matches the StatusClaim structure
                return new {
                        status_list = new {
                                uri = statusListUri,
                                idx = index
                        }
                };
        }

        /// <summary>
        /// Validates the SD-JWT VC payload according to draft-ietf-oauth-sd-jwt-vc-13.
        /// </summary>
        private static void ValidateSdJwtVcPayload(SdJwtVcPayload payload, string vctIdentifier) {
                // VCT claim validation
                if (!string.IsNullOrEmpty(payload.Vct) && payload.Vct != vctIdentifier)
                        throw new ArgumentException($"Payload vct claim '{payload.Vct}' does not match provided VCT identifier '{vctIdentifier}'");

                // Validate VCT identifier is a Collision-Resistant Name (RFC7515 Section 2)
                if (!IsValidCollisionResistantName(vctIdentifier))
                        throw new ArgumentException($"VCT identifier '{vctIdentifier}' is not a valid Collision-Resistant Name");

                // Validate confirmation method if present
                if (payload.Confirmation != null) {
                        // Basic validation - more detailed validation would be implementation-specific
                        var confirmationJson = System.Text.Json.JsonSerializer.Serialize(payload.Confirmation);
                        if (!confirmationJson.Contains("jwk") && !confirmationJson.Contains("jku") && !confirmationJson.Contains("kid"))
                                throw new ArgumentException("Confirmation method must contain at least one of: jwk, jku, kid");
                }

                // Validate timing claims
                if (payload.NotBefore.HasValue && payload.ExpiresAt.HasValue &&
                    payload.NotBefore.Value >= payload.ExpiresAt.Value) {
                        throw new ArgumentException("Not-before time must be before expiration time");
                }

                if (payload.IssuedAt.HasValue && payload.ExpiresAt.HasValue &&
                    payload.IssuedAt.Value >= payload.ExpiresAt.Value) {
                        throw new ArgumentException("Issued-at time must be before expiration time");
                }
        }

        /// <summary>
        /// Validates if a string is a valid Collision-Resistant Name according to RFC7515.
        /// Simplified validation - checks for URI format.
        /// </summary>
        private static bool IsValidCollisionResistantName(string name) {
                if (string.IsNullOrWhiteSpace(name))
                        return false;

                // Must be a URI or contain a namespace separator (':')
                return Uri.TryCreate(name, UriKind.Absolute, out _) || name.Contains(':');
        }
}