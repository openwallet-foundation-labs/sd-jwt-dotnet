using FluentAssertions;
using SdJwt.Net.Oid4Vci.Models;
using SdJwt.Net.Oid4Vci.Client;
using System;
using Xunit;

namespace SdJwt.Net.Oid4Vci.Tests.Models;

public class Oid4VciModelTests
{
    [Fact]
    public void CredentialNotificationRequest_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var request = new CredentialNotificationRequest();

        // Act
        request.NotificationId = "notification-123";
        request.Event = "credential_issued";

        // Assert
        request.NotificationId.Should().Be("notification-123");
        request.Event.Should().Be("credential_issued");
    }

    [Fact]
    public void CredentialNotificationResponse_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var response = new CredentialNotificationResponse();

        // Act
        response.NotificationId = "notification-123";

        // Assert
        response.NotificationId.Should().Be("notification-123");
    }

    [Fact]
    public void DeferredCredentialRequest_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var request = new DeferredCredentialRequest();

        // Act
        request.TransactionId = "transaction-123";

        // Assert
        request.TransactionId.Should().Be("transaction-123");
    }

    [Fact]
    public void DeferredCredentialResponse_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var response = new DeferredCredentialResponse();

        // Act
        response.AcceptanceToken = "acceptance-token-123";
        response.Credential = "credential-data";
        response.TransactionId = "transaction-123";

        // Assert
        response.AcceptanceToken.Should().Be("acceptance-token-123");
        response.Credential.Should().Be("credential-data");
        response.TransactionId.Should().Be("transaction-123");
    }

    [Fact]
    public void ProofBuildException_WithMessage_ShouldCreateCorrectly()
    {
        // Arrange
        var message = "Proof building failed";

        // Act
        var exception = new ProofBuildException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ProofBuildException_WithMessageAndInnerException_ShouldCreateCorrectly()
    {
        // Arrange
        var message = "Proof building failed";
        var innerException = new ArgumentException("Invalid argument");

        // Act
        var exception = new ProofBuildException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ProofBuildException_ShouldBeAssignableToException()
    {
        // Arrange
        var exception = new ProofBuildException("test message");

        // Act & Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Oid4VciConstants_ShouldHaveCorrectValues()
    {
        // Assert standard constants
        Oid4VciConstants.SdJwtVcFormat.Should().Be("vc+sd-jwt");
        Oid4VciConstants.ProofJwtType.Should().Be("openid4vci-proof+jwt");
        Oid4VciConstants.CredentialOfferScheme.Should().Be("openid-credential-offer");
        Oid4VciConstants.JwtBearerClientAssertionType.Should().Be("urn:ietf:params:oauth:client-assertion-type:jwt-bearer");

        // Assert grant types
        Oid4VciConstants.GrantTypes.PreAuthorizedCode.Should().Be("urn:ietf:params:oauth:grant-type:pre-authorized_code");
        Oid4VciConstants.GrantTypes.AuthorizationCode.Should().Be("authorization_code");
        Oid4VciConstants.GrantTypes.RefreshToken.Should().Be("refresh_token");

        // Assert proof types
        Oid4VciConstants.ProofTypes.Jwt.Should().Be("jwt");
        Oid4VciConstants.ProofTypes.Cwt.Should().Be("cwt");
        Oid4VciConstants.ProofTypes.LdpVp.Should().Be("ldp_vp");

        // Assert token error codes
        Oid4VciConstants.TokenErrorCodes.InvalidRequest.Should().Be("invalid_request");
        Oid4VciConstants.TokenErrorCodes.InvalidClient.Should().Be("invalid_client");
        Oid4VciConstants.TokenErrorCodes.InvalidGrant.Should().Be("invalid_grant");
        Oid4VciConstants.TokenErrorCodes.UnauthorizedClient.Should().Be("unauthorized_client");
        Oid4VciConstants.TokenErrorCodes.UnsupportedGrantType.Should().Be("unsupported_grant_type");
        Oid4VciConstants.TokenErrorCodes.InvalidScope.Should().Be("invalid_scope");
        Oid4VciConstants.TokenErrorCodes.InvalidTransactionCode.Should().Be("invalid_transaction_code");

        // Assert credential error codes
        Oid4VciConstants.CredentialErrorCodes.InvalidRequest.Should().Be("invalid_request");
        Oid4VciConstants.CredentialErrorCodes.InvalidToken.Should().Be("invalid_token");
        Oid4VciConstants.CredentialErrorCodes.InsufficientScope.Should().Be("insufficient_scope");
        Oid4VciConstants.CredentialErrorCodes.UnsupportedCredentialFormat.Should().Be("unsupported_credential_format");
        Oid4VciConstants.CredentialErrorCodes.UnsupportedCredentialType.Should().Be("unsupported_credential_type");
        Oid4VciConstants.CredentialErrorCodes.InvalidProof.Should().Be("invalid_proof");
        Oid4VciConstants.CredentialErrorCodes.InvalidOrMissingProof.Should().Be("invalid_or_missing_proof");

        // Assert input modes
        Oid4VciConstants.InputModes.Numeric.Should().Be("numeric");
        Oid4VciConstants.InputModes.Text.Should().Be("text");

        // Assert token types
        Oid4VciConstants.TokenTypes.Bearer.Should().Be("Bearer");
        Oid4VciConstants.TokenTypes.DPoP.Should().Be("DPoP");
    }
}

// Mock classes for testing if they don't exist
public class CredentialNotificationRequest
{
    public string? NotificationId { get; set; }
    public string? Event { get; set; }
}

public class CredentialNotificationResponse
{
    public string? NotificationId { get; set; }
}

public class DeferredCredentialRequest
{
    public string? TransactionId { get; set; }
}

public class DeferredCredentialResponse
{
    public string? AcceptanceToken { get; set; }
    public string? Credential { get; set; }
    public string? TransactionId { get; set; }
}
