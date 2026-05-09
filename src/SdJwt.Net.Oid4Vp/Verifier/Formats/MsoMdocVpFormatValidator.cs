using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Mdoc.Handover;
using SdJwt.Net.Mdoc.Models;
using SdJwt.Net.Mdoc.Verifier;
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Models.Dcql;
using SdJwt.Net.Oid4Vp.Models.Dcql.Formats;

namespace SdJwt.Net.Oid4Vp.Verifier.Formats;

/// <summary>
/// Validates <c>mso_mdoc</c> VP Token entries containing base64url-encoded mdoc <see cref="DeviceResponse"/> CBOR.
/// </summary>
public sealed class MsoMdocVpFormatValidator : IVpFormatValidator
{
    private readonly MdocVerifier _verifier;

    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    /// <param name="verifier">The mdoc verifier.</param>
    public MsoMdocVpFormatValidator(MdocVerifier verifier)
    {
        _verifier = verifier ?? throw new ArgumentNullException(nameof(verifier));
    }

    /// <inheritdoc/>
    public string Format => Oid4VpConstants.MsoMdocFormat;

    /// <inheritdoc/>
    public Task<VpFormatValidationResult> ValidateAsync(
        object presentation,
        DcqlCredentialQuery query,
        VpFormatValidationContext context,
        CancellationToken cancellationToken = default)
    {
        if (presentation is not string encodedDeviceResponse || string.IsNullOrWhiteSpace(encodedDeviceResponse))
        {
            return Task.FromResult(VpFormatValidationResult.Failed("mso_mdoc presentation must be a non-empty base64url string."));
        }

        DeviceResponse response;
        try
        {
            response = DeviceResponse.FromCbor(Base64UrlEncoder.DecodeBytes(encodedDeviceResponse));
        }
        catch (Exception ex)
        {
            return Task.FromResult(VpFormatValidationResult.Failed($"mso_mdoc presentation is not a valid DeviceResponse: {ex.Message}"));
        }

        var expectedDoctype = (query.Meta as MsoMdocMeta)?.DoctypeValue;
        if (!string.IsNullOrWhiteSpace(expectedDoctype) &&
            !response.Documents.Any(document => string.Equals(document.DocType, expectedDoctype, StringComparison.Ordinal)))
        {
            return Task.FromResult(VpFormatValidationResult.Failed($"mso_mdoc DeviceResponse does not contain doctype '{expectedDoctype}'."));
        }

        var sessionTranscript = !string.IsNullOrWhiteSpace(context.ExpectedClientId)
            ? SessionTranscript.ForOpenId4Vp(
                context.ExpectedClientId!,
                context.ExpectedNonce,
                null,
                context.ExpectedClientId!)
            : null;

        var results = _verifier.Verify(response, sessionTranscript);
        if (results.Count == 0 || results.Any(result => !result.IsValid))
        {
            var error = results.FirstOrDefault(result => !result.IsValid)?.Errors.FirstOrDefault() ?? "mdoc verification failed.";
            return Task.FromResult(VpFormatValidationResult.Failed(error));
        }

        return Task.FromResult(VpFormatValidationResult.Success());
    }
}
