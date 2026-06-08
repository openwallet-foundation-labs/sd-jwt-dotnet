using System.Security.Cryptography;

namespace SdJwt.Net.Mdoc.Cose;

/// <summary>
/// Derives the session keys used by ISO/IEC 18013-5 device authentication.
/// </summary>
public static class MdocKeyDerivation
{
    private const int EMacKeyLength = 32;

    /// <summary>
    /// UTF-8 bytes of the HKDF info string for the device MAC key.
    /// </summary>
    private static readonly byte[] EMacKeyInfo = System.Text.Encoding.UTF8.GetBytes("EMacKey");

    /// <summary>
    /// Derives the <c>EMacKey</c> used to authenticate a <c>DeviceMac</c> (COSE_Mac0) per
    /// ISO/IEC 18013-5 §9.1.3: HKDF-SHA-256 over the ECDH shared secret, with salt
    /// <c>SHA-256(SessionTranscriptBytes)</c> and info <c>"EMacKey"</c>.
    /// </summary>
    /// <param name="privateKey">The local party's key including its private component (the reader's EReaderKey).</param>
    /// <param name="publicKey">The peer's public key (the device's ephemeral key from the MSO).</param>
    /// <param name="sessionTranscriptBytes">The CBOR-encoded SessionTranscript.</param>
    /// <returns>The 32-byte EMacKey.</returns>
    /// <exception cref="PlatformNotSupportedException">Thrown on target frameworks that lack raw ECDH/HKDF support.</exception>
    public static byte[] DeriveEMacKey(CoseKey privateKey, CoseKey publicKey, byte[] sessionTranscriptBytes)
    {
        if (privateKey == null)
        {
            throw new ArgumentNullException(nameof(privateKey));
        }

        if (publicKey == null)
        {
            throw new ArgumentNullException(nameof(publicKey));
        }

        if (sessionTranscriptBytes == null)
        {
            throw new ArgumentNullException(nameof(sessionTranscriptBytes));
        }

        if (!privateKey.HasPrivateKey)
        {
            throw new InvalidOperationException("A private key is required to derive the EMacKey.");
        }

#if NETSTANDARD2_1
        throw new PlatformNotSupportedException(
            "Device MAC key derivation requires raw ECDH and HKDF, which are not available on this target framework. " +
            "Use a net8.0 or later runtime, or use DeviceSignature-based device authentication.");
#else
        using var ownKey = CreateEcDiffieHellman(privateKey, includePrivate: true);
        using var peerKey = CreateEcDiffieHellman(publicKey, includePrivate: false);

        var sharedSecret = ownKey.DeriveRawSecretAgreement(peerKey.PublicKey);

        using var salt = SHA256.Create();
        var saltBytes = salt.ComputeHash(sessionTranscriptBytes);

        return HKDF.DeriveKey(
            HashAlgorithmName.SHA256,
            ikm: sharedSecret,
            outputLength: EMacKeyLength,
            salt: saltBytes,
            info: EMacKeyInfo);
#endif
    }

#if !NETSTANDARD2_1
    private static ECDiffieHellman CreateEcDiffieHellman(CoseKey key, bool includePrivate)
    {
        var parameters = new ECParameters
        {
            Curve = MapCurve(key.Curve),
            Q = new ECPoint { X = key.X, Y = key.Y }
        };

        if (includePrivate)
        {
            parameters.D = key.D;
        }

        var ecdh = ECDiffieHellman.Create();
        ecdh.ImportParameters(parameters);
        return ecdh;
    }

    private static ECCurve MapCurve(CoseCurve curve)
    {
        return curve switch
        {
            CoseCurve.P256 => ECCurve.NamedCurves.nistP256,
            CoseCurve.P384 => ECCurve.NamedCurves.nistP384,
            CoseCurve.P521 => ECCurve.NamedCurves.nistP521,
            _ => throw new NotSupportedException($"Curve {curve} is not supported for ECDH.")
        };
    }
#endif
}
