using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Internal;
using SdJwt.Net.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
    public string SdJwt { get; }

    /// <summary>
    /// A read-only list of all disclosures that were part of the original issuance.
    /// </summary>
    public IReadOnlyList<Disclosure> AllDisclosures { get; }

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

        var parts = issuance.Split(SdJwtConstants.DisclosureSeparator);
        if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
        {
            throw new ArgumentException("Invalid issuance format: missing or empty SD-JWT part.", nameof(issuance));
        }

        SdJwt = parts[0];
        // Explicitly filter out any empty or whitespace parts before parsing them as disclosures.
        AllDisclosures = [.. parts.Skip(1)
                              .Where(p => !string.IsNullOrWhiteSpace(p))
                              .Select(Disclosure.Parse)];

        var unverifiedToken = new JwtSecurityToken(SdJwt);
        _hashAlgorithm = unverifiedToken.Payload.Claims
            .FirstOrDefault(c => c.Type == SdJwtConstants.SdAlgorithmClaim)?.Value
            ?? SdJwtConstants.DefaultHashAlgorithm;

        _logger.LogDebug("Parsed SD-JWT with hash algorithm '{HashAlgorithm}' and {DisclosureCount} potential disclosures.", _hashAlgorithm, AllDisclosures.Count);
    }

    /// <summary>
    /// Creates a presentation by selecting which claims to disclose.
    /// </summary>
    /// <param name="disclosureSelector">A function that takes a Disclosure and returns true if it should be included in the presentation.</param>
    /// <param name="kbJwtPayload">Optional payload for creating a Key Binding JWT. Should include 'aud' and 'nonce'.</param>
    /// <param name="kbJwtSigningKey">Optional signing key for the Key Binding JWT.</param>
    /// <param name="kbJwtSigningAlgorithm">The JWS algorithm to use for signing the Key Binding JWT (e.g., "ES256"). Required if kbJwtSigningKey is provided.</param>
    /// <returns>The final presentation string to be sent to the Verifier.</returns>
    public string CreatePresentation(
        Func<Disclosure, bool> disclosureSelector,
        JwtPayload? kbJwtPayload = null,
        SecurityKey? kbJwtSigningKey = null,
        string? kbJwtSigningAlgorithm = null) 
    {
        if (disclosureSelector is null)
        {
           throw new ArgumentNullException(nameof(disclosureSelector), "Disclosure selector cannot be null.");
        }

        if (kbJwtSigningKey != null && string.IsNullOrWhiteSpace(kbJwtSigningAlgorithm))
        {
            throw new ArgumentException("The Key Binding JWT signing algorithm must be provided when a signing key is present.", nameof(kbJwtSigningAlgorithm));
        }

        var selectedDisclosures = AllDisclosures
            .Where(disclosureSelector)
            .Select(d => d.EncodedValue)
            .ToList();

        _logger.LogInformation("Creating presentation with {SelectedCount} disclosed claims.", selectedDisclosures.Count);

        var presentationParts = new List<string> { SdJwt };
        presentationParts.AddRange(selectedDisclosures);

        if (kbJwtPayload != null && kbJwtSigningKey != null && kbJwtSigningAlgorithm != null)
        {
            _logger.LogInformation("Creating Key Binding JWT using algorithm {Algorithm}.", kbJwtSigningAlgorithm);

            var sdHash = SdJwtUtils.CreateDigest(_hashAlgorithm, SdJwt);
            kbJwtPayload[SdJwtConstants.SdHashClaim] = sdHash;



            var tokenHandler = new JwtSecurityTokenHandler();

            // Use SecurityTokenDescriptor for robust token creation.
            var kbTokenDescriptor = new SecurityTokenDescriptor
            {
                // The payload of the Key Binding JWT.
                Subject = new ClaimsIdentity(kbJwtPayload.Claims),

                // The signing credentials, which the handler uses to set the 'alg' header.
                SigningCredentials = new SigningCredentials(kbJwtSigningKey, kbJwtSigningAlgorithm),

                // Add our custom header claims here. This is the crucial step.
                AdditionalHeaderClaims = new Dictionary<string, object>
                {
                    { JwtHeaderParameterNames.Typ, SdJwtConstants.KbJwtHeaderType }
                }
            };

            // Create and write the token. The handler correctly merges the 'alg' and 'typ' headers.
            var kbToken = tokenHandler.CreateJwtSecurityToken(kbTokenDescriptor);
            var kbJwt = tokenHandler.WriteToken(kbToken);
            presentationParts.Add(kbJwt);
        }

        return string.Join(SdJwtConstants.DisclosureSeparator, presentationParts);
    }
}