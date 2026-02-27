using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Internal;
using SdJwt.Net.Models;
using SdJwt.Net.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SdJwt.Net.Holder;

/// <summary>
/// Represents the Holder of an SD-JWT. This class is responsible for parsing a
/// full SD-JWT issuance and creating a presentation by selecting which claims to disclose.
/// </summary>
public class SdJwtHolder
{
    private readonly ILogger _logger;
    private readonly string _hashAlgorithm;

    /// <summary>
    /// The SD-JWT part of the issuance.
    /// </summary>
    public string SdJwt
    {
        get;
    }

    /// <summary>
    /// A read-only list of all disclosures that were part of the original issuance.
    /// </summary>
    public IReadOnlyList<Disclosure> AllDisclosures
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SdJwtHolder"/> class by parsing a full SD-JWT issuance string.
    /// </summary>
    /// <param name="issuance">The full string from the Issuer (SD-JWT~disclosure1~...).</param>
    /// <param name="logger">An optional logger for diagnostics.</param>
    public SdJwtHolder(string issuance, ILogger<SdJwtHolder>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(issuance))
        {
            throw new ArgumentException("Issuance cannot be null or whitespace.", nameof(issuance));
        }

        _logger = logger ?? NullLogger<SdJwtHolder>.Instance;
        _logger.LogInformation("Parsing SD-JWT issuance string.");

        var parsed = SdJwtParser.ParsePresentation(issuance);
        if (parsed.IsKeyBindingPresentation)
        {
            throw new FormatException("Invalid issuance format: Holder MUST receive an SD-JWT without Key Binding JWT.");
        }

        SdJwt = parsed.RawSdJwt;
        AllDisclosures = parsed.Disclosures;

        try
        {
            var unverifiedToken = new JwtSecurityToken(SdJwt);
            _hashAlgorithm = unverifiedToken.Payload.Claims
                .FirstOrDefault(c => c.Type == SdJwtConstants.SdAlgorithmClaim)?.Value
                ?? SdJwtConstants.DefaultHashAlgorithm;
        }
        catch (SecurityTokenMalformedException ex)
        {
            throw new ArgumentException("Invalid SD-JWT format: not a valid JWT.", nameof(issuance), ex);
        }

        _logger.LogDebug("Parsed SD-JWT with hash algorithm '{HashAlgorithm}' and {DisclosureCount} potential disclosures.", _hashAlgorithm, AllDisclosures.Count);
    }

    /// <summary>
    /// Creates a presentation by selecting which claims to disclose.
    /// </summary>
    public string CreatePresentation(
        Func<Disclosure, bool> disclosureSelector,
        JwtPayload? kbJwtPayload = null,
        SecurityKey? kbJwtSigningKey = null,
        string? kbJwtSigningAlgorithm = null,
        SdJwtHolderOptions? holderOptions = null)
    {
        if (disclosureSelector is null)
        {
            throw new ArgumentNullException(nameof(disclosureSelector));
        }
        holderOptions ??= new SdJwtHolderOptions();

        if (kbJwtSigningKey != null && string.IsNullOrWhiteSpace(kbJwtSigningAlgorithm))
        {
            throw new ArgumentException("The Key Binding JWT signing algorithm must be provided when a signing key is present.", nameof(kbJwtSigningAlgorithm));
        }

        var selectedDisclosures = AllDisclosures
            .Where(disclosureSelector)
            .Select(d => d.EncodedValue)
            .ToList();

        if (holderOptions.StrictMode)
        {
            ValidateSelectedDisclosures(selectedDisclosures);
        }

        _logger.LogInformation("Creating presentation with {SelectedCount} disclosed claims.", selectedDisclosures.Count);

        List<string> presentationParts = [SdJwt, .. selectedDisclosures];

        if (kbJwtPayload != null && kbJwtSigningKey != null && kbJwtSigningAlgorithm != null)
        {
            _logger.LogInformation("Creating Key Binding JWT using algorithm {Algorithm}.", kbJwtSigningAlgorithm);

            var compactSdJwt = BuildCompactSdJwtForHash(selectedDisclosures);
            var sdHash = SdJwtUtils.CreateDigest(_hashAlgorithm, compactSdJwt);
            kbJwtPayload[SdJwtConstants.SdHashClaim] = sdHash;

            var tokenHandler = new JwtSecurityTokenHandler();
            var kbHeader = new JwtHeader(new SigningCredentials(kbJwtSigningKey, kbJwtSigningAlgorithm))
            {
                [JwtHeaderParameterNames.Typ] = SdJwtConstants.KbJwtHeaderType
            };
            var kbToken = new JwtSecurityToken(kbHeader, kbJwtPayload);
            presentationParts.Add(tokenHandler.WriteToken(kbToken));
        }
        else
        {
            // RFC 9901 requires the final empty component for SD-JWT presentations without KB-JWT.
            presentationParts.Add(string.Empty);
        }

        return string.Join(SdJwtConstants.DisclosureSeparator, presentationParts);
    }

    private string BuildCompactSdJwtForHash(IReadOnlyList<string> selectedDisclosures)
    {
        var compactParts = new List<string> { SdJwt };
        compactParts.AddRange(selectedDisclosures);
        return string.Join(SdJwtConstants.DisclosureSeparator, compactParts) + SdJwtConstants.DisclosureSeparator;
    }

    private void ValidateSelectedDisclosures(IReadOnlyList<string> selectedDisclosures)
    {
        var selectedDigestSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var encodedDisclosure in selectedDisclosures)
        {
            var digest = SdJwtUtils.CreateDigest(_hashAlgorithm, encodedDisclosure);
            if (!selectedDigestSet.Add(digest))
            {
                throw new InvalidOperationException("Holder selected duplicate disclosures for the same digest.");
            }
        }

        var unverifiedToken = new JwtSecurityToken(SdJwt);
        var payloadNode = JsonNode.Parse(unverifiedToken.Payload.SerializeToJson())!;
        var allowedDigests = new HashSet<string>(StringComparer.Ordinal);
        CollectEmbeddedDigests(payloadNode, allowedDigests);

        foreach (var disclosure in AllDisclosures.Where(d => selectedDisclosures.Contains(d.EncodedValue)))
        {
            CollectEmbeddedDigests(JsonNode.Parse(JsonSerializer.Serialize(disclosure.ClaimValue, SdJwtConstants.DefaultJsonSerializerOptions)), allowedDigests);
        }

        foreach (var selectedDisclosure in selectedDisclosures)
        {
            var digest = SdJwtUtils.CreateDigest(_hashAlgorithm, selectedDisclosure);
            if (!allowedDigests.Contains(digest))
            {
                throw new InvalidOperationException("Selected disclosure is not referenced by the SD-JWT or another selected disclosure.");
            }
        }
    }

    private static void CollectEmbeddedDigests(JsonNode? node, HashSet<string> digests)
    {
        if (node is null)
        {
            return;
        }

        if (node is JsonObject obj)
        {
            if (obj.TryGetPropertyValue(SdJwtConstants.SdClaim, out var sdNode) && sdNode is JsonArray sdArray)
            {
                foreach (var digestNode in sdArray)
                {
                    if (digestNode is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var digest))
                    {
                        digests.Add(digest);
                    }
                }
            }

            foreach (var kv in obj)
            {
                CollectEmbeddedDigests(kv.Value, digests);
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                if (item is JsonObject digestObj &&
                    digestObj.Count == 1 &&
                    digestObj.TryGetPropertyValue("...", out var digestNode) &&
                    digestNode is JsonValue jsonValue &&
                    jsonValue.TryGetValue<string>(out var digest))
                {
                    digests.Add(digest);
                }
                CollectEmbeddedDigests(item, digests);
            }
        }
    }
}
