using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Models;
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Utils;
using SdJwt.Net.Verifier;
using SdJwt.Net.Vc.Verifier;

namespace SdJwt.Net.Oid4Vp.Verifier;

/// <summary>
/// Validates VP tokens in OID4VP authorization responses.
/// Performs comprehensive validation including signature verification and key binding.
/// </summary>
public class VpTokenValidator
{
    private readonly ILogger<VpTokenValidator>? _logger;
    private readonly SdVerifier _sdVerifier;
    private readonly SdJwtVcVerifier? _vcVerifier;
    private readonly bool _useSdJwtVcValidation;

    /// <summary>
    /// Initializes a new instance of the VpTokenValidator class.
    /// </summary>
    /// <param name="keyProvider">Key provider for signature verification</param>
    /// <param name="useSdJwtVcValidation">Whether to use SD-JWT VC specific validation (recommended for OID4VP compliance)</param>
    /// <param name="logger">Optional logger</param>
    public VpTokenValidator(
        Func<JwtSecurityToken, Task<SecurityKey>> keyProvider,
        bool useSdJwtVcValidation = true,
        ILogger<VpTokenValidator>? logger = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(keyProvider);
#else
        if (keyProvider == null)
            throw new ArgumentNullException(nameof(keyProvider));
#endif

        _logger = logger;
        _sdVerifier = new SdVerifier(keyProvider);
        _useSdJwtVcValidation = useSdJwtVcValidation;

        if (useSdJwtVcValidation)
        {
            // Create SD-JWT VC verifier without logger to avoid type mismatch
            _vcVerifier = new SdJwtVcVerifier(keyProvider);
            _logger?.LogInformation("VpTokenValidator initialized with SD-JWT VC validation enabled");
        }
        else
        {
            _logger?.LogWarning("VpTokenValidator initialized with generic SD-JWT validation (SD-JWT VC validation disabled)");
        }
    }

    /// <summary>
    /// Validates an authorization response containing VP tokens.
    /// </summary>
    /// <param name="response">The authorization response to validate</param>
    /// <param name="expectedNonce">The expected nonce value from the authorization request</param>
    /// <param name="options">Validation options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    public async Task<VpTokenValidationResult> ValidateAsync(
        AuthorizationResponse response,
        string expectedNonce,
        VpTokenValidationOptions options,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedNonce);
        ArgumentNullException.ThrowIfNull(options);
#else
        if (response == null)
            throw new ArgumentNullException(nameof(response));
        if (string.IsNullOrWhiteSpace(expectedNonce))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(expectedNonce));
        if (options == null)
            throw new ArgumentNullException(nameof(options));
#endif

        try
        {
            // Validate response structure
            response.Validate();

            if (response.IsError)
            {
                _logger?.LogWarning("Authorization response contains error: {Error} - {Description}",
                    response.Error, response.ErrorDescription);
                return VpTokenValidationResult.Failed($"Authorization response error: {response.Error}");
            }

            if (!response.HasVpTokens)
            {
                _logger?.LogWarning("Authorization response does not contain VP tokens");
                return VpTokenValidationResult.Failed("No VP tokens in response");
            }

            var vpTokens = response.GetVpTokens();
            if (options.ValidatePresentationSubmission)
            {
                var submissionValidation = ValidatePresentationSubmission(response.PresentationSubmission, vpTokens, options);
                if (!submissionValidation.IsValid)
                {
                    _logger?.LogWarning("Presentation submission validation failed: {Error}", submissionValidation.ErrorMessage);
                    return VpTokenValidationResult.Failed(submissionValidation.ErrorMessage ?? "Presentation submission validation failed");
                }
            }

            var results = new List<VpTokenResult>();

            // Validate each VP token
            for (int i = 0; i < vpTokens.Length; i++)
            {
                var token = vpTokens[i];
                _logger?.LogDebug("Validating VP token {Index}: {Token}", i,
                    token.Length > 50 ? $"{token[..50]}..." : token);

                var result = await ValidateVpTokenAsync(token, expectedNonce, options, cancellationToken);
                results.Add(new VpTokenResult
                {
                    Index = i,
                    Token = token,
                    IsValid = result.IsValid,
                    Error = result.Error,
                    VerificationResult = result.IsValid ? result.VerificationResult : null,
                    Claims = result.IsValid ? result.Claims : new Dictionary<string, object>()
                });

                if (!result.IsValid && options.StopOnFirstFailure)
                {
                    _logger?.LogWarning("VP token validation failed at index {Index}: {Error}", i, result.Error);
                    break;
                }
            }

            var allValid = results.All(r => r.IsValid);
            var validCount = results.Count(r => r.IsValid);

            _logger?.LogInformation("VP token validation completed. Valid: {ValidCount}/{TotalCount}",
                validCount, results.Count);

            return new VpTokenValidationResult
            {
                IsValid = allValid,
                ValidatedTokens = results.ToArray(),
                PresentationSubmission = response.PresentationSubmission
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "VP token validation failed with exception");
            return VpTokenValidationResult.Failed($"Validation exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates a single VP token string.
    /// </summary>
    /// <param name="vpToken">The VP token to validate</param>
    /// <param name="expectedNonce">The expected nonce value</param>
    /// <param name="options">Validation options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Single token validation result</returns>
    public async Task<SingleVpTokenResult> ValidateVpTokenAsync(
        string vpToken,
        string expectedNonce,
        VpTokenValidationOptions options,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(vpToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedNonce);
        ArgumentNullException.ThrowIfNull(options);
#else
        if (string.IsNullOrWhiteSpace(vpToken))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(vpToken));
        if (string.IsNullOrWhiteSpace(expectedNonce))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(expectedNonce));
        if (options == null)
            throw new ArgumentNullException(nameof(options));
#endif

        try
        {
            // Parse the presentation
            var parsed = SdJwtParser.ParsePresentation(vpToken);
            _logger?.LogDebug("Parsed presentation with {DisclosureCount} disclosures and key binding: {HasKeyBinding}",
                parsed.Disclosures.Count, !string.IsNullOrEmpty(parsed.RawKeyBindingJwt));

            // Validate key binding JWT is present for presentations
            if (string.IsNullOrEmpty(parsed.RawKeyBindingJwt))
            {
                _logger?.LogWarning("VP token missing required key binding JWT");
                return SingleVpTokenResult.Failed("Key binding JWT is required for presentations");
            }

            // Create validation parameters
            var validationParams = CreateValidationParameters(options);
            var kbValidationParams = CreateKeyBindingValidationParameters(options);

            // Verify the presentation using the appropriate verifier
            VerificationResult verificationResult;

            if (_useSdJwtVcValidation && _vcVerifier != null)
            {
                _logger?.LogDebug("Using SD-JWT VC specific verification for OID4VP compliance");

                // Use SD-JWT VC verifier for enhanced validation
                // This validates: vct claim, iss claim, typ header, collision-resistant names
                verificationResult = await _vcVerifier.VerifyAsync(
                    vpToken, validationParams, kbValidationParams, expectedNonce);
            }
            else
            {
                _logger?.LogDebug("Using generic SD-JWT verification");

                // Fallback to generic verification
                verificationResult = await _sdVerifier.VerifyAsync(
                    vpToken, validationParams, kbValidationParams, expectedNonce);
            }

            if (!verificationResult.KeyBindingVerified)
            {
                _logger?.LogWarning("Key binding verification failed");
                return SingleVpTokenResult.Failed("Key binding verification failed");
            }

            // Extract and validate claims
            var claims = ExtractClaims(verificationResult);

            // Note: Nonce validation is performed by the base verifier via expectedNonce parameter
            // No need for manual validation here as it's already been validated

            // Validate key binding JWT freshness (OID4VP Section 14.1)
            if (!ValidateKeyBindingFreshness(parsed.RawKeyBindingJwt, options))
            {
                _logger?.LogWarning("Key binding JWT freshness validation failed");
                return SingleVpTokenResult.Failed("Key binding JWT is too old or missing iat claim");
            }

            // Additional custom validations
            if (options.CustomValidation != null)
            {
                var customResult = await options.CustomValidation(verificationResult, cancellationToken);
                if (!customResult.IsValid)
                {
                    _logger?.LogWarning("Custom validation failed: {Error}", customResult.ErrorMessage);
                    return SingleVpTokenResult.Failed($"Custom validation failed: {customResult.ErrorMessage}");
                }
            }

            // Validate optional OID4VP request bindings.
            var bindingResult = ValidateOid4VpBindings(parsed.RawKeyBindingJwt, options);
            if (!bindingResult.IsValid)
            {
                return SingleVpTokenResult.Failed(bindingResult.ErrorMessage ?? "OID4VP binding validation failed");
            }

            _logger?.LogDebug("VP token validation successful");
            return new SingleVpTokenResult
            {
                IsValid = true,
                VerificationResult = verificationResult,
                Claims = claims
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "VP token validation failed with exception");
            return SingleVpTokenResult.Failed($"Validation exception: {ex.Message}");
        }
    }

    private static TokenValidationParameters CreateValidationParameters(VpTokenValidationOptions options)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = options.ValidateIssuer,
            ValidIssuers = options.ValidIssuers,
            ValidateAudience = options.ValidateAudience,
            ValidAudiences = options.ValidAudiences,
            ValidateLifetime = options.ValidateLifetime,
            ClockSkew = options.ClockSkew,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            RequireExpirationTime = options.RequireExpirationTime
        };
    }

    private static TokenValidationParameters CreateKeyBindingValidationParameters(
        VpTokenValidationOptions options)
    {
        // Note: Nonce validation is handled by SdVerifier.VerifyAsync via expectedKbJwtNonce parameter,
        // not through TokenValidationParameters which doesn't support nonce validation.

        var validAudiences = options.ValidKeyBindingAudiences?.ToList() ?? new List<string>();

        // Add ExpectedClientId if provided and not already in the list
        if (!string.IsNullOrEmpty(options.ExpectedClientId) && !validAudiences.Contains(options.ExpectedClientId))
        {
            validAudiences.Add(options.ExpectedClientId);
        }

        return new TokenValidationParameters
        {
            ValidateAudience = options.ValidateKeyBindingAudience,
            ValidAudiences = validAudiences.Any() ? validAudiences : null,
            ValidAudience = !string.IsNullOrEmpty(options.ExpectedClientId) ? options.ExpectedClientId : null,
            ValidateLifetime = options.ValidateKeyBindingLifetime,
            ClockSkew = options.ClockSkew,
            ValidateIssuer = false, // Key binding JWTs typically don't have issuers
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true
        };
    }

    private static Dictionary<string, object> ExtractClaims(VerificationResult verificationResult)
    {
        var claims = new Dictionary<string, object>();

        // Add claims from the verified principal
        foreach (var claim in verificationResult.ClaimsPrincipal.Claims)
        {
            if (claims.ContainsKey(claim.Type))
            {
                // Handle multiple claims with the same type
                if (claims[claim.Type] is List<string> list)
                {
                    list.Add(claim.Value);
                }
                else
                {
                    claims[claim.Type] = new List<string> { claims[claim.Type].ToString()!, claim.Value };
                }
            }
            else
            {
                claims[claim.Type] = claim.Value;
            }
        }

        return claims;
    }

    // Note: ValidateKeyBindingNonce method removed - nonce validation is now handled
    // by the base SdVerifier.VerifyAsync via the expectedKbJwtNonce parameter

    private bool ValidateKeyBindingFreshness(string? keyBindingJwt, VpTokenValidationOptions options)
    {
        if (!options.ValidateKeyBindingFreshness)
        {
            return true; // Freshness validation disabled
        }

        if (string.IsNullOrEmpty(keyBindingJwt))
        {
            return false;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(keyBindingJwt);

            // Check for iat claim (required for freshness validation)
            // Access directly from payload to avoid claim type transformation issues
            if (!token.Payload.TryGetValue("iat", out var iatObj))
            {
                _logger?.LogWarning("Key binding JWT missing required 'iat' claim for freshness validation");
                return false;
            }

            long iat;
            if (iatObj is long l)
            {
                iat = l;
            }
            else if (iatObj is int i)
            {
                iat = i;
            }
            else if (iatObj is string s && long.TryParse(s, out var parsed))
            {
                iat = parsed;
            }
            else
            {
                _logger?.LogWarning("Failed to parse iat claim value from payload");
                return false;
            }

            var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iat);
            var age = DateTimeOffset.UtcNow - issuedAt;
            var maxAge = options.MaxKeyBindingAge + options.ClockSkew;

            if (age > maxAge)
            {
                _logger?.LogWarning(
                    "Key binding JWT is too old. Issued at: {IssuedAt}, Age: {Age}, Max allowed: {MaxAge}",
                    issuedAt, age, maxAge);
                return false;
            }

            _logger?.LogDebug(
                "Key binding JWT freshness validated. Age: {Age}, Max allowed: {MaxAge}",
                age, maxAge);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error validating key binding JWT freshness");
            return false;
        }
    }

    private CustomValidationResult ValidateOid4VpBindings(string? keyBindingJwt, VpTokenValidationOptions options)
    {
        if (string.IsNullOrWhiteSpace(keyBindingJwt))
        {
            return CustomValidationResult.Failed("Key binding JWT is required for OID4VP binding checks");
        }

        JwtSecurityToken token;
        try
        {
            token = new JwtSecurityTokenHandler().ReadJwtToken(keyBindingJwt);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to parse key binding JWT for OID4VP binding checks");
            return CustomValidationResult.Failed("Failed to parse key binding JWT");
        }

        if (!string.IsNullOrWhiteSpace(options.ExpectedWalletNonce) &&
            !ValidateWalletNonceBinding(token, options.ExpectedWalletNonce!))
        {
            return CustomValidationResult.Failed("wallet_nonce binding validation failed");
        }

        if (!string.IsNullOrWhiteSpace(options.ExpectedTransactionData) &&
            !ValidateTransactionDataBinding(token, options.ExpectedTransactionData!))
        {
            return CustomValidationResult.Failed("transaction_data binding validation failed");
        }

        if (options.ValidateVerifierInfo)
        {
            var verifierInfoValidation = ValidateVerifierInfo(options);
            if (!verifierInfoValidation.IsValid)
            {
                return verifierInfoValidation;
            }
        }

        return CustomValidationResult.Success();
    }

    private static bool ValidateWalletNonceBinding(JwtSecurityToken keyBindingJwt, string expectedWalletNonce)
    {
        if (!keyBindingJwt.Payload.TryGetValue("wallet_nonce", out var walletNonceObj) || walletNonceObj == null)
        {
            return false;
        }

        var presentedWalletNonce = walletNonceObj.ToString();
        if (string.IsNullOrWhiteSpace(presentedWalletNonce))
        {
            return false;
        }

        return FixedTimeEquals(presentedWalletNonce, expectedWalletNonce);
    }

    private static bool ValidateTransactionDataBinding(JwtSecurityToken keyBindingJwt, string base64UrlTransactionData)
    {
        byte[] transactionDataBytes;
        try
        {
            transactionDataBytes = Base64UrlEncoder.DecodeBytes(base64UrlTransactionData);
        }
        catch
        {
            return false;
        }

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(transactionDataBytes);
        var expectedHash = Base64UrlEncoder.Encode(hashBytes);

        if (keyBindingJwt.Payload.TryGetValue("transaction_data_hash", out var singleHashObj) &&
            singleHashObj is string singleHash &&
            FixedTimeEquals(singleHash, expectedHash))
        {
            return true;
        }

        if (!keyBindingJwt.Payload.TryGetValue("transaction_data_hashes", out var hashesObj) || hashesObj == null)
        {
            return false;
        }

        if (hashesObj is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                var hash = element.GetString();
                return !string.IsNullOrWhiteSpace(hash) && FixedTimeEquals(hash, expectedHash);
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in element.EnumerateArray())
                {
                    var hash = entry.GetString();
                    if (!string.IsNullOrWhiteSpace(hash) && FixedTimeEquals(hash, expectedHash))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        if (hashesObj is IEnumerable<object> hashesEnumerable)
        {
            foreach (var entry in hashesEnumerable)
            {
                if (entry is string hash && FixedTimeEquals(hash, expectedHash))
                {
                    return true;
                }
            }

            return false;
        }

        return hashesObj is string singleValue && FixedTimeEquals(singleValue, expectedHash);
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        if (leftBytes.Length != rightBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static CustomValidationResult ValidateVerifierInfo(VpTokenValidationOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.VerifierInfo))
        {
            return CustomValidationResult.Failed("verifier_info is required when ValidateVerifierInfo is enabled");
        }

        if (!LooksLikeCompactJwt(options.VerifierInfo!))
        {
            return CustomValidationResult.Success();
        }

        JwtSecurityToken verifierInfoJwt;
        try
        {
            verifierInfoJwt = new JwtSecurityTokenHandler().ReadJwtToken(options.VerifierInfo);
        }
        catch (Exception ex)
        {
            return CustomValidationResult.Failed($"Invalid verifier_info JWT: {ex.Message}");
        }

        if (options.ValidVerifierInfoIssuers != null && options.ValidVerifierInfoIssuers.Any())
        {
            var iss = verifierInfoJwt.Issuer;
            if (string.IsNullOrWhiteSpace(iss) || !options.ValidVerifierInfoIssuers.Contains(iss, StringComparer.Ordinal))
            {
                return CustomValidationResult.Failed("verifier_info issuer is not allowed");
            }
        }

        return CustomValidationResult.Success();
    }

    private static bool LooksLikeCompactJwt(string value)
    {
        var parts = value.Split('.');
        return parts.Length == 3 && parts.All(p => !string.IsNullOrWhiteSpace(p));
    }

    private static CustomValidationResult ValidatePresentationSubmission(
        PresentationSubmission? submission,
        string[] vpTokens,
        VpTokenValidationOptions options)
    {
        if (submission == null)
        {
            return CustomValidationResult.Failed("presentation_submission is required");
        }

        try
        {
            submission.Validate();
        }
        catch (Exception ex)
        {
            return CustomValidationResult.Failed($"presentation_submission is invalid: {ex.Message}");
        }

        if (!string.IsNullOrWhiteSpace(options.ExpectedPresentationDefinitionId) &&
            !FixedTimeEquals(submission.DefinitionId, options.ExpectedPresentationDefinitionId!))
        {
            return CustomValidationResult.Failed("presentation_submission.definition_id does not match expected definition");
        }

        if (options.ExpectedInputDescriptorIds != null)
        {
            var expectedIds = options.ExpectedInputDescriptorIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet(StringComparer.Ordinal);

            if (expectedIds.Count > 0)
            {
                var submittedIds = submission.DescriptorMap.Select(m => m.Id).ToHashSet(StringComparer.Ordinal);
                if (options.RequireAllExpectedInputDescriptors)
                {
                    var missing = expectedIds.Where(id => !submittedIds.Contains(id)).ToArray();
                    if (missing.Length > 0)
                    {
                        return CustomValidationResult.Failed(
                            $"presentation_submission is missing expected descriptor mappings: {string.Join(", ", missing)}");
                    }
                }
                else if (!expectedIds.Overlaps(submittedIds))
                {
                    return CustomValidationResult.Failed("presentation_submission does not contain any expected descriptor mappings");
                }
            }
        }

        var referencedIndexes = new HashSet<int>();
        foreach (var mapping in submission.DescriptorMap)
        {
            var index = ExtractTokenIndex(mapping.Path);
            if (index < 0 || index >= vpTokens.Length)
            {
                return CustomValidationResult.Failed(
                    $"presentation_submission path '{mapping.Path}' references vp_token index {index}, but only {vpTokens.Length} token(s) are present");
            }

            referencedIndexes.Add(index);
        }

        if (referencedIndexes.Count > vpTokens.Length)
        {
            return CustomValidationResult.Failed("presentation_submission references more vp_token entries than provided");
        }

        return CustomValidationResult.Success();
    }

    private static int ExtractTokenIndex(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path == "$")
        {
            return 0;
        }

        var match = Regex.Match(path, @"\[(\d+)\]", RegexOptions.CultureInvariant);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var index))
        {
            return index;
        }

        return 0;
    }
}

/// <summary>
/// Options for VP token validation.
/// </summary>
public class VpTokenValidationOptions
{
    /// <summary>
    /// Gets or sets whether to validate the issuer.
    /// Default: true.
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Gets or sets the valid issuers.
    /// </summary>
    public IEnumerable<string>? ValidIssuers
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether to validate the audience.
    /// Default: false (not typically required for presentations).
    /// </summary>
    public bool ValidateAudience { get; set; } = false;

    /// <summary>
    /// Gets or sets the valid audiences.
    /// </summary>
    public IEnumerable<string>? ValidAudiences
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether to validate token lifetime.
    /// Default: true.
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to require expiration time.
    /// Default: true.
    /// </summary>
    public bool RequireExpirationTime { get; set; } = true;

    /// <summary>
    /// Gets or sets the clock skew allowance.
    /// Default: 5 minutes.
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets whether to validate key binding audience.
    /// Default: true (required for OID4VP compliance per Section 8.6).
    /// </summary>
    public bool ValidateKeyBindingAudience { get; set; } = true;

    /// <summary>
    /// Gets or sets the valid key binding audiences.
    /// </summary>
    public IEnumerable<string>? ValidKeyBindingAudiences
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the expected client ID for key binding audience validation.
    /// When set, this will be used as the expected audience in KB-JWT.
    /// </summary>
    public string? ExpectedClientId
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether to validate key binding lifetime.
    /// Default: true.
    /// </summary>
    public bool ValidateKeyBindingLifetime { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate key binding JWT freshness (iat claim).
    /// Default: true (required for OID4VP replay protection per Section 14.1).
    /// </summary>
    public bool ValidateKeyBindingFreshness { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum age allowed for key binding JWT.
    /// Default: 10 minutes (for replay protection).
    /// </summary>
    public TimeSpan MaxKeyBindingAge { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Gets or sets the expected wallet nonce to validate against KB-JWT claim `wallet_nonce`.
    /// Optional. Used when the verifier maintains wallet nonce state across interactions.
    /// </summary>
    public string? ExpectedWalletNonce
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the expected transaction data (base64url encoded) to validate against
    /// KB-JWT `transaction_data_hash`/`transaction_data_hashes`.
    /// Optional. Used for transaction data binding checks.
    /// </summary>
    public string? ExpectedTransactionData
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether verifier_info validation should be enforced.
    /// Default: false.
    /// </summary>
    public bool ValidateVerifierInfo { get; set; } = false;

    /// <summary>
    /// Gets or sets verifier_info payload value (JWT or string) used for validation checks.
    /// </summary>
    public string? VerifierInfo
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets allowed issuer values when verifier_info is a JWT.
    /// </summary>
    public IEnumerable<string>? ValidVerifierInfoIssuers
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether to stop validation on first failure.
    /// Default: false (validate all tokens).
    /// </summary>
    public bool StopOnFirstFailure { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to validate presentation_submission structure and token references.
    /// Default: true.
    /// </summary>
    public bool ValidatePresentationSubmission { get; set; } = true;

    /// <summary>
    /// Gets or sets the expected presentation definition identifier for response binding.
    /// Optional.
    /// </summary>
    public string? ExpectedPresentationDefinitionId
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets expected input descriptor identifiers that should be present in descriptor_map.
    /// Optional.
    /// </summary>
    public IEnumerable<string>? ExpectedInputDescriptorIds
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether all expected descriptor IDs are required in descriptor_map.
    /// Default: true.
    /// </summary>
    public bool RequireAllExpectedInputDescriptors { get; set; } = true;

    /// <summary>
    /// Gets or sets custom validation function.
    /// </summary>
    public Func<VerificationResult, CancellationToken, Task<CustomValidationResult>>? CustomValidation
    {
        get; set;
    }

    /// <summary>
    /// Creates default validation options suitable for OID4VP compliance.
    /// Enables all security validations including KB-JWT audience and freshness checks.
    /// </summary>
    /// <param name="expectedClientId">The expected client ID (verifier identifier)</param>
    /// <returns>Configured validation options for OID4VP</returns>
    public static VpTokenValidationOptions CreateForOid4Vp(string expectedClientId)
    {
        return new VpTokenValidationOptions
        {
            ValidateIssuer = true,
            ValidateAudience = false,  // Main JWT typically doesn't have audience
            ValidateLifetime = true,
            ValidateKeyBindingAudience = true,
            ValidateKeyBindingLifetime = false,  // KB-JWTs use iat-based freshness validation, not exp-based lifetime
            ValidateKeyBindingFreshness = true,
            ExpectedClientId = expectedClientId,
            MaxKeyBindingAge = TimeSpan.FromMinutes(10),
            ClockSkew = TimeSpan.FromMinutes(5),
            RequireExpirationTime = true,
            StopOnFirstFailure = false
        };
    }

    /// <summary>
    /// Creates relaxed validation options for testing/development.
    /// Disables strict OID4VP validations.
    /// </summary>
    /// <returns>Relaxed validation options</returns>
    public static VpTokenValidationOptions CreateForTesting()
    {
        return new VpTokenValidationOptions
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateKeyBindingAudience = false,
            ValidateKeyBindingLifetime = false,
            ValidateKeyBindingFreshness = false,
            MaxKeyBindingAge = TimeSpan.FromHours(24),
            ClockSkew = TimeSpan.FromMinutes(30),
            RequireExpirationTime = false,
            StopOnFirstFailure = false
        };
    }
}

/// <summary>
/// Result of custom validation.
/// </summary>
public class CustomValidationResult
{
    /// <summary>
    /// Gets or sets whether validation passed.
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the error message if validation failed.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static CustomValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    public static CustomValidationResult Failed(string errorMessage) => new()
    {
        IsValid = false,
        ErrorMessage = errorMessage
    };
}

/// <summary>
/// Result of VP token validation.
/// </summary>
public class VpTokenValidationResult
{
    /// <summary>
    /// Gets or sets whether all tokens are valid.
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the error message if validation failed.
    /// </summary>
    public string? Error
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the validated tokens.
    /// </summary>
    public VpTokenResult[] ValidatedTokens { get; set; } = Array.Empty<VpTokenResult>();

    /// <summary>
    /// Gets or sets the presentation submission.
    /// </summary>
    public PresentationSubmission? PresentationSubmission
    {
        get; set;
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="error">The error message</param>
    public static VpTokenValidationResult Failed(string error) => new()
    {
        IsValid = false,
        Error = error
    };
}

/// <summary>
/// Result of individual VP token validation.
/// </summary>
public class VpTokenResult
{
    /// <summary>
    /// Gets or sets the index of this token in the VP token array.
    /// </summary>
    public int Index
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the VP token string.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this token is valid.
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the error message if validation failed.
    /// </summary>
    public string? Error
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the verification result.
    /// </summary>
    public VerificationResult? VerificationResult
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the extracted claims.
    /// </summary>
    public Dictionary<string, object> Claims { get; set; } = new();
}

/// <summary>
/// Result of single VP token validation.
/// </summary>
public class SingleVpTokenResult
{
    /// <summary>
    /// Gets or sets whether the token is valid.
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the error message if validation failed.
    /// </summary>
    public string? Error
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the verification result.
    /// </summary>
    public VerificationResult? VerificationResult
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the extracted claims.
    /// </summary>
    public Dictionary<string, object> Claims { get; set; } = new();

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="error">The error message</param>
    public static SingleVpTokenResult Failed(string error) => new()
    {
        IsValid = false,
        Error = error
    };
}
