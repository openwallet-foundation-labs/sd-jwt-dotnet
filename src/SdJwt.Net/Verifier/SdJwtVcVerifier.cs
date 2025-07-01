using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Models;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace SdJwt.Net.Verifier;

/// <summary>
/// A specialized verifier for SD-JWT Verifiable Credentials (VCs).
/// It wraps the generic <see cref="SdVerifier"/> and adds VC-specific validation logic,
/// including optional revocation checking via Status Lists.
/// </summary>
public class SdJwtVcVerifier
{
    private readonly SdVerifier _sdVerifier;
    private readonly Func<JwtSecurityToken, Task<SecurityKey>> _issuerKeyProvider;
    private readonly StatusListOptions _statusListOptions;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _statusListCache;
    private readonly ILogger<SdJwtVcVerifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SdJwtVcVerifier"/> class.
    /// </summary>
    /// <param name="issuerKeyProvider">A function that resolves the Issuer's public key for a given JWT. This is used for both the main SD-JWT and the Status List Credential.</param>
    /// <param name="statusListOptions">Optional settings for Status List handling, including HttpClient and caching.</param>
    /// <param name="logger">An optional logger for diagnostics.</param>
    public SdJwtVcVerifier(
        Func<JwtSecurityToken, Task<SecurityKey>> issuerKeyProvider,
        StatusListOptions? statusListOptions = null,
        ILogger<SdJwtVcVerifier>? logger = null)
    {
        _issuerKeyProvider = issuerKeyProvider ?? throw new ArgumentNullException(nameof(issuerKeyProvider));
        _logger = logger ?? NullLogger<SdJwtVcVerifier>.Instance;

        // Pass a casted logger to the underlying verifier if possible.
        var verifierLogger = logger as ILogger<SdVerifier>;
        _sdVerifier = new SdVerifier(issuerKeyProvider, verifierLogger);

        _statusListOptions = statusListOptions ?? new StatusListOptions();
        _httpClient = _statusListOptions.HttpClient ?? new HttpClient();
        _statusListCache = new MemoryCache(new MemoryCacheOptions());
    }

    /// <summary>
    /// Verifies an SD-JWT-VC presentation, including signature, disclosures, key binding, and optionally, status list checks.
    /// </summary>
    /// <param name="presentation">The presentation string from the Holder.</param>
    /// <param name="issuerValidationParameters">Token validation parameters for the main SD-JWT.</param>
    /// <param name="kbJwtValidationParameters">Optional validation parameters for the Key Binding JWT.</param>
    /// <returns>A strongly-typed <see cref="SdJwtVcVerificationResult"/> if verification is successful.</returns>
    public async Task<SdJwtVcVerificationResult> VerifyAsync(
        string presentation,
        TokenValidationParameters issuerValidationParameters,
        TokenValidationParameters? kbJwtValidationParameters = null)
    {
        // 1. Perform base SD-JWT verification using the wrapped verifier.
        var baseResult = await _sdVerifier.VerifyAsync(presentation, issuerValidationParameters, kbJwtValidationParameters);
        _logger.LogDebug("Base SD-JWT verification successful. Performing additional VC-specific checks.");

        // 2. Validate VC structure and deserialize the payload.
        var vcPayload = SdJwtVcVerifier.DeserializeAndValidateVcStructure(baseResult);

        // 3. Perform Status List check if a status claim is present.
        if (vcPayload.Status != null)
        {
            _logger.LogInformation("VC contains a status claim. Performing status check for index {StatusIndex}.", vcPayload.Status.StatusListIndex);

            var isStatusValid = await CheckStatus(vcPayload.Status, issuerValidationParameters);

            if (!isStatusValid)
            {
                throw new SecurityTokenException($"Verification failed: Credential status at index {vcPayload.Status.StatusListIndex} is marked as invalid/revoked.");
            }
            _logger.LogInformation("Status check passed for index {StatusIndex}.", vcPayload.Status.StatusListIndex);
        }

        var vcType = baseResult.ClaimsPrincipal.FindFirst("vct");
        _logger.LogInformation("Successfully verified and parsed SD-JWT-VC of type '{VcType}'.", vcType);

        // 4. Return the final, strongly-typed result.
        return new SdJwtVcVerificationResult(
            baseResult.ClaimsPrincipal,
            baseResult.KeyBindingVerified,
            vcPayload
        );
    }

    /// <summary>
    /// Fetches, validates, and checks a Status List to determine if a credential's status is valid.
    /// </summary>
    private async Task<bool> CheckStatus(StatusClaim statusClaim, TokenValidationParameters validationParameters)
    {
        var cacheKey = $"StatusList_{statusClaim.StatusListCredential}";

        if (!_statusListCache.TryGetValue(cacheKey, out BitArray? statusBits))
        {
            _logger.LogDebug("Status list not found in cache. Fetching from {StatusListUrl}", statusClaim.StatusListCredential);

            // Fetch and verify the Status List Credential JWT.
            var statusListJwt = await _httpClient.GetStringAsync(statusClaim.StatusListCredential);
            var tokenHandler = new JwtSecurityTokenHandler();

            // The key provider must be able to resolve the key for the status list's issuer.
            var unverifiedToken = new JwtSecurityToken(statusListJwt);
            var statusListSigningKey = await _issuerKeyProvider(unverifiedToken);

            var tempValidationParams = validationParameters.Clone();
            tempValidationParams.IssuerSigningKey = statusListSigningKey;

            tokenHandler.ValidateToken(statusListJwt, tempValidationParams, out var validatedToken);
            var statusListPayload = ((JwtSecurityToken)validatedToken).Payload;

            var encodedList = statusListPayload.Sub ?? throw new SecurityTokenException("Status List JWT is missing the required 'sub' claim containing the bitstring.");
            var listBytes = Base64UrlEncoder.DecodeBytes(encodedList);
            statusBits = new BitArray(listBytes);

            if (_statusListOptions.CacheDuration > TimeSpan.Zero)
            {
                _statusListCache.Set(cacheKey, statusBits, _statusListOptions.CacheDuration);
                _logger.LogDebug("Status list for {StatusListUrl} cached for {CacheDuration}.", statusClaim.StatusListCredential, _statusListOptions.CacheDuration);
            }
        }
        else
        {
            _logger.LogDebug("Status list found in cache for {StatusListUrl}", statusClaim.StatusListCredential);
        }

        // Check the bit at the specified index.
        if (statusClaim.StatusListIndex >= statusBits!.Length)
        {
            throw new SecurityTokenException($"Status list index {statusClaim.StatusListIndex} is out of bounds for the given status list of length {statusBits.Length}.");
        }

        // The spec says a value of '1' means the assertion is not valid (i.e., revoked).
        bool isRevoked = statusBits[statusClaim.StatusListIndex];
        return !isRevoked;
    }

    /// <summary>
    /// Validates the presence of required VC claims ('vct', 'vc') and deserializes the 'vc' claim.
    /// </summary>
    private static VerifiableCredentialPayload DeserializeAndValidateVcStructure(VerificationResult baseResult)
    {
        var claims = baseResult.ClaimsPrincipal.Claims;

        if (claims.FirstOrDefault(c => c.Type == "vct") == null)
        {
            throw new SecurityTokenException("Verification failed: The SD-JWT is missing the required 'vct' (Verifiable Credential Type) claim.");
        }

        var vcClaim = claims.FirstOrDefault(c => c.Type == "vc")
            ?? throw new SecurityTokenException("Verification failed: The SD-JWT is missing the required 'vc' (Verifiable Credential) claim.");

        try
        {
            var vcPayload = JsonSerializer.Deserialize<VerifiableCredentialPayload>(vcClaim.Value, SdJwtConstants.DefaultJsonSerializerOptions);
            return vcPayload ?? throw new JsonException("Deserialized 'vc' claim payload is null.");
        }
        catch (JsonException ex)
        {
            throw new SecurityTokenException("Failed to deserialize the 'vc' claim into a valid Verifiable Credential payload.", ex);
        }
    }
}