using FluentAssertions;
using SdJwt.Net.Oid4Vp.Federation;
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Models.Dcql;
using SdJwt.Net.OidFederation.Logic;
using SdJwt.Net.OidFederation.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Federation;

public class FederatedVerifierValidatorTests
{
    [Fact]
    public void ValidateTrustChain_WithTrustedVerifier_ReturnsTrustedMetadata()
    {
        var trustChain = CreateVerifierTrustChain("https://trust-anchor.example.com");
        var request = new AuthorizationRequest
        {
            ClientId = "https://verifier.example.com",
            ClientIdScheme = FederatedVerifierValidator.OpenIdFederationClientIdScheme,
            ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
            Nonce = "nonce",
            PresentationDefinition = PresentationDefinition.CreateSimple("pd", "UniversityDegree")
        };

        var result = FederatedVerifierValidator.ValidateTrustChain(
            request,
            trustChain,
            new FederatedVerifierValidationOptions
            {
                AllowedTrustAnchors = ["https://trust-anchor.example.com"]
            });

        result.IsTrusted.Should().BeTrue();
        result.Metadata.Should().NotBeNull();
        result.TrustAnchor.Should().Be("https://trust-anchor.example.com");
    }

    [Fact]
    public void ValidateTrustChain_WithClientIdMismatch_ReturnsUntrusted()
    {
        var trustChain = CreateVerifierTrustChain("https://trust-anchor.example.com");
        var request = new AuthorizationRequest
        {
            ClientId = "https://other-verifier.example.com",
            ClientIdScheme = FederatedVerifierValidator.OpenIdFederationClientIdScheme,
            ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
            Nonce = "nonce",
            PresentationDefinition = PresentationDefinition.CreateSimple("pd", "UniversityDegree")
        };

        var result = FederatedVerifierValidator.ValidateTrustChain(request, trustChain);

        result.IsTrusted.Should().BeFalse();
        result.ErrorMessage.Should().Contain("client_id");
    }

    [Fact]
    public void SatisfiesOpenIdFederationAuthorities_WithMatchingTrustAnchor_ReturnsTrue()
    {
        var trustChain = CreateVerifierTrustChain("https://trust-anchor.example.com");
        var authorities = new[]
        {
            new TrustedAuthority
            {
                Type = "openid_federation",
                Values = ["https://trust-anchor.example.com"]
            }
        };

        var result = FederatedVerifierValidator.SatisfiesOpenIdFederationAuthorities(trustChain, authorities);

        result.Should().BeTrue();
    }

    private static TrustChainResult CreateVerifierTrustChain(string trustAnchor)
    {
        return TrustChainResult.Success(
            trustAnchor,
            new EntityConfiguration
            {
                Issuer = "https://verifier.example.com",
                Subject = "https://verifier.example.com",
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
                JwkSet = new { keys = Array.Empty<object>() },
                Metadata = new EntityMetadata
                {
                    OpenIdRelyingPartyVerifier = VerifierMetadata.CreateForSdJwtVc()
                }
            },
            Array.Empty<EntityStatement>());
    }
}
