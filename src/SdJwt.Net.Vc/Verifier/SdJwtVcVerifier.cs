using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Vc.Metadata;
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
    /// Initializes a verifier with a metadata-based issuer signing key resolver.
    /// </summary>
    /// <param name="issuerSigningKeyResolver">Resolver used to determine issuer signing keys from metadata.</param>
    /// <param name="logger">An optional logger for diagnostics.</param>
    public SdJwtVcVerifier(IJwtVcIssuerSigningKeyResolver issuerSigningKeyResolver, ILogger<SdJwtVcVerifier>? logger = null)
        : this(
            token => issuerSigningKeyResolver.ResolveSigningKeyAsync(token),
            logger)
    {
        if (issuerSigningKeyResolver == null)
        {
            throw new ArgumentNullException(nameof(issuerSigningKeyResolver));
        }
    }

    /// <summary>
    /// Initializes a verifier that resolves issuer signing keys via JWT VC Issuer Metadata.
    /// </summary>
    /// <param name="issuerMetadataResolver">Issuer metadata resolver.</param>
    /// <param name="httpClient">HTTP client used to retrieve remote JWKS when <c>jwks_uri</c> is provided.</param>
    /// <param name="keyResolverOptions">Optional key resolver options.</param>
    /// <param name="logger">An optional logger for diagnostics.</param>
    public SdJwtVcVerifier(
        IJwtVcIssuerMetadataResolver issuerMetadataResolver,
        HttpClient httpClient,
        JwtVcIssuerSigningKeyResolverOptions? keyResolverOptions = null,
        ILogger<SdJwtVcVerifier>? logger = null)
        : this(
            new JwtVcIssuerSigningKeyResolver(issuerMetadataResolver, httpClient, keyResolverOptions),
            logger)
    {
        if (issuerMetadataResolver == null)
        {
            throw new ArgumentNullException(nameof(issuerMetadataResolver));
        }
        if (httpClient == null)
        {
            throw new ArgumentNullException(nameof(httpClient));
        }
    }

    /// <summary>
    /// Verifies an SD-JWT VC according to draft-ietf-oauth-sd-jwt-vc-13 and returns a strongly-typed verification result.
    /// </summary>
    /// <param name="presentation">The presentation string from the Holder.</param>
    /// <param name="validationParameters">Token validation parameters to apply to the main SD-JWT.</param>
    /// <param name="kbJwtValidationParameters">Optional validation parameters for the Key Binding JWT.</param>
    /// <param name="expectedKbJwtNonce">Optional expected nonce value to verify against the Key Binding JWT.</param>
    /// <param name="expectedVctType">Optional expected VCT identifier to validate against.</param>
    /// <param name="validateTypeIntegrity">Whether to validate VCT integrity hash if present.</param>
    /// <param name="verificationPolicy">Optional verification policy for metadata, legacy typ handling, and status validation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="SdJwtVcVerificationResult"/> containing the rehydrated claims and SD-JWT VC payload if verification is successful.</returns>
    public async Task<SdJwtVcVerificationResult> VerifyAsync(
        string presentation,
        TokenValidationParameters validationParameters,
        TokenValidationParameters? kbJwtValidationParameters = null,
        string? expectedKbJwtNonce = null,
        string? expectedVctType = null,
        bool validateTypeIntegrity = true,
        SdJwtVcVerificationPolicy? verificationPolicy = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Starting SD-JWT VC verification according to draft-ietf-oauth-sd-jwt-vc-13");
        verificationPolicy ??= new SdJwtVcVerificationPolicy();

        // Use the base verifier for core SD-JWT verification
        VerificationResult baseResult;
        try
        {
            baseResult = await _baseSdVerifier.VerifyAsync(presentation, validationParameters, kbJwtValidationParameters, expectedKbJwtNonce);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Base SD-JWT verification failed.");
            throw;
        }

        if (baseResult == null)
        {
            _logger?.LogError("Base SD-JWT verification returned null result.");
            throw new InvalidOperationException("Base SD-JWT verification returned null result.");
        }

        // Validate typ header
        // We need to parse the header from the presentation string (first part)
        var parts = presentation.Split('.');
        if (parts.Length > 0)
        {
            try
            {
                var headerJson = Base64UrlEncoder.Decode(parts[0]);
                using var doc = JsonDocument.Parse(headerJson);
                if (doc.RootElement.TryGetProperty("typ", out var typElement))
                {
                    var typ = typElement.GetString();
                    var allowedTypValues = verificationPolicy.AcceptLegacyTyp
                        ? new[] { SdJwtConstants.SdJwtVcTypeName, SdJwtConstants.SdJwtVcLegacyTypeName }
                        : new[] { SdJwtConstants.SdJwtVcTypeName };
                    if (!allowedTypValues.Contains(typ, StringComparer.Ordinal))
                    {
                        throw new SecurityTokenException($"Invalid typ header: '{typ}'. Expected '{string.Join("' or '", allowedTypValues)}'.");
                    }
                }
                else
                {
                    // typ is REQUIRED for SD-JWT VC
                    throw new SecurityTokenException("Missing required 'typ' header.");
                }
            }
            catch (Exception ex) when (ex is not SecurityTokenException)
            {
                _logger?.LogWarning(ex, "Failed to parse header for typ validation");
                // If we can't parse the header, we should fail validation
                throw new SecurityTokenException("Invalid JWT header format - cannot validate typ claim.", ex);
            }
        }

        // Extract and validate required VCT claim
        var vctClaim = baseResult.ClaimsPrincipal.FindFirst("vct")?.Value
            ?? throw new SecurityTokenException("Missing required 'vct' claim in SD-JWT VC.");

        // Validate required iss claim
        if (!baseResult.ClaimsPrincipal.HasClaim(c => c.Type == JwtRegisteredClaimNames.Iss))
        {
            throw new SecurityTokenException("Missing required 'iss' claim in SD-JWT VC.");
        }

        _logger?.LogDebug("Found vct claim: {VctClaim}", vctClaim);

        // Validate expected VCT type if provided
        var effectiveExpectedVct = !string.IsNullOrEmpty(expectedVctType) ? expectedVctType : verificationPolicy.ExpectedVctType;
        if (!string.IsNullOrEmpty(effectiveExpectedVct) && vctClaim != effectiveExpectedVct)
            throw new SecurityTokenException($"Expected VCT type '{effectiveExpectedVct}' but found '{vctClaim}'");

        // Validate VCT integrity if present and required
        if (validateTypeIntegrity)
        {
            await ValidateTypeMetadataAsync(vctClaim, baseResult.ClaimsPrincipal, verificationPolicy, cancellationToken).ConfigureAwait(false);
        }
        else if (verificationPolicy.RequireTypeMetadata)
        {
            await ValidateTypeMetadataAsync(vctClaim, baseResult.ClaimsPrincipal, verificationPolicy, cancellationToken).ConfigureAwait(false);
        }

        // Parse the SD-JWT VC payload from claims
        var sdJwtVcPayload = ParseSdJwtVcPayload(baseResult.ClaimsPrincipal, vctClaim);

        // Validate SD-JWT VC structure according to draft-13
        ValidateSdJwtVc(sdJwtVcPayload, vctClaim);
        await ValidateStatusAsync(sdJwtVcPayload, verificationPolicy, cancellationToken).ConfigureAwait(false);

        _logger?.LogInformation("SD-JWT VC verification completed successfully");

        return new SdJwtVcVerificationResult(baseResult.ClaimsPrincipal, baseResult.KeyBindingVerified, vctClaim, sdJwtVcPayload);
    }

    /// <summary>
    /// Verifies an SD-JWT VC in JSON serialization format according to draft-ietf-oauth-sd-jwt-vc-13.
    /// </summary>
    /// <param name="jsonSerialization">The SD-JWT VC in JWS JSON Serialization format.</param>
    /// <param name="validationParameters">Token validation parameters.</param>
    /// <param name="kbJwtValidationParameters">Optional KB-JWT validation parameters.</param>
    /// <param name="expectedKbJwtNonce">Optional expected nonce value to verify against the Key Binding JWT.</param>
    /// <param name="expectedVctType">Optional expected VCT identifier.</param>
    /// <param name="validateTypeIntegrity">Whether to validate VCT integrity hash if present.</param>
    /// <param name="verificationPolicy">Optional verification policy for metadata, legacy typ handling, and status validation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A verification result with SD-JWT VC specific information.</returns>
    public async Task<SdJwtVcVerificationResult> VerifyJsonSerializationAsync(
        string jsonSerialization,
        TokenValidationParameters validationParameters,
        TokenValidationParameters? kbJwtValidationParameters = null,
        string? expectedKbJwtNonce = null,
        string? expectedVctType = null,
        bool validateTypeIntegrity = true,
        SdJwtVcVerificationPolicy? verificationPolicy = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Starting SD-JWT VC JSON serialization verification");
        verificationPolicy ??= new SdJwtVcVerificationPolicy();

        // Use the base verifier for core verification
        var baseResult = await _baseSdVerifier.VerifyJsonSerializationAsync(jsonSerialization, validationParameters, kbJwtValidationParameters, expectedKbJwtNonce);

        if (baseResult == null)
        {
            _logger?.LogError("Base SD-JWT verification returned null result.");
            throw new InvalidOperationException("Base SD-JWT verification returned null result.");
        }

        // Extract and validate VCT claim
        var vctClaim = baseResult.ClaimsPrincipal.FindFirst("vct")?.Value
            ?? throw new SecurityTokenException("Missing required 'vct' claim in SD-JWT VC.");

        // Validate required iss claim
        if (!baseResult.ClaimsPrincipal.HasClaim(c => c.Type == JwtRegisteredClaimNames.Iss))
        {
            throw new SecurityTokenException("Missing required 'iss' claim in SD-JWT VC.");
        }

        var effectiveExpectedVct = !string.IsNullOrEmpty(expectedVctType) ? expectedVctType : verificationPolicy.ExpectedVctType;
        if (!string.IsNullOrEmpty(effectiveExpectedVct) && vctClaim != effectiveExpectedVct)
            throw new SecurityTokenException($"Expected VCT type '{effectiveExpectedVct}' but found '{vctClaim}'");

        if (validateTypeIntegrity || verificationPolicy.RequireTypeMetadata)
        {
            await ValidateTypeMetadataAsync(vctClaim, baseResult.ClaimsPrincipal, verificationPolicy, cancellationToken).ConfigureAwait(false);
        }

        // Parse and validate payload
        var sdJwtVcPayload = ParseSdJwtVcPayload(baseResult.ClaimsPrincipal, vctClaim);
        ValidateSdJwtVc(sdJwtVcPayload, vctClaim);
        await ValidateStatusAsync(sdJwtVcPayload, verificationPolicy, cancellationToken).ConfigureAwait(false);

        _logger?.LogInformation("SD-JWT VC JSON serialization verification completed successfully");

        return new SdJwtVcVerificationResult(baseResult.ClaimsPrincipal, baseResult.KeyBindingVerified, vctClaim, sdJwtVcPayload);
    }

    /// <summary>
    /// Verifies an SD-JWT VC in the context of an OID4VP presentation according to draft-ietf-oauth-sd-jwt-vc-13.
    /// This method includes additional validations specific to OID4VP flows such as nonce and audience validation.
    /// </summary>
    /// <param name="presentation">The presentation string from the Holder.</param>
    /// <param name="validationParameters">Token validation parameters to apply to the main SD-JWT.</param>
    /// <param name="expectedNonce">The nonce from the authorization request that must match the KB-JWT.</param>
    /// <param name="expectedAudience">The expected audience (client_id) for the presentation.</param>
    /// <param name="kbJwtValidationParameters">Optional validation parameters for the Key Binding JWT.</param>
    /// <param name="expectedVctType">Optional expected VCT identifier to validate against.</param>
    /// <param name="validateTypeIntegrity">Whether to validate VCT integrity hash if present.</param>
    /// <param name="verificationPolicy">Optional verification policy for metadata, legacy typ handling, and status validation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="SdJwtVcVerificationResult"/> containing the rehydrated claims and SD-JWT VC payload if verification is successful.</returns>
    public async Task<SdJwtVcVerificationResult> VerifyForOid4VpAsync(
        string presentation,
        TokenValidationParameters validationParameters,
        string expectedNonce,
        string expectedAudience,
        TokenValidationParameters? kbJwtValidationParameters = null,
        string? expectedVctType = null,
        bool validateTypeIntegrity = true,
        SdJwtVcVerificationPolicy? verificationPolicy = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Starting SD-JWT VC verification for OID4VP presentation");

        // Ensure nonce validation is configured for OID4VP
        if (string.IsNullOrEmpty(expectedNonce))
        {
            throw new SecurityTokenException("Nonce is required for OID4VP presentation verification");
        }

        // Configure KB-JWT validation for OID4VP if not provided
        kbJwtValidationParameters ??= new TokenValidationParameters
        {
            ValidateIssuer = false, // KB-JWT issuer should match holder
            ValidateAudience = true,
            ValidAudience = expectedAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            ValidateIssuerSigningKey = true
        };

        // Perform standard SD-JWT VC verification with nonce validation
        var result = await VerifyAsync(presentation, validationParameters, kbJwtValidationParameters,
            expectedNonce, expectedVctType, validateTypeIntegrity, verificationPolicy, cancellationToken);

        // Additional OID4VP-specific validations (audience and freshness)
        // Note: Nonce is already validated by VerifyAsync above
        ValidateOid4VpKbJwt(presentation, expectedAudience);

        _logger?.LogInformation("SD-JWT VC verification for OID4VP completed successfully");
        return result;
    }

    /// <summary>
    /// Validates the Key Binding JWT for OID4VP-specific requirements (audience validation).
    /// Note: Nonce validation is handled by the base SdVerifier.
    /// </summary>
    private void ValidateOid4VpKbJwt(string presentation, string expectedAudience)
    {
        _logger?.LogDebug("Performing OID4VP Key Binding JWT validation");

        // Extract Key Binding JWT
        var parts = presentation.Split('~');
        if (parts.Length < 2)
        {
            throw new SecurityTokenException("Invalid SD-JWT VC format: missing key binding JWT for OID4VP");
        }

        var kbJwtPart = parts.LastOrDefault(p => !string.IsNullOrEmpty(p));
        if (string.IsNullOrEmpty(kbJwtPart))
        {
            throw new SecurityTokenException("Key Binding JWT is required for OID4VP presentation");
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var kbJwt = handler.ReadJwtToken(kbJwtPart);

            // Note: Nonce validation is handled by the base SdVerifier via expectedKbJwtNonce parameter
            // We only need to validate audience here

            // Validate audience claim (OID4VP Section 8.6)
            var audClaim = kbJwt.Claims.FirstOrDefault(c => c.Type == "aud")?.Value;
            if (string.IsNullOrEmpty(audClaim))
            {
                throw new SecurityTokenException("KB-JWT must contain 'aud' claim for OID4VP");
            }

            if (audClaim != expectedAudience)
            {
                throw new SecurityTokenException($"KB-JWT audience '{audClaim}' does not match expected audience");
            }

            // Validate iat claim for freshness (OID4VP Section 14.1)
            var iatClaim = kbJwt.Claims.FirstOrDefault(c => c.Type == "iat");
            if (iatClaim == null)
            {
                throw new SecurityTokenException("KB-JWT must contain 'iat' claim for OID4VP");
            }

            if (long.TryParse(iatClaim.Value, out var iat))
            {
                var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iat);
                var maxAge = TimeSpan.FromMinutes(10); // Configurable for replay protection

                if (DateTimeOffset.UtcNow - issuedAt > maxAge)
                {
                    throw new SecurityTokenException($"KB-JWT is too old for OID4VP (issued at {issuedAt})");
                }
            }

            _logger?.LogDebug("OID4VP Key Binding JWT validation completed successfully");
        }
        catch (Exception ex) when (ex is not SecurityTokenException)
        {
            _logger?.LogError(ex, "Failed to validate OID4VP Key Binding JWT");
            throw new SecurityTokenException("Failed to validate Key Binding JWT for OID4VP", ex);
        }
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
        payload.IssuedAt = GetNumericDateClaim(claimsPrincipal, JwtRegisteredClaimNames.Iat);
        payload.NotBefore = GetNumericDateClaim(claimsPrincipal, JwtRegisteredClaimNames.Nbf);
        payload.ExpiresAt = GetNumericDateClaim(claimsPrincipal, JwtRegisteredClaimNames.Exp);

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

    private static long? GetNumericDateClaim(ClaimsPrincipal claimsPrincipal, string claimName)
    {
        var claim = claimsPrincipal.FindFirst(claimName);

        // Try alternative claim mappings for exp and nbf
        if (claim == null)
        {
            // Try direct string claim names as fallback
            if (claimName == JwtRegisteredClaimNames.Exp)
            {
                claim = claimsPrincipal.FindFirst("exp") ?? claimsPrincipal.FindFirst(ClaimTypes.Expiration);
            }
            else if (claimName == JwtRegisteredClaimNames.Nbf)
            {
                claim = claimsPrincipal.FindFirst("nbf");
            }
            else if (claimName == JwtRegisteredClaimNames.Iat)
            {
                claim = claimsPrincipal.FindFirst("iat");
            }
        }

        if (claim == null)
            return null;

        if (long.TryParse(claim.Value, out var val))
            return val;

        // Try parsing as DateTime if it was converted
        if (DateTime.TryParse(claim.Value, out var date))
        {
            return new DateTimeOffset(date).ToUnixTimeSeconds();
        }

        return null;
    }

    private static async Task ValidateTypeMetadataAsync(
        string vctClaim,
        ClaimsPrincipal claimsPrincipal,
        SdJwtVcVerificationPolicy verificationPolicy,
        CancellationToken cancellationToken)
    {
        var resolver = verificationPolicy.TypeMetadataResolver;
        var requiresResolution = verificationPolicy.RequireTypeMetadata || !string.IsNullOrWhiteSpace(claimsPrincipal.FindFirst(SdJwtConstants.VctIntegrityClaim)?.Value);
        if (!requiresResolution)
        {
            return;
        }

        if (resolver == null)
        {
            throw new SecurityTokenException("Type metadata validation requires a configured type metadata resolver.");
        }

        var resolution = await resolver.ResolveAsync(vctClaim, cancellationToken).ConfigureAwait(false);
        if (!string.Equals(resolution.Metadata.Vct, vctClaim, StringComparison.Ordinal))
        {
            throw new SecurityTokenException("Type metadata 'vct' does not match credential vct.");
        }

        var vctIntegrity = claimsPrincipal.FindFirst(SdJwtConstants.VctIntegrityClaim)?.Value;
        if (!string.IsNullOrWhiteSpace(vctIntegrity) &&
            !IntegrityMetadataValidator.Validate(resolution.RawJson, vctIntegrity))
        {
            throw new SecurityTokenException("vct#integrity validation failed.");
        }
    }

    private static async Task ValidateStatusAsync(
        SdJwtVcPayload payload,
        SdJwtVcVerificationPolicy verificationPolicy,
        CancellationToken cancellationToken)
    {
        if (payload.Status == null)
        {
            return;
        }

        if (!verificationPolicy.RequireStatusCheck)
        {
            return;
        }

        if (verificationPolicy.StatusValidator == null)
        {
            throw new SecurityTokenException("Status validation is required by policy but no status validator is configured.");
        }

        var isStatusValid = await verificationPolicy.StatusValidator.ValidateAsync(payload.Status, cancellationToken).ConfigureAwait(false);
        if (!isStatusValid)
        {
            throw new SecurityTokenException("Credential status validation failed.");
        }
    }
}
