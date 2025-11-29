using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Models;
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Utils;
using SdJwt.Net.Verifier;

namespace SdJwt.Net.Oid4Vp.Verifier;

/// <summary>
/// Validates VP tokens in OID4VP authorization responses.
/// Performs comprehensive validation including signature verification and key binding.
/// </summary>
public class VpTokenValidator
{
    private readonly ILogger<VpTokenValidator>? _logger;
    private readonly SdVerifier _sdVerifier;

    /// <summary>
    /// Initializes a new instance of the VpTokenValidator class.
    /// </summary>
    /// <param name="keyProvider">Key provider for signature verification</param>
    /// <param name="logger">Optional logger</param>
    public VpTokenValidator(Func<JwtSecurityToken, Task<SecurityKey>> keyProvider, ILogger<VpTokenValidator>? logger = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(keyProvider);
#else
        if (keyProvider == null)
            throw new ArgumentNullException(nameof(keyProvider));
#endif

        _logger = logger;
        _sdVerifier = new SdVerifier(keyProvider);
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
            var kbValidationParams = CreateKeyBindingValidationParameters(expectedNonce, options);

            // Verify the presentation using the core verifier
            var verificationResult = await _sdVerifier.VerifyAsync(
                vpToken, validationParams, kbValidationParams);

            if (!verificationResult.KeyBindingVerified)
            {
                _logger?.LogWarning("Key binding verification failed");
                return SingleVpTokenResult.Failed("Key binding verification failed");
            }

            // Extract and validate claims
            var claims = ExtractClaims(verificationResult);
            
            // Validate nonce in key binding JWT
            if (!ValidateKeyBindingNonce(parsed.RawKeyBindingJwt, expectedNonce))
            {
                _logger?.LogWarning("Key binding JWT nonce validation failed");
                return SingleVpTokenResult.Failed("Key binding nonce validation failed");
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
        string expectedNonce, 
        VpTokenValidationOptions options)
    {
        return new TokenValidationParameters
        {
            ValidateAudience = options.ValidateKeyBindingAudience,
            ValidAudiences = options.ValidKeyBindingAudiences,
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

    private bool ValidateKeyBindingNonce(string? keyBindingJwt, string expectedNonce)
    {
        if (string.IsNullOrEmpty(keyBindingJwt))
        {
            return false;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(keyBindingJwt);
            
            var nonceClaim = token.Claims.FirstOrDefault(c => c.Type == "nonce");
            if (nonceClaim?.Value != expectedNonce)
            {
                _logger?.LogWarning("Nonce mismatch in key binding JWT. Expected: {Expected}, Got: {Actual}",
                    expectedNonce, nonceClaim?.Value);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to validate key binding nonce");
            return false;
        }
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
    public IEnumerable<string>? ValidIssuers { get; set; }

    /// <summary>
    /// Gets or sets whether to validate the audience.
    /// Default: false (not typically required for presentations).
    /// </summary>
    public bool ValidateAudience { get; set; } = false;

    /// <summary>
    /// Gets or sets the valid audiences.
    /// </summary>
    public IEnumerable<string>? ValidAudiences { get; set; }

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
    /// Default: false.
    /// </summary>
    public bool ValidateKeyBindingAudience { get; set; } = false;

    /// <summary>
    /// Gets or sets the valid key binding audiences.
    /// </summary>
    public IEnumerable<string>? ValidKeyBindingAudiences { get; set; }

    /// <summary>
    /// Gets or sets whether to validate key binding lifetime.
    /// Default: true.
    /// </summary>
    public bool ValidateKeyBindingLifetime { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to stop validation on first failure.
    /// Default: false (validate all tokens).
    /// </summary>
    public bool StopOnFirstFailure { get; set; } = false;

    /// <summary>
    /// Gets or sets custom validation function.
    /// </summary>
    public Func<VerificationResult, CancellationToken, Task<CustomValidationResult>>? CustomValidation { get; set; }
}

/// <summary>
/// Result of custom validation.
/// </summary>
public class CustomValidationResult
{
    /// <summary>
    /// Gets or sets whether validation passed.
    /// </summary>
    public bool IsValid { get; set; }

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
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the error message if validation failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets the validated tokens.
    /// </summary>
    public VpTokenResult[] ValidatedTokens { get; set; } = Array.Empty<VpTokenResult>();

    /// <summary>
    /// Gets or sets the presentation submission.
    /// </summary>
    public PresentationSubmission? PresentationSubmission { get; set; }

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
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the VP token string.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this token is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the error message if validation failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets the verification result.
    /// </summary>
    public VerificationResult? VerificationResult { get; set; }

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
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the error message if validation failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets the verification result.
    /// </summary>
    public VerificationResult? VerificationResult { get; set; }

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