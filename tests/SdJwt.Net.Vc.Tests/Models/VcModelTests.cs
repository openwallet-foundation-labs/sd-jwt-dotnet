using FluentAssertions;
using SdJwt.Net.Vc.Models;
using System.Collections.Generic;
using Xunit;

namespace SdJwt.Net.Vc.Tests.Models;

public class VcModelTests
{
    [Fact]
    public void Address_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var address = new Address();

        // Act
        address.StreetAddress = "123 Main St";
        address.Locality = "Anytown";
        address.Region = "CA";
        address.PostalCode = "12345";
        address.Country = "US";

        // Assert
        address.StreetAddress.Should().Be("123 Main St");
        address.Locality.Should().Be("Anytown");
        address.Region.Should().Be("CA");
        address.PostalCode.Should().Be("12345");
        address.Country.Should().Be("US");
    }

    [Fact]
    public void ClaimMetadata_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var claimMetadata = new ClaimMetadata();
        var display = new DisplayMetadata[]
        {
            new DisplayMetadata { Name = "Display Name", Locale = "en-US" }
        };

        // Act
        claimMetadata.Mandatory = true;
        claimMetadata.SelectiveDisclosure = "always";
        claimMetadata.Display = display;
        claimMetadata.SvgId = "svg-123";

        // Assert
        claimMetadata.Mandatory.Should().BeTrue();
        claimMetadata.SelectiveDisclosure.Should().Be("always");
        claimMetadata.Display.Should().BeEquivalentTo(display);
        claimMetadata.SvgId.Should().Be("svg-123");
    }

    [Fact]
    public void CredentialSubject_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var credentialSubject = new CredentialSubject();
        var additionalData = new Dictionary<string, object>
        {
            ["name"] = "John Doe",
            ["age"] = 30
        };

        // Act
        credentialSubject.Id = "did:example:123";
        credentialSubject.AdditionalData = additionalData;

        // Assert
        credentialSubject.Id.Should().Be("did:example:123");
        credentialSubject.AdditionalData.Should().BeEquivalentTo(additionalData);
    }

    [Fact]
    public void DisplayMetadata_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var displayMetadata = new DisplayMetadata();

        // Act
        displayMetadata.Name = "Full Name";
        displayMetadata.Locale = "en-US";
        displayMetadata.Description = "A person's full legal name";

        // Assert
        displayMetadata.Name.Should().Be("Full Name");
        displayMetadata.Locale.Should().Be("en-US");
        displayMetadata.Description.Should().Be("A person's full legal name");
    }

    [Fact]
    public void JsonWebKey_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var jwk = new JsonWebKey();

        // Act
        jwk.KeyType = "EC";
        jwk.Use = "sig";
        jwk.Curve = "P-256";
        jwk.X = "MKBCTNIcKUSDii11ySs3526iDZ8AiTo7Tu6KPAqv7D4";
        jwk.Y = "4Etl6SRW2YiLUrN5vfvVHuhp7x8PxltmWWlbbM4IFyM";
        jwk.KeyId = "test-key-id";
        jwk.Algorithm = "ES256";

        // Assert
        jwk.KeyType.Should().Be("EC");
        jwk.Use.Should().Be("sig");
        jwk.Curve.Should().Be("P-256");
        jwk.X.Should().Be("MKBCTNIcKUSDii11ySs3526iDZ8AiTo7Tu6KPAqv7D4");
        jwk.Y.Should().Be("4Etl6SRW2YiLUrN5vfvVHuhp7x8PxltmWWlbbM4IFyM");
        jwk.KeyId.Should().Be("test-key-id");
        jwk.Algorithm.Should().Be("ES256");
    }

    [Fact]
    public void JwkSet_ShouldAllowSettingKeys()
    {
        // Arrange
        var jwkSet = new JwkSet();
        var keys = new JsonWebKey[]
        {
            new JsonWebKey { KeyType = "EC", KeyId = "key1" },
            new JsonWebKey { KeyType = "RSA", KeyId = "key2" }
        };

        // Act
        jwkSet.Keys = keys;

        // Assert
        jwkSet.Keys.Should().BeEquivalentTo(keys);
    }

    [Fact]
    public void JwtVcIssuerMetadata_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var metadata = new JwtVcIssuerMetadata();

        // Act
        metadata.Issuer = "https://issuer.example.com";
        metadata.Jwks = new JwkSet { Keys = new JsonWebKey[0] };

        // Assert
        metadata.Issuer.Should().Be("https://issuer.example.com");
        metadata.Jwks.Should().NotBeNull();
    }

    [Fact]
    public void KeyBindingJwtPayload_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var payload = new KeyBindingJwtPayload();

        // Act
        payload.Audience = "https://verifier.example.com";
        payload.IssuedAt = 1234567890;
        payload.Nonce = "test-nonce";

        // Assert
        payload.Audience.Should().Be("https://verifier.example.com");
        payload.IssuedAt.Should().Be(1234567890);
        payload.Nonce.Should().Be("test-nonce");
    }

    [Fact]
    public void TypeMetadata_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var typeMetadata = new TypeMetadata();
        var claimsMetadata = new ClaimMetadata[]
        {
            new ClaimMetadata { Mandatory = true, SelectiveDisclosure = "always" }
        };
        var display = new DisplayMetadata[]
        {
            new DisplayMetadata { Name = "Test Credential" }
        };

        // Act
        typeMetadata.Vct = "https://example.com/test-credential";
        typeMetadata.Name = "Test Credential";
        typeMetadata.Claims = claimsMetadata;
        typeMetadata.Display = display;

        // Assert
        typeMetadata.Vct.Should().Be("https://example.com/test-credential");
        typeMetadata.Name.Should().Be("Test Credential");
        typeMetadata.Claims.Should().BeEquivalentTo(claimsMetadata);
        typeMetadata.Display.Should().BeEquivalentTo(display);
    }
}

// Mock classes for testing if they don't exist
public class LogoInfo
{
    public string? Uri { get; set; }
}
