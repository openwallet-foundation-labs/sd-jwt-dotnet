using FluentAssertions;
using SdJwt.Net.Oid4Vci.Federation;
using SdJwt.Net.Oid4Vci.Models;
using SdJwt.Net.OidFederation.Logic;
using SdJwt.Net.OidFederation.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vci.Tests.Federation;

public class FederatedCredentialIssuerValidatorTests
{
    [Fact]
    public void ValidateTrustChain_WithTrustedCredentialIssuer_ReturnsTrustedMetadata()
    {
        var trustChain = TrustChainResult.Success(
            "https://trust-anchor.example.com",
            CreateEntityConfiguration(new CredentialIssuerMetadata
            {
                CredentialIssuer = "https://issuer.example.com",
                CredentialEndpoint = "https://issuer.example.com/credential"
            }),
            Array.Empty<EntityStatement>());

        var result = FederatedCredentialIssuerValidator.ValidateTrustChain(
            trustChain,
            new FederatedCredentialIssuerValidationOptions
            {
                ExpectedCredentialIssuer = "https://issuer.example.com",
                AllowedTrustAnchors = ["https://trust-anchor.example.com"]
            });

        result.IsTrusted.Should().BeTrue();
        result.Metadata.Should().NotBeNull();
        result.Metadata!.CredentialIssuer.Should().Be("https://issuer.example.com");
        result.TrustAnchor.Should().Be("https://trust-anchor.example.com");
    }

    [Fact]
    public void ValidateTrustChain_WithMissingCredentialIssuerMetadata_ReturnsUntrusted()
    {
        var trustChain = TrustChainResult.Success(
            "https://trust-anchor.example.com",
            new EntityConfiguration
            {
                Issuer = "https://issuer.example.com",
                Subject = "https://issuer.example.com",
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
                JwkSet = new { keys = Array.Empty<object>() },
                Metadata = new EntityMetadata()
            },
            Array.Empty<EntityStatement>());

        var result = FederatedCredentialIssuerValidator.ValidateTrustChain(trustChain);

        result.IsTrusted.Should().BeFalse();
        result.ErrorMessage.Should().Contain("openid_credential_issuer");
    }

    [Fact]
    public void ValidateTrustChain_WithUnexpectedCredentialIssuer_ReturnsUntrusted()
    {
        var trustChain = TrustChainResult.Success(
            "https://trust-anchor.example.com",
            CreateEntityConfiguration(new CredentialIssuerMetadata
            {
                CredentialIssuer = "https://issuer.example.com",
                CredentialEndpoint = "https://issuer.example.com/credential"
            }),
            Array.Empty<EntityStatement>());

        var result = FederatedCredentialIssuerValidator.ValidateTrustChain(
            trustChain,
            new FederatedCredentialIssuerValidationOptions
            {
                ExpectedCredentialIssuer = "https://other-issuer.example.com"
            });

        result.IsTrusted.Should().BeFalse();
        result.ErrorMessage.Should().Contain("credential issuer");
    }

    private static EntityConfiguration CreateEntityConfiguration(CredentialIssuerMetadata metadata)
    {
        return new EntityConfiguration
        {
            Issuer = metadata.CredentialIssuer,
            Subject = metadata.CredentialIssuer,
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            JwkSet = new { keys = Array.Empty<object>() },
            Metadata = new EntityMetadata
            {
                OpenIdCredentialIssuer = metadata
            }
        };
    }
}
