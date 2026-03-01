using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cose;
using SdJwt.Net.Mdoc.Handover;
using SdJwt.Net.Mdoc.Models;

namespace SdJwt.Net.Mdoc.Verifier;

/// <summary>
/// Verifies mdoc credentials per ISO 18013-5.
/// </summary>
public class MdocVerifier
{
    private readonly MdocVerificationOptions _options;
    private readonly ICoseCryptoProvider _cryptoProvider;

    /// <summary>
    /// Creates a new MdocVerifier with default options.
    /// </summary>
    public MdocVerifier() : this(new MdocVerificationOptions())
    {
    }

    /// <summary>
    /// Creates a new MdocVerifier with the specified options.
    /// </summary>
    /// <param name="options">Verification options.</param>
    public MdocVerifier(MdocVerificationOptions options)
        : this(options, new DefaultCoseCryptoProvider())
    {
    }

    /// <summary>
    /// Creates a new MdocVerifier with custom crypto provider.
    /// </summary>
    /// <param name="options">Verification options.</param>
    /// <param name="cryptoProvider">COSE crypto provider.</param>
    public MdocVerifier(MdocVerificationOptions options, ICoseCryptoProvider cryptoProvider)
    {
        _options = options;
        _cryptoProvider = cryptoProvider;
    }

    /// <summary>
    /// Verifies a DeviceResponse.
    /// </summary>
    /// <param name="response">The device response to verify.</param>
    /// <param name="sessionTranscript">Session transcript for device authentication.</param>
    /// <returns>Verification result for each document.</returns>
    public List<MdocVerificationResult> Verify(DeviceResponse response, SessionTranscript? sessionTranscript = null)
    {
        var results = new List<MdocVerificationResult>();

        foreach (var doc in response.Documents)
        {
            results.Add(VerifyDocument(doc, sessionTranscript));
        }

        return results;
    }

    /// <summary>
    /// Verifies a Document.
    /// </summary>
    /// <param name="document">The document to verify.</param>
    /// <param name="sessionTranscript">Session transcript for device authentication.</param>
    /// <returns>Verification result.</returns>
    public MdocVerificationResult VerifyDocument(Document document, SessionTranscript? sessionTranscript = null)
    {
        var result = new MdocVerificationResult();

        // Check allowed doc types
        if (_options.AllowedDocTypes.Count > 0 &&
            !_options.AllowedDocTypes.Contains(document.DocType))
        {
            result.Errors.Add($"Document type '{document.DocType}' is not allowed.");
            return result;
        }

        // Verify IssuerAuth signature
        var issuerAuthResult = VerifyIssuerAuth(document.IssuerSigned);
        if (!issuerAuthResult.IssuerSignatureValid)
        {
            result.Errors.AddRange(issuerAuthResult.Errors);
            return result;
        }

        result.IssuerSignatureValid = true;
        result.MobileSecurityObject = issuerAuthResult.MobileSecurityObject;

        // Verify MSO docType matches Document docType
        if (result.MobileSecurityObject?.DocType != document.DocType)
        {
            result.Errors.Add("MSO docType does not match Document docType.");
            return result;
        }

        // Verify digests
        var digestsValid = VerifyDigests(document, result.MobileSecurityObject);
        result.DigestsValid = digestsValid;
        if (!digestsValid)
        {
            result.Errors.Add("One or more value digests do not match.");
            return result;
        }

        // Verify validity period
        if (_options.VerifyValidity)
        {
            var validityValid = VerifyValidity(result.MobileSecurityObject);
            result.ValidityPeriodValid = validityValid;
            if (!validityValid)
            {
                result.Errors.Add("Credential is outside its validity period.");
                return result;
            }
        }
        else
        {
            result.ValidityPeriodValid = true;
        }

        // Verify device signature if present and required
        if (_options.VerifyDeviceSignature &&
            document.DeviceSigned?.DeviceAuth != null &&
            sessionTranscript != null)
        {
            var deviceSigValid = VerifyDeviceSignature(
                document,
                result.MobileSecurityObject,
                sessionTranscript);
            result.DeviceSignatureValid = deviceSigValid;
            if (!deviceSigValid)
            {
                result.Errors.Add("Device signature verification failed.");
                return result;
            }
        }

        // Extract verified claims
        result.VerifiedClaims = ExtractClaims(document);
        result.IsValid = true;

        return result;
    }

    /// <summary>
    /// Verifies an IssuerSigned structure and extracts the MSO.
    /// </summary>
    /// <param name="issuerSigned">The issuer-signed data.</param>
    /// <returns>Verification result with MSO.</returns>
    public MdocVerificationResult VerifyIssuerAuth(IssuerSigned issuerSigned)
    {
        var result = new MdocVerificationResult();

        if (issuerSigned.IssuerAuth == null || issuerSigned.IssuerAuth.Length == 0)
        {
            result.Errors.Add("IssuerAuth is missing.");
            return result;
        }

        // Parse COSE_Sign1
        CoseSign1 coseSign1;
        try
        {
            coseSign1 = CoseSign1.FromCbor(issuerSigned.IssuerAuth);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to parse IssuerAuth: {ex.Message}");
            return result;
        }

        // Extract public key from x5chain
        ECDsa? publicKey = null;
        if (coseSign1.UnprotectedHeaders.TryGetValue("x5chain", out var x5chainObj) &&
            x5chainObj is byte[] certBytes)
        {
            try
            {
                publicKey = LoadCertificatePublicKey(certBytes);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to extract public key from certificate: {ex.Message}");
                return result;
            }
        }

        if (publicKey == null)
        {
            result.Errors.Add("No public key available for signature verification.");
            return result;
        }

        // Verify signature
        if (!_cryptoProvider.Verify(coseSign1, publicKey))
        {
            result.Errors.Add("Issuer signature verification failed.");
            return result;
        }

        result.IssuerSignatureValid = true;

        // Parse MSO
        if (coseSign1.Payload == null || coseSign1.Payload.Length == 0)
        {
            result.Errors.Add("MSO payload is missing.");
            return result;
        }

        try
        {
            result.MobileSecurityObject = MobileSecurityObject.FromCbor(coseSign1.Payload);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to parse MSO: {ex.Message}");
            return result;
        }

        return result;
    }

    private bool VerifyDigests(Document document, MobileSecurityObject mso)
    {
        foreach (var (nameSpace, items) in document.IssuerSigned.NameSpaces)
        {
            if (!mso.ValueDigests.TryGetValue(nameSpace, out var digestMapping))
            {
                return false;
            }

            foreach (var item in items)
            {
                if (!digestMapping.Digests.TryGetValue(item.DigestId, out var expectedDigest))
                {
                    return false;
                }

                // Compute digest of tagged item
                var itemBytes = CBORObject.FromObjectAndTag(item.ToCbor(), 24).EncodeToBytes();
                var actualDigest = ComputeSha256(itemBytes);

                if (!CryptographicOperations.FixedTimeEquals(actualDigest, expectedDigest))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool VerifyValidity(MobileSecurityObject mso)
    {
        var now = _options.CurrentTime ?? DateTimeOffset.UtcNow;
        var tolerance = _options.ClockSkewTolerance;

        if (mso.ValidityInfo == null)
        {
            return false;
        }

        var validFrom = mso.ValidityInfo.ValidFrom;
        var validUntil = mso.ValidityInfo.ValidUntil;

        return now >= validFrom - tolerance && now <= validUntil + tolerance;
    }

    private bool VerifyDeviceSignature(
        Document document,
        MobileSecurityObject mso,
        SessionTranscript sessionTranscript)
    {
        if (document.DeviceSigned?.DeviceAuth?.DeviceSignature == null)
        {
            return false;
        }

        if (mso.DeviceKeyInfo?.DeviceKey == null || mso.DeviceKeyInfo.DeviceKey.Length == 0)
        {
            return false;
        }

        // Parse device signature
        CoseSign1 deviceSig;
        try
        {
            deviceSig = CoseSign1.FromCbor(document.DeviceSigned.DeviceAuth.DeviceSignature);
        }
        catch
        {
            return false;
        }

        // Get device public key - DeviceKey is stored as CBOR-encoded CoseKey
        ECDsa? deviceKey;
        try
        {
            var coseKey = CoseKey.FromCbor(mso.DeviceKeyInfo.DeviceKey);
            deviceKey = coseKey.ToECDsa();
        }
        catch
        {
            return false;
        }

        // Verify with external_aad = SessionTranscript
        var externalAad = sessionTranscript.ToCbor();
        return _cryptoProvider.Verify(deviceSig, deviceKey, externalAad);
    }

    private static Dictionary<string, Dictionary<string, object>> ExtractClaims(Document document)
    {
        var claims = new Dictionary<string, Dictionary<string, object>>();

        foreach (var (nameSpace, items) in document.IssuerSigned.NameSpaces)
        {
            var nsClaims = new Dictionary<string, object>();
            foreach (var item in items)
            {
                if (item.ElementValue != null)
                {
                    nsClaims[item.ElementIdentifier] = item.ElementValue;
                }
            }
            claims[nameSpace] = nsClaims;
        }

        return claims;
    }

    private static ECDsa LoadCertificatePublicKey(byte[] certBytes)
    {
#if NET9_0_OR_GREATER
        using var cert = X509CertificateLoader.LoadCertificate(certBytes);
#else
        using var cert = new X509Certificate2(certBytes);
#endif
        var publicKey = cert.GetECDsaPublicKey();
        if (publicKey == null)
        {
            throw new InvalidOperationException("Certificate does not contain an ECDsa public key.");
        }
        return publicKey;
    }

    private static byte[] ComputeSha256(byte[] data)
    {
#if NETSTANDARD2_1
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(data);
#else
        return SHA256.HashData(data);
#endif
    }
}
