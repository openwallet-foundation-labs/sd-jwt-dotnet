using System.Security.Cryptography;
using PeterO.Cbor;

namespace SdJwt.Net.Mdoc.Cose;

/// <summary>
/// Default implementation of ICoseCryptoProvider using .NET cryptographic primitives.
/// </summary>
public class DefaultCoseCryptoProvider : ICoseCryptoProvider
{
    /// <inheritdoc/>
    public byte[] Sign(CoseSign1 coseSign1, ECDsa privateKey)
    {
        if (coseSign1 == null) throw new ArgumentNullException(nameof(coseSign1));
        if (privateKey == null) throw new ArgumentNullException(nameof(privateKey));

        // Build protected header
        var protectedHeader = CBORObject.NewMap();
        protectedHeader.Add(1, (int)coseSign1.Algorithm); // alg
        var protectedBytes = protectedHeader.EncodeToBytes();

        // Build Sig_structure: ["Signature1", protected, external_aad, payload]
        var sigStructure = CBORObject.NewArray();
        sigStructure.Add("Signature1");
        sigStructure.Add(protectedBytes);
        sigStructure.Add(Array.Empty<byte>()); // external_aad
        sigStructure.Add(coseSign1.Payload ?? Array.Empty<byte>());
        var toBeSigned = sigStructure.EncodeToBytes();

        // Sign
        var hashAlgorithm = GetHashAlgorithm(coseSign1.Algorithm);
        var signature = SignDataP1363(privateKey, toBeSigned, hashAlgorithm);

        // Build COSE_Sign1 structure
        var result = CBORObject.NewArray();
        result.Add(protectedBytes);

        // Build unprotected header
        var unprotected = CBORObject.NewMap();
        foreach (var (key, value) in coseSign1.UnprotectedHeaders)
        {
            if (key == "x5chain" && value is byte[] certBytes)
            {
                unprotected.Add(33, certBytes); // x5chain header label is 33
            }
        }
        result.Add(unprotected);

        result.Add(coseSign1.Payload);
        result.Add(signature);

        return result.EncodeToBytes();
    }

    /// <inheritdoc/>
    public bool Verify(CoseSign1 coseSign1, ECDsa publicKey, byte[]? externalAad = null)
    {
        if (coseSign1 == null) throw new ArgumentNullException(nameof(coseSign1));
        if (publicKey == null) throw new ArgumentNullException(nameof(publicKey));

        if (coseSign1.Signature == null || coseSign1.Signature.Length == 0)
        {
            return false;
        }

        // Build protected header
        var protectedHeader = CBORObject.NewMap();
        protectedHeader.Add(1, (int)coseSign1.Algorithm);
        var protectedBytes = protectedHeader.EncodeToBytes();

        // Build Sig_structure
        var sigStructure = CBORObject.NewArray();
        sigStructure.Add("Signature1");
        sigStructure.Add(protectedBytes);
        sigStructure.Add(externalAad ?? Array.Empty<byte>());
        sigStructure.Add(coseSign1.Payload ?? Array.Empty<byte>());
        var toBeSigned = sigStructure.EncodeToBytes();

        var hashAlgorithm = GetHashAlgorithm(coseSign1.Algorithm);

        try
        {
            return VerifyDataP1363(publicKey, toBeSigned, coseSign1.Signature, hashAlgorithm);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public Task<byte[]> SignAsync(
        byte[] payload,
        CoseKey privateKey,
        CoseAlgorithm algorithm,
        byte[]? protectedHeaders = null,
        byte[]? externalAad = null)
    {
        if (payload == null) throw new ArgumentNullException(nameof(payload));
        if (privateKey == null) throw new ArgumentNullException(nameof(privateKey));

        if (!privateKey.HasPrivateKey)
        {
            throw new InvalidOperationException("Private key is required for signing.");
        }

        using var ecDsa = privateKey.ToECDsa();
        var hashAlgorithm = GetHashAlgorithm(algorithm);

        var signature = SignDataP1363(ecDsa, payload, hashAlgorithm);

        return Task.FromResult(signature);
    }

    /// <inheritdoc/>
    public Task<bool> VerifyAsync(
        byte[] signatureStructure,
        byte[] signature,
        CoseKey publicKey,
        CoseAlgorithm algorithm)
    {
        if (signatureStructure == null) throw new ArgumentNullException(nameof(signatureStructure));
        if (signature == null) throw new ArgumentNullException(nameof(signature));
        if (publicKey == null) throw new ArgumentNullException(nameof(publicKey));

        using var ecDsa = publicKey.ToECDsa();
        var hashAlgorithm = GetHashAlgorithm(algorithm);

        try
        {
            var isValid = VerifyDataP1363(ecDsa, signatureStructure, signature, hashAlgorithm);
            return Task.FromResult(isValid);
        }
        catch (CryptographicException)
        {
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc/>
    public Task<byte[]> MacAsync(
        byte[] payload,
        byte[] key,
        CoseAlgorithm algorithm,
        byte[]? externalAad = null)
    {
        if (payload == null) throw new ArgumentNullException(nameof(payload));
        if (key == null) throw new ArgumentNullException(nameof(key));

        using var hmac = new HMACSHA256(key);
        var mac = hmac.ComputeHash(payload);

        return Task.FromResult(mac);
    }

    /// <inheritdoc/>
    public Task<bool> VerifyMacAsync(
        byte[] macStructure,
        byte[] mac,
        byte[] key)
    {
        if (macStructure == null) throw new ArgumentNullException(nameof(macStructure));
        if (mac == null) throw new ArgumentNullException(nameof(mac));
        if (key == null) throw new ArgumentNullException(nameof(key));

        using var hmac = new HMACSHA256(key);
        var computedMac = hmac.ComputeHash(macStructure);

        // Use constant-time comparison
        var isValid = CryptographicOperations.FixedTimeEquals(computedMac, mac);

        return Task.FromResult(isValid);
    }

    private static HashAlgorithmName GetHashAlgorithm(CoseAlgorithm algorithm)
    {
        return algorithm switch
        {
            CoseAlgorithm.ES256 => HashAlgorithmName.SHA256,
            CoseAlgorithm.ES384 => HashAlgorithmName.SHA384,
            CoseAlgorithm.ES512 => HashAlgorithmName.SHA512,
            _ => throw new NotSupportedException($"Algorithm {algorithm} is not supported.")
        };
    }

    private static byte[] SignDataP1363(ECDsa ecdsa, byte[] data, HashAlgorithmName hashAlgorithm)
    {
#if NETSTANDARD2_1
        // netstandard2.1 doesn't support DSASignatureFormat, so we need to convert DER to P1363
        var derSignature = ecdsa.SignData(data, hashAlgorithm);
        return ConvertDerToP1363(derSignature, ecdsa.KeySize);
#else
        return ecdsa.SignData(data, hashAlgorithm, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
#endif
    }

    private static bool VerifyDataP1363(ECDsa ecdsa, byte[] data, byte[] signature, HashAlgorithmName hashAlgorithm)
    {
#if NETSTANDARD2_1
        // netstandard2.1 doesn't support DSASignatureFormat, so we need to convert P1363 to DER
        var derSignature = ConvertP1363ToDer(signature);
        return ecdsa.VerifyData(data, derSignature, hashAlgorithm);
#else
        return ecdsa.VerifyData(data, signature, hashAlgorithm, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
#endif
    }

#if NETSTANDARD2_1
    private static byte[] ConvertDerToP1363(byte[] derSignature, int keySize)
    {
        // DER format: 0x30 [length] 0x02 [r-length] [r] 0x02 [s-length] [s]
        var componentLength = keySize / 8;
        var result = new byte[componentLength * 2];

        var offset = 2; // Skip 0x30 and total length

        // Read R
        if (derSignature[offset] != 0x02) throw new InvalidOperationException("Invalid DER signature");
        offset++;
        var rLength = (int)derSignature[offset++];
        var rStart = offset;
        if (derSignature[rStart] == 0x00) { rStart++; rLength--; }
        var rPadding = componentLength - rLength;
        Array.Copy(derSignature, rStart, result, rPadding, rLength);
        offset += derSignature[offset - 1] == 0x00 ? rLength + 1 : rLength;

        // Read S
        if (derSignature[offset] != 0x02) throw new InvalidOperationException("Invalid DER signature");
        offset++;
        var sLength = (int)derSignature[offset++];
        var sStart = offset;
        if (derSignature[sStart] == 0x00) { sStart++; sLength--; }
        var sPadding = componentLength - sLength;
        Array.Copy(derSignature, sStart, result, componentLength + sPadding, sLength);

        return result;
    }

    private static byte[] ConvertP1363ToDer(byte[] p1363Signature)
    {
        var componentLength = p1363Signature.Length / 2;

        // Find actual R length (skip leading zeros)
        var rStart = 0;
        while (rStart < componentLength && p1363Signature[rStart] == 0) rStart++;
        var rLength = componentLength - rStart;
        var rNeedsPadding = (p1363Signature[rStart] & 0x80) != 0;

        // Find actual S length (skip leading zeros)
        var sStart = componentLength;
        while (sStart < p1363Signature.Length && p1363Signature[sStart] == 0) sStart++;
        var sLength = p1363Signature.Length - sStart;
        var sNeedsPadding = (p1363Signature[sStart] & 0x80) != 0;

        var totalLength = 2 + rLength + (rNeedsPadding ? 1 : 0) + 2 + sLength + (sNeedsPadding ? 1 : 0);
        var result = new byte[2 + totalLength];
        var offset = 0;

        result[offset++] = 0x30;
        result[offset++] = (byte)totalLength;

        result[offset++] = 0x02;
        result[offset++] = (byte)(rLength + (rNeedsPadding ? 1 : 0));
        if (rNeedsPadding) result[offset++] = 0x00;
        Array.Copy(p1363Signature, rStart, result, offset, rLength);
        offset += rLength;

        result[offset++] = 0x02;
        result[offset++] = (byte)(sLength + (sNeedsPadding ? 1 : 0));
        if (sNeedsPadding) result[offset++] = 0x00;
        Array.Copy(p1363Signature, sStart, result, offset, sLength);

        return result;
    }
#endif
}
