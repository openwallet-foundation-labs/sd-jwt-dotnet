using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.OidFederation.Logic;
using SdJwt.Net.OidFederation.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Xunit;

namespace SdJwt.Net.OidFederation.Tests.Logic;

/// <summary>
/// Tests for <see cref="TrustMarkVerifier"/> — cryptographic verification of signed Trust Mark JWTs.
/// </summary>
public class TrustMarkVerifierTests
{
    private const string IssuerId = "https://tmi.example.com";
    private const string TrustMarkType = "https://tmi.example.com/mark/certified";
    private const string SubjectEntity = "https://leaf.example.com";

    private static (ECDsaSecurityKey Key, string Kid) CreateKey()
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var key = new ECDsaSecurityKey(ecdsa) { KeyId = Guid.NewGuid().ToString("N") };
        return (key, key.KeyId);
    }

    private static string CreateTrustMarkJwt(
        ECDsaSecurityKey signingKey,
        string iss = IssuerId,
        string sub = SubjectEntity,
        string trustMarkType = TrustMarkType,
        DateTimeOffset? exp = null,
        bool includeIat = true,
        string typ = "trust-mark+jwt")
    {
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.EcdsaSha256);
        var now = DateTimeOffset.UtcNow;
        var payload = new JwtPayload
        {
            ["iss"] = iss,
            ["sub"] = sub,
            ["trust_mark_type"] = trustMarkType
        };
        if (includeIat)
        {
            payload["iat"] = now.ToUnixTimeSeconds();
        }
        if (exp.HasValue)
        {
            payload["exp"] = exp.Value.ToUnixTimeSeconds();
        }

        var header = new JwtHeader(creds);
        header["typ"] = typ;
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(header, payload));
    }

    private static TrustMark TrustMarkFor(string jwt)
        => new()
        {
            Id = TrustMarkType,
            TrustMarkValue = jwt,
            Issuer = IssuerId,
            Subject = SubjectEntity
        };

    [Fact]
    public async Task VerifyAsync_WithValidlySignedTrustMark_Succeeds()
    {
        var (key, _) = CreateKey();
        var jwt = CreateTrustMarkJwt(key);
        var verifier = TrustMarkVerifier.FromTrustedKeys(
            new Dictionary<string, SecurityKey> { [IssuerId] = key });

        var result = await verifier.VerifyAsync(TrustMarkFor(jwt), SubjectEntity);

        result.IsValid.Should().BeTrue(result.Error);
        result.TrustMarkType.Should().Be(TrustMarkType);
        result.Issuer.Should().Be(IssuerId);
        result.Subject.Should().Be(SubjectEntity);
    }

    [Fact]
    public async Task VerifyAsync_WithForgedSignature_Fails()
    {
        var (signingKey, _) = CreateKey();
        var (trustedKey, _) = CreateKey(); // different key registered as trusted
        var jwt = CreateTrustMarkJwt(signingKey);
        var verifier = TrustMarkVerifier.FromTrustedKeys(
            new Dictionary<string, SecurityKey> { [IssuerId] = trustedKey });

        var result = await verifier.VerifyAsync(TrustMarkFor(jwt));

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be(OidFederationConstants.ErrorCodes.TrustMarkValidationFailed);
    }

    [Fact]
    public async Task VerifyAsync_WithUntrustedIssuer_Fails()
    {
        var (key, _) = CreateKey();
        var jwt = CreateTrustMarkJwt(key);
        // No keys registered → issuer key cannot be resolved (self-asserted trust mark).
        var verifier = TrustMarkVerifier.FromTrustedKeys(new Dictionary<string, SecurityKey>());

        var result = await verifier.VerifyAsync(TrustMarkFor(jwt));

        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("Could not resolve");
    }

    [Fact]
    public async Task VerifyAsync_WithWrongTyp_Fails()
    {
        var (key, _) = CreateKey();
        var jwt = CreateTrustMarkJwt(key, typ: "jwt");
        var verifier = TrustMarkVerifier.FromTrustedKeys(
            new Dictionary<string, SecurityKey> { [IssuerId] = key });

        var result = await verifier.VerifyAsync(TrustMarkFor(jwt));

        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("typ");
    }

    [Fact]
    public async Task VerifyAsync_WithExpiredTrustMark_Fails()
    {
        var (key, _) = CreateKey();
        var jwt = CreateTrustMarkJwt(key, exp: DateTimeOffset.UtcNow.AddHours(-1));
        var verifier = TrustMarkVerifier.FromTrustedKeys(
            new Dictionary<string, SecurityKey> { [IssuerId] = key });

        var result = await verifier.VerifyAsync(TrustMarkFor(jwt));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyAsync_WithSubjectMismatch_Fails()
    {
        var (key, _) = CreateKey();
        var jwt = CreateTrustMarkJwt(key, sub: "https://someone-else.example.com");
        var verifier = TrustMarkVerifier.FromTrustedKeys(
            new Dictionary<string, SecurityKey> { [IssuerId] = key });

        var result = await verifier.VerifyAsync(TrustMarkFor(jwt), SubjectEntity);

        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("sub");
    }

    [Fact]
    public async Task VerifyAsync_WithTrustMarkTypeMismatch_Fails()
    {
        var (key, _) = CreateKey();
        var jwt = CreateTrustMarkJwt(key, trustMarkType: "https://tmi.example.com/mark/other");
        var verifier = TrustMarkVerifier.FromTrustedKeys(
            new Dictionary<string, SecurityKey> { [IssuerId] = key });

        // TrustMark.Id is the certified type, but the JWT asserts a different type.
        var result = await verifier.VerifyAsync(TrustMarkFor(jwt));

        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("trust_mark_type");
    }

    [Fact]
    public async Task VerifyAsync_WithMissingIat_Fails()
    {
        var (key, _) = CreateKey();
        var jwt = CreateTrustMarkJwt(key, includeIat: false);
        var verifier = TrustMarkVerifier.FromTrustedKeys(
            new Dictionary<string, SecurityKey> { [IssuerId] = key });

        var result = await verifier.VerifyAsync(TrustMarkFor(jwt));

        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("iat");
    }

    [Fact]
    public async Task VerifyAsync_WithUnsignedPlainValue_Fails()
    {
        var verifier = TrustMarkVerifier.FromTrustedKeys(new Dictionary<string, SecurityKey>());
        var trustMark = new TrustMark { Id = TrustMarkType, TrustMarkValue = "not-a-jwt", Issuer = IssuerId };

        var result = await verifier.VerifyAsync(trustMark);

        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("signed JWT");
    }
}
