using SdJwt.Net.Oid4Vci.Models;
using SdJwt.Net.Oid4Vci.Client;
using SdJwt.Net.Oid4Vci.Issuer;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace SdJwt.Net.Oid4Vci.Tests;

public class CredentialOfferTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
#if NET6_0_OR_GREATER
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
#endif
    };

    [Fact]
    public void CredentialOffer_Serialization_ProducesCorrectJson()
    {
        // Arrange
        var offer = new CredentialOffer
        {
            CredentialIssuer = "https://issuer.example.com",
            CredentialConfigurationIds = new[] { "UniversityDegree_SDJWT", "EmployeeID_SDJWT" }
        };

        offer.AddPreAuthorizedCodeGrant("code-123", new TransactionCode { Length = 4, InputMode = "numeric" });

        // Act
        var json = JsonSerializer.Serialize(offer, JsonOptions);

        // Assert
        Assert.Contains("\"credential_issuer\":\"https://issuer.example.com\"", json);
        Assert.Contains("\"credential_configuration_ids\":[\"UniversityDegree_SDJWT\",\"EmployeeID_SDJWT\"]", json);
        Assert.Contains("\"urn:ietf:params:oauth:grant-type:pre-authorized_code\"", json);
        Assert.Contains("\"pre-authorized_code\":\"code-123\"", json);
        Assert.Contains("\"length\":4", json);
        Assert.Contains("\"input_mode\":\"numeric\"", json);
    }

    [Fact]
    public void CredentialOfferBuilder_FluentAPI_ProducesCorrectOffer()
    {
        // Act
        var json = CredentialOfferBuilder.Create("https://my-issuer.com")
            .AddConfigurationId("MyIdentityCard")
            .AddConfigurationId("MyDriversLicense")
            .UsePreAuthorizedCode("code-123", pinLength: 4)
            .BuildJson();

        // Assert
        var offer = JsonSerializer.Deserialize<CredentialOffer>(json, JsonOptions);
        Assert.NotNull(offer);
        Assert.Equal("https://my-issuer.com", offer.CredentialIssuer);
        Assert.Equal(2, offer.CredentialConfigurationIds.Length);
        Assert.Contains("MyIdentityCard", offer.CredentialConfigurationIds);
        Assert.Contains("MyDriversLicense", offer.CredentialConfigurationIds);

        var grant = offer.GetPreAuthorizedCodeGrant();
        Assert.NotNull(grant);
        Assert.Equal("code-123", grant.PreAuthorizedCode);
        Assert.NotNull(grant.TransactionCode);
        Assert.Equal(4, grant.TransactionCode.Length);
        Assert.Equal("numeric", grant.TransactionCode.InputMode);
    }

    [Fact]
    public void CredentialOfferBuilder_AuthorizationCodeGrant_ProducesCorrectOffer()
    {
        // Act
        var json = CredentialOfferBuilder.Create("https://my-issuer.com")
            .AddConfigurationId("MyIdentityCard")
            .UseAuthorizationCode("state-123", "https://auth-server.example.com")
            .BuildJson();

        // Assert
        var offer = JsonSerializer.Deserialize<CredentialOffer>(json, JsonOptions);
        Assert.NotNull(offer);

        var grant = offer.GetAuthorizationCodeGrant();
        Assert.NotNull(grant);
        Assert.Equal("state-123", grant.IssuerState);
        Assert.Equal("https://auth-server.example.com", grant.AuthorizationServer);
    }

    [Fact]
    public void CredentialOfferParser_ParseValidUri_Success()
    {
        // Arrange
        var originalOffer = new CredentialOffer
        {
            CredentialIssuer = "https://issuer.example.com",
            CredentialConfigurationIds = new[] { "UniversityDegree" }
        };
        originalOffer.AddPreAuthorizedCodeGrant("auth-code-123");

        var uri = CredentialOfferParser.CreateUri(originalOffer);

        // Act
        var parsedOffer = CredentialOfferParser.Parse(uri);

        // Assert
        Assert.Equal(originalOffer.CredentialIssuer, parsedOffer.CredentialIssuer);
        Assert.Equal(originalOffer.CredentialConfigurationIds, parsedOffer.CredentialConfigurationIds);

        var grant = parsedOffer.GetPreAuthorizedCodeGrant();
        Assert.NotNull(grant);
        Assert.Equal("auth-code-123", grant.PreAuthorizedCode);
    }

    [Fact]
    public void CredentialOfferParser_ParseInvalidScheme_ThrowsException()
    {
        // Arrange
        var invalidUri = "https://example.com?credential_offer={...}";

        // Act & Assert
        Assert.Throws<CredentialOfferParseException>(() => CredentialOfferParser.Parse(invalidUri));
    }

    [Fact]
    public void CredentialOfferParser_ParseCredentialOfferUri_ThrowsParseException()
    {
        // Arrange
        var uri = "openid-credential-offer://?credential_offer_uri=https://example.com/offer.json";

        // Act & Assert
        Assert.Throws<CredentialOfferParseException>(() => CredentialOfferParser.Parse(uri));
    }

    [Fact]
    public async Task CredentialOfferParser_ParseAsync_CredentialOfferUri_Success()
    {
        // Arrange
        var offerJson = """
            {
              "credential_issuer": "https://issuer.example.com",
              "credential_configuration_ids": ["UniversityDegree"]
            }
            """;
        var uri = "openid-credential-offer://?credential_offer_uri=https://example.com/offer.json";
        using var httpClient = new HttpClient(new StaticHttpHandler((request, _) =>
        {
            if (request.RequestUri!.AbsoluteUri == "https://example.com/offer.json")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(offerJson, System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("not found")
            };
        }));

        // Act
        var offer = await CredentialOfferParser.ParseAsync(uri, httpClient);

        // Assert
        Assert.Equal("https://issuer.example.com", offer.CredentialIssuer);
        Assert.Single(offer.CredentialConfigurationIds);
        Assert.Equal("UniversityDegree", offer.CredentialConfigurationIds[0]);
    }

    [Fact]
    public async Task CredentialOfferParser_ParseAsync_CredentialOfferUri_InvalidContentType_Throws()
    {
        // Arrange
        var uri = "openid-credential-offer://?credential_offer_uri=https://example.com/offer.json";
        using var httpClient = new HttpClient(new StaticHttpHandler((request, _) =>
        {
            if (request.RequestUri!.AbsoluteUri == "https://example.com/offer.json")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"credential_issuer\":\"https://issuer.example.com\",\"credential_configuration_ids\":[\"a\"]}", System.Text.Encoding.UTF8, "text/plain")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }));

        // Act & Assert
        await Assert.ThrowsAsync<CredentialOfferParseException>(() => CredentialOfferParser.ParseAsync(uri, httpClient));
    }

    [Fact]
    public void CredentialRequest_Create_ProducesCorrectRequest()
    {
        // Act
        var request = CredentialRequest.Create("UniversityDegree", "proof-jwt-token");

        // Assert
        Assert.Equal(Oid4VciConstants.SdJwtVcFormat, request.Format);
        Assert.Equal("UniversityDegree", request.Vct);
        Assert.NotNull(request.Proof);
        Assert.Equal("jwt", request.Proof.ProofType);
        Assert.Equal("proof-jwt-token", request.Proof.Jwt);
    }

    [Fact]
    public void CredentialRequest_CreateByIdentifier_ProducesCorrectRequest()
    {
        // Act
        var request = CredentialRequest.CreateByIdentifier("credential-config-123", "proof-jwt-token");

        // Assert
        Assert.Equal(Oid4VciConstants.SdJwtVcFormat, request.Format);
        Assert.Equal("credential-config-123", request.CredentialIdentifier);
        Assert.Null(request.Vct);
        Assert.NotNull(request.Proof);
        Assert.Equal("jwt", request.Proof.ProofType);
        Assert.Equal("proof-jwt-token", request.Proof.Jwt);
    }

    [Fact]
    public void CredentialResponse_Success_ProducesCorrectResponse()
    {
        // Act
        var response = CredentialResponse.Success("sd-jwt-credential", "new-nonce", 3600, "notification-123");

        // Assert
        Assert.Equal("sd-jwt-credential", response.Credential);
        Assert.Equal("new-nonce", response.CNonce);
        Assert.Equal(3600, response.CNonceExpiresIn);
        Assert.Equal("notification-123", response.NotificationId);
        Assert.Null(response.AcceptanceToken);
    }

    [Fact]
    public void CredentialResponse_Deferred_ProducesCorrectResponse()
    {
        // Act
        var response = CredentialResponse.Deferred("acceptance-token-123", "new-nonce", 3600);

        // Assert
        Assert.Equal("acceptance-token-123", response.AcceptanceToken);
        Assert.Equal("new-nonce", response.CNonce);
        Assert.Equal(3600, response.CNonceExpiresIn);
        Assert.Null(response.Credential);
        Assert.Null(response.NotificationId);
    }

    [Fact]
    public void CredentialErrorResponse_Create_ProducesCorrectError()
    {
        // Act
        var error = CredentialErrorResponse.Create("invalid_proof", "The provided proof is invalid", "https://example.com/error", "retry-nonce", 1800);

        // Assert
        Assert.Equal("invalid_proof", error.Error);
        Assert.Equal("The provided proof is invalid", error.ErrorDescription);
        Assert.Equal("https://example.com/error", error.ErrorUri);
        Assert.Equal("retry-nonce", error.CNonce);
        Assert.Equal(1800, error.CNonceExpiresIn);
    }

    [Fact]
    public void TransactionCode_Serialization_ProducesCorrectJson()
    {
        // Arrange
        var txCode = new TransactionCode
        {
            Length = 6,
            InputMode = "text",
            Description = "Enter your PIN code"
        };

        // Act
        var json = JsonSerializer.Serialize(txCode, JsonOptions);

        // Assert
        Assert.Contains("\"length\":6", json);
        Assert.Contains("\"input_mode\":\"text\"", json);
        Assert.Contains("\"description\":\"Enter your PIN code\"", json);
    }

    [Fact]
    public void TokenRequest_Serialization_ProducesCorrectJson()
    {
        // Arrange
        var tokenRequest = new TokenRequest
        {
            GrantType = Oid4VciConstants.GrantTypes.PreAuthorizedCode,
            PreAuthorizedCode = "pre-auth-123",
            TransactionCode = "123456",
            ClientId = "wallet-client"
        };

        // Act
        var json = JsonSerializer.Serialize(tokenRequest, JsonOptions);

        // Assert
        Assert.Contains($"\"grant_type\":\"{Oid4VciConstants.GrantTypes.PreAuthorizedCode}\"", json);
        Assert.Contains("\"pre-authorized_code\":\"pre-auth-123\"", json);
        Assert.Contains("\"tx_code\":\"123456\"", json);
        Assert.Contains("\"client_id\":\"wallet-client\"", json);
    }

    [Fact]
    public void TokenResponse_Serialization_ProducesCorrectJson()
    {
        // Arrange
        var tokenResponse = new TokenResponse
        {
            AccessToken = "access-token-123",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            CNonce = "nonce-456",
            CNonceExpiresIn = 300
        };

        // Act
        var json = JsonSerializer.Serialize(tokenResponse, JsonOptions);

        // Assert
        Assert.Contains("\"access_token\":\"access-token-123\"", json);
        Assert.Contains("\"token_type\":\"Bearer\"", json);
        Assert.Contains("\"expires_in\":3600", json);
        Assert.Contains("\"c_nonce\":\"nonce-456\"", json);
        Assert.Contains("\"c_nonce_expires_in\":300", json);
    }

    private sealed class StaticHttpHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> callback) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _callback = callback;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_callback(request, cancellationToken));
        }
    }
}
