using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Models;
using SdJwt.Net.Wallet.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SdJwt.Net.Wallet.Formats;

/// <summary>
/// Format plugin for SD-JWT VC credential format (vc+sd-jwt).
/// </summary>
public class SdJwtVcFormatPlugin : ICredentialFormatPlugin
{
    /// <summary>
    /// JWT header type for SD-JWT VC.
    /// </summary>
    private const string SdJwtVcHeaderType = "vc+sd-jwt";

    /// <summary>
    /// MIME type for SD-JWT VC format.
    /// </summary>
    public const string MimeType = "application/vc+sd-jwt";

    /// <inheritdoc/>
    public string FormatId => SdJwtVcHeaderType;

    /// <inheritdoc/>
    public string DisplayName => "SD-JWT Verifiable Credential";

    /// <inheritdoc/>
    public Task<ParsedCredential> ParseAsync(
        string credential,
        ParseOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (credential == null)
        {
            throw new ArgumentNullException(nameof(credential));
        }

        options ??= new ParseOptions();

        try
        {
            var holder = new SdJwtHolder(credential);

            // Parse the JWT to extract claims
            var jwt = new JwtSecurityToken(holder.SdJwt);

            // Extract issuer
            var issuer = jwt.Payload.Iss;

            // Extract subject
            var subject = jwt.Payload.Sub;

            // Extract type from vct claim
            var vctClaim = jwt.Payload.Claims.FirstOrDefault(c => c.Type == "vct");
            var credentialType = vctClaim?.Value ?? "unknown";

            // Extract validity period
            var issuedAt = jwt.IssuedAt != DateTime.MinValue
                ? new DateTimeOffset(jwt.IssuedAt, TimeSpan.Zero)
                : (DateTimeOffset?)null;

            var expiration = jwt.ValidTo != DateTime.MinValue
                ? new DateTimeOffset(jwt.ValidTo, TimeSpan.Zero)
                : (DateTimeOffset?)null;

            // Extract cnf claim for key binding
            var cnfClaim = jwt.Payload.Claims.FirstOrDefault(c => c.Type == "cnf");
            string? keyBinding = cnfClaim?.Value;

            // Build claims dictionary
            var claims = new Dictionary<string, object>();
            foreach (var claim in jwt.Payload.Claims)
            {
                claims[claim.Type] = claim.Value;
            }

            // Build disclosure info
            var disclosures = new List<DisclosureInfo>();
            if (options.ExtractDisclosures)
            {
                foreach (var disclosure in holder.AllDisclosures)
                {
                    disclosures.Add(new DisclosureInfo
                    {
                        Path = disclosure.ClaimName ?? "[array element]",
                        Salt = disclosure.Salt,
                        Value = disclosure.ClaimValue,
                        IsSelected = true // All disclosures are available initially
                    });
                }
            }

            var parsed = new ParsedCredential
            {
                FormatId = FormatId,
                Issuer = issuer ?? string.Empty,
                Subject = subject,
                Type = credentialType,
                IssuedAt = issuedAt,
                ExpiresAt = expiration,
                Claims = claims,
                Disclosures = disclosures,
                RawCredential = credential,
                KeyBinding = keyBinding,
                Metadata = new Dictionary<string, object?>
                {
                    ["algorithm"] = jwt.Header.Alg,
                    ["sd_alg"] = jwt.Payload.Claims.FirstOrDefault(c => c.Type == "_sd_alg")?.Value,
                    ["disclosureCount"] = holder.AllDisclosures.Count
                }
            };

            return Task.FromResult(parsed);
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new InvalidOperationException($"Failed to parse SD-JWT VC: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public Task<string> CreatePresentationAsync(
        ParsedCredential credential,
        IReadOnlyList<string> disclosurePaths,
        PresentationContext context,
        IKeyManager keyManager,
        CancellationToken cancellationToken = default)
    {
        if (credential == null)
        {
            throw new ArgumentNullException(nameof(credential));
        }
        if (disclosurePaths == null)
        {
            throw new ArgumentNullException(nameof(disclosurePaths));
        }
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (keyManager == null)
        {
            throw new ArgumentNullException(nameof(keyManager));
        }

        return CreatePresentationInternalAsync(credential, disclosurePaths, context, keyManager, cancellationToken);
    }

    private async Task<string> CreatePresentationInternalAsync(
        ParsedCredential credential,
        IReadOnlyList<string> disclosurePaths,
        PresentationContext context,
        IKeyManager keyManager,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(credential.RawCredential))
        {
            throw new InvalidOperationException("RawCredential is required for presentation creation.");
        }

        var holder = new SdJwtHolder(credential.RawCredential);

        // Get the security key from the key manager
        var keyInfo = await keyManager.GetKeyInfoAsync(context.KeyId, cancellationToken).ConfigureAwait(false);
        if (keyInfo == null)
        {
            throw new InvalidOperationException($"Key with ID '{context.KeyId}' not found.");
        }

        var signingKey = await keyManager.GetSecurityKeyAsync(context.KeyId, cancellationToken).ConfigureAwait(false);
        if (signingKey == null)
        {
            throw new InvalidOperationException($"Failed to retrieve signing key for ID '{context.KeyId}'.");
        }

        // Create disclosure selector based on paths
        var disclosurePathSet = new HashSet<string>(disclosurePaths, StringComparer.OrdinalIgnoreCase);

        // Create Key Binding JWT payload
        var kbJwtPayload = new JwtPayload(
            issuer: null,
            audience: context.Audience,
            claims: null,
            notBefore: null,
            expires: context.IssuedAt.AddMinutes(5).UtcDateTime,
            issuedAt: context.IssuedAt.UtcDateTime);

        kbJwtPayload["nonce"] = context.Nonce;

        // Create the presentation
        var presentation = holder.CreatePresentation(
            disclosure =>
            {
                // Select disclosures based on path matching
                var path = disclosure.ClaimName ?? "[array element]";
                return disclosurePathSet.Contains(path) || disclosurePathSet.Contains("*");
            },
            kbJwtPayload,
            signingKey,
            context.SigningAlgorithm);

        return presentation;
    }

    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(
        ParsedCredential credential,
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        if (credential == null)
        {
            throw new ArgumentNullException(nameof(credential));
        }

        context ??= new ValidationContext();

        var errors = new List<string>();

        // Check expiration
        if (context.ValidateExpiration && credential.ExpiresAt.HasValue)
        {
            var now = DateTimeOffset.UtcNow;
            if (credential.ExpiresAt.Value.Add(-context.ClockSkewTolerance) < now)
            {
                errors.Add("Credential has expired.");
            }
        }

        // Check issuer
        if (!string.IsNullOrEmpty(context.ExpectedIssuer) &&
            !string.Equals(credential.Issuer, context.ExpectedIssuer, StringComparison.Ordinal))
        {
            errors.Add($"Issuer mismatch. Expected '{context.ExpectedIssuer}', got '{credential.Issuer}'.");
        }

        // Check not yet valid
        if (context.ValidateExpiration && credential.IssuedAt.HasValue)
        {
            var now = DateTimeOffset.UtcNow;
            if (credential.IssuedAt.Value.Add(context.ClockSkewTolerance) > now)
            {
                errors.Add("Credential is not yet valid.");
            }
        }

        var result = new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };

        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public bool CanHandle(string credential)
    {
        if (string.IsNullOrWhiteSpace(credential))
        {
            return false;
        }

        // Check if it looks like an SD-JWT (has tildes for disclosures)
        if (!credential.Contains('~'))
        {
            return false;
        }

        try
        {
            // Extract the JWT part (before the first tilde)
            var jwtPart = credential.Split('~')[0];
            var jwt = new JwtSecurityToken(jwtPart);

            // Check if the header type is vc+sd-jwt
            if (jwt.Header.Typ == SdJwtVcHeaderType)
            {
                return true;
            }

            // Also check for vct claim which indicates VC
            return jwt.Payload.Claims.Any(c => c.Type == "vct");
        }
        catch
        {
            return false;
        }
    }
}
