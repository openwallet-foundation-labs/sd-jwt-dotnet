using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Eudiw;
using SdJwt.Net.Wallet.Attestation;
using SdJwt.Net.Wallet.Audit;
using SdJwt.Net.Eudiw.Arf;
using SdJwt.Net.Wallet.Core;
using SdJwt.Net.Wallet.Protocols;
using SdJwt.Net.Wallet.Storage;
using Xunit;

namespace SdJwt.Net.Eudiw.Tests;

/// <summary>
/// Tests for the EudiWallet class.
/// </summary>
public class EudiWalletTests
{
    private readonly InMemoryCredentialStore _store;
    private readonly MockKeyManager _keyManager;
    private readonly EudiWallet _wallet;

    public EudiWalletTests()
    {
        _store = new InMemoryCredentialStore();
        _keyManager = new MockKeyManager();
        _wallet = new EudiWallet(_store, _keyManager);
    }

    #region Construction Tests

    [Fact]
    public void Constructor_WithDefaultOptions_SetsDefaultValues()
    {
        // Arrange & Act
        var wallet = new EudiWallet(_store, _keyManager);

        // Assert
        wallet.IsArfEnforced.Should().BeTrue();
        wallet.MinimumHaipLevel.Should().Be(2);
        wallet.DisplayName.Should().Be("EUDI Wallet");
    }

    [Fact]
    public void Constructor_WithCustomOptions_UsesProvidedValues()
    {
        // Arrange
        var options = new EudiWalletOptions
        {
            WalletId = "test-wallet",
            DisplayName = "Test EUDI Wallet",
            EnforceArfCompliance = false,
            MinimumHaipLevel = 3
        };

        // Act
        var wallet = new EudiWallet(_store, _keyManager, eudiOptions: options);

        // Assert
        wallet.WalletId.Should().Be("test-wallet");
        wallet.DisplayName.Should().Be("Test EUDI Wallet");
        wallet.IsArfEnforced.Should().BeFalse();
        wallet.MinimumHaipLevel.Should().Be(3);
    }

    [Fact]
    public void Constructor_WithNullStore_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () => new EudiWallet(null!, _keyManager);
        act.Should().Throw<ArgumentNullException>().WithParameterName("store");
    }

    [Fact]
    public void Constructor_WithNullKeyManager_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () => new EudiWallet(_store, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("keyManager");
    }

    [Fact]
    public async Task Constructor_WithOid4VciAdapterOption_EnablesCredentialOfferProcessing()
    {
        // Arrange
        var adapter = new StubOid4VciAdapter
        {
            OfferToReturn = new CredentialOfferInfo
            {
                CredentialIssuer = "https://issuer.example.com",
                CredentialConfigurationIds = ["pid"]
            }
        };
        var options = new EudiWalletOptions
        {
            Oid4VciAdapter = adapter,
            EnforceArfCompliance = false
        };
        var wallet = new EudiWallet(_store, _keyManager, eudiOptions: options);

        // Act
        var offer = await wallet.ProcessCredentialOfferAsync("openid-credential-offer://example");

        // Assert
        offer.CredentialIssuer.Should().Be("https://issuer.example.com");
    }

    [Fact]
    public async Task Constructor_WithOid4VpAdapterOption_EnablesPresentationRequestProcessing()
    {
        // Arrange
        var adapter = new StubOid4VpAdapter
        {
            RequestToReturn = new PresentationRequestInfo
            {
                RequestId = "req-1",
                ClientId = "verifier-1",
                ResponseUri = "https://verifier.example.com/response",
                Nonce = "nonce-1"
            }
        };
        var options = new EudiWalletOptions
        {
            Oid4VpAdapter = adapter,
            EnforceArfCompliance = false
        };
        var wallet = new EudiWallet(_store, _keyManager, eudiOptions: options);

        // Act
        var request = await wallet.ProcessPresentationRequestAsync("openid4vp://example");

        // Assert
        request.RequestId.Should().Be("req-1");
    }

    [Fact]
    public async Task Constructor_WithWalletAttestationProvider_EnablesAttestationMethods()
    {
        // Arrange
        var generated = await _keyManager.GenerateKeyAsync(new KeyGenerationOptions { KeyId = "key-1", Algorithm = "ES256" });
        var provider = new StubWalletAttestationsProvider();
        var options = new EudiWalletOptions
        {
            WalletAttestationsProvider = provider,
            EnforceArfCompliance = false
        };
        var wallet = new EudiWallet(_store, _keyManager, eudiOptions: options);

        // Act
        var token = await wallet.GenerateWalletAttestationAsync(generated.KeyId);

        // Assert
        token.Should().Be("wia-token");
    }

    #endregion

    #region Algorithm Validation Tests

    [Theory]
    [InlineData("ES256", true)]
    [InlineData("ES384", true)]
    [InlineData("ES512", true)]
    [InlineData("PS256", false)]  // PS algorithms not in ARF/EUDIW spec
    [InlineData("PS384", false)]
    [InlineData("PS512", false)]
    [InlineData("RS256", false)]
    [InlineData("HS256", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ValidateAlgorithm_ReturnsExpectedResult(string? algorithm, bool expected)
    {
        // Act
        var result = _wallet.ValidateAlgorithm(algorithm!);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Credential Type Validation Tests

    [Theory]
    [InlineData(EudiwConstants.Pid.DocType, true, ArfCredentialType.Pid)]
    [InlineData(EudiwConstants.Mdl.DocType, true, ArfCredentialType.Mdl)]
    [InlineData("unknown.type", false, null)]
    [InlineData("", false, null)]
    [InlineData(null, false, null)]
    public void ValidateCredentialType_ReturnsExpectedResult(
        string? credentialType,
        bool expectedValid,
        ArfCredentialType? expectedType)
    {
        // Act
        var result = _wallet.ValidateCredentialType(credentialType!);

        // Assert
        result.IsValid.Should().Be(expectedValid);
        result.CredentialType.Should().Be(expectedType);
    }

    #endregion

    #region Member State Validation Tests

    [Theory]
    [InlineData("DE", true)]
    [InlineData("FR", true)]
    [InlineData("IT", true)]
    [InlineData("NL", true)]
    [InlineData("AT", true)]
    [InlineData("US", false)]
    [InlineData("GB", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ValidateMemberState_ReturnsExpectedResult(string? countryCode, bool expected)
    {
        // Act
        var result = _wallet.ValidateMemberState(countryCode!);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetSupportedMemberStates_ReturnsAllEuMemberStates()
    {
        // Act
        var memberStates = _wallet.GetSupportedMemberStates();

        // Assert
        memberStates.Should().Contain("DE");
        memberStates.Should().Contain("FR");
        memberStates.Should().Contain("IT");
        memberStates.Should().HaveCountGreaterOrEqualTo(27);
    }

    #endregion

    #region PID Claims Validation Tests

    [Fact]
    public void ValidatePidClaims_WithAllMandatoryClaims_ReturnsValid()
    {
        // Arrange
        var claims = new Dictionary<string, object>
        {
            ["family_name"] = "Mustermann",
            ["given_name"] = "Erika",
            ["birth_date"] = "1964-08-12",
            ["issuance_date"] = "2024-01-01",
            ["expiry_date"] = "2029-01-01",
            ["issuing_authority"] = "Bundesdruckerei",
            ["issuing_country"] = "DE"
        };

        // Act
        var result = _wallet.ValidatePidClaims(claims);

        // Assert
        result.IsValid.Should().BeTrue();
        result.CredentialType.Should().Be(ArfCredentialType.Pid);
    }

    [Fact]
    public void ValidatePidClaims_WithMissingMandatoryClaims_ReturnsInvalid()
    {
        // Arrange
        var claims = new Dictionary<string, object>
        {
            ["family_name"] = "Mustermann",
            ["given_name"] = "Erika"
            // Missing: birth_date, issuance_date, expiry_date, issuing_authority, issuing_country
        };

        // Act
        var result = _wallet.ValidatePidClaims(claims);

        // Assert
        result.IsValid.Should().BeFalse();
        result.MissingClaims.Should().Contain("birth_date");
        result.MissingClaims.Should().Contain("issuing_country");
    }

    [Fact]
    public void ValidatePidClaims_WithNullClaims_ReturnsInvalid()
    {
        // Act
        var result = _wallet.ValidatePidClaims(null!);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Trust Validation Tests

    [Fact]
    public async Task ValidateIssuerTrustAsync_WithEuMemberStateIssuer_ReturnsTrusted()
    {
        // Arrange
        var issuerUrl = "https://pid-provider.bundesdruckerei.de";

        // Act
        var result = await _wallet.ValidateIssuerTrustAsync(issuerUrl);

        // Assert
        result.IsTrusted.Should().BeTrue();
        result.MemberState.Should().Be("DE");
    }

    [Fact]
    public async Task ValidateIssuerTrustAsync_WithNonEuIssuer_ReturnsUntrusted()
    {
        // Arrange
        var issuerUrl = "https://issuer.example.com";

        // Act
        var result = await _wallet.ValidateIssuerTrustAsync(issuerUrl);

        // Assert
        result.IsTrusted.Should().BeFalse();
    }

    #endregion

    #region Trust List Cache Tests

    [Fact]
    public void IsTrustListCacheEmpty_Initially_ReturnsTrue()
    {
        // Act & Assert
        _wallet.IsTrustListCacheEmpty.Should().BeTrue();
    }

    [Fact]
    public void ClearTrustListCache_DoesNotThrow()
    {
        // Act & Assert
        var act = () => _wallet.ClearTrustListCache();
        act.Should().NotThrow();
    }

    #endregion

    #region Supported Credential Types Tests

    [Fact]
    public void SupportedCredentialTypes_IncludesPidAndMdl()
    {
        // Act
        var types = _wallet.SupportedCredentialTypes;

        // Assert
        types.Should().Contain(EudiwConstants.Pid.DocType);
        types.Should().Contain(EudiwConstants.Mdl.DocType);
    }

    #endregion

    /// <summary>
    /// Mock key manager for testing.
    /// </summary>
    private class MockKeyManager : IKeyManager
    {
        private readonly Dictionary<string, KeyInfo> _keys = new();

        public Task<KeyInfo> GenerateKeyAsync(KeyGenerationOptions options, CancellationToken cancellationToken = default)
        {
            var keyId = options.KeyId ?? Guid.NewGuid().ToString("N");
            var keyInfo = new KeyInfo
            {
                KeyId = keyId,
                Algorithm = options.Algorithm ?? "ES256",
                CreatedAt = DateTimeOffset.UtcNow
            };
            _keys[keyId] = keyInfo;
            return Task.FromResult(keyInfo);
        }

        public Task<byte[]> SignAsync(string keyId, byte[] data, string algorithm, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new byte[64]);
        }

        public Task<JsonWebKey> GetPublicKeyAsync(string keyId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new JsonWebKey
            {
                Kid = keyId,
                Kty = "EC",
                Crv = "P-256"
            });
        }

        public Task<SecurityKey> GetSecurityKeyAsync(string keyId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<SecurityKey>(new JsonWebKey
            {
                Kid = keyId,
                Kty = "EC",
                Crv = "P-256"
            });
        }

        public Task<KeyInfo?> GetKeyInfoAsync(string keyId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_keys.TryGetValue(keyId, out var info) ? info : null);
        }

        public Task<bool> DeleteKeyAsync(string keyId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_keys.Remove(keyId));
        }

        public Task<IReadOnlyList<KeyInfo>> ListKeysAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<KeyInfo>>(_keys.Values.ToList());
        }

        public Task<bool> KeyExistsAsync(string keyId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_keys.ContainsKey(keyId));
        }
    }

    private sealed class StubOid4VciAdapter : IOid4VciAdapter
    {
        public CredentialOfferInfo OfferToReturn { get; set; } = new();

        public Task<CredentialOfferInfo> ParseOfferAsync(string offer, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(OfferToReturn);
        }

        public Task<IDictionary<string, object>> ResolveIssuerMetadataAsync(string issuer, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TokenResult> ExchangeTokenAsync(string tokenEndpoint, TokenExchangeOptions options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IssuanceResult> RequestCredentialAsync(
            string credentialEndpoint,
            CredentialRequestOptions options,
            IKeyManager keyManager,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IssuanceResult> PollDeferredCredentialAsync(
            string deferredEndpoint,
            string transactionId,
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> BuildAuthorizationUrlAsync(
            string authorizationEndpoint,
            string clientId,
            string redirectUri,
            string? scope = null,
            string? authorizationDetails = null,
            string? state = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class StubOid4VpAdapter : IOid4VpAdapter
    {
        public PresentationRequestInfo RequestToReturn { get; set; } = new();

        public Task<PresentationRequestInfo> ParseRequestAsync(string request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RequestToReturn);
        }

        public Task<PresentationRequestInfo> ResolveRequestUriAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<CredentialMatch>> FindMatchingCredentialsAsync(
            PresentationRequestInfo request,
            IReadOnlyList<StoredCredential> availableCredentials,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PresentationSubmissionResult> SubmitPresentationAsync(
            PresentationRequestInfo request,
            PresentationSubmissionOptions options,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PresentationSubmissionResult> SendErrorResponseAsync(
            PresentationRequestInfo request,
            string errorCode,
            string? errorDescription = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateClientAsync(PresentationRequestInfo request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class StubWalletAttestationsProvider : IWalletAttestationsProvider
    {
        public Task<string> GetWalletAttestationAsync(KeyInfo keyInfo, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("wia-token");
        }

        public Task<string> GetKeyAttestationAsync(IReadOnlyList<KeyInfo> keys, string? nonce = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("wua-token");
        }
    }
}
