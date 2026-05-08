using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Models.Dcql;
using SdJwt.Net.Oid4Vp.Models.Dcql.Formats;
using SdJwt.Net.Vc.Verifier;
using SdJwt.Net.Verifier;

namespace SdJwt.Net.Oid4Vp.Verifier.Formats;

/// <summary>
/// Validates <c>dc+sd-jwt</c> VP Token entries.
/// </summary>
public sealed class SdJwtVcVpFormatValidator : IVpFormatValidator
{
    private readonly SdJwtVcVerifier _verifier;

    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    /// <param name="verifier">The SD-JWT VC verifier.</param>
    public SdJwtVcVpFormatValidator(SdJwtVcVerifier verifier)
    {
        _verifier = verifier ?? throw new ArgumentNullException(nameof(verifier));
    }

    /// <inheritdoc/>
    public string Format => Oid4VpConstants.SdJwtVcFormat;

    /// <inheritdoc/>
    public async Task<VpFormatValidationResult> ValidateAsync(
        object presentation,
        DcqlCredentialQuery query,
        VpFormatValidationContext context,
        CancellationToken cancellationToken = default)
    {
        if (presentation is not string presentationString || string.IsNullOrWhiteSpace(presentationString))
        {
            return VpFormatValidationResult.Failed("dc+sd-jwt presentation must be a non-empty string.");
        }

        var expectedVct = (query.Meta as SdJwtVcMeta)?.VctValues?.FirstOrDefault();
        var validationParameters = context.ValidationParameters ?? new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true
        };

        try
        {
            await _verifier.VerifyAsync(
                presentationString,
                validationParameters,
                context.RequireCryptographicHolderBinding ? context.KeyBindingValidationParameters : null,
                context.RequireCryptographicHolderBinding ? context.ExpectedNonce : null,
                expectedVct,
                verificationPolicy: new SdJwtVcVerificationPolicy { AcceptLegacyTyp = true },
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return VpFormatValidationResult.Failed(ex.Message);
        }

        return VpFormatValidationResult.Success();
    }
}
