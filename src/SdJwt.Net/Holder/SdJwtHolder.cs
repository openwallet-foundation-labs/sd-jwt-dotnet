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
public class SdJwtHolder {
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
        public SdJwtHolder(string issuance, ILogger<SdJwtHolder>? logger = null) {
                if (string.IsNullOrWhiteSpace(issuance)) {
                        throw new ArgumentException("Issuance cannot be null or whitespace.", nameof(issuance));
                }

                _logger = logger ?? NullLogger<SdJwtHolder>.Instance;
                _logger.LogInformation("Parsing SD-JWT issuance string.");

                var parts = issuance.Split([SdJwtConstants.DisclosureSeparator], StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0])) {
                        throw new ArgumentException("Invalid issuance format: missing or empty SD-JWT part.", nameof(issuance));
                }

                SdJwt = parts[0];
                AllDisclosures = [.. parts.Skip(1).Select(Disclosure.Parse)];

                try {
                        var unverifiedToken = new JwtSecurityToken(SdJwt);
                        _hashAlgorithm = unverifiedToken.Payload.Claims
                            .FirstOrDefault(c => c.Type == SdJwtConstants.SdAlgorithmClaim)?.Value
                            ?? SdJwtConstants.DefaultHashAlgorithm;
                }
                catch (SecurityTokenMalformedException ex) {
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
            string? kbJwtSigningAlgorithm = null) {
                if (disclosureSelector is null) {
                        throw new ArgumentNullException(nameof(disclosureSelector));
                }

                if (kbJwtSigningKey != null && string.IsNullOrWhiteSpace(kbJwtSigningAlgorithm)) {
                        throw new ArgumentException("The Key Binding JWT signing algorithm must be provided when a signing key is present.", nameof(kbJwtSigningAlgorithm));
                }

                var selectedDisclosures = AllDisclosures
                    .Where(disclosureSelector)
                    .Select(d => d.EncodedValue)
                    .ToList();

                _logger.LogInformation("Creating presentation with {SelectedCount} disclosed claims.", selectedDisclosures.Count);

                List<string> presentationParts = [SdJwt, .. selectedDisclosures];

                if (kbJwtPayload != null && kbJwtSigningKey != null && kbJwtSigningAlgorithm != null) {
                        _logger.LogInformation("Creating Key Binding JWT using algorithm {Algorithm}.", kbJwtSigningAlgorithm);

                        var sdHash = SdJwtUtils.CreateDigest(_hashAlgorithm, SdJwt);
                        kbJwtPayload[SdJwtConstants.SdHashClaim] = sdHash;

                        var tokenHandler = new JwtSecurityTokenHandler();
                        var kbHeader = new JwtHeader(new SigningCredentials(kbJwtSigningKey, kbJwtSigningAlgorithm)) {
                                [JwtHeaderParameterNames.Typ] = SdJwtConstants.KbJwtHeaderType
                        };
                        var kbToken = new JwtSecurityToken(kbHeader, kbJwtPayload);
                        presentationParts.Add(tokenHandler.WriteToken(kbToken));
                }

                return string.Join(SdJwtConstants.DisclosureSeparator, presentationParts);
        }
}