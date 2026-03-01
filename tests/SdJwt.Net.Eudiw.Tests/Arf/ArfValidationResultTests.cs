using FluentAssertions;
using SdJwt.Net.Eudiw.Arf;
using Xunit;

namespace SdJwt.Net.Eudiw.Tests.Arf;

/// <summary>
/// Tests for ARF Validation Result model.
/// </summary>
public class ArfValidationResultTests
{
    [Fact]
    public void Valid_CreatesSuccessResult()
    {
        // Act
        var result = ArfValidationResult.Valid(ArfCredentialType.Pid);

        // Assert
        result.IsValid.Should().BeTrue();
        result.CredentialType.Should().Be(ArfCredentialType.Pid);
        result.Error.Should().BeNull();
        result.MissingClaims.Should().BeEmpty();
    }

    [Fact]
    public void Invalid_CreatesFailureResult()
    {
        // Act
        var result = ArfValidationResult.Invalid("Invalid credential format");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Be("Invalid credential format");
    }

    [Fact]
    public void Invalid_WithMissingClaims_IncludesMissingList()
    {
        // Act
        var result = ArfValidationResult.Invalid(
            "Missing mandatory claims",
            new[] { "family_name", "given_name" });

        // Assert
        result.IsValid.Should().BeFalse();
        result.MissingClaims.Should().Contain("family_name");
        result.MissingClaims.Should().Contain("given_name");
    }

    [Fact]
    public void IsValid_DefaultsToFalse()
    {
        // Arrange & Act
        var result = new ArfValidationResult();

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void MissingClaims_DefaultsToEmpty()
    {
        // Arrange & Act
        var result = new ArfValidationResult();

        // Assert
        result.MissingClaims.Should().BeEmpty();
    }
}
