using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.SiopV2;
using SdJwt.Net.SiopV2.Did;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.SiopV2.Tests.Did;

public class DidResolverTests
{
    // -------------------------------------------------------------------------
    // DidJwkResolver
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DidJwkResolver_WithP256Key_ResolvesCorrectJwk()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var jwk = JsonWebKeyConverter.ConvertFromSecurityKey(new ECDsaSecurityKey(ecdsa));
        var jwkJson = SiopSubject.SerializePublicJwk(jwk);
        var did = "did:jwk:" + Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(jwkJson));

        var resolver = new DidJwkResolver();
        var resolvedKey = await resolver.ResolveKeyAsync(did, keyId: null);

        var resolved = resolvedKey as JsonWebKey;
        resolved.Should().NotBeNull();
        resolved!.Kty.Should().Be("EC");
        resolved.Crv.Should().Be("P-256");
        resolved.X.Should().Be(jwk.X);
        resolved.Y.Should().Be(jwk.Y);
    }

    [Fact]
    public async Task DidJwkResolver_WithInvalidBase64_ThrowsSecurityTokenException()
    {
        var resolver = new DidJwkResolver();

        var act = () => resolver.ResolveKeyAsync("did:jwk:!!not-base64!!", keyId: null);

        await act.Should().ThrowAsync<SecurityTokenException>();
    }

    [Fact]
    public async Task DidJwkResolver_WithNonJwkPrefix_ThrowsArgumentException()
    {
        var resolver = new DidJwkResolver();

        var act = () => resolver.ResolveKeyAsync("did:key:z6MkSomething", keyId: null);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DidJwkResolver_WithMalformedJsonPayload_ThrowsSecurityTokenException()
    {
        var did = "did:jwk:" + Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes("not-json"));
        var resolver = new DidJwkResolver();

        var act = () => resolver.ResolveKeyAsync(did, keyId: null);

        await act.Should().ThrowAsync<SecurityTokenException>();
    }

    // -------------------------------------------------------------------------
    // DidKeyResolver
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DidKeyResolver_WithP256Key_ResolvesCorrectKeyCoordinates()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var ecParams = ecdsa.ExportParameters(false);
        var did = BuildP256DidKey(ecParams);

        var resolver = new DidKeyResolver();
        var resolvedKey = await resolver.ResolveKeyAsync(did, keyId: null);

        var resolved = resolvedKey as JsonWebKey;
        resolved.Should().NotBeNull();
        resolved!.Kty.Should().Be(JsonWebAlgorithmsKeyTypes.EllipticCurve);
        resolved.Crv.Should().Be("P-256");
        resolved.X.Should().Be(Base64UrlEncoder.Encode(ecParams.Q.X!));
        resolved.Y.Should().Be(Base64UrlEncoder.Encode(ecParams.Q.Y!));
    }

    [Fact]
    public async Task DidKeyResolver_WithEd25519Key_ReturnsOkpKey()
    {
        // Known Ed25519 did:key test vector (W3C did-key-2020 spec example)
        // 32-byte all-zeros key is invalid; use a published test vector instead.
        // This vector encodes a 32-byte Ed25519 public key.
        const string did = "did:key:z6MkhaXgBZDvotDkL5257faiztiGiC2QtKLGpbnnEGta2doK";

        var resolver = new DidKeyResolver();
        var resolvedKey = await resolver.ResolveKeyAsync(did, keyId: null);

        var resolved = resolvedKey as JsonWebKey;
        resolved.Should().NotBeNull();
        resolved!.Kty.Should().Be("OKP");
        resolved.Crv.Should().Be("Ed25519");
        resolved.X.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DidKeyResolver_WithNonBase58Prefix_ThrowsSecurityTokenException()
    {
        var resolver = new DidKeyResolver();

        // 'm' is base64 multibase, not base58btc
        var act = () => resolver.ResolveKeyAsync("did:key:mSomething", keyId: null);

        await act.Should().ThrowAsync<SecurityTokenException>().WithMessage("*multibase*");
    }

    [Fact]
    public async Task DidKeyResolver_WithNonDidKeyPrefix_ThrowsArgumentException()
    {
        var resolver = new DidKeyResolver();

        var act = () => resolver.ResolveKeyAsync("did:jwk:somevalue", keyId: null);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DidKeyResolver_WithUnsupportedMulticodec_ThrowsSecurityTokenException()
    {
        // Build a did:key with an unsupported multicodec prefix (e.g. RSA = 0x85 0x24)
        var unsupportedBytes = new byte[] { 0x85, 0x24, 0x00, 0x01 };
        var encoded = "z" + Base58Encode(unsupportedBytes);
        var did = "did:key:" + encoded;

        var resolver = new DidKeyResolver();

        var act = () => resolver.ResolveKeyAsync(did, keyId: null);

        await act.Should().ThrowAsync<SecurityTokenException>().WithMessage("*Unsupported*");
    }

    // -------------------------------------------------------------------------
    // End-to-end: validator with DID subject
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Validator_WithDidJwkSubject_ValidatesSuccessfully()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var signingKey = new ECDsaSecurityKey(ecdsa);
        var publicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(signingKey);
        var jwkJson = SiopSubject.SerializePublicJwk(publicJwk);
        var subject = "did:jwk:" + Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(jwkJson));

        var token = BuildDidSubjectToken(subject, signingKey);

        var validator = new SelfIssuedIdTokenValidator();
        var result = await validator.ValidateAsync(token, new SelfIssuedIdTokenValidationParameters
        {
            ExpectedAudience = "https://rp.example.com",
            ExpectedNonce = "nonce-456",
            DidKeyResolver = new DidJwkResolver()
        });

        result.Subject.Should().Be(subject);
        result.Payload.Should().NotBeNull();
    }

    [Fact]
    public async Task Validator_WithDidSubjectButNoResolver_ThrowsSecurityTokenException()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var signingKey = new ECDsaSecurityKey(ecdsa);
        var publicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(signingKey);
        var jwkJson = SiopSubject.SerializePublicJwk(publicJwk);
        var subject = "did:jwk:" + Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(jwkJson));

        var token = BuildDidSubjectToken(subject, signingKey);

        var validator = new SelfIssuedIdTokenValidator();
        var act = () => validator.ValidateAsync(token, new SelfIssuedIdTokenValidationParameters
        {
            ExpectedAudience = "https://rp.example.com",
            ExpectedNonce = "nonce-456"
            // DidKeyResolver intentionally omitted
        });

        await act.Should().ThrowAsync<SecurityTokenException>()
            .WithMessage("*IDidKeyResolver*");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds a minimal SIOPv2 ID Token with iss == sub == the provided DID.
    /// </summary>
    private static string BuildDidSubjectToken(string subject, SecurityKey signingKey)
    {
        var handler = new JwtSecurityTokenHandler();
        var payload = new JwtPayload
        {
            { JwtRegisteredClaimNames.Iss, subject },
            { JwtRegisteredClaimNames.Sub, subject },
            { JwtRegisteredClaimNames.Aud, "https://rp.example.com" },
            { JwtRegisteredClaimNames.Nonce, "nonce-456" },
            { JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds() }
        };
        return handler.WriteToken(new JwtSecurityToken(
            new JwtHeader(new SigningCredentials(signingKey, SecurityAlgorithms.EcdsaSha256)),
            payload));
    }

    /// <summary>
    /// Constructs a did:key DID from a P-256 EC public key using uncompressed point format.
    /// Both compressed and uncompressed are valid per the spec; uncompressed is used here
    /// because Windows CNG's ImportSubjectPublicKeyInfo reliably accepts it.
    /// </summary>
    private static string BuildP256DidKey(ECParameters ecParams)
    {
        var x = ecParams.Q.X!;
        var y = ecParams.Q.Y!;

        // Pad X and Y to 32 bytes each (P-256 field size)
        var xPad = new byte[32];
        var yPad = new byte[32];
        x.CopyTo(xPad, 32 - x.Length);
        y.CopyTo(yPad, 32 - y.Length);

        // Uncompressed point: 0x04 + X + Y (65 bytes total)
        var uncompressed = new byte[65];
        uncompressed[0] = 0x04;
        xPad.CopyTo(uncompressed, 1);
        yPad.CopyTo(uncompressed, 33);

        // P-256 multicodec prefix: 0x80 0x24
        var multicodecBytes = new byte[] { 0x80, 0x24 }.Concat(uncompressed).ToArray();
        return "did:key:z" + Base58Encode(multicodecBytes);
    }

    private const string Base58Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    private static string Base58Encode(byte[] bytes)
    {
        var value = new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
        var sb = new StringBuilder();
        while (value > BigInteger.Zero)
        {
            value = BigInteger.DivRem(value, 58, out var remainder);
            sb.Insert(0, Base58Alphabet[(int)remainder]);
        }
        foreach (var b in bytes)
        {
            if (b == 0)
                sb.Insert(0, '1');
            else
                break;
        }
        return sb.ToString();
    }
}
