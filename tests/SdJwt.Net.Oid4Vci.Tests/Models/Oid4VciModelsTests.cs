using FluentAssertions;
using SdJwt.Net.Oid4Vci.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vci.Tests.Models;

public class CredentialRequestTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateCredentialRequest()
    {
        // Arrange
        var format = "vc+sd-jwt";
        var proof = new CredentialProof
        {
            ProofType = "jwt",
            Jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        };

        // Act
        var request = new CredentialRequest
        {
            Format = format,
            Proof = proof
        };

        // Assert
        request.Should().NotBeNull();
        request.Format.Should().Be(format);
        request.Proof.Should().Be(proof);
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotThrow()
    {
        // Arrange
        var request = new CredentialRequest
        {
            Format = "vc+sd-jwt",
            Vct = "ExampleCredential",
            Proof = new CredentialProof
            {
                ProofType = "jwt",
                Jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
            }
        };

        // Act & Assert
        var act = () => request.Validate();
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidFormat_ShouldThrow(string? invalidFormat)
    {
        // Arrange
        var request = new CredentialRequest
        {
            Format = invalidFormat!,
            Proof = new CredentialProof
            {
                ProofType = "jwt",
                Jwt = "valid-jwt"
            }
        };

        // Act & Assert
        var act = () => request.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Format*required*");
    }

    [Fact]
    public void Validate_WithMissingVctAndIdentifier_ShouldThrow()
    {
        // Arrange
        var request = new CredentialRequest
        {
            Format = "vc+sd-jwt",
            // Missing both Vct and CredentialIdentifier
            Proof = new CredentialProof
            {
                ProofType = "jwt",
                Jwt = "valid-jwt"
            }
        };

        // Act & Assert
        var act = () => request.Validate();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Validate_WithInvalidProof_ShouldThrow()
    {
        // Arrange
        var request = new CredentialRequest
        {
            Format = "vc+sd-jwt",
            Vct = "ExampleCredential",
            Proof = new CredentialProof
            {
                ProofType = "", // Invalid proof type
                Jwt = "valid-jwt"
            }
        };

        // Act & Assert
        var act = () => request.Validate();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Validate_WithCredentialDefinitionAndIdentifier_ShouldThrow()
    {
        // Arrange
        var request = new CredentialRequest
        {
            Format = "vc+sd-jwt",
            CredentialDefinition = new { type = "ExampleCredential" },
            CredentialIdentifier = "example_credential",
            Proof = new CredentialProof
            {
                ProofType = "jwt",
                Jwt = "valid-jwt"
            }
        };

        // Act & Assert
        var act = () => request.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Cannot specify both*");
    }

    [Fact]
    public void Create_WithValidParameters_ShouldCreateRequest()
    {
        // Arrange
        var vct = "ExampleCredential";
        var proofJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

        // Act
        var request = CredentialRequest.Create(vct, proofJwt);

        // Assert
        request.Should().NotBeNull();
        request.Format.Should().Be(Oid4VciConstants.SdJwtVcFormat);
        request.Vct.Should().Be(vct);
        request.Proof.Should().NotBeNull();
        request.Proof!.ProofType.Should().Be(Oid4VciConstants.ProofTypes.Jwt);
        request.Proof.Jwt.Should().Be(proofJwt);
    }

    [Fact]
    public void CreateByIdentifier_WithValidParameters_ShouldCreateRequest()
    {
        // Arrange
        var credentialIdentifier = "example_credential";
        var proofJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

        // Act
        var request = CredentialRequest.CreateByIdentifier(credentialIdentifier, proofJwt);

        // Assert
        request.Should().NotBeNull();
        request.Format.Should().Be(Oid4VciConstants.SdJwtVcFormat);
        request.CredentialIdentifier.Should().Be(credentialIdentifier);
        request.Proof.Should().NotBeNull();
        request.Proof!.ProofType.Should().Be(Oid4VciConstants.ProofTypes.Jwt);
        request.Proof.Jwt.Should().Be(proofJwt);
    }
}

public class CredentialResponseTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateCredentialResponse()
    {
        // Arrange
        var credential = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

        // Act
        var response = new CredentialResponse
        {
            Credential = credential
        };

        // Assert
        response.Should().NotBeNull();
        response.Credential.Should().Be(credential);
    }

    [Fact]
    public void Validate_WithValidResponse_ShouldNotThrow()
    {
        // Arrange
        var response = new CredentialResponse
        {
            Credential = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        };

        // Act & Assert
        var act = () => response.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithMissingBothCredentialAndAcceptanceToken_ShouldThrow()
    {
        // Arrange
        var response = new CredentialResponse();

        // Act & Assert
        var act = () => response.Validate();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Validate_WithBothCredentialAndAcceptanceToken_ShouldThrow()
    {
        // Arrange
        var response = new CredentialResponse
        {
            Credential = "credential",
            AcceptanceToken = "token"
        };

        // Act & Assert
        var act = () => response.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Cannot have both*");
    }

    [Fact]
    public void Success_ShouldCreateSuccessfulResponse()
    {
        // Arrange
        var credential = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        var cNonce = "custom-nonce";
        var expiresIn = 7200;
        var notificationId = "notification-123";

        // Act
        var response = CredentialResponse.Success(credential, cNonce, expiresIn, notificationId);

        // Assert
        response.Should().NotBeNull();
        response.Credential.Should().Be(credential);
        response.CNonce.Should().Be(cNonce);
        response.CNonceExpiresIn.Should().Be(expiresIn);
        response.NotificationId.Should().Be(notificationId);
    }

    [Fact]
    public void Deferred_ShouldCreateDeferredResponse()
    {
        // Arrange
        var acceptanceToken = "acceptance-token-123";

        // Act
        var response = CredentialResponse.Deferred(acceptanceToken);

        // Assert
        response.Should().NotBeNull();
        response.AcceptanceToken.Should().Be(acceptanceToken);
        response.TransactionId.Should().Be(acceptanceToken);
        response.Credential.Should().BeNull();
    }
}

public class CredentialProofTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateCredentialProof()
    {
        // Arrange
        var proofType = "jwt";
        var jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

        // Act
        var proof = new CredentialProof
        {
            ProofType = proofType,
            Jwt = jwt
        };

        // Assert
        proof.Should().NotBeNull();
        proof.ProofType.Should().Be(proofType);
        proof.Jwt.Should().Be(jwt);
    }

    [Fact]
    public void Validate_WithValidProof_ShouldNotThrow()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "jwt",
            Jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        };

        // Act & Assert
        var act = () => proof.Validate();
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidProofType_ShouldThrow(string? invalidProofType)
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = invalidProofType!,
            Jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        };

        // Act & Assert
        var act = () => proof.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*ProofType*required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidJwt_ShouldThrow(string? invalidJwt)
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "jwt",
            Jwt = invalidJwt!
        };

        // Act & Assert
        var act = () => proof.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*JWT*required*");
    }

    [Fact]
    public void Validate_WithUnsupportedProofType_ShouldThrow()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "unsupported-type",
            Jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        };

        // Act & Assert
        var act = () => proof.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Unsupported proof type*");
    }

    [Fact]
    public void Validate_WithCwtProofType_ShouldValidate()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "cwt",
            Cwt = "valid-cwt"
        };

        // Act & Assert
        var act = () => proof.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithCwtProofTypeMissingCwt_ShouldThrow()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "cwt"
        };

        // Act & Assert
        var act = () => proof.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*CWT*required*");
    }

    [Fact]
    public void Validate_WithLdpVpProofType_ShouldValidate()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "ldp_vp",
            LdpVp = new { type = "VerifiablePresentation" }
        };

        // Act & Assert
        var act = () => proof.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithLdpVpProofTypeMissingLdpVp_ShouldThrow()
    {
        // Arrange
        var proof = new CredentialProof
        {
            ProofType = "ldp_vp"
        };

        // Act & Assert
        var act = () => proof.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*LDP VP*required*");
    }
}

public class TokenRequestTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateTokenRequest()
    {
        // Arrange
        var grantType = "urn:ietf:params:oauth:grant-type:pre-authorized_code";
        var preAuthorizedCode = "code-123456";

        // Act
        var request = new TokenRequest
        {
            GrantType = grantType,
            PreAuthorizedCode = preAuthorizedCode
        };

        // Assert
        request.Should().NotBeNull();
        request.GrantType.Should().Be(grantType);
        request.PreAuthorizedCode.Should().Be(preAuthorizedCode);
    }

    [Fact]
    public void Validate_WithValidPreAuthorizedCodeRequest_ShouldNotThrow()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.PreAuthorizedCode,
            PreAuthorizedCode = "code-123456"
        };

        // Act & Assert
        var act = () => request.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithValidAuthorizationCodeRequest_ShouldNotThrow()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.AuthorizationCode,
            Code = "auth-code-123",
            RedirectUri = "https://client.example.com/callback"
        };

        // Act & Assert
        var act = () => request.Validate();
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidGrantType_ShouldThrow(string? invalidGrantType)
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = invalidGrantType!
        };

        // Act & Assert
        var act = () => request.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*GrantType*required*");
    }

    [Fact]
    public void Validate_WithUnsupportedGrantType_ShouldThrow()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = "unsupported-grant-type"
        };

        // Act & Assert
        var act = () => request.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Unsupported grant type*");
    }

    [Fact]
    public void Validate_WithPreAuthorizedCodeGrantMissingCode_ShouldThrow()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.PreAuthorizedCode
            // Missing PreAuthorizedCode
        };

        // Act & Assert
        var act = () => request.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*PreAuthorizedCode*required*");
    }

    [Fact]
    public void Validate_WithAuthorizationCodeGrantMissingCode_ShouldThrow()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.AuthorizationCode,
            RedirectUri = "https://client.example.com/callback"
            // Missing Code
        };

        // Act & Assert
        var act = () => request.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Code*required*");
    }

    [Fact]
    public void Validate_WithClientAssertion_ShouldValidateClientAssertionType()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.PreAuthorizedCode,
            PreAuthorizedCode = "code-123",
            ClientAssertion = "jwt-assertion",
            ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"
        };

        // Act & Assert
        var act = () => request.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithClientAssertionMissingType_ShouldThrow()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.PreAuthorizedCode,
            PreAuthorizedCode = "code-123",
            ClientAssertion = "jwt-assertion"
            // Missing ClientAssertionType
        };

        // Act & Assert
        var act = () => request.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*ClientAssertionType*required*");
    }

    [Fact]
    public void Validate_WithInvalidClientAssertionType_ShouldThrow()
    {
        // Arrange
        var request = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.PreAuthorizedCode,
            PreAuthorizedCode = "code-123",
            ClientAssertion = "jwt-assertion",
            ClientAssertionType = "invalid-type"
        };

        // Act & Assert
        var act = () => request.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Invalid ClientAssertionType*");
    }
}

public class TokenResponseTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateTokenResponse()
    {
        // Arrange
        var accessToken = "access-token-123";
        var tokenType = "Bearer";
        var expiresIn = 3600;

        // Act
        var response = new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = tokenType,
            ExpiresIn = expiresIn
        };

        // Assert
        response.Should().NotBeNull();
        response.AccessToken.Should().Be(accessToken);
        response.TokenType.Should().Be(tokenType);
        response.ExpiresIn.Should().Be(expiresIn);
    }

    [Fact]
    public void Validate_WithValidResponse_ShouldNotThrow()
    {
        // Arrange
        var response = new TokenResponse
        {
            AccessToken = "access-token-123",
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        // Act & Assert
        var act = () => response.Validate();
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidAccessToken_ShouldThrow(string? invalidToken)
    {
        // Arrange
        var response = new TokenResponse
        {
            AccessToken = invalidToken!,
            TokenType = "Bearer"
        };

        // Act & Assert
        var act = () => response.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*AccessToken*required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidTokenType_ShouldThrow(string? invalidTokenType)
    {
        // Arrange
        var response = new TokenResponse
        {
            AccessToken = "valid-token",
            TokenType = invalidTokenType!
        };

        // Act & Assert
        var act = () => response.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*TokenType*required*");
    }

    [Fact]
    public void Validate_WithNegativeExpiresIn_ShouldThrow()
    {
        // Arrange
        var response = new TokenResponse
        {
            AccessToken = "valid-token",
            TokenType = "Bearer",
            ExpiresIn = -1
        };

        // Act & Assert
        var act = () => response.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*ExpiresIn*non-negative*");
    }

    [Fact]
    public void Validate_WithNegativeCNonceExpiresIn_ShouldThrow()
    {
        // Arrange
        var response = new TokenResponse
        {
            AccessToken = "valid-token",
            TokenType = "Bearer",
            CNonceExpiresIn = -1
        };

        // Act & Assert
        var act = () => response.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*CNonceExpiresIn*non-negative*");
    }

    [Fact]
    public void Success_ShouldCreateSuccessfulResponse()
    {
        // Arrange
        var accessToken = "access-token-123";

        // Act
        var response = TokenResponse.Success(accessToken);

        // Assert
        response.Should().NotBeNull();
        response.AccessToken.Should().Be(accessToken);
        response.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public void Success_WithCustomParameters_ShouldUseProvidedValues()
    {
        // Arrange
        var accessToken = "access-token-123";
        var expiresIn = 7200;
        var cNonce = "custom-nonce";
        var cNonceExpiresIn = 1800;

        // Act
        var response = TokenResponse.Success(accessToken, expiresIn, cNonce, cNonceExpiresIn);

        // Assert
        response.Should().NotBeNull();
        response.AccessToken.Should().Be(accessToken);
        response.ExpiresIn.Should().Be(expiresIn);
        response.CNonce.Should().Be(cNonce);
        response.CNonceExpiresIn.Should().Be(cNonceExpiresIn);
    }
}

public class CredentialOfferTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateCredentialOffer()
    {
        // Arrange
        var credentialIssuer = "https://issuer.example.com";
        var configurationIds = new[] { "UniversityDegreeCredential", "EmployeeIDCredential" };

        // Act
        var offer = new CredentialOffer
        {
            CredentialIssuer = credentialIssuer,
            CredentialConfigurationIds = configurationIds
        };

        // Assert
        offer.Should().NotBeNull();
        offer.CredentialIssuer.Should().Be(credentialIssuer);
        offer.CredentialConfigurationIds.Should().BeEquivalentTo(configurationIds);
    }

    [Fact]
    public void AddPreAuthorizedCodeGrant_ShouldAddGrant()
    {
        // Arrange
        var offer = new CredentialOffer
        {
            CredentialIssuer = "https://issuer.example.com",
            CredentialConfigurationIds = new[] { "UniversityDegreeCredential" }
        };
        var preAuthorizedCode = "code-123";
        var transactionCode = new TransactionCode { Length = 4, InputMode = "numeric" };
        var interval = 5;

        // Act
        offer.AddPreAuthorizedCodeGrant(preAuthorizedCode, transactionCode, interval);

        // Assert
        offer.Grants.Should().NotBeNull();
        offer.Grants!.Should().ContainKey(Oid4VciConstants.GrantTypes.PreAuthorizedCode);

        var grant = offer.GetPreAuthorizedCodeGrant();
        grant.Should().NotBeNull();
        grant!.PreAuthorizedCode.Should().Be(preAuthorizedCode);
        grant.TransactionCode.Should().Be(transactionCode);
        grant.Interval.Should().Be(interval);
    }

    [Fact]
    public void AddAuthorizationCodeGrant_ShouldAddGrant()
    {
        // Arrange
        var offer = new CredentialOffer
        {
            CredentialIssuer = "https://issuer.example.com",
            CredentialConfigurationIds = new[] { "UniversityDegreeCredential" }
        };
        var issuerState = "state-123";
        var authorizationServer = "https://auth.example.com";

        // Act
        offer.AddAuthorizationCodeGrant(issuerState, authorizationServer);

        // Assert
        offer.Grants.Should().NotBeNull();
        offer.Grants!.Should().ContainKey(Oid4VciConstants.GrantTypes.AuthorizationCode);

        var grant = offer.GetAuthorizationCodeGrant();
        grant.Should().NotBeNull();
        grant!.IssuerState.Should().Be(issuerState);
        grant.AuthorizationServer.Should().Be(authorizationServer);
    }

    [Fact]
    public void GetPreAuthorizedCodeGrant_WithNoGrant_ShouldReturnNull()
    {
        // Arrange
        var offer = new CredentialOffer
        {
            CredentialIssuer = "https://issuer.example.com",
            CredentialConfigurationIds = new[] { "UniversityDegreeCredential" }
        };

        // Act
        var grant = offer.GetPreAuthorizedCodeGrant();

        // Assert
        grant.Should().BeNull();
    }

    [Fact]
    public void GetAuthorizationCodeGrant_WithNoGrant_ShouldReturnNull()
    {
        // Arrange
        var offer = new CredentialOffer
        {
            CredentialIssuer = "https://issuer.example.com",
            CredentialConfigurationIds = new[] { "UniversityDegreeCredential" }
        };

        // Act
        var grant = offer.GetAuthorizationCodeGrant();

        // Assert
        grant.Should().BeNull();
    }
}

public class PreAuthorizedCodeGrantTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateGrant()
    {
        // Arrange
        var preAuthorizedCode = "code-123456";
        var transactionCode = new TransactionCode { Length = 6, InputMode = "numeric" };
        var interval = 10;

        // Act
        var grant = new PreAuthorizedCodeGrant
        {
            PreAuthorizedCode = preAuthorizedCode,
            TransactionCode = transactionCode,
            Interval = interval
        };

        // Assert
        grant.Should().NotBeNull();
        grant.PreAuthorizedCode.Should().Be(preAuthorizedCode);
        grant.TransactionCode.Should().Be(transactionCode);
        grant.Interval.Should().Be(interval);
    }
}

public class AuthorizationCodeGrantTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateGrant()
    {
        // Arrange
        var issuerState = "state-123456";
        var authorizationServer = "https://auth.example.com";

        // Act
        var grant = new AuthorizationCodeGrant
        {
            IssuerState = issuerState,
            AuthorizationServer = authorizationServer
        };

        // Assert
        grant.Should().NotBeNull();
        grant.IssuerState.Should().Be(issuerState);
        grant.AuthorizationServer.Should().Be(authorizationServer);
    }
}

public class TransactionCodeTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateTransactionCode()
    {
        // Arrange
        var length = 4;
        var inputMode = "numeric";
        var description = "Please enter the PIN";

        // Act
        var transactionCode = new TransactionCode
        {
            Length = length,
            InputMode = inputMode,
            Description = description
        };

        // Assert
        transactionCode.Should().NotBeNull();
        transactionCode.Length.Should().Be(length);
        transactionCode.InputMode.Should().Be(inputMode);
        transactionCode.Description.Should().Be(description);
    }

    [Fact]
    public void Constructor_WithMinimalParameters_ShouldCreateTransactionCode()
    {
        // Act
        var transactionCode = new TransactionCode();

        // Assert
        transactionCode.Should().NotBeNull();
        transactionCode.Length.Should().BeNull();
        transactionCode.InputMode.Should().BeNull();
        transactionCode.Description.Should().BeNull();
    }
}

public class CredentialErrorResponseTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateErrorResponse()
    {
        // Arrange
        var error = "invalid_request";
        var description = "The request is malformed";
        var errorUri = "https://issuer.example.com/errors/invalid_request";
        var cNonce = "fresh-nonce";
        var cNonceExpiresIn = 300;

        // Act
        var response = new CredentialErrorResponse
        {
            Error = error,
            ErrorDescription = description,
            ErrorUri = errorUri,
            CNonce = cNonce,
            CNonceExpiresIn = cNonceExpiresIn
        };

        // Assert
        response.Should().NotBeNull();
        response.Error.Should().Be(error);
        response.ErrorDescription.Should().Be(description);
        response.ErrorUri.Should().Be(errorUri);
        response.CNonce.Should().Be(cNonce);
        response.CNonceExpiresIn.Should().Be(cNonceExpiresIn);
    }

    [Fact]
    public void Create_WithValidParameters_ShouldCreateErrorResponse()
    {
        // Arrange
        var error = "invalid_proof";
        var description = "The proof is invalid";
        var errorUri = "https://issuer.example.com/errors/invalid_proof";
        var cNonce = "fresh-nonce";
        var cNonceExpiresIn = 300;

        // Act
        var response = CredentialErrorResponse.Create(error, description, errorUri, cNonce, cNonceExpiresIn);

        // Assert
        response.Should().NotBeNull();
        response.Error.Should().Be(error);
        response.ErrorDescription.Should().Be(description);
        response.ErrorUri.Should().Be(errorUri);
        response.CNonce.Should().Be(cNonce);
        response.CNonceExpiresIn.Should().Be(cNonceExpiresIn);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidError_ShouldThrow(string? invalidError)
    {
        // Act & Assert
        var act = () => CredentialErrorResponse.Create(invalidError!);
        act.Should().Throw<ArgumentException>();
    }
}

public class TokenErrorResponseTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateErrorResponse()
    {
        // Arrange
        var error = "invalid_grant";
        var description = "The provided grant is invalid";
        var errorUri = "https://issuer.example.com/errors/invalid_grant";

        // Act
        var response = new TokenErrorResponse
        {
            Error = error,
            ErrorDescription = description,
            ErrorUri = errorUri
        };

        // Assert
        response.Should().NotBeNull();
        response.Error.Should().Be(error);
        response.ErrorDescription.Should().Be(description);
        response.ErrorUri.Should().Be(errorUri);
    }

    [Fact]
    public void Create_WithValidParameters_ShouldCreateErrorResponse()
    {
        // Arrange
        var error = "unsupported_grant_type";
        var description = "The grant type is not supported";
        var errorUri = "https://issuer.example.com/errors/unsupported_grant_type";

        // Act
        var response = TokenErrorResponse.Create(error, description, errorUri);

        // Assert
        response.Should().NotBeNull();
        response.Error.Should().Be(error);
        response.ErrorDescription.Should().Be(description);
        response.ErrorUri.Should().Be(errorUri);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidError_ShouldThrow(string? invalidError)
    {
        // Act & Assert
        var act = () => TokenErrorResponse.Create(invalidError!);
        act.Should().Throw<ArgumentException>();
    }
}

public class Oid4VciConstantsTests
{
    [Fact]
    public void GrantTypes_ShouldHaveCorrectValues()
    {
        // Assert
        Oid4VciConstants.GrantTypes.PreAuthorizedCode.Should().Be("urn:ietf:params:oauth:grant-type:pre-authorized_code");
        Oid4VciConstants.GrantTypes.AuthorizationCode.Should().Be("authorization_code");
        Oid4VciConstants.GrantTypes.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public void ProofTypes_ShouldHaveCorrectValues()
    {
        // Assert
        Oid4VciConstants.ProofTypes.Jwt.Should().Be("jwt");
        Oid4VciConstants.ProofTypes.Cwt.Should().Be("cwt");
        Oid4VciConstants.ProofTypes.LdpVp.Should().Be("ldp_vp");
    }

    [Fact]
    public void SdJwtVcFormat_ShouldHaveCorrectValue()
    {
        // Assert
        Oid4VciConstants.SdJwtVcFormat.Should().Be("vc+sd-jwt");
    }

    [Fact]
    public void ProofJwtType_ShouldHaveCorrectValue()
    {
        // Assert
        Oid4VciConstants.ProofJwtType.Should().Be("openid4vci-proof+jwt");
    }

    [Fact]
    public void TokenErrorCodes_ShouldHaveCorrectValues()
    {
        // Assert
        Oid4VciConstants.TokenErrorCodes.InvalidRequest.Should().Be("invalid_request");
        Oid4VciConstants.TokenErrorCodes.InvalidClient.Should().Be("invalid_client");
        Oid4VciConstants.TokenErrorCodes.InvalidGrant.Should().Be("invalid_grant");
        Oid4VciConstants.TokenErrorCodes.UnauthorizedClient.Should().Be("unauthorized_client");
        Oid4VciConstants.TokenErrorCodes.UnsupportedGrantType.Should().Be("unsupported_grant_type");
        Oid4VciConstants.TokenErrorCodes.InvalidScope.Should().Be("invalid_scope");
        Oid4VciConstants.TokenErrorCodes.InvalidTransactionCode.Should().Be("invalid_transaction_code");
    }

    [Fact]
    public void CredentialErrorCodes_ShouldHaveCorrectValues()
    {
        // Assert
        Oid4VciConstants.CredentialErrorCodes.InvalidRequest.Should().Be("invalid_request");
        Oid4VciConstants.CredentialErrorCodes.InvalidToken.Should().Be("invalid_token");
        Oid4VciConstants.CredentialErrorCodes.InsufficientScope.Should().Be("insufficient_scope");
        Oid4VciConstants.CredentialErrorCodes.UnsupportedCredentialFormat.Should().Be("unsupported_credential_format");
        Oid4VciConstants.CredentialErrorCodes.UnsupportedCredentialType.Should().Be("unsupported_credential_type");
        Oid4VciConstants.CredentialErrorCodes.InvalidProof.Should().Be("invalid_proof");
        Oid4VciConstants.CredentialErrorCodes.InvalidOrMissingProof.Should().Be("invalid_or_missing_proof");
    }

    [Fact]
    public void InputModes_ShouldHaveCorrectValues()
    {
        // Assert
        Oid4VciConstants.InputModes.Numeric.Should().Be("numeric");
        Oid4VciConstants.InputModes.Text.Should().Be("text");
    }

    [Fact]
    public void TokenTypes_ShouldHaveCorrectValues()
    {
        // Assert
        Oid4VciConstants.TokenTypes.Bearer.Should().Be("Bearer");
        Oid4VciConstants.TokenTypes.DPoP.Should().Be("DPoP");
    }
}
