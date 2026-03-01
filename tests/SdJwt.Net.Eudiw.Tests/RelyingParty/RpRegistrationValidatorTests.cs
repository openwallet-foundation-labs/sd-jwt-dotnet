using FluentAssertions;
using SdJwt.Net.Eudiw.RelyingParty;
using Xunit;

namespace SdJwt.Net.Eudiw.Tests.RelyingParty;

/// <summary>
/// Tests for Relying Party Registration Validator per EUDIW requirements.
/// </summary>
public class RpRegistrationValidatorTests
{
    private readonly RpRegistrationValidator _validator;

    public RpRegistrationValidatorTests()
    {
        _validator = new RpRegistrationValidator();
    }

    #region Basic Validation

    [Fact]
    public void Validate_WithValidRegistration_ReturnsSuccess()
    {
        // Arrange
        var registration = CreateValidRpRegistration();

        // Act
        var result = _validator.Validate(registration);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNullRegistration_ReturnsFailure()
    {
        // Act
        var result = _validator.Validate(null);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidClientId_ReturnsFailure(string? clientId)
    {
        // Arrange
        var registration = CreateValidRpRegistration();
        registration.ClientId = clientId!;

        // Act
        var result = _validator.Validate(registration);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("client_id");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidOrganizationName_ReturnsFailure(string? orgName)
    {
        // Arrange
        var registration = CreateValidRpRegistration();
        registration.OrganizationName = orgName!;

        // Act
        var result = _validator.Validate(registration);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("organization");
    }

    #endregion

    #region Redirect URI Validation

    [Fact]
    public void Validate_WithValidRedirectUri_ReturnsSuccess()
    {
        // Arrange
        var registration = CreateValidRpRegistration();
        registration.RedirectUris = new[] { "https://rp.example.com/callback" };

        // Act
        var result = _validator.Validate(registration);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithHttpRedirectUri_ReturnsFailure()
    {
        // Arrange
        var registration = CreateValidRpRegistration();
        registration.RedirectUris = new[] { "http://insecure.example.com/callback" };

        // Act
        var result = _validator.Validate(registration);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("HTTPS");
    }

    [Fact]
    public void Validate_WithLocalhostUri_ReturnsSuccessInDevelopment()
    {
        // Arrange
        var registration = CreateValidRpRegistration();
        registration.RedirectUris = new[] { "http://localhost:8080/callback" };

        // Act
        var result = _validator.Validate(registration, allowLocalhost: true);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyRedirectUris_ReturnsFailure()
    {
        // Arrange
        var registration = CreateValidRpRegistration();
        registration.RedirectUris = Array.Empty<string>();

        // Act
        var result = _validator.Validate(registration);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("redirect");
    }

    #endregion

    #region Response Type Validation

    [Theory]
    [InlineData("vp_token")]
    [InlineData("vp_token id_token")]
    public void Validate_WithValidResponseType_ReturnsSuccess(string responseType)
    {
        // Arrange
        var registration = CreateValidRpRegistration();
        registration.ResponseTypes = new[] { responseType };

        // Act
        var result = _validator.Validate(registration);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("token")]
    [InlineData("code")]
    [InlineData("id_token")]
    public void Validate_WithInvalidResponseType_ReturnsFailure(string invalidResponseType)
    {
        // Arrange - EUDIW only supports vp_token response types
        var registration = CreateValidRpRegistration();
        registration.ResponseTypes = new[] { invalidResponseType };

        // Act
        var result = _validator.Validate(registration);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("response_type");
    }

    #endregion

    #region Status Enumeration

    [Fact]
    public void RpStatus_HasActiveStatus()
    {
        // Assert
        Enum.IsDefined(typeof(RpStatus), RpStatus.Active).Should().BeTrue();
    }

    [Fact]
    public void RpStatus_HasPendingStatus()
    {
        // Assert
        Enum.IsDefined(typeof(RpStatus), RpStatus.Pending).Should().BeTrue();
    }

    [Fact]
    public void RpStatus_HasSuspendedStatus()
    {
        // Assert
        Enum.IsDefined(typeof(RpStatus), RpStatus.Suspended).Should().BeTrue();
    }

    [Fact]
    public void RpStatus_HasRevokedStatus()
    {
        // Assert
        Enum.IsDefined(typeof(RpStatus), RpStatus.Revoked).Should().BeTrue();
    }

    #endregion

    #region Trust Framework Compliance

    [Fact]
    public void Validate_WithEuTrustFramework_ChecksTrustAnchor()
    {
        // Arrange
        var registration = CreateValidRpRegistration();
        registration.TrustFramework = "eu.eudiw.trust";

        // Act
        var result = _validator.Validate(registration);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithUnknownTrustFramework_ReturnsWarning()
    {
        // Arrange
        var registration = CreateValidRpRegistration();
        registration.TrustFramework = "unknown.framework";

        // Act
        var result = _validator.Validate(registration);

        // Assert
        result.Warnings.Should().Contain(w => w.Contains("trust framework"));
    }

    #endregion

    #region Helper Methods

    private static RpRegistration CreateValidRpRegistration()
    {
        return new RpRegistration
        {
            ClientId = "https://rp.example.com",
            OrganizationName = "Test Organization",
            RedirectUris = new[] { "https://rp.example.com/callback" },
            ResponseTypes = new[] { "vp_token" },
            Status = RpStatus.Active,
            TrustFramework = "eu.eudiw.trust"
        };
    }

    #endregion
}
