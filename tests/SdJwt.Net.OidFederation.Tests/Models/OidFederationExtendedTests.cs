using FluentAssertions;
using SdJwt.Net.OidFederation.Models;
using Xunit;

namespace SdJwt.Net.OidFederation.Tests.Models;

/// <summary>
/// Extended test coverage for OidFederation models to ensure comprehensive coverage.
/// </summary>
public class OidFederationExtendedTests
{
    [Fact]
    public void TrustMark_ToString_ShouldReturnMeaningfulString()
    {
        // Arrange
        var trustMark = TrustMark.Create(
            "security-certified",
            "trusted-issuer",
            "https://security.gov/issuer");

        // Act
        var result = trustMark.ToString();

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("security-certified");
        result.Should().Contain("https://security.gov/issuer");
    }

    [Fact]
    public void TrustMark_TrustMarkIssuer_PropertyAlias_ShouldWorkCorrectly()
    {
        // Arrange
        var trustMark = new TrustMark();
        var issuer = "https://test-issuer.example.com";

        // Act
        trustMark.TrustMarkIssuer = issuer;

        // Assert
        trustMark.Issuer.Should().Be(issuer);
        trustMark.TrustMarkIssuer.Should().Be(issuer);
    }

    [Fact]
    public void TrustMark_AdditionalAttributes_ShouldAllowCustomData()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");
        var customData = new Dictionary<string, object>
        {
            ["custom_field"] = "custom_value",
            ["numeric_field"] = 42,
            ["boolean_field"] = true,
            ["nested_object"] = new { prop1 = "value1", prop2 = 123 }
        };

        // Act
        trustMark.AdditionalAttributes = customData;

        // Assert
        trustMark.AdditionalAttributes.Should().NotBeNull();
        trustMark.AdditionalAttributes!["custom_field"].Should().Be("custom_value");
        trustMark.AdditionalAttributes!["numeric_field"].Should().Be(42);
        trustMark.AdditionalAttributes!["boolean_field"].Should().Be(true);
        trustMark.AdditionalAttributes!["nested_object"].Should().NotBeNull();
    }

    [Fact]
    public void TrustMark_Create_WithNullIssuer_ShouldAllowNullIssuer()
    {
        // Act
        var trustMark = TrustMark.Create("test-id", "test-value", null);

        // Assert
        trustMark.Should().NotBeNull();
        trustMark.Id.Should().Be("test-id");
        trustMark.TrustMarkValue.Should().Be("test-value");
        trustMark.TrustMarkIssuer.Should().BeNull();
        trustMark.IssuedAt.Should().BeGreaterThan(0);
        trustMark.ExpiresAt.Should().BeGreaterThan(trustMark.IssuedAt!.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TrustMark_Create_WithEmptyOrWhitespaceIssuer_ShouldTreatAsNull(string emptyIssuer)
    {
        // Act
        var trustMark = TrustMark.Create("test-id", "test-value", emptyIssuer);

        // Assert
        trustMark.Should().NotBeNull();
        trustMark.TrustMarkIssuer.Should().BeNull();
    }

    [Fact]
    public void TrustMark_IsExpired_WithNullExpiresAt_ShouldReturnFalse()
    {
        // Arrange
        var trustMark = new TrustMark
        {
            Id = "test-id",
            TrustMarkValue = "test-value",
            TrustMarkIssuer = "https://issuer.example.com",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = null
        };

        // Act
        var isExpired = trustMark.IsExpired();

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void TrustMark_IsExpired_WithClockSkew_AndNullExpiresAt_ShouldReturnFalse()
    {
        // Arrange
        var trustMark = new TrustMark
        {
            Id = "test-id",
            TrustMarkValue = "test-value",
            TrustMarkIssuer = "https://issuer.example.com",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = null
        };
        var clockSkew = TimeSpan.FromMinutes(10);

        // Act
        var isExpired = trustMark.IsExpired(clockSkew);

        // Assert
        isExpired.Should().BeFalse();
    }

    [Theory]
    [InlineData(5)]    // 5 minutes after expiry
    [InlineData(60)]   // 1 hour after expiry
    [InlineData(1440)] // 1 day after expiry
    public void TrustMark_IsExpired_WithVariousExpiredTimes_ShouldReturnTrue(int minutesAfterExpiry)
    {
        // Arrange
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(-minutesAfterExpiry);
        var trustMark = new TrustMark
        {
            Id = "test-id",
            TrustMarkValue = "test-value",
            TrustMarkIssuer = "https://issuer.example.com",
            IssuedAt = expiresAt.AddHours(-24).ToUnixTimeSeconds(),
            ExpiresAt = expiresAt.ToUnixTimeSeconds()
        };

        // Act
        var isExpired = trustMark.IsExpired();

        // Assert
        isExpired.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]    // 1 minute before expiry
    [InlineData(30)]   // 30 minutes before expiry
    [InlineData(120)]  // 2 hours before expiry
    public void TrustMark_IsExpired_WithVariousValidTimes_ShouldReturnFalse(int minutesBeforeExpiry)
    {
        // Arrange
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(minutesBeforeExpiry);
        var trustMark = new TrustMark
        {
            Id = "test-id",
            TrustMarkValue = "test-value",
            TrustMarkIssuer = "https://issuer.example.com",
            IssuedAt = expiresAt.AddHours(-24).ToUnixTimeSeconds(),
            ExpiresAt = expiresAt.ToUnixTimeSeconds()
        };

        // Act
        var isExpired = trustMark.IsExpired();

        // Assert
        isExpired.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 5)]   // Expired 1 min ago, 5 min clock skew -> Not expired
    [InlineData(3, 5)]   // Expired 3 min ago, 5 min clock skew -> Not expired  
    [InlineData(7, 5)]   // Expired 7 min ago, 5 min clock skew -> Expired
    [InlineData(10, 5)]  // Expired 10 min ago, 5 min clock skew -> Expired
    public void TrustMark_IsExpired_WithClockSkew_ShouldAccountForSkewCorrectly(int minutesAfterExpiry, int clockSkewMinutes)
    {
        // Arrange
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(-minutesAfterExpiry);
        var trustMark = new TrustMark
        {
            Id = "test-id",
            TrustMarkValue = "test-value",
            TrustMarkIssuer = "https://issuer.example.com",
            IssuedAt = expiresAt.AddHours(-24).ToUnixTimeSeconds(),
            ExpiresAt = expiresAt.ToUnixTimeSeconds()
        };
        var clockSkew = TimeSpan.FromMinutes(clockSkewMinutes);

        // Act
        var isExpired = trustMark.IsExpired(clockSkew);

        // Assert
        var shouldBeExpired = minutesAfterExpiry > clockSkewMinutes;
        isExpired.Should().Be(shouldBeExpired);
    }

    [Fact]
    public void TrustMark_IsValid_WithValidTrustMark_ShouldReturnTrue()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");

        // Act
        var isValid = trustMark.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void TrustMark_IsValid_WithInvalidTrustMark_ShouldReturnFalse()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");
        trustMark.Id = ""; // Make it invalid

        // Act
        var isValid = trustMark.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void TrustMark_IsValid_WithExpiredTrustMark_ShouldReturnFalse()
    {
        // Arrange
        var trustMark = new TrustMark
        {
            Id = "test-id",
            TrustMarkValue = "test-value", 
            TrustMarkIssuer = "https://issuer.example.com",
            IssuedAt = DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds() // Expired 1 hour ago
        };

        // Act
        var isValid = trustMark.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void TrustMark_Create_WithNoValidityHours_ShouldSetDefault24Hours()
    {
        // Arrange
        var id = "test-trust-mark";
        var value = "test-value";
        var issuer = "https://issuer.example.com";

        // Act
        var trustMark = TrustMark.Create(id, value, issuer);

        // Assert
        var expectedExpiry = trustMark.IssuedAt!.Value + (24 * 3600); // Default 24 hours
        trustMark.ExpiresAt.Should().Be(expectedExpiry);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(168)] // 1 week
    [InlineData(8760)] // 1 year
    public void TrustMark_Create_WithCustomValidityHours_ShouldSetCorrectExpiration(int validityHours)
    {
        // Arrange
        var id = "test-trust-mark";
        var value = "test-value";
        var issuer = "https://issuer.example.com";

        // Act
        var trustMark = TrustMark.Create(id, value, issuer, validityHours);

        // Assert
        var expectedExpiry = trustMark.IssuedAt!.Value + (validityHours * 3600);
        trustMark.ExpiresAt.Should().Be(expectedExpiry);
    }

    [Fact]
    public void TrustMark_Validate_WithValidSubject_ShouldNotThrow()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");
        trustMark.Subject = "https://subject.example.com";

        // Act & Assert
        var act = () => trustMark.Validate();
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("invalid-subject-url")]
    [InlineData("not a url at all")]
    [InlineData("file://local/path")]
    public void TrustMark_Validate_WithInvalidSubjectUrl_ShouldThrow(string invalidSubject)
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");
        trustMark.Subject = invalidSubject;

        // Act & Assert
        var act = () => trustMark.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Trust mark subject must be a valid HTTPS URL");
    }

    [Theory]
    [InlineData("http://insecure-subject.example.com")]
    [InlineData("ftp://subject.example.com")]
    [InlineData("mailto:subject@example.com")]
    public void TrustMark_Validate_WithInsecureSubjectUrl_ShouldThrow(string invalidSubject)
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");
        trustMark.Subject = invalidSubject;

        // Act & Assert
        var act = () => trustMark.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Trust mark subject must be a valid HTTPS URL");
    }

    [Fact]
    public void TrustMark_Validate_WithNullSubject_ShouldNotThrow()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");
        trustMark.Subject = null;

        // Act & Assert
        var act = () => trustMark.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void TrustMark_Validate_WithEmptySubject_ShouldNotThrow()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");
        trustMark.Subject = "";

        // Act & Assert
        var act = () => trustMark.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void CommonTrustMarks_ShouldHaveCorrectValues()
    {
        // Assert - Verify all common trust mark constants
        CommonTrustMarks.EidasCompliant.Should().Be("https://eidas.europa.eu/trustmark/compliant");
        CommonTrustMarks.SecurityCertified.Should().Be("https://security.gov/trustmark/certified");
        CommonTrustMarks.EducationalInstitution.Should().Be("https://education.gov/trustmark/institution");
        CommonTrustMarks.FinancialInstitution.Should().Be("https://finance.gov/trustmark/institution");
        CommonTrustMarks.HealthcareProvider.Should().Be("https://healthcare.gov/trustmark/provider");
        CommonTrustMarks.GovernmentAgency.Should().Be("https://government.gov/trustmark/agency");
    }

    [Fact]
    public void TrustMark_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var trustMark = new TrustMark();

        // Assert
        trustMark.Id.Should().BeNull();
        trustMark.TrustMarkValue.Should().BeNull();
        trustMark.TrustMarkIssuer.Should().BeNull();
        trustMark.Subject.Should().BeNull();
        trustMark.IssuedAt.Should().BeNull();
        trustMark.ExpiresAt.Should().BeNull();
        trustMark.AdditionalAttributes.Should().BeNull();
    }

    [Fact]
    public void TrustMark_Properties_ShouldBeSettable()
    {
        // Arrange
        var trustMark = new TrustMark();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Act
        trustMark.Id = "custom-id";
        trustMark.TrustMarkValue = "custom-value";
        trustMark.TrustMarkIssuer = "https://custom-issuer.example.com";
        trustMark.Subject = "https://custom-subject.example.com";
        trustMark.IssuedAt = now;
        trustMark.ExpiresAt = now + 3600;

        // Assert
        trustMark.Id.Should().Be("custom-id");
        trustMark.TrustMarkValue.Should().Be("custom-value");
        trustMark.TrustMarkIssuer.Should().Be("https://custom-issuer.example.com");
        trustMark.Subject.Should().Be("https://custom-subject.example.com");
        trustMark.IssuedAt.Should().Be(now);
        trustMark.ExpiresAt.Should().Be(now + 3600);
    }

    [Fact]
    public void TrustMark_Create_WithZeroValidityHours_ShouldStillSetExpiration()
    {
        // Arrange
        var id = "test-id";
        var value = "test-value";
        var issuer = "https://issuer.example.com";

        // Act
        var trustMark = TrustMark.Create(id, value, issuer, 0);

        // Assert
        trustMark.ExpiresAt.Should().Be(trustMark.IssuedAt); // Should expire immediately
    }

    [Fact]
    public void TrustMark_Create_WithNegativeValidityHours_ShouldCreateExpiredTrustMark()
    {
        // Arrange
        var id = "test-id";
        var value = "test-value";
        var issuer = "https://issuer.example.com";

        // Act
        var trustMark = TrustMark.Create(id, value, issuer, -1);

        // Assert
        trustMark.ExpiresAt.Should().BeLessThan(trustMark.IssuedAt!.Value);
        trustMark.IsExpired().Should().BeTrue();
    }

    [Theory]
    [InlineData("http://insecure.example.com")]
    [InlineData("ftp://example.com")]
    [InlineData("not-a-url")]
    public void TrustMark_Create_WithInvalidIssuer_ShouldThrow(string invalidIssuer)
    {
        // Act & Assert
        var act = () => TrustMark.Create("id", "value", invalidIssuer);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("issuer");
    }

    [Fact]
    public void TrustMark_Validate_WithInvalidTimestamps_ShouldThrow()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");
        trustMark.ExpiresAt = trustMark.IssuedAt!.Value - 100; // Expires before issued

        // Act & Assert
        var act = () => trustMark.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("ExpiresAt must be after IssuedAt");
    }

    [Fact]
    public void TrustMark_Validate_WithNullIssuedAt_ShouldNotValidateTimestamps()
    {
        // Arrange
        var trustMark = new TrustMark
        {
            Id = "test-id",
            TrustMarkValue = "test-value",
            TrustMarkIssuer = "https://issuer.example.com",
            IssuedAt = null,
            ExpiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        // Act & Assert - Should not throw because IssuedAt is null
        var act = () => trustMark.Validate();
        act.Should().NotThrow();
    }
}
