using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Models;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace SdJwt.Net.Verifier;

/// <summary>
/// A specialized verifier for SD-JWT Verifiable Credentials (VCs). It orchestrates
/// the validation of the core SD-JWT, the VC-specific structure, and credential status.
/// </summary>
public class SdJwtVcVerifier
{
    private readonly SdVerifier _sdVerifier;
    private readonly Func<JwtSecurityToken, Task<SecurityKey>> _issuerKeyProvider;
    private readonly StatusListOptions _statusListOptions;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _statusListCache;
    private readonly ILogger<SdJwtVcVerifier> _logger;
    private readonly string _trustedIssuer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SdJwtVcVerifier"/> class.
    /// </summary>
    /// <param name="trustedIssuer">The issuer name that this verifier trusts. This will be used to validate the issuer claim within the VC and the issuer of the Status List.</param>
    /// <param name="issuerKeyProvider">A function that resolves the Issuer's public key for a given JWT.</param>
    /// <param name="statusListOptions">Optional settings for Status List handling.</param>
    /// <param name="logger">An optional logger for diagnostics.</param>
    public SdJwtVcVerifier(
        string trustedIssuer,
        Func<JwtSecurityToken, Task<SecurityKey>> issuerKeyProvider,
        StatusListOptions? statusListOptions = null,
        ILogger<SdJwtVcVerifier>? logger = null)
    {

        if (string.IsNullOrWhiteSpace(trustedIssuer)) { throw new ArgumentException("Value cannot be null or whitespace.", nameof(trustedIssuer)); }

        _trustedIssuer = trustedIssuer;
        _issuerKeyProvider = issuerKeyProvider ?? throw new ArgumentNullException(nameof(issuerKeyProvider));
        _logger = logger ?? NullLogger<SdJwtVcVerifier>.Instance;

        var verifierLogger = logger as ILogger<SdVerifier>;
        _sdVerifier = new SdVerifier(issuerKeyProvider, verifierLogger);

        _statusListOptions = statusListOptions ?? new StatusListOptions();
        _httpClient = _statusListOptions.HttpClient ?? new HttpClient();
        _statusListCache = new MemoryCache(new MemoryCacheOptions());
    }

    /// <summary>
    /// Verifies a full SD-JWT-VC presentation.
    /// </summary>
    /// <param name="presentation">The presentation string from the Holder.</param>
    /// <param name="kbJwtValidationParameters">Optional validation parameters for the Key Binding JWT.</param>
    public async Task<SdJwtVcVerificationResult> VerifyAsync(
        string presentation,
        TokenValidationParameters? kbJwtValidationParameters = null)
    {
        // Step 1: Create validation parameters specifically for the main SD-JWT-VC.
        // For this token type, the issuer is NOT at the top level, so ValidateIssuer must be false.
        var sdJwtVcValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
        };

        // Step 2: Call the base verifier to handle cryptographic validation of the SD-JWT and KB-JWT.
        var baseResult = await _sdVerifier.VerifyAsync(presentation, sdJwtVcValidationParams, kbJwtValidationParameters);
        _logger.LogDebug("Base SD-JWT verification successful. Performing additional VC-specific checks.");

        // Step 3: Validate the VC structure and deserialize the payload.
        var vcPayload = DeserializeAndValidateVcStructure(baseResult);

        // Step 4: Check if the issuer inside the VC payload matches the one this verifier trusts.
        if (vcPayload.Issuer != _trustedIssuer)
        {
            throw new SecurityTokenInvalidIssuerException($"The issuer of the credential ('{vcPayload.Issuer}') does not match the trusted issuer ('{_trustedIssuer}').");
        }

        // Step 5: If a status claim is present, perform the revocation check.
        if (vcPayload.Status != null)
        {
            _logger.LogInformation("VC contains a status claim. Performing status check for index {StatusIndex}.", vcPayload.Status.StatusListIndex);
            var isStatusValid = await CheckStatus(vcPayload.Status);

            if (!isStatusValid)
            {
                throw new SecurityTokenException($"Verification failed: Credential status at index {vcPayload.Status.StatusListIndex} is marked as invalid/revoked.");
            }

            _logger.LogInformation("Status check passed for index {StatusIndex}.", vcPayload.Status.StatusListIndex);
        }

        var vcType = baseResult.ClaimsPrincipal.FindFirst("vct")?.Value;
        _logger.LogInformation("Successfully verified and parsed SD-JWT-VC of type '{VcType}'.", vcType);

        return new SdJwtVcVerificationResult(
            baseResult.ClaimsPrincipal,
            baseResult.KeyBindingVerified,
            vcPayload
        );
    }

    private async Task<bool> CheckStatus(StatusClaim statusClaim)
    {
        var cacheKey = $"StatusList_{statusClaim.StatusListCredential}";

        if (!_statusListCache.TryGetValue(cacheKey, out BitArray? statusBits))
        {
            _logger.LogDebug("Status list not found in cache. Fetching from {StatusListUrl}", statusClaim.StatusListCredential);

            var statusListJwt = await _httpClient.GetStringAsync(statusClaim.StatusListCredential);
            var tokenHandler = new JwtSecurityTokenHandler();
            var unverifiedToken = new JwtSecurityToken(statusListJwt);
            var statusListSigningKey = await _issuerKeyProvider(unverifiedToken);

            // Create a single, robust set of validation parameters.
            // Let the handler validate the issuer and signature.
            var validationParameters = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidIssuer = _trustedIssuer,
                ValidateAudience = false,

                // By convention, Status List JWTs may not have an 'exp' claim,
                // so we disable the full lifetime validation.
                ValidateLifetime = false,
                IssuerSigningKey = statusListSigningKey
            };

            // We still manually check that the token was not issued in the future,
            // as ValidateLifetime has been disabled.
            if (unverifiedToken.IssuedAt > DateTime.UtcNow.AddMinutes(5)) // Allow 5 mins clock skew
            {
                throw new SecurityTokenInvalidLifetimeException($"The status list was issued in the future ('{unverifiedToken.IssuedAt}').");
            }

            // Validate the token using the configured parameters.
            tokenHandler.ValidateToken(statusListJwt, validationParameters, out var validatedToken);
            var statusListPayload = ((JwtSecurityToken)validatedToken).Payload;

            var encodedList = statusListPayload.Sub ?? throw new SecurityTokenException("Status List JWT is missing 'sub' claim.");
            var listBytes = Base64UrlEncoder.DecodeBytes(encodedList);
            statusBits = new BitArray(listBytes);

            if (_statusListOptions.CacheDuration > TimeSpan.Zero)
            {
                _statusListCache.Set(cacheKey, statusBits, _statusListOptions.CacheDuration);
            }
        }
        else
        {
            _logger.LogDebug("Status list found in cache for {StatusListUrl}", statusClaim.StatusListCredential);
        }

        if (statusClaim.StatusListIndex >= statusBits!.Length)
        {
            throw new SecurityTokenException($"Status list index {statusClaim.StatusListIndex} out of bounds.");
        }

        // Return true if the bit is 0 (not revoked), false if it is 1 (revoked).
        return !statusBits[statusClaim.StatusListIndex];
    }

    private static VerifiableCredentialPayload DeserializeAndValidateVcStructure(VerificationResult baseResult)
    {
        var claims = baseResult.ClaimsPrincipal.Claims;
        if (!claims.Any(c => c.Type == "vct"))
        {
            throw new SecurityTokenException("Verification failed: Missing 'vct' claim.");
        }
        var vcClaim = claims.FirstOrDefault(c => c.Type == "vc")
            ?? throw new SecurityTokenException("Verification failed: Missing 'vc' claim.");

        try
        {
            var vcPayload = JsonSerializer.Deserialize<VerifiableCredentialPayload>(vcClaim.Value, SdJwtConstants.DefaultJsonSerializerOptions);
            return vcPayload ?? throw new JsonException("Deserialized 'vc' claim payload is null.");
        }
        catch (JsonException ex)
        {
            throw new SecurityTokenException("Failed to deserialize the 'vc' claim.", ex);
        }
    }
}