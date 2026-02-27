using Microsoft.Extensions.Logging;
using SdJwt.Net.HAIP.Models;
using SdJwt.Net.HAIP.Validators;

namespace SdJwt.Net.HAIP.Extensions;

/// <summary>
/// Strongly typed OID4VCI option surface used by HAIP profile extensions.
/// </summary>
public interface IHaipOid4VciOptions {
        /// <summary>
        /// Gets or sets allowed signing algorithms.
        /// </summary>
        string[]? AllowedSigningAlgorithms { get; set; }

        /// <summary>
        /// Gets or sets forbidden signing algorithms.
        /// </summary>
        string[]? ForbiddenSigningAlgorithms { get; set; }

        /// <summary>
        /// Gets or sets whether proof-of-possession is required.
        /// </summary>
        bool RequireProofOfPossession { get; set; }

        /// <summary>
        /// Gets or sets whether secure transport is required.
        /// </summary>
        bool RequireSecureTransport { get; set; }

        /// <summary>
        /// Gets or sets whether PKCE is required.
        /// </summary>
        bool RequirePkce { get; set; }

        /// <summary>
        /// Gets or sets whether PAR is required.
        /// </summary>
        bool RequirePushedAuthorizationRequests { get; set; }

        /// <summary>
        /// Gets or sets whether DPoP or mTLS is required.
        /// </summary>
        bool RequireDpopOrMtls { get; set; }

        /// <summary>
        /// Gets or sets whether HSM-backed keys are required.
        /// </summary>
        bool RequireHardwareSecurityModule { get; set; }

        /// <summary>
        /// Gets or sets whether qualified electronic signatures are required.
        /// </summary>
        bool RequireQualifiedElectronicSignature { get; set; }

        /// <summary>
        /// Gets or sets allowed client authentication methods.
        /// </summary>
        string[]? ClientAuthenticationMethods { get; set; }

        /// <summary>
        /// Gets or sets whether wallet attestation is required.
        /// </summary>
        bool RequireWalletAttestation { get; set; }

        /// <summary>
        /// Gets or sets whether qualified wallet attestation is required.
        /// </summary>
        bool RequireQualifiedWalletAttestation { get; set; }

        /// <summary>
        /// Gets or sets whether compliance auditing is enabled.
        /// </summary>
        bool EnableComplianceAuditing { get; set; }

        /// <summary>
        /// Gets or sets auditing options.
        /// </summary>
        object? AuditingOptions { get; set; }
}

/// <summary>
/// Strongly typed OID4VP option surface used by HAIP profile extensions.
/// </summary>
public interface IHaipOid4VpOptions {
        /// <summary>
        /// Gets or sets the response mode.
        /// </summary>
        string? ResponseMode { get; set; }

        /// <summary>
        /// Gets or sets allowed client ID schemes.
        /// </summary>
        string[]? AllowedClientIdSchemes { get; set; }

        /// <summary>
        /// Gets or sets whether verifier attestation is required.
        /// </summary>
        bool RequireVerifierAttestation { get; set; }

        /// <summary>
        /// Gets or sets whether signed request objects are required.
        /// </summary>
        bool RequireSignedRequest { get; set; }

        /// <summary>
        /// Gets or sets whether qualified verifier attestation is required.
        /// </summary>
        bool RequireQualifiedVerifierAttestation { get; set; }
}

/// <summary>
/// Extension methods for integrating HAIP with OID4VCI
/// </summary>
public static class HaipOid4VciExtensions {
        /// <summary>
        /// Configures OID4VCI options for HAIP compliance
        /// </summary>
        /// <param name="options">OID4VCI configuration options</param>
        /// <param name="level">Required HAIP compliance level</param>
        /// <param name="config">Optional HAIP configuration (uses defaults if null)</param>
        public static void UseHaipProfile(this IHaipOid4VciOptions options, HaipLevel level, HaipConfiguration? config = null) {
                config ??= HaipConfiguration.GetDefault(level);

                ConfigureCryptographicRequirements(options, level);
                ConfigureProtocolSecurity(options, level);
                ConfigureClientAuthentication(options, level);
                ConfigureAuditing(options, config);
        }

        private static void ConfigureCryptographicRequirements(IHaipOid4VciOptions options, HaipLevel level) {
                // Configure allowed signing algorithms based on HAIP level
                var allowedAlgorithms = level switch {
                        HaipLevel.Level1_High => HaipConstants.Level1_Algorithms,
                        HaipLevel.Level2_VeryHigh => HaipConstants.Level2_Algorithms,
                        HaipLevel.Level3_Sovereign => HaipConstants.Level3_Algorithms,
                        _ => HaipConstants.Level1_Algorithms
                };

                options.AllowedSigningAlgorithms = allowedAlgorithms;
                options.ForbiddenSigningAlgorithms = HaipConstants.ForbiddenAlgorithms;
        }

        private static void ConfigureProtocolSecurity(IHaipOid4VciOptions options, HaipLevel level) {
                // Mandatory protocol security requirements
                options.RequireProofOfPossession = true;
                options.RequireSecureTransport = true;
                options.RequirePkce = true;

                // Level-specific requirements
                if (level >= HaipLevel.Level2_VeryHigh) {
                        options.RequirePushedAuthorizationRequests = true;
                        options.RequireDpopOrMtls = true;
                }

                if (level == HaipLevel.Level3_Sovereign) {
                        options.RequireHardwareSecurityModule = true;
                        options.RequireQualifiedElectronicSignature = true;
                }
        }

        private static void ConfigureClientAuthentication(IHaipOid4VciOptions options, HaipLevel level) {
                var allowedMethods = level switch {
                        HaipLevel.Level1_High => HaipConstants.ClientAuthMethods.Level1_Allowed,
                        HaipLevel.Level2_VeryHigh => HaipConstants.ClientAuthMethods.Level2_Required,
                        HaipLevel.Level3_Sovereign => HaipConstants.ClientAuthMethods.Level3_Required,
                        _ => HaipConstants.ClientAuthMethods.Level1_Allowed
                };

                options.ClientAuthenticationMethods = allowedMethods;

                if (level >= HaipLevel.Level2_VeryHigh) {
                        options.RequireWalletAttestation = true;
                }

                if (level == HaipLevel.Level3_Sovereign) {
                        options.RequireQualifiedWalletAttestation = true;
                }
        }

        private static void ConfigureAuditing(IHaipOid4VciOptions options, HaipConfiguration config) {
                options.EnableComplianceAuditing = true;
                options.AuditingOptions = config.AuditingOptions;
        }
}

/// <summary>
/// Extension methods for integrating HAIP with OID4VP
/// </summary>
public static class HaipOid4VpExtensions {
        /// <summary>
        /// Configures OID4VP presentation request options for HAIP compliance
        /// </summary>
        /// <param name="options">OID4VP presentation request options</param>
        /// <param name="level">Required HAIP compliance level</param>
        public static void EnforceHaip(this IHaipOid4VpOptions options, HaipLevel level) {
                // HAIP mandates signed response (JARM)
                options.ResponseMode = "direct_post.jwt";

                // Configure allowed client ID schemes based on HAIP requirements
                var allowedClientIdSchemes = new[]
                {
            "redirect_uri",
            "x509_san_dns",
            "verifier_attestation",
            "entity_id" // For federation scenarios
        };

                options.AllowedClientIdSchemes = allowedClientIdSchemes;

                // Level-specific requirements
                if (level >= HaipLevel.Level2_VeryHigh) {
                        options.RequireVerifierAttestation = true;
                        options.RequireSignedRequest = true;
                }

                if (level == HaipLevel.Level3_Sovereign) {
                        options.RequireQualifiedVerifierAttestation = true;
                        // Restrict to only the most secure client ID schemes
                        options.AllowedClientIdSchemes = new[] { "verifier_attestation", "x509_san_dns" };
                }
        }
}

/// <summary>
/// HAIP validation service for runtime compliance checking
/// </summary>
public class HaipValidationService {
        private readonly IHaipCryptoValidator _cryptoValidator;
        private readonly ILogger<HaipValidationService> _logger;
        private readonly HaipConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="HaipValidationService"/> class
        /// </summary>
        /// <param name="cryptoValidator">The cryptographic validator for HAIP compliance</param>
        /// <param name="config">The HAIP configuration</param>
        /// <param name="logger">The logger instance</param>
        public HaipValidationService(
            IHaipCryptoValidator cryptoValidator,
            HaipConfiguration config,
            ILogger<HaipValidationService> logger) {
                _cryptoValidator = cryptoValidator;
                _config = config;
                _logger = logger;
        }

        /// <summary>
        /// Validates a credential issuance request for HAIP compliance
        /// </summary>
        /// <param name="context">The validation context containing request details</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>A task that represents the asynchronous validation operation. The task result contains the compliance result.</returns>
        public async Task<HaipComplianceResult> ValidateIssuanceRequestAsync(
            HaipIssuanceValidationContext context,
            CancellationToken cancellationToken = default) {
                var result = new HaipComplianceResult();
                result.AuditTrail.ValidatorId = nameof(HaipValidationService);
                result.AuditTrail.AddStep("Starting issuance validation", true);

                try {
                        // Validate cryptographic requirements
                        if (context.SigningKey != null && context.SigningAlgorithm != null) {
                                var cryptoResult = _cryptoValidator.ValidateKeyCompliance(context.SigningKey, context.SigningAlgorithm);
                                result.Violations.AddRange(cryptoResult.Violations);
                                result.AuditTrail.Steps.AddRange(cryptoResult.AuditTrail.Steps);
                        }

                        // Validate protocol security
                        await ValidateProtocolSecurity(context, result, cancellationToken);

                        // Validate trust framework (if configured)
                        if (_config.TrustFrameworks.Any()) {
                                await ValidateTrustFramework(context, result, cancellationToken);
                        }

                        result.IsCompliant = !result.Violations.Any(v => v.Severity == HaipSeverity.Critical);
                        result.AuditTrail.Complete();

                        return result;
                }
                catch (Exception ex) {
                        _logger.LogError(ex, "Error during HAIP validation");
                        result.AddViolation($"Validation error: {ex.Message}", HaipViolationType.InsufficientAssuranceLevel);
                        result.AuditTrail.Complete();
                        return result;
                }
        }

        private async Task ValidateProtocolSecurity(
            HaipIssuanceValidationContext context,
            HaipComplianceResult result,
            CancellationToken cancellationToken) {
                // Validate proof of possession
                if (!context.HasProofOfPossession) {
                        result.AddViolation(
                            "Proof of possession is required for HAIP compliance",
                            HaipViolationType.MissingProofOfPossession);
                }

                // Validate secure transport
                if (!context.IsSecureTransport) {
                        result.AddViolation(
                            "Secure transport (HTTPS) is required for HAIP compliance",
                            HaipViolationType.InsecureTransport);
                }

                // Level-specific validations
                if (_config.RequiredLevel >= HaipLevel.Level2_VeryHigh) {
                        if (!context.HasWalletAttestation) {
                                result.AddViolation(
                                    "Wallet attestation is required for HAIP Level 2+",
                                    HaipViolationType.InsecureClientAuthentication);
                        }
                }

                result.AuditTrail.AddStep("Protocol security validation",
                    !result.Violations.Any(v => v.Severity == HaipSeverity.Critical));

                await Task.CompletedTask; // Placeholder for async operations
        }

        private async Task ValidateTrustFramework(
            HaipIssuanceValidationContext context,
            HaipComplianceResult result,
            CancellationToken cancellationToken) {
                // This would integrate with the existing OpenID Federation implementation
                // to validate trust chains according to configured trust frameworks

                foreach (var framework in _config.TrustFrameworks) {
                        // Placeholder for trust framework validation
                        // Would use SdJwt.Net.OidFederation for actual validation
                        result.AuditTrail.AddStep($"Trust framework validation: {framework}", true);
                }

                await Task.CompletedTask;
        }
}

/// <summary>
/// Context for HAIP validation containing request details and security requirements
/// </summary>
public class HaipIssuanceValidationContext {
        /// <summary>
        /// Gets or sets the signing algorithm used for the credential
        /// </summary>
        public string? SigningAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the signing key used for the credential
        /// </summary>
        public Microsoft.IdentityModel.Tokens.SecurityKey? SigningKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether proof of possession is present
        /// </summary>
        public bool HasProofOfPossession { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether secure transport (HTTPS) is being used
        /// </summary>
        public bool IsSecureTransport { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether wallet attestation is present
        /// </summary>
        public bool HasWalletAttestation { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the credential issuer
        /// </summary>
        public string? IssuerIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the client identifier making the request
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Gets or sets additional context information for validation
        /// </summary>
        public Dictionary<string, object> AdditionalContext { get; set; } = new();
}
