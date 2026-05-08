using FluentAssertions;
using SdJwt.Net.VcDm.Models;
using SdJwt.Net.VcDm.Validation;
using Xunit;

namespace SdJwt.Net.VcDm.Tests;

public class VcDmValidatorTests
{
    private readonly VcDmValidator _validator = new();

    // -----------------------------------------------------------------------
    // Valid cases
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_ValidMinimalCredential_IsValid()
    {
        var vc = BuildValid();
        var result = _validator.Validate(vc);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidPresentation_IsValid()
    {
        var vp = new VerifiablePresentation();
        var result = _validator.Validate(vp);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_V1Context_IsAccepted_ForBackwardCompat()
    {
        var vc = BuildValid();
        vc.Context = [VcDmContexts.V1];
        var result = _validator.Validate(vc);
        result.IsValid.Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // Context errors
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_MissingContext_ReturnsError()
    {
        var vc = BuildValid();
        vc.Context = [];
        var result = _validator.Validate(vc);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainMatch("*@context*required*");
    }

    [Fact]
    public void Validate_WrongBaseContext_ReturnsError()
    {
        var vc = BuildValid();
        vc.Context = ["https://wrong.example.com/context"];
        var result = _validator.Validate(vc);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainMatch("*@context[0]*");
    }

    // -----------------------------------------------------------------------
    // Type errors
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_MissingVerifiableCredentialType_ReturnsError()
    {
        var vc = BuildValid();
        vc.Type = ["SomeOtherType"];
        var result = _validator.Validate(vc);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainMatch("*VerifiableCredential*");
    }

    [Fact]
    public void Validate_EmptyTypeArray_ReturnsError()
    {
        var vc = BuildValid();
        vc.Type = [];
        var result = _validator.Validate(vc);
        result.IsValid.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // Issuer / subject errors
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_NullIssuer_ReturnsError()
    {
        var vc = BuildValid();
        vc.Issuer = null!;
        var result = _validator.Validate(vc);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainMatch("*issuer*required*");
    }

    [Fact]
    public void Validate_EmptyCredentialSubject_ReturnsError()
    {
        var vc = BuildValid();
        vc.CredentialSubject = [];
        var result = _validator.Validate(vc);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainMatch("*credentialSubject*required*");
    }

    // -----------------------------------------------------------------------
    // Status / schema / termsOfUse / evidence errors
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_StatusWithoutType_ReturnsError()
    {
        var vc = BuildValid();
        vc.CredentialStatus = [new UnknownCredentialStatus("")];
        var result = _validator.Validate(vc);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainMatch("*credentialStatus*type*required*");
    }

    [Fact]
    public void Validate_SchemaWithoutId_ReturnsError()
    {
        var vc = BuildValid();
        vc.CredentialSchema = [new CredentialSchema { Type = "JsonSchema" }];
        var result = _validator.Validate(vc);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainMatch("*credentialSchema*id*required*");
    }

    [Fact]
    public void Validate_SchemaWithoutType_ReturnsError()
    {
        var vc = BuildValid();
        vc.CredentialSchema = [new CredentialSchema { Id = "https://schema.example.com/1" }];
        var result = _validator.Validate(vc);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainMatch("*credentialSchema*type*required*");
    }

    [Fact]
    public void Validate_TermsOfUseWithoutType_ReturnsError()
    {
        var vc = BuildValid();
        vc.TermsOfUse = [new TermsOfUse()];
        var result = _validator.Validate(vc);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainMatch("*termsOfUse*type*required*");
    }

    [Fact]
    public void Validate_EvidenceWithoutType_ReturnsError()
    {
        var vc = BuildValid();
        vc.Evidence = [new Evidence { Type = [] }];
        var result = _validator.Validate(vc);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainMatch("*evidence*type*required*");
    }

    // -----------------------------------------------------------------------
    // Temporal errors
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_ValidFromAfterValidUntil_ReturnsError()
    {
        var vc = BuildValid();
        vc.ValidFrom = DateTimeOffset.UtcNow.AddYears(1);
        vc.ValidUntil = DateTimeOffset.UtcNow;
        var result = _validator.Validate(vc);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainMatch("*validFrom*validUntil*");
    }

    // -----------------------------------------------------------------------
    // Multiple errors reported at once
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_MultipleErrors_ReturnsAllErrors()
    {
        var vc = new VerifiableCredential
        {
            Context = [],
            Type = [],
            Issuer = null!,
            CredentialSubject = []
        };

        var result = _validator.Validate(vc);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
    }

    private static VerifiableCredential BuildValid() => new()
    {
        Context = [VcDmContexts.V2],
        Type = ["VerifiableCredential"],
        Issuer = new Issuer("https://example.com"),
        CredentialSubject = [new CredentialSubject { Id = "did:example:123" }]
    };
}
