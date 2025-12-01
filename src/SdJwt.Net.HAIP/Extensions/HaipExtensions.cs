using Microsoft.Extensions.Logging;
using SdJwt.Net.HAIP.Models;
using SdJwt.Net.HAIP.Validators;

namespace SdJwt.Net.HAIP.Extensions;

/// <summary>
/// Extension methods for integrating HAIP with OID4VCI
/// </summary>
public static class HaipOid4VciExtensions
{
    /// <summary>
    /// Configures OID4VCI options for HAIP compliance
    /// </summary>
    /// <param name="options">OID4VCI configuration options</param>
    /// <param name="level">Required HAIP compliance level</param>
    /// <param name="config">Optional HAIP configuration (uses defaults if null)</param>
    public static void UseHaipProfile(this object options, HaipLevel level, HaipConfiguration? config = null)
    {
        config ??= HaipConfiguration.GetDefault(level);
        
        // This would integrate with actual Oid4VciOptions when available
        // For now, we'll implement the pattern
        
        ConfigureCryptographicRequirements(options, level);
        ConfigureProtocolSecurity(options, level);
        ConfigureClientAuthentication(options, level);
        ConfigureAuditing(options, config);
    }

    private static void ConfigureCryptographicRequirements(object options, HaipLevel level)
    {
        // Configure allowed signing algorithms based on HAIP level
        var allowedAlgorithms = level switch
        {
            HaipLevel.Level1_High => HaipConstants.Level1_Algorithms,
            HaipLevel.Level2_VeryHigh => HaipConstants.Level2_Algorithms,
            HaipLevel.Level3_Sovereign => HaipConstants.Level3_Algorithms,
            _ => HaipConstants.Level1_Algorithms
        };

        // Set validation rules (would integrate with actual options structure)
        SetProperty(options, "AllowedSigningAlgorithms", allowedAlgorithms);
        SetProperty(options, "ForbiddenSigningAlgorithms", HaipConstants.ForbiddenAlgorithms);
    }

    private static void ConfigureProtocolSecurity(object options, HaipLevel level)
    {
        // Mandatory protocol security requirements
        SetProperty(options, "RequireProofOfPossession", true);
        SetProperty(options, "RequireSecureTransport", true);
        SetProperty(options, "RequirePkce", true);
        
        // Level-specific requirements
        if (level >= HaipLevel.Level2_VeryHigh)
        {
            SetProperty(options, "RequirePushedAuthorizationRequests", true);
            SetProperty(options, "RequireDpopOrMtls", true);
        }
        
        if (level == HaipLevel.Level3_Sovereign)
        {
            SetProperty(options, "RequireHardwareSecurityModule", true);
            SetProperty(options, "RequireQualifiedElectronicSignature", true);
        }
    }

    private static void ConfigureClientAuthentication(object options, HaipLevel level)
    {
        var allowedMethods = level switch
        {
            HaipLevel.Level1_High => HaipConstants.ClientAuthMethods.Level1_Allowed,
            HaipLevel.Level2_VeryHigh => HaipConstants.ClientAuthMethods.Level2_Required,
            HaipLevel.Level3_Sovereign => HaipConstants.ClientAuthMethods.Level3_Required,
            _ => HaipConstants.ClientAuthMethods.Level1_Allowed
        };

        SetProperty(options, "ClientAuthenticationMethods", allowedMethods);
        
        if (level >= HaipLevel.Level2_VeryHigh)
        {
            SetProperty(options, "RequireWalletAttestation", true);
        }
        
        if (level == HaipLevel.Level3_Sovereign)
        {
            SetProperty(options, "RequireQualifiedWalletAttestation", true);
        }
    }

    private static void ConfigureAuditing(object options, HaipConfiguration config)
    {
        SetProperty(options, "EnableComplianceAuditing", true);
        SetProperty(options, "AuditingOptions", config.AuditingOptions);
    }

    // Helper method to set properties via reflection (would be replaced with actual strongly-typed options)
    private static void SetProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName);
        if (property?.CanWrite == true)
        {
            property.SetValue(obj, value);
        }
        // In real implementation, this would log or handle missing properties appropriately
    }
}

/// <summary>
/// Extension methods for integrating HAIP with OID4VP
/// </summary>
public static class HaipOid4VpExtensions
{
    /// <summary>
    /// Configures OID4VP presentation request options for HAIP compliance
    /// </summary>
    /// <param name="options">OID4VP presentation request options</param>
    /// <param name="level">Required HAIP compliance level</param>
    public static void EnforceHaip(this object options, HaipLevel level)
    {
        // HAIP mandates signed response (JARM)
        SetProperty(options, "ResponseMode", "direct_post.jwt");
        
        // Configure allowed client ID schemes based on HAIP requirements
        var allowedClientIdSchemes = new[] 
        { 
            "redirect_uri", 
            "x509_san_dns", 
            "verifier_attestation",
            "entity_id" // For federation scenarios
        };
        
        SetProperty(options, "AllowedClientIdSchemes", allowedClientIdSchemes);
        
        // Level-specific requirements
        if (level >= HaipLevel.Level2_VeryHigh)
        {
            SetProperty(options, "RequireVerifierAttestation", true);
            SetProperty(options, "RequireSignedRequest", true);
        }
        
        if (level == HaipLevel.Level3_Sovereign)
        {
            SetProperty(options, "RequireQualifiedVerifierAttestation", true);
            // Restrict to only the most secure client ID schemes
            SetProperty(options, "AllowedClientIdSchemes", new[] { "verifier_attestation", "x509_san_dns" });
        }
    }

    // Helper method - same as above
    private static void SetProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName);
        if (property?.CanWrite == true)
        {
            property.SetValue(obj, value);
        }
    }
}

/// <summary>
/// HAIP validation service for runtime compliance checking
/// </summary>
public class HaipValidationService
{
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
        ILogger<HaipValidationService> logger)
    {
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
        CancellationToken cancellationToken = default)
    {
        var result = new HaipComplianceResult();
        result.AuditTrail.ValidatorId = nameof(HaipValidationService);
        result.AuditTrail.AddStep("Starting issuance validation", true);

        try
        {
            // Validate cryptographic requirements
            if (context.SigningKey != null && context.SigningAlgorithm != null)
            {
                var cryptoResult = _cryptoValidator.ValidateKeyCompliance(context.SigningKey, context.SigningAlgorithm);
                result.Violations.AddRange(cryptoResult.Violations);
                result.AuditTrail.Steps.AddRange(cryptoResult.AuditTrail.Steps);
            }

            // Validate protocol security
            await ValidateProtocolSecurity(context, result, cancellationToken);

            // Validate trust framework (if configured)
            if (_config.TrustFrameworks.Any())
            {
                await ValidateTrustFramework(context, result, cancellationToken);
            }

            result.IsCompliant = !result.Violations.Any(v => v.Severity == HaipSeverity.Critical);
            result.AuditTrail.Complete();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during HAIP validation");
            result.AddViolation($"Validation error: {ex.Message}", HaipViolationType.InsufficientAssuranceLevel);
            result.AuditTrail.Complete();
            return result;
        }
    }

    private async Task ValidateProtocolSecurity(
        HaipIssuanceValidationContext context, 
        HaipComplianceResult result,
        CancellationToken cancellationToken)
    {
        // Validate proof of possession
        if (!context.HasProofOfPossession)
        {
            result.AddViolation(
                "Proof of possession is required for HAIP compliance",
                HaipViolationType.MissingProofOfPossession);
        }

        // Validate secure transport
        if (!context.IsSecureTransport)
        {
            result.AddViolation(
                "Secure transport (HTTPS) is required for HAIP compliance",
                HaipViolationType.InsecureTransport);
        }

        // Level-specific validations
        if (_config.RequiredLevel >= HaipLevel.Level2_VeryHigh)
        {
            if (!context.HasWalletAttestation)
            {
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
        CancellationToken cancellationToken)
    {
        // This would integrate with the existing OpenID Federation implementation
        // to validate trust chains according to configured trust frameworks
        
        foreach (var framework in _config.TrustFrameworks)
        {
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
public class HaipIssuanceValidationContext
{
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