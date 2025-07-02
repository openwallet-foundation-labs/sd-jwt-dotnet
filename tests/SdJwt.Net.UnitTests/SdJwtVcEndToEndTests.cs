using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using RichardSzalay.MockHttp;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Verifier;
using Xunit;

namespace SdJwt.Net.Tests;

public class SdJwtVcEndToEndTests : TestBase
{
    private readonly SdJwtVcIssuer _vcIssuer;
    private const string StatusListUrl = "https://issuer.example.com/status/1";

    public SdJwtVcEndToEndTests()
    {
        _vcIssuer = new SdJwtVcIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
    }

    private (SdJwtVcVerifier, MockHttpMessageHandler) CreateVerifierWithMockedStatusList(BitArray statusBits, bool enableCache = false)
    {
        var statusListManager = new StatusListManager(IssuerSigningKey, IssuerSigningAlgorithm);
        var statusListJwt = statusListManager.CreateStatusListCredential(TrustedIssuer, statusBits);

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(StatusListUrl).Respond("application/jwt", statusListJwt);

        var verifier = new SdJwtVcVerifier(
            TrustedIssuer,
            _ => Task.FromResult(IssuerSigningKey),
            new StatusListOptions
            {
                HttpClient = mockHttp.ToHttpClient(),
                CacheDuration = enableCache ? TimeSpan.FromMinutes(5) : TimeSpan.Zero
            }
        );
        return (verifier, mockHttp);
    }

    [Fact]
    public async Task VerifyAsync_WithValidAndNonRevokedCredential_Succeeds()
    {
        // Arrange
        var (verifier, mockHttp) = CreateVerifierWithMockedStatusList(new BitArray(100, false));
        var issuance = CreateTestIssuance(42);
        var holder = new SdJwtHolder(issuance.Issuance);
        var presentation = holder.CreatePresentation(d => true, new JwtPayload { { "aud", "v" } }, HolderPrivateKey, HolderSigningAlgorithm);
        var kbParams = new TokenValidationParameters { ValidAudience = "v", IssuerSigningKey = HolderPublicKey };

        // Act
        var result = await verifier.VerifyAsync(presentation, kbParams);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.KeyBindingVerified);
        Assert.True(result.VerifiableCredential.CredentialSubject.ContainsKey("given_name"));
        Assert.Equal(1, mockHttp.GetMatchCount(mockHttp.Fallback.Requests.Single()));
    }

    [Fact]
    public async Task VerifyAsync_WithStatusListCache_OnlyFetchesOnce()
    {
        // Arrange
        var (verifier, mockHttp) = CreateVerifierWithMockedStatusList(new BitArray(100, false), enableCache: true);
        var kbParams = new TokenValidationParameters { ValidAudience = "v", IssuerSigningKey = HolderPublicKey };

        // Act & Assert
        // First call - should fetch from network
        var issuance1 = CreateTestIssuance(10);
        var presentation1 = new SdJwtHolder(issuance1.Issuance).CreatePresentation(d => true, new JwtPayload { { "aud", "v" } }, HolderPrivateKey, HolderSigningAlgorithm);
        await verifier.VerifyAsync(presentation1, kbParams);
        Assert.Equal(1, mockHttp.GetMatchCount(mockHttp.Fallback.Requests.Single()));

        // Second call - should use cache
        var issuance2 = CreateTestIssuance(20);
        var presentation2 = new SdJwtHolder(issuance2.Issuance).CreatePresentation(d => true, new JwtPayload { { "aud", "v" } }, HolderPrivateKey, HolderSigningAlgorithm);
        await verifier.VerifyAsync(presentation2, kbParams);
        Assert.Equal(1, mockHttp.GetMatchCount(mockHttp.Fallback.Requests.Single())); // Still 1!
    }

    [Fact]
    public async Task VerifyAsync_WithRevokedCredential_ThrowsSecurityTokenException()
    {
        // Arrange
        var statusBits = new BitArray(100, false);
        statusBits[42] = true; // Revoke index 42
        var (verifier, _) = CreateVerifierWithMockedStatusList(statusBits);
        var issuance = CreateTestIssuance(42); // Points to the revoked index
        var presentation = new SdJwtHolder(issuance.Issuance).CreatePresentation(d => true, new JwtPayload(), HolderPrivateKey, HolderSigningAlgorithm);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<SecurityTokenException>(() => verifier.VerifyAsync(presentation));
        Assert.Contains("status is marked as invalid/revoked", ex.Message);
    }

    [Fact]
    public async Task VerifyAsync_WithUntrustedIssuerInVc_ThrowsInvalidIssuerException()
    {
        // Arrange
        var (verifier, _) = CreateVerifierWithMockedStatusList(new BitArray(10, false));
        var vcPayload = new VerifiableCredentialPayload { Issuer = "https://untrusted.com" }; // Different issuer
        var issuance = _vcIssuer.Issue(vcPayload, "TestCredential", new SdIssuanceOptions());
        var presentation = new SdJwtHolder(issuance.Issuance).CreatePresentation(d => true);

        // Act & Assert
        await Assert.ThrowsAsync<SecurityTokenInvalidIssuerException>(() => verifier.VerifyAsync(presentation));
    }

    private IssuerOutput CreateTestIssuance(int statusIndex)
    {
        var vcPayload = new VerifiableCredentialPayload
        {
            Issuer = TrustedIssuer,
            Status = new StatusClaim(StatusListUrl, statusIndex),
            CredentialSubject = new() { { "given_name", "Jane" } }
        };
        var options = new SdIssuanceOptions { DisclosureStructure = new { vc = new { credential_subject = new { given_name = true } } } };
        return _vcIssuer.Issue(vcPayload, "TestCredential", options, HolderPublicJwk);
    }
}