using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Internal;
using SdJwt.Net.Models;
using SdJwt.Net.Utils;

namespace SdJwt.Net.Verifier;

/// <summary>
/// A record to hold the result of a successful verification.
/// </summary>
/// <param name="ClaimsPrincipal">The verified claims principal containing claims from both the SD-JWT and the disclosed arrays.</param>
/// <param name="KeyBindingVerified">Indicates if a Key Binding JWT was present and successfully verified.</param>
public record VerificationResult(ClaimsPrincipal ClaimsPrincipal, bool KeyBindingVerified);

/// <summary>
/// Represents the result of a successful SD-JWT-VC verification, extending the base result
/// with the strongly-typed Verifiable Credential payload.
/// </summary>
public record SdJwtVcVerificationResult(
    ClaimsPrincipal ClaimsPrincipal,
    bool KeyBindingVerified,
    VerifiableCredentialPayload VerifiableCredential) : VerificationResult(ClaimsPrincipal, KeyBindingVerified);

/// <summary>
/// Responsible for verifying SD-JWT presentations. This class handles the core logic of
/// validating signatures, matching disclosure digests, and verifying key binding.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SdVerifier"/> class.
/// </remarks>
/// <param name="issuerKeyProvider">A function that resolves the Issuer's public key based on the unverified SD-JWT header/payload.</param>
/// <param name="logger">An optional logger for diagnostics.</param>
public class SdVerifier(Func<JwtSecurityToken, Task<SecurityKey>> issuerKeyProvider, ILogger<SdVerifier>? logger = null)
{
    private readonly Func<JwtSecurityToken, Task<SecurityKey>> _issuerKeyProvider = issuerKeyProvider ?? throw new ArgumentNullException(nameof(issuerKeyProvider));
    private readonly ILogger _logger = logger ?? NullLogger<SdVerifier>.Instance;

    /// <summary>
    /// Verifies an SD-JWT presentation string.
    /// </summary>
    /// <param name="presentation">The presentation string from the Holder.</param>
    /// <param name="issuerValidationParameters">Token validation parameters for the main SD-JWT (e.g., valid issuer, audience).</param>
    /// <param name="kbJwtValidationParameters">Optional validation parameters for the Key Binding JWT.</param>
    /// <returns>A <see cref="VerificationResult"/> containing the combined claims if verification is successful.</returns>
    public async Task<VerificationResult> VerifyAsync(
        string presentation,
        TokenValidationParameters issuerValidationParameters,
        TokenValidationParameters? kbJwtValidationParameters = null)
    {

        if (string.IsNullOrWhiteSpace(presentation)) { throw new ArgumentException("Value cannot be null or whitespace.", nameof(presentation)); }
        if (issuerValidationParameters == null) { throw new ArgumentNullException(nameof(issuerValidationParameters)); }

        _logger.LogInformation("Starting verification of SD-JWT presentation.");

        var (sdJwt, disclosures, kbJwt) = SdJwtParser.ParsePresentation(presentation);

        var tokenHandler = new JwtSecurityTokenHandler();
        _logger.LogDebug("Verifying SD-JWT signature...");
        var unverifiedToken = new JwtSecurityToken(sdJwt);
        var issuerSigningKey = await _issuerKeyProvider(unverifiedToken);

        var validationParams = issuerValidationParameters.Clone();
        validationParams.IssuerSigningKey = issuerSigningKey;
        var principal = tokenHandler.ValidateToken(sdJwt, validationParams, out var validatedToken);
        var jwtPayload = ((JwtSecurityToken)validatedToken).Payload;
        _logger.LogDebug("SD-JWT signature is valid.");

        var hashAlgorithm = jwtPayload.Claims.FirstOrDefault(c => c.Type == SdJwtConstants.SdAlgorithmClaim)?.Value ?? SdJwtConstants.DefaultHashAlgorithm;
        var availableDisclosures = new Dictionary<string, Disclosure>();
        var disclosedDigests = new HashSet<string>();

        foreach (var disclosure in disclosures)
        {
            var digest = SdJwtUtils.CreateDigest(hashAlgorithm, disclosure.EncodedValue);
            if (!availableDisclosures.TryAdd(digest, disclosure))
            {
                throw new SecurityTokenException($"Duplicate disclosure detected for digest: {digest}");
            }
            disclosedDigests.Add(digest);
        }
        _logger.LogDebug("Verifying {DisclosureCount} disclosures against the SD-JWT.", disclosedDigests.Count);

        var payloadJson = jwtPayload.SerializeToJson();
        var payloadNode = JsonNode.Parse(payloadJson)!.AsObject();
        var usedDigests = SdVerifier.RehydrateNode(payloadNode, availableDisclosures, hashAlgorithm);

        if (disclosedDigests.Except(usedDigests).Any())
        {
            throw new SecurityTokenException("Presentation contains disclosures that do not correspond to any digest in the SD-JWT.");
        }

        var kbVerified = false;
        if (kbJwt != null && kbJwtValidationParameters != null)
        {
            _logger.LogInformation("Key Binding JWT found. Starting verification...");
            var expectedSdHash = SdJwtUtils.CreateDigest(hashAlgorithm, sdJwt);
            tokenHandler.ValidateToken(kbJwt, kbJwtValidationParameters, out var validatedKbToken);
            var kbPayload = ((JwtSecurityToken)validatedKbToken).Payload;

            var sdHashClaimValue = kbPayload.Claims.FirstOrDefault(c => c.Type == SdJwtConstants.SdHashClaim)?.Value ?? throw new SecurityTokenException("Key Binding JWT is missing the required 'sd_hash' claim.");

            if (!CryptographicOperations.FixedTimeEquals(
                Base64UrlEncoder.DecodeBytes(sdHashClaimValue),
                Base64UrlEncoder.DecodeBytes(expectedSdHash)))
            {
                _logger.LogWarning("sd_hash in Key Binding JWT does not match the SD-JWT. Verification failed.");
                throw new SecurityTokenException("sd_hash in Key Binding JWT does not match the SD-JWT.");
            }

            _logger.LogInformation("Key Binding JWT verified successfully.");
            kbVerified = true;
        }


        // Remove SD-JWT metadata before building the final claims principal
        payloadNode.Remove(SdJwtConstants.SdAlgorithmClaim);
        payloadNode.Remove(SdJwtConstants.SdClaim);

        // Deserialize the rehydrated JSON into a dictionary
        var rehydratedDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(
            payloadNode.ToJsonString(),
            SdJwtConstants.DefaultJsonSerializerOptions
        )!;

        // Convert the dictionary into an IEnumerable<Claim>.
        // For complex types (objects, arrays), the value is stored as a raw JSON string,
        // and the ValueType is set to JsonClaimValueTypes.Json.
        var rehydratedClaims = rehydratedDictionary.Select(kvp =>
        {
            string value;
            string valueType = ClaimValueTypes.String;

            if (kvp.Value is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Object || je.ValueKind == JsonValueKind.Array)
                {
                    value = je.GetRawText();
                    valueType = JsonClaimValueTypes.Json;
                }
                else
                {
                    value = je.ToString();
                }
            }
            else
            {
                value = kvp.Value?.ToString() ?? "";
            }

            return new Claim(kvp.Key, value, valueType);
        });

        // Create a new identity from the rehydrated claims.
        var combinedIdentity = new ClaimsIdentity(rehydratedClaims);

        // Add claims from the original JWT that were not part of the SD mechanism (e.g., iss, exp).
        var originalNonSdClaims = principal.Claims.Where(c =>
            !rehydratedDictionary.ContainsKey(c.Type) &&
            c.Type != SdJwtConstants.SdAlgorithmClaim &&
            c.Type != SdJwtConstants.SdClaim
        );
        combinedIdentity.AddClaims(originalNonSdClaims);

        _logger.LogInformation("SD-JWT presentation verification successful.");
        return new VerificationResult(new ClaimsPrincipal(combinedIdentity), kbVerified);
    }

    private static HashSet<string> RehydrateNode(JsonNode node, IReadOnlyDictionary<string, Disclosure> availableDisclosures, string hashAlgorithm)
    {
        var usedDigests = new HashSet<string>();

        if (node is JsonObject obj)
        {
            if (obj.ContainsKey(SdJwtConstants.SdClaim) && obj[SdJwtConstants.SdClaim] is JsonArray sdArray)
            {
                foreach (var digestNode in sdArray.ToList()) // Iterate on a copy
                {
                    var digest = digestNode!.GetValue<string>();
                    if (availableDisclosures.TryGetValue(digest, out var disclosure))
                    {
                        obj[disclosure.ClaimName] = JsonNode.Parse(JsonSerializer.Serialize(disclosure.ClaimValue, SdJwtConstants.DefaultJsonSerializerOptions));
                        usedDigests.Add(digest);
                    }
                }
            }

            foreach (var key in obj.Select(kv => kv.Key).ToList())
            {
                if (obj[key] != null) usedDigests.UnionWith(SdVerifier.RehydrateNode(obj[key]!, availableDisclosures, hashAlgorithm));
            }
        }
        else if (node is JsonArray arr)
        {
            for (int i = 0; i < arr.Count; i++)
            {
                if (arr[i] is JsonObject arrayItemObj && arrayItemObj.ContainsKey("..."))
                {
                    var digest = arrayItemObj["..."]!.GetValue<string>();
                    if (availableDisclosures.TryGetValue(digest, out var disclosure))
                    {
                        arr[i] = JsonNode.Parse(JsonSerializer.Serialize(disclosure.ClaimValue, SdJwtConstants.DefaultJsonSerializerOptions));
                        usedDigests.Add(digest);
                    }
                }

                if (arr[i] != null) usedDigests.UnionWith(SdVerifier.RehydrateNode(arr[i]!, availableDisclosures, hashAlgorithm));
            }
        }
        return usedDigests;
    }
}