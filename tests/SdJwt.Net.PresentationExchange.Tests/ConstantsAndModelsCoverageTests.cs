using SdJwt.Net.PresentationExchange.Models;
using Xunit;

namespace SdJwt.Net.PresentationExchange.Tests;

/// <summary>
/// Tests for PresentationExchangeConstants and related model classes.
/// </summary>
public class ConstantsAndModelsCoverageTests
{
    #region PresentationExchangeConstants Tests

    [Fact]
    public void SpecVersion_HasCorrectValue()
    {
        Assert.Equal("https://identity.foundation/presentation-exchange/spec/v2.1.1/", PresentationExchangeConstants.SpecVersion);
    }

    [Fact]
    public void Formats_JwtConstant_HasCorrectValue()
    {
        Assert.Equal("jwt", PresentationExchangeConstants.Formats.Jwt);
    }

    [Fact]
    public void Formats_JwtVcConstant_HasCorrectValue()
    {
        Assert.Equal("jwt_vc", PresentationExchangeConstants.Formats.JwtVc);
    }

    [Fact]
    public void Formats_JwtVpConstant_HasCorrectValue()
    {
        Assert.Equal("jwt_vp", PresentationExchangeConstants.Formats.JwtVp);
    }

    [Fact]
    public void Formats_SdJwtConstant_HasCorrectValue()
    {
        Assert.Equal("sd-jwt", PresentationExchangeConstants.Formats.SdJwt);
    }

    [Fact]
    public void Formats_SdJwtVcConstant_HasCorrectValue()
    {
        Assert.Equal("vc+sd-jwt", PresentationExchangeConstants.Formats.SdJwtVc);
    }

    [Fact]
    public void Formats_LdpConstant_HasCorrectValue()
    {
        Assert.Equal("ldp", PresentationExchangeConstants.Formats.Ldp);
    }

    [Fact]
    public void Formats_LdpVcConstant_HasCorrectValue()
    {
        Assert.Equal("ldp_vc", PresentationExchangeConstants.Formats.LdpVc);
    }

    [Fact]
    public void Formats_LdpVpConstant_HasCorrectValue()
    {
        Assert.Equal("ldp_vp", PresentationExchangeConstants.Formats.LdpVp);
    }

    [Fact]
    public void Formats_AllArray_ContainsAllFormats()
    {
        var all = PresentationExchangeConstants.Formats.All;

        Assert.Contains("jwt", all);
        Assert.Contains("jwt_vc", all);
        Assert.Contains("jwt_vp", all);
        Assert.Contains("sd-jwt", all);
        Assert.Contains("vc+sd-jwt", all);
        Assert.Contains("ldp", all);
        Assert.Contains("ldp_vc", all);
        Assert.Contains("ldp_vp", all);
        Assert.Equal(8, all.Length);
    }

    [Fact]
    public void Operators_EqualConstant_HasCorrectValue()
    {
        Assert.Equal("eq", PresentationExchangeConstants.Operators.Equal);
    }

    [Fact]
    public void Operators_NotEqualConstant_HasCorrectValue()
    {
        Assert.Equal("ne", PresentationExchangeConstants.Operators.NotEqual);
    }

    [Fact]
    public void Operators_LessThanConstant_HasCorrectValue()
    {
        Assert.Equal("lt", PresentationExchangeConstants.Operators.LessThan);
    }

    [Fact]
    public void Operators_LessThanOrEqualConstant_HasCorrectValue()
    {
        Assert.Equal("le", PresentationExchangeConstants.Operators.LessThanOrEqual);
    }

    [Fact]
    public void Operators_GreaterThanConstant_HasCorrectValue()
    {
        Assert.Equal("gt", PresentationExchangeConstants.Operators.GreaterThan);
    }

    [Fact]
    public void Operators_GreaterThanOrEqualConstant_HasCorrectValue()
    {
        Assert.Equal("ge", PresentationExchangeConstants.Operators.GreaterThanOrEqual);
    }

    [Fact]
    public void Operators_InConstant_HasCorrectValue()
    {
        Assert.Equal("in", PresentationExchangeConstants.Operators.In);
    }

    [Fact]
    public void Operators_NotInConstant_HasCorrectValue()
    {
        Assert.Equal("not_in", PresentationExchangeConstants.Operators.NotIn);
    }

    [Fact]
    public void Operators_ContainsConstant_HasCorrectValue()
    {
        Assert.Equal("contains", PresentationExchangeConstants.Operators.Contains);
    }

    [Fact]
    public void Operators_StartsWithConstant_HasCorrectValue()
    {
        Assert.Equal("starts_with", PresentationExchangeConstants.Operators.StartsWith);
    }

    [Fact]
    public void Operators_EndsWithConstant_HasCorrectValue()
    {
        Assert.Equal("ends_with", PresentationExchangeConstants.Operators.EndsWith);
    }

    [Fact]
    public void Operators_MatchesConstant_HasCorrectValue()
    {
        Assert.Equal("matches", PresentationExchangeConstants.Operators.Matches);
    }

    [Fact]
    public void Operators_ExistsConstant_HasCorrectValue()
    {
        Assert.Equal("exists", PresentationExchangeConstants.Operators.Exists);
    }

    [Fact]
    public void Operators_TypeConstant_HasCorrectValue()
    {
        Assert.Equal("type", PresentationExchangeConstants.Operators.Type);
    }

    [Fact]
    public void Operators_AllArray_ContainsAllOperators()
    {
        var all = PresentationExchangeConstants.Operators.All;

        Assert.Contains("eq", all);
        Assert.Contains("ne", all);
        Assert.Contains("lt", all);
        Assert.Contains("le", all);
        Assert.Contains("gt", all);
        Assert.Contains("ge", all);
        Assert.Contains("in", all);
        Assert.Contains("not_in", all);
        Assert.Contains("contains", all);
        Assert.Contains("starts_with", all);
        Assert.Contains("ends_with", all);
        Assert.Contains("matches", all);
        Assert.Contains("exists", all);
        Assert.Contains("type", all);
        Assert.Equal(14, all.Length);
    }

    [Fact]
    public void SubmissionRules_AllConstant_HasCorrectValue()
    {
        Assert.Equal("all", PresentationExchangeConstants.SubmissionRules.All);
    }

    [Fact]
    public void SubmissionRules_PickConstant_HasCorrectValue()
    {
        Assert.Equal("pick", PresentationExchangeConstants.SubmissionRules.Pick);
    }

    [Fact]
    public void ErrorCodes_NoMatchingCredentials_HasCorrectValue()
    {
        Assert.Equal("no_matching_credentials", PresentationExchangeConstants.ErrorCodes.NoMatchingCredentials);
    }

    [Fact]
    public void ErrorCodes_InvalidPresentationDefinition_HasCorrectValue()
    {
        Assert.Equal("invalid_presentation_definition", PresentationExchangeConstants.ErrorCodes.InvalidPresentationDefinition);
    }

    [Fact]
    public void ErrorCodes_ConstraintEvaluationFailed_HasCorrectValue()
    {
        Assert.Equal("constraint_evaluation_failed", PresentationExchangeConstants.ErrorCodes.ConstraintEvaluationFailed);
    }

    [Fact]
    public void ErrorCodes_JsonPathEvaluationFailed_HasCorrectValue()
    {
        Assert.Equal("jsonpath_evaluation_failed", PresentationExchangeConstants.ErrorCodes.JsonPathEvaluationFailed);
    }

    [Fact]
    public void ErrorCodes_SubmissionRequirementNotSatisfied_HasCorrectValue()
    {
        Assert.Equal("submission_requirement_not_satisfied", PresentationExchangeConstants.ErrorCodes.SubmissionRequirementNotSatisfied);
    }

    [Fact]
    public void Defaults_MaxCredentials_HasCorrectValue()
    {
        Assert.Equal(1000, PresentationExchangeConstants.Defaults.MaxCredentials);
    }

    [Fact]
    public void Defaults_ConstraintEvaluationTimeoutMs_HasCorrectValue()
    {
        Assert.Equal(5000, PresentationExchangeConstants.Defaults.ConstraintEvaluationTimeoutMs);
    }

    [Fact]
    public void Defaults_MaxConstraintDepth_HasCorrectValue()
    {
        Assert.Equal(10, PresentationExchangeConstants.Defaults.MaxConstraintDepth);
    }

    [Fact]
    public void CommonJsonPaths_CredentialType_HasCorrectValue()
    {
        Assert.Equal("$.type", PresentationExchangeConstants.CommonJsonPaths.CredentialType);
    }

    [Fact]
    public void CommonJsonPaths_Issuer_HasCorrectValue()
    {
        Assert.Equal("$.iss", PresentationExchangeConstants.CommonJsonPaths.Issuer);
    }

    [Fact]
    public void CommonJsonPaths_Subject_HasCorrectValue()
    {
        Assert.Equal("$.sub", PresentationExchangeConstants.CommonJsonPaths.Subject);
    }

    [Fact]
    public void CommonJsonPaths_VcType_HasCorrectValue()
    {
        Assert.Equal("$.vc.type", PresentationExchangeConstants.CommonJsonPaths.VcType);
    }

    [Fact]
    public void CommonJsonPaths_VcCredentialSubject_HasCorrectValue()
    {
        Assert.Equal("$.vc.credentialSubject", PresentationExchangeConstants.CommonJsonPaths.VcCredentialSubject);
    }

    [Fact]
    public void CommonJsonPaths_VctType_HasCorrectValue()
    {
        Assert.Equal("$.vct", PresentationExchangeConstants.CommonJsonPaths.VctType);
    }

    #endregion

    #region InputDescriptorMapping Tests

    [Fact]
    public void InputDescriptorMapping_DefaultState()
    {
        var mapping = new InputDescriptorMapping();

        Assert.Equal(string.Empty, mapping.Id);
        Assert.Equal(string.Empty, mapping.Format);
        Assert.Equal(string.Empty, mapping.Path);
        Assert.Null(mapping.PathNested);
    }

    [Fact]
    public void InputDescriptorMapping_SetAllProperties()
    {
        var mapping = new InputDescriptorMapping
        {
            Id = "driver_license",
            Format = "vc+sd-jwt",
            Path = "$",
            PathNested = new PathMapping
            {
                Format = "jwt",
                Path = new[] { "$.verifiableCredential[0]" }
            }
        };

        Assert.Equal("driver_license", mapping.Id);
        Assert.Equal("vc+sd-jwt", mapping.Format);
        Assert.Equal("$", mapping.Path);
        Assert.NotNull(mapping.PathNested);
        Assert.Equal("jwt", mapping.PathNested.Format);
        Assert.NotNull(mapping.PathNested.Path);
        Assert.Single(mapping.PathNested.Path);
    }

    #endregion

    #region PresentationSubmission Tests

    [Fact]
    public void PresentationSubmission_DefaultState()
    {
        var submission = new PresentationSubmission();

        Assert.Equal(string.Empty, submission.Id);
        Assert.Equal(string.Empty, submission.DefinitionId);
        Assert.NotNull(submission.DescriptorMap);
        Assert.Empty(submission.DescriptorMap);
    }

    [Fact]
    public void PresentationSubmission_SetAllProperties()
    {
        var submission = new PresentationSubmission
        {
            Id = "submission_1",
            DefinitionId = "definition_1",
            DescriptorMap = new[]
            {
                new InputDescriptorMapping { Id = "desc_1", Format = "jwt", Path = "$" },
                new InputDescriptorMapping { Id = "desc_2", Format = "sd-jwt", Path = "$[0]" }
            }
        };

        Assert.Equal("submission_1", submission.Id);
        Assert.Equal("definition_1", submission.DefinitionId);
        Assert.NotNull(submission.DescriptorMap);
        Assert.Equal(2, submission.DescriptorMap.Length);
    }

    #endregion

    #region SubmissionRequirement Tests

    [Fact]
    public void SubmissionRequirement_DefaultState()
    {
        var requirement = new SubmissionRequirement();

        Assert.Null(requirement.Name);
        Assert.Null(requirement.Purpose);
        Assert.Equal(string.Empty, requirement.Rule);
        Assert.Null(requirement.Count);
        Assert.Null(requirement.Min);
        Assert.Null(requirement.Max);
        Assert.Null(requirement.From);
        Assert.Null(requirement.FromNested);
    }

    [Fact]
    public void SubmissionRequirement_SetAllProperties()
    {
        var requirement = new SubmissionRequirement
        {
            Name = "Age Verification",
            Purpose = "Verify the user is over 18",
            Rule = "all",
            Count = 1,
            Min = 1,
            Max = 2,
            From = "age_credentials",
            FromNested = new[]
            {
                new SubmissionRequirement { Rule = "pick", Count = 1, From = "nested_group" }
            }
        };

        Assert.Equal("Age Verification", requirement.Name);
        Assert.Equal("Verify the user is over 18", requirement.Purpose);
        Assert.Equal("all", requirement.Rule);
        Assert.Equal(1, requirement.Count);
        Assert.Equal(1, requirement.Min);
        Assert.Equal(2, requirement.Max);
        Assert.Equal("age_credentials", requirement.From);
        Assert.NotNull(requirement.FromNested);
        Assert.Single(requirement.FromNested);
    }

    [Fact]
    public void SubmissionRequirement_AllRule()
    {
        var requirement = new SubmissionRequirement
        {
            Rule = PresentationExchangeConstants.SubmissionRules.All,
            From = "test_group"
        };

        Assert.Equal("all", requirement.Rule);
    }

    [Fact]
    public void SubmissionRequirement_PickRule()
    {
        var requirement = new SubmissionRequirement
        {
            Rule = PresentationExchangeConstants.SubmissionRules.Pick,
            Count = 2,
            From = "test_group"
        };

        Assert.Equal("pick", requirement.Rule);
        Assert.Equal(2, requirement.Count);
    }

    #endregion

    #region CredentialMatch Tests

    [Fact]
    public void CredentialMatch_DefaultState()
    {
        var match = new CredentialMatch();

        Assert.NotNull(match.Credential);
        Assert.Equal(string.Empty, match.InputDescriptorId);
        Assert.NotNull(match.PathMappings);
        Assert.Empty(match.PathMappings);
        Assert.Equal(string.Empty, match.Format);
        Assert.Equal(0.0, match.MatchScore);
    }

    [Fact]
    public void CredentialMatch_SetProperties()
    {
        var match = new CredentialMatch
        {
            Credential = "eyJhbGciOiJFUzI1NiJ9...",
            InputDescriptorId = "descriptor_1",
            Format = "vc+sd-jwt",
            MatchScore = 0.95
        };
        match.PathMappings.Add("$.name", "$.credentialSubject.name");
        match.PathMappings.Add("$.age", "$.credentialSubject.age");

        Assert.Equal("eyJhbGciOiJFUzI1NiJ9...", match.Credential);
        Assert.Equal("descriptor_1", match.InputDescriptorId);
        Assert.Equal(2, match.PathMappings.Count);
        Assert.Equal(0.95, match.MatchScore);
    }

    [Fact]
    public void CredentialMatch_WithDisclosures()
    {
        var match = new CredentialMatch
        {
            InputDescriptorId = "desc_1",
            Format = "vc+sd-jwt",
            Disclosures = new[] { "disclosure1", "disclosure2" }
        };

        Assert.NotNull(match.Disclosures);
        Assert.Equal(2, match.Disclosures.Length);
    }

    #endregion

    #region SelectedCredential Tests

    [Fact]
    public void SelectedCredential_DefaultState()
    {
        var selected = new SelectedCredential();

        Assert.NotNull(selected.Credential);
        Assert.Equal(string.Empty, selected.InputDescriptorId);
        Assert.Equal(string.Empty, selected.Format);
        Assert.NotNull(selected.PathMappings);
        Assert.Empty(selected.PathMappings);
        Assert.Equal(0.0, selected.MatchScore);
    }

    [Fact]
    public void SelectedCredential_SetAllProperties()
    {
        var selected = new SelectedCredential
        {
            Credential = "credential_token",
            InputDescriptorId = "input_desc_1",
            Format = "vc+sd-jwt",
            MatchScore = 0.8,
            Disclosures = new[] { "disc1", "disc2" }
        };
        selected.PathMappings.Add("$.name", "$.subject.name");
        selected.Metadata.Add("source", "wallet");

        Assert.Equal("credential_token", selected.Credential);
        Assert.Equal("input_desc_1", selected.InputDescriptorId);
        Assert.Equal("vc+sd-jwt", selected.Format);
        Assert.Equal(0.8, selected.MatchScore);
        Assert.NotNull(selected.Disclosures);
        Assert.Equal(2, selected.Disclosures.Length);
        Assert.Single(selected.PathMappings);
        Assert.Single(selected.Metadata);
    }

    [Fact]
    public void SelectedCredential_ForSdJwt_CreatesCorrectInstance()
    {
        var credential = new { iss = "https://issuer.example" };
        var disclosures = new[] { "disc1" };
        var pathMappings = new Dictionary<string, string> { { "$.name", "$.subject.name" } };

        var selected = SelectedCredential.ForSdJwt(
            "descriptor_1",
            credential,
            disclosures,
            pathMappings,
            0.9);

        Assert.Equal("descriptor_1", selected.InputDescriptorId);
        Assert.Equal(PresentationExchangeConstants.Formats.SdJwtVc, selected.Format);
        Assert.NotNull(selected.Disclosures);
        Assert.Single(selected.Disclosures);
        Assert.Equal(0.9, selected.MatchScore);
    }

    #endregion

    #region PathMapping Tests

    [Fact]
    public void PathMapping_DefaultState()
    {
        var mapping = new PathMapping();

        Assert.Null(mapping.Path);
        Assert.Null(mapping.Format);
    }

    [Fact]
    public void PathMapping_SetAllProperties()
    {
        var mapping = new PathMapping
        {
            Path = new[] { "$.credentialSubject.name", "$.name" },
            Format = "vc+sd-jwt"
        };

        Assert.NotNull(mapping.Path);
        Assert.Equal(2, mapping.Path.Length);
        Assert.Equal("$.credentialSubject.name", mapping.Path[0]);
        Assert.Equal("vc+sd-jwt", mapping.Format);
    }

    [Fact]
    public void PathMapping_Validate_WithValidPath_Succeeds()
    {
        var mapping = new PathMapping
        {
            Path = new[] { "$.name" }
        };

        var exception = Record.Exception(() => mapping.Validate());
        Assert.Null(exception);
    }

    [Fact]
    public void PathMapping_Validate_WithEmptyPath_ThrowsException()
    {
        var mapping = new PathMapping
        {
            Path = new[] { "" }
        };

        Assert.Throws<InvalidOperationException>(() => mapping.Validate());
    }

    #endregion
}
