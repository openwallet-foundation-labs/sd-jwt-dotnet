using SdJwt.Net.OidFederation.Models;

namespace SdJwt.Net.OidFederation.Tests;

/// <summary>
/// Tests for MetadataPolicyRules, MetadataPolicy, and EntityConstraints classes.
/// </summary>
public class MetadataPolicyConstraintsTests
{
    #region MetadataPolicyRules Tests

    /// <summary>
    /// Tests MetadataPolicyRules default initialization.
    /// </summary>
    [Fact]
    public void MetadataPolicyRules_DefaultInitialization_ShouldHaveNullFieldPolicies()
    {
        // Act
        var rules = new MetadataPolicyRules();

        // Assert
        rules.FieldPolicies.Should().BeNull();
    }

    /// <summary>
    /// Tests MetadataPolicyRules.Validate passes with null field policies.
    /// </summary>
    [Fact]
    public void MetadataPolicyRules_Validate_ShouldPassWithNullFieldPolicies()
    {
        // Arrange
        var rules = new MetadataPolicyRules();

        // Act & Assert
        var act = () => rules.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests MetadataPolicyRules.Validate passes with valid field policies.
    /// </summary>
    [Fact]
    public void MetadataPolicyRules_Validate_ShouldPassWithValidFieldPolicies()
    {
        // Arrange
        var rules = new MetadataPolicyRules
        {
            FieldPolicies = new Dictionary<string, object>
            {
                { "response_types", PolicyOperators.CreateSubsetOf("code", "token") },
                { "grant_types", PolicyOperators.CreateOneOf("authorization_code") }
            }
        };

        // Act & Assert
        var act = () => rules.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests MetadataPolicyRules.Validate throws with empty field name.
    /// </summary>
    [Fact]
    public void MetadataPolicyRules_Validate_ShouldThrowWithEmptyFieldName()
    {
        // Arrange
        var rules = new MetadataPolicyRules
        {
            FieldPolicies = new Dictionary<string, object>
            {
                { "", new object() }
            }
        };

        // Act & Assert
        var act = () => rules.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Field name cannot be null or empty");
    }

    /// <summary>
    /// Tests MetadataPolicyRules.Validate throws with whitespace field name.
    /// </summary>
    [Fact]
    public void MetadataPolicyRules_Validate_ShouldThrowWithWhitespaceFieldName()
    {
        // Arrange
        var rules = new MetadataPolicyRules
        {
            FieldPolicies = new Dictionary<string, object>
            {
                { "   ", new object() }
            }
        };

        // Act & Assert
        var act = () => rules.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Field name cannot be null or empty");
    }

    /// <summary>
    /// Tests GetFieldPolicy returns null when field is not found.
    /// </summary>
    [Fact]
    public void MetadataPolicyRules_GetFieldPolicy_ShouldReturnNullWhenNotFound()
    {
        // Arrange
        var rules = new MetadataPolicyRules();

        // Act
        var result = rules.GetFieldPolicy("non_existent_field");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests GetFieldPolicy returns correct policy when field exists.
    /// </summary>
    [Fact]
    public void MetadataPolicyRules_GetFieldPolicy_ShouldReturnPolicyWhenFound()
    {
        // Arrange
        var expectedPolicy = PolicyOperators.CreateValue("expected");
        var rules = new MetadataPolicyRules
        {
            FieldPolicies = new Dictionary<string, object>
            {
                { "test_field", expectedPolicy }
            }
        };

        // Act
        var result = rules.GetFieldPolicy("test_field");

        // Assert
        result.Should().BeEquivalentTo(expectedPolicy);
    }

    /// <summary>
    /// Tests SetFieldPolicy creates dictionary and sets policy.
    /// </summary>
    [Fact]
    public void MetadataPolicyRules_SetFieldPolicy_ShouldCreateDictionaryAndSetPolicy()
    {
        // Arrange
        var rules = new MetadataPolicyRules();
        var policy = PolicyOperators.CreateEssential();

        // Act
        rules.SetFieldPolicy("new_field", policy);

        // Assert
        rules.FieldPolicies.Should().NotBeNull();
        rules.GetFieldPolicy("new_field").Should().BeEquivalentTo(policy);
    }

    /// <summary>
    /// Tests SetFieldPolicy overwrites existing policy.
    /// </summary>
    [Fact]
    public void MetadataPolicyRules_SetFieldPolicy_ShouldOverwriteExistingPolicy()
    {
        // Arrange
        var rules = new MetadataPolicyRules
        {
            FieldPolicies = new Dictionary<string, object>
            {
                { "field", PolicyOperators.CreateValue("old") }
            }
        };
        var newPolicy = PolicyOperators.CreateValue("new");

        // Act
        rules.SetFieldPolicy("field", newPolicy);

        // Assert
        rules.GetFieldPolicy("field").Should().BeEquivalentTo(newPolicy);
    }

    #endregion

    #region MetadataPolicy Tests

    /// <summary>
    /// Tests MetadataPolicy default initialization.
    /// </summary>
    [Fact]
    public void MetadataPolicy_DefaultInitialization_ShouldHaveNullProperties()
    {
        // Act
        var policy = new MetadataPolicy();

        // Assert
        policy.FederationEntity.Should().BeNull();
        policy.OpenIdRelyingParty.Should().BeNull();
        policy.OpenIdProvider.Should().BeNull();
        policy.OAuthAuthorizationServer.Should().BeNull();
        policy.OpenIdCredentialIssuer.Should().BeNull();
        policy.OpenIdRelyingPartyVerifier.Should().BeNull();
        policy.AdditionalPolicies.Should().BeNull();
    }

    /// <summary>
    /// Tests MetadataPolicy.Validate passes with null policies.
    /// </summary>
    [Fact]
    public void MetadataPolicy_Validate_ShouldPassWithNullPolicies()
    {
        // Arrange
        var policy = new MetadataPolicy();

        // Act & Assert
        var act = () => policy.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests MetadataPolicy.Validate validates all protocol policies.
    /// </summary>
    [Fact]
    public void MetadataPolicy_Validate_ShouldValidateAllProtocolPolicies()
    {
        // Arrange
        var policy = new MetadataPolicy
        {
            FederationEntity = new MetadataPolicyRules
            {
                FieldPolicies = new Dictionary<string, object> { { "", new object() } }
            }
        };

        // Act & Assert
        var act = () => policy.Validate();
        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Tests MetadataPolicy.Validate validates additional policies.
    /// </summary>
    [Fact]
    public void MetadataPolicy_Validate_ShouldValidateAdditionalPolicies()
    {
        // Arrange
        var policy = new MetadataPolicy
        {
            AdditionalPolicies = new Dictionary<string, MetadataPolicyRules>
            {
                {
                    "custom_protocol",
                    new MetadataPolicyRules
                    {
                        FieldPolicies = new Dictionary<string, object> { { "  ", new object() } }
                    }
                }
            }
        };

        // Act & Assert
        var act = () => policy.Validate();
        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Tests GetPolicyRules for federation_entity protocol.
    /// </summary>
    [Fact]
    public void MetadataPolicy_GetPolicyRules_ShouldReturnFederationEntityRules()
    {
        // Arrange
        var expectedRules = new MetadataPolicyRules();
        var policy = new MetadataPolicy { FederationEntity = expectedRules };

        // Act
        var result = policy.GetPolicyRules("federation_entity");

        // Assert
        result.Should().BeSameAs(expectedRules);
    }

    /// <summary>
    /// Tests GetPolicyRules for openid_relying_party protocol.
    /// </summary>
    [Fact]
    public void MetadataPolicy_GetPolicyRules_ShouldReturnOpenIdRelyingPartyRules()
    {
        // Arrange
        var expectedRules = new MetadataPolicyRules();
        var policy = new MetadataPolicy { OpenIdRelyingParty = expectedRules };

        // Act
        var result = policy.GetPolicyRules("openid_relying_party");

        // Assert
        result.Should().BeSameAs(expectedRules);
    }

    /// <summary>
    /// Tests GetPolicyRules for openid_provider protocol.
    /// </summary>
    [Fact]
    public void MetadataPolicy_GetPolicyRules_ShouldReturnOpenIdProviderRules()
    {
        // Arrange
        var expectedRules = new MetadataPolicyRules();
        var policy = new MetadataPolicy { OpenIdProvider = expectedRules };

        // Act
        var result = policy.GetPolicyRules("openid_provider");

        // Assert
        result.Should().BeSameAs(expectedRules);
    }

    /// <summary>
    /// Tests GetPolicyRules for oauth_authorization_server protocol.
    /// </summary>
    [Fact]
    public void MetadataPolicy_GetPolicyRules_ShouldReturnOAuthAuthorizationServerRules()
    {
        // Arrange
        var expectedRules = new MetadataPolicyRules();
        var policy = new MetadataPolicy { OAuthAuthorizationServer = expectedRules };

        // Act
        var result = policy.GetPolicyRules("oauth_authorization_server");

        // Assert
        result.Should().BeSameAs(expectedRules);
    }

    /// <summary>
    /// Tests GetPolicyRules for openid_credential_issuer protocol.
    /// </summary>
    [Fact]
    public void MetadataPolicy_GetPolicyRules_ShouldReturnOpenIdCredentialIssuerRules()
    {
        // Arrange
        var expectedRules = new MetadataPolicyRules();
        var policy = new MetadataPolicy { OpenIdCredentialIssuer = expectedRules };

        // Act
        var result = policy.GetPolicyRules("openid_credential_issuer");

        // Assert
        result.Should().BeSameAs(expectedRules);
    }

    /// <summary>
    /// Tests GetPolicyRules for openid_relying_party_verifier protocol.
    /// </summary>
    [Fact]
    public void MetadataPolicy_GetPolicyRules_ShouldReturnOpenIdRelyingPartyVerifierRules()
    {
        // Arrange
        var expectedRules = new MetadataPolicyRules();
        var policy = new MetadataPolicy { OpenIdRelyingPartyVerifier = expectedRules };

        // Act
        var result = policy.GetPolicyRules("openid_relying_party_verifier");

        // Assert
        result.Should().BeSameAs(expectedRules);
    }

    /// <summary>
    /// Tests GetPolicyRules for additional protocol.
    /// </summary>
    [Fact]
    public void MetadataPolicy_GetPolicyRules_ShouldReturnAdditionalPolicyRules()
    {
        // Arrange
        var expectedRules = new MetadataPolicyRules();
        var policy = new MetadataPolicy
        {
            AdditionalPolicies = new Dictionary<string, MetadataPolicyRules>
            {
                { "custom_protocol", expectedRules }
            }
        };

        // Act
        var result = policy.GetPolicyRules("custom_protocol");

        // Assert
        result.Should().BeSameAs(expectedRules);
    }

    /// <summary>
    /// Tests GetPolicyRules returns null for unknown protocol.
    /// </summary>
    [Fact]
    public void MetadataPolicy_GetPolicyRules_ShouldReturnNullForUnknownProtocol()
    {
        // Arrange
        var policy = new MetadataPolicy();

        // Act
        var result = policy.GetPolicyRules("unknown_protocol");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region EntityConstraints Tests

    /// <summary>
    /// Tests EntityConstraints default initialization.
    /// </summary>
    [Fact]
    public void EntityConstraints_DefaultInitialization_ShouldHaveNullProperties()
    {
        // Act
        var constraints = new EntityConstraints();

        // Assert
        constraints.MaxPathLength.Should().BeNull();
        constraints.NamingConstraints.Should().BeNull();
        constraints.AdditionalConstraints.Should().BeNull();
    }

    /// <summary>
    /// Tests EntityConstraints.Validate passes with null properties.
    /// </summary>
    [Fact]
    public void EntityConstraints_Validate_ShouldPassWithNullProperties()
    {
        // Arrange
        var constraints = new EntityConstraints();

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests EntityConstraints.Validate passes with valid MaxPathLength.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void EntityConstraints_Validate_ShouldPassWithValidMaxPathLength(int maxPathLength)
    {
        // Arrange
        var constraints = new EntityConstraints { MaxPathLength = maxPathLength };

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests EntityConstraints.Validate throws with negative MaxPathLength.
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void EntityConstraints_Validate_ShouldThrowWithNegativeMaxPathLength(int maxPathLength)
    {
        // Arrange
        var constraints = new EntityConstraints { MaxPathLength = maxPathLength };

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("MaxPathLength must be non-negative");
    }

    /// <summary>
    /// Tests EntityConstraints.Validate validates NamingConstraints.
    /// </summary>
    [Fact]
    public void EntityConstraints_Validate_ShouldValidateNamingConstraints()
    {
        // Arrange
        var constraints = new EntityConstraints
        {
            NamingConstraints = new NamingConstraints
            {
                Permitted = new[] { "" }
            }
        };

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Tests EntityConstraints with additional constraints property.
    /// </summary>
    [Fact]
    public void EntityConstraints_AdditionalConstraints_ShouldBeSettable()
    {
        // Arrange
        var constraints = new EntityConstraints
        {
            AdditionalConstraints = new Dictionary<string, object>
            {
                { "custom_constraint", "custom_value" }
            }
        };

        // Assert
        constraints.AdditionalConstraints.Should().ContainKey("custom_constraint");
        constraints.AdditionalConstraints["custom_constraint"].Should().Be("custom_value");
    }

    #endregion
}
