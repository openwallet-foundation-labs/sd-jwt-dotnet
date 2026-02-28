using SdJwt.Net.OidFederation.Models;

namespace SdJwt.Net.OidFederation.Tests;

/// <summary>
/// Tests for TrustMark, EntityStatement, and OidFederationConstants classes.
/// </summary>
public class EntityStatementConstantsTests
{
    #region TrustMark Additional Tests

    /// <summary>
    /// Tests TrustMark.IsExpired with clock skew.
    /// </summary>
    [Fact]
    public void TrustMark_IsExpired_ShouldRespectClockSkew()
    {
        // Arrange
        var trustMark = new TrustMark
        {
            Id = "test-mark",
            TrustMarkValue = "value",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeSeconds()
        };

        // Act - with large clock skew, should not be expired
        var isExpiredWithLargeSkew = trustMark.IsExpired(TimeSpan.FromMinutes(10));

        // Assert
        isExpiredWithLargeSkew.Should().BeFalse();
    }

    /// <summary>
    /// Tests TrustMark.IsExpired returns false when no expiration set.
    /// </summary>
    [Fact]
    public void TrustMark_IsExpired_ShouldReturnFalseWhenNoExpiration()
    {
        // Arrange
        var trustMark = new TrustMark
        {
            Id = "test-mark",
            TrustMarkValue = "value"
        };

        // Act
        var isExpired = trustMark.IsExpired();

        // Assert
        isExpired.Should().BeFalse();
    }

    /// <summary>
    /// Tests TrustMark.TrustMarkIssuer alias property.
    /// </summary>
    [Fact]
    public void TrustMark_TrustMarkIssuer_ShouldAliasIssuer()
    {
        // Arrange
        var trustMark = new TrustMark
        {
            Id = "test-mark",
            TrustMarkValue = "value",
            TrustMarkIssuer = "https://issuer.example.com"
        };

        // Assert
        trustMark.Issuer.Should().Be("https://issuer.example.com");
        trustMark.TrustMarkIssuer.Should().Be("https://issuer.example.com");
    }

    /// <summary>
    /// Tests TrustMark.ToString returns expected format.
    /// </summary>
    [Fact]
    public void TrustMark_ToString_ShouldReturnExpectedFormat()
    {
        // Arrange
        var trustMark = new TrustMark
        {
            Id = "test-id",
            TrustMarkValue = "value",
            Issuer = "https://issuer.example.com"
        };

        // Act
        var result = trustMark.ToString();

        // Assert
        result.Should().Contain("TrustMark");
        result.Should().Contain("test-id");
        result.Should().Contain("https://issuer.example.com");
    }

    /// <summary>
    /// Tests TrustMark.Create throws with empty id.
    /// </summary>
    [Fact]
    public void TrustMark_Create_ShouldThrowWithEmptyId()
    {
        // Act & Assert
        var act = () => TrustMark.Create("", "value");
        act.Should().Throw<ArgumentException>()
           .WithMessage("*id*");
    }

    /// <summary>
    /// Tests TrustMark.Create throws with empty value.
    /// </summary>
    [Fact]
    public void TrustMark_Create_ShouldThrowWithEmptyValue()
    {
        // Act & Assert
        var act = () => TrustMark.Create("id", "");
        act.Should().Throw<ArgumentException>()
           .WithMessage("*value*");
    }

    /// <summary>
    /// Tests TrustMark.Create throws with invalid HTTP issuer.
    /// </summary>
    [Fact]
    public void TrustMark_Create_ShouldThrowWithHttpIssuer()
    {
        // Act & Assert
        var act = () => TrustMark.Create("id", "value", "http://insecure.example.com");
        act.Should().Throw<ArgumentException>()
           .WithMessage("*HTTPS URL*");
    }

    /// <summary>
    /// Tests TrustMark.Create normalizes whitespace issuer to null.
    /// </summary>
    [Fact]
    public void TrustMark_Create_ShouldNormalizeWhitespaceIssuer()
    {
        // Act
        var trustMark = TrustMark.Create("id", "value", "   ");

        // Assert
        trustMark.Issuer.Should().BeNull();
    }

    #endregion

    #region EntityStatement Additional Tests

    /// <summary>
    /// Tests EntityStatement.Create with custom validity.
    /// </summary>
    [Fact]
    public void EntityStatement_Create_ShouldSetCorrectExpiration()
    {
        // Arrange
        var validityHours = 48;

        // Act
        var statement = EntityStatement.Create(
            "https://issuer.example.com",
            "https://subject.example.com",
            validityHours);

        // Assert
        var expectedExpiry = statement.IssuedAt + (validityHours * 3600);
        statement.ExpiresAt.Should().Be(expectedExpiry);
    }

    /// <summary>
    /// Tests EntityStatement.Validate throws with same issuer and subject.
    /// </summary>
    [Fact]
    public void EntityStatement_Validate_ShouldThrowWithSameIssuerAndSubject()
    {
        // Arrange
        var statement = new EntityStatement
        {
            Issuer = "https://same.example.com",
            Subject = "https://same.example.com",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds()
        };

        // Act & Assert
        var act = () => statement.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Issuer and Subject must be different*");
    }

    /// <summary>
    /// Tests EntityStatement.Validate validates source endpoint.
    /// </summary>
    [Fact]
    public void EntityStatement_Validate_ShouldValidateSourceEndpoint()
    {
        // Arrange
        var statement = EntityStatement.Create("https://issuer.example.com", "https://subject.example.com");
        statement.SourceEndpoint = "http://insecure.example.com";

        // Act & Assert
        var act = () => statement.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*SourceEndpoint must be a valid HTTPS URL*");
    }

    #endregion

    #region OidFederationConstants Tests

    /// <summary>
    /// Tests OidFederationConstants.WellKnownEndpoints values.
    /// </summary>
    [Fact]
    public void OidFederationConstants_WellKnownEndpoints_ShouldHaveCorrectValues()
    {
        OidFederationConstants.WellKnownEndpoints.EntityConfiguration.Should().Be("/.well-known/openid-federation");
    }

    /// <summary>
    /// Tests OidFederationConstants.JwtHeaders values.
    /// </summary>
    [Fact]
    public void OidFederationConstants_JwtHeaders_ShouldHaveCorrectValues()
    {
        OidFederationConstants.JwtHeaders.EntityConfigurationType.Should().Be("entity-configuration+jwt");
        OidFederationConstants.JwtHeaders.EntityStatementType.Should().Be("entity-statement+jwt");
        OidFederationConstants.JwtHeaders.TrustMarkType.Should().Be("trust-mark+jwt");
    }

    /// <summary>
    /// Tests OidFederationConstants.SigningAlgorithms.All contains expected algorithms.
    /// </summary>
    [Fact]
    public void OidFederationConstants_SigningAlgorithms_ShouldContainExpectedAlgorithms()
    {
        OidFederationConstants.SigningAlgorithms.All.Should().Contain("ES256");
        OidFederationConstants.SigningAlgorithms.All.Should().Contain("ES384");
        OidFederationConstants.SigningAlgorithms.All.Should().Contain("ES512");
        OidFederationConstants.SigningAlgorithms.All.Should().Contain("RS256");
        OidFederationConstants.SigningAlgorithms.All.Should().Contain("RS384");
        OidFederationConstants.SigningAlgorithms.All.Should().Contain("RS512");
        OidFederationConstants.SigningAlgorithms.All.Should().Contain("PS256");
        OidFederationConstants.SigningAlgorithms.All.Should().Contain("PS384");
        OidFederationConstants.SigningAlgorithms.All.Should().Contain("PS512");
    }

    /// <summary>
    /// Tests OidFederationConstants.SigningAlgorithms.Recommended contains expected algorithms.
    /// </summary>
    [Fact]
    public void OidFederationConstants_SigningAlgorithms_Recommended_ShouldContainExpectedAlgorithms()
    {
        OidFederationConstants.SigningAlgorithms.Recommended.Should().Contain("ES256");
        OidFederationConstants.SigningAlgorithms.Recommended.Should().Contain("RS256");
    }

    /// <summary>
    /// Tests OidFederationConstants.EntityTypes values.
    /// </summary>
    [Fact]
    public void OidFederationConstants_EntityTypes_ShouldHaveCorrectValues()
    {
        OidFederationConstants.EntityTypes.TrustAnchor.Should().Be("trust_anchor");
        OidFederationConstants.EntityTypes.IntermediateAuthority.Should().Be("intermediate_authority");
        OidFederationConstants.EntityTypes.LeafEntity.Should().Be("leaf_entity");
    }

    #endregion
}
