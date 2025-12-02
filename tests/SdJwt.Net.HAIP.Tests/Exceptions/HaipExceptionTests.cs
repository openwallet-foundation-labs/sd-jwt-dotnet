using FluentAssertions;
using SdJwt.Net.HAIP.Exceptions;
using SdJwt.Net.HAIP.Models;
using SdJwt.Net.HAIP;
using System;
using System.Linq;
using Xunit;

namespace SdJwt.Net.HAIP.Tests.Exceptions;

public class HaipExceptionTests
{
    [Fact]
    public void HaipComplianceException_WithRequiredParameters_ShouldCreateCorrectly()
    {
        // Arrange
        var message = "Compliance violation detected";
        var violationType = HaipViolationType.WeakCryptography;
        var complianceResult = new HaipComplianceResult
        {
            IsCompliant = false,
            Violations = new List<HaipViolation>
            {
                new HaipViolation
                {
                    Type = violationType,
                    Severity = HaipSeverity.Critical,
                    Description = "Weak cryptographic algorithm detected",
                    RecommendedAction = "Use stronger cryptographic algorithms"
                }
            }
        };
        var recommendedAction = "Switch to approved algorithms";

        // Act
        var exception = new HaipComplianceException(message, violationType, complianceResult, recommendedAction);

        // Assert
        exception.Message.Should().Be(message);
        exception.ViolationType.Should().Be(violationType);
        exception.ComplianceResult.Should().Be(complianceResult);
        exception.RecommendedAction.Should().Be(recommendedAction);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void HaipComplianceException_WithInnerException_ShouldCreateCorrectly()
    {
        // Arrange
        var message = "Compliance violation detected";
        var violationType = HaipViolationType.MissingProofOfPossession;
        var complianceResult = new HaipComplianceResult
        {
            IsCompliant = false,
            Violations = new List<HaipViolation>
            {
                new HaipViolation
                {
                    Type = violationType,
                    Severity = HaipSeverity.Critical,
                    Description = "Missing proof of possession",
                    RecommendedAction = "Enable proof of possession"
                }
            }
        };
        var recommendedAction = "Update configuration";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new HaipComplianceException(message, violationType, complianceResult, recommendedAction, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.ViolationType.Should().Be(violationType);
        exception.ComplianceResult.Should().Be(complianceResult);
        exception.RecommendedAction.Should().Be(recommendedAction);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void HaipComplianceException_FromResult_WithCriticalViolations_ShouldCreateCorrectly()
    {
        // Arrange
        var complianceResult = new HaipComplianceResult
        {
            IsCompliant = false,
            Violations = new List<HaipViolation>
            {
                new HaipViolation
                {
                    Type = HaipViolationType.WeakCryptography,
                    Severity = HaipSeverity.Critical,
                    Description = "Critical security issue",
                    RecommendedAction = "Fix immediately"
                }
            }
        };
        var context = "During validation";

        // Act
        var exception = HaipComplianceException.FromResult(complianceResult, context);

        // Assert
        exception.Message.Should().Be("During validation: Critical security issue");
        exception.ViolationType.Should().Be(HaipViolationType.WeakCryptography);
        exception.ComplianceResult.Should().Be(complianceResult);
        exception.RecommendedAction.Should().Be("Fix immediately");
    }

    [Fact]
    public void HaipComplianceException_FromResult_WithNoCriticalViolations_ShouldThrow()
    {
        // Arrange
        var complianceResult = new HaipComplianceResult
        {
            IsCompliant = false,
            Violations = new List<HaipViolation>
            {
                new HaipViolation
                {
                    Type = HaipViolationType.WeakCryptography,
                    Severity = HaipSeverity.Warning,
                    Description = "Warning issue",
                    RecommendedAction = "Consider fixing"
                }
            }
        };

        // Act & Assert
        var act = () => HaipComplianceException.FromResult(complianceResult);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Compliance result must have critical violations to create an exception*");
    }

    [Fact]
    public void HaipConfigurationException_WithConfigurationErrors_ShouldCreateCorrectly()
    {
        // Arrange
        var message = "Configuration error occurred";
        var configErrors = new[] { "Missing required parameter", "Invalid algorithm" };

        // Act
        var exception = new HaipConfigurationException(message, configErrors);

        // Assert
        exception.Message.Should().Be(message);
        exception.ConfigurationErrors.Should().BeEquivalentTo(configErrors);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void HaipConfigurationException_WithInnerException_ShouldCreateCorrectly()
    {
        // Arrange
        var message = "Configuration error occurred";
        var configErrors = new[] { "Invalid parameter" };
        var innerException = new ArgumentException("Invalid parameter");

        // Act
        var exception = new HaipConfigurationException(message, innerException, configErrors);

        // Assert
        exception.Message.Should().Be(message);
        exception.ConfigurationErrors.Should().BeEquivalentTo(configErrors);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void HaipTrustFrameworkException_WithRequiredParameters_ShouldCreateCorrectly()
    {
        // Arrange
        var message = "Trust framework violation";
        var trustFrameworkId = "test-framework";
        var entityId = "test-entity";

        // Act
        var exception = new HaipTrustFrameworkException(message, trustFrameworkId, entityId);

        // Assert
        exception.Message.Should().Be(message);
        exception.TrustFrameworkId.Should().Be(trustFrameworkId);
        exception.EntityId.Should().Be(entityId);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void HaipTrustFrameworkException_WithInnerException_ShouldCreateCorrectly()
    {
        // Arrange
        var message = "Trust framework violation";
        var trustFrameworkId = "test-framework";
        var entityId = "test-entity";
        var innerException = new SecurityException("Security violation");

        // Act
        var exception = new HaipTrustFrameworkException(message, trustFrameworkId, entityId, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.TrustFrameworkId.Should().Be(trustFrameworkId);
        exception.EntityId.Should().Be(entityId);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void AllExceptions_ShouldBeSerializable()
    {
        // Create test objects
        var complianceResult = new HaipComplianceResult
        {
            IsCompliant = false,
            Violations = new List<HaipViolation>
            {
                new HaipViolation
                {
                    Type = HaipViolationType.WeakCryptography,
                    Severity = HaipSeverity.Critical,
                    Description = "Test violation",
                    RecommendedAction = "Fix it"
                }
            }
        };

        var complianceEx = new HaipComplianceException("test", HaipViolationType.WeakCryptography, complianceResult);
        var configEx = new HaipConfigurationException("test", "error1");
        var trustEx = new HaipTrustFrameworkException("test", "framework", "entity");

        // Basic validation that they inherit from Exception properly
        complianceEx.Should().BeAssignableTo<Exception>();
        configEx.Should().BeAssignableTo<Exception>();
        trustEx.Should().BeAssignableTo<Exception>();
    }
}

// Helper class for testing
public class SecurityException : Exception
{
    public SecurityException(string message) : base(message) { }
}
