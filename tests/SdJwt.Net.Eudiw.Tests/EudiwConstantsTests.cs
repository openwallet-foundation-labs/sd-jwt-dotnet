using FluentAssertions;
using SdJwt.Net.Eudiw;
using Xunit;

namespace SdJwt.Net.Eudiw.Tests;

/// <summary>
/// Tests for EUDIW constants ensuring correct values for eIDAS 2.0 compliance.
/// </summary>
public class EudiwConstantsTests
{
    #region PID Constants

    [Fact]
    public void Pid_DocType_HasCorrectValue()
    {
        // Assert
        EudiwConstants.Pid.DocType.Should().Be("eu.europa.ec.eudi.pid.1");
    }

    [Fact]
    public void Pid_Namespace_HasCorrectValue()
    {
        // Assert
        EudiwConstants.Pid.Namespace.Should().Be("eu.europa.ec.eudi.pid.1");
    }

    [Fact]
    public void Pid_MandatoryClaims_ContainsRequiredFields()
    {
        // Assert
        EudiwConstants.Pid.MandatoryClaims.Should().Contain("family_name");
        EudiwConstants.Pid.MandatoryClaims.Should().Contain("given_name");
        EudiwConstants.Pid.MandatoryClaims.Should().Contain("birth_date");
        EudiwConstants.Pid.MandatoryClaims.Should().Contain("issuance_date");
        EudiwConstants.Pid.MandatoryClaims.Should().Contain("expiry_date");
        EudiwConstants.Pid.MandatoryClaims.Should().Contain("issuing_authority");
        EudiwConstants.Pid.MandatoryClaims.Should().Contain("issuing_country");
    }

    [Fact]
    public void Pid_OptionalClaims_ContainsExpectedFields()
    {
        // Assert
        EudiwConstants.Pid.OptionalClaims.Should().Contain("age_over_18");
        EudiwConstants.Pid.OptionalClaims.Should().Contain("age_over_21");
        EudiwConstants.Pid.OptionalClaims.Should().Contain("nationality");
        EudiwConstants.Pid.OptionalClaims.Should().Contain("resident_address");
    }

    #endregion

    #region mDL Constants

    [Fact]
    public void Mdl_DocType_HasCorrectValue()
    {
        // Assert
        EudiwConstants.Mdl.DocType.Should().Be("org.iso.18013.5.1.mDL");
    }

    [Fact]
    public void Mdl_Namespace_HasCorrectValue()
    {
        // Assert
        EudiwConstants.Mdl.Namespace.Should().Be("org.iso.18013.5.1");
    }

    #endregion

    #region Trust List Constants

    [Fact]
    public void TrustList_LotlUrl_HasCorrectValue()
    {
        // Assert
        EudiwConstants.TrustList.LotlUrl.Should().Be("https://ec.europa.eu/tools/lotl/eu-lotl.xml");
    }

    [Fact]
    public void TrustList_LotlJsonUrl_HasCorrectValue()
    {
        // Assert
        EudiwConstants.TrustList.LotlJsonUrl.Should().Be("https://eudi.ec.europa.eu/trust/lotl.json");
    }

    #endregion

    #region Algorithm Constants

    [Fact]
    public void Algorithms_SignatureAlgorithm_IsES256()
    {
        // Assert - ARF mandates ES256 minimum (HAIP Level 2)
        EudiwConstants.Algorithms.SignatureAlgorithm.Should().Be("ES256");
    }

    [Fact]
    public void Algorithms_DigestAlgorithm_IsSHA256()
    {
        // Assert
        EudiwConstants.Algorithms.DigestAlgorithm.Should().Be("SHA-256");
    }

    [Fact]
    public void Algorithms_SupportedAlgorithms_ContainsES256()
    {
        // Assert - Must support ES256 minimum
        EudiwConstants.Algorithms.SupportedAlgorithms.Should().Contain("ES256");
    }

    [Fact]
    public void Algorithms_SupportedAlgorithms_ContainsES384()
    {
        // Assert - Should support higher security levels
        EudiwConstants.Algorithms.SupportedAlgorithms.Should().Contain("ES384");
    }

    [Fact]
    public void Algorithms_SupportedAlgorithms_ContainsES512()
    {
        // Assert - Should support highest security level
        EudiwConstants.Algorithms.SupportedAlgorithms.Should().Contain("ES512");
    }

    #endregion

    #region Member State Codes

    [Fact]
    public void MemberStates_Contains27EuMemberStates()
    {
        // Assert - All 27 EU member states should be supported
        EudiwConstants.MemberStates.All.Should().HaveCount(27);
    }

    [Theory]
    [InlineData("DE")]
    [InlineData("FR")]
    [InlineData("IT")]
    [InlineData("ES")]
    [InlineData("NL")]
    [InlineData("BE")]
    [InlineData("AT")]
    [InlineData("PL")]
    public void MemberStates_ContainsMajorEuCountries(string countryCode)
    {
        // Assert
        EudiwConstants.MemberStates.All.Should().Contain(countryCode);
    }

    #endregion

    #region QEAA/EAA Type Constants

    [Fact]
    public void Qeaa_VctPrefix_HasCorrectValue()
    {
        // Assert - QEAA Verifiable Credential Type prefix
        EudiwConstants.Qeaa.VctPrefix.Should().Be("urn:eu:europa:ec:eudi:qeaa:");
    }

    [Fact]
    public void Eaa_VctPrefix_HasCorrectValue()
    {
        // Assert - EAA Verifiable Credential Type prefix
        EudiwConstants.Eaa.VctPrefix.Should().Be("urn:eu:europa:ec:eudi:eaa:");
    }

    #endregion
}
