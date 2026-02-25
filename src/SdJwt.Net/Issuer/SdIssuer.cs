using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Internal;
using SdJwt.Net.Models;
using SdJwt.Net.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SdJwt.Net.Issuer;

/// <summary>
/// A record to hold the output of the Issuer's issuance process.
/// </summary>
/// <param name="Issuance">The final SD-JWT issuance string, combining the SD-JWT and all disclosures.</param>
/// <param name="SdJwt">The signed SD-JWT part.</param>
/// <param name="Disclosures">A list of all created disclosures.</param>
public record IssuerOutput(string Issuance, string SdJwt, IReadOnlyList<Disclosure> Disclosures);

/// <summary>
/// Responsible for creating SD-JWTs based on a set of claims and disclosure options.
/// </summary>
public class SdIssuer {
        private readonly SecurityKey _signingKey;
        private readonly string _signingAlgorithm;
        private readonly string _hashAlgorithm;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SdIssuer"/> class.
        /// </summary>
        /// <param name="signingKey">The security key to sign the SD-JWT.</param>
        /// <param name="signingAlgorithm">The JWT signing algorithm (e.g., "ES256", "EdDSA").</param>
        /// <param name="hashAlgorithm">The hashing algorithm for disclosures (e.g., "sha-256").</param>
        /// <param name="logger">An optional logger for diagnostics.</param>
        public SdIssuer(
            SecurityKey signingKey,
            string signingAlgorithm,
            string hashAlgorithm = SdJwtConstants.DefaultHashAlgorithm,
            ILogger<SdIssuer>? logger = null) {
                if (string.IsNullOrWhiteSpace(signingAlgorithm)) { throw new ArgumentException("Value cannot be null or whitespace.", nameof(signingAlgorithm)); }
                if (string.IsNullOrWhiteSpace(hashAlgorithm)) { throw new ArgumentException("Value cannot be null or whitespace.", nameof(hashAlgorithm)); }

                _signingKey = signingKey ?? throw new ArgumentNullException(nameof(signingKey));
                _signingAlgorithm = signingAlgorithm;
                _hashAlgorithm = hashAlgorithm;
                _logger = logger ?? NullLogger<SdIssuer>.Instance;
        }

        /// <summary>
        /// Creates an SD-JWT based on the provided claims and options.
        /// </summary>
        /// <param name="claims">The payload for the JWT.</param>
        /// <param name="options">Options defining disclosable claims, decoys, and security policies.</param>
        /// <param name="holderPublicKey">Optional: The holder's public key (as a Jwk) to include in the 'cnf' claim for key binding.</param>
        /// <param name="tokenType">Optional: The type header value (default: "sd+jwt").</param>
        /// <returns>An <see cref="IssuerOutput"/> containing the full issuance string, the SD-JWT, and all disclosures.</returns>
        public IssuerOutput Issue(JwtPayload claims, SdIssuanceOptions options, JsonWebKey? holderPublicKey = null, string? tokenType = null) {
                if (claims == null) { throw new ArgumentNullException(nameof(claims)); }
                if (options == null) { throw new ArgumentNullException(nameof(options)); }

                if (!options.AllowWeakAlgorithms && _hashAlgorithm.ToUpperInvariant() is "MD5" or "SHA-1") {
                        throw new NotSupportedException($"The hash algorithm '{_hashAlgorithm}' is weak and not allowed by default. Set SdIssuanceOptions.AllowWeakAlgorithms to true to override.");
                }

                _logger.LogInformation("Starting SD-JWT issuance. Hash algorithm: {HashAlgorithm}, Signing algorithm: {SigningAlgorithm}", _hashAlgorithm, _signingAlgorithm);

                var disclosures = new List<Disclosure>();
                var payloadJson = JsonSerializer.Serialize(claims, SdJwtConstants.DefaultJsonSerializerOptions);
                var payloadNode = JsonNode.Parse(payloadJson)!.AsObject();

                var optionsNode = options.DisclosureStructure != null
                    ? JsonNode.Parse(JsonSerializer.Serialize(options.DisclosureStructure, SdJwtConstants.DefaultJsonSerializerOptions))
                    : new JsonObject();

                var rootSdArray = new JsonArray();

                ProcessNode(payloadNode, optionsNode, disclosures, options.MakeAllClaimsDisclosable, rootSdArray);
                if (rootSdArray.Count > 0)
                        payloadNode[SdJwtConstants.SdClaim] = rootSdArray;
                else
                        payloadNode.Remove(SdJwtConstants.SdClaim);

                if (options.DecoyDigests > 0) {
                        var sdClaim = payloadNode[SdJwtConstants.SdClaim] as JsonArray ?? [];
                        for (int i = 0; i < options.DecoyDigests; i++) {
                                var decoyDisclosure = new Disclosure(SdJwtUtils.GenerateSalt(), "decoy", Guid.NewGuid());
                                sdClaim.Add(SdJwtUtils.CreateDigest(_hashAlgorithm, decoyDisclosure.EncodedValue));
                        }
                        payloadNode[SdJwtConstants.SdClaim] = sdClaim;
                }

                if (payloadNode[SdJwtConstants.SdClaim] is JsonArray sdArray) {
                        var shuffledDigests = sdArray.Select(d => d!.GetValue<string>()).OrderBy(_ => new Random().Next()).ToArray();
                        payloadNode[SdJwtConstants.SdClaim] = new JsonArray([.. shuffledDigests.Select(s => JsonValue.Create(s))]);
                }

                payloadNode[SdJwtConstants.SdAlgorithmClaim] = _hashAlgorithm;
                if (holderPublicKey != null) {
                        var cnfJson = JsonSerializer.Serialize(new { jwk = holderPublicKey }, SdJwtConstants.DefaultJsonSerializerOptions);
                        payloadNode[SdJwtConstants.CnfClaim] = JsonNode.Parse(cnfJson);
                }

                // Create JwtPayload directly using JSON string to preserve array structures
                var payloadJsonString = payloadNode.ToJsonString();

                var finalPayload = JwtPayload.Deserialize(payloadJsonString);

                var tokenHandler = new JwtSecurityTokenHandler();

                // 1. Create the SecurityTokenDescriptor.
                var tokenDescriptor = new SecurityTokenDescriptor {
                        // 2. Set the subject (payload) of the token from our processed claims.
                        Subject = new ClaimsIdentity(finalPayload.Claims),

                        // 3. Set the signing credentials. This will set the 'alg' header.
                        SigningCredentials = new SigningCredentials(_signingKey, _signingAlgorithm),

                        // 4. Set the custom 'typ' header.
                        TokenType = tokenType ?? SdJwtConstants.SdJwtTypeName
                };

                if (finalPayload.TryGetValue(JwtRegisteredClaimNames.Nbf, out var nbfObj)) {
                        if (nbfObj is long nbfLong)
                                tokenDescriptor.NotBefore = DateTime.UnixEpoch.AddSeconds(nbfLong);
                        else if (nbfObj is int nbfInt)
                                tokenDescriptor.NotBefore = DateTime.UnixEpoch.AddSeconds(nbfInt);
                }

                if (finalPayload.TryGetValue(JwtRegisteredClaimNames.Exp, out var expObj)) {
                        if (expObj is long expLong)
                                tokenDescriptor.Expires = DateTime.UnixEpoch.AddSeconds(expLong);
                        else if (expObj is int expInt)
                                tokenDescriptor.Expires = DateTime.UnixEpoch.AddSeconds(expInt);
                }

                // 5. Create and sign the token. The handler will correctly merge the headers.
                var sdJwtToken = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
                var sdJwt = tokenHandler.WriteToken(sdJwtToken);

                var issuanceParts = new List<string> { sdJwt };
                issuanceParts.AddRange(disclosures.Select(d => d.EncodedValue));

                _logger.LogDebug("Created {DisclosureCount} real disclosures and {DecoyCount} decoys.", disclosures.Count, options.DecoyDigests);
                _logger.LogInformation("Successfully created SD-JWT issuance string.");
                return new IssuerOutput(string.Join(SdJwtConstants.DisclosureSeparator, issuanceParts), sdJwt, disclosures);
        }

        /// <summary>
        /// Creates an SD-JWT and returns it in JWS Flattened JSON Serialization format.
        /// </summary>
        /// <param name="claims">The payload for the JWT.</param>
        /// <param name="options">Options defining disclosable claims, decoys, and security policies.</param>
        /// <param name="holderPublicKey">Optional: The holder's public key (as a Jwk) to include in the 'cnf' claim for key binding.</param>
        /// <returns>An SD-JWT in JWS Flattened JSON Serialization format</returns>
        public SdJwtJsonSerialization IssueAsJsonSerialization(JwtPayload claims, SdIssuanceOptions options, JsonWebKey? holderPublicKey = null) {
                var issuerOutput = Issue(claims, options, holderPublicKey);
                return SdJwtJsonSerializer.ToFlattenedJsonSerialization(issuerOutput.Issuance);
        }

        /// <summary>
        /// Creates an SD-JWT and returns it in JWS General JSON Serialization format.
        /// </summary>
        /// <param name="claims">The payload for the JWT.</param>
        /// <param name="options">Options defining disclosable claims, decoys, and security policies.</param>
        /// <param name="holderPublicKey">Optional: The holder's public key (as a Jwk) to include in the 'cnf' claim for key binding.</param>
        /// <param name="additionalSignatures">Optional additional signatures for multi-signature scenarios</param>
        /// <returns>An SD-JWT in JWS General JSON Serialization format</returns>
        public SdJwtGeneralJsonSerialization IssueAsGeneralJsonSerialization(
            JwtPayload claims,
            SdIssuanceOptions options,
            JsonWebKey? holderPublicKey = null,
            SdJwtSignature[]? additionalSignatures = null) {
                var issuerOutput = Issue(claims, options, holderPublicKey);
                return SdJwtJsonSerializer.ToGeneralJsonSerialization(issuerOutput.Issuance, additionalSignatures);
        }

        private void ProcessNode(JsonNode node, JsonNode? options, List<Disclosure> disclosures, bool forceDisclose = false, JsonArray? parentSdArray = null) {
                if (node is JsonObject obj) {
                        var optionsObj = options as JsonObject;
                        var keys = obj.Select(kvp => kvp.Key).ToList();

                        // For nested objects, always prepare a new _sd array
                        JsonArray? thisLevelSdArray = parentSdArray == null ? [] : null;

                        foreach (var key in keys) {
                                if (key == SdJwtConstants.SdClaim) continue;

                                var value = obj[key];
                                var shouldDisclose = forceDisclose || (optionsObj != null && optionsObj.ContainsKey(key) && optionsObj[key] is JsonValue v && v.GetValue<bool>());
                                var hasNestedDisclosures = optionsObj?[key] is JsonObject or JsonArray;

                                if (shouldDisclose) {
                                        obj.Remove(key);
                                        var disclosure = new Disclosure(SdJwtUtils.GenerateSalt(), key, value!);
                                        disclosures.Add(disclosure);
                                        var digest = SdJwtUtils.CreateDigest(_hashAlgorithm, disclosure.EncodedValue);

                                        if (parentSdArray != null) {
                                                parentSdArray.Add(digest);
                                        }
                                        else {
                                                thisLevelSdArray?.Add(digest);
                                        }
                                }
                                else if (value != null) {
                                        // Process nested structures
                                        if (value is JsonObject nestedObj && hasNestedDisclosures) {
                                                var nestedSdArray = new JsonArray();
                                                ProcessNode(value, optionsObj?[key], disclosures, false, nestedSdArray);
                                                if (nestedSdArray.Count > 0) {
                                                        nestedObj[SdJwtConstants.SdClaim] = nestedSdArray;
                                                }
                                        }
                                        else if (value is JsonArray) {
                                                ProcessNode(value, optionsObj?[key], disclosures, false, null);
                                        }
                                }
                        }

                        // After processing all keys, if this is a root level object and any digests were added, set _sd
                        if (parentSdArray == null && thisLevelSdArray != null && thisLevelSdArray.Count > 0) {
                                obj[SdJwtConstants.SdClaim] = thisLevelSdArray;
                        }
                }
                else if (node is JsonArray arr) {
                        var optionsArr = options as JsonArray;
                        for (int i = 0; i < arr.Count; i++) {
                                var item = arr[i];
                                var shouldDisclose = forceDisclose || (optionsArr != null && i < optionsArr.Count && optionsArr[i] is JsonValue v && v.GetValue<bool>());

                                if (shouldDisclose && item != null) {
                                        var disclosure = new Disclosure(SdJwtUtils.GenerateSalt(), null!, item);
                                        disclosures.Add(disclosure);
                                        var digest = SdJwtUtils.CreateDigest(_hashAlgorithm, disclosure.EncodedValue);

                                        arr[i] = new JsonObject { { "...", digest } };
                                }
                                else if (item != null) {
                                        ProcessNode(item, optionsArr?[i], disclosures, false, null);
                                }
                        }
                }
        }
}