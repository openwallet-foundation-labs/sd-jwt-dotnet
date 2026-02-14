using FluentAssertions;
using SdJwt.Net.OidFederation.Models;
using Xunit;

namespace SdJwt.Net.OidFederation.Tests.Models;

public class TrustMarkTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateTrustMark()
    {
        // Arrange
        var id = "test-trust-mark";
        var value = "test-value";
        var issuer = "https://issuer.example.com";

        // Act
        var trustMark = TrustMark.Create(id, value, issuer);

        // Assert
        trustMark.Should().NotBeNull();
        trustMark.Id.Should().Be(id);
        trustMark.TrustMarkValue.Should().Be(value);
        trustMark.TrustMarkIssuer.Should().Be(issuer);
        trustMark.IssuedAt.Should().BeGreaterThan(0);
        trustMark.ExpiresAt.Should().NotBeNull();
        trustMark.ExpiresAt.Should().BeGreaterThan(trustMark.IssuedAt!.Value);
    }

    [Fact]
    public void Create_WithCustomValidityHours_ShouldSetCorrectExpiration()
    {
        // Arrange
        var id = "test-trust-mark";
        var value = "test-value";
        var issuer = "https://issuer.example.com";
        var validityHours = 48;

        // Act
        var trustMark = TrustMark.Create(id, value, issuer, validityHours);

        // Assert
        var expectedExpiry = trustMark.IssuedAt!.Value + (validityHours * 3600);
        trustMark.ExpiresAt.Should().Be(expectedExpiry);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidId_ShouldThrow(string? invalidId)
    {
        // Act & Assert
        var act = () => TrustMark.Create(invalidId!, "value", "https://issuer.example.com");
        act.Should().Throw<ArgumentException>()
           .WithParameterName("id");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidValue_ShouldThrow(string? invalidValue)
    {
        // Act & Assert
        var act = () => TrustMark.Create("id", invalidValue!, "https://issuer.example.com");
        act.Should().Throw<ArgumentException>()
           .WithParameterName("value");
    }

    [Theory]
    [InlineData("http://insecure.example.com")]
    [InlineData("ftp://example.com")]
    [InlineData("not-a-url")]
    public void Create_WithInvalidIssuer_ShouldThrow(string invalidIssuer)
    {
        // Act & Assert
        var act = () => TrustMark.Create("id", "value", invalidIssuer);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("issuer");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceIssuer_ShouldTreatAsNull(string emptyIssuer)
    {
        // Act & Assert - Empty/whitespace issuer is treated as null, which is allowed
        var act = () => TrustMark.Create("id", "value", emptyIssuer);
        act.Should().NotThrow();
    }

    [Fact]
    public void Create_WithNullIssuer_ShouldSucceed()
    {
        // Act & Assert - null issuer is allowed
        var act = () => TrustMark.Create("id", "value", null);
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithValidTrustMark_ShouldNotThrow()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");

        // Act & Assert
        var act = () => trustMark.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldThrow()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");
        trustMark.Id = "";

        // Act & Assert
        var act = () => trustMark.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Trust mark ID is required");
    }

    [Fact]
    public void Validate_WithEmptyValue_ShouldThrow()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");
        trustMark.TrustMarkValue = "";

        // Act & Assert
        var act = () => trustMark.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Trust mark value is required");
    }

    [Fact]
    public void Validate_WithInvalidIssuerUrl_ShouldThrow()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");
        trustMark.TrustMarkIssuer = "invalid-url";

        // Act & Assert
        var act = () => trustMark.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Trust mark issuer must be a valid HTTPS URL");
    }

    [Fact]
    public void Validate_WithExpiredTrustMark_ShouldThrow()
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

        // Act & Assert
        var act = () => trustMark.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Trust mark has expired");
    }

    [Fact]
    public void Validate_WithInvalidTimestamps_ShouldThrow()
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
    public void IsExpired_WithExpiredTrustMark_ShouldReturnTrue()
    {
        // Arrange
        var trustMark = new TrustMark
        {
            Id = "test-id",
            TrustMarkValue = "test-value",
            TrustMarkIssuer = "https://issuer.example.com",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds()
        };

        // Act
        var isExpired = trustMark.IsExpired();

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WithValidTrustMark_ShouldReturnFalse()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");

        // Act
        var isExpired = trustMark.IsExpired();

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithClockSkew_ShouldAccountForSkew()
    {
        // Arrange
        var trustMark = new TrustMark
        {
            Id = "test-id",
            TrustMarkValue = "test-value",
            TrustMarkIssuer = "https://issuer.example.com",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-3).ToUnixTimeSeconds() // Expired by 3 minutes
        };
        var clockSkew = TimeSpan.FromMinutes(5);

        // Act
        var isExpired = trustMark.IsExpired(clockSkew);

        // Assert
        isExpired.Should().BeFalse(); // Should not be considered expired due to clock skew
    }

    [Fact]
    public void ToString_ShouldReturnMeaningfulString()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");

        // Act
        var result = trustMark.ToString();

        // Assert
        result.Should().Contain("test-id");
        result.Should().Contain("https://issuer.example.com");
    }

    [Fact]
    public void Validate_WithValidSubject_ShouldNotThrow()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");
        trustMark.Subject = "https://subject.example.com";

        // Act & Assert
        var act = () => trustMark.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithInvalidSubjectUrl_ShouldThrow()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");
        trustMark.Subject = "invalid-subject-url";

        // Act & Assert
        var act = () => trustMark.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Trust mark subject must be a valid HTTPS URL");
    }

    [Theory]
    [InlineData("http://insecure-subject.example.com")]
    [InlineData("ftp://subject.example.com")]
    public void Validate_WithInsecureSubjectUrl_ShouldThrow(string invalidSubject)
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
    public void IsValid_WithValidTrustMark_ShouldReturnTrue()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");

        // Act
        var isValid = trustMark.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithInvalidTrustMark_ShouldReturnFalse()
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
    public void IsExpired_WithNullExpiresAt_ShouldReturnFalse()
    {
        // Arrange
        var trustMark = new TrustMark
        {
            Id = "test-id",
            TrustMarkValue = "test-value",
            TrustMarkIssuer = "https://issuer.example.com",
            ExpiresAt = null
        };

        // Act
        var isExpired = trustMark.IsExpired();

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithClockSkew_WithNullExpiresAt_ShouldReturnFalse()
    {
        // Arrange
        var trustMark = new TrustMark
        {
            Id = "test-id",
            TrustMarkValue = "test-value",
            TrustMarkIssuer = "https://issuer.example.com",
            ExpiresAt = null
        };
        var clockSkew = TimeSpan.FromMinutes(5);

        // Act
        var isExpired = trustMark.IsExpired(clockSkew);

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void Create_WithNoValidityHours_ShouldSetDefault24Hours()
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

    [Fact]
    public void TrustMarkIssuer_PropertyAlias_ShouldWorkCorrectly()
    {
        // Arrange
        var trustMark = new TrustMark();
        var testIssuer = "https://test-issuer.example.com";

        // Act
        trustMark.TrustMarkIssuer = testIssuer;

        // Assert
        trustMark.Issuer.Should().Be(testIssuer);
        trustMark.TrustMarkIssuer.Should().Be(testIssuer);
    }

    [Fact]
    public void AdditionalAttributes_ShouldAllowCustomData()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-id", "test-value", "https://issuer.example.com");
        var customData = new Dictionary<string, object>
        {
            ["custom_field"] = "custom_value",
            ["another_field"] = 123
        };

        // Act
        trustMark.AdditionalAttributes = customData;

        // Assert
        trustMark.AdditionalAttributes.Should().ContainKey("custom_field");
        trustMark.AdditionalAttributes!["custom_field"].Should().Be("custom_value");
        trustMark.AdditionalAttributes.Should().ContainKey("another_field");
        trustMark.AdditionalAttributes!["another_field"].Should().Be(123);
    }
}

public class CommonTrustMarksTests
{
    [Fact]
    public void CommonTrustMarks_ShouldHaveCorrectValues()
    {
        // Assert
        CommonTrustMarks.EidasCompliant.Should().Be("https://eidas.europa.eu/trustmark/compliant");
        CommonTrustMarks.SecurityCertified.Should().Be("https://security.gov/trustmark/certified");
        CommonTrustMarks.EducationalInstitution.Should().Be("https://education.gov/trustmark/institution");
        CommonTrustMarks.FinancialInstitution.Should().Be("https://finance.gov/trustmark/institution");
        CommonTrustMarks.HealthcareProvider.Should().Be("https://healthcare.gov/trustmark/provider");
        CommonTrustMarks.GovernmentAgency.Should().Be("https://government.gov/trustmark/agency");
    }
}
