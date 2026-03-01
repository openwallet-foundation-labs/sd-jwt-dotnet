using FluentAssertions;
using SdJwt.Net.Oid4Vp.DcApi;
using SdJwt.Net.Oid4Vp.DcApi.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.DcApi;

/// <summary>
/// Tests for DcApiResponseValidator following TDD methodology.
/// </summary>
public class DcApiResponseValidatorTests
{
    #region Basic Validation Tests

    [Fact]
    public async Task ValidateAsync_WithValidResponse_ReturnsSuccess()
    {
        // Arrange
        var validator = CreateValidator();
        var response = CreateValidResponse();
        var options = CreateValidOptions();

        // Act
        var result = await validator.ValidateAsync(response, options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_WithMismatchedOrigin_ReturnsOriginMismatchError()
    {
        // Arrange
        var validator = CreateValidator();
        var response = CreateValidResponse("https://malicious.example.com");
        var options = CreateValidOptions();

        // Act
        var result = await validator.ValidateAsync(response, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("origin_mismatch");
    }

    [Fact]
    public async Task ValidateAsync_WithMismatchedNonce_ReturnsNonceMismatchError()
    {
        // Arrange
        var validator = CreateValidator();
        var response = CreateValidResponse();
        var options = CreateValidOptions();
        options.ExpectedNonce = "different-nonce";

        // Act
        var result = await validator.ValidateAsync(response, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("nonce_mismatch");
    }

    [Fact]
    public async Task ValidateAsync_WithNullResponse_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = CreateValidator();
        var options = CreateValidOptions();

        // Act
        var act = async () => await validator.ValidateAsync(null!, options);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ValidateAsync_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = CreateValidator();
        var response = CreateValidResponse();

        // Act
        var act = async () => await validator.ValidateAsync(response, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Origin Validation Tests

    [Fact]
    public async Task ValidateAsync_WithOriginValidationDisabled_SkipsOriginCheck()
    {
        // Arrange
        var validator = CreateValidator();
        var response = CreateValidResponse("https://different-origin.example.com");
        var options = CreateValidOptions();
        options.ValidateOrigin = false;

        // Act
        var result = await validator.ValidateAsync(response, options);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Expiry Validation Tests

    [Fact]
    public async Task ValidateAsync_WithExpiredPresentation_ReturnsExpiredError()
    {
        // Arrange
        var validator = CreateValidator();
        var response = CreateResponseWithExpiredPresentation();
        var options = CreateValidOptions();
        options.MaxAge = TimeSpan.FromMinutes(5);

        // Act
        var result = await validator.ValidateAsync(response, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("presentation_expired");
    }

    #endregion

    #region Result Model Tests

    [Fact]
    public void DcApiValidationResult_Success_CreatesValidResult()
    {
        // Act
        var result = DcApiValidationResult.Success(
            new List<VerifiedCredential> { new VerifiedCredential { Type = "TestCredential" } },
            null);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Error.Should().BeNull();
        result.ErrorCode.Should().BeNull();
        result.VerifiedCredentials.Should().HaveCount(1);
    }

    [Fact]
    public void DcApiValidationResult_Failure_CreatesInvalidResult()
    {
        // Act
        var result = DcApiValidationResult.Failure("Test error", "test_error");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Be("Test error");
        result.ErrorCode.Should().Be("test_error");
        result.VerifiedCredentials.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private static DcApiResponseValidator CreateValidator()
    {
        // Create a mock VP token validator
        return new DcApiResponseValidator(null!); // Will inject real validator in implementation
    }

    private static DcApiResponse CreateValidResponse(string? origin = null)
    {
        return new DcApiResponse
        {
            Protocol = DcApiConstants.Protocol,
            Origin = origin ?? "https://verifier.example.com",
            VpToken = "eyJ...", // Placeholder token
            Nonce = "test-nonce-123"
        };
    }

    private static DcApiResponse CreateResponseWithExpiredPresentation()
    {
        return new DcApiResponse
        {
            Protocol = DcApiConstants.Protocol,
            Origin = "https://verifier.example.com",
            VpToken = "eyJ...", // Expired token
            Nonce = "test-nonce-123",
            IssuedAt = DateTimeOffset.UtcNow.AddMinutes(-30)
        };
    }

    private static DcApiValidationOptions CreateValidOptions()
    {
        return new DcApiValidationOptions
        {
            ExpectedOrigin = "https://verifier.example.com",
            ExpectedNonce = "test-nonce-123",
            ValidateOrigin = true,
            MaxAge = TimeSpan.FromMinutes(10)
        };
    }

    #endregion
}
