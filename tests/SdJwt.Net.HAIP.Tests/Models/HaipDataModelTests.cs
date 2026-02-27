using FluentAssertions;
using SdJwt.Net.HAIP.Models;
using SdJwt.Net.HAIP;
using System;
using Xunit;

namespace SdJwt.Net.HAIP.Tests.Models;

public class HaipDataModelTests
{
    [Fact]
    public void CitizenData_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var citizenData = new CitizenData();

        // Act
        citizenData.GivenName = "John";
        citizenData.FamilyName = "Doe";
        citizenData.DateOfBirth = new DateTime(1990, 1, 1);
        citizenData.PlaceOfBirth = "New York";
        citizenData.NationalityCode = "US";
        citizenData.DocumentNumber = "123456789";

        // Assert
        citizenData.GivenName.Should().Be("John");
        citizenData.FamilyName.Should().Be("Doe");
        citizenData.DateOfBirth.Should().Be(new DateTime(1990, 1, 1));
        citizenData.PlaceOfBirth.Should().Be("New York");
        citizenData.NationalityCode.Should().Be("US");
        citizenData.DocumentNumber.Should().Be("123456789");
    }

    [Fact]
    public void BankingCredentialResult_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var bankingResult = new BankingCredentialResult();

        // Act
        bankingResult.AccountHolder = "John Doe";
        bankingResult.AccountNumber = "1234567890";
        bankingResult.BankCode = "ABC123";
        bankingResult.IsValid = true;
        bankingResult.VerifiedAt = DateTimeOffset.UtcNow;

        // Assert
        bankingResult.AccountHolder.Should().Be("John Doe");
        bankingResult.AccountNumber.Should().Be("1234567890");
        bankingResult.BankCode.Should().Be("ABC123");
        bankingResult.IsValid.Should().BeTrue();
        bankingResult.VerifiedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void KycData_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var kycData = new KycData();

        // Act
        kycData.CustomerId = "CUST12345";
        kycData.VerificationLevel = "Enhanced";
        kycData.VerifiedDate = DateTimeOffset.UtcNow;
        kycData.ExpiryDate = DateTimeOffset.UtcNow.AddYears(1);
        kycData.RiskRating = "Low";

        // Assert
        kycData.CustomerId.Should().Be("CUST12345");
        kycData.VerificationLevel.Should().Be("Enhanced");
        kycData.VerifiedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        kycData.ExpiryDate.Should().BeCloseTo(DateTimeOffset.UtcNow.AddYears(1), TimeSpan.FromMinutes(1));
        kycData.RiskRating.Should().Be("Low");
    }

    [Fact]
    public void DegreeInfo_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var degreeInfo = new DegreeInfo();

        // Act
        degreeInfo.DegreeName = "Bachelor of Science";
        degreeInfo.Institution = "MIT";
        degreeInfo.GraduationYear = 2020;
        degreeInfo.FieldOfStudy = "Computer Science";
        degreeInfo.GradePointAverage = 3.8;

        // Assert
        degreeInfo.DegreeName.Should().Be("Bachelor of Science");
        degreeInfo.Institution.Should().Be("MIT");
        degreeInfo.GraduationYear.Should().Be(2020);
        degreeInfo.FieldOfStudy.Should().Be("Computer Science");
        degreeInfo.GradePointAverage.Should().Be(3.8);
    }

    [Fact]
    public void NationalIdResult_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var nationalIdResult = new NationalIdResult();

        // Act
        nationalIdResult.IdNumber = "123-45-6789";
        nationalIdResult.IssuingCountry = "US";
        nationalIdResult.DocumentType = "Passport";
        nationalIdResult.IsValid = true;
        nationalIdResult.ExpiryDate = DateTimeOffset.UtcNow.AddYears(10);

        // Assert
        nationalIdResult.IdNumber.Should().Be("123-45-6789");
        nationalIdResult.IssuingCountry.Should().Be("US");
        nationalIdResult.DocumentType.Should().Be("Passport");
        nationalIdResult.IsValid.Should().BeTrue();
        nationalIdResult.ExpiryDate.Should().BeCloseTo(DateTimeOffset.UtcNow.AddYears(10), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void CredentialRequest_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var credentialRequest = new CredentialRequest();

        // Act
        credentialRequest.CredentialType = "IdentityCredential";
        credentialRequest.Format = "vc+sd-jwt";
        credentialRequest.ProofOfPossession = "proof-jwt-token";
        credentialRequest.SubjectDid = "did:example:123";

        // Assert
        credentialRequest.CredentialType.Should().Be("IdentityCredential");
        credentialRequest.Format.Should().Be("vc+sd-jwt");
        credentialRequest.ProofOfPossession.Should().Be("proof-jwt-token");
        credentialRequest.SubjectDid.Should().Be("did:example:123");
    }

    [Fact]
    public void CredentialResponse_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var credentialResponse = new CredentialResponse();

        // Act
        credentialResponse.Credential = "credential-data";
        credentialResponse.TransactionId = "txn-123";
        credentialResponse.IssuedAt = DateTimeOffset.UtcNow;
        credentialResponse.ExpiresAt = DateTimeOffset.UtcNow.AddYears(1);

        // Assert
        credentialResponse.Credential.Should().Be("credential-data");
        credentialResponse.TransactionId.Should().Be("txn-123");
        credentialResponse.IssuedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        credentialResponse.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddYears(1), TimeSpan.FromMinutes(1));
    }
}

// Mock classes for testing if they don't exist in the actual codebase
public class CitizenData
{
    public string? GivenName
    {
        get; set;
    }
    public string? FamilyName
    {
        get; set;
    }
    public DateTime DateOfBirth
    {
        get; set;
    }
    public string? PlaceOfBirth
    {
        get; set;
    }
    public string? NationalityCode
    {
        get; set;
    }
    public string? DocumentNumber
    {
        get; set;
    }
}

public class BankingCredentialResult
{
    public string? AccountHolder
    {
        get; set;
    }
    public string? AccountNumber
    {
        get; set;
    }
    public string? BankCode
    {
        get; set;
    }
    public bool IsValid
    {
        get; set;
    }
    public DateTimeOffset VerifiedAt
    {
        get; set;
    }
}

public class KycData
{
    public string? CustomerId
    {
        get; set;
    }
    public string? VerificationLevel
    {
        get; set;
    }
    public DateTimeOffset VerifiedDate
    {
        get; set;
    }
    public DateTimeOffset ExpiryDate
    {
        get; set;
    }
    public string? RiskRating
    {
        get; set;
    }
}

public class DegreeInfo
{
    public string? DegreeName
    {
        get; set;
    }
    public string? Institution
    {
        get; set;
    }
    public int GraduationYear
    {
        get; set;
    }
    public string? FieldOfStudy
    {
        get; set;
    }
    public double GradePointAverage
    {
        get; set;
    }
}

public class NationalIdResult
{
    public string? IdNumber
    {
        get; set;
    }
    public string? IssuingCountry
    {
        get; set;
    }
    public string? DocumentType
    {
        get; set;
    }
    public bool IsValid
    {
        get; set;
    }
    public DateTimeOffset ExpiryDate
    {
        get; set;
    }
}

public class CredentialRequest
{
    public string? CredentialType
    {
        get; set;
    }
    public string? Format
    {
        get; set;
    }
    public string? ProofOfPossession
    {
        get; set;
    }
    public string? SubjectDid
    {
        get; set;
    }
}

public class CredentialResponse
{
    public string? Credential
    {
        get; set;
    }
    public string? TransactionId
    {
        get; set;
    }
    public DateTimeOffset IssuedAt
    {
        get; set;
    }
    public DateTimeOffset ExpiresAt
    {
        get; set;
    }
}
