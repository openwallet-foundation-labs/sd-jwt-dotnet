using FluentAssertions;
using Microsoft.Extensions.Options;
using SdJwt.Net.Oid4Vci.AspNetCore.Options;
using SdJwt.Net.Oid4Vci.AspNetCore.Services;
using Xunit;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Tests;

public class InMemoryAccessTokenServiceTests
{
    private static InMemoryAccessTokenService Create(int tokenLifetime = 300, int nonceLifetime = 300)
    {
        var svc = new InMemoryAccessTokenService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<InMemoryAccessTokenService>.Instance,
            tokenLifetime,
            nonceLifetime);
        return svc;
    }

    [Fact]
    public async Task IssueForPreAuthorizedCodeAsync_WithValidCode_ShouldReturnToken()
    {
        var svc = Create();
        svc.RegisterPreAuthorizedCode("code-abc", null, new[] { "IdentityCredential" });

        var result = await svc.IssueForPreAuthorizedCodeAsync("code-abc", null);

        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.CNonce.Should().NotBeNullOrWhiteSpace();
        result.ExpiresInSeconds.Should().Be(300);
        result.AuthorizedConfigurationIds.Should().Contain("IdentityCredential");
    }

    [Fact]
    public async Task IssueForPreAuthorizedCodeAsync_WithInvalidCode_ShouldReturnNull()
    {
        var svc = Create();

        var result = await svc.IssueForPreAuthorizedCodeAsync("nonexistent-code", null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task IssueForPreAuthorizedCodeAsync_WithAlreadyUsedCode_ShouldReturnNull()
    {
        var svc = Create();
        svc.RegisterPreAuthorizedCode("one-time-code", null, new[] { "Credential" });

        var first = await svc.IssueForPreAuthorizedCodeAsync("one-time-code", null);
        var second = await svc.IssueForPreAuthorizedCodeAsync("one-time-code", null);

        first.Should().NotBeNull();
        second.Should().BeNull();
    }

    [Fact]
    public async Task IssueForPreAuthorizedCodeAsync_WithCorrectTxCode_ShouldSucceed()
    {
        var svc = Create();
        svc.RegisterPreAuthorizedCode("pin-code", "1234", new[] { "Credential" });

        var result = await svc.IssueForPreAuthorizedCodeAsync("pin-code", "1234");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task IssueForPreAuthorizedCodeAsync_WithWrongTxCode_ShouldReturnNull()
    {
        var svc = Create();
        svc.RegisterPreAuthorizedCode("pin-code", "1234", new[] { "Credential" });

        var result = await svc.IssueForPreAuthorizedCodeAsync("pin-code", "9999");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_WithValidToken_ShouldReturnTokenInfo()
    {
        var svc = Create();
        svc.RegisterPreAuthorizedCode("code-v1", null, new[] { "Credential" });
        var issued = await svc.IssueForPreAuthorizedCodeAsync("code-v1", null);

        var validated = await svc.ValidateAsync(issued!.Token);

        validated.Should().NotBeNull();
        validated!.Token.Should().Be(issued.Token);
        validated.AuthorizedConfigurationIds.Should().Contain("Credential");
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidToken_ShouldReturnNull()
    {
        var svc = Create();
        var result = await svc.ValidateAsync("invalid-token");
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_WithExpiredToken_ShouldReturnNull()
    {
        var svc = Create(tokenLifetime: -1);
        svc.RegisterPreAuthorizedCode("exp-code", null, new[] { "Credential" });
        var issued = await svc.IssueForPreAuthorizedCodeAsync("exp-code", null);

        var result = await svc.ValidateAsync(issued!.Token);

        result.Should().BeNull();
    }

    [Fact]
    public void RegisterPreAuthorizedCode_WithEmptyCode_ShouldThrow()
    {
        var svc = Create();
        var act = () => svc.RegisterPreAuthorizedCode(string.Empty, null, new[] { "Credential" });
        act.Should().Throw<ArgumentException>();
    }
}

public class InMemoryDeferredCredentialStoreTests
{
    [Fact]
    public async Task SaveAndRetrieve_ShouldReturnStoredRequest()
    {
        var store = new InMemoryDeferredCredentialStore(Microsoft.Extensions.Logging.Abstractions.NullLogger<InMemoryDeferredCredentialStore>.Instance);
        var request = new SdJwt.Net.Oid4Vci.Models.CredentialRequest { CredentialConfigurationId = "UniversityDegree" };

        await store.SaveAsync("txn-001", request, "access-token-xyz");
        var retrieved = await store.RetrieveAsync("txn-001");

        retrieved.Should().NotBeNull();
        retrieved!.Value.Request.CredentialConfigurationId.Should().Be("UniversityDegree");
        retrieved.Value.AccessToken.Should().Be("access-token-xyz");
    }

    [Fact]
    public async Task RetrieveAsync_WithInvalidId_ShouldReturnNull()
    {
        var store = new InMemoryDeferredCredentialStore(Microsoft.Extensions.Logging.Abstractions.NullLogger<InMemoryDeferredCredentialStore>.Instance);
        var result = await store.RetrieveAsync("nonexistent");
        result.Should().BeNull();
    }

    [Fact]
    public async Task RetrieveAsync_IsConsumeOnRead_ShouldBeNullOnSecondCall()
    {
        var store = new InMemoryDeferredCredentialStore(Microsoft.Extensions.Logging.Abstractions.NullLogger<InMemoryDeferredCredentialStore>.Instance);
        var request = new SdJwt.Net.Oid4Vci.Models.CredentialRequest { CredentialConfigurationId = "UniversityDegree" };

        await store.SaveAsync("txn-replay", request, "token");
        var first = await store.RetrieveAsync("txn-replay");
        var second = await store.RetrieveAsync("txn-replay");

        first.Should().NotBeNull();
        second.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_WithEmptyTransactionId_ShouldThrow()
    {
        var store = new InMemoryDeferredCredentialStore(Microsoft.Extensions.Logging.Abstractions.NullLogger<InMemoryDeferredCredentialStore>.Instance);
        var act = async () => await store.SaveAsync(string.Empty,
            new SdJwt.Net.Oid4Vci.Models.CredentialRequest(), "token");
        await act.Should().ThrowAsync<ArgumentException>();
    }
}

public class CredentialIssuerOptionsTests
{
    [Fact]
    public void BuildMetadata_WithValidOptions_ShouldReturnCorrectMetadata()
    {
        var options = new CredentialIssuerOptions
        {
            IssuerUrl = "https://issuer.example.com",
            CredentialEndpointPath = "/credential",
            DeferredCredentialEndpointPath = "/deferred-credential"
        };

        var metadata = options.BuildMetadata();

        metadata.CredentialIssuer.Should().Be("https://issuer.example.com");
        metadata.CredentialEndpoint.Should().Be("https://issuer.example.com/credential");
        metadata.DeferredCredentialEndpoint.Should().Be("https://issuer.example.com/deferred-credential");
    }

    [Fact]
    public void BuildMetadata_WithTrailingSlashOnIssuerUrl_ShouldNormalize()
    {
        var options = new CredentialIssuerOptions
        {
            IssuerUrl = "https://issuer.example.com/",
            CredentialEndpointPath = "/credential"
        };

        var metadata = options.BuildMetadata();

        metadata.CredentialEndpoint.Should().NotContain("//credential");
        metadata.CredentialEndpoint.Should().EndWith("/credential");
    }

    [Fact]
    public void BuildMetadata_WithEmptyIssuerUrl_ShouldThrow()
    {
        var options = new CredentialIssuerOptions { IssuerUrl = string.Empty };
        var act = () => options.BuildMetadata();
        act.Should().Throw<InvalidOperationException>().WithMessage("*IssuerUrl*");
    }

    [Fact]
    public void BuildMetadata_WithAuthorizationServers_ShouldIncludeThem()
    {
        var options = new CredentialIssuerOptions
        {
            IssuerUrl = "https://issuer.example.com",
            AuthorizationServers = new[] { "https://auth.example.com" }
        };

        var metadata = options.BuildMetadata();

        metadata.AuthorizationServers.Should().NotBeNull();
        metadata.AuthorizationServers!.Should().Contain("https://auth.example.com");
    }

    [Fact]
    public void BuildMetadata_WithNoAuthorizationServers_ShouldHaveNullServers()
    {
        var options = new CredentialIssuerOptions
        {
            IssuerUrl = "https://issuer.example.com"
        };

        var metadata = options.BuildMetadata();

        metadata.AuthorizationServers.Should().BeNull();
    }
}
