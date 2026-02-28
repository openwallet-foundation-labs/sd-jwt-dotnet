using System.Text.Json;
using SdJwt.Net.PresentationExchange.Services;
using Xunit;

namespace SdJwt.Net.PresentationExchange.Tests;

/// <summary>
/// Tests for evaluation result classes to improve code coverage.
/// </summary>
public class EvaluationResultCoverageTests
{
    #region ConstraintEvaluationResult Tests

    [Fact]
    public void ConstraintEvaluationResult_DefaultState_IsSuccessfulTrue()
    {
        var result = new ConstraintEvaluationResult();

        Assert.False(result.IsSuccessful);
        Assert.Empty(result.FieldResults);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
        Assert.Null(result.DisclosureResult);
        Assert.Null(result.SubjectResult);
        Assert.Empty(result.Metadata);
    }

    [Fact]
    public void ConstraintEvaluationResult_AddError_SetsIsSuccessfulFalse()
    {
        var result = new ConstraintEvaluationResult { IsSuccessful = true };

        result.AddError("ERR001", "Test error message", "$.field.path");

        Assert.False(result.IsSuccessful);
        Assert.Single(result.Errors);
        Assert.Equal("ERR001", result.Errors[0].Code);
        Assert.Equal("Test error message", result.Errors[0].Message);
        Assert.Equal("$.field.path", result.Errors[0].FieldPath);
    }

    [Fact]
    public void ConstraintEvaluationResult_AddError_WithoutFieldPath()
    {
        var result = new ConstraintEvaluationResult { IsSuccessful = true };

        result.AddError("ERR002", "Error without path");

        Assert.False(result.IsSuccessful);
        Assert.Single(result.Errors);
        Assert.Null(result.Errors[0].FieldPath);
    }

    [Fact]
    public void ConstraintEvaluationResult_AddWarning_PreservesSuccessState()
    {
        var result = new ConstraintEvaluationResult { IsSuccessful = true };

        result.AddWarning("WARN001", "Test warning", "$.warning.path");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Warnings);
        Assert.Equal("WARN001", result.Warnings[0].Code);
        Assert.Equal("Test warning", result.Warnings[0].Message);
        Assert.Equal("$.warning.path", result.Warnings[0].FieldPath);
    }

    [Fact]
    public void ConstraintEvaluationResult_AddWarning_WithoutFieldPath()
    {
        var result = new ConstraintEvaluationResult { IsSuccessful = true };

        result.AddWarning("WARN002", "Warning without path");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Warnings);
        Assert.Null(result.Warnings[0].FieldPath);
    }

    [Fact]
    public void ConstraintEvaluationResult_MergeFieldResult_Success()
    {
        var result = new ConstraintEvaluationResult { IsSuccessful = true };
        var fieldResult = new FieldEvaluationResult { IsSuccessful = true };

        result.MergeFieldResult("$.name", fieldResult);

        Assert.Single(result.FieldResults);
        Assert.True(result.FieldResults["$.name"].IsSuccessful);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ConstraintEvaluationResult_MergeFieldResult_WithErrors()
    {
        var result = new ConstraintEvaluationResult { IsSuccessful = true };
        var fieldResult = new FieldEvaluationResult { IsSuccessful = false };
        fieldResult.AddError("FIELD_ERR", "Field failed");

        result.MergeFieldResult("$.name", fieldResult);

        Assert.False(result.IsSuccessful);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void ConstraintEvaluationResult_MergeFieldResult_WithWarnings()
    {
        var result = new ConstraintEvaluationResult { IsSuccessful = true };
        var fieldResult = new FieldEvaluationResult { IsSuccessful = true };
        fieldResult.AddWarning("FIELD_WARN", "Field warning");

        result.MergeFieldResult("$.name", fieldResult);

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Warnings);
    }

    [Fact]
    public void ConstraintEvaluationResult_MergeDisclosureResult_Success()
    {
        var result = new ConstraintEvaluationResult { IsSuccessful = true };
        var disclosureResult = new DisclosureEvaluationResult
        {
            IsSuccessful = true,
            SupportsSelectiveDisclosure = true
        };

        result.MergeDisclosureResult(disclosureResult);

        Assert.NotNull(result.DisclosureResult);
        Assert.True(result.DisclosureResult.SupportsSelectiveDisclosure);
    }

    [Fact]
    public void ConstraintEvaluationResult_MergeDisclosureResult_WithErrors()
    {
        var result = new ConstraintEvaluationResult { IsSuccessful = true };
        var disclosureResult = new DisclosureEvaluationResult();
        disclosureResult.AddError("DISC_ERR", "Disclosure error");

        result.MergeDisclosureResult(disclosureResult);

        Assert.False(result.IsSuccessful);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void ConstraintEvaluationResult_MergeDisclosureResult_WithWarnings()
    {
        var result = new ConstraintEvaluationResult { IsSuccessful = true };
        var disclosureResult = new DisclosureEvaluationResult();
        disclosureResult.AddWarning("DISC_WARN", "Disclosure warning");

        result.MergeDisclosureResult(disclosureResult);

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Warnings);
    }

    [Fact]
    public void ConstraintEvaluationResult_MergeSubjectResult_Success()
    {
        var result = new ConstraintEvaluationResult { IsSuccessful = true };
        var subjectResult = new SubjectEvaluationResult
        {
            IsSuccessful = true,
            SubjectEqualsIssuer = true
        };

        result.MergeSubjectResult(subjectResult);

        Assert.NotNull(result.SubjectResult);
        Assert.True(result.SubjectResult.SubjectEqualsIssuer);
    }

    [Fact]
    public void ConstraintEvaluationResult_MergeSubjectResult_WithErrors()
    {
        var result = new ConstraintEvaluationResult { IsSuccessful = true };
        var subjectResult = new SubjectEvaluationResult();
        subjectResult.AddError("SUBJ_ERR", "Subject error");

        result.MergeSubjectResult(subjectResult);

        Assert.False(result.IsSuccessful);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void ConstraintEvaluationResult_MergeSubjectResult_WithWarnings()
    {
        var result = new ConstraintEvaluationResult { IsSuccessful = true };
        var subjectResult = new SubjectEvaluationResult();
        subjectResult.AddWarning("SUBJ_WARN", "Subject warning");

        result.MergeSubjectResult(subjectResult);

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Warnings);
    }

    [Fact]
    public void ConstraintEvaluationResult_GetSatisfiedFieldPaths_ReturnsCorrectPaths()
    {
        var result = new ConstraintEvaluationResult();
        result.FieldResults["$.name"] = new FieldEvaluationResult { IsSuccessful = true };
        result.FieldResults["$.age"] = new FieldEvaluationResult { IsSuccessful = false };
        result.FieldResults["$.email"] = new FieldEvaluationResult { IsSuccessful = true };

        var satisfied = result.GetSatisfiedFieldPaths();

        Assert.Equal(2, satisfied.Length);
        Assert.Contains("$.name", satisfied);
        Assert.Contains("$.email", satisfied);
    }

    [Fact]
    public void ConstraintEvaluationResult_GetUnsatisfiedFieldPaths_ReturnsCorrectPaths()
    {
        var result = new ConstraintEvaluationResult();
        result.FieldResults["$.name"] = new FieldEvaluationResult { IsSuccessful = true };
        result.FieldResults["$.age"] = new FieldEvaluationResult { IsSuccessful = false };
        result.FieldResults["$.email"] = new FieldEvaluationResult { IsSuccessful = false };

        var unsatisfied = result.GetUnsatisfiedFieldPaths();

        Assert.Equal(2, unsatisfied.Length);
        Assert.Contains("$.age", unsatisfied);
        Assert.Contains("$.email", unsatisfied);
    }

    [Fact]
    public void ConstraintEvaluationResult_AreAllRequiredFieldsSatisfied_TrueWhenAllRequired()
    {
        var result = new ConstraintEvaluationResult();
        result.FieldResults["$.name"] = new FieldEvaluationResult { IsSuccessful = true, IsOptional = false };
        result.FieldResults["$.age"] = new FieldEvaluationResult { IsSuccessful = true, IsOptional = false };
        result.FieldResults["$.nickname"] = new FieldEvaluationResult { IsSuccessful = false, IsOptional = true };

        Assert.True(result.AreAllRequiredFieldsSatisfied());
    }

    [Fact]
    public void ConstraintEvaluationResult_AreAllRequiredFieldsSatisfied_FalseWhenRequiredFails()
    {
        var result = new ConstraintEvaluationResult();
        result.FieldResults["$.name"] = new FieldEvaluationResult { IsSuccessful = true, IsOptional = false };
        result.FieldResults["$.age"] = new FieldEvaluationResult { IsSuccessful = false, IsOptional = false };

        Assert.False(result.AreAllRequiredFieldsSatisfied());
    }

    [Fact]
    public void ConstraintEvaluationResult_Metadata_CanStoreValues()
    {
        var result = new ConstraintEvaluationResult();
        result.Metadata["custom"] = "value";
        result.Metadata["count"] = 42;

        Assert.Equal("value", result.Metadata["custom"]);
        Assert.Equal(42, result.Metadata["count"]);
    }

    #endregion

    #region FieldEvaluationResult Tests

    [Fact]
    public void FieldEvaluationResult_DefaultState()
    {
        var result = new FieldEvaluationResult();

        Assert.Null(result.FieldId);
        Assert.False(result.IsSuccessful);
        Assert.False(result.IsOptional);
        Assert.Null(result.MatchedPath);
        Assert.Null(result.FieldValue);
        Assert.Null(result.FilterDetails);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void FieldEvaluationResult_AddError_AddsAndFails()
    {
        var result = new FieldEvaluationResult { IsSuccessful = true };

        result.AddError("ERR", "Error message");

        Assert.False(result.IsSuccessful);
        Assert.Single(result.Errors);
        Assert.Equal("ERR", result.Errors[0].Code);
        Assert.Equal("Error message", result.Errors[0].Message);
    }

    [Fact]
    public void FieldEvaluationResult_AddWarning_DoesNotFail()
    {
        var result = new FieldEvaluationResult { IsSuccessful = true };

        result.AddWarning("WARN", "Warning message");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Warnings);
        Assert.Equal("WARN", result.Warnings[0].Code);
    }

    [Fact]
    public void FieldEvaluationResult_GetStringValue_ReturnsString()
    {
        var json = JsonDocument.Parse("\"test value\"");
        var result = new FieldEvaluationResult { FieldValue = json.RootElement };

        Assert.Equal("test value", result.GetStringValue());
    }

    [Fact]
    public void FieldEvaluationResult_GetStringValue_ReturnsNullForNonString()
    {
        var json = JsonDocument.Parse("42");
        var result = new FieldEvaluationResult { FieldValue = json.RootElement };

        Assert.Null(result.GetStringValue());
    }

    [Fact]
    public void FieldEvaluationResult_GetStringValue_ReturnsNullWhenNoValue()
    {
        var result = new FieldEvaluationResult();

        Assert.Null(result.GetStringValue());
    }

    [Fact]
    public void FieldEvaluationResult_GetNumericValue_ReturnsNumber()
    {
        var json = JsonDocument.Parse("42.5");
        var result = new FieldEvaluationResult { FieldValue = json.RootElement };

        Assert.Equal(42.5, result.GetNumericValue());
    }

    [Fact]
    public void FieldEvaluationResult_GetNumericValue_ReturnsNullForNonNumber()
    {
        var json = JsonDocument.Parse("\"not a number\"");
        var result = new FieldEvaluationResult { FieldValue = json.RootElement };

        Assert.Null(result.GetNumericValue());
    }

    [Fact]
    public void FieldEvaluationResult_GetNumericValue_ReturnsNullWhenNoValue()
    {
        var result = new FieldEvaluationResult();

        Assert.Null(result.GetNumericValue());
    }

    [Fact]
    public void FieldEvaluationResult_SetAllProperties()
    {
        var json = JsonDocument.Parse("\"value\"");
        var filterResult = new FilterEvaluationResult { IsSuccessful = true };

        var result = new FieldEvaluationResult
        {
            FieldId = "field_1",
            IsSuccessful = true,
            IsOptional = true,
            MatchedPath = "$.data.field",
            FieldValue = json.RootElement,
            FilterDetails = filterResult
        };

        Assert.Equal("field_1", result.FieldId);
        Assert.True(result.IsSuccessful);
        Assert.True(result.IsOptional);
        Assert.Equal("$.data.field", result.MatchedPath);
        Assert.Equal(JsonValueKind.String, result.FieldValue?.ValueKind);
        Assert.NotNull(result.FilterDetails);
    }

    #endregion

    #region DisclosureEvaluationResult Tests

    [Fact]
    public void DisclosureEvaluationResult_DefaultState()
    {
        var result = new DisclosureEvaluationResult();

        Assert.True(result.IsSuccessful);
        Assert.False(result.SupportsSelectiveDisclosure);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void DisclosureEvaluationResult_AddError_AddsAndFails()
    {
        var result = new DisclosureEvaluationResult();

        result.AddError("DISC_ERR", "Disclosure error message");

        Assert.False(result.IsSuccessful);
        Assert.Single(result.Errors);
        Assert.Equal("DISC_ERR", result.Errors[0].Code);
        Assert.Equal("Disclosure error message", result.Errors[0].Message);
    }

    [Fact]
    public void DisclosureEvaluationResult_AddWarning_DoesNotFail()
    {
        var result = new DisclosureEvaluationResult();

        result.AddWarning("DISC_WARN", "Disclosure warning");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Warnings);
        Assert.Equal("DISC_WARN", result.Warnings[0].Code);
    }

    [Fact]
    public void DisclosureEvaluationResult_SupportsSelectiveDisclosure_CanBeSet()
    {
        var result = new DisclosureEvaluationResult { SupportsSelectiveDisclosure = true };

        Assert.True(result.SupportsSelectiveDisclosure);
    }

    #endregion

    #region SubjectEvaluationResult Tests

    [Fact]
    public void SubjectEvaluationResult_DefaultState()
    {
        var result = new SubjectEvaluationResult();

        Assert.True(result.IsSuccessful);
        Assert.Null(result.SubjectEqualsIssuer);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void SubjectEvaluationResult_AddError_AddsAndFails()
    {
        var result = new SubjectEvaluationResult();

        result.AddError("SUBJ_ERR", "Subject error message");

        Assert.False(result.IsSuccessful);
        Assert.Single(result.Errors);
        Assert.Equal("SUBJ_ERR", result.Errors[0].Code);
        Assert.Equal("Subject error message", result.Errors[0].Message);
    }

    [Fact]
    public void SubjectEvaluationResult_AddWarning_DoesNotFail()
    {
        var result = new SubjectEvaluationResult();

        result.AddWarning("SUBJ_WARN", "Subject warning");

        Assert.True(result.IsSuccessful);
        Assert.Single(result.Warnings);
        Assert.Equal("SUBJ_WARN", result.Warnings[0].Code);
    }

    [Fact]
    public void SubjectEvaluationResult_SubjectEqualsIssuer_CanBeSet()
    {
        var result = new SubjectEvaluationResult { SubjectEqualsIssuer = true };

        Assert.True(result.SubjectEqualsIssuer);
    }

    #endregion

    #region Error and Warning Type Tests

    [Fact]
    public void ConstraintEvaluationError_PropertyDefaults()
    {
        var error = new ConstraintEvaluationError();

        Assert.Equal(string.Empty, error.Code);
        Assert.Equal(string.Empty, error.Message);
        Assert.Null(error.FieldPath);
    }

    [Fact]
    public void ConstraintEvaluationError_SetAllProperties()
    {
        var error = new ConstraintEvaluationError
        {
            Code = "ERR001",
            Message = "Error occurred",
            FieldPath = "$.path"
        };

        Assert.Equal("ERR001", error.Code);
        Assert.Equal("Error occurred", error.Message);
        Assert.Equal("$.path", error.FieldPath);
    }

    [Fact]
    public void ConstraintEvaluationWarning_PropertyDefaults()
    {
        var warning = new ConstraintEvaluationWarning();

        Assert.Equal(string.Empty, warning.Code);
        Assert.Equal(string.Empty, warning.Message);
        Assert.Null(warning.FieldPath);
    }

    [Fact]
    public void ConstraintEvaluationWarning_SetAllProperties()
    {
        var warning = new ConstraintEvaluationWarning
        {
            Code = "WARN001",
            Message = "Warning issued",
            FieldPath = "$.warning.path"
        };

        Assert.Equal("WARN001", warning.Code);
        Assert.Equal("Warning issued", warning.Message);
        Assert.Equal("$.warning.path", warning.FieldPath);
    }

    [Fact]
    public void FieldEvaluationError_PropertyDefaults()
    {
        var error = new FieldEvaluationError();

        Assert.Equal(string.Empty, error.Code);
        Assert.Equal(string.Empty, error.Message);
    }

    [Fact]
    public void FieldEvaluationError_SetAllProperties()
    {
        var error = new FieldEvaluationError
        {
            Code = "FIELD_ERR",
            Message = "Field error"
        };

        Assert.Equal("FIELD_ERR", error.Code);
        Assert.Equal("Field error", error.Message);
    }

    [Fact]
    public void FieldEvaluationWarning_PropertyDefaults()
    {
        var warning = new FieldEvaluationWarning();

        Assert.Equal(string.Empty, warning.Code);
        Assert.Equal(string.Empty, warning.Message);
    }

    [Fact]
    public void FieldEvaluationWarning_SetAllProperties()
    {
        var warning = new FieldEvaluationWarning
        {
            Code = "FIELD_WARN",
            Message = "Field warning"
        };

        Assert.Equal("FIELD_WARN", warning.Code);
        Assert.Equal("Field warning", warning.Message);
    }

    [Fact]
    public void DisclosureEvaluationError_PropertyDefaults()
    {
        var error = new DisclosureEvaluationError();

        Assert.Equal(string.Empty, error.Code);
        Assert.Equal(string.Empty, error.Message);
    }

    [Fact]
    public void DisclosureEvaluationError_SetAllProperties()
    {
        var error = new DisclosureEvaluationError
        {
            Code = "DISC_ERR",
            Message = "Disclosure error"
        };

        Assert.Equal("DISC_ERR", error.Code);
        Assert.Equal("Disclosure error", error.Message);
    }

    [Fact]
    public void DisclosureEvaluationWarning_PropertyDefaults()
    {
        var warning = new DisclosureEvaluationWarning();

        Assert.Equal(string.Empty, warning.Code);
        Assert.Equal(string.Empty, warning.Message);
    }

    [Fact]
    public void DisclosureEvaluationWarning_SetAllProperties()
    {
        var warning = new DisclosureEvaluationWarning
        {
            Code = "DISC_WARN",
            Message = "Disclosure warning"
        };

        Assert.Equal("DISC_WARN", warning.Code);
        Assert.Equal("Disclosure warning", warning.Message);
    }

    [Fact]
    public void SubjectEvaluationError_PropertyDefaults()
    {
        var error = new SubjectEvaluationError();

        Assert.Equal(string.Empty, error.Code);
        Assert.Equal(string.Empty, error.Message);
    }

    [Fact]
    public void SubjectEvaluationError_SetAllProperties()
    {
        var error = new SubjectEvaluationError
        {
            Code = "SUBJ_ERR",
            Message = "Subject error"
        };

        Assert.Equal("SUBJ_ERR", error.Code);
        Assert.Equal("Subject error", error.Message);
    }

    [Fact]
    public void SubjectEvaluationWarning_PropertyDefaults()
    {
        var warning = new SubjectEvaluationWarning();

        Assert.Equal(string.Empty, warning.Code);
        Assert.Equal(string.Empty, warning.Message);
    }

    [Fact]
    public void SubjectEvaluationWarning_SetAllProperties()
    {
        var warning = new SubjectEvaluationWarning
        {
            Code = "SUBJ_WARN",
            Message = "Subject warning"
        };

        Assert.Equal("SUBJ_WARN", warning.Code);
        Assert.Equal("Subject warning", warning.Message);
    }

    #endregion

    #region FilterEvaluationResult Tests

    [Fact]
    public void FilterEvaluationResult_DefaultState()
    {
        var result = new FilterEvaluationResult();

        Assert.False(result.IsSuccessful);
        Assert.NotNull(result.Errors);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Details);
        Assert.Empty(result.Details);
    }

    [Fact]
    public void FilterEvaluationResult_SetSuccessful()
    {
        var result = new FilterEvaluationResult
        {
            IsSuccessful = true
        };
        result.Details["matched_path"] = "$.name";

        Assert.True(result.IsSuccessful);
        Assert.Contains("matched_path", result.Details.Keys);
        Assert.Equal("$.name", result.Details["matched_path"]);
    }

    [Fact]
    public void FilterEvaluationResult_AddError()
    {
        var result = new FilterEvaluationResult
        {
            IsSuccessful = true
        };
        result.AddError("FILTER_001", "Filter did not match");

        Assert.False(result.IsSuccessful);
        Assert.Single(result.Errors);
        Assert.Contains("FILTER_001: Filter did not match", result.Errors);
    }

    [Fact]
    public void FilterEvaluationResult_MultipleErrors()
    {
        var result = new FilterEvaluationResult();
        result.AddError("ERR_1", "First error");
        result.AddError("ERR_2", "Second error");

        Assert.False(result.IsSuccessful);
        Assert.Equal(2, result.Errors.Count);
    }

    #endregion
}
