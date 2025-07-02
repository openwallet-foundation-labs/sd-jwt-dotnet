using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Internal;
using SdJwt.Net.Models;
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
public class SdIssuer
{
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
        ILogger<SdIssuer>? logger = null)
    {
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
    /// <returns>An <see cref="IssuerOutput"/> containing the full issuance string, the SD-JWT, and all disclosures.</returns>
    public IssuerOutput Issue(JwtPayload claims, SdIssuanceOptions options, JsonWebKey? holderPublicKey = null)
    {
        if (claims == null) { throw new ArgumentNullException(nameof(claims)); }
        if (options == null) { throw new ArgumentNullException(nameof(options)); }

        if (!options.AllowWeakAlgorithms && _hashAlgorithm.ToUpperInvariant() is "MD5" or "SHA-1")
        {
            throw new NotSupportedException($"The hash algorithm '{_hashAlgorithm}' is weak and not allowed by default. Set SdIssuanceOptions.AllowWeakAlgorithms to true to override.");
        }

        _logger.LogInformation("Starting SD-JWT issuance. Hash algorithm: {HashAlgorithm}, Signing algorithm: {SigningAlgorithm}", _hashAlgorithm, _signingAlgorithm);

        var disclosures = new List<Disclosure>();
        var payloadJson = JsonSerializer.Serialize(claims, SdJwtConstants.DefaultJsonSerializerOptions);
        var payloadNode = JsonNode.Parse(payloadJson)!.AsObject();

        var optionsNode = options.DisclosureStructure != null
            ? JsonNode.Parse(JsonSerializer.Serialize(options.DisclosureStructure, SdJwtConstants.DefaultJsonSerializerOptions))
            : new JsonObject();

        ProcessNode(payloadNode, optionsNode, disclosures, options.MakeAllClaimsDisclosable);

        if (options.DecoyDigests > 0)
        {
            var sdClaim = payloadNode[SdJwtConstants.SdClaim] as JsonArray ?? new JsonArray();
            for (int i = 0; i < options.DecoyDigests; i++)
            {
                var decoyDisclosure = new Disclosure(SdJwtUtils.GenerateSalt(), "decoy", Guid.NewGuid());
                sdClaim.Add(SdJwtUtils.CreateDigest(_hashAlgorithm, decoyDisclosure.EncodedValue));
            }
            payloadNode[SdJwtConstants.SdClaim] = sdClaim;
        }

        if (payloadNode[SdJwtConstants.SdClaim] is JsonArray sdArray)
        {
            // Replace the problematic line with the following code to fix the error:  
            var shuffledDigests = sdArray.Select(d => d!.GetValue<string>()).OrderBy(_ => new Random().Next()).ToArray();
            payloadNode[SdJwtConstants.SdClaim] = new JsonArray([.. shuffledDigests.Select(s => JsonValue.Create(s))]);
        }

        payloadNode[SdJwtConstants.SdAlgorithmClaim] = _hashAlgorithm;
        if (holderPublicKey != null)
        {
            var cnfJson = JsonSerializer.Serialize(new { jwk = holderPublicKey }, SdJwtConstants.DefaultJsonSerializerOptions);
            payloadNode[SdJwtConstants.CnfClaim] = JsonNode.Parse(cnfJson);
        }

        var finalPayload = JwtPayload.Deserialize(payloadNode.ToJsonString());
        var tokenHandler = new JwtSecurityTokenHandler();

        // 1. Create the SecurityTokenDescriptor.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
        
            // 2. Set the subject (payload) of the token from our processed claims.
            Subject = new ClaimsIdentity(finalPayload.Claims),

            // 3. Set the signing credentials. This will set the 'alg' header.
            SigningCredentials = new SigningCredentials(_signingKey, _signingAlgorithm),

            // 4. Add the custom 'typ' header.
            AdditionalHeaderClaims =  new Dictionary<string, object>
            {
                { "typ", SdJwtConstants.SdJwtTypeName }
            }
        };

        // 5. Create and sign the token. The handler will correctly merge the headers.
        var sdJwtToken = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        var sdJwt = tokenHandler.WriteToken(sdJwtToken);

        var issuanceParts = new List<string> { sdJwt };
        issuanceParts.AddRange(disclosures.Select(d => d.EncodedValue));

        _logger.LogDebug("Created {DisclosureCount} real disclosures and {DecoyCount} decoys.", disclosures.Count, options.DecoyDigests);
        _logger.LogInformation("Successfully created SD-JWT issuance string.");
        return new IssuerOutput(string.Join(SdJwtConstants.DisclosureSeparator, issuanceParts), sdJwt, disclosures);
    }

    private void ProcessNode(JsonNode node, JsonNode? options, List<Disclosure> disclosures, bool forceDisclose = false)
    {
        if (node is JsonObject obj)
        {
            var optionsObj = options as JsonObject;
            var digests = new List<string>();
            var keys = obj.Select(kvp => kvp.Key).ToList();
            foreach (var key in keys)
            {
                var value = obj[key];
                var shouldDisclose = forceDisclose || (optionsObj != null && optionsObj.ContainsKey(key) && optionsObj[key] is JsonValue v && v.GetValue<bool>());

                if (shouldDisclose)
                {
                    obj.Remove(key);
                    var disclosure = new Disclosure(SdJwtUtils.GenerateSalt(), key, JsonNode.Parse(value!.ToJsonString())!);
                    disclosures.Add(disclosure);
                    digests.Add(SdJwtUtils.CreateDigest(_hashAlgorithm, disclosure.EncodedValue));
                }
                else if (value != null)
                {
                    ProcessNode(value, optionsObj?[key], disclosures);
                }
            }
            if (digests.Count > 0)
            {
                var sdArray = obj.ContainsKey(SdJwtConstants.SdClaim) ? obj[SdJwtConstants.SdClaim]!.AsArray() : [];
                foreach (var digest in digests) sdArray.Add(digest);
                obj[SdJwtConstants.SdClaim] = sdArray;
            }
        }
        else if (node is JsonArray arr)
        {
            var optionsArr = options as JsonArray;
            for (int i = 0; i < arr.Count; i++)
            {
                var item = arr[i];
                var shouldDisclose = forceDisclose || (optionsArr != null && i < optionsArr.Count && optionsArr[i] is JsonValue v && v.GetValue<bool>());

                if (shouldDisclose)
                {
                    var disclosure = new Disclosure(SdJwtUtils.GenerateSalt(), "...", JsonNode.Parse(item!.ToJsonString())!);
                    disclosures.Add(disclosure);
                    arr[i] = new JsonObject { { "...", SdJwtUtils.CreateDigest(_hashAlgorithm, disclosure.EncodedValue) } };
                }
                else if (item != null)
                {
                    ProcessNode(item, optionsArr?[i], disclosures);
                }
            }
        }
    }
}