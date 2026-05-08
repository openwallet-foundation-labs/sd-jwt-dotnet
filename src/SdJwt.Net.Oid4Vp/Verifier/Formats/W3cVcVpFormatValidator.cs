using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Models.Dcql;
using SdJwt.Net.Oid4Vp.Models.Dcql.Formats;
using SdJwt.Net.VcDm.Models;
using SdJwt.Net.VcDm.Serialization;
using SdJwt.Net.VcDm.Validation;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace SdJwt.Net.Oid4Vp.Verifier.Formats;

/// <summary>
/// Performs structural validation for W3C VC VP Token entries.
/// Cryptographic JWT/Data Integrity verification is delegated to application-specific trust configuration.
/// </summary>
public sealed class W3cVcVpFormatValidator : IVpFormatValidator
{
    private readonly VcDmValidator _validator;

    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    /// <param name="validator">The VCDM structural validator.</param>
    public W3cVcVpFormatValidator(VcDmValidator? validator = null)
    {
        _validator = validator ?? new VcDmValidator();
    }

    /// <inheritdoc/>
    public string Format => Oid4VpConstants.JwtVcJsonFormat;

    /// <inheritdoc/>
    public Task<VpFormatValidationResult> ValidateAsync(
        object presentation,
        DcqlCredentialQuery query,
        VpFormatValidationContext context,
        CancellationToken cancellationToken = default)
    {
        return query.Format switch
        {
            Oid4VpConstants.JwtVcJsonFormat or Oid4VpConstants.JwtVcJsonLdFormat
                => Task.FromResult(ValidateJwtPresentation(presentation, query, context)),
            Oid4VpConstants.LdpVcFormat
                => Task.FromResult(ValidateDataIntegrityPresentation(presentation, query, context)),
            _ => Task.FromResult(VpFormatValidationResult.Failed($"Unsupported W3C VC format '{query.Format}'."))
        };
    }

    private VpFormatValidationResult ValidateJwtPresentation(
        object presentation,
        DcqlCredentialQuery query,
        VpFormatValidationContext context)
    {
        if (presentation is not string jwt || string.IsNullOrWhiteSpace(jwt))
        {
            return VpFormatValidationResult.Failed($"{query.Format} presentation must be a non-empty JWT string.");
        }

        JwtSecurityToken token;
        try
        {
            token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        }
        catch (Exception ex)
        {
            return VpFormatValidationResult.Failed($"Invalid W3C JWT VP: {ex.Message}");
        }

        if (!string.IsNullOrWhiteSpace(context.ExpectedNonce) &&
            token.Payload.TryGetValue(JwtRegisteredClaimNames.Nonce, out var nonceObj) &&
            !string.Equals(nonceObj?.ToString(), context.ExpectedNonce, StringComparison.Ordinal))
        {
            return VpFormatValidationResult.Failed("W3C JWT VP nonce does not match the authorization request nonce.");
        }

        if (!string.IsNullOrWhiteSpace(context.ExpectedClientId) &&
            !token.Audiences.Contains(context.ExpectedClientId, StringComparer.Ordinal))
        {
            return VpFormatValidationResult.Failed("W3C JWT VP audience does not match the verifier client_id.");
        }

        var typeValidation = ValidateTypeValues(token.Payload, query);
        return typeValidation ?? VpFormatValidationResult.Success();
    }

    private VpFormatValidationResult ValidateDataIntegrityPresentation(
        object presentation,
        DcqlCredentialQuery query,
        VpFormatValidationContext context)
    {
        VerifiablePresentation vp;
        try
        {
            var json = presentation is JsonElement element
                ? element.GetRawText()
                : JsonSerializer.Serialize(presentation, VcDmSerializerOptions.Default);
            vp = JsonSerializer.Deserialize<VerifiablePresentation>(json, VcDmSerializerOptions.Default)
                ?? throw new JsonException("Presentation deserialized to null.");
        }
        catch (Exception ex)
        {
            return VpFormatValidationResult.Failed($"Invalid W3C Data Integrity VP: {ex.Message}");
        }

        var validation = _validator.Validate(vp);
        if (!validation.IsValid)
        {
            return VpFormatValidationResult.Failed(string.Join("; ", validation.Errors));
        }

        if (context.RequireCryptographicHolderBinding &&
            (vp.Proof == null || vp.Proof.Length == 0))
        {
            return VpFormatValidationResult.Failed("W3C Data Integrity VP proof is required.");
        }

        if (vp.Proof != null && !string.IsNullOrWhiteSpace(context.ExpectedNonce) &&
            vp.Proof.Any(proof => !string.Equals(proof.Challenge, context.ExpectedNonce, StringComparison.Ordinal)))
        {
            return VpFormatValidationResult.Failed("W3C Data Integrity VP challenge does not match the authorization request nonce.");
        }

        if (vp.Proof != null && !string.IsNullOrWhiteSpace(context.ExpectedClientId) &&
            vp.Proof.Any(proof => !string.Equals(proof.Domain, context.ExpectedClientId, StringComparison.Ordinal)))
        {
            return VpFormatValidationResult.Failed("W3C Data Integrity VP domain does not match the verifier client_id.");
        }

        return VpFormatValidationResult.Success();
    }

    private static VpFormatValidationResult? ValidateTypeValues(JwtPayload payload, DcqlCredentialQuery query)
    {
        if (query.Meta is not W3cVcMeta w3cMeta || w3cMeta.TypeValues == null || w3cMeta.TypeValues.Length == 0)
        {
            return null;
        }

        var credentialTypes = ExtractTypeValues(payload);
        if (credentialTypes.Count == 0)
        {
            return VpFormatValidationResult.Failed("W3C JWT VC does not expose credential type values.");
        }

        var matches = w3cMeta.TypeValues.Any(requiredSet =>
            requiredSet.All(required => credentialTypes.Contains(required, StringComparer.Ordinal)));

        return matches
            ? null
            : VpFormatValidationResult.Failed("W3C JWT VC type values do not satisfy DCQL meta.type_values.");
    }

    private static HashSet<string> ExtractTypeValues(JwtPayload payload)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        if (payload.TryGetValue("vc", out var vcObj))
        {
            ExtractTypesFromObject(vcObj, result);
        }

        if (payload.TryGetValue("type", out var typeObj))
        {
            ExtractTypesFromObject(typeObj, result);
        }

        return result;
    }

    private static void ExtractTypesFromObject(object? value, HashSet<string> target)
    {
        switch (value)
        {
            case null:
                return;
            case string s:
                target.Add(s);
                return;
            case JsonElement element when element.ValueKind == JsonValueKind.String:
                target.Add(element.GetString()!);
                return;
            case JsonElement element when element.ValueKind == JsonValueKind.Array:
                foreach (var entry in element.EnumerateArray())
                {
                    if (entry.ValueKind == JsonValueKind.String)
                    {
                        target.Add(entry.GetString()!);
                    }
                }
                return;
            case JsonElement element when element.ValueKind == JsonValueKind.Object && element.TryGetProperty("type", out var type):
                ExtractTypesFromObject(type, target);
                return;
            case IEnumerable<object> values:
                foreach (var entry in values)
                {
                    ExtractTypesFromObject(entry, target);
                }
                return;
        }
    }
}
