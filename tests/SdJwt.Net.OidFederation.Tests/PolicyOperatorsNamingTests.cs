using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.OidFederation.Models;
using System.Security.Cryptography;

namespace SdJwt.Net.OidFederation.Tests;

/// <summary>
/// Tests for PolicyOperators and NamingConstraints classes.
/// </summary>
public class PolicyOperatorsNamingTests
{
    #region Test Fixtures

    private readonly ECDsaSecurityKey _signingKey;
    private readonly object _jwkSet;

    /// <summary>
    /// Initializes test fixtures for the coverage tests.
    /// </summary>
    public PolicyOperatorsNamingTests()
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _signingKey = new ECDsaSecurityKey(ecdsa);
        _jwkSet = new
        {
            keys = new[] { new { kty = "EC", crv = "P-256", x = "test", y = "test", use = "sig" } }
        };
    }

    #endregion

    #region PolicyOperators Tests

    /// <summary>
    /// Tests that PolicyOperators.Value constant has the expected value.
    /// </summary>
    [Fact]
    public void PolicyOperators_ValueConstant_ShouldBeCorrect()
    {
        // Assert
        PolicyOperators.Value.Should().Be("value");
    }

    /// <summary>
    /// Tests that PolicyOperators.Add constant has the expected value.
    /// </summary>
    [Fact]
    public void PolicyOperators_AddConstant_ShouldBeCorrect()
    {
        // Assert
        PolicyOperators.Add.Should().Be("add");
    }

    /// <summary>
    /// Tests that PolicyOperators.Default constant has the expected value.
    /// </summary>
    [Fact]
    public void PolicyOperators_DefaultConstant_ShouldBeCorrect()
    {
        // Assert
        PolicyOperators.Default.Should().Be("default");
    }

    /// <summary>
    /// Tests that PolicyOperators.Essential constant has the expected value.
    /// </summary>
    [Fact]
    public void PolicyOperators_EssentialConstant_ShouldBeCorrect()
    {
        // Assert
        PolicyOperators.Essential.Should().Be("essential");
    }

    /// <summary>
    /// Tests that PolicyOperators.OneOf constant has the expected value.
    /// </summary>
    [Fact]
    public void PolicyOperators_OneOfConstant_ShouldBeCorrect()
    {
        // Assert
        PolicyOperators.OneOf.Should().Be("one_of");
    }

    /// <summary>
    /// Tests that PolicyOperators.SubsetOf constant has the expected value.
    /// </summary>
    [Fact]
    public void PolicyOperators_SubsetOfConstant_ShouldBeCorrect()
    {
        // Assert
        PolicyOperators.SubsetOf.Should().Be("subset_of");
    }

    /// <summary>
    /// Tests that PolicyOperators.SupersetOf constant has the expected value.
    /// </summary>
    [Fact]
    public void PolicyOperators_SupersetOfConstant_ShouldBeCorrect()
    {
        // Assert
        PolicyOperators.SupersetOf.Should().Be("superset_of");
    }

    /// <summary>
    /// Tests that CreateValue returns correct dictionary structure.
    /// </summary>
    [Fact]
    public void PolicyOperators_CreateValue_ShouldReturnCorrectDictionary()
    {
        // Arrange
        var testValue = "test-value";

        // Act
        var result = PolicyOperators.CreateValue(testValue);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey(PolicyOperators.Value);
        result[PolicyOperators.Value].Should().Be(testValue);
    }

    /// <summary>
    /// Tests that CreateValue works with different value types.
    /// </summary>
    [Theory]
    [InlineData("string-value")]
    [InlineData(123)]
    [InlineData(true)]
    public void PolicyOperators_CreateValue_ShouldWorkWithVariousTypes(object value)
    {
        // Act
        var result = PolicyOperators.CreateValue(value);

        // Assert
        result.Should().NotBeNull();
        result[PolicyOperators.Value].Should().Be(value);
    }

    /// <summary>
    /// Tests that CreateEssential returns correct dictionary with default true.
    /// </summary>
    [Fact]
    public void PolicyOperators_CreateEssential_DefaultsToTrue()
    {
        // Act
        var result = PolicyOperators.CreateEssential();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey(PolicyOperators.Essential);
        result[PolicyOperators.Essential].Should().Be(true);
    }

    /// <summary>
    /// Tests that CreateEssential accepts explicit boolean parameter.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PolicyOperators_CreateEssential_ShouldAcceptBooleanParameter(bool isEssential)
    {
        // Act
        var result = PolicyOperators.CreateEssential(isEssential);

        // Assert
        result.Should().NotBeNull();
        result[PolicyOperators.Essential].Should().Be(isEssential);
    }

    /// <summary>
    /// Tests that CreateOneOf returns correct dictionary with allowed values.
    /// </summary>
    [Fact]
    public void PolicyOperators_CreateOneOf_ShouldReturnCorrectDictionary()
    {
        // Arrange
        var allowedValues = new object[] { "value1", "value2", "value3" };

        // Act
        var result = PolicyOperators.CreateOneOf(allowedValues);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey(PolicyOperators.OneOf);
        result[PolicyOperators.OneOf].Should().BeEquivalentTo(allowedValues);
    }

    /// <summary>
    /// Tests that CreateOneOf works with empty array.
    /// </summary>
    [Fact]
    public void PolicyOperators_CreateOneOf_ShouldWorkWithEmptyArray()
    {
        // Act
        var result = PolicyOperators.CreateOneOf();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey(PolicyOperators.OneOf);
        ((object[])result[PolicyOperators.OneOf]).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that CreateSubsetOf returns correct dictionary with allowed values.
    /// </summary>
    [Fact]
    public void PolicyOperators_CreateSubsetOf_ShouldReturnCorrectDictionary()
    {
        // Arrange
        var allowedValues = new object[] { "ES256", "ES384", "RS256" };

        // Act
        var result = PolicyOperators.CreateSubsetOf(allowedValues);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey(PolicyOperators.SubsetOf);
        result[PolicyOperators.SubsetOf].Should().BeEquivalentTo(allowedValues);
    }

    #endregion

    #region NamingConstraints Tests

    /// <summary>
    /// Tests NamingConstraints default initialization.
    /// </summary>
    [Fact]
    public void NamingConstraints_DefaultInitialization_ShouldHaveNullArrays()
    {
        // Act
        var constraints = new NamingConstraints();

        // Assert
        constraints.Permitted.Should().BeNull();
        constraints.Excluded.Should().BeNull();
    }

    /// <summary>
    /// Tests that NamingConstraints.Validate passes with null arrays.
    /// </summary>
    [Fact]
    public void NamingConstraints_Validate_ShouldPassWithNullArrays()
    {
        // Arrange
        var constraints = new NamingConstraints();

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that NamingConstraints.Validate passes with valid permitted patterns.
    /// </summary>
    [Fact]
    public void NamingConstraints_Validate_ShouldPassWithValidPermittedPatterns()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Permitted = new[] { "https://example.com/*", "https://domain.org/*" }
        };

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that NamingConstraints.Validate passes with valid excluded patterns.
    /// </summary>
    [Fact]
    public void NamingConstraints_Validate_ShouldPassWithValidExcludedPatterns()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Excluded = new[] { "https://excluded.com/*", "https://blocked.org/*" }
        };

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that NamingConstraints.Validate throws with empty permitted pattern.
    /// </summary>
    [Fact]
    public void NamingConstraints_Validate_ShouldThrowWithEmptyPermittedPattern()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Permitted = new[] { "https://valid.com/*", "" }
        };

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Permitted pattern cannot be null or empty");
    }

    /// <summary>
    /// Tests that NamingConstraints.Validate throws with whitespace permitted pattern.
    /// </summary>
    [Fact]
    public void NamingConstraints_Validate_ShouldThrowWithWhitespacePermittedPattern()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Permitted = new[] { "   " }
        };

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Permitted pattern cannot be null or empty");
    }

    /// <summary>
    /// Tests that NamingConstraints.Validate throws with empty excluded pattern.
    /// </summary>
    [Fact]
    public void NamingConstraints_Validate_ShouldThrowWithEmptyExcludedPattern()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Excluded = new[] { "" }
        };

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Excluded pattern cannot be null or empty");
    }

    /// <summary>
    /// Tests that NamingConstraints.Validate throws with whitespace excluded pattern.
    /// </summary>
    [Fact]
    public void NamingConstraints_Validate_ShouldThrowWithWhitespaceExcludedPattern()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Excluded = new[] { "https://valid.com/*", "  " }
        };

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Excluded pattern cannot be null or empty");
    }

    /// <summary>
    /// Tests IsPermitted returns false for null or empty URL.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NamingConstraints_IsPermitted_ShouldReturnFalseForNullOrEmptyUrl(string? url)
    {
        // Arrange
        var constraints = new NamingConstraints();

        // Act
        var result = constraints.IsPermitted(url!);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests IsPermitted returns true when no constraints are defined.
    /// </summary>
    [Fact]
    public void NamingConstraints_IsPermitted_ShouldReturnTrueWithNoConstraints()
    {
        // Arrange
        var constraints = new NamingConstraints();

        // Act
        var result = constraints.IsPermitted("https://any.example.com");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests IsPermitted returns false when URL matches excluded pattern.
    /// </summary>
    [Fact]
    public void NamingConstraints_IsPermitted_ShouldReturnFalseWhenExcluded()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Excluded = new[] { "https://blocked.example.com/*" }
        };

        // Act
        var result = constraints.IsPermitted("https://blocked.example.com/entity");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests IsPermitted returns true when URL matches permitted pattern.
    /// </summary>
    [Fact]
    public void NamingConstraints_IsPermitted_ShouldReturnTrueWhenPermitted()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Permitted = new[] { "https://allowed.example.com/*" }
        };

        // Act
        var result = constraints.IsPermitted("https://allowed.example.com/entity");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests IsPermitted returns false when URL does not match any permitted pattern.
    /// </summary>
    [Fact]
    public void NamingConstraints_IsPermitted_ShouldReturnFalseWhenNotInPermitted()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Permitted = new[] { "https://allowed.example.com/*" }
        };

        // Act
        var result = constraints.IsPermitted("https://other.example.com/entity");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests IsPermitted with wildcard pattern matching all URLs.
    /// </summary>
    [Fact]
    public void NamingConstraints_IsPermitted_ShouldMatchWildcardPattern()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Permitted = new[] { "*" }
        };

        // Act
        var result = constraints.IsPermitted("https://any.domain.com/path");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests IsPermitted with exact URL match (no wildcards).
    /// </summary>
    [Fact]
    public void NamingConstraints_IsPermitted_ShouldMatchExactUrl()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Permitted = new[] { "https://exact.example.com" }
        };

        // Act
        var result = constraints.IsPermitted("https://exact.example.com");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests IsPermitted applies excluded patterns before permitted.
    /// </summary>
    [Fact]
    public void NamingConstraints_IsPermitted_ShouldApplyExcludedBeforePermitted()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Permitted = new[] { "*" },
            Excluded = new[] { "https://blocked.example.com/*" }
        };

        // Act
        var result = constraints.IsPermitted("https://blocked.example.com/entity");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests IsPermitted with case-insensitive matching.
    /// </summary>
    [Fact]
    public void NamingConstraints_IsPermitted_ShouldBeCaseInsensitive()
    {
        // Arrange
        var constraints = new NamingConstraints
        {
            Permitted = new[] { "https://EXAMPLE.COM/*" }
        };

        // Act
        var result = constraints.IsPermitted("https://example.com/path");

        // Assert
        result.Should().BeTrue();
    }

    #endregion
}
