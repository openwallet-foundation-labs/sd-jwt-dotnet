using FluentAssertions;
using SdJwt.Net.OidFederation.Models;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.OidFederation.Tests.Models;

public class EntityMetadataEnhancedTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var metadata = new EntityMetadata();

        // Assert
        metadata.Should().NotBeNull();
        metadata.OpenIdCredentialIssuer.Should().BeNull();
        metadata.OpenIdRelyingParty.Should().BeNull();
        metadata.OpenIdProvider.Should().BeNull();
        metadata.FederationEntity.Should().BeNull();
        metadata.OAuthAuthorizationServer.Should().BeNull();
        metadata.OAuthResource.Should().BeNull();
        metadata.OpenIdRelyingPartyVerifier.Should().BeNull();
    }

    [Fact]
    public void OpenIdCredentialIssuer_WithValidData_ShouldSerializeCorrectly()
    {
        // Arrange
        var metadata = new EntityMetadata
        {
            OpenIdCredentialIssuer = new
            {
                credential_issuer = "https://issuer.example.com",
                credential_endpoint = "https://issuer.example.com/credential",
                batch_credential_endpoint = "https://issuer.example.com/batch_credential"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(metadata, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<EntityMetadata>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        json.Should().Contain("openid_credential_issuer");
        deserialized.Should().NotBeNull();
        deserialized!.OpenIdCredentialIssuer.Should().NotBeNull();
    }

    [Fact]
    public void OpenIdRelyingParty_WithValidData_ShouldSerializeCorrectly()
    {
        // Arrange
        var metadata = new EntityMetadata
        {
            OpenIdRelyingParty = new
            {
                client_id = "rp-client-123",
                redirect_uris = new[] { "https://rp.example.com/callback" },
                response_types = new[] { "code" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(metadata, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<EntityMetadata>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        json.Should().Contain("openid_relying_party");
        deserialized.Should().NotBeNull();
        deserialized!.OpenIdRelyingParty.Should().NotBeNull();
    }

    [Fact]
    public void FederationEntity_WithValidData_ShouldSerializeCorrectly()
    {
        // Arrange
        var metadata = new EntityMetadata
        {
            FederationEntity = new FederationEntityMetadata
            {
                FederationFetchEndpoint = "https://federation.example.com/fetch",
                FederationListEndpoint = "https://federation.example.com/list",
                FederationTrustMarkStatusEndpoint = "https://federation.example.com/trust_mark_status"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(metadata, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<EntityMetadata>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        json.Should().Contain("federation_entity");
        deserialized.Should().NotBeNull();
        deserialized!.FederationEntity.Should().NotBeNull();
    }

    [Fact]
    public void OpenIdProvider_WithValidData_ShouldSerializeCorrectly()
    {
        // Arrange
        var metadata = new EntityMetadata
        {
            OpenIdProvider = new
            {
                issuer = "https://op.example.com",
                authorization_endpoint = "https://op.example.com/auth",
                token_endpoint = "https://op.example.com/token",
                jwks_uri = "https://op.example.com/jwks"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(metadata, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<EntityMetadata>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        json.Should().Contain("openid_provider");
        deserialized.Should().NotBeNull();
        deserialized!.OpenIdProvider.Should().NotBeNull();
    }

    [Fact]
    public void OAuthAuthorizationServer_WithValidData_ShouldSerializeCorrectly()
    {
        // Arrange
        var metadata = new EntityMetadata
        {
            OAuthAuthorizationServer = new
            {
                issuer = "https://as.example.com",
                authorization_endpoint = "https://as.example.com/auth",
                token_endpoint = "https://as.example.com/token"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(metadata, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<EntityMetadata>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        json.Should().Contain("oauth_authorization_server");
        deserialized.Should().NotBeNull();
        deserialized!.OAuthAuthorizationServer.Should().NotBeNull();
    }

    [Fact]
    public void Equality_WithSameContent_ShouldBeEqual()
    {
        // Arrange
        var metadata1 = new EntityMetadata
        {
            OpenIdCredentialIssuer = new { credential_issuer = "https://issuer.example.com" }
        };

        var metadata2 = new EntityMetadata
        {
            OpenIdCredentialIssuer = new { credential_issuer = "https://issuer.example.com" }
        };

        // Act & Assert
        metadata1.Should().BeEquivalentTo(metadata2);
    }

    [Fact]
    public void ComplexMetadata_WithAllFields_ShouldSerializeCorrectly()
    {
        // Arrange
        var metadata = new EntityMetadata
        {
            OpenIdCredentialIssuer = new
            {
                credential_issuer = "https://issuer.example.com",
                credential_endpoint = "https://issuer.example.com/credential"
            },
            OpenIdRelyingParty = new
            {
                client_id = "rp-123",
                redirect_uris = new[] { "https://rp.example.com/callback" }
            },
            FederationEntity = new FederationEntityMetadata
            {
                FederationFetchEndpoint = "https://federation.example.com/fetch"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(metadata, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<EntityMetadata>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        json.Should().Contain("openid_credential_issuer");
        json.Should().Contain("openid_relying_party");
        json.Should().Contain("federation_entity");

        deserialized.Should().NotBeNull();
        deserialized!.OpenIdCredentialIssuer.Should().NotBeNull();
        deserialized.OpenIdRelyingParty.Should().NotBeNull();
        deserialized.FederationEntity.Should().NotBeNull();
    }

    [Fact]
    public void SupportsProtocol_WithValidProtocols_ShouldReturnCorrectResults()
    {
        // Arrange
        var metadata = new EntityMetadata
        {
            OpenIdCredentialIssuer = new { credential_issuer = "https://issuer.example.com" },
            OAuthAuthorizationServer = new { issuer = "https://as.example.com" }
        };

        // Act & Assert
        metadata.SupportsProtocol("openid_credential_issuer").Should().BeTrue();
        metadata.SupportsProtocol("oauth_authorization_server").Should().BeTrue();
        metadata.SupportsProtocol("openid_provider").Should().BeFalse();
        metadata.SupportsProtocol("unknown_protocol").Should().BeFalse();
    }

    [Fact]
    public void GetProtocolMetadata_WithValidProtocols_ShouldReturnMetadata()
    {
        // Arrange
        var credentialIssuerMetadata = new { credential_issuer = "https://issuer.example.com" };
        var metadata = new EntityMetadata
        {
            OpenIdCredentialIssuer = credentialIssuerMetadata
        };

        // Act
        var result = metadata.GetProtocolMetadata("openid_credential_issuer");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(credentialIssuerMetadata);
    }
}

public class EntityConstraintsEnhancedTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var constraints = new EntityConstraints();

        // Assert
        constraints.Should().NotBeNull();
        constraints.MaxPathLength.Should().BeNull();
        constraints.NamingConstraints.Should().BeNull();
    }

    [Fact]
    public void MaxPathLength_WithValidValue_ShouldSerializeCorrectly()
    {
        // Arrange
        var constraints = new EntityConstraints
        {
            MaxPathLength = 5
        };

        // Act
        var json = JsonSerializer.Serialize(constraints, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<EntityConstraints>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        json.Should().Contain("max_path_length");
        json.Should().Contain("5");
        deserialized.Should().NotBeNull();
        deserialized!.MaxPathLength.Should().Be(5);
    }

    [Fact]
    public void NamingConstraints_WithValidData_ShouldSerializeCorrectly()
    {
        // Arrange
        var constraints = new EntityConstraints
        {
            NamingConstraints = new NamingConstraints
            {
                Permitted = new[] { ".example.com", ".test.org" },
                Excluded = new[] { ".blocked.com" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(constraints, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<EntityConstraints>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        json.Should().Contain("naming_constraints");
        deserialized.Should().NotBeNull();
        deserialized!.NamingConstraints.Should().NotBeNull();
        deserialized.NamingConstraints!.Permitted.Should().Contain(".example.com");
        deserialized.NamingConstraints.Excluded.Should().Contain(".blocked.com");
    }

    [Fact]
    public void ComplexConstraints_WithAllFields_ShouldSerializeCorrectly()
    {
        // Arrange
        var constraints = new EntityConstraints
        {
            MaxPathLength = 3,
            NamingConstraints = new NamingConstraints
            {
                Permitted = new[] { ".trusted.com" },
                Excluded = new[] { ".untrusted.com" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(constraints, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<EntityConstraints>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        json.Should().Contain("max_path_length");
        json.Should().Contain("naming_constraints");

        deserialized.Should().NotBeNull();
        deserialized!.MaxPathLength.Should().Be(3);
        deserialized.NamingConstraints.Should().NotBeNull();
    }

    [Fact]
    public void Equality_WithSameContent_ShouldBeEqual()
    {
        // Arrange
        var constraints1 = new EntityConstraints
        {
            MaxPathLength = 5,
            NamingConstraints = new NamingConstraints
            {
                Permitted = new[] { ".example.com" }
            }
        };

        var constraints2 = new EntityConstraints
        {
            MaxPathLength = 5,
            NamingConstraints = new NamingConstraints
            {
                Permitted = new[] { ".example.com" }
            }
        };

        // Act & Assert
        constraints1.Should().BeEquivalentTo(constraints2);
    }
}

public class FederationEntityMetadataEnhancedTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var metadata = new FederationEntityMetadata();

        // Assert
        metadata.Should().NotBeNull();
        metadata.FederationFetchEndpoint.Should().BeNull();
        metadata.FederationListEndpoint.Should().BeNull();
        metadata.FederationResolveEndpoint.Should().BeNull();
        metadata.FederationTrustMarkStatusEndpoint.Should().BeNull();
        metadata.Name.Should().BeNull();
        metadata.Contacts.Should().BeNull();
    }

    [Fact]
    public void Validate_WithValidEndpoints_ShouldNotThrow()
    {
        // Arrange
        var metadata = new FederationEntityMetadata
        {
            FederationFetchEndpoint = "https://federation.example.com/fetch",
            FederationListEndpoint = "https://federation.example.com/list"
        };

        // Act & Assert
        var act = () => metadata.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithInvalidEndpoint_ShouldThrow()
    {
        // Arrange
        var metadata = new FederationEntityMetadata
        {
            FederationFetchEndpoint = "not-a-url"
        };

        // Act & Assert
        var act = () => metadata.Validate();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Validate_WithHttpEndpoint_ShouldThrow()
    {
        // Arrange
        var metadata = new FederationEntityMetadata
        {
            FederationFetchEndpoint = "http://insecure.example.com/fetch"
        };

        // Act & Assert
        var act = () => metadata.Validate();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Serialization_ShouldWorkCorrectly()
    {
        // Arrange
        var metadata = new FederationEntityMetadata
        {
            FederationFetchEndpoint = "https://federation.example.com/fetch",
            Name = "Test Federation",
            Contacts = new[] { "admin@federation.example.com" }
        };

        // Act
        var json = JsonSerializer.Serialize(metadata, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<FederationEntityMetadata>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        json.Should().Contain("federation_fetch_endpoint");
        json.Should().Contain("name");
        json.Should().Contain("contacts");

        deserialized.Should().NotBeNull();
        deserialized!.FederationFetchEndpoint.Should().Be("https://federation.example.com/fetch");
        deserialized.Name.Should().Be("Test Federation");
        deserialized.Contacts.Should().Contain("admin@federation.example.com");
    }
}

public class NamingConstraintsEnhancedTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var constraints = new NamingConstraints();

        // Assert
        constraints.Should().NotBeNull();
        constraints.Permitted.Should().BeNull();
        constraints.Excluded.Should().BeNull();
    }

    [Fact]
    public void Permitted_WithValidDomains_ShouldSerializeCorrectly()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Permitted = new[] { ".example.com", ".test.org", ".gov.us" }
        };

        // Act
        var json = JsonSerializer.Serialize(constraints, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<NamingConstraints>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        json.Should().Contain("permitted");
        deserialized.Should().NotBeNull();
        deserialized!.Permitted.Should().HaveCount(3);
        deserialized.Permitted.Should().Contain(".example.com");
        deserialized.Permitted.Should().Contain(".test.org");
        deserialized.Permitted.Should().Contain(".gov.us");
    }

    [Fact]
    public void Excluded_WithValidDomains_ShouldSerializeCorrectly()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Excluded = new[] { ".malicious.com", ".spam.net" }
        };

        // Act
        var json = JsonSerializer.Serialize(constraints, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<NamingConstraints>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        json.Should().Contain("excluded");
        deserialized.Should().NotBeNull();
        deserialized!.Excluded.Should().HaveCount(2);
        deserialized.Excluded.Should().Contain(".malicious.com");
        deserialized.Excluded.Should().Contain(".spam.net");
    }

    [Fact]
    public void CompleteConstraints_WithBothPermittedAndExcluded_ShouldSerializeCorrectly()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Permitted = new[] { ".trusted.com", ".secure.org" },
            Excluded = new[] { ".untrusted.com", ".insecure.net" }
        };

        // Act
        var json = JsonSerializer.Serialize(constraints, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<NamingConstraints>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        json.Should().Contain("permitted");
        json.Should().Contain("excluded");

        deserialized.Should().NotBeNull();
        deserialized!.Permitted.Should().HaveCount(2);
        deserialized.Excluded.Should().HaveCount(2);
    }

    [Fact]
    public void EmptyArrays_ShouldSerializeCorrectly()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Permitted = Array.Empty<string>(),
            Excluded = Array.Empty<string>()
        };

        // Act
        var json = JsonSerializer.Serialize(constraints, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<NamingConstraints>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Permitted.Should().BeEmpty();
        deserialized.Excluded.Should().BeEmpty();
    }
}
