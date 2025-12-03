using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Vc.Models;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using MsftJsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;
using ModelJsonWebKey = SdJwt.Net.Vc.Models.JsonWebKey;

namespace SdJwt.Net.Vc.Tests;

public class VcEnhancedTests : TestBase
{
    [Fact]
    public void SdJwtVcPayload_WithValidData_ShouldValidate()
    {
        // Arrange
        var payload = new SdJwtVcPayload
        {
            Vct = "ExampleCredential",
            Issuer = "https://issuer.example.com",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        // Act & Assert
        payload.Should().NotBeNull();
        payload.Vct.Should().Be("ExampleCredential");
        payload.Issuer.Should().Be("https://issuer.example.com");
    }

    [Fact]
    public void SdJwtVcPayload_WithExpirationDate_ShouldSetCorrectly()
    {
        // Arrange
        var issuedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var futureTime = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds();
        var payload = new SdJwtVcPayload
        {
            Vct = "ExampleCredential",
            Issuer = "https://issuer.example.com",
            IssuedAt = issuedTime,
            ExpiresAt = futureTime
        };

        // Act & Assert
        payload.ExpiresAt.Should().Be(futureTime);
        payload.ExpiresAt.Value.Should().BeGreaterThan(payload.IssuedAt!.Value);
    }

    [Fact]
    public void SdJwtVcPayload_WithAdditionalData_ShouldStoreCorrectly()
    {
        // Arrange
        var payload = new SdJwtVcPayload
        {
            Vct = "ExampleCredential",
            Issuer = "https://issuer.example.com",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["name"] = "John Doe",
                ["age"] = 30,
                ["verified"] = true
            }
        };

        // Act & Assert
        payload.AdditionalData.Should().NotBeNull();
        payload.AdditionalData.Should().HaveCount(3);
        payload.AdditionalData["name"].Should().Be("John Doe");
        payload.AdditionalData["age"].Should().Be(30);
        payload.AdditionalData["verified"].Should().Be(true);
    }

    [Fact]
    public void SdJwtVcPayload_WithStatus_ShouldStoreCorrectly()
    {
        // Arrange
        var statusData = new
        {
            status_list = new
            {
                idx = 42,
                uri = "https://issuer.example.com/status"
            }
        };

        var payload = new SdJwtVcPayload
        {
            Vct = "ExampleCredential",
            Issuer = "https://issuer.example.com",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Status = statusData
        };

        // Act & Assert
        payload.Status.Should().NotBeNull();
        payload.Status.Should().Be(statusData);
    }

    [Fact]
    public void ConfirmationMethod_WithJwk_ShouldCreateCorrectly()
    {
        // Arrange
        var jwk = new MsftJsonWebKey();
        jwk.Kty = "EC";
        jwk.Crv = "P-256";

        // Act
        var confirmation = new ConfirmationMethod
        {
            Jwk = jwk
        };

        // Assert
        confirmation.Should().NotBeNull();
        confirmation.Jwk.Should().Be(jwk);
        ((MsftJsonWebKey)confirmation.Jwk!).Kty.Should().Be("EC");
        ((MsftJsonWebKey)confirmation.Jwk!).Crv.Should().Be("P-256");
    }

    [Fact]
    public void ConfirmationMethod_WithJkuAndKid_ShouldCreateCorrectly()
    {
        // Arrange
        var jku = "https://issuer.example.com/.well-known/jwks.json";
        var kid = "key-1";

        // Act
        var confirmation = new ConfirmationMethod
        {
            JwkSetUrl = jku,
            KeyId = kid
        };

        // Assert
        confirmation.Should().NotBeNull();
        confirmation.JwkSetUrl.Should().Be(jku);
        confirmation.KeyId.Should().Be(kid);
    }

    [Fact]
    public void ConfirmationMethod_WithAdditionalData_ShouldStoreCorrectly()
    {
        // Arrange
        var confirmation = new ConfirmationMethod
        {
            Jwk = new MsftJsonWebKey { Kty = "EC" },
            JwkSetUrl = "https://example.com/jwks.json",
            KeyId = "key-id",
            AdditionalData = new Dictionary<string, object>
            {
                ["custom"] = "value",
                ["x5t"] = "abc123"
            }
        };

        // Assert
        confirmation.Should().NotBeNull();
        confirmation.Jwk.Should().NotBeNull();
        confirmation.JwkSetUrl.Should().NotBeNullOrEmpty();
        confirmation.KeyId.Should().NotBeNullOrEmpty();
        confirmation.AdditionalData.Should().ContainKey("custom");
        confirmation.AdditionalData.Should().ContainKey("x5t");
    }

    [Fact]
    public void ConfirmationMethod_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var confirmation = new ConfirmationMethod();

        // Assert
        confirmation.Should().NotBeNull();
        confirmation.Jwk.Should().BeNull();
        confirmation.JwkSetUrl.Should().BeNull();
        confirmation.KeyId.Should().BeNull();
        confirmation.AdditionalData.Should().BeNull();
    }

    [Fact]
    public void ConfirmationMethod_WithEmptyAdditionalData_ShouldHandleCorrectly()
    {
        // Act
        var confirmation = new ConfirmationMethod
        {
            AdditionalData = new Dictionary<string, object>()
        };

        // Assert
        confirmation.AdditionalData.Should().NotBeNull();
        confirmation.AdditionalData.Should().BeEmpty();
    }

    [Fact]
    public void KeyBindingJwtPayload_WithRequiredFields_ShouldCreateSuccessfully()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Act
        var payload = new KeyBindingJwtPayload
        {
            Audience = "https://verifier.example.com",
            IssuedAt = now,
            SdHash = "sha256-abcdef123456"
        };

        // Assert
        payload.Should().NotBeNull();
        payload.Audience.Should().Be("https://verifier.example.com");
        payload.IssuedAt.Should().Be(now);
        payload.SdHash.Should().Be("sha256-abcdef123456");
    }

    [Fact]
    public void KeyBindingJwtPayload_WithNonce_ShouldStoreCorrectly()
    {
        // Arrange
        var nonce = "random-nonce-123";
        var payload = new KeyBindingJwtPayload
        {
            Audience = "https://verifier.example.com",
            Nonce = nonce,
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            SdHash = "sha256-abcdef123456"
        };

        // Assert
        payload.Nonce.Should().Be(nonce);
    }

    [Fact]
    public void KeyBindingJwtPayload_WithAdditionalData_ShouldStoreCorrectly()
    {
        // Arrange
        var payload = new KeyBindingJwtPayload
        {
            Audience = "https://verifier.example.com",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            SdHash = "sha256-abcdef123456",
            AdditionalData = new Dictionary<string, object>
            {
                ["custom_claim"] = "custom_value",
                ["exp"] = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
            }
        };

        // Assert
        payload.AdditionalData.Should().ContainKey("custom_claim");
        payload.AdditionalData.Should().ContainKey("exp");
        payload.AdditionalData["custom_claim"].Should().Be("custom_value");
    }

    [Fact]
    public void KeyBindingJwtPayload_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var payload = new KeyBindingJwtPayload();

        // Assert
        payload.Should().NotBeNull();
        payload.Audience.Should().BeNull();
        payload.Nonce.Should().BeNull();
        payload.IssuedAt.Should().Be(0);
        payload.SdHash.Should().BeNull();
        payload.AdditionalData.Should().BeNull();
    }

    [Fact]
    public void JwtVcIssuerMetadata_WithJwksUri_ShouldCreateCorrectly()
    {
        // Act
        var metadata = new JwtVcIssuerMetadata
        {
            Issuer = "https://issuer.example.com",
            JwksUri = "https://issuer.example.com/.well-known/jwks.json"
        };

        // Assert
        metadata.Should().NotBeNull();
        metadata.Issuer.Should().Be("https://issuer.example.com");
        metadata.JwksUri.Should().Be("https://issuer.example.com/.well-known/jwks.json");
        metadata.Jwks.Should().BeNull();
    }

    [Fact]
    public void JwtVcIssuerMetadata_WithJwks_ShouldCreateCorrectly()
    {
        // Arrange
        var jwks = new JwkSet
        {
            Keys = new[]
            {
                new ModelJsonWebKey
                {
                    KeyType = "EC",
                    KeyId = "key-1",
                    Curve = "P-256",
                    X = "f83OJ3D2xF1Bg8vub9tLe1gHMzV76e8Tus9uPHvRVEU",
                    Y = "x_FEzRu9m36HLN_tue659LNpXW6pCyStikYjKIWI5a0"
                }
            }
        };

        // Act
        var metadata = new JwtVcIssuerMetadata
        {
            Issuer = "https://issuer.example.com",
            Jwks = jwks
        };

        // Assert
        metadata.Should().NotBeNull();
        metadata.Issuer.Should().Be("https://issuer.example.com");
        metadata.Jwks.Should().Be(jwks);
        metadata.JwksUri.Should().BeNull();
    }

    [Fact]
    public void JwtVcIssuerMetadata_WithAdditionalData_ShouldStoreCorrectly()
    {
        // Act
        var metadata = new JwtVcIssuerMetadata
        {
            Issuer = "https://issuer.example.com",
            JwksUri = "https://issuer.example.com/.well-known/jwks.json",
            AdditionalData = new Dictionary<string, object>
            {
                ["credential_issuer"] = "https://issuer.example.com/credential",
                ["supported_algorithms"] = new[] { "ES256", "RS256" }
            }
        };

        // Assert
        metadata.AdditionalData.Should().ContainKey("credential_issuer");
        metadata.AdditionalData.Should().ContainKey("supported_algorithms");
    }

    [Fact]
    public void JwtVcIssuerMetadata_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var metadata = new JwtVcIssuerMetadata();

        // Assert
        metadata.Should().NotBeNull();
        metadata.Issuer.Should().BeNull();
        metadata.JwksUri.Should().BeNull();
        metadata.Jwks.Should().BeNull();
        metadata.AdditionalData.Should().BeNull();
    }

    [Fact]
    public void SdJwtVcPayload_WithVctIntegrity_ShouldStoreCorrectly()
    {
        // Arrange
        var vctIntegrity = "sha256-abc123def456";
        var payload = new SdJwtVcPayload
        {
            Vct = "ExampleCredential",
            VctIntegrity = vctIntegrity,
            Issuer = "https://issuer.example.com",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        // Act & Assert
        payload.VctIntegrity.Should().Be(vctIntegrity);
        payload.Vct.Should().Be("ExampleCredential");
    }

    [Fact]
    public void SdJwtVcPayload_WithNotBeforeTime_ShouldSetCorrectly()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var notBefore = now + 3600; // 1 hour in the future
        var expiresAt = notBefore + 86400; // 24 hours after notBefore

        var payload = new SdJwtVcPayload
        {
            Vct = "ExampleCredential",
            Issuer = "https://issuer.example.com",
            IssuedAt = now,
            NotBefore = notBefore,
            ExpiresAt = expiresAt
        };

        // Act & Assert
        payload.NotBefore.Should().Be(notBefore);
        payload.NotBefore.Value.Should().BeGreaterThan(payload.IssuedAt!.Value);
        payload.ExpiresAt.Value.Should().BeGreaterThan(payload.NotBefore.Value);
    }

    [Fact]
    public void SdJwtVcPayload_WithComplexNestedData_ShouldSerializeCorrectly()
    {
        // Arrange
        var payload = new SdJwtVcPayload
        {
            Vct = "UniversityDegreeCredential",
            Issuer = "https://university.example.edu",
            Subject = "did:example:student123",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["credentialSubject"] = new
                {
                    id = "did:example:student123",
                    degree = new
                    {
                        type = "BachelorDegree",
                        name = "Bachelor of Science and Arts",
                        degreeSchool = new
                        {
                            name = "Example University",
                            location = "Example City"
                        }
                    },
                    graduationDate = "2023-05-15"
                },
                ["evidence"] = new[]
                {
                    new { type = "transcript", source = "registrar" },
                    new { type = "diploma", source = "graduation_ceremony" }
                }
            }
        };

        // Act & Assert
        payload.Should().NotBeNull();
        payload.Vct.Should().Be("UniversityDegreeCredential");
        payload.AdditionalData.Should().ContainKey("credentialSubject");
        payload.AdditionalData.Should().ContainKey("evidence");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SdJwtVcPayload_WithInvalidVct_ShouldHandleGracefully(string? invalidVct)
    {
        // Arrange & Act
        var payload = new SdJwtVcPayload
        {
            Vct = invalidVct,
            Issuer = "https://issuer.example.com",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        // Assert
        payload.Vct.Should().Be(invalidVct);
        payload.Issuer.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SdJwtVcPayload_WithAllOptionalFields_ShouldCreateSuccessfully()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var jwk = new MsftJsonWebKey { Kty = "EC", Crv = "P-256" };
        var confirmation = new ConfirmationMethod { Jwk = jwk };

        var payload = new SdJwtVcPayload
        {
            Vct = "TestCredential",
            VctIntegrity = "sha256-abc123",
            Issuer = "https://issuer.example.com",
            Subject = "did:example:subject",
            IssuedAt = now,
            NotBefore = now,
            ExpiresAt = now + 86400,
            Confirmation = confirmation,
            Status = new { active = true },
            AdditionalData = new Dictionary<string, object>
            {
                ["custom_field"] = "custom_value"
            }
        };

        // Act & Assert
        payload.Should().NotBeNull();
        payload.Vct.Should().Be("TestCredential");
        payload.VctIntegrity.Should().Be("sha256-abc123");
        payload.Issuer.Should().Be("https://issuer.example.com");
        payload.Subject.Should().Be("did:example:subject");
        payload.IssuedAt.Should().Be(now);
        payload.NotBefore.Should().Be(now);
        payload.ExpiresAt.Should().Be(now + 86400);
        payload.Confirmation.Should().Be(confirmation);
        payload.Status.Should().NotBeNull();
        payload.AdditionalData.Should().ContainKey("custom_field");
    }

    [Fact]
    public void SdJwtVcPayload_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var payload = new SdJwtVcPayload();

        // Assert
        payload.Should().NotBeNull();
        payload.Vct.Should().BeNull();
        payload.VctIntegrity.Should().BeNull();
        payload.Issuer.Should().BeNull();
        payload.Subject.Should().BeNull();
        payload.IssuedAt.Should().BeNull();
        payload.NotBefore.Should().BeNull();
        payload.ExpiresAt.Should().BeNull();
        payload.Confirmation.Should().BeNull();
        payload.Status.Should().BeNull();
        payload.AdditionalData.Should().BeNull();
    }

    [Fact]
    public void SdJwtVcPayload_WithTimeValidation_ShouldHandleEdgeCases()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var past = now - 86400; // 1 day ago
        var future = now + 86400; // 1 day in the future

        var payload = new SdJwtVcPayload
        {
            Vct = "TestCredential",
            Issuer = "https://issuer.example.com",
            IssuedAt = past,
            NotBefore = now,
            ExpiresAt = future
        };

        // Assert
        payload.IssuedAt.Value.Should().BeLessThan(payload.NotBefore.Value);
        payload.NotBefore.Value.Should().BeLessThan(payload.ExpiresAt.Value);
    }

    [Fact]
    public void KeyBindingJwtPayload_WithMinimalData_ShouldWork()
    {
        // Act
        var payload = new KeyBindingJwtPayload
        {
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        // Assert
        payload.Audience.Should().BeNull();
        payload.Nonce.Should().BeNull();
        payload.SdHash.Should().BeNull();
        payload.IssuedAt.Should().BeGreaterThan(0);
    }

    [Fact]
    public void KeyBindingJwtPayload_WithAllFields_ShouldWork()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var payload = new KeyBindingJwtPayload
        {
            Audience = "https://verifier.example.com",
            Nonce = "challenge-nonce-xyz",
            IssuedAt = now,
            SdHash = "sha256-hash-of-sd-jwt",
            AdditionalData = new Dictionary<string, object>
            {
                ["exp"] = now + 300, // 5 minutes
                ["custom"] = "value"
            }
        };

        // Assert
        payload.Audience.Should().Be("https://verifier.example.com");
        payload.Nonce.Should().Be("challenge-nonce-xyz");
        payload.IssuedAt.Should().Be(now);
        payload.SdHash.Should().Be("sha256-hash-of-sd-jwt");
        payload.AdditionalData.Should().HaveCount(2);
        payload.AdditionalData["custom"].Should().Be("value");
    }

    [Fact]
    public void JwtVcIssuerMetadata_WithBothJwksAndUri_ShouldAllowBothForTesting()
    {
        // Arrange
        var jwks = new JwkSet { Keys = System.Array.Empty<ModelJsonWebKey>() };

        // Act
        var metadata = new JwtVcIssuerMetadata
        {
            Issuer = "https://issuer.example.com",
            JwksUri = "https://issuer.example.com/.well-known/jwks.json",
            Jwks = jwks
        };

        // Assert
        metadata.JwksUri.Should().NotBeNull();
        metadata.Jwks.Should().NotBeNull();
    }

    [Fact]
    public void JwkSet_WithEmptyKeys_ShouldWork()
    {
        // Act
        var jwks = new JwkSet
        {
            Keys = System.Array.Empty<ModelJsonWebKey>()
        };

        // Assert
        jwks.Should().NotBeNull();
        jwks.Keys.Should().NotBeNull();
        jwks.Keys.Should().BeEmpty();
    }

    [Fact]
    public void JsonWebKey_WithCompleteData_ShouldWork()
    {
        // Act
        var jwk = new ModelJsonWebKey
        {
            KeyType = "EC",
            KeyId = "key-1",
            Curve = "P-256",
            X = "x-coordinate",
            Y = "y-coordinate",
            Use = "sig",
            Algorithm = "ES256"
        };

        // Assert
        jwk.Should().NotBeNull();
        jwk.KeyType.Should().Be("EC");
        jwk.KeyId.Should().Be("key-1");
        jwk.Curve.Should().Be("P-256");
        jwk.X.Should().Be("x-coordinate");
        jwk.Y.Should().Be("y-coordinate");
        jwk.Use.Should().Be("sig");
        jwk.Algorithm.Should().Be("ES256");
    }

    [Fact]
    public void JsonWebKey_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var jwk = new ModelJsonWebKey();

        // Assert
        jwk.Should().NotBeNull();
        jwk.KeyType.Should().BeNull();
        jwk.KeyId.Should().BeNull();
        jwk.Curve.Should().BeNull();
        jwk.X.Should().BeNull();
        jwk.Y.Should().BeNull();
        jwk.Use.Should().BeNull();
        jwk.Algorithm.Should().BeNull();
    }
}
