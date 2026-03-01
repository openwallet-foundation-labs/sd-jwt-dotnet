using SdJwt.Net.Oid4Vp.DcApi.Models;
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Verifier;

namespace SdJwt.Net.Oid4Vp.DcApi;

/// <summary>
/// Validates responses received via the Digital Credentials API.
/// </summary>
public class DcApiResponseValidator
{
    private readonly VpTokenValidator? _vpTokenValidator;
    private readonly DcApiOriginValidator _originValidator;

    /// <summary>
    /// Initializes a new instance with the specified VP token validator.
    /// </summary>
    /// <param name="vpTokenValidator">The VP token validator to use.</param>
    public DcApiResponseValidator(VpTokenValidator? vpTokenValidator)
    {
        _vpTokenValidator = vpTokenValidator;
        _originValidator = new DcApiOriginValidator();
    }

    /// <summary>
    /// Validates a DC API response.
    /// </summary>
    /// <param name="response">The raw response from navigator.credentials.get().</param>
    /// <param name="options">Validation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with verified credentials or error details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when response or options are null.</exception>
    public async Task<DcApiValidationResult> ValidateAsync(
        DcApiResponse response,
        DcApiValidationOptions options,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(options);
#else
        if (response == null)
            throw new ArgumentNullException(nameof(response));
        if (options == null)
            throw new ArgumentNullException(nameof(options));
#endif

        // Validate protocol
        if (response.Protocol != DcApiConstants.Protocol)
        {
            return DcApiValidationResult.Failure(
                $"Invalid protocol: expected '{DcApiConstants.Protocol}', got '{response.Protocol}'",
                DcApiConstants.ErrorCodes.InvalidProtocol);
        }

        // Validate origin
        if (options.ValidateOrigin)
        {
            if (!_originValidator.ValidateOrigin(response.Origin, options.ExpectedOrigin))
            {
                return DcApiValidationResult.Failure(
                    "Origin mismatch: response origin does not match expected client_id",
                    DcApiConstants.ErrorCodes.OriginMismatch);
            }
        }

        // Validate nonce
        if (!string.IsNullOrEmpty(options.ExpectedNonce))
        {
            if (response.Nonce != options.ExpectedNonce)
            {
                return DcApiValidationResult.Failure(
                    "Nonce mismatch: response nonce does not match expected value",
                    DcApiConstants.ErrorCodes.NonceMismatch);
            }
        }

        // Validate presentation age
        if (response.IssuedAt.HasValue)
        {
            var age = DateTimeOffset.UtcNow - response.IssuedAt.Value;
            if (age > options.MaxAge + options.ClockSkew)
            {
                return DcApiValidationResult.Failure(
                    $"Presentation expired: age {age.TotalMinutes:F1} minutes exceeds maximum allowed {options.MaxAge.TotalMinutes:F1} minutes",
                    DcApiConstants.ErrorCodes.PresentationExpired);
            }
        }

        // Validate VP token if validator is provided
        if (_vpTokenValidator is not null && !string.IsNullOrEmpty(response.VpToken))
        {
            // Create AuthorizationResponse for VpTokenValidator
            var authResponse = new AuthorizationResponse
            {
                VpToken = response.VpToken,
                PresentationSubmission = response.PresentationSubmission
            };

            var vpOptions = new VpTokenValidationOptions
            {
                ExpectedClientId = options.ExpectedOrigin,
                ValidateKeyBindingAudience = true,
                ValidateKeyBindingFreshness = true,
                MaxKeyBindingAge = options.MaxAge
            };

            var vpResult = await _vpTokenValidator.ValidateAsync(
                authResponse,
                options.ExpectedNonce,
                vpOptions,
                cancellationToken);

            if (!vpResult.IsValid)
            {
                return DcApiValidationResult.Failure(
                    vpResult.Error ?? "VP token validation failed",
                    "vp_token_invalid");
            }

            // Extract credentials from VP token
            var credentials = ExtractCredentials(vpResult);
            return DcApiValidationResult.Success(credentials, response.PresentationSubmission);
        }

        // Return success with empty credentials if no validator
        return DcApiValidationResult.Success(
            Array.Empty<VerifiedCredential>(),
            response.PresentationSubmission);
    }

    private static List<VerifiedCredential> ExtractCredentials(VpTokenValidationResult vpResult)
    {
        var credentials = new List<VerifiedCredential>();

        foreach (var token in vpResult.ValidatedTokens)
        {
            if (token.IsValid && token.Claims is not null)
            {
                credentials.Add(new VerifiedCredential
                {
                    Type = token.Claims.TryGetValue("vct", out var vct) ? vct?.ToString() ?? "unknown" : "unknown",
                    Claims = token.Claims,
                    Issuer = token.Claims.TryGetValue("iss", out var iss) ? iss?.ToString() : null,
                    Subject = token.Claims.TryGetValue("sub", out var sub) ? sub?.ToString() : null
                });
            }
        }

        return credentials;
    }
}
