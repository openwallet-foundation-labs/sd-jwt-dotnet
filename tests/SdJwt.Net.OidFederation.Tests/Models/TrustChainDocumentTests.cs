using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.OidFederation.Models;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.OidFederation.Tests.Models;

public class TrustChainDocumentTests
{
    [Fact]
    public void FromJson_WithTrustChainArray_ParsesEntries()
    {
        var subjectConfiguration = CreateJwt("https://leaf.example.com", "https://leaf.example.com", "entity-configuration+jwt");
        var subordinateStatement = CreateJwt("https://anchor.example.com", "https://leaf.example.com", "entity-statement+jwt");
        var json = JsonSerializer.Serialize(new[] { subjectConfiguration, subordinateStatement });

        var document = TrustChainDocument.FromJson(json);

        document.Entries.Should().Equal(subjectConfiguration, subordinateStatement);
        document.Count.Should().Be(2);
    }

    [Fact]
    public void ToJson_WithTrustChainEntries_ReturnsJsonArray()
    {
        var document = TrustChainDocument.Create(
            CreateJwt("https://leaf.example.com", "https://leaf.example.com", "entity-configuration+jwt"),
            CreateJwt("https://anchor.example.com", "https://leaf.example.com", "entity-statement+jwt"));

        var json = document.ToJson();
        var entries = JsonSerializer.Deserialize<string[]>(json);

        entries.Should().Equal(document.Entries);
    }

    [Fact]
    public void ValidateSyntax_WithNonCompactJwt_ThrowsInvalidOperationException()
    {
        var act = () => TrustChainDocument.Create("not-a-jwt");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*compact JWT*");
    }

    [Fact]
    public void ValidateContinuity_WithValidLeafToAnchorChain_DoesNotThrow()
    {
        var document = TrustChainDocument.Create(
            CreateJwt("https://leaf.example.com", "https://leaf.example.com", "entity-configuration+jwt"),
            CreateJwt("https://intermediate.example.com", "https://leaf.example.com", "entity-statement+jwt"),
            CreateJwt("https://anchor.example.com", "https://intermediate.example.com", "entity-statement+jwt"),
            CreateJwt("https://anchor.example.com", "https://anchor.example.com", "entity-configuration+jwt"));

        var act = () => document.ValidateContinuity();

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateContinuity_WithBrokenIssuerSubjectPath_ThrowsInvalidOperationException()
    {
        var document = TrustChainDocument.Create(
            CreateJwt("https://leaf.example.com", "https://leaf.example.com", "entity-configuration+jwt"),
            CreateJwt("https://anchor.example.com", "https://other.example.com", "entity-statement+jwt"));

        var act = () => document.ValidateContinuity();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*does not continue*");
    }

    [Fact]
    public void GetExpirationTime_WithMultipleEntries_ReturnsEarliestExpiration()
    {
        var earliest = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
        var later = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var document = TrustChainDocument.Create(
            CreateJwt("https://leaf.example.com", "https://leaf.example.com", "entity-configuration+jwt", later),
            CreateJwt("https://anchor.example.com", "https://leaf.example.com", "entity-statement+jwt", earliest));

        var expiration = document.GetExpirationTime();

        expiration.Should().Be(earliest);
    }

    [Fact]
    public void OidFederationConstants_ContentTypes_ShouldIncludeTrustChainJson()
    {
        OidFederationConstants.ContentTypes.TrustChain.Should().Be("application/trust-chain+json");
    }

    private static string CreateJwt(string issuer, string subject, string typ, long? exp = null)
    {
        var header = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
        {
            alg = "none",
            typ
        })));
        var payload = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
        {
            iss = issuer,
            sub = subject,
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            exp = exp ?? DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
        })));

        return $"{header}.{payload}.";
    }
}
