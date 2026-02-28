using FluentAssertions;
using SdJwt.Net.Oid4Vp.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Coverage;

/// <summary>
/// Tests for <see cref="Field"/> and <see cref="Constraints"/> classes, covering
/// factory methods, fluent builders, validation, and constraint composition.
/// </summary>
public class FieldConstraintsTests
{
    #region Field Tests

    /// <summary>
    /// Tests that CreateForCredentialType() creates valid field constraint.
    /// </summary>
    [Fact]
    public void Field_CreateForCredentialType_WithValidType_ReturnsInstance()
    {
        // Act
        var result = Field.CreateForCredentialType("IdentityCredential");

        // Assert
        result.Path.Should().Contain(Oid4VpConstants.JsonPaths.CredentialType);
        result.Filter.Should().ContainKey("type");
        result.Filter.Should().ContainKey("const");
        result.Filter!["const"].Should().Be("IdentityCredential");
    }

    /// <summary>
    /// Tests that CreateForCredentialType() throws ArgumentException when type is null.
    /// </summary>
    [Fact]
    public void Field_CreateForCredentialType_WithNullType_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => Field.CreateForCredentialType(null!);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateForCredentialType() throws ArgumentException when type is empty.
    /// </summary>
    [Fact]
    public void Field_CreateForCredentialType_WithEmptyType_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => Field.CreateForCredentialType("");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateForIssuer() creates valid field constraint.
    /// </summary>
    [Fact]
    public void Field_CreateForIssuer_WithValidIssuer_ReturnsInstance()
    {
        // Act
        var result = Field.CreateForIssuer("https://issuer.example.com", "Verify issuer");

        // Assert
        result.Path.Should().Contain(Oid4VpConstants.JsonPaths.Issuer);
        result.Purpose.Should().Be("Verify issuer");
        result.Filter!["const"].Should().Be("https://issuer.example.com");
    }

    /// <summary>
    /// Tests that CreateForIssuer() uses default purpose when not provided.
    /// </summary>
    [Fact]
    public void Field_CreateForIssuer_WithNoPurpose_UsesDefaultPurpose()
    {
        // Act
        var result = Field.CreateForIssuer("https://issuer.example.com");

        // Assert
        result.Purpose.Should().Contain("issuer");
    }

    /// <summary>
    /// Tests that CreateForIssuer() throws ArgumentException when issuer is null.
    /// </summary>
    [Fact]
    public void Field_CreateForIssuer_WithNullIssuer_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => Field.CreateForIssuer(null!);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateForSubject() creates valid field constraint.
    /// </summary>
    [Fact]
    public void Field_CreateForSubject_WithValidSubject_ReturnsInstance()
    {
        // Act
        var result = Field.CreateForSubject("did:example:123", "Verify subject");

        // Assert
        result.Path.Should().Contain(Oid4VpConstants.JsonPaths.Subject);
        result.Purpose.Should().Be("Verify subject");
        result.Filter!["const"].Should().Be("did:example:123");
    }

    /// <summary>
    /// Tests that CreateForSubject() throws ArgumentException when subject is null.
    /// </summary>
    [Fact]
    public void Field_CreateForSubject_WithNullSubject_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => Field.CreateForSubject(null!);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateForPath() creates valid field constraint.
    /// </summary>
    [Fact]
    public void Field_CreateForPath_WithValidPath_ReturnsInstance()
    {
        // Act
        var result = Field.CreateForPath("$.name", "Name", "Get user name", true);

        // Assert
        result.Path.Should().Contain("$.name");
        result.Name.Should().Be("Name");
        result.Purpose.Should().Be("Get user name");
        result.Optional.Should().BeTrue();
    }

    /// <summary>
    /// Tests that CreateForPath() throws ArgumentException when path is null.
    /// </summary>
    [Fact]
    public void Field_CreateForPath_WithNullPath_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => Field.CreateForPath(null!);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateWithFilter() creates valid field constraint with custom filter.
    /// </summary>
    [Fact]
    public void Field_CreateWithFilter_WithValidParams_ReturnsInstance()
    {
        // Arrange
        var filter = new Dictionary<string, object>
        {
            ["type"] = "number",
            ["minimum"] = 18
        };

        // Act
        var result = Field.CreateWithFilter("$.age", filter, "Age", "Verify age", false);

        // Assert
        result.Path.Should().Contain("$.age");
        result.Filter.Should().ContainKey("minimum");
        result.Name.Should().Be("Age");
        result.Optional.Should().BeFalse();
    }

    /// <summary>
    /// Tests that CreateWithFilter() throws ArgumentException when path is null.
    /// </summary>
    [Fact]
    public void Field_CreateWithFilter_WithNullPath_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => Field.CreateWithFilter(null!, new Dictionary<string, object>());
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that CreateWithFilter() throws ArgumentNullException when filter is null.
    /// </summary>
    [Fact]
    public void Field_CreateWithFilter_WithNullFilter_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => Field.CreateWithFilter("$.age", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that WithStringValue() adds string constraint to filter.
    /// </summary>
    [Fact]
    public void Field_WithStringValue_AddsConstraint()
    {
        // Arrange
        var field = new Field { Path = new[] { "$.name" } };

        // Act
        var result = field.WithStringValue("John");

        // Assert
        result.Should().BeSameAs(field);
        result.Filter.Should().ContainKey("const");
        result.Filter!["const"].Should().Be("John");
    }

    /// <summary>
    /// Tests that WithStringValue() throws ArgumentException when value is null.
    /// </summary>
    [Fact]
    public void Field_WithStringValue_WithNullValue_ThrowsArgumentException()
    {
        // Arrange
        var field = new Field { Path = new[] { "$.name" } };

        // Act & Assert
        var act = () => field.WithStringValue(null!);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that WithStringEnum() adds enum constraint to filter.
    /// </summary>
    [Fact]
    public void Field_WithStringEnum_AddsConstraint()
    {
        // Arrange
        var field = new Field { Path = new[] { "$.status" } };

        // Act
        var result = field.WithStringEnum("active", "pending", "inactive");

        // Assert
        result.Should().BeSameAs(field);
        result.Filter.Should().ContainKey("enum");
    }

    /// <summary>
    /// Tests that WithStringEnum() throws ArgumentNullException when values is null.
    /// </summary>
    [Fact]
    public void Field_WithStringEnum_WithNullValues_ThrowsArgumentNullException()
    {
        // Arrange
        var field = new Field { Path = new[] { "$.status" } };

        // Act & Assert
        var act = () => field.WithStringEnum(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that WithStringEnum() throws ArgumentException when values is empty.
    /// </summary>
    [Fact]
    public void Field_WithStringEnum_WithEmptyValues_ThrowsArgumentException()
    {
        // Arrange
        var field = new Field { Path = new[] { "$.status" } };

        // Act & Assert
        var act = () => field.WithStringEnum();
        act.Should().Throw<ArgumentException>().WithMessage("*At least one*");
    }

    /// <summary>
    /// Tests that AsOptional() sets Optional property.
    /// </summary>
    [Fact]
    public void Field_AsOptional_SetsOptionalTrue()
    {
        // Arrange
        var field = new Field { Path = new[] { "$.name" } };

        // Act
        var result = field.AsOptional();

        // Assert
        result.Should().BeSameAs(field);
        result.Optional.Should().BeTrue();
    }

    /// <summary>
    /// Tests that AsOptional(false) sets Optional to false.
    /// </summary>
    [Fact]
    public void Field_AsOptional_WithFalse_SetsOptionalFalse()
    {
        // Act
        var result = new Field { Path = new[] { "$.name" } }.AsOptional(false);

        // Assert
        result.Optional.Should().BeFalse();
    }

    /// <summary>
    /// Tests that WithIntentToRetain() sets IntentToRetain property.
    /// </summary>
    [Fact]
    public void Field_WithIntentToRetain_SetsProperty()
    {
        // Arrange
        var field = new Field { Path = new[] { "$.name" } };

        // Act
        var result = field.WithIntentToRetain();

        // Assert
        result.Should().BeSameAs(field);
        result.IntentToRetain.Should().BeTrue();
    }

    /// <summary>
    /// Tests that WithIntentToRetain(false) sets IntentToRetain to false.
    /// </summary>
    [Fact]
    public void Field_WithIntentToRetain_WithFalse_SetsPropertyFalse()
    {
        // Act
        var result = new Field { Path = new[] { "$.name" } }.WithIntentToRetain(false);

        // Assert
        result.IntentToRetain.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Validate() succeeds with valid field.
    /// </summary>
    [Fact]
    public void Field_Validate_WithValidField_DoesNotThrow()
    {
        // Arrange
        var field = Field.CreateForCredentialType("IdentityCredential");

        // Act & Assert
        var act = () => field.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when path is null.
    /// </summary>
    [Fact]
    public void Field_Validate_WithNullPath_ThrowsInvalidOperationException()
    {
        // Arrange
        var field = new Field { Path = null! };

        // Act & Assert
        var act = () => field.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*path*required*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when path is empty.
    /// </summary>
    [Fact]
    public void Field_Validate_WithEmptyPath_ThrowsInvalidOperationException()
    {
        // Arrange
        var field = new Field { Path = Array.Empty<string>() };

        // Act & Assert
        var act = () => field.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*path*required*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when path contains null.
    /// </summary>
    [Fact]
    public void Field_Validate_WithNullInPath_ThrowsInvalidOperationException()
    {
        // Arrange
        var field = new Field { Path = new string[] { "$.name", null! } };

        // Act & Assert
        var act = () => field.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*null or empty*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when path contains empty string.
    /// </summary>
    [Fact]
    public void Field_Validate_WithEmptyInPath_ThrowsInvalidOperationException()
    {
        // Arrange
        var field = new Field { Path = new[] { "$.name", "" } };

        // Act & Assert
        var act = () => field.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*null or empty*");
    }

    #endregion

    #region Constraints Tests

    /// <summary>
    /// Tests that CreateWithRequiredDisclosure() creates valid constraints.
    /// </summary>
    [Fact]
    public void Constraints_CreateWithRequiredDisclosure_ReturnsInstance()
    {
        // Arrange
        var field1 = Field.CreateForCredentialType("Identity");
        var field2 = Field.CreateForIssuer("https://issuer.example.com");

        // Act
        var result = Constraints.CreateWithRequiredDisclosure(field1, field2);

        // Assert
        result.LimitDisclosure.Should().Be("required");
        result.Fields.Should().HaveCount(2);
    }

    /// <summary>
    /// Tests that CreateWithPreferredDisclosure() creates valid constraints.
    /// </summary>
    [Fact]
    public void Constraints_CreateWithPreferredDisclosure_ReturnsInstance()
    {
        // Arrange
        var field = Field.CreateForCredentialType("Identity");

        // Act
        var result = Constraints.CreateWithPreferredDisclosure(field);

        // Assert
        result.LimitDisclosure.Should().Be("preferred");
        result.Fields.Should().HaveCount(1);
    }

    /// <summary>
    /// Tests that Validate() succeeds with valid constraints.
    /// </summary>
    [Fact]
    public void Constraints_Validate_WithValidConstraints_DoesNotThrow()
    {
        // Arrange
        var constraints = Constraints.CreateWithRequiredDisclosure(
            Field.CreateForCredentialType("Identity"));

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when limit_disclosure is invalid.
    /// </summary>
    [Fact]
    public void Constraints_Validate_WithInvalidLimitDisclosure_ThrowsInvalidOperationException()
    {
        // Arrange
        var constraints = new Constraints { LimitDisclosure = "invalid" };

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*'required' or 'preferred'*");
    }

    /// <summary>
    /// Tests that Validate() cascades validation to fields.
    /// </summary>
    [Fact]
    public void Constraints_Validate_CascadesToFields()
    {
        // Arrange
        var constraints = new Constraints
        {
            Fields = new[] { new Field { Path = Array.Empty<string>() } }  // Invalid
        };

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that Validate() cascades validation to status constraints.
    /// </summary>
    [Fact]
    public void Constraints_Validate_CascadesToStatusConstraints()
    {
        // Arrange
        var constraints = new Constraints
        {
            Statuses = new StatusConstraints
            {
                Active = new StatusDirective { Directive = "invalid" }
            }
        };

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Tests property setters and getters for Constraints.
    /// </summary>
    [Fact]
    public void Constraints_Properties_CanBeSetAndRetrieved()
    {
        // Arrange & Act
        var constraints = new Constraints
        {
            SubjectIsIssuer = "required",
            SameSubject = new[] { "group1" }
        };

        // Assert
        constraints.SubjectIsIssuer.Should().Be("required");
        constraints.SameSubject.Should().Contain("group1");
    }

    #endregion
}
