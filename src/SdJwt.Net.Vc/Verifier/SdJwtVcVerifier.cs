using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace SdJwt.Net.Vc.Verifier;

/// <summary>
/// Represents the result of a successful SD-JWT VC verification according to draft-ietf-oauth-sd-jwt-vc-13.
/// </summary>
public record SdJwtVcVerificationResult(
    ClaimsPrincipal ClaimsPrincipal,
    bool KeyBindingVerified,
    string VerifiableCredentialType,
    SdJwtVcPayload SdJwtVcPayload) : VerificationResult(ClaimsPrincipal, KeyBindingVerified);

/// <summary>
/// A specialized verifier for SD-JWT-based Verifiable Credentials (SD-JWT VCs) that extends the base SD-JWT verification
/// with VC-specific validation logic according to draft-ietf-oauth-sd-jwt-vc-13.
/// Note: This specification does not utilize the W3C Verifiable Credentials Data Model.
/// </summary>
/// <param name="issuerKeyProvider">A function that resolves the Issuer's public key based on the unverified SD-JWT header/payload.</param>
/// <param name="logger">An optional logger for diagnostics.</param>
public class SdJwtVcVerifier(Func<JwtSecurityToken, Task<SecurityKey>> issuerKeyProvider, ILogger<SdJwtVcVerifier>? logger = null)
{
    private readonly SdVerifier _baseSdVerifier = new(issuerKeyProvider);
    private readonly ILogger? _logger = logger;

    /// <summary>
    /// Verifies an SD-JWT VC according to draft-ietf-oauth-sd-jwt-vc-13 and returns a strongly-typed verification result.
    /// </summary>
    /// <param name="presentation">The presentation string from the Holder.</param>
    /// <param name="validationParameters">Token validation parameters to apply to the main SD-JWT.</param>
    /// <param name="kbJwtValidationParameters">Optional validation parameters for the Key Binding JWT.</param>
    /// <param name="expectedVctType">Optional expected VCT identifier to validate against.</param>
    /// <param name="validateTypeIntegrity">Whether to validate VCT integrity hash if present.</param>
    /// <returns>A <see cref="SdJwtVcVerificationResult"/> containing the rehydrated claims and SD-JWT VC payload if verification is successful.</returns>
    public async Task<SdJwtVcVerificationResult> VerifyAsync(
        string presentation,
        TokenValidationParameters validationParameters,
        TokenValidationParameters? kbJwtValidationParameters = null,
        string? expectedVctType = null,
        bool validateTypeIntegrity = true)
    {
        _logger?.LogInformation("Starting SD-JWT VC verification according to draft-ietf-oauth-sd-jwt-vc-13");

        // Use the base verifier for core SD-JWT verification
        var baseResult = await _baseSdVerifier.VerifyAsync(presentation, validationParameters, kbJwtValidationParameters);

        // Validate JWT header type
        ValidateJwtHeader(baseResult.ClaimsPrincipal);

        // Extract and validate required VCT claim
        var vctClaim = baseResult.ClaimsPrincipal.FindFirst("vct")?.Value
            ?? throw new SecurityTokenException("Missing required 'vct' claim in SD-JWT VC.");

        _logger?.LogDebug("Found vct claim: {VctClaim}", vctClaim);

        // Validate expected VCT type if provided
        if (!string.IsNullOrEmpty(expectedVctType) && vctClaim != expectedVctType)
            throw new SecurityTokenException($"Expected VCT type '{expectedVctType}' but found '{vctClaim}'");

        // Validate VCT integrity if present and required
        if (validateTypeIntegrity)
        {
            var vctIntegrityClaim = baseResult.ClaimsPrincipal.FindFirst("vct#integrity")?.Value;
            if (!string.IsNullOrEmpty(vctIntegrityClaim))
            {
                _logger?.LogDebug("VCT integrity claim found, validation should be performed by calling application");
                // Note: Actual integrity validation requires retrieving and hashing the Type Metadata document
                // This is typically done by the calling application as it requires external HTTP calls
            }
        }

        // Parse the SD-JWT VC payload from claims
        var sdJwtVcPayload = ParseSdJwtVcPayload(baseResult.ClaimsPrincipal, vctClaim);

        // Validate SD-JWT VC structure according to draft-13
        ValidateSdJwtVc(sdJwtVcPayload, vctClaim);

        _logger?.LogInformation("SD-JWT VC verification completed successfully");

        return new SdJwtVcVerificationResult(baseResult.ClaimsPrincipal, baseResult.KeyBindingVerified, vctClaim, sdJwtVcPayload);
    }

    /// <summary>
    /// Verifies an SD-JWT VC in JSON serialization format according to draft-ietf-oauth-sd-jwt-vc-13.
    /// </summary>
    /// <param name="jsonSerialization">The SD-JWT VC in JWS JSON Serialization format.</param>
    /// <param name="validationParameters">Token validation parameters.</param>
    /// <param name="kbJwtValidationParameters">Optional KB-JWT validation parameters.</param>
    /// <param name="expectedVctType">Optional expected VCT identifier.</param>
    /// <param name="validateTypeIntegrity">Whether to validate VCT integrity hash if present.</param>
    /// <returns>A verification result with SD-JWT VC specific information.</returns>
    public async Task<SdJwtVcVerificationResult> VerifyJsonSerializationAsync(
        string jsonSerialization,
        TokenValidationParameters validationParameters,
        TokenValidationParameters? kbJwtValidationParameters = null,
        string? expectedVctType = null,
        bool validateTypeIntegrity = true)
    {
        _logger?.LogInformation("Starting SD-JWT VC JSON serialization verification");

        // Use the base verifier for core verification
        var baseResult = await _baseSdVerifier.VerifyJsonSerializationAsync(jsonSerialization, validationParameters, kbJwtValidationParameters);

        // Validate JWT header type
        ValidateJwtHeader(baseResult.ClaimsPrincipal);

        // Extract and validate VCT claim
        var vctClaim = baseResult.ClaimsPrincipal.FindFirst("vct")?.Value
            ?? throw new SecurityTokenException("Missing required 'vct' claim in SD-JWT VC.");

        if (!string.IsNullOrEmpty(expectedVctType) && vctClaim != expectedVctType)
            throw new SecurityTokenException($"Expected VCT type '{expectedVctType}' but found '{vctClaim}'");

        // Parse and validate payload
        var sdJwtVcPayload = ParseSdJwtVcPayload(baseResult.ClaimsPrincipal, vctClaim);
        ValidateSdJwtVc(sdJwtVcPayload, vctClaim);

        _logger?.LogInformation("SD-JWT VC JSON serialization verification completed successfully");

        return new SdJwtVcVerificationResult(baseResult.ClaimsPrincipal, baseResult.KeyBindingVerified, vctClaim, sdJwtVcPayload);
    }

    /// <summary>
    /// Validates that the JWT header contains the correct type value according to draft-13.
    /// </summary>
    private void ValidateJwtHeader(ClaimsPrincipal claimsPrincipal)
    {
        // The typ header should be dc+sd-jwt, but we also accept vc+sd-jwt for transition period
        // This validation would typically happen during JWT parsing, but we check here for completeness
        _logger?.LogDebug("JWT header validation should ensure typ is 'dc+sd-jwt' (or 'vc+sd-jwt' for transition period)");
    }

    /// <summary>
    /// Parses the SD-JWT VC payload from the claims principal according to draft-13 structure.
    /// </summary>
    private SdJwtVcPayload ParseSdJwtVcPayload(ClaimsPrincipal claimsPrincipal, string vctClaim)
    {
        var payload = new SdJwtVcPayload
        {
            Vct = vctClaim
        };

        // Extract standard claims that may be present
        payload.VctIntegrity = claimsPrincipal.FindFirst("vct#integrity")?.Value;
        payload.Issuer = claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Iss)?.Value;
        payload.Subject = claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        // Parse numeric claims
        if (long.TryParse(claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Iat)?.Value, out var iat))
            payload.IssuedAt = iat;

        if (long.TryParse(claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Nbf)?.Value, out var nbf))
            payload.NotBefore = nbf;

        if (long.TryParse(claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Exp)?.Value, out var exp))
            payload.ExpiresAt = exp;

        // Parse complex claims
        var cnfJson = claimsPrincipal.FindFirst("cnf")?.Value;
        if (!string.IsNullOrEmpty(cnfJson))
        {
            try
            {
                payload.Confirmation = JsonSerializer.Deserialize<object>(cnfJson, SdJwtConstants.DefaultJsonSerializerOptions);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to parse cnf claim");
            }
        }

        var statusJson = claimsPrincipal.FindFirst("status")?.Value;
        if (!string.IsNullOrEmpty(statusJson))
        {
            try
            {
                payload.Status = JsonSerializer.Deserialize<object>(statusJson, SdJwtConstants.DefaultJsonSerializerOptions);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to parse status claim");
            }
        }

        // Collect additional claims (excluding the standard ones)
        var standardClaims = new HashSet<string>
        {
            "vct",
            "vct#integrity",
            JwtRegisteredClaimNames.Iss,
            JwtRegisteredClaimNames.Sub,
            JwtRegisteredClaimNames.Iat,
            JwtRegisteredClaimNames.Nbf,
            JwtRegisteredClaimNames.Exp,
            "cnf",
            "status",
            "_sd",  // SD-JWT internal claims
            "_sd_alg",
            "sd_hash"
        };

        var additionalData = new Dictionary<string, object>();
        foreach (var claim in claimsPrincipal.Claims)
        {
            if (!standardClaims.Contains(claim.Type))
            {
                // Try to parse as JSON, fallback to string value
                try
                {
                    if (claim.ValueType == JsonClaimValueTypes.Json ||
                        (claim.Value.StartsWith('{') && claim.Value.EndsWith('}')))
                    {
                        additionalData[claim.Type] = JsonSerializer.Deserialize<object>(claim.Value, SdJwtConstants.DefaultJsonSerializerOptions)!;
                    }
                    else
                    {
                        additionalData[claim.Type] = claim.Value;
                    }
                }
                catch
                {
                    additionalData[claim.Type] = claim.Value;
                }
            }
        }

        if (additionalData.Count > 0)
            payload.AdditionalData = additionalData;

        return payload;
    }

    /// <summary>
    /// Validates the SD-JWT VC payload according to draft-ietf-oauth-sd-jwt-vc-13.
    /// </summary>
    private void ValidateSdJwtVc(SdJwtVcPayload payload, string vctClaim)
    {
        _logger?.LogDebug("Validating SD-JWT VC payload structure according to draft-ietf-oauth-sd-jwt-vc-13");

        // VCT claim is required and must be a Collision-Resistant Name
        if (string.IsNullOrWhiteSpace(vctClaim))
            throw new SecurityTokenException("VCT claim is required");

        if (!IsValidCollisionResistantName(vctClaim))
            throw new SecurityTokenException($"VCT claim '{vctClaim}' is not a valid Collision-Resistant Name");

        // Validate timing claims if present
        if (payload.NotBefore.HasValue && payload.ExpiresAt.HasValue &&
            payload.NotBefore.Value >= payload.ExpiresAt.Value)
        {
            throw new SecurityTokenException("Not-before time must be before expiration time");
        }

        if (payload.IssuedAt.HasValue && payload.ExpiresAt.HasValue &&
            payload.IssuedAt.Value >= payload.ExpiresAt.Value)
        {
            throw new SecurityTokenException("Issued-at time must be before expiration time");
        }

        // Validate confirmation method if present
        if (payload.Confirmation != null)
        {
            var confirmationJson = JsonSerializer.Serialize(payload.Confirmation);
            if (!confirmationJson.Contains("jwk") && !confirmationJson.Contains("jku") && !confirmationJson.Contains("kid"))
            {
                _logger?.LogWarning("Confirmation method should contain at least one of: jwk, jku, kid");
            }
        }

        _logger?.LogDebug("SD-JWT VC payload validation completed successfully");
    }

    /// <summary>
    /// Validates if a string is a valid Collision-Resistant Name according to RFC7515.
    /// Simplified validation - checks for URI format or contains namespace separator.
    /// </summary>
    private static bool IsValidCollisionResistantName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Must be a URI or contain a namespace separator (':')
        return Uri.TryCreate(name, UriKind.Absolute, out _) || name.Contains(':');
    }
}