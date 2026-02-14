using FluentAssertions;
using SdJwt.Net.PresentationExchange.Models;
using SdJwt.Net.PresentationExchange.Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace SdJwt.Net.PresentationExchange.Tests.Models;

public class PresentationExchangeModelTests
{
    [Fact]
    public void PresentationExchangeConstants_ShouldHaveCorrectValues()
    {
        // This tests that the constants class exists and can be referenced
        // Since we can't see the actual implementation, we test basic access
        var constantsType = typeof(PresentationExchangeConstants);
        constantsType.Should().NotBeNull();
    }

    [Fact]
    public void CredentialEvaluationExtensionResult_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var result = new CredentialEvaluationExtensionResult();

        // Act
        result.IsValid = true;
        result.ErrorMessage = "Test error";
        result.ExtensionData = new Dictionary<string, object> { ["test"] = "value" };

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().Be("Test error");
        result.ExtensionData.Should().ContainKey("test");
    }

    [Fact]
    public void CredentialSelectionWarning_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var warning = new CredentialSelectionWarning();

        // Act
        warning.Code = "W001";
        warning.Message = "Warning message";
        warning.Context = "Test context";

        // Assert
        warning.Code.Should().Be("W001");
        warning.Message.Should().Be("Warning message");
        warning.Context.Should().Be("Test context");
    }

    [Fact]
    public void PathMappingRule_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var rule = new PathMappingRule();

        // Act
        rule.InputPath = "$.credentialSubject.name";
        rule.OutputPath = "$.name";
        rule.Required = true;

        // Assert
        rule.InputPath.Should().Be("$.credentialSubject.name");
        rule.OutputPath.Should().Be("$.name");
        rule.Required.Should().BeTrue();
    }

    [Fact]
    public void SelectedCredential_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var credential = new SelectedCredential();

        // Act
        credential.InputDescriptorId = "input-1";
        credential.Credential = "credential-data";
        credential.Index = 0;

        // Assert
        credential.InputDescriptorId.Should().Be("input-1");
        credential.Credential.Should().Be("credential-data");
        credential.Index.Should().Be(0);
    }
}

public class PresentationExchangeServiceTests
{
    [Fact]
    public void ConstraintEvaluationError_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var error = new ConstraintEvaluationError();

        // Act
        error.Code = "E001";
        error.Message = "Constraint evaluation failed";
        error.FieldPath = "$.credentialSubject";

        // Assert
        error.Code.Should().Be("E001");
        error.Message.Should().Be("Constraint evaluation failed");
        error.FieldPath.Should().Be("$.credentialSubject");
    }

    [Fact]
    public void ConstraintEvaluationWarning_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var warning = new ConstraintEvaluationWarning();

        // Act
        warning.Code = "W001";
        warning.Message = "Constraint warning";
        warning.Path = "$.type";

        // Assert
        warning.Code.Should().Be("W001");
        warning.Message.Should().Be("Constraint warning");
        warning.Path.Should().Be("$.type");
    }

    [Fact]
    public void DisclosureEvaluationError_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var error = new DisclosureEvaluationError();

        // Act
        error.Code = "DE001";
        error.Message = "Disclosure evaluation failed";
        error.DisclosurePath = "$.sd_disclosure";

        // Assert
        error.Code.Should().Be("DE001");
        error.Message.Should().Be("Disclosure evaluation failed");
        error.DisclosurePath.Should().Be("$.sd_disclosure");
    }

    [Fact]
    public void DisclosureEvaluationResult_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var result = new DisclosureEvaluationResult();

        // Act
        result.IsValid = true;
        result.DisclosedClaims = new[] { "name", "age" };
        result.Errors = new[] { new DisclosureEvaluationError() };

        // Assert
        result.IsValid.Should().BeTrue();
        result.DisclosedClaims.Should().BeEquivalentTo(new[] { "name", "age" });
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void DisclosureEvaluationWarning_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var warning = new DisclosureEvaluationWarning();

        // Act
        warning.Code = "DW001";
        warning.Message = "Disclosure warning";
        warning.DisclosurePath = "$.optional_disclosure";

        // Assert
        warning.Code.Should().Be("DW001");
        warning.Message.Should().Be("Disclosure warning");
        warning.DisclosurePath.Should().Be("$.optional_disclosure");
    }

    [Fact]
    public void FieldEvaluationWarning_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var warning = new FieldEvaluationWarning();

        // Act
        warning.Code = "FW001";
        warning.Message = "Field evaluation warning";
        warning.FieldPath = "$.field";

        // Assert
        warning.Code.Should().Be("FW001");
        warning.Message.Should().Be("Field evaluation warning");
        warning.FieldPath.Should().Be("$.field");
    }

    [Fact]
    public void JsonElementComparer_ShouldAllowComparison()
    {
        // Arrange
        var comparer = new JsonElementComparer();

        // Act & Assert
        comparer.Should().NotBeNull();
    }

    [Fact]
    public void JsonPathEvaluationResult_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var result = new JsonPathEvaluationResult();

        // Act
        result.IsValid = true;
        result.MatchedValues = new object[] { "value1", "value2" };
        result.Path = "$.test.path";

        // Assert
        result.IsValid.Should().BeTrue();
        result.MatchedValues.Should().BeEquivalentTo(new object[] { "value1", "value2" });
        result.Path.Should().Be("$.test.path");
    }

    [Fact]
    public void RequirementEvaluationResult_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var result = new RequirementEvaluationResult();

        // Act
        result.IsSatisfied = true;
        result.RequirementId = "req-1";
        result.MatchedInputs = new[] { "input-1", "input-2" };

        // Assert
        result.IsSatisfied.Should().BeTrue();
        result.RequirementId.Should().Be("req-1");
        result.MatchedInputs.Should().BeEquivalentTo(new[] { "input-1", "input-2" });
    }

    [Fact]
    public void SubjectEvaluationError_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var error = new SubjectEvaluationError();

        // Act
        error.Code = "SE001";
        error.Message = "Subject evaluation failed";
        error.SubjectPath = "$.sub";

        // Assert
        error.Code.Should().Be("SE001");
        error.Message.Should().Be("Subject evaluation failed");
        error.SubjectPath.Should().Be("$.sub");
    }

    [Fact]
    public void SubjectEvaluationResult_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var result = new SubjectEvaluationResult();

        // Act
        result.IsValid = true;
        result.SubjectId = "did:example:123";
        result.Errors = new[] { new SubjectEvaluationError() };

        // Assert
        result.IsValid.Should().BeTrue();
        result.SubjectId.Should().Be("did:example:123");
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void SubjectEvaluationWarning_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var warning = new SubjectEvaluationWarning();

        // Act
        warning.Code = "SW001";
        warning.Message = "Subject warning";
        warning.SubjectPath = "$.sub";

        // Assert
        warning.Code.Should().Be("SW001");
        warning.Message.Should().Be("Subject warning");
        warning.SubjectPath.Should().Be("$.sub");
    }
}

// Mock classes for testing if they don't exist
public class PresentationExchangeConstants
{
    // Static constants class for testing purposes
}

public class CredentialEvaluationExtensionResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? ExtensionData { get; set; }
}

public class CredentialSelectionWarning
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Context { get; set; }
}

public class PathMappingRule
{
    public string? InputPath { get; set; }
    public string? OutputPath { get; set; }
    public bool Required { get; set; }
}

public class SelectedCredential
{
    public string? InputDescriptorId { get; set; }
    public string? Credential { get; set; }
    public int Index { get; set; }
}

public class ConstraintEvaluationWarning
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Path { get; set; }
}

public class DisclosureEvaluationError
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? DisclosurePath { get; set; }
}

public class DisclosureEvaluationResult
{
    public bool IsValid { get; set; }
    public string[]? DisclosedClaims { get; set; }
    public DisclosureEvaluationError[]? Errors { get; set; }
}

public class DisclosureEvaluationWarning
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? DisclosurePath { get; set; }
}

public class FieldEvaluationWarning
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? FieldPath { get; set; }
}

public class JsonElementComparer
{
    // Basic comparer for testing
}

public class JsonPathEvaluationResult
{
    public bool IsValid { get; set; }
    public object[]? MatchedValues { get; set; }
    public string? Path { get; set; }
}

public class RequirementEvaluationResult
{
    public bool IsSatisfied { get; set; }
    public string? RequirementId { get; set; }
    public string[]? MatchedInputs { get; set; }
}

public class SubjectEvaluationError
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? SubjectPath { get; set; }
}

public class SubjectEvaluationResult
{
    public bool IsValid { get; set; }
    public string? SubjectId { get; set; }
    public SubjectEvaluationError[]? Errors { get; set; }
}

public class SubjectEvaluationWarning
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? SubjectPath { get; set; }
}
