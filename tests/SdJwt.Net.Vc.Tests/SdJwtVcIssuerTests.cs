using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Issuer;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace SdJwt.Net.Vc.Tests;

public class SdJwtVcIssuerTests : TestBase
{
    [Fact]
    public void IssueSimple_ShouldWork()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new Dictionary<string, object>
        {
            { "name", "John Doe" },
            { "age", 30 }
        };
        var options = new SdIssuanceOptions { DisclosureStructure = new { name = true } };

        var output = vcIssuer.IssueSimple(
            "https://example.com/vct",
            TrustedIssuer,
            "did:example:123",
            claims,
            options,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            HolderPublicJwk,
            null,
            "integrity-hash"
        );

        Assert.NotNull(output);
        var jwt = new JwtSecurityToken(output.SdJwt);
        Assert.Equal("https://example.com/vct", jwt.Payload["vct"]);
        Assert.Equal("integrity-hash", jwt.Payload["vct#integrity"]);
        Assert.Equal(TrustedIssuer, jwt.Payload.Iss);
        Assert.Equal("did:example:123", jwt.Payload.Sub);
    }

    [Fact]
    public void IssueSimple_ShouldThrow_WhenInvalidArguments()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new Dictionary<string, object> { { "test", "value" } };
        var options = new SdIssuanceOptions();

        Assert.Throws<ArgumentException>(() => vcIssuer.IssueSimple(
            "https://example.com/vct",
            "", // Empty issuer
            "did:example:123",
            claims,
            options
        ));

        Assert.Throws<ArgumentException>(() => vcIssuer.IssueSimple(
            "https://example.com/vct",
            TrustedIssuer,
            "", // Empty subject
            claims,
            options
        ));

        Assert.Throws<ArgumentException>(() => vcIssuer.IssueSimple(
            "https://example.com/vct",
            TrustedIssuer,
            "did:example:123",
            new Dictionary<string, object>(), // Empty claims
            options
        ));
    }

    [Fact]
    public void CreateKeyBindingConfiguration_ShouldWork()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var config = vcIssuer.CreateKeyBindingConfiguration(HolderPublicJwk);

        Assert.NotNull(config);
        Assert.Equal(HolderPublicJwk, config.Jwk);
    }

    [Fact]
    public void CreateKeyBindingConfiguration_ShouldThrow_WhenNullKey()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        Assert.Throws<ArgumentNullException>(() => vcIssuer.CreateKeyBindingConfiguration(null!));
    }

    [Fact]
    public void CreateStatusReference_ShouldWork()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var status = vcIssuer.CreateStatusReference("https://example.com/status", 5);

        Assert.NotNull(status);
        var json = System.Text.Json.JsonSerializer.Serialize(status);
        Assert.Contains("https://example.com/status", json);
        Assert.Contains("5", json);
    }

    [Fact]
    public void CreateStatusReference_ShouldThrow_WhenInvalidArguments()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);

        Assert.Throws<ArgumentException>(() => vcIssuer.CreateStatusReference("", 0));
        Assert.Throws<ArgumentException>(() => vcIssuer.CreateStatusReference("https://example.com", -1));
    }

    [Fact]
    public void Issue_ShouldThrow_WhenVctIsNotCollisionResistant()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123",
            AdditionalData = new Dictionary<string, object> { { "test", "value" } }
        };

        Assert.Throws<ArgumentException>(() => vcIssuer.Issue("invalid_vct", payload, new SdIssuanceOptions()));
    }

    [Fact]
    public void Issue_ShouldThrow_WhenNullArguments()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123",
            AdditionalData = new Dictionary<string, object> { { "test", "value" } }
        };

        Assert.Throws<ArgumentException>(() => vcIssuer.Issue("", payload, new SdIssuanceOptions()));
        Assert.Throws<ArgumentNullException>(() => vcIssuer.Issue("https://example.com/vct", null!, new SdIssuanceOptions()));
        Assert.Throws<ArgumentNullException>(() => vcIssuer.Issue("https://example.com/vct", payload, null!));
    }

    [Fact]
    public void Issue_ShouldValidateTimingClaims()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // nbf > exp
        var payload1 = new SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123",
            NotBefore = now + 100,
            ExpiresAt = now + 50,
            AdditionalData = new Dictionary<string, object> { { "test", "value" } }
        };

        Assert.Throws<ArgumentException>(() => vcIssuer.Issue("https://example.com/vct", payload1, new SdIssuanceOptions()));

        // iat > exp
        var payload2 = new SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123",
            IssuedAt = now + 100,
            ExpiresAt = now + 50,
            AdditionalData = new Dictionary<string, object> { { "test", "value" } }
        };

        Assert.Throws<ArgumentException>(() => vcIssuer.Issue("https://example.com/vct", payload2, new SdIssuanceOptions()));
    }

    [Fact]
    public void Issue_ShouldValidateConfirmationMethod()
    {
        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:123",
            Confirmation = new { invalid = "data" },
            AdditionalData = new Dictionary<string, object> { { "test", "value" } }
        };

        Assert.Throws<ArgumentException>(() => vcIssuer.Issue("https://example.com/vct", payload, new SdIssuanceOptions()));
    }
}
