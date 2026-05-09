using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace SdJwt.Net.SiopV2.Did;

/// <summary>
/// Resolves verification keys from did:key DIDs (SIOPv2 draft-13 Section 6.2.2).
/// Supports Ed25519 (multicodec 0xED01), P-256 (0x1200), P-384 (0x1201), and P-521 (0x1202).
/// The multibase prefix 'z' (base58btc) is the only accepted encoding, as required by the spec.
/// Both compressed (0x02/0x03) and uncompressed (0x04) EC point formats are accepted.
/// </summary>
public sealed class DidKeyResolver : IDidKeyResolver
{
    // Multicodec two-byte varint prefixes
    private static readonly byte[] Ed25519Prefix = [0xED, 0x01];
    private static readonly byte[] P256Prefix = [0x80, 0x24];
    private static readonly byte[] P384Prefix = [0x81, 0x24];
    private static readonly byte[] P521Prefix = [0x82, 0x24];

    // DER-encoded curve OIDs
    // P-256: 1.2.840.10045.3.1.7
    private static readonly byte[] P256CurveOid = [0x06, 0x08, 0x2A, 0x86, 0x48, 0xCE, 0x3D, 0x03, 0x01, 0x07];
    // P-384: 1.3.132.0.34
    private static readonly byte[] P384CurveOid = [0x06, 0x05, 0x2B, 0x81, 0x04, 0x00, 0x22];
    // P-521: 1.3.132.0.35
    private static readonly byte[] P521CurveOid = [0x06, 0x05, 0x2B, 0x81, 0x04, 0x00, 0x23];
    // EC public key algorithm OID: 1.2.840.10045.2.1
    private static readonly byte[] EcAlgorithmOid = [0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x3D, 0x02, 0x01];

    /// <inheritdoc />
    public Task<SecurityKey> ResolveKeyAsync(string did, string? keyId, CancellationToken cancellationToken = default)
    {
        const string didKeyPrefix = "did:key:";
        if (!did.StartsWith(didKeyPrefix, StringComparison.Ordinal))
        {
            throw new ArgumentException($"Not a did:key DID: '{did}'", nameof(did));
        }

        var multibaseKey = did.Substring(didKeyPrefix.Length);
        if (multibaseKey.Length == 0 || multibaseKey[0] != 'z')
        {
            throw new SecurityTokenException(
                $"did:key uses unsupported multibase encoding in '{did}'. Only base58btc ('z' prefix) is supported.");
        }

        byte[] keyBytes;
        try
        {
            keyBytes = Base58Decode(multibaseKey.Substring(1));
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException($"did:key base58btc decoding failed for '{did}'.", ex);
        }

        var key = ParseMulticodecKey(keyBytes, did);
        return Task.FromResult(key);
    }

    private static SecurityKey ParseMulticodecKey(byte[] keyBytes, string did)
    {
        if (HasPrefix(keyBytes, Ed25519Prefix))
        {
            var rawKey = keyBytes[Ed25519Prefix.Length..];
            if (rawKey.Length != 32)
            {
                throw new SecurityTokenException($"Invalid Ed25519 key length {rawKey.Length} in did:key '{did}' (expected 32).");
            }
            return new JsonWebKey { Kty = "OKP", Crv = "Ed25519", X = Base64UrlEncoder.Encode(rawKey) };
        }

        if (HasPrefix(keyBytes, P256Prefix))
        {
            return ParseEcKey(keyBytes[P256Prefix.Length..], "P-256", P256CurveOid, did);
        }

        if (HasPrefix(keyBytes, P384Prefix))
        {
            return ParseEcKey(keyBytes[P384Prefix.Length..], "P-384", P384CurveOid, did);
        }

        if (HasPrefix(keyBytes, P521Prefix))
        {
            return ParseEcKey(keyBytes[P521Prefix.Length..], "P-521", P521CurveOid, did);
        }

        throw new SecurityTokenException(
            $"Unsupported multicodec key type in did:key '{did}'. Supported: Ed25519 (0xED01), P-256 (0x1200), P-384 (0x1201), P-521 (0x1202).");
    }

    private static SecurityKey ParseEcKey(byte[] pointBytes, string crv, byte[] curveOid, string did)
    {
        // Build a SubjectPublicKeyInfo DER blob so ECDsa can import and decompress the point.
        var spki = BuildEcSpki(pointBytes, curveOid);

        ECParameters ecParams;
        try
        {
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(spki, out _);
            ecParams = ecdsa.ExportParameters(false);
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException($"EC key import failed for did:key '{did}'.", ex);
        }

        return new JsonWebKey
        {
            Kty = JsonWebAlgorithmsKeyTypes.EllipticCurve,
            Crv = crv,
            X = Base64UrlEncoder.Encode(ecParams.Q.X!),
            Y = Base64UrlEncoder.Encode(ecParams.Q.Y!)
        };
    }

    /// <summary>
    /// Builds a DER SubjectPublicKeyInfo for an EC public key point (compressed or uncompressed).
    /// Structure: SEQUENCE { SEQUENCE { OID ecPublicKey, OID curve }, BIT STRING { 0x00, point } }
    /// </summary>
    private static byte[] BuildEcSpki(byte[] pointBytes, byte[] curveOid)
    {
        // algorithmIdentifier SEQUENCE { ecOid, curveOid }
        var algIdContent = Concat(EcAlgorithmOid, curveOid);
        var algId = WrapSequence(algIdContent);

        // subjectPublicKey BIT STRING: unused-bits byte (0x00) + point
        var bitStringContent = new byte[1 + pointBytes.Length];
        bitStringContent[0] = 0x00;
        pointBytes.CopyTo(bitStringContent, 1);
        var bitString = WrapTag(0x03, bitStringContent);

        return WrapSequence(Concat(algId, bitString));
    }

    private static byte[] WrapSequence(byte[] content) => WrapTag(0x30, content);

    private static byte[] WrapTag(byte tag, byte[] content)
    {
        var lengthBytes = EncodeDerLength(content.Length);
        var result = new byte[1 + lengthBytes.Length + content.Length];
        result[0] = tag;
        lengthBytes.CopyTo(result, 1);
        content.CopyTo(result, 1 + lengthBytes.Length);
        return result;
    }

    private static byte[] EncodeDerLength(int length)
    {
        if (length < 0x80)
        {
            return [(byte)length];
        }

        if (length <= 0xFF)
        {
            return [0x81, (byte)length];
        }

        return [0x82, (byte)(length >> 8), (byte)(length & 0xFF)];
    }

    private static byte[] Concat(byte[] a, byte[] b)
    {
        var result = new byte[a.Length + b.Length];
        a.CopyTo(result, 0);
        b.CopyTo(result, a.Length);
        return result;
    }

    private static bool HasPrefix(byte[] data, byte[] prefix)
    {
        if (data.Length < prefix.Length)
        {
            return false;
        }

        for (int i = 0; i < prefix.Length; i++)
        {
            if (data[i] != prefix[i])
            {
                return false;
            }
        }

        return true;
    }

    // RFC 4648 base58btc alphabet
    private const string Base58Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    private static byte[] Base58Decode(string input)
    {
        int leadingZeros = 0;
        foreach (var c in input)
        {
            if (c == '1')
            {
                leadingZeros++;
            }
            else
            {
                break;
            }
        }

        var value = System.Numerics.BigInteger.Zero;
        var base58 = new System.Numerics.BigInteger(58);

        foreach (var c in input)
        {
            int digit = Base58Alphabet.IndexOf(c);
            if (digit < 0)
            {
                throw new FormatException($"Invalid base58btc character '{c}'.");
            }

            value = value * base58 + digit;
        }

        var bytes = value.ToByteArray(isUnsigned: true, isBigEndian: true);
        var output = new byte[leadingZeros + bytes.Length];
        bytes.CopyTo(output, leadingZeros);
        return output;
    }
}
