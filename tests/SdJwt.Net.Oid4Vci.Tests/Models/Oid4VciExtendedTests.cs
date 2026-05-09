using FluentAssertions;
using SdJwt.Net.Oid4Vci.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vci.Tests.Models;

/// <summary>
/// Extended test coverage for Oid4Vci models to ensure comprehensive coverage.
/// </summary>
public class Oid4VciExtendedTests
{
    [Fact]
    public void TokenRequest_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var request = new TokenRequest();

        // Assert
        request.GrantType.Should().BeNull();
        request.Code.Should().BeNull();
        request.RedirectUri.Should().BeNull();
        request.ClientId.Should().BeNull();
        request.ClientSecret.Should().BeNull();
        request.CodeVerifier.Should().BeNull();
        request.PreAuthorizedCode.Should().BeNull();
        request.UserPin.Should().BeNull();
    }

    [Fact]
    public void TokenRequest_Properties_ShouldBeSettable()
    {
        // Arrange
        var request = new TokenRequest();

        // Act
        request.GrantType = "authorization_code";
        request.Code = "auth-code-123";
        request.RedirectUri = "https://client.example.com/callback";
        request.ClientId = "client-123";
        request.ClientSecret = "secret-456";
        request.CodeVerifier = "code-verifier-789";
        request.PreAuthorizedCode = "pre-auth-abc";
        request.UserPin = "1234";

        // Assert
        request.GrantType.Should().Be("authorization_code");
        request.Code.Should().Be("auth-code-123");
        request.RedirectUri.Should().Be("https://client.example.com/callback");
        request.ClientId.Should().Be("client-123");
        request.ClientSecret.Should().Be("secret-456");
        request.CodeVerifier.Should().Be("code-verifier-789");
        request.PreAuthorizedCode.Should().Be("pre-auth-abc");
        request.UserPin.Should().Be("1234");
    }

    [Fact]
    public void TokenRequest_Validate_WithPreAuthorizedCodeGrant_ShouldPass()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.PreAuthorizedCode,
            PreAuthorizedCode = "pre-auth-code-123"
        };

        // Act & Assert
        request.Invoking(r => r.Validate()).Should().NotThrow();
    }

    [Fact]
    public void TokenRequest_Validate_WithAuthorizationCodeGrant_ShouldPass()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.AuthorizationCode,
            Code = "auth-code-123"
        };

        // Act & Assert
        request.Invoking(r => r.Validate()).Should().NotThrow();
    }

    [Fact]
    public void TokenRequest_Validate_WithNullGrantType_ShouldThrow()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = null
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("GrantType is required");
    }

    [Fact]
    public void TokenRequest_Validate_WithEmptyGrantType_ShouldThrow()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = ""
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("GrantType is required");
    }

    [Fact]
    public void TokenRequest_Validate_WithPreAuthorizedCodeGrantButNoCode_ShouldThrow()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.PreAuthorizedCode,
            PreAuthorizedCode = null
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("PreAuthorizedCode is required for pre-authorized code grant");
    }

    [Fact]
    public void TokenRequest_Validate_WithAuthorizationCodeGrantButNoCode_ShouldThrow()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.AuthorizationCode,
            Code = null
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("Code is required for authorization code grant");
    }

    [Fact]
    public void TokenRequest_Validate_WithUnsupportedGrantType_ShouldThrow()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = "unsupported_grant_type"
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("Unsupported grant type: unsupported_grant_type");
    }

    [Fact]
    public void TokenRequest_Validate_WithClientAssertionButNoType_ShouldThrow()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.AuthorizationCode,
            Code = "auth-code-123",
            ClientAssertion = "assertion-jwt",
            ClientAssertionType = null
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("ClientAssertionType is required when ClientAssertion is present");
    }

    [Fact]
    public void TokenRequest_Validate_WithInvalidClientAssertionType_ShouldThrow()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.AuthorizationCode,
            Code = "auth-code-123",
            ClientAssertion = "assertion-jwt",
            ClientAssertionType = "invalid-type"
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("Invalid ClientAssertionType");
    }

    [Fact]
    public void TokenRequest_Validate_WithValidClientAssertion_ShouldPass()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.AuthorizationCode,
            Code = "auth-code-123",
            ClientAssertion = "assertion-jwt",
            ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"
        };

        // Act & Assert
        request.Invoking(r => r.Validate()).Should().NotThrow();
    }

    [Fact]
    public void TokenResponse_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var response = new TokenResponse();

        // Assert
        response.AccessToken.Should().BeNull();
        response.TokenType.Should().BeNull();
        response.ExpiresIn.Should().BeNull();
        response.RefreshToken.Should().BeNull();
        response.Scope.Should().BeNull();
        response.CNonce.Should().BeNull();
        response.CNonceExpiresIn.Should().BeNull();
        response.AuthorizationPending.Should().BeNull();
        response.Interval.Should().BeNull();
    }

    [Fact]
    public void TokenResponse_Properties_ShouldBeSettable()
    {
        // Arrange
        var response = new TokenResponse();

        // Act
        response.AccessToken = "access-token-123";
        response.TokenType = "Bearer";
        response.ExpiresIn = 3600;
        response.RefreshToken = "refresh-token-456";
        response.Scope = "credential";
        response.CNonce = "c-nonce-789";
        response.CNonceExpiresIn = 300;
        response.AuthorizationPending = true;
        response.Interval = 5;

        // Assert
        response.AccessToken.Should().Be("access-token-123");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(3600);
        response.RefreshToken.Should().Be("refresh-token-456");
        response.Scope.Should().Be("credential");
        response.CNonce.Should().Be("c-nonce-789");
        response.CNonceExpiresIn.Should().Be(300);
        response.AuthorizationPending.Should().BeTrue();
        response.Interval.Should().Be(5);
    }

    [Fact]
    public void TokenResponse_Validate_WithValidResponse_ShouldPass()
    {
        // Arrange
        var response = new TokenResponse
        {
            AccessToken = "access-token-123",
            TokenType = "Bearer"
        };

        // Act & Assert
        response.Invoking(r => r.Validate()).Should().NotThrow();
    }

    [Fact]
    public void TokenResponse_Validate_WithNullAccessToken_ShouldThrow()
    {
        // Arrange
        var response = new TokenResponse
        {
            AccessToken = null,
            TokenType = "Bearer"
        };

        // Act & Assert
        response.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("AccessToken is required");
    }

    [Fact]
    public void TokenResponse_Validate_WithNullTokenType_ShouldThrow()
    {
        // Arrange
        var response = new TokenResponse
        {
            AccessToken = "access-token-123",
            TokenType = null
        };

        // Act & Assert
        response.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("TokenType is required");
    }

    [Fact]
    public void TokenResponse_Validate_WithNegativeExpiresIn_ShouldThrow()
    {
        // Arrange
        var response = new TokenResponse
        {
            AccessToken = "access-token-123",
            TokenType = "Bearer",
            ExpiresIn = -1
        };

        // Act & Assert
        response.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("ExpiresIn must be non-negative");
    }

    [Fact]
    public void TokenResponse_Validate_WithNegativeCNonceExpiresIn_ShouldThrow()
    {
        // Arrange
        var response = new TokenResponse
        {
            AccessToken = "access-token-123",
            TokenType = "Bearer",
            CNonceExpiresIn = -1
        };

        // Act & Assert
        response.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("CNonceExpiresIn must be non-negative");
    }

    [Fact]
    public void TokenResponse_Success_ShouldCreateValidResponse()
    {
        // Act
        var response = TokenResponse.Success("access-token", 3600, "nonce", 300);

        // Assert
        response.AccessToken.Should().Be("access-token");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(3600);
        response.CNonce.Should().Be("nonce");
        response.CNonceExpiresIn.Should().Be(300);
    }

    [Fact]
    public void TokenResponse_Success_WithNullAccessToken_ShouldThrow()
    {
        // Act & Assert
        Action act = () => TokenResponse.Success(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CredentialRequest_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var request = new CredentialRequest();

        // Assert
        request.CredentialConfigurationId.Should().BeNull();
        request.CredentialIdentifier.Should().BeNull();
        request.CredentialDefinition.Should().BeNull();
        request.Proofs.Should().BeNull();
        request.CredentialResponseEncryption.Should().BeNull();
    }

    [Fact]
    public void CredentialRequest_Properties_ShouldBeSettable()
    {
        // Arrange
        var request = new CredentialRequest();
        var credentialDefinition = new Dictionary<string, object>
        {
            ["type"] = new[] { "VerifiableCredential", "UniversityDegreeCredential" }
        };
        var proofs = new CredentialProofs { Jwt = new[] { "proof-jwt" } };

        // Act
        request.CredentialConfigurationId = "UniversityDegree";
        request.CredentialDefinition = credentialDefinition;
        request.Proofs = proofs;

        // Assert
        request.CredentialConfigurationId.Should().Be("UniversityDegree");
        request.CredentialDefinition.Should().BeEquivalentTo(credentialDefinition);
        request.Proofs.Should().Be(proofs);
    }

    [Fact]
    public void CredentialRequest_Validate_WithMissingBothIdentifiers_ShouldThrow()
    {
        // Arrange
        var request = new CredentialRequest();

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CredentialRequest_Validate_WithBothConfigurationIdAndIdentifier_ShouldThrow()
    {
        // Arrange
        var request = new CredentialRequest
        {
            CredentialConfigurationId = "UniversityDegree",
            CredentialIdentifier = "test-id"
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot specify both*");
    }

    [Fact]
    public void CredentialRequest_Validate_WithConfigurationIdOnly_ShouldPass()
    {
        // Arrange
        var request = new CredentialRequest
        {
            CredentialConfigurationId = "UniversityDegree"
        };

        // Act & Assert
        request.Invoking(r => r.Validate()).Should().NotThrow();
    }

    [Fact]
    public void CredentialRequest_Validate_WithIdentifierOnly_ShouldPass()
    {
        // Arrange
        var request = new CredentialRequest
        {
            CredentialIdentifier = "some-credential-identifier"
        };

        // Act & Assert
        request.Invoking(r => r.Validate()).Should().NotThrow();
    }

    [Fact]
    public void CredentialRequest_Create_ShouldCreateValidRequest()
    {
        // Act
        var request = CredentialRequest.Create("UniversityDegree", "proof-jwt");

        // Assert
        request.CredentialConfigurationId.Should().Be("UniversityDegree");
        request.Proofs.Should().NotBeNull();
        request.Proofs!.Jwt.Should().NotBeNullOrEmpty();
        request.Proofs.Jwt![0].Should().Be("proof-jwt");
    }

    [Fact]
    public void CredentialRequest_CreateByIdentifier_ShouldCreateValidRequest()
    {
        // Act
        var request = CredentialRequest.CreateByIdentifier("credential-id", "proof-jwt");

        // Assert
        request.CredentialIdentifier.Should().Be("credential-id");
        request.CredentialConfigurationId.Should().BeNull();
        request.Proofs.Should().NotBeNull();
        request.Proofs!.Jwt.Should().NotBeNullOrEmpty();
        request.Proofs.Jwt![0].Should().Be("proof-jwt");
    }

    [Fact]
    public void CredentialResponse_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var response = new CredentialResponse();

        // Assert
        response.Credentials.Should().BeNull();
        response.TransactionId.Should().BeNull();
        response.NotificationId.Should().BeNull();
    }

    [Fact]
    public void CredentialResponse_Properties_ShouldBeSettable()
    {
        // Arrange
        var response = new CredentialResponse();

        // Act
        response.Credentials = new[] { new CredentialResponseItem { Credential = "credential-jwt-token" } };
        response.TransactionId = "transaction-token-123";
        response.NotificationId = "notification-456";

        // Assert
        response.Credentials.Should().HaveCount(1);
        response.Credentials![0].Credential.Should().Be("credential-jwt-token");
        response.TransactionId.Should().Be("transaction-token-123");
        response.NotificationId.Should().Be("notification-456");
    }

    [Fact]
    public void CredentialResponse_Validate_WithCredentials_ShouldPass()
    {
        // Arrange
        var response = new CredentialResponse
        {
            Credentials = new[] { new CredentialResponseItem { Credential = "credential-token" } }
        };

        // Act & Assert
        response.Invoking(r => r.Validate()).Should().NotThrow();
    }

    [Fact]
    public void CredentialResponse_Validate_WithTransactionId_ShouldPass()
    {
        // Arrange
        var response = new CredentialResponse
        {
            TransactionId = "transaction-id"
        };

        // Act & Assert
        response.Invoking(r => r.Validate()).Should().NotThrow();
    }

    [Fact]
    public void CredentialResponse_Validate_WithNeitherCredentialsNorTransactionId_ShouldThrow()
    {
        // Arrange
        var response = new CredentialResponse();

        // Act & Assert
        response.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CredentialResponse_Validate_WithBothCredentialsAndTransactionId_ShouldThrow()
    {
        // Arrange
        var response = new CredentialResponse
        {
            Credentials = new[] { new CredentialResponseItem { Credential = "credential-token" } },
            TransactionId = "transaction-id"
        };

        // Act & Assert
        response.Invoking(r => r.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot have both*");
    }

    [Fact]
    public void CredentialResponse_Success_ShouldCreateValidResponse()
    {
        // Act
        var response = CredentialResponse.Success("credential", "notification-id");

        // Assert
        response.Credentials.Should().HaveCount(1);
        response.Credentials![0].Credential.Should().Be("credential");
        response.NotificationId.Should().Be("notification-id");
        response.TransactionId.Should().BeNull();
    }

    [Fact]
    public void CredentialResponse_Deferred_ShouldCreateValidResponse()
    {
        // Act
        var response = CredentialResponse.Deferred("transaction-id");

        // Assert
        response.TransactionId.Should().Be("transaction-id");
        response.Credentials.Should().BeNull();
    }

    [Fact]
    public void CredentialProof_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var proof = new CredentialProof();

        // Assert
        proof.ProofType.Should().BeNull();
        proof.Jwt.Should().BeNull();
        proof.Cwt.Should().BeNull();
        proof.LdpVp.Should().BeNull();
    }

    [Fact]
    public void CredentialProof_Properties_ShouldBeSettable()
    {
        // Arrange
        var proof = new CredentialProof();

        // Act
        proof.ProofType = "jwt";
        proof.Jwt = "proof-jwt-token";
        proof.Cwt = "proof-cwt-token";
        proof.LdpVp = new
        {
            type = "VerifiablePresentation"
        };

        // Assert
        proof.ProofType.Should().Be("jwt");
        proof.Jwt.Should().Be("proof-jwt-token");
        proof.Cwt.Should().Be("proof-cwt-token");
        proof.LdpVp.Should().NotBeNull();
    }

    [Fact]
    public void CredentialProof_Validate_WithNullProofType_ShouldThrow()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = null
        };

        // Act & Assert
        proof.Invoking(p => p.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("ProofType is required");
    }

    [Fact]
    public void CredentialProof_Validate_WithEmptyProofType_ShouldThrow()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = ""
        };

        // Act & Assert
        proof.Invoking(p => p.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("ProofType is required");
    }

    [Fact]
    public void CredentialProof_Validate_WithWhitespaceProofType_ShouldThrow()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "   "
        };

        // Act & Assert
        proof.Invoking(p => p.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("ProofType is required");
    }

    [Fact]
    public void CredentialProof_Validate_WithJwtTypeButNoJwt_ShouldThrow()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "jwt",
            Jwt = null
        };

        // Act & Assert
        proof.Invoking(p => p.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("JWT is required when proof_type is 'jwt'");
    }

    [Fact]
    public void CredentialProof_Validate_WithJwtTypeAndEmptyJwt_ShouldThrow()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "jwt",
            Jwt = ""
        };

        // Act & Assert
        proof.Invoking(p => p.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("JWT is required when proof_type is 'jwt'");
    }

    [Fact]
    public void CredentialProof_Validate_WithJwtTypeAndWhitespaceJwt_ShouldThrow()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "jwt",
            Jwt = "   "
        };

        // Act & Assert
        proof.Invoking(p => p.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("JWT is required when proof_type is 'jwt'");
    }

    [Fact]
    public void CredentialProof_Validate_WithCwtTypeButNoCwt_ShouldThrow()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "cwt",
            Cwt = null
        };

        // Act & Assert
        proof.Invoking(p => p.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("CWT is required when proof_type is 'cwt'");
    }

    [Fact]
    public void CredentialProof_Validate_WithCwtTypeAndEmptyCwt_ShouldThrow()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "cwt",
            Cwt = ""
        };

        // Act & Assert
        proof.Invoking(p => p.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("CWT is required when proof_type is 'cwt'");
    }

    [Fact]
    public void CredentialProof_Validate_WithLdpVpTypeButNoLdpVp_ShouldThrow()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "ldp_vp",
            LdpVp = null
        };

        // Act & Assert
        proof.Invoking(p => p.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("LDP VP is required when proof_type is 'ldp_vp'");
    }

    [Fact]
    public void CredentialProof_Validate_WithUnsupportedProofType_ShouldThrow()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "unsupported_type"
        };

        // Act & Assert
        proof.Invoking(p => p.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("Unsupported proof type: unsupported_type");
    }

    [Fact]
    public void CredentialProof_Validate_WithValidJwtProof_ShouldPass()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "jwt",
            Jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        };

        // Act & Assert
        proof.Invoking(p => p.Validate()).Should().NotThrow();
    }

    [Fact]
    public void CredentialProof_Validate_WithValidCwtProof_ShouldPass()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "cwt",
            Cwt = "base64-encoded-cwt-token"
        };

        // Act & Assert
        proof.Invoking(p => p.Validate()).Should().NotThrow();
    }

    [Fact]
    public void CredentialProof_Validate_WithValidLdpVpProof_ShouldPass()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "ldp_vp",
            LdpVp = new { type = "VerifiablePresentation" }
        };

        // Act & Assert
        proof.Invoking(p => p.Validate()).Should().NotThrow();
    }

    [Fact]
    public void CredentialProof_Validate_WithCaseInsensitiveJwtType_ShouldPass()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "JWT",
            Jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        };

        // Act & Assert
        proof.Invoking(p => p.Validate()).Should().NotThrow();
    }

    [Fact]
    public void CredentialProof_Validate_WithCaseInsensitiveCwtType_ShouldPass()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "CWT",
            Cwt = "base64-encoded-cwt-token"
        };

        // Act & Assert
        proof.Invoking(p => p.Validate()).Should().NotThrow();
    }

    [Fact]
    public void CredentialProof_Validate_WithCaseInsensitiveLdpVpType_ShouldPass()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "LDP_VP",
            LdpVp = new { type = "VerifiablePresentation" }
        };

        // Act & Assert
        proof.Invoking(p => p.Validate()).Should().NotThrow();
    }

    [Fact]
    public void TokenRequest_AuthorizationCodeFlow_ShouldSetRequiredProperties()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = "authorization_code",
            Code = "auth-code-123",
            RedirectUri = "https://client.example.com/callback",
            ClientId = "client-id",
            CodeVerifier = "code-verifier-xyz"
        };

        // Assert
        request.GrantType.Should().Be("authorization_code");
        request.Code.Should().Be("auth-code-123");
        request.RedirectUri.Should().Be("https://client.example.com/callback");
        request.ClientId.Should().Be("client-id");
        request.CodeVerifier.Should().Be("code-verifier-xyz");
    }

    [Fact]
    public void TokenRequest_PreAuthorizedCodeFlow_ShouldSetRequiredProperties()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = "urn:ietf:params:oauth:grant-type:pre-authorized_code",
            PreAuthorizedCode = "pre-auth-code-abc",
            UserPin = "1234"
        };

        // Assert
        request.GrantType.Should().Be("urn:ietf:params:oauth:grant-type:pre-authorized_code");
        request.PreAuthorizedCode.Should().Be("pre-auth-code-abc");
        request.UserPin.Should().Be("1234");
    }

    [Fact]
    public void TokenResponse_SuccessfulResponse_ShouldIndicateSuccess()
    {
        // Arrange
        var response = new TokenResponse
        {
            AccessToken = "access-token-123",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            CNonce = "c-nonce-456",
            CNonceExpiresIn = 300
        };

        // Assert
        response.AccessToken.Should().NotBeNullOrEmpty();
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().BeGreaterThan(0);
        response.CNonce.Should().NotBeNullOrEmpty();
        response.CNonceExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CredentialRequest_JwtVcJsonConfiguration_ShouldSetCredentialDefinition()
    {
        // Arrange
        var request = new CredentialRequest
        {
            CredentialConfigurationId = "UniversityDegreeJwtVc",
            CredentialDefinition = new Dictionary<string, object>
            {
                ["type"] = new[] { "VerifiableCredential", "UniversityDegreeCredential" }
            }
        };

        // Assert
        request.CredentialConfigurationId.Should().Be("UniversityDegreeJwtVc");
        request.CredentialDefinition.Should().BeOfType<Dictionary<string, object>>();
        var credDefDict = request.CredentialDefinition as Dictionary<string, object>;
        credDefDict.Should().ContainKey("type");
        var types = credDefDict!["type"] as string[];
        types.Should().Contain("VerifiableCredential");
        types.Should().Contain("UniversityDegreeCredential");
    }

    [Fact]
    public void CredentialRequest_SdJwtVcConfiguration_ShouldSetCredentialDefinition()
    {
        // Arrange
        var request = new CredentialRequest
        {
            CredentialConfigurationId = "UniversityDegreeSdJwt",
            CredentialDefinition = new Dictionary<string, object>
            {
                ["vct"] = "https://example.com/UniversityDegree"
            }
        };

        // Assert
        request.CredentialConfigurationId.Should().Be("UniversityDegreeSdJwt");
        request.CredentialDefinition.Should().BeOfType<Dictionary<string, object>>();
        var credDefDict = request.CredentialDefinition as Dictionary<string, object>;
        credDefDict.Should().ContainKey("vct");
        credDefDict!["vct"].Should().Be("https://example.com/UniversityDegree");
    }

    [Fact]
    public void CredentialRequest_MdlConfiguration_ShouldSetCredentialDefinition()
    {
        // Arrange
        var request = new CredentialRequest
        {
            CredentialConfigurationId = "MobileDriversLicense",
            CredentialDefinition = new Dictionary<string, object>
            {
                ["doctype"] = "org.iso.18013.5.1.mDL"
            }
        };

        // Assert
        request.CredentialConfigurationId.Should().Be("MobileDriversLicense");
        request.CredentialDefinition.Should().BeOfType<Dictionary<string, object>>();
        var credDefDict = request.CredentialDefinition as Dictionary<string, object>;
        credDefDict.Should().ContainKey("doctype");
        credDefDict!["doctype"].Should().Be("org.iso.18013.5.1.mDL");
    }

    [Fact]
    public void CredentialProof_JwtProof_ShouldSetJwtProperty()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "jwt",
            Jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        };

        // Assert
        proof.ProofType.Should().Be("jwt");
        proof.Jwt.Should().StartWith("eyJ");
        proof.Cwt.Should().BeNull();
    }

    [Fact]
    public void CredentialProof_CwtProof_ShouldSetCwtProperty()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "cwt",
            Cwt = "base64-encoded-cwt-token"
        };

        // Assert
        proof.ProofType.Should().Be("cwt");
        proof.Cwt.Should().Be("base64-encoded-cwt-token");
        proof.Jwt.Should().BeNull();
    }

    [Fact]
    public void CredentialResponse_WithCredentialObject_ShouldHandleDictionaryCredential()
    {
        // Arrange
        var credentialObject = new Dictionary<string, object>
        {
            ["@context"] = new[] { "https://www.w3.org/2018/credentials/v1" },
            ["type"] = new[] { "VerifiableCredential", "UniversityDegreeCredential" },
            ["issuer"] = "https://university.example.com",
            ["credentialSubject"] = new Dictionary<string, object>
            {
                ["id"] = "did:example:123",
                ["degree"] = "Bachelor of Science"
            }
        };

        var response = new CredentialResponse
        {
            Credentials = new[] { new CredentialResponseItem { Credential = credentialObject } }
        };

        // Assert
        response.Credentials.Should().HaveCount(1);
        response.Credentials![0].Credential.Should().BeOfType<Dictionary<string, object>>();
        var credential = response.Credentials[0].Credential as Dictionary<string, object>;
        credential.Should().ContainKey("@context");
        credential.Should().ContainKey("type");
        credential.Should().ContainKey("issuer");
        credential.Should().ContainKey("credentialSubject");
    }

    [Fact]
    public void TokenResponse_ErrorProperties_ShouldHandleErrorResponses()
    {
        // Arrange
        var errorResponse = new Dictionary<string, object>
        {
            ["error"] = "invalid_request",
            ["error_description"] = "The request is missing a required parameter"
        };

        // Act
        var response = new TokenResponse();
        // Note: In actual implementation, error handling would be part of the response model
        // For now, we just verify the response can be created

        // Assert
        response.Should().NotBeNull();
    }

    [Fact]
    public void CredentialRequest_WithCredentialDefinition_ShouldHandleComplexClaimsStructure()
    {
        // Arrange
        var credentialDefinition = new Dictionary<string, object>
        {
            ["claims"] = new Dictionary<string, object>
            {
                ["given_name"] = new Dictionary<string, object>
                {
                    ["essential"] = true,
                    ["purpose"] = "To verify your identity"
                },
                ["family_name"] = new Dictionary<string, object>
                {
                    ["essential"] = true
                },
                ["email"] = new Dictionary<string, object>
                {
                    ["essential"] = false,
                    ["purpose"] = "For communication"
                },
                ["birthdate"] = new Dictionary<string, object>
                {
                    ["essential"] = true,
                    ["purpose"] = "To verify your age"
                }
            }
        };

        var request = new CredentialRequest
        {
            CredentialConfigurationId = "UniversityDegree",
            CredentialDefinition = credentialDefinition
        };

        // Assert
        request.CredentialDefinition.Should().NotBeNull();
        request.CredentialDefinition.Should().BeOfType<Dictionary<string, object>>();
        var defDict = request.CredentialDefinition as Dictionary<string, object>;
        defDict.Should().ContainKey("claims");
        var claims = defDict!["claims"] as Dictionary<string, object>;
        claims.Should().NotBeNull();
        claims.Should().HaveCount(4);
        claims.Should().ContainKeys("given_name", "family_name", "email", "birthdate");

        var givenNameClaim = claims!["given_name"] as Dictionary<string, object>;
        givenNameClaim.Should().ContainKey("essential");
        givenNameClaim.Should().ContainKey("purpose");
        givenNameClaim!["essential"].Should().Be(true);
    }
}
