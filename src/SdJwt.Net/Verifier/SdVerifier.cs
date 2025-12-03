using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Internal;
using SdJwt.Net.Models;
using SdJwt.Net.Serialization;
using SdJwt.Net.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SdJwt.Net.Verifier;

/// <summary>
/// A record to hold the result of a successful verification.
/// </summary>
/// <param name="ClaimsPrincipal">The verified claims principal containing claims from both the SD-JWT and the disclosed arrays.</param>
/// <param name="KeyBindingVerified">Indicates if a Key Binding JWT was present and successfully verified.</param>
/// <param name="KeyBindingJwtPayload">The payload of the Key Binding JWT, if present and verified.</param>
public record VerificationResult(ClaimsPrincipal ClaimsPrincipal, bool KeyBindingVerified, JwtPayload? KeyBindingJwtPayload = null);

/// <summary>
/// Responsible for verifying the core cryptographic integrity of SD-JWT presentations.
/// This class handles the validation of signatures, matching disclosure digests, and verifying key binding.
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
    /// Verifies the signatures and structure of an SD-JWT presentation.
    /// </summary>
    /// <param name="presentation">The presentation string from the Holder.</param>
    /// <param name="validationParameters">Token validation parameters to apply to the main SD-JWT.</param>
    /// <param name="kbJwtValidationParameters">Optional validation parameters for the Key Binding JWT.</param>
    /// <param name="expectedKbJwtNonce">Optional expected nonce value to verify against the Key Binding JWT.</param>
    /// <returns>A <see cref="VerificationResult"/> containing the rehydrated claims if verification is successful.</returns>
    public async Task<VerificationResult> VerifyAsync(
        string presentation,
        TokenValidationParameters validationParameters,
        TokenValidationParameters? kbJwtValidationParameters = null,
        string? expectedKbJwtNonce = null)
    {
        if (string.IsNullOrWhiteSpace(presentation)) { throw new ArgumentException("Value cannot be null or whitespace.", nameof(presentation)); }
        if (validationParameters == null) { throw new ArgumentNullException(nameof(validationParameters)); }

        _logger.LogInformation("Starting base SD-JWT verification.");

        var (sdJwt, disclosures, kbJwt) = SdJwtParser.ParsePresentation(presentation);

        var tokenHandler = new JwtSecurityTokenHandler();
        _logger.LogDebug("Verifying SD-JWT signature...");
        var unverifiedToken = new JwtSecurityToken(sdJwt);
        var issuerSigningKey = await _issuerKeyProvider(unverifiedToken);

        var sdJwtValidationParams = validationParameters.Clone();
        sdJwtValidationParams.IssuerSigningKey = issuerSigningKey;

        var principal = tokenHandler.ValidateToken(sdJwt, sdJwtValidationParams, out var validatedToken);
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
        JwtPayload? kbPayload = null;
        if (kbJwt != null && kbJwtValidationParameters != null)
        {
            _logger.LogInformation("Key Binding JWT found. Starting verification...");
            var expectedSdHash = SdJwtUtils.CreateDigest(hashAlgorithm, sdJwt);

            tokenHandler.ValidateToken(kbJwt, kbJwtValidationParameters, out var validatedKbToken);
            kbPayload = ((JwtSecurityToken)validatedKbToken).Payload;

            // We must find it by its claim type constant.
            var sdHashClaimValue = kbPayload.Claims.FirstOrDefault(c => c.Type == SdJwtConstants.SdHashClaim)?.Value
                ?? throw new SecurityTokenException($"Key Binding JWT is missing the required '{SdJwtConstants.SdHashClaim}' claim.");

            if (!CryptographicOperations.FixedTimeEquals(
                Base64UrlEncoder.DecodeBytes(sdHashClaimValue),
                Base64UrlEncoder.DecodeBytes(expectedSdHash)))
            {
                _logger.LogWarning("sd_hash in Key Binding JWT does not match the SD-JWT. Verification failed.");
                throw new SecurityTokenException("sd_hash in Key Binding JWT does not match the SD-JWT.");
            }

            if (!string.IsNullOrEmpty(expectedKbJwtNonce))
            {
                var nonce = kbPayload.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Nonce)?.Value;
                if (!string.Equals(nonce, expectedKbJwtNonce, StringComparison.Ordinal))
                {
                    _logger.LogWarning("Nonce in Key Binding JWT ('{Nonce}') does not match expected value ('{ExpectedNonce}').", nonce, expectedKbJwtNonce);
                    throw new SecurityTokenException($"Nonce in Key Binding JWT does not match expected value.");
                }
            }

            _logger.LogInformation("Key Binding JWT verified successfully.");
            kbVerified = true;
        }

        payloadNode.Remove(SdJwtConstants.SdAlgorithmClaim);
        payloadNode.Remove(SdJwtConstants.SdClaim);

        var rehydratedDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadNode.ToJsonString(), SdJwtConstants.DefaultJsonSerializerOptions)!;

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

        var combinedIdentity = new ClaimsIdentity(rehydratedClaims);

        var originalNonSdClaims = principal.Claims.Where(c =>
            !rehydratedDictionary.ContainsKey(c.Type) &&
            c.Type != SdJwtConstants.SdAlgorithmClaim &&
            c.Type != SdJwtConstants.SdClaim
        );
        combinedIdentity.AddClaims(originalNonSdClaims);

        _logger.LogInformation("Base SD-JWT verification successful.");
        return new VerificationResult(new ClaimsPrincipal(combinedIdentity), kbVerified, kbPayload);
    }

    /// <summary>
    /// Verifies an SD-JWT or SD-JWT+KB in JWS JSON Serialization format.
    /// </summary>
    /// <param name="jsonSerialization">The SD-JWT in JWS JSON Serialization format</param>
    /// <param name="validationParameters">Parameters for validating the main SD-JWT</param>
    /// <param name="kbJwtValidationParameters">Optional parameters for validating the Key Binding JWT</param>
    /// <param name="expectedKbJwtNonce">Optional expected nonce value to verify against the Key Binding JWT.</param>
    /// <returns>A verification result containing the claims and key binding status</returns>
    public async Task<VerificationResult> VerifyJsonSerializationAsync(
        string jsonSerialization,
        TokenValidationParameters validationParameters,
        TokenValidationParameters? kbJwtValidationParameters = null,
        string? expectedKbJwtNonce = null)
    {
        if (string.IsNullOrEmpty(jsonSerialization))
            throw new ArgumentException("JSON serialization cannot be null or empty", nameof(jsonSerialization));

        if (!SdJwtJsonSerializer.IsValidJsonSerialization(jsonSerialization))
            throw new ArgumentException("Invalid JWS JSON Serialization format", nameof(jsonSerialization));

        _logger.LogInformation("Starting verification of SD-JWT in JWS JSON Serialization format");

        try
        {
            // Try to parse as Flattened JSON Serialization first
            var flattened = JsonSerializer.Deserialize<SdJwtJsonSerialization>(jsonSerialization, SdJwtConstants.DefaultJsonSerializerOptions);
            if (flattened != null && !string.IsNullOrEmpty(flattened.Payload))
            {
                var compactSdJwt = SdJwtJsonSerializer.FromFlattenedJsonSerialization(flattened);
                return await VerifyAsync(compactSdJwt, validationParameters, kbJwtValidationParameters, expectedKbJwtNonce);
            }

            // Try to parse as General JSON Serialization
            var general = JsonSerializer.Deserialize<SdJwtGeneralJsonSerialization>(jsonSerialization, SdJwtConstants.DefaultJsonSerializerOptions);
            if (general != null && !string.IsNullOrEmpty(general.Payload))
            {
                var compactSdJwt = SdJwtJsonSerializer.FromGeneralJsonSerialization(general);
                return await VerifyAsync(compactSdJwt, validationParameters, kbJwtValidationParameters, expectedKbJwtNonce);
            }

            throw new ArgumentException("Unable to parse JWS JSON Serialization", nameof(jsonSerialization));
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            _logger.LogError(ex, "Failed to verify SD-JWT in JWS JSON Serialization format");
            throw new SecurityTokenException("Failed to verify SD-JWT in JWS JSON Serialization format", ex);
        }
    }

    private static HashSet<string> RehydrateNode(JsonNode node, IReadOnlyDictionary<string, Disclosure> availableDisclosures, string hashAlgorithm)
    {
        var usedDigests = new HashSet<string>();

        if (node is JsonObject obj)
        {
            if (obj.ContainsKey(SdJwtConstants.SdClaim))
            {
                var sdNode = obj[SdJwtConstants.SdClaim];
                
                // Handle both array and string cases (single-element arrays might be deserialized as strings)
                if (sdNode is JsonArray sdArray)
                {
                    foreach (var digestNode in sdArray.ToList())
                    {
                        var digest = digestNode!.GetValue<string>();
                        if (availableDisclosures.TryGetValue(digest, out var disclosure))
                        {
                            obj[disclosure.ClaimName] = JsonNode.Parse(JsonSerializer.Serialize(disclosure.ClaimValue, SdJwtConstants.DefaultJsonSerializerOptions));
                            usedDigests.Add(digest);
                        }
                    }
                }
                else if (sdNode is JsonValue sdValue && sdValue.TryGetValue<string>(out var singleDigest))
                {
                    // Handle case where single-element array was deserialized as a string
                    if (availableDisclosures.TryGetValue(singleDigest, out var disclosure))
                    {
                        obj[disclosure.ClaimName] = JsonNode.Parse(JsonSerializer.Serialize(disclosure.ClaimValue, SdJwtConstants.DefaultJsonSerializerOptions));
                        usedDigests.Add(singleDigest);
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