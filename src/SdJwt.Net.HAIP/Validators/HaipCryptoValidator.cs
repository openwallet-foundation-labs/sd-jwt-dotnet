using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.HAIP.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace SdJwt.Net.HAIP.Validators;

/// <summary>
/// Interface for HAIP cryptographic validation
/// </summary>
public interface IHaipCryptoValidator
{
    /// <summary>
    /// Validates cryptographic compliance for a key and algorithm
    /// </summary>
    HaipComplianceResult ValidateKeyCompliance(SecurityKey key, string algorithm);
    
    /// <summary>
    /// Validates JWT header compliance
    /// </summary>
    HaipComplianceResult ValidateJwtHeader(JwtHeader header);
    
    /// <summary>
    /// Validates algorithm against HAIP requirements
    /// </summary>
    HaipAlgorithmValidation ValidateAlgorithm(string algorithm);
}

/// <summary>
/// HAIP cryptographic validator implementation
/// </summary>
public class HaipCryptoValidator : IHaipCryptoValidator
{
    private readonly HaipLevel _requiredLevel;
    private readonly ILogger<HaipCryptoValidator> _logger;
    
    public HaipCryptoValidator(HaipLevel requiredLevel, ILogger<HaipCryptoValidator> logger)
    {
        _requiredLevel = requiredLevel;
        _logger = logger;
    }

    /// <summary>
    /// Validates complete cryptographic compliance
    /// </summary>
    public HaipComplianceResult ValidateKeyCompliance(SecurityKey key, string algorithm)
    {
        var result = new HaipComplianceResult { AchievedLevel = _requiredLevel };
        result.AuditTrail.ValidatorId = nameof(HaipCryptoValidator);
        result.AuditTrail.AddStep("Starting cryptographic validation", true);
        
        try
        {
            // 1. Validate Algorithm
            var algorithmResult = ValidateAlgorithm(algorithm);
            result.AuditTrail.AddStep($"Algorithm validation: {algorithm}", algorithmResult.IsValid, algorithmResult.Details);
            
            if (!algorithmResult.IsValid)
            {
                result.AddViolation(
                    algorithmResult.ErrorMessage!,
                    HaipViolationType.WeakCryptography,
                    HaipSeverity.Critical,
                    $"Use one of: {string.Join(", ", GetAllowedAlgorithms())}"
                );
            }
            
            // 2. Validate Key Strength
            var keyStrengthResult = ValidateKeyStrength(key);
            result.AuditTrail.AddStep("Key strength validation", keyStrengthResult.IsValid, keyStrengthResult.Details);
            
            if (!keyStrengthResult.IsValid)
            {
                result.AddViolation(
                    keyStrengthResult.ErrorMessage!,
                    HaipViolationType.WeakKeyStrength,
                    HaipSeverity.Critical
                );
            }
            
            // 3. Validate Hardware Security (Level 3 only)
            if (_requiredLevel == HaipLevel.Level3_Sovereign)
            {
                var hsmResult = ValidateHardwareSecurityModule(key);
                result.AuditTrail.AddStep("HSM validation", hsmResult.IsValid, hsmResult.Details);
                
                if (!hsmResult.IsValid)
                {
                    result.AddViolation(
                        hsmResult.ErrorMessage!,
                        HaipViolationType.InsufficientAssuranceLevel,
                        HaipSeverity.Critical,
                        "Use hardware security module for key storage"
                    );
                }
            }
            
            // Determine achieved level
            if (result.Violations.Count == 0)
            {
                result.IsCompliant = true;
                result.AchievedLevel = DetermineAchievedLevel(algorithm, key);
            }
            
            result.AuditTrail.Complete();
            _logger.LogInformation("Cryptographic validation completed. Compliant: {IsCompliant}, Level: {Level}, Violations: {ViolationCount}", 
                result.IsCompliant, result.AchievedLevel, result.Violations.Count);
                
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cryptographic validation");
            result.AddViolation($"Validation error: {ex.Message}", HaipViolationType.WeakCryptography);
            result.AuditTrail.Complete();
            return result;
        }
    }

    /// <summary>
    /// Validates JWT header for HAIP compliance
    /// </summary>
    public HaipComplianceResult ValidateJwtHeader(JwtHeader header)
    {
        var result = new HaipComplianceResult { AchievedLevel = _requiredLevel };
        result.AuditTrail.ValidatorId = nameof(HaipCryptoValidator);
        
        // Validate algorithm
        if (header.Alg == null)
        {
            result.AddViolation("JWT header missing algorithm", HaipViolationType.WeakCryptography);
            return result;
        }
        
        var algorithmResult = ValidateAlgorithm(header.Alg);
        if (!algorithmResult.IsValid)
        {
            result.AddViolation(algorithmResult.ErrorMessage!, HaipViolationType.WeakCryptography);
        }
        
        // Validate key ID is present (recommended for key rotation)
        if (string.IsNullOrEmpty(header.Kid))
        {
            result.AddViolation("Key ID (kid) should be present for key identification", 
                HaipViolationType.WeakCryptography, HaipSeverity.Warning);
        }
        
        result.IsCompliant = result.Violations.All(v => v.Severity != HaipSeverity.Critical);
        return result;
    }

    /// <summary>
    /// Validates algorithm against HAIP requirements
    /// </summary>
    public HaipAlgorithmValidation ValidateAlgorithm(string algorithm)
    {
        if (string.IsNullOrWhiteSpace(algorithm))
        {
            return HaipAlgorithmValidation.Failed("Algorithm cannot be null or empty");
        }
        
        // Check forbidden algorithms first
        if (HaipConstants.ForbiddenAlgorithms.Contains(algorithm))
        {
            return HaipAlgorithmValidation.Failed(
                $"Algorithm '{algorithm}' is explicitly forbidden in HAIP. " +
                $"Forbidden algorithms: {string.Join(", ", HaipConstants.ForbiddenAlgorithms)}");
        }
        
        // Check level-specific allowed algorithms
        var allowedAlgorithms = GetAllowedAlgorithms();
        if (!allowedAlgorithms.Contains(algorithm))
        {
            return HaipAlgorithmValidation.Failed(
                $"Algorithm '{algorithm}' is not approved for HAIP {_requiredLevel}. " +
                $"Allowed algorithms: {string.Join(", ", allowedAlgorithms)}");
        }
        
        return HaipAlgorithmValidation.Success($"Algorithm '{algorithm}' is compliant with HAIP {_requiredLevel}");
    }
    
    private string[] GetAllowedAlgorithms()
    {
        return _requiredLevel switch
        {
            HaipLevel.Level1_High => HaipConstants.Level1_Algorithms,
            HaipLevel.Level2_VeryHigh => HaipConstants.Level2_Algorithms,
            HaipLevel.Level3_Sovereign => HaipConstants.Level3_Algorithms,
            _ => throw new ArgumentException($"Invalid HAIP level: {_requiredLevel}")
        };
    }
    
    private HaipKeyStrengthValidation ValidateKeyStrength(SecurityKey key)
    {
        return key switch
        {
            ECDsaSecurityKey ecKey => ValidateECKeyStrength(ecKey),
            RsaSecurityKey rsaKey => ValidateRSAKeyStrength(rsaKey),
            SymmetricSecurityKey _ => HaipKeyStrengthValidation.Failed("Symmetric keys are not allowed in HAIP"),
            _ => HaipKeyStrengthValidation.Failed($"Unsupported key type: {key.GetType().Name}")
        };
    }
    
    private HaipKeyStrengthValidation ValidateECKeyStrength(ECDsaSecurityKey ecKey)
    {
        try
        {
            var keySize = ecKey.ECDsa.KeySize;
            var minimumKeySize = GetMinimumECKeySize();
            
            if (keySize < minimumKeySize)
            {
                return HaipKeyStrengthValidation.Failed(
                    $"EC key size {keySize} bits is below minimum {minimumKeySize} bits for HAIP {_requiredLevel}");
            }
            
            return HaipKeyStrengthValidation.Success(
                $"EC key size {keySize} bits meets HAIP {_requiredLevel} requirements");
        }
        catch (Exception ex)
        {
            return HaipKeyStrengthValidation.Failed($"Error validating EC key strength: {ex.Message}");
        }
    }
    
    private HaipKeyStrengthValidation ValidateRSAKeyStrength(RsaSecurityKey rsaKey)
    {
        try
        {
            var keySize = rsaKey.Rsa?.KeySize ?? rsaKey.Parameters.Modulus?.Length * 8 ?? 0;
            var minimumKeySize = GetMinimumRSAKeySize();
            
            if (keySize < minimumKeySize)
            {
                return HaipKeyStrengthValidation.Failed(
                    $"RSA key size {keySize} bits is below minimum {minimumKeySize} bits for HAIP {_requiredLevel}");
            }
            
            return HaipKeyStrengthValidation.Success(
                $"RSA key size {keySize} bits meets HAIP {_requiredLevel} requirements");
        }
        catch (Exception ex)
        {
            return HaipKeyStrengthValidation.Failed($"Error validating RSA key strength: {ex.Message}");
        }
    }
    
    private HaipHSMValidation ValidateHardwareSecurityModule(SecurityKey key)
    {
        // In a real implementation, this would check if the key is stored in an HSM
        // For now, we'll check for HSM-specific properties or metadata
        
        if (key is ECDsaSecurityKey ecKey)
        {
            // Check if the ECDsa instance indicates HSM usage
            // This is implementation-specific and would depend on the HSM provider
            var isHsmBacked = CheckECKeyHSMBacking(ecKey);
            
            if (!isHsmBacked)
            {
                return HaipHSMValidation.Failed(
                    "Hardware Security Module backing required for sovereign level compliance");
            }
        }
        
        return HaipHSMValidation.Success("HSM backing validated successfully");
    }
    
    private static bool CheckECKeyHSMBacking(ECDsaSecurityKey ecKey)
    {
        // This would be implemented based on specific HSM provider
        // For example, checking if the key handle indicates HSM storage
        // or if the ECDsa instance is from an HSM provider
        
        // For demonstration, we'll assume software keys are not HSM-backed
        return false; // Would be actual HSM detection logic
    }
    
    private int GetMinimumECKeySize()
    {
        return _requiredLevel switch
        {
            HaipLevel.Level1_High => HaipConstants.KeySizes.Level1_EcMinimum,
            HaipLevel.Level2_VeryHigh => HaipConstants.KeySizes.Level2_EcMinimum,
            HaipLevel.Level3_Sovereign => HaipConstants.KeySizes.Level3_EcMinimum,
            _ => HaipConstants.KeySizes.Level1_EcMinimum
        };
    }
    
    private int GetMinimumRSAKeySize()
    {
        return _requiredLevel switch
        {
            HaipLevel.Level1_High => HaipConstants.KeySizes.Level1_RsaMinimum,
            HaipLevel.Level2_VeryHigh => HaipConstants.KeySizes.Level2_RsaMinimum,
            HaipLevel.Level3_Sovereign => HaipConstants.KeySizes.Level3_RsaMinimum,
            _ => HaipConstants.KeySizes.Level1_RsaMinimum
        };
    }
    
    private HaipLevel DetermineAchievedLevel(string algorithm, SecurityKey key)
    {
        // Determine the highest HAIP level this key/algorithm combination can achieve
        
        if (HaipConstants.Level3_Algorithms.Contains(algorithm) && 
            ValidateKeyStrength(key).IsValid &&
            (_requiredLevel != HaipLevel.Level3_Sovereign || ValidateHardwareSecurityModule(key).IsValid))
        {
            return HaipLevel.Level3_Sovereign;
        }
        
        if (HaipConstants.Level2_Algorithms.Contains(algorithm) && ValidateKeyStrength(key).IsValid)
        {
            return HaipLevel.Level2_VeryHigh;
        }
        
        if (HaipConstants.Level1_Algorithms.Contains(algorithm) && ValidateKeyStrength(key).IsValid)
        {
            return HaipLevel.Level1_High;
        }
        
        return HaipLevel.Level1_High; // Default fallback
    }
}

/// <summary>
/// Result of algorithm validation
/// </summary>
public class HaipAlgorithmValidation
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Details { get; set; }
    
    public static HaipAlgorithmValidation Success(string? details = null) => 
        new() { IsValid = true, Details = details };
    
    public static HaipAlgorithmValidation Failed(string errorMessage) => 
        new() { IsValid = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Result of key strength validation
/// </summary>
public class HaipKeyStrengthValidation
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Details { get; set; }
    
    public static HaipKeyStrengthValidation Success(string? details = null) => 
        new() { IsValid = true, Details = details };
    
    public static HaipKeyStrengthValidation Failed(string errorMessage) => 
        new() { IsValid = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Result of HSM validation
/// </summary>
public class HaipHSMValidation
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Details { get; set; }
    
    public static HaipHSMValidation Success(string? details = null) => 
        new() { IsValid = true, Details = details };
    
    public static HaipHSMValidation Failed(string errorMessage) => 
        new() { IsValid = false, ErrorMessage = errorMessage };
}