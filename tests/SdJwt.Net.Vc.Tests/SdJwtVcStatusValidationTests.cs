using System.Net;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.StatusList.Issuer;
using SdJwt.Net.StatusList.Verifier;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Vc.Verifier;
using Xunit;

namespace SdJwt.Net.Vc.Tests;

public class SdJwtVcStatusValidationTests : TestBase
{
    [Fact]
    public async Task VerifyAsync_WithStatusListValidatorAndValidStatus_Succeeds()
    {
        // Arrange
        const string statusListUri = "https://status.example.com/lists/1";
        var statusManager = new StatusListManager(IssuerSigningKey, IssuerSigningAlgorithm);
        var statusListToken = await statusManager.CreateStatusListTokenAsync(
            statusListUri,
            new byte[] { 0 }, // valid
            bits: 1);

        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var payload = new SdJwtVcPayload
        {
            Issuer = TrustedIssuer,
            Subject = "did:example:holder",
            Status = vcIssuer.CreateStatusReference(statusListUri, 0),
            AdditionalData = new Dictionary<string, object>
            {
                ["given_name"] = "Alice"
            }
        };
        var issuance = vcIssuer.Issue("https://types.example.com/pid", payload, new SdIssuanceOptions());
        var holder = new SdJwtHolder(issuance.Issuance);
        var presentation = holder.CreatePresentation(_ => true);

        using var statusHttp = new HttpClient(new StubHttpHandler(new Dictionary<string, HttpResponseMessage>
        {
            [statusListUri] = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(statusListToken, Encoding.UTF8, "application/statuslist+jwt")
            }
        }));
        using var statusValidator = new StatusListSdJwtVcStatusValidator(
            _ => Task.FromResult<SecurityKey>(IssuerSigningKey),
            new StatusListOptions { CacheStatusLists = false, ValidateStatusListTiming = false },
            statusHttp);

        var verifier = new SdJwtVcVerifier(_ => Task.FromResult(IssuerSigningKey));
        var verificationPolicy = new SdJwtVcVerificationPolicy
        {
            RequireStatusCheck = true,
            StatusValidator = statusValidator
        };
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        // Act
        var result = await verifier.VerifyAsync(
            presentation,
            validationParameters,
            verificationPolicy: verificationPolicy);

        // Assert
        Assert.Equal("https://types.example.com/pid", result.VerifiableCredentialType);
    }

    [Fact]
    public async Task VerifyAsync_WithStatusListValidatorAndInvalidStatus_ReturnsFalse()
    {
        // Arrange
        const string statusListUri = "https://status.example.com/lists/2";
        var statusManager = new StatusListManager(IssuerSigningKey, IssuerSigningAlgorithm);
        var statusListToken = await statusManager.CreateStatusListTokenAsync(
            statusListUri,
            new byte[] { 255 }, // ensure first status entries are invalid/revoked
            bits: 1);

        var vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var statusClaim = vcIssuer.CreateStatusReference(statusListUri, 0);

        using var statusHttp = new HttpClient(new StubHttpHandler(new Dictionary<string, HttpResponseMessage>
        {
            [statusListUri] = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(statusListToken, Encoding.UTF8, "application/statuslist+jwt")
            }
        }));
        using var statusValidator = new StatusListSdJwtVcStatusValidator(
            _ => Task.FromResult<SecurityKey>(IssuerSigningKey),
            new StatusListOptions { CacheStatusLists = false, ValidateStatusListTiming = false },
            statusHttp);

        // Act
        var isValid = await statusValidator.ValidateAsync(statusClaim);

        // Assert
        Assert.False(isValid);
    }

    private sealed class StubHttpHandler(Dictionary<string, HttpResponseMessage> responses) : HttpMessageHandler
    {
        private readonly Dictionary<string, HttpResponseMessage> _responses = responses;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var key = request.RequestUri!.AbsoluteUri;
            if (_responses.TryGetValue(key, out var response))
            {
                return Task.FromResult(CloneResponse(response));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("not found", Encoding.UTF8, "text/plain")
            });
        }

        private static HttpResponseMessage CloneResponse(HttpResponseMessage original)
        {
            var clone = new HttpResponseMessage(original.StatusCode);
            if (original.Content != null)
            {
                var payload = original.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var mediaType = original.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                clone.Content = new StringContent(payload, Encoding.UTF8, mediaType);
            }

            return clone;
        }
    }
}
