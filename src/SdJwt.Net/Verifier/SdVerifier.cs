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
    /// <param name="verifierOptions">Optional verifier behavior and key binding policy settings.</param>
    /// <returns>A <see cref="VerificationResult"/> containing the rehydrated claims if verification is successful.</returns>
    public async Task<VerificationResult> VerifyAsync(
        string presentation,
        TokenValidationParameters validationParameters,
        TokenValidationParameters? kbJwtValidationParameters = null,
        string? expectedKbJwtNonce = null,
        SdVerifierOptions? verifierOptions = null)
    {
        if (string.IsNullOrWhiteSpace(presentation))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(presentation));
        }
        if (validationParameters == null)
        {
            throw new ArgumentNullException(nameof(validationParameters));
        }
        verifierOptions ??= new SdVerifierOptions();

        _logger.LogInformation("Starting base SD-JWT verification.");

        var parsedPresentation = SdJwtParser.ParsePresentation(presentation);
        var (sdJwt, disclosures, kbJwt) = parsedPresentation;

        if (verifierOptions.KeyBinding.RequireKeyBinding &&
            (string.IsNullOrEmpty(kbJwt) || kbJwtValidationParameters == null))
        {
            throw new SecurityTokenException("Key Binding is required by verifier policy but was not provided with validation parameters.");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        _logger.LogDebug("Verifying SD-JWT signature...");
        var unverifiedToken = new JwtSecurityToken(sdJwt);
        var issuerSigningKey = await _issuerKeyProvider(unverifiedToken);

        var sdJwtValidationParams = validationParameters.Clone();
        sdJwtValidationParams.IssuerSigningKey = issuerSigningKey;
        sdJwtValidationParams.RequireExpirationTime = false;

        SecurityToken validatedToken;
        ClaimsPrincipal principal;
        principal = tokenHandler.ValidateToken(sdJwt, sdJwtValidationParams, out validatedToken);
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
        if (verifierOptions.StrictMode)
        {
            ValidateEmbeddedDigests(payloadNode);
        }

        var usedDigests = SdVerifier.RehydrateNode(payloadNode, availableDisclosures, hashAlgorithm, verifierOptions.StrictMode);

        if (disclosedDigests.Except(usedDigests).Any())
        {
            throw new SecurityTokenException("Presentation contains disclosures that do not correspond to any digest in the SD-JWT.");
        }

        var kbVerified = false;
        JwtPayload? kbPayload = null;
        if (kbJwt != null && kbJwtValidationParameters != null)
        {
            _logger.LogInformation("Key Binding JWT found. Starting verification...");
            var expectedSdHash = SdJwtUtils.CreateDigest(hashAlgorithm, parsedPresentation.CompactSdJwt);

            // RFC 9901 / SD-JWT VC key binding: the KB-JWT MUST be validated
            // using the holder binding key from the cnf claim.
            var cnfClaim = jwtPayload.Claims.FirstOrDefault(c => c.Type == SdJwtConstants.CnfClaim);
            if (cnfClaim == null)
            {
                throw new SecurityTokenException("Key Binding JWT validation requires the 'cnf' claim in the SD-JWT.");
            }

            try
            {
                var cnfJson = JsonDocument.Parse(cnfClaim.Value);
                if (cnfJson.RootElement.TryGetProperty("jwk", out var jwkElement))
                {
                    var jwkJson = jwkElement.GetRawText();
                    var holderPublicKey = new JsonWebKey(jwkJson);
                    var kbParams = kbJwtValidationParameters.Clone();
                    kbParams.IssuerSigningKey = holderPublicKey;
                    tokenHandler.ValidateToken(kbJwt, kbParams, out var validatedKbToken);
                    kbPayload = ((JwtSecurityToken)validatedKbToken).Payload;
                }
                else
                {
                    throw new SecurityTokenException("cnf claim does not contain jwk property");
                }
            }
            catch (JsonException ex)
            {
                throw new SecurityTokenException("Failed to parse cnf claim", ex);
            }

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

            if (!string.IsNullOrEmpty(verifierOptions.KeyBinding.ExpectedNonce))
            {
                var nonce = kbPayload.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Nonce)?.Value;
                if (!string.Equals(nonce, verifierOptions.KeyBinding.ExpectedNonce, StringComparison.Ordinal))
                {
                    throw new SecurityTokenException("Nonce in Key Binding JWT does not match verifier policy.");
                }
            }

            if (!string.IsNullOrEmpty(verifierOptions.KeyBinding.ExpectedAudience))
            {
                var aud = kbPayload.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Aud)?.Value;
                if (!string.Equals(aud, verifierOptions.KeyBinding.ExpectedAudience, StringComparison.Ordinal))
                {
                    throw new SecurityTokenException("Audience in Key Binding JWT does not match verifier policy.");
                }
            }

            if (verifierOptions.KeyBinding.MaxKeyBindingJwtAge is TimeSpan maxAge)
            {
                var iatClaim = kbPayload.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat)?.Value;
                if (!long.TryParse(iatClaim, out var iatUnixSeconds))
                {
                    throw new SecurityTokenException("Key Binding JWT is missing a valid iat claim.");
                }
                var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iatUnixSeconds);
                if (DateTimeOffset.UtcNow - issuedAt > maxAge)
                {
                    throw new SecurityTokenException("Key Binding JWT is older than allowed by verifier policy.");
                }
            }

            _logger.LogInformation("Key Binding JWT verified successfully.");
            kbVerified = true;
        }
        else if (kbJwt != null && verifierOptions.KeyBinding.RequireKeyBinding)
        {
            throw new SecurityTokenException("Key Binding JWT was provided but no validation parameters were supplied.");
        }

        RemoveSdClaims(payloadNode);
        payloadNode.Remove(SdJwtConstants.SdAlgorithmClaim);

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
    /// <param name="verifierOptions">Optional verifier behavior and key binding policy settings.</param>
    /// <returns>A verification result containing the claims and key binding status</returns>
    public async Task<VerificationResult> VerifyJsonSerializationAsync(
        string jsonSerialization,
        TokenValidationParameters validationParameters,
        TokenValidationParameters? kbJwtValidationParameters = null,
        string? expectedKbJwtNonce = null,
        SdVerifierOptions? verifierOptions = null)
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
                return await VerifyAsync(compactSdJwt, validationParameters, kbJwtValidationParameters, expectedKbJwtNonce, verifierOptions);
            }

            // Try to parse as General JSON Serialization
            var general = JsonSerializer.Deserialize<SdJwtGeneralJsonSerialization>(jsonSerialization, SdJwtConstants.DefaultJsonSerializerOptions);
            if (general != null && !string.IsNullOrEmpty(general.Payload))
            {
                var compactSdJwt = SdJwtJsonSerializer.FromGeneralJsonSerialization(general);
                return await VerifyAsync(compactSdJwt, validationParameters, kbJwtValidationParameters, expectedKbJwtNonce, verifierOptions);
            }

            throw new ArgumentException("Unable to parse JWS JSON Serialization", nameof(jsonSerialization));
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            _logger.LogError(ex, "Failed to verify SD-JWT in JWS JSON Serialization format");
            throw new SecurityTokenException("Failed to verify SD-JWT in JWS JSON Serialization format", ex);
        }
    }

    private static HashSet<string> RehydrateNode(
        JsonNode node,
        IReadOnlyDictionary<string, Disclosure> availableDisclosures,
        string hashAlgorithm,
        bool strictMode)
    {
        var usedDigests = new HashSet<string>();

        if (node is JsonObject obj)
        {
            if (obj.ContainsKey(SdJwtConstants.SdClaim))
            {
                var sdNode = obj[SdJwtConstants.SdClaim];

                if (sdNode is JsonArray sdArray)
                {
                    foreach (var digestNode in sdArray.ToList())
                    {
                        var digest = digestNode!.GetValue<string>();
                        if (availableDisclosures.TryGetValue(digest, out var disclosure))
                        {
                            if (string.IsNullOrWhiteSpace(disclosure.ClaimName))
                            {
                                throw new SecurityTokenException("Disclosure for object _sd digest MUST be a three-element disclosure.");
                            }
                            if (disclosure.ClaimName == SdJwtConstants.SdClaim || disclosure.ClaimName == "...")
                            {
                                throw new SecurityTokenException("Disclosure claim name MUST NOT be _sd or ...");
                            }
                            if (strictMode && obj.ContainsKey(disclosure.ClaimName))
                            {
                                throw new SecurityTokenException("Disclosure claim name already exists at the _sd insertion level.");
                            }
                            obj[disclosure.ClaimName] = JsonNode.Parse(JsonSerializer.Serialize(disclosure.ClaimValue, SdJwtConstants.DefaultJsonSerializerOptions));
                            usedDigests.Add(digest);
                        }
                    }
                }
                else if (strictMode)
                {
                    throw new SecurityTokenException("The _sd claim MUST be an array of digest strings.");
                }
            }

            foreach (var key in obj.Select(kv => kv.Key).ToList())
            {
                if (obj[key] != null)
                    usedDigests.UnionWith(SdVerifier.RehydrateNode(obj[key]!, availableDisclosures, hashAlgorithm, strictMode));
            }
        }
        else if (node is JsonArray arr)
        {
            var unresolvedIndices = new List<int>();
            for (int i = 0; i < arr.Count; i++)
            {
                if (arr[i] is JsonObject arrayItemObj && arrayItemObj.ContainsKey("..."))
                {
                    if (strictMode && (arrayItemObj.Count != 1 || arrayItemObj["..."] is not JsonValue))
                    {
                        throw new SecurityTokenException("Array digest placeholder MUST be an object with only the '...' key.");
                    }
                    var digest = arrayItemObj["..."]!.GetValue<string>();
                    if (availableDisclosures.TryGetValue(digest, out var disclosure))
                    {
                        if (!string.IsNullOrWhiteSpace(disclosure.ClaimName))
                        {
                            throw new SecurityTokenException("Disclosure for array digest MUST be a two-element disclosure.");
                        }
                        arr[i] = JsonNode.Parse(JsonSerializer.Serialize(disclosure.ClaimValue, SdJwtConstants.DefaultJsonSerializerOptions));
                        usedDigests.Add(digest);
                    }
                    else
                    {
                        unresolvedIndices.Add(i);
                    }
                }

                if (arr[i] != null)
                    usedDigests.UnionWith(SdVerifier.RehydrateNode(arr[i]!, availableDisclosures, hashAlgorithm, strictMode));
            }

            if (unresolvedIndices.Count > 0)
            {
                unresolvedIndices.Sort();
                unresolvedIndices.Reverse();
                foreach (var index in unresolvedIndices)
                {
                    arr.RemoveAt(index);
                }
            }
        }
        return usedDigests;
    }

    private static void ValidateEmbeddedDigests(JsonNode node)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        ValidateEmbeddedDigestsRecursive(node, seen);
    }

    private static void ValidateEmbeddedDigestsRecursive(JsonNode? node, HashSet<string> seen)
    {
        if (node is null)
        {
            return;
        }

        if (node is JsonObject obj)
        {
            if (obj.TryGetPropertyValue(SdJwtConstants.SdClaim, out var sdNode))
            {
                if (sdNode is not JsonArray sdArray)
                {
                    throw new SecurityTokenException("The _sd claim MUST be an array of digest strings.");
                }
                foreach (var digestNode in sdArray)
                {
                    if (digestNode is not JsonValue digestValue || !digestValue.TryGetValue<string>(out var digest))
                    {
                        throw new SecurityTokenException("The _sd claim MUST contain digest strings only.");
                    }
                    if (!seen.Add(digest))
                    {
                        throw new SecurityTokenException("Duplicate digest encountered in SD-JWT payload.");
                    }
                }
            }

            foreach (var kv in obj)
            {
                ValidateEmbeddedDigestsRecursive(kv.Value, seen);
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                if (item is JsonObject digestObj && digestObj.ContainsKey("..."))
                {
                    if (digestObj.Count != 1 || digestObj["..."] is not JsonValue digestValue || !digestValue.TryGetValue<string>(out var digest))
                    {
                        throw new SecurityTokenException("Array digest placeholder MUST be an object with only the '...' key and string value.");
                    }
                    if (!seen.Add(digest))
                    {
                        throw new SecurityTokenException("Duplicate digest encountered in SD-JWT payload.");
                    }
                }
                ValidateEmbeddedDigestsRecursive(item, seen);
            }
        }
    }

    private static void RemoveSdClaims(JsonNode? node)
    {
        if (node is null)
        {
            return;
        }

        if (node is JsonObject obj)
        {
            obj.Remove(SdJwtConstants.SdClaim);
            foreach (var kv in obj.ToList())
            {
                RemoveSdClaims(kv.Value);
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                RemoveSdClaims(item);
            }
        }
    }
}
