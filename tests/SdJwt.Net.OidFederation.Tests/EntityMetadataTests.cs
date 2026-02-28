using SdJwt.Net.OidFederation.Models;

namespace SdJwt.Net.OidFederation.Tests;

/// <summary>
/// Tests for CommonTrustMarks, FederationEntityMetadata, and EntityMetadata classes.
/// </summary>
public class EntityMetadataTests
{
    #region CommonTrustMarks Tests

    /// <summary>
    /// Tests CommonTrustMarks constants have expected values.
    /// </summary>
    [Fact]
    public void CommonTrustMarks_Constants_ShouldHaveExpectedValues()
    {
        // Assert
        CommonTrustMarks.EidasCompliant.Should().Be("https://eidas.europa.eu/trustmark/compliant");
        CommonTrustMarks.SecurityCertified.Should().Be("https://security.gov/trustmark/certified");
        CommonTrustMarks.EducationalInstitution.Should().Be("https://education.gov/trustmark/institution");
        CommonTrustMarks.FinancialInstitution.Should().Be("https://finance.gov/trustmark/institution");
        CommonTrustMarks.HealthcareProvider.Should().Be("https://healthcare.gov/trustmark/provider");
        CommonTrustMarks.GovernmentAgency.Should().Be("https://government.gov/trustmark/agency");
    }

    #endregion

    #region FederationEntityMetadata Tests

    /// <summary>
    /// Tests FederationEntityMetadata default initialization.
    /// </summary>
    [Fact]
    public void FederationEntityMetadata_DefaultInitialization_ShouldHaveNullProperties()
    {
        // Act
        var metadata = new FederationEntityMetadata();

        // Assert
        metadata.FederationFetchEndpoint.Should().BeNull();
        metadata.FederationListEndpoint.Should().BeNull();
        metadata.FederationResolveEndpoint.Should().BeNull();
        metadata.FederationTrustMarkStatusEndpoint.Should().BeNull();
        metadata.Name.Should().BeNull();
        metadata.Contacts.Should().BeNull();
    }

    /// <summary>
    /// Tests FederationEntityMetadata.Validate passes with null endpoints.
    /// </summary>
    [Fact]
    public void FederationEntityMetadata_Validate_ShouldPassWithNullEndpoints()
    {
        // Arrange
        var metadata = new FederationEntityMetadata();

        // Act & Assert
        var act = () => metadata.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests FederationEntityMetadata.Validate passes with valid HTTPS endpoints.
    /// </summary>
    [Fact]
    public void FederationEntityMetadata_Validate_ShouldPassWithValidHttpsEndpoints()
    {
        // Arrange
        var metadata = new FederationEntityMetadata
        {
            FederationFetchEndpoint = "https://example.com/fetch",
            FederationListEndpoint = "https://example.com/list",
            FederationResolveEndpoint = "https://example.com/resolve",
            FederationTrustMarkStatusEndpoint = "https://example.com/status"
        };

        // Act & Assert
        var act = () => metadata.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests FederationEntityMetadata.Validate throws with invalid HTTP endpoint.
    /// </summary>
    [Fact]
    public void FederationEntityMetadata_Validate_ShouldThrowWithHttpEndpoint()
    {
        // Arrange
        var metadata = new FederationEntityMetadata
        {
            FederationFetchEndpoint = "http://insecure.example.com/fetch"
        };

        // Act & Assert
        var act = () => metadata.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*must be a valid HTTPS URL*");
    }

    /// <summary>
    /// Tests FederationEntityMetadata.Validate throws with empty contact.
    /// </summary>
    [Fact]
    public void FederationEntityMetadata_Validate_ShouldThrowWithEmptyContact()
    {
        // Arrange
        var metadata = new FederationEntityMetadata
        {
            Contacts = new[] { "valid@example.com", "" }
        };

        // Act & Assert
        var act = () => metadata.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Contact cannot be null or empty");
    }

    #endregion

    #region EntityMetadata Tests

    /// <summary>
    /// Tests EntityMetadata.SupportsProtocol for various protocols.
    /// </summary>
    [Theory]
    [InlineData("federation_entity", true)]
    [InlineData("openid_relying_party", false)]
    [InlineData("openid_provider", false)]
    [InlineData("unknown_protocol", false)]
    public void EntityMetadata_SupportsProtocol_ShouldReturnCorrectValue(string protocol, bool expectedResult)
    {
        // Arrange
        var metadata = new EntityMetadata
        {
            FederationEntity = new FederationEntityMetadata()
        };

        // Act
        var result = metadata.SupportsProtocol(protocol);

        // Assert
        result.Should().Be(expectedResult);
    }

    /// <summary>
    /// Tests EntityMetadata.SupportsProtocol with additional metadata.
    /// </summary>
    [Fact]
    public void EntityMetadata_SupportsProtocol_ShouldCheckAdditionalMetadata()
    {
        // Arrange
        var metadata = new EntityMetadata
        {
            AdditionalMetadata = new Dictionary<string, object>
            {
                { "custom_protocol", new { name = "Custom" } }
            }
        };

        // Act
        var result = metadata.SupportsProtocol("custom_protocol");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests EntityMetadata.GetProtocolMetadata for various protocols.
    /// </summary>
    [Fact]
    public void EntityMetadata_GetProtocolMetadata_ShouldReturnCorrectMetadata()
    {
        // Arrange
        var federationEntity = new FederationEntityMetadata { Name = "Test" };
        var metadata = new EntityMetadata
        {
            FederationEntity = federationEntity
        };

        // Act
        var result = metadata.GetProtocolMetadata("federation_entity");

        // Assert
        result.Should().BeSameAs(federationEntity);
    }

    /// <summary>
    /// Tests EntityMetadata.GetProtocolMetadata returns null for unknown protocol.
    /// </summary>
    [Fact]
    public void EntityMetadata_GetProtocolMetadata_ShouldReturnNullForUnknown()
    {
        // Arrange
        var metadata = new EntityMetadata();

        // Act
        var result = metadata.GetProtocolMetadata("unknown_protocol");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests EntityMetadata.Validate validates FederationEntity.
    /// </summary>
    [Fact]
    public void EntityMetadata_Validate_ShouldValidateFederationEntity()
    {
        // Arrange
        var metadata = new EntityMetadata
        {
            FederationEntity = new FederationEntityMetadata
            {
                FederationFetchEndpoint = "http://invalid.example.com"
            }
        };

        // Act & Assert
        var act = () => metadata.Validate();
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion
}
