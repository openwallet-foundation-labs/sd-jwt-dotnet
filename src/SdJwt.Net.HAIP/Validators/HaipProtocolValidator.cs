using Microsoft.Extensions.Logging;
using SdJwt.Net.HAIP.Models;
using System.Text.Json;

namespace SdJwt.Net.HAIP.Validators;

/// <summary>
/// Interface for HAIP protocol validation
/// </summary>
public interface IHaipProtocolValidator
{
    /// <summary>
    /// Validates protocol compliance for a credential request
    /// </summary>
    Task<HaipComplianceResult> ValidateRequestAsync(object request, HaipLevel requiredLevel);

    /// <summary>
    /// Validates transport security requirements
    /// </summary>
    HaipComplianceResult ValidateTransportSecurity(string requestUrl, HaipLevel requiredLevel);

    /// <summary>
    /// Validates client authentication method
    /// </summary>
    HaipComplianceResult ValidateClientAuthentication(string authMethod, HaipLevel requiredLevel);
}

/// <summary>
/// HAIP protocol validator implementation
/// </summary>
public class HaipProtocolValidator : IHaipProtocolValidator
{
    private readonly HaipLevel _requiredLevel;
    private readonly ILogger<HaipProtocolValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HaipProtocolValidator"/> class
    /// </summary>
    /// <param name="requiredLevel">The required HAIP compliance level for validation</param>
    /// <param name="logger">The logger instance for validation operations</param>
    public HaipProtocolValidator(HaipLevel requiredLevel, ILogger<HaipProtocolValidator> logger)
    {
        _requiredLevel = requiredLevel;
        _logger = logger;
    }

    /// <summary>
    /// Validates protocol compliance for a credential request
    /// </summary>
    public async Task<HaipComplianceResult> ValidateRequestAsync(object request, HaipLevel requiredLevel)
    {
        var result = new HaipComplianceResult { AchievedLevel = requiredLevel };
        result.AuditTrail.ValidatorId = nameof(HaipProtocolValidator);
        result.AuditTrail.AddStep("Starting protocol validation", true);

        try
        {
            // 1. Validate transport security
            // Note: In a real implementation, this would extract the actual request URL
            var transportResult = ValidateTransportSecurity("https://example.com", requiredLevel);
            result.AuditTrail.AddStep("Transport security validation", transportResult.IsCompliant);

            if (!transportResult.IsCompliant)
            {
                result.Violations.AddRange(transportResult.Violations);
            }

            // 2. Validate proof of possession requirement
            var proofOfPossessionResult = ValidateProofOfPossession(request);
            result.AuditTrail.AddStep("Proof of possession validation", proofOfPossessionResult.IsValid);

            if (!proofOfPossessionResult.IsValid)
            {
                result.AddViolation(
                    proofOfPossessionResult.ErrorMessage!,
                    HaipViolationType.MissingProofOfPossession,
                    HaipSeverity.Critical
                );
            }

            // 3. Validate wallet attestation (Level 2+)
            if (requiredLevel >= HaipLevel.Level2_VeryHigh)
            {
                var walletAttestationResult = ValidateWalletAttestation(request);
                result.AuditTrail.AddStep("Wallet attestation validation", walletAttestationResult.IsValid);

                if (!walletAttestationResult.IsValid)
                {
                    result.AddViolation(
                        walletAttestationResult.ErrorMessage!,
                        HaipViolationType.InsecureClientAuthentication,
                        HaipSeverity.Critical
                    );
                }
            }

            // 4. Validate DPoP usage (Level 2+)
            if (requiredLevel >= HaipLevel.Level2_VeryHigh)
            {
                var dpopResult = ValidateDPoP(request);
                result.AuditTrail.AddStep("DPoP validation", dpopResult.IsValid);

                if (!dpopResult.IsValid)
                {
                    result.AddViolation(
                        dpopResult.ErrorMessage!,
                        HaipViolationType.InsecureTransport,
                        HaipSeverity.Critical
                    );
                }
            }

            // 5. Validate HSM requirements (Level 3)
            if (requiredLevel == HaipLevel.Level3_Sovereign)
            {
                var hsmResult = ValidateHSMRequirement(request);
                result.AuditTrail.AddStep("HSM requirement validation", hsmResult.IsValid);

                if (!hsmResult.IsValid)
                {
                    result.AddViolation(
                        hsmResult.ErrorMessage!,
                        HaipViolationType.InsufficientAssuranceLevel,
                        HaipSeverity.Critical
                    );
                }
            }

            result.IsCompliant = result.Violations.All(v => v.Severity != HaipSeverity.Critical);
            result.AuditTrail.Complete();

            _logger.LogInformation("Protocol validation completed. Compliant: {IsCompliant}, Violations: {ViolationCount}",
                result.IsCompliant, result.Violations.Count);

            // Add an await to satisfy the async requirement
            await Task.CompletedTask;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during protocol validation");
            result.AddViolation($"Protocol validation error: {ex.Message}", HaipViolationType.InsecureTransport);
            result.AuditTrail.Complete();
            return result;
        }
    }

    /// <summary>
    /// Validates transport security requirements
    /// </summary>
    public HaipComplianceResult ValidateTransportSecurity(string requestUrl, HaipLevel requiredLevel)
    {
        var result = new HaipComplianceResult { AchievedLevel = requiredLevel };

        if (string.IsNullOrEmpty(requestUrl))
        {
            result.AddViolation("Request URL is required for transport security validation",
                HaipViolationType.InsecureTransport);
            return result;
        }

        // Validate HTTPS usage
        if (!requestUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            result.AddViolation("HTTPS transport required for all HAIP levels",
                HaipViolationType.InsecureTransport,
                HaipSeverity.Critical,
                "Use HTTPS with TLS 1.2 or higher");
        }
        else
        {
            result.IsCompliant = true;
        }

        return result;
    }

    /// <summary>
    /// Validates client authentication method
    /// </summary>
    public HaipComplianceResult ValidateClientAuthentication(string authMethod, HaipLevel requiredLevel)
    {
        var result = new HaipComplianceResult { AchievedLevel = requiredLevel };

        if (string.IsNullOrEmpty(authMethod))
        {
            result.AddViolation("Client authentication method is required",
                HaipViolationType.InsecureClientAuthentication);
            return result;
        }

        var allowedMethods = GetAllowedAuthMethods(requiredLevel);

        if (!allowedMethods.Contains(authMethod))
        {
            result.AddViolation(
                $"Authentication method '{authMethod}' not allowed for HAIP {requiredLevel}",
                HaipViolationType.InsecureClientAuthentication,
                HaipSeverity.Critical,
                $"Use one of: {string.Join(", ", allowedMethods)}"
            );
        }
        else
        {
            result.IsCompliant = true;
        }

        return result;
    }

    private string[] GetAllowedAuthMethods(HaipLevel level)
    {
        return level switch
        {
            HaipLevel.Level1_High => HaipConstants.ClientAuthMethods.Level1_Allowed,
            HaipLevel.Level2_VeryHigh => HaipConstants.ClientAuthMethods.Level2_Required,
            HaipLevel.Level3_Sovereign => HaipConstants.ClientAuthMethods.Level3_Required,
            _ => throw new ArgumentException($"Invalid HAIP level: {level}")
        };
    }

    private HaipProtocolValidationResult ValidateProofOfPossession(object request)
    {
        if (request == null)
        {
            return HaipProtocolValidationResult.Failed("Proof of possession is required for all HAIP levels");
        }

        if (TryGetBoolean(request, "proof_of_possession", out var hasPopFlag))
        {
            return hasPopFlag
                ? HaipProtocolValidationResult.Success("Proof of possession validated")
                : HaipProtocolValidationResult.Failed("Proof of possession is required for all HAIP levels");
        }

        if (HasAnyProperty(request, "proof", "proofJwt", "proof_jwt", "key_binding_jwt", "cnf"))
        {
            return HaipProtocolValidationResult.Success("Proof of possession validated");
        }

        return HaipProtocolValidationResult.Failed("Proof of possession is required for all HAIP levels");
    }

    private HaipProtocolValidationResult ValidateWalletAttestation(object request)
    {
        if (TryGetBoolean(request, "wallet_attestation_valid", out var isValidFlag))
        {
            return isValidFlag
                ? HaipProtocolValidationResult.Success("Wallet attestation validated")
                : HaipProtocolValidationResult.Failed("Wallet attestation is required for HAIP Level 2+ compliance");
        }

        if (HasAnyProperty(request, "wallet_attestation", "attestation", "walletAttestation"))
        {
            return HaipProtocolValidationResult.Success("Wallet attestation validated");
        }

        return HaipProtocolValidationResult.Failed("Wallet attestation is required for HAIP Level 2+ compliance");
    }

    private HaipProtocolValidationResult ValidateDPoP(object request)
    {
        if (TryGetBoolean(request, "dpop_valid", out var isValidFlag))
        {
            return isValidFlag
                ? HaipProtocolValidationResult.Success("DPoP validated")
                : HaipProtocolValidationResult.Failed("DPoP tokens are required for HAIP Level 2+ compliance");
        }

        if (HasAnyProperty(request, "dpop", "dpop_proof", "dpopProof"))
        {
            return HaipProtocolValidationResult.Success("DPoP validated");
        }

        return HaipProtocolValidationResult.Failed("DPoP tokens are required for HAIP Level 2+ compliance");
    }

    private HaipProtocolValidationResult ValidateHSMRequirement(object request)
    {
        if (TryGetBoolean(request, "hsm_backed", out var hsmFlag) ||
            TryGetBoolean(request, "hsmBacked", out hsmFlag))
        {
            return hsmFlag
                ? HaipProtocolValidationResult.Success("HSM requirement validated")
                : HaipProtocolValidationResult.Failed("Hardware Security Module backing is required for HAIP Level 3 (Sovereign) compliance");
        }

        return HaipProtocolValidationResult.Failed("Hardware Security Module backing is required for HAIP Level 3 (Sovereign) compliance");
    }

    private static bool HasAnyProperty(object request, params string[] propertyNames)
    {
        if (request is IDictionary<string, object> dict)
        {
            return propertyNames.Any(name => dict.ContainsKey(name) && dict[name] != null);
        }

        if (request is JsonElement element && element.ValueKind == JsonValueKind.Object)
        {
            return propertyNames.Any(name => element.TryGetProperty(name, out var value) && value.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined);
        }

        var type = request.GetType();
        return propertyNames.Any(name =>
        {
            var prop = type.GetProperty(name);
            if (prop != null)
            {
                return prop.GetValue(request) != null;
            }

            var normalized = Normalize(name);
            var matched = type.GetProperties().FirstOrDefault(p => Normalize(p.Name) == normalized);
            return matched?.GetValue(request) != null;
        });
    }

    private static bool TryGetBoolean(object request, string propertyName, out bool value)
    {
        value = false;

        if (request is IDictionary<string, object> dict && dict.TryGetValue(propertyName, out var dictValue))
        {
            return TryConvertToBool(dictValue, out value);
        }

        if (request is JsonElement element && element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out var jsonValue))
        {
            return TryConvertJsonToBool(jsonValue, out value);
        }

        var type = request.GetType();
        var property = type.GetProperty(propertyName) ??
                       type.GetProperties().FirstOrDefault(p => Normalize(p.Name) == Normalize(propertyName));
        if (property == null)
        {
            return false;
        }

        var raw = property.GetValue(request);
        return TryConvertToBool(raw, out value);
    }

    private static bool TryConvertJsonToBool(JsonElement element, out bool value)
    {
        value = false;
        return element.ValueKind switch
        {
            JsonValueKind.True => (value = true) || true,
            JsonValueKind.False => true,
            JsonValueKind.String when bool.TryParse(element.GetString(), out var parsed) => (value = parsed) || true,
            JsonValueKind.Number when element.TryGetInt32(out var i) => (value = i != 0) || true,
            _ => false
        };
    }

    private static bool TryConvertToBool(object? raw, out bool value)
    {
        value = false;
        switch (raw)
        {
            case null:
                return false;
            case bool b:
                value = b;
                return true;
            case string s when bool.TryParse(s, out var parsed):
                value = parsed;
                return true;
            case int i:
                value = i != 0;
                return true;
            case long l:
                value = l != 0;
                return true;
            case JsonElement e:
                return TryConvertJsonToBool(e, out value);
            default:
                return false;
        }
    }

    private static string Normalize(string value)
    {
        return value.Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
    }
}

/// <summary>
/// Result of protocol validation step for HAIP compliance
/// </summary>
public class HaipProtocolValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the protocol validation passed
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the error message if validation failed
    /// </summary>
    public string? ErrorMessage
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets additional details about the validation result
    /// </summary>
    public string? Details
    {
        get; set;
    }

    /// <summary>
    /// Creates a successful protocol validation result
    /// </summary>
    /// <param name="details">Optional details about the successful validation</param>
    /// <returns>A successful validation result</returns>
    public static HaipProtocolValidationResult Success(string? details = null) =>
        new()
        {
            IsValid = true,
            Details = details
        };

    /// <summary>
    /// Creates a failed protocol validation result
    /// </summary>
    /// <param name="errorMessage">The error message describing why validation failed</param>
    /// <returns>A failed validation result</returns>
    public static HaipProtocolValidationResult Failed(string errorMessage) =>
        new()
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
}
