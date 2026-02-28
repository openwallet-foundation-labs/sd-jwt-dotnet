using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.OidFederation.Logic;
using SdJwt.Net.OidFederation.Models;
using System.Security.Cryptography;

namespace SdJwt.Net.OidFederation.Tests;

/// <summary>
/// Tests for TrustChainResult and TrustChainResultExtensions classes.
/// </summary>
public class TrustChainResultTests
{
    private readonly ECDsaSecurityKey _signingKey;
    private readonly object _jwkSet;

    /// <summary>
    /// Initializes test fixtures for the tests.
    /// </summary>
    public TrustChainResultTests()
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _signingKey = new ECDsaSecurityKey(ecdsa);
        _jwkSet = new
        {
            keys = new[] { new { kty = "EC", crv = "P-256", x = "test", y = "test", use = "sig" } }
        };
    }

    #region TrustChainResult Tests

    /// <summary>
    /// Tests TrustChainResult.Success creates valid result.
    /// </summary>
    [Fact]
    public void TrustChainResult_Success_ShouldCreateValidResult()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var chain = new List<EntityStatement>();

        // Act
        var result = TrustChainResult.Success(trustAnchor, config, chain);

        // Assert
        result.IsValid.Should().BeTrue();
        result.TrustAnchor.Should().Be(trustAnchor);
        result.EntityConfiguration.Should().BeSameAs(config);
        result.TrustChain.Should().BeEmpty();
        result.ErrorMessage.Should().BeNull();
    }

    /// <summary>
    /// Tests TrustChainResult.Success with metadata and validation details.
    /// </summary>
    [Fact]
    public void TrustChainResult_Success_ShouldAcceptOptionalParameters()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var chain = new List<EntityStatement>();
        var metadata = new EntityMetadata();
        var validationDetails = new[] { "Detail 1", "Detail 2" };

        // Act
        var result = TrustChainResult.Success(trustAnchor, config, chain, metadata, validationDetails);

        // Assert
        result.ValidatedMetadata.Should().BeSameAs(metadata);
        result.ValidationDetails.Should().BeEquivalentTo(validationDetails);
    }

    /// <summary>
    /// Tests TrustChainResult.Failed creates invalid result.
    /// </summary>
    [Fact]
    public void TrustChainResult_Failed_ShouldCreateInvalidResult()
    {
        // Arrange
        var errorMessage = "Test error message";

        // Act
        var result = TrustChainResult.Failed(errorMessage);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        result.TrustAnchor.Should().BeNull();
        result.EntityConfiguration.Should().BeNull();
        result.TrustChain.Should().BeEmpty();
    }

    /// <summary>
    /// Tests TrustChainResult.Failed with validation details.
    /// </summary>
    [Fact]
    public void TrustChainResult_Failed_ShouldAcceptValidationDetails()
    {
        // Arrange
        var errorMessage = "Test error";
        var validationDetails = new[] { "Detail 1", "Detail 2" };

        // Act
        var result = TrustChainResult.Failed(errorMessage, validationDetails);

        // Assert
        result.ValidationDetails.Should().BeEquivalentTo(validationDetails);
    }

    /// <summary>
    /// Tests TrustChainResult.PathLength property.
    /// </summary>
    [Fact]
    public void TrustChainResult_PathLength_ShouldReturnChainCount()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var statements = new List<EntityStatement>
        {
            EntityStatement.Create("https://issuer1.example.com", "https://subject1.example.com"),
            EntityStatement.Create("https://issuer2.example.com", "https://subject2.example.com")
        };

        // Act
        var result = TrustChainResult.Success(trustAnchor, config, statements);

        // Assert
        result.PathLength.Should().Be(2);
    }

    /// <summary>
    /// Tests TrustChainResult.GetTrustChainEntities for invalid result.
    /// </summary>
    [Fact]
    public void TrustChainResult_GetTrustChainEntities_ShouldReturnEmptyForInvalidResult()
    {
        // Arrange
        var result = TrustChainResult.Failed("Error");

        // Act
        var entities = result.GetTrustChainEntities();

        // Assert
        entities.Should().BeEmpty();
    }

    /// <summary>
    /// Tests TrustChainResult.GetTrustChainEntities returns entities in order.
    /// </summary>
    [Fact]
    public void TrustChainResult_GetTrustChainEntities_ShouldReturnEntitiesInOrder()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var statements = new List<EntityStatement>
        {
            EntityStatement.Create("https://intermediate.example.com", "https://entity.example.com")
        };
        var result = TrustChainResult.Success(trustAnchor, config, statements);

        // Act
        var entities = result.GetTrustChainEntities();

        // Assert
        entities.Should().NotBeEmpty();
        entities.First().Should().Be(trustAnchor);
    }

    /// <summary>
    /// Tests TrustChainResult.GetAllTrustMarks with no trust marks.
    /// </summary>
    [Fact]
    public void TrustChainResult_GetAllTrustMarks_ShouldReturnEmptyWhenNoTrustMarks()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement>());

        // Act
        var trustMarks = result.GetAllTrustMarks();

        // Assert
        trustMarks.Should().BeEmpty();
    }

    /// <summary>
    /// Tests TrustChainResult.GetAllTrustMarks includes marks from configuration.
    /// </summary>
    [Fact]
    public void TrustChainResult_GetAllTrustMarks_ShouldIncludeConfigurationTrustMarks()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        config.TrustMarks = new[]
        {
            TrustMark.Create("trust-mark-1", "value1", "https://issuer.example.com")
        };
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement>());

        // Act
        var trustMarks = result.GetAllTrustMarks();

        // Assert
        trustMarks.Should().HaveCount(1);
        trustMarks[0].Id.Should().Be("trust-mark-1");
    }

    /// <summary>
    /// Tests TrustChainResult.GetAllTrustMarks includes marks from entity statements.
    /// </summary>
    [Fact]
    public void TrustChainResult_GetAllTrustMarks_ShouldIncludeStatementTrustMarks()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var statement = EntityStatement.Create("https://issuer.example.com", "https://subject.example.com");
        statement.TrustMarks = new[]
        {
            TrustMark.Create("statement-trust-mark", "value", "https://issuer.example.com")
        };
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement> { statement });

        // Act
        var trustMarks = result.GetAllTrustMarks();

        // Assert
        trustMarks.Should().Contain(tm => tm.Id == "statement-trust-mark");
    }

    /// <summary>
    /// Tests TrustChainResult.HasTrustMark returns false when trust mark not found.
    /// </summary>
    [Fact]
    public void TrustChainResult_HasTrustMark_ShouldReturnFalseWhenNotFound()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement>());

        // Act
        var hasTrustMark = result.HasTrustMark("non-existent-trust-mark");

        // Assert
        hasTrustMark.Should().BeFalse();
    }

    /// <summary>
    /// Tests TrustChainResult.HasTrustMark returns true when trust mark exists and is valid.
    /// </summary>
    [Fact]
    public void TrustChainResult_HasTrustMark_ShouldReturnTrueWhenFoundAndValid()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        config.TrustMarks = new[]
        {
            TrustMark.Create("existing-trust-mark", "value", "https://issuer.example.com", 24)
        };
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement>());

        // Act
        var hasTrustMark = result.HasTrustMark("existing-trust-mark");

        // Assert
        hasTrustMark.Should().BeTrue();
    }

    /// <summary>
    /// Tests TrustChainResult.GetEffectiveMetadata returns null when metadata is null.
    /// </summary>
    [Fact]
    public void TrustChainResult_GetEffectiveMetadata_ShouldReturnNullWhenNoMetadata()
    {
        // Arrange
        var result = TrustChainResult.Failed("Error");

        // Act
        var metadata = result.GetEffectiveMetadata("federation_entity");

        // Assert
        metadata.Should().BeNull();
    }

    /// <summary>
    /// Tests TrustChainResult.GetEffectiveMetadata returns protocol metadata.
    /// </summary>
    [Fact]
    public void TrustChainResult_GetEffectiveMetadata_ShouldReturnProtocolMetadata()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var validatedMetadata = new EntityMetadata
        {
            FederationEntity = new FederationEntityMetadata { Name = "Test Entity" }
        };
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement>(), validatedMetadata);

        // Act
        var metadata = result.GetEffectiveMetadata("federation_entity");

        // Assert
        metadata.Should().NotBeNull();
        ((FederationEntityMetadata)metadata!).Name.Should().Be("Test Entity");
    }

    /// <summary>
    /// Tests TrustChainResult.GetEffectiveConstraints with no constraints.
    /// </summary>
    [Fact]
    public void TrustChainResult_GetEffectiveConstraints_ShouldReturnConstraintsWithNoMaxPathLength()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement>());

        // Act
        var constraints = result.GetEffectiveConstraints();

        // Assert
        constraints.Should().NotBeNull();
        constraints.MaxPathLength.Should().BeNull();
    }

    /// <summary>
    /// Tests TrustChainResult.GetEffectiveConstraints returns most restrictive path length.
    /// </summary>
    [Fact]
    public void TrustChainResult_GetEffectiveConstraints_ShouldReturnMostRestrictivePathLength()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        config.Constraints = new EntityConstraints { MaxPathLength = 5 };

        var statement1 = EntityStatement.Create("https://issuer1.example.com", "https://subject1.example.com");
        statement1.Constraints = new EntityConstraints { MaxPathLength = 3 };

        var statement2 = EntityStatement.Create("https://issuer2.example.com", "https://subject2.example.com");
        statement2.Constraints = new EntityConstraints { MaxPathLength = 7 };

        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement> { statement1, statement2 });

        // Act
        var constraints = result.GetEffectiveConstraints();

        // Assert
        constraints.MaxPathLength.Should().Be(3);
    }

    #endregion

    #region TrustChainResultExtensions Tests

    /// <summary>
    /// Tests SupportsProtocol returns false for invalid result.
    /// </summary>
    [Fact]
    public void TrustChainResultExtensions_SupportsProtocol_ShouldReturnFalseForInvalidResult()
    {
        // Arrange
        var result = TrustChainResult.Failed("Error");

        // Act
        var supports = result.SupportsProtocol("federation_entity");

        // Assert
        supports.Should().BeFalse();
    }

    /// <summary>
    /// Tests SupportsProtocol returns false when metadata is null.
    /// </summary>
    [Fact]
    public void TrustChainResultExtensions_SupportsProtocol_ShouldReturnFalseWhenNoMetadata()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement>(), null);

        // Act
        var supports = result.SupportsProtocol("federation_entity");

        // Assert
        supports.Should().BeFalse();
    }

    /// <summary>
    /// Tests SupportsProtocol returns true when protocol is supported.
    /// </summary>
    [Fact]
    public void TrustChainResultExtensions_SupportsProtocol_ShouldReturnTrueWhenSupported()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var metadata = new EntityMetadata
        {
            FederationEntity = new FederationEntityMetadata()
        };
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement>(), metadata);

        // Act
        var supports = result.SupportsProtocol("federation_entity");

        // Assert
        supports.Should().BeTrue();
    }

    /// <summary>
    /// Tests GetTrustChainSummary for invalid result.
    /// </summary>
    [Fact]
    public void TrustChainResultExtensions_GetTrustChainSummary_ShouldReturnErrorForInvalidResult()
    {
        // Arrange
        var result = TrustChainResult.Failed("Test error message");

        // Act
        var summary = result.GetTrustChainSummary();

        // Assert
        summary.Should().Contain("Invalid trust chain");
        summary.Should().Contain("Test error message");
    }

    /// <summary>
    /// Tests GetTrustChainSummary for valid result.
    /// </summary>
    [Fact]
    public void TrustChainResultExtensions_GetTrustChainSummary_ShouldReturnSummaryForValidResult()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement>());

        // Act
        var summary = result.GetTrustChainSummary();

        // Assert
        summary.Should().Contain("Valid trust chain");
        summary.Should().Contain("length 0");
    }

    /// <summary>
    /// Tests SatisfiesRequirements returns false for invalid result.
    /// </summary>
    [Fact]
    public void TrustChainResultExtensions_SatisfiesRequirements_ShouldReturnFalseForInvalidResult()
    {
        // Arrange
        var result = TrustChainResult.Failed("Error");
        var requirements = new TrustChainRequirements();

        // Act
        var satisfies = result.SatisfiesRequirements(requirements);

        // Assert
        satisfies.Should().BeFalse();
    }

    /// <summary>
    /// Tests SatisfiesRequirements returns true when all requirements are met.
    /// </summary>
    [Fact]
    public void TrustChainResultExtensions_SatisfiesRequirements_ShouldReturnTrueWhenAllMet()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement>());
        var requirements = new TrustChainRequirements { MaxPathLength = 5 };

        // Act
        var satisfies = result.SatisfiesRequirements(requirements);

        // Assert
        satisfies.Should().BeTrue();
    }

    /// <summary>
    /// Tests SatisfiesRequirements returns false when path length exceeds max.
    /// </summary>
    [Fact]
    public void TrustChainResultExtensions_SatisfiesRequirements_ShouldReturnFalseWhenPathLengthExceeded()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var statements = new List<EntityStatement>();
        for (int i = 0; i < 5; i++)
        {
            statements.Add(EntityStatement.Create($"https://issuer{i}.example.com", $"https://subject{i}.example.com"));
        }
        var result = TrustChainResult.Success(trustAnchor, config, statements);
        var requirements = new TrustChainRequirements { MaxPathLength = 2 };

        // Act
        var satisfies = result.SatisfiesRequirements(requirements);

        // Assert
        satisfies.Should().BeFalse();
    }

    /// <summary>
    /// Tests SatisfiesRequirements returns false when required trust marks are missing.
    /// </summary>
    [Fact]
    public void TrustChainResultExtensions_SatisfiesRequirements_ShouldReturnFalseWhenTrustMarksMissing()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement>());
        var requirements = new TrustChainRequirements
        {
            RequiredTrustMarks = new[] { "required-trust-mark" }
        };

        // Act
        var satisfies = result.SatisfiesRequirements(requirements);

        // Assert
        satisfies.Should().BeFalse();
    }

    /// <summary>
    /// Tests SatisfiesRequirements returns false when required protocols are missing.
    /// </summary>
    [Fact]
    public void TrustChainResultExtensions_SatisfiesRequirements_ShouldReturnFalseWhenProtocolsMissing()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement>(), new EntityMetadata());
        var requirements = new TrustChainRequirements
        {
            RequiredProtocols = new[] { "openid_credential_issuer" }
        };

        // Act
        var satisfies = result.SatisfiesRequirements(requirements);

        // Assert
        satisfies.Should().BeFalse();
    }

    /// <summary>
    /// Tests SatisfiesRequirements returns false when trust anchor is not allowed.
    /// </summary>
    [Fact]
    public void TrustChainResultExtensions_SatisfiesRequirements_ShouldReturnFalseWhenTrustAnchorNotAllowed()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement>());
        var requirements = new TrustChainRequirements
        {
            AllowedTrustAnchors = new[] { "https://other-anchor.example.com" }
        };

        // Act
        var satisfies = result.SatisfiesRequirements(requirements);

        // Assert
        satisfies.Should().BeFalse();
    }

    /// <summary>
    /// Tests SatisfiesRequirements returns true when trust anchor is in allowed list.
    /// </summary>
    [Fact]
    public void TrustChainResultExtensions_SatisfiesRequirements_ShouldReturnTrueWhenTrustAnchorAllowed()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var config = EntityConfiguration.Create("https://entity.example.com", _jwkSet);
        var result = TrustChainResult.Success(trustAnchor, config, new List<EntityStatement>());
        var requirements = new TrustChainRequirements
        {
            AllowedTrustAnchors = new[] { trustAnchor }
        };

        // Act
        var satisfies = result.SatisfiesRequirements(requirements);

        // Assert
        satisfies.Should().BeTrue();
    }

    #endregion
}
