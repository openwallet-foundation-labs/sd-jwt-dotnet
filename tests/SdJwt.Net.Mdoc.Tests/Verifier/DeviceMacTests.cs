using System.Security.Cryptography;
using FluentAssertions;
using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cose;
using SdJwt.Net.Mdoc.Handover;
using SdJwt.Net.Mdoc.Issuer;
using SdJwt.Net.Mdoc.Models;
using SdJwt.Net.Mdoc.Namespaces;
using SdJwt.Net.Mdoc.Verifier;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Verifier;

/// <summary>
/// Tests for ISO/IEC 18013-5 DeviceMac (COSE_Mac0) verification with ECDH/HKDF EMacKey derivation.
/// </summary>
public class DeviceMacTests : TestBase
{
    private readonly ICoseCryptoProvider _cryptoProvider = new DefaultCoseCryptoProvider();

    [Fact]
    public void DeriveEMacKey_BothParties_ProduceIdenticalKey()
    {
        using var readerKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var deviceKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var transcript = SampleTranscript("nonce-1").ToCbor();

        var readerCose = CoseKey.FromECDsa(readerKey);
        var deviceCose = CoseKey.FromECDsa(deviceKey);

        // Reader derives with its private key + device public key.
        var readerSide = MdocKeyDerivation.DeriveEMacKey(readerCose, deviceCose.GetPublicKey(), transcript);
        // Device derives with its private key + reader public key.
        var deviceSide = MdocKeyDerivation.DeriveEMacKey(deviceCose, readerCose.GetPublicKey(), transcript);

        readerSide.Should().Equal(deviceSide);
        readerSide.Should().HaveCount(32);
    }

    [Fact]
    public void DeriveEMacKey_DifferentTranscript_ProducesDifferentKey()
    {
        using var readerKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var deviceKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var readerCose = CoseKey.FromECDsa(readerKey);
        var deviceCose = CoseKey.FromECDsa(deviceKey);

        var keyA = MdocKeyDerivation.DeriveEMacKey(readerCose, deviceCose.GetPublicKey(), SampleTranscript("a").ToCbor());
        var keyB = MdocKeyDerivation.DeriveEMacKey(readerCose, deviceCose.GetPublicKey(), SampleTranscript("b").ToCbor());

        keyA.Should().NotEqual(keyB);
    }

    [Fact]
    public async Task VerifyDocument_WithValidDeviceMac_Succeeds()
    {
        using var readerKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var readerCose = CoseKey.FromECDsa(readerKey);

        var (document, transcript) = await BuildDocumentWithDeviceMacAsync(readerCose, "nonce-valid");

        var options = new MdocVerificationOptions
        {
            VerifyCertificateChain = false,
            VerifyDeviceSignature = true,
            ReaderEphemeralKey = readerCose
        };
        var verifier = new MdocVerifier(options, _cryptoProvider);

        var result = verifier.VerifyDocument(document, transcript);

        result.IssuerSignatureValid.Should().BeTrue(string.Join("; ", result.Errors));
        result.DeviceSignatureValid.Should().BeTrue(string.Join("; ", result.Errors));
    }

    [Fact]
    public async Task VerifyDocument_WithDeviceMac_NoReaderKey_FailsClosed()
    {
        using var readerKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var readerCose = CoseKey.FromECDsa(readerKey);

        var (document, transcript) = await BuildDocumentWithDeviceMacAsync(readerCose, "nonce-noreader");

        // Reader ephemeral key intentionally omitted → cannot derive EMacKey → must fail closed.
        var options = new MdocVerificationOptions
        {
            VerifyCertificateChain = false,
            VerifyDeviceSignature = true
        };
        var verifier = new MdocVerifier(options, _cryptoProvider);

        var result = verifier.VerifyDocument(document, transcript);

        result.DeviceSignatureValid.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyDocument_WithDeviceMac_TamperedTranscript_Fails()
    {
        using var readerKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var readerCose = CoseKey.FromECDsa(readerKey);

        var (document, _) = await BuildDocumentWithDeviceMacAsync(readerCose, "nonce-original");

        // Verify against a transcript with a different nonce than the MAC was computed over.
        var tamperedTranscript = SampleTranscript("nonce-tampered");

        var options = new MdocVerificationOptions
        {
            VerifyCertificateChain = false,
            VerifyDeviceSignature = true,
            ReaderEphemeralKey = readerCose
        };
        var verifier = new MdocVerifier(options, _cryptoProvider);

        var result = verifier.VerifyDocument(document, tamperedTranscript);

        result.DeviceSignatureValid.Should().BeFalse();
    }

    private static SessionTranscript SampleTranscript(string nonce)
        => SessionTranscript.ForOpenId4Vp(
            "https://verifier.example.com",
            nonce,
            null,
            "https://verifier.example.com/callback");

    private async Task<(Document Document, SessionTranscript Transcript)> BuildDocumentWithDeviceMacAsync(
        CoseKey readerCose,
        string nonce)
    {
        var deviceCose = CoseKey.FromECDsa(DeviceKey);

        // A self-signed issuer certificate so the verifier can extract the issuer public key (x5chain).
        var certRequest = new System.Security.Cryptography.X509Certificates.CertificateRequest(
            "CN=Test Issuer", IssuerSigningKey, HashAlgorithmName.SHA256);
        using var cert = certRequest.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddYears(1));

        var document = await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(IssuerSigningKey)
            .WithIssuerCertificate(cert.RawData)
            .WithDeviceKey(deviceCose)
            .AddMdlElement(MdlDataElement.FamilyName, "Doe")
            .AddMdlElement(MdlDataElement.GivenName, "John")
            .WithValidity(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1))
            .BuildAsync(_cryptoProvider);

        var transcript = SampleTranscript(nonce);
        var transcriptBytes = transcript.ToCbor();

        // Holder side: derive EMacKey with device private key + reader public key.
        var eMacKey = MdocKeyDerivation.DeriveEMacKey(deviceCose, readerCose.GetPublicKey(), transcriptBytes);

        // Build COSE_Mac0 over MAC_structure ["MAC0", protected, external_aad, payload].
        var protectedHeader = CBORObject.NewMap();
        protectedHeader.Add(1, 5); // alg: HMAC 256/256
        var protectedBytes = protectedHeader.EncodeToBytes();

        var macStructure = CBORObject.NewArray();
        macStructure.Add("MAC0");
        macStructure.Add(protectedBytes);
        macStructure.Add(transcriptBytes);
        macStructure.Add(Array.Empty<byte>());
        var tag = await _cryptoProvider.MacAsync(macStructure.EncodeToBytes(), eMacKey, CoseAlgorithm.HMAC256);

        var mac0 = CBORObject.NewArray();
        mac0.Add(protectedBytes);
        mac0.Add(CBORObject.NewMap());
        mac0.Add(CBORObject.Null);
        mac0.Add(tag);

        document.DeviceSigned = new DeviceSigned
        {
            DeviceAuth = new DeviceAuth
            {
                DeviceMac = mac0.EncodeToBytes()
            }
        };

        return (document, transcript);
    }
}
