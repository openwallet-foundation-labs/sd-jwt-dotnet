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

        // Verify MSO revocation status if enabled
        if (_options.VerifyRevocation)
        {
            if (_options.RevocationProvider == null)
            {
                result.Errors.Add("Revocation checking is enabled but no RevocationProvider is configured.");
                return result;
            }

            var isRevoked = _options.RevocationProvider
                .IsRevokedAsync(GetIssuerCertificateBytes(document), document.DocType)
                .GetAwaiter().GetResult();
            result.RevocationCheckPassed = !isRevoked;
            if (isRevoked)
            {
                result.Errors.Add("MSO revocation check failed: credential has been revoked.");
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
        byte[]? certBytes = null;
        if (coseSign1.UnprotectedHeaders.TryGetValue("x5chain", out var x5chainObj) &&
            x5chainObj is byte[] rawCertBytes)
        {
            certBytes = rawCertBytes;
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

        // Verify certificate chain if enabled
        if (_options.VerifyCertificateChain && certBytes != null)
        {
            var chainValid = VerifyCertificateChain(certBytes);
            result.CertificateChainValid = chainValid;
            if (!chainValid)
            {
                result.Errors.Add("Issuer certificate chain validation failed.");
            }
        }

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

                // Compute digest of tagged item using algorithm from MSO
                var itemBytes = CBORObject.FromObjectAndTag(item.ToCbor(), 24).EncodeToBytes();
                var actualDigest = ComputeDigest(itemBytes, mso.DigestAlgorithm);

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
        if (document.DeviceSigned?.DeviceAuth == null)
        {
            return false;
        }

        var deviceAuth = document.DeviceSigned.DeviceAuth;

        // DeviceSignature (COSE_Sign1) takes priority over DeviceMac (COSE_Mac0)
        if (deviceAuth.DeviceSignature != null)
        {
            return VerifyDeviceSign1(deviceAuth.DeviceSignature, mso, sessionTranscript);
        }

        if (deviceAuth.DeviceMac != null)
        {
            return VerifyDeviceMac0(deviceAuth.DeviceMac, mso, sessionTranscript);
        }

        return false;
    }

    private bool VerifyDeviceSign1(
        byte[] deviceSignatureBytes,
        MobileSecurityObject mso,
        SessionTranscript sessionTranscript)
    {
        if (mso.DeviceKeyInfo?.DeviceKey == null || mso.DeviceKeyInfo.DeviceKey.Length == 0)
        {
            return false;
        }

        // Parse device signature
        CoseSign1 deviceSig;
        try
        {
            deviceSig = CoseSign1.FromCbor(deviceSignatureBytes);
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

    private bool VerifyDeviceMac0(
        byte[] deviceMacBytes,
        MobileSecurityObject mso,
        SessionTranscript sessionTranscript)
    {
        if (mso.DeviceKeyInfo?.DeviceKey == null || mso.DeviceKeyInfo.DeviceKey.Length == 0)
        {
            return false;
        }

        try
        {
            // Parse COSE_Mac0: [protected, unprotected, payload, tag]
            var macObj = CBORObject.DecodeFromBytes(deviceMacBytes);
            if (macObj.HasMostOuterTag(17))
            {
                macObj = macObj.UntagOne();
            }
            if (macObj.Type != CBORType.Array || macObj.Count < 4)
            {
                return false;
            }

            var protectedHeader = macObj[0].GetByteString();
            var payload = macObj[2].IsNull ? Array.Empty<byte>() : macObj[2].GetByteString();
            var tag = macObj[3].GetByteString();

            // Build MAC_structure: ["MAC0", protected, external_aad, payload]
            var macStructure = CBORObject.NewArray();
            macStructure.Add("MAC0");
            macStructure.Add(protectedHeader);
            macStructure.Add(sessionTranscript.ToCbor());
            macStructure.Add(payload);
            var macStructureBytes = macStructure.EncodeToBytes();

            // Derive shared key from device key (ECDH)
            // For DeviceMac, the key is typically derived via ECDH between reader and device.
            // The device key from MSO is used to derive the shared secret.
            var coseKey = CoseKey.FromCbor(mso.DeviceKeyInfo.DeviceKey);
            var deviceKeyBytes = coseKey.ToECDsa().ExportSubjectPublicKeyInfo();

            // Use the raw device key bytes as MAC key (simplified; real ECDH would require reader private key)
            return _cryptoProvider.VerifyMacAsync(macStructureBytes, tag, deviceKeyBytes).GetAwaiter().GetResult();
        }
        catch
        {
            return false;
        }
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

    private bool VerifyCertificateChain(byte[] certBytes)
    {
#if NETSTANDARD2_1
        // Limited chain validation on netstandard2.1
        try
        {
            using var cert = new X509Certificate2(certBytes);
            // Check basic validity
            if (cert.NotAfter < DateTime.UtcNow || cert.NotBefore > DateTime.UtcNow)
            {
                return false;
            }

            // Check against trusted issuers
            if (_options.TrustedIssuers.Count > 0)
            {
                return MatchesTrustedIssuer(cert);
            }

            return true;
        }
        catch
        {
            return false;
        }
#else
#if NET9_0_OR_GREATER
        using var cert = X509CertificateLoader.LoadCertificate(certBytes);
#else
        using var cert = new X509Certificate2(certBytes);
#endif
        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

        // Add trusted issuer certificates as extra store certs
        foreach (var trustedCertBytes in _options.TrustedIssuers)
        {
#if NET9_0_OR_GREATER
            var trustedCert = X509CertificateLoader.LoadCertificate(trustedCertBytes);
#else
            var trustedCert = new X509Certificate2(trustedCertBytes);
#endif
            chain.ChainPolicy.ExtraStore.Add(trustedCert);
        }

        if (_options.TrustedIssuers.Count > 0)
        {
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            foreach (var trustedCertBytes in _options.TrustedIssuers)
            {
#if NET9_0_OR_GREATER
                var trustedCert = X509CertificateLoader.LoadCertificate(trustedCertBytes);
#else
                var trustedCert = new X509Certificate2(trustedCertBytes);
#endif
                chain.ChainPolicy.CustomTrustStore.Add(trustedCert);
            }
        }

        var isValid = chain.Build(cert);

        if (!isValid && _options.TrustedIssuers.Count > 0)
        {
            // Also try direct match against trusted issuers
            return MatchesTrustedIssuer(cert);
        }

        return isValid;
#endif
    }

    private bool MatchesTrustedIssuer(X509Certificate2 cert)
    {
        var certThumbprint = cert.GetCertHash();
        foreach (var trustedCertBytes in _options.TrustedIssuers)
        {
#if NET9_0_OR_GREATER
            using var trustedCert = X509CertificateLoader.LoadCertificate(trustedCertBytes);
#else
            using var trustedCert = new X509Certificate2(trustedCertBytes);
#endif
            if (CryptographicOperations.FixedTimeEquals(certThumbprint, trustedCert.GetCertHash()))
            {
                return true;
            }
        }
        return false;
    }

    private static byte[] GetIssuerCertificateBytes(Document document)
    {
        if (document.IssuerSigned.IssuerAuth == null || document.IssuerSigned.IssuerAuth.Length == 0)
        {
            return Array.Empty<byte>();
        }

        try
        {
            var coseSign1 = CoseSign1.FromCbor(document.IssuerSigned.IssuerAuth);
            if (coseSign1.UnprotectedHeaders.TryGetValue("x5chain", out var x5chainObj) &&
                x5chainObj is byte[] rawCertBytes)
            {
                return rawCertBytes;
            }
        }
        catch
        {
            // Fall through and return empty
        }

        return Array.Empty<byte>();
    }

    private static byte[] ComputeDigest(byte[] data, string algorithm)
    {
#if NETSTANDARD2_1
        using var hashAlgorithm = algorithm switch
        {
            "SHA-256" => (HashAlgorithm)SHA256.Create(),
            "SHA-384" => SHA384.Create(),
            "SHA-512" => SHA512.Create(),
            _ => throw new NotSupportedException($"Digest algorithm '{algorithm}' is not supported.")
        };
        return hashAlgorithm.ComputeHash(data);
#else
        return algorithm switch
        {
            "SHA-256" => SHA256.HashData(data),
            "SHA-384" => SHA384.HashData(data),
            "SHA-512" => SHA512.HashData(data),
            _ => throw new NotSupportedException($"Digest algorithm '{algorithm}' is not supported.")
        };
#endif
    }
}
