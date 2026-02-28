using SdJwt.Net.PresentationExchange.Models;
using Xunit;

namespace SdJwt.Net.PresentationExchange.Tests;

/// <summary>
/// Tests for FormatConstraints and related model classes to improve code coverage.
/// </summary>
public class FormatConstraintsCoverageTests
{
    #region FormatConstraints Tests

    [Fact]
    public void FormatConstraints_DefaultState_AllNull()
    {
        var constraints = new FormatConstraints();

        Assert.Null(constraints.Jwt);
        Assert.Null(constraints.JwtVc);
        Assert.Null(constraints.JwtVp);
        Assert.Null(constraints.SdJwt);
        Assert.Null(constraints.SdJwtVc);
        Assert.Null(constraints.Ldp);
        Assert.Null(constraints.LdpVc);
        Assert.Null(constraints.LdpVp);
    }

    [Fact]
    public void FormatConstraints_HasAnyFormat_FalseWhenEmpty()
    {
        var constraints = new FormatConstraints();
        Assert.False(constraints.HasAnyFormat());
    }

    [Fact]
    public void FormatConstraints_HasAnyFormat_TrueForJwt()
    {
        var constraints = new FormatConstraints { Jwt = new JwtFormatConstraints() };
        Assert.True(constraints.HasAnyFormat());
    }

    [Fact]
    public void FormatConstraints_HasAnyFormat_TrueForJwtVc()
    {
        var constraints = new FormatConstraints { JwtVc = new JwtFormatConstraints() };
        Assert.True(constraints.HasAnyFormat());
    }

    [Fact]
    public void FormatConstraints_HasAnyFormat_TrueForJwtVp()
    {
        var constraints = new FormatConstraints { JwtVp = new JwtFormatConstraints() };
        Assert.True(constraints.HasAnyFormat());
    }

    [Fact]
    public void FormatConstraints_HasAnyFormat_TrueForSdJwt()
    {
        var constraints = new FormatConstraints { SdJwt = new SdJwtFormatConstraints() };
        Assert.True(constraints.HasAnyFormat());
    }

    [Fact]
    public void FormatConstraints_HasAnyFormat_TrueForSdJwtVc()
    {
        var constraints = new FormatConstraints { SdJwtVc = new SdJwtFormatConstraints() };
        Assert.True(constraints.HasAnyFormat());
    }

    [Fact]
    public void FormatConstraints_HasAnyFormat_TrueForLdp()
    {
        var constraints = new FormatConstraints { Ldp = new LdpFormatConstraints() };
        Assert.True(constraints.HasAnyFormat());
    }

    [Fact]
    public void FormatConstraints_HasAnyFormat_TrueForLdpVc()
    {
        var constraints = new FormatConstraints { LdpVc = new LdpFormatConstraints() };
        Assert.True(constraints.HasAnyFormat());
    }

    [Fact]
    public void FormatConstraints_HasAnyFormat_TrueForLdpVp()
    {
        var constraints = new FormatConstraints { LdpVp = new LdpFormatConstraints() };
        Assert.True(constraints.HasAnyFormat());
    }

    [Fact]
    public void FormatConstraints_GetSupportedFormats_EmptyWhenNoFormats()
    {
        var constraints = new FormatConstraints();
        var formats = constraints.GetSupportedFormats();
        Assert.Empty(formats);
    }

    [Fact]
    public void FormatConstraints_GetSupportedFormats_ReturnsAllConfiguredFormats()
    {
        var constraints = new FormatConstraints
        {
            Jwt = new JwtFormatConstraints(),
            JwtVc = new JwtFormatConstraints(),
            SdJwtVc = new SdJwtFormatConstraints(),
            Ldp = new LdpFormatConstraints()
        };

        var formats = constraints.GetSupportedFormats();

        Assert.Equal(4, formats.Length);
        Assert.Contains(PresentationExchangeConstants.Formats.Jwt, formats);
        Assert.Contains(PresentationExchangeConstants.Formats.JwtVc, formats);
        Assert.Contains(PresentationExchangeConstants.Formats.SdJwtVc, formats);
        Assert.Contains(PresentationExchangeConstants.Formats.Ldp, formats);
    }

    [Theory]
    [InlineData(PresentationExchangeConstants.Formats.Jwt, true, false, false, false, false, false, false, false)]
    [InlineData(PresentationExchangeConstants.Formats.JwtVc, false, true, false, false, false, false, false, false)]
    [InlineData(PresentationExchangeConstants.Formats.JwtVp, false, false, true, false, false, false, false, false)]
    [InlineData(PresentationExchangeConstants.Formats.SdJwt, false, false, false, true, false, false, false, false)]
    [InlineData(PresentationExchangeConstants.Formats.SdJwtVc, false, false, false, false, true, false, false, false)]
    [InlineData(PresentationExchangeConstants.Formats.Ldp, false, false, false, false, false, true, false, false)]
    [InlineData(PresentationExchangeConstants.Formats.LdpVc, false, false, false, false, false, false, true, false)]
    [InlineData(PresentationExchangeConstants.Formats.LdpVp, false, false, false, false, false, false, false, true)]
    public void FormatConstraints_SupportsFormat_ReturnsCorrectResult(
        string formatToCheck, bool jwt, bool jwtVc, bool jwtVp, bool sdJwt, bool sdJwtVc, bool ldp, bool ldpVc, bool ldpVp)
    {
        var constraints = new FormatConstraints
        {
            Jwt = jwt ? new JwtFormatConstraints() : null,
            JwtVc = jwtVc ? new JwtFormatConstraints() : null,
            JwtVp = jwtVp ? new JwtFormatConstraints() : null,
            SdJwt = sdJwt ? new SdJwtFormatConstraints() : null,
            SdJwtVc = sdJwtVc ? new SdJwtFormatConstraints() : null,
            Ldp = ldp ? new LdpFormatConstraints() : null,
            LdpVc = ldpVc ? new LdpFormatConstraints() : null,
            LdpVp = ldpVp ? new LdpFormatConstraints() : null
        };

        Assert.True(constraints.SupportsFormat(formatToCheck));
    }

    [Fact]
    public void FormatConstraints_SupportsFormat_ReturnsFalseForUnknownFormat()
    {
        var constraints = new FormatConstraints { Jwt = new JwtFormatConstraints() };
        Assert.False(constraints.SupportsFormat("unknown_format"));
    }

    [Fact]
    public void FormatConstraints_CreateForSdJwtVc_CreatesCorrectConstraints()
    {
        var constraints = FormatConstraints.CreateForSdJwtVc();

        Assert.NotNull(constraints.SdJwtVc);
        Assert.Null(constraints.Jwt);
        Assert.Null(constraints.JwtVc);
        Assert.Null(constraints.JwtVp);
        Assert.Null(constraints.SdJwt);
        Assert.Null(constraints.Ldp);
        Assert.Null(constraints.LdpVc);
        Assert.Null(constraints.LdpVp);
    }

    [Fact]
    public void FormatConstraints_CreateForJwtVc_WithoutAlgorithms()
    {
        var constraints = FormatConstraints.CreateForJwtVc();

        Assert.NotNull(constraints.JwtVc);
        Assert.Null(constraints.JwtVc.Alg);
    }

    [Fact]
    public void FormatConstraints_CreateForJwtVc_WithAlgorithms()
    {
        var algorithms = new[] { "ES256", "RS256" };
        var constraints = FormatConstraints.CreateForJwtVc(algorithms);

        Assert.NotNull(constraints.JwtVc);
        Assert.NotNull(constraints.JwtVc.Alg);
        Assert.Equal(2, constraints.JwtVc.Alg.Length);
        Assert.Contains("ES256", constraints.JwtVc.Alg);
        Assert.Contains("RS256", constraints.JwtVc.Alg);
    }

    [Fact]
    public void FormatConstraints_CreateForVcFormats_CreatesBothFormats()
    {
        var constraints = FormatConstraints.CreateForVcFormats();

        Assert.NotNull(constraints.SdJwtVc);
        Assert.NotNull(constraints.JwtVc);
        Assert.Null(constraints.Jwt);
        Assert.Null(constraints.JwtVp);
        Assert.Null(constraints.SdJwt);
    }

    [Fact]
    public void FormatConstraints_CreateForJwtFormats_CreatesAllJwtFormats()
    {
        var constraints = FormatConstraints.CreateForJwtFormats();

        Assert.NotNull(constraints.Jwt);
        Assert.NotNull(constraints.JwtVc);
        Assert.NotNull(constraints.JwtVp);
        Assert.NotNull(constraints.SdJwt);
        Assert.NotNull(constraints.SdJwtVc);
        Assert.Null(constraints.Ldp);
        Assert.Null(constraints.LdpVc);
        Assert.Null(constraints.LdpVp);
    }

    [Fact]
    public void FormatConstraints_CreateForAllFormats_CreatesAll()
    {
        var constraints = FormatConstraints.CreateForAllFormats();

        Assert.NotNull(constraints.Jwt);
        Assert.NotNull(constraints.JwtVc);
        Assert.NotNull(constraints.JwtVp);
        Assert.NotNull(constraints.SdJwt);
        Assert.NotNull(constraints.SdJwtVc);
        Assert.NotNull(constraints.Ldp);
        Assert.NotNull(constraints.LdpVc);
        Assert.NotNull(constraints.LdpVp);
    }

    [Fact]
    public void FormatConstraints_Validate_ThrowsWhenNoFormats()
    {
        var constraints = new FormatConstraints();
        Assert.Throws<InvalidOperationException>(() => constraints.Validate());
    }

    [Fact]
    public void FormatConstraints_Validate_SucceedsWithValidFormat()
    {
        var constraints = new FormatConstraints { SdJwtVc = new SdJwtFormatConstraints() };
        constraints.Validate(); // Should not throw
    }

    #endregion

    #region JwtFormatConstraints Tests

    [Fact]
    public void JwtFormatConstraints_DefaultState()
    {
        var constraints = new JwtFormatConstraints();

        Assert.Null(constraints.Alg);
        Assert.Null(constraints.ProofType);
    }

    [Fact]
    public void JwtFormatConstraints_SetAlgorithms()
    {
        var constraints = new JwtFormatConstraints
        {
            Alg = new[] { "ES256", "ES384", "ES512" }
        };

        Assert.NotNull(constraints.Alg);
        Assert.Equal(3, constraints.Alg.Length);
    }

    [Fact]
    public void JwtFormatConstraints_SetProofType()
    {
        var constraints = new JwtFormatConstraints
        {
            ProofType = new[] { "JsonWebSignature2020" }
        };

        Assert.NotNull(constraints.ProofType);
        Assert.Single(constraints.ProofType);
    }

    [Fact]
    public void JwtFormatConstraints_Validate_Succeeds()
    {
        var constraints = new JwtFormatConstraints
        {
            Alg = new[] { "ES256" },
            ProofType = new[] { "jwt" }
        };

        constraints.Validate(); // Should not throw
    }

    [Fact]
    public void JwtFormatConstraints_Validate_ThrowsOnEmptyAlgorithm()
    {
        var constraints = new JwtFormatConstraints { Alg = new[] { "" } };
        Assert.Throws<InvalidOperationException>(() => constraints.Validate());
    }

    [Fact]
    public void JwtFormatConstraints_Validate_ThrowsOnWhitespaceAlgorithm()
    {
        var constraints = new JwtFormatConstraints { Alg = new[] { "  " } };
        Assert.Throws<InvalidOperationException>(() => constraints.Validate());
    }

    [Fact]
    public void JwtFormatConstraints_Validate_ThrowsOnNullAlgorithm()
    {
        var constraints = new JwtFormatConstraints { Alg = new string[] { null! } };
        Assert.Throws<InvalidOperationException>(() => constraints.Validate());
    }

    [Fact]
    public void JwtFormatConstraints_Validate_ThrowsOnEmptyProofType()
    {
        var constraints = new JwtFormatConstraints { ProofType = new[] { "" } };
        Assert.Throws<InvalidOperationException>(() => constraints.Validate());
    }

    [Fact]
    public void JwtFormatConstraints_Validate_ThrowsOnWhitespaceProofType()
    {
        var constraints = new JwtFormatConstraints { ProofType = new[] { "  " } };
        Assert.Throws<InvalidOperationException>(() => constraints.Validate());
    }

    #endregion

    #region SdJwtFormatConstraints Tests

    [Fact]
    public void SdJwtFormatConstraints_DefaultState()
    {
        var constraints = new SdJwtFormatConstraints();

        Assert.Null(constraints.Alg);
        Assert.Null(constraints.SdAlg);
        Assert.Null(constraints.KbAlg);
    }

    [Fact]
    public void SdJwtFormatConstraints_SetAllProperties()
    {
        var constraints = new SdJwtFormatConstraints
        {
            Alg = new[] { "ES256" },
            SdAlg = new[] { "sha-256" },
            KbAlg = new[] { "ES256" }
        };

        Assert.NotNull(constraints.Alg);
        Assert.NotNull(constraints.SdAlg);
        Assert.NotNull(constraints.KbAlg);
    }

    [Fact]
    public void SdJwtFormatConstraints_Validate_Succeeds()
    {
        var constraints = new SdJwtFormatConstraints
        {
            Alg = new[] { "ES256" },
            SdAlg = new[] { "sha-256" },
            KbAlg = new[] { "ES256" }
        };

        constraints.Validate(); // Should not throw
    }

    [Fact]
    public void SdJwtFormatConstraints_Validate_ThrowsOnEmptyAlgorithm()
    {
        var constraints = new SdJwtFormatConstraints { Alg = new[] { "" } };
        Assert.Throws<InvalidOperationException>(() => constraints.Validate());
    }

    [Fact]
    public void SdJwtFormatConstraints_Validate_ThrowsOnEmptySdAlgorithm()
    {
        var constraints = new SdJwtFormatConstraints { SdAlg = new[] { "" } };
        Assert.Throws<InvalidOperationException>(() => constraints.Validate());
    }

    [Fact]
    public void SdJwtFormatConstraints_Validate_ThrowsOnWhitespaceSdAlgorithm()
    {
        var constraints = new SdJwtFormatConstraints { SdAlg = new[] { "   " } };
        Assert.Throws<InvalidOperationException>(() => constraints.Validate());
    }

    [Fact]
    public void SdJwtFormatConstraints_Validate_ThrowsOnEmptyKbAlgorithm()
    {
        var constraints = new SdJwtFormatConstraints { KbAlg = new[] { "" } };
        Assert.Throws<InvalidOperationException>(() => constraints.Validate());
    }

    [Fact]
    public void SdJwtFormatConstraints_Validate_ThrowsOnWhitespaceKbAlgorithm()
    {
        var constraints = new SdJwtFormatConstraints { KbAlg = new[] { "  " } };
        Assert.Throws<InvalidOperationException>(() => constraints.Validate());
    }

    #endregion

    #region LdpFormatConstraints Tests

    [Fact]
    public void LdpFormatConstraints_DefaultState()
    {
        var constraints = new LdpFormatConstraints();
        Assert.Null(constraints.ProofType);
    }

    [Fact]
    public void LdpFormatConstraints_SetProofType()
    {
        var constraints = new LdpFormatConstraints
        {
            ProofType = new[] { "Ed25519Signature2018", "JsonWebSignature2020" }
        };

        Assert.NotNull(constraints.ProofType);
        Assert.Equal(2, constraints.ProofType.Length);
    }

    [Fact]
    public void LdpFormatConstraints_Validate_Succeeds()
    {
        var constraints = new LdpFormatConstraints
        {
            ProofType = new[] { "Ed25519Signature2018" }
        };

        constraints.Validate(); // Should not throw
    }

    [Fact]
    public void LdpFormatConstraints_Validate_ThrowsOnEmptyProofType()
    {
        var constraints = new LdpFormatConstraints { ProofType = new[] { "" } };
        Assert.Throws<InvalidOperationException>(() => constraints.Validate());
    }

    [Fact]
    public void LdpFormatConstraints_Validate_ThrowsOnWhitespaceProofType()
    {
        var constraints = new LdpFormatConstraints { ProofType = new[] { "  " } };
        Assert.Throws<InvalidOperationException>(() => constraints.Validate());
    }

    [Fact]
    public void LdpFormatConstraints_Validate_SucceedsWithNull()
    {
        var constraints = new LdpFormatConstraints();
        constraints.Validate(); // Should not throw
    }

    #endregion

    #region Constraints Tests

    [Fact]
    public void Constraints_DefaultState()
    {
        var constraints = new Constraints();

        Assert.Null(constraints.Fields);
        Assert.Null(constraints.LimitDisclosure);
        Assert.Null(constraints.SubjectIsIssuer);
        Assert.Null(constraints.IsHolder);
        Assert.Null(constraints.SameSubject);
        Assert.Null(constraints.ExtensionData);
    }

    [Fact]
    public void Constraints_SetAllProperties()
    {
        var constraints = new Constraints
        {
            Fields = new[] { new Field { Path = new[] { "$.name" } } },
            LimitDisclosure = "required",
            SubjectIsIssuer = "required",
            IsHolder = new[] { "did:example:holder" },
            SameSubject = new[] { "did:example:subject" },
            ExtensionData = new Dictionary<string, object> { ["custom"] = "value" }
        };

        Assert.NotNull(constraints.Fields);
        Assert.Single(constraints.Fields);
        Assert.Equal("required", constraints.LimitDisclosure);
        Assert.Equal("required", constraints.SubjectIsIssuer);
        Assert.NotNull(constraints.IsHolder);
        Assert.NotNull(constraints.SameSubject);
        Assert.NotNull(constraints.ExtensionData);
    }

    [Fact]
    public void Constraints_Validate_SucceedsWithNullFields()
    {
        var constraints = new Constraints();
        constraints.Validate(); // Should not throw
    }

    [Fact]
    public void Constraints_Validate_SucceedsWithValidFields()
    {
        var constraints = new Constraints
        {
            Fields = new[]
            {
                new Field { Path = new[] { "$.name" } },
                new Field { Path = new[] { "$.age" } }
            }
        };

        constraints.Validate(); // Should not throw
    }

    [Fact]
    public void Constraints_Validate_SucceedsWithValidLimitDisclosure()
    {
        var constraints = new Constraints { LimitDisclosure = "required" };
        constraints.Validate(); // Should not throw
    }

    [Fact]
    public void Constraints_Validate_SucceedsWithPreferredLimitDisclosure()
    {
        var constraints = new Constraints { LimitDisclosure = "preferred" };
        constraints.Validate(); // Should not throw
    }

    #endregion
}
