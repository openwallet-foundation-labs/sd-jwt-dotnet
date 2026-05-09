using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.SiopV2;
using System.Security.Cryptography;
using Xunit;

namespace SdJwt.Net.SiopV2.Tests;

public class JwkThumbprintTests
{
    [Fact]
    public void ComputeSubject_WithEcPublicJwk_ReturnsStableThumbprint()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var jwk = JsonWebKeyConverter.ConvertFromSecurityKey(new ECDsaSecurityKey(ecdsa));

        var first = SiopSubject.CreateJwkThumbprintSubject(jwk);
        var second = SiopSubject.CreateJwkThumbprintSubject(new JsonWebKey(SiopSubject.SerializePublicJwk(jwk)));

        first.Should().Be(second);
        first.Should().StartWith(SiopConstants.SubjectSyntaxTypes.JwkThumbprintSha256Prefix);
        first.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ComputeSubject_WithMissingRequiredEcMember_ThrowsInvalidOperationException()
    {
        var jwk = new JsonWebKey("""{"kty":"EC","crv":"P-256","x":"abc"}""");

        var action = () => SiopSubject.CreateJwkThumbprintSubject(jwk);

        action.Should().Throw<InvalidOperationException>().WithMessage("*y*");
    }
}
