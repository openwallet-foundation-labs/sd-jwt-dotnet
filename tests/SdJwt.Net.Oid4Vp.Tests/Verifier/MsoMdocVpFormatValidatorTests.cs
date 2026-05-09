using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Mdoc.Models;
using SdJwt.Net.Mdoc.Verifier;
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Models.Dcql;
using SdJwt.Net.Oid4Vp.Models.Dcql.Formats;
using SdJwt.Net.Oid4Vp.Verifier.Formats;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Verifier;

public class MsoMdocVpFormatValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WithEmptyPresentation_ShouldFail()
    {
        var validator = new MsoMdocVpFormatValidator(new MdocVerifier());
        var query = CreateMdocQuery("org.iso.18013.5.1.mDL");

        var result = await validator.ValidateAsync(string.Empty, query, new VpFormatValidationContext());

        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("mso_mdoc");
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidDeviceResponseCbor_ShouldFail()
    {
        var validator = new MsoMdocVpFormatValidator(new MdocVerifier());
        var query = CreateMdocQuery("org.iso.18013.5.1.mDL");

        var result = await validator.ValidateAsync(
            Base64UrlEncoder.Encode(new byte[] { 0x01, 0x02, 0x03 }),
            query,
            new VpFormatValidationContext());

        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("DeviceResponse");
    }

    [Fact]
    public async Task ValidateAsync_WithMismatchedDoctype_ShouldFailBeforeMdocVerification()
    {
        var validator = new MsoMdocVpFormatValidator(new MdocVerifier());
        var query = CreateMdocQuery("org.iso.18013.5.1.mDL");
        var response = new DeviceResponse
        {
            Documents =
            [
                new Document { DocType = "org.iso.18013.5.1.pid" }
            ]
        };

        var result = await validator.ValidateAsync(
            Base64UrlEncoder.Encode(response.ToCbor()),
            query,
            new VpFormatValidationContext());

        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("doctype");
    }

    private static DcqlCredentialQuery CreateMdocQuery(string doctype)
    {
        return new DcqlCredentialQuery
        {
            Id = "mdl",
            Format = Oid4VpConstants.MsoMdocFormat,
            Meta = new MsoMdocMeta
            {
                DoctypeValue = doctype
            }
        };
    }
}
