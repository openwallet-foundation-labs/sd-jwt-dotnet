using FluentAssertions;
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Verifier;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Models;

public class Oid4VpModelTests
{
    [Fact]
    public void PathNestedDescriptor_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var descriptor = new PathNestedDescriptor();

        // Act
        descriptor.Format = "vc+sd-jwt";
        descriptor.Path = new[] { "$.credentialSubject.name" };
        descriptor.PathNested = new PathNestedDescriptor
        {
            Format = "nested-format",
            Path = new[] { "$.nested.value" }
        };

        // Assert
        descriptor.Format.Should().Be("vc+sd-jwt");
        descriptor.Path.Should().BeEquivalentTo(new[] { "$.credentialSubject.name" });
        descriptor.PathNested.Should().NotBeNull();
        descriptor.PathNested.Format.Should().Be("nested-format");
    }

    [Fact]
    public void StatusConstraints_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var constraints = new StatusConstraints();

        // Act
        constraints.Active = StatusDirective.Required();
        constraints.Suspended = StatusDirective.Disallowed();
        constraints.Revoked = StatusDirective.Disallowed();

        // Assert
        constraints.Active.Should().NotBeNull();
        constraints.Active.Directive.Should().Be("required");
        constraints.Suspended.Should().NotBeNull();
        constraints.Suspended.Directive.Should().Be("disallowed");
        constraints.Revoked.Should().NotBeNull();
        constraints.Revoked.Directive.Should().Be("disallowed");
    }

    [Fact]
    public void StatusConstraints_Validate_WithValidConstraints_ShouldNotThrow()
    {
        // Arrange
        var constraints = new StatusConstraints
        {
            Active = StatusDirective.Required(),
            Suspended = StatusDirective.Disallowed(),
            Revoked = StatusDirective.Disallowed()
        };

        // Act & Assert
        var act = () => constraints.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void StatusDirective_Required_ShouldCreateCorrectDirective()
    {
        // Act
        var directive = StatusDirective.Required();

        // Assert
        directive.Directive.Should().Be("required");
    }

    [Fact]
    public void StatusDirective_Allowed_ShouldCreateCorrectDirective()
    {
        // Act
        var directive = StatusDirective.Allowed();

        // Assert
        directive.Directive.Should().Be("allowed");
    }

    [Fact]
    public void StatusDirective_Disallowed_ShouldCreateCorrectDirective()
    {
        // Act
        var directive = StatusDirective.Disallowed();

        // Assert
        directive.Directive.Should().Be("disallowed");
    }

    [Fact]
    public void StatusDirective_Validate_WithValidDirective_ShouldNotThrow()
    {
        // Arrange
        var directive = new StatusDirective { Directive = "required" };

        // Act & Assert
        var act = () => directive.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void StatusDirective_Validate_WithInvalidDirective_ShouldThrow()
    {
        // Arrange
        var directive = new StatusDirective { Directive = "invalid" };

        // Act & Assert
        var act = () => directive.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Status directive must be 'required', 'allowed', or 'disallowed'");
    }

    [Fact]
    public void StatusDirective_Validate_WithEmptyDirective_ShouldThrow()
    {
        // Arrange
        var directive = new StatusDirective { Directive = "" };

        // Act & Assert
        var act = () => directive.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Status directive is required");
    }
}

public class Oid4VpVerifierTests
{
    [Fact]
    public void CustomValidationResult_Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = CustomValidationResult.Success();

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void CustomValidationResult_Failed_ShouldCreateFailedResult()
    {
        // Arrange
        var errorMessage = "Custom validation failed";

        // Act
        var result = CustomValidationResult.Failed(errorMessage);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void SingleVpTokenResult_Failed_ShouldCreateFailedResult()
    {
        // Arrange
        var error = "Token validation failed";

        // Act
        var result = SingleVpTokenResult.Failed(error);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Be(error);
        result.VerificationResult.Should().BeNull();
        result.Claims.Should().BeEmpty();
    }

    [Fact]
    public void VpTokenValidationResult_Failed_ShouldCreateFailedResult()
    {
        // Arrange
        var error = "Validation failed";

        // Act
        var result = VpTokenValidationResult.Failed(error);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Be(error);
        result.ValidatedTokens.Should().BeEmpty();
        result.PresentationSubmission.Should().BeNull();
    }

    [Fact]
    public void VpTokenResult_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var result = new VpTokenResult();
        var claims = new Dictionary<string, object>
        {
            ["name"] = "John Doe",
            ["age"] = 30
        };

        // Act
        result.Index = 0;
        result.Token = "test.token.value";
        result.IsValid = true;
        result.Error = null;
        result.Claims = claims;

        // Assert
        result.Index.Should().Be(0);
        result.Token.Should().Be("test.token.value");
        result.IsValid.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Claims.Should().BeEquivalentTo(claims);
    }

    [Fact]
    public void VpTokenValidationOptions_ShouldHaveCorrectDefaults()
    {
        // Act
        var options = new VpTokenValidationOptions();

        // Assert
        options.ValidateIssuer.Should().BeTrue();
        options.ValidateAudience.Should().BeFalse();
        options.ValidateLifetime.Should().BeTrue();
        options.RequireExpirationTime.Should().BeTrue();
        options.ClockSkew.Should().Be(TimeSpan.FromMinutes(5));
        options.ValidateKeyBindingAudience.Should().BeTrue();
        options.ValidateKeyBindingLifetime.Should().BeTrue();
        options.StopOnFirstFailure.Should().BeFalse();
    }
}

// Mock classes for testing if they don't exist
public class PathNestedDescriptor
{
    public string? Format { get; set; }
    public string[]? Path { get; set; }
    public PathNestedDescriptor? PathNested { get; set; }
}
