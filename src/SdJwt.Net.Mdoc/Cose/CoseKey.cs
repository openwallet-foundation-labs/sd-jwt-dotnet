using System.Security.Cryptography;
using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cbor;

namespace SdJwt.Net.Mdoc.Cose;

/// <summary>
/// COSE_Key representation per RFC 8152.
/// </summary>
public class CoseKey : ICborSerializable
{
    // COSE_Key parameter labels
    private const int KeyTypeLabel = 1;
    private const int AlgorithmLabel = 3;
    private const int CurveLabel = -1;
    private const int XCoordinateLabel = -2;
    private const int YCoordinateLabel = -3;
    private const int PrivateKeyLabel = -4;

    /// <summary>
    /// Gets the key type.
    /// </summary>
    public CoseKeyType KeyType { get; private set; }

    /// <summary>
    /// Gets the curve for EC2 keys.
    /// </summary>
    public CoseCurve Curve { get; private set; }

    /// <summary>
    /// Gets the X coordinate for EC2 keys.
    /// </summary>
    public byte[] X { get; private set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets the Y coordinate for EC2 keys.
    /// </summary>
    public byte[] Y { get; private set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets the private key bytes (D parameter) if available.
    /// </summary>
    public byte[]? D { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this key contains the private key portion.
    /// </summary>
    public bool HasPrivateKey => D != null && D.Length > 0;

    /// <summary>
    /// Creates a CoseKey from an ECDsa key.
    /// </summary>
    /// <param name="ecDsa">The ECDsa key.</param>
    /// <returns>A new CoseKey instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when ecDsa is null.</exception>
    public static CoseKey FromECDsa(ECDsa ecDsa)
    {
        if (ecDsa == null) throw new ArgumentNullException(nameof(ecDsa));

        var parameters = ecDsa.ExportParameters(includePrivateParameters: true);
        var curve = GetCoseCurve(parameters.Curve);

        return new CoseKey
        {
            KeyType = CoseKeyType.EC2,
            Curve = curve,
            X = parameters.Q.X ?? Array.Empty<byte>(),
            Y = parameters.Q.Y ?? Array.Empty<byte>(),
            D = parameters.D
        };
    }

    /// <summary>
    /// Creates a CoseKey from CBOR bytes.
    /// </summary>
    /// <param name="cborData">The CBOR-encoded key data.</param>
    /// <returns>A new CoseKey instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when cborData is null.</exception>
    public static CoseKey FromCbor(byte[] cborData)
    {
        if (cborData == null) throw new ArgumentNullException(nameof(cborData));

        var cbor = CBORObject.DecodeFromBytes(cborData);
        return FromCborObject(cbor);
    }

    /// <summary>
    /// Creates a CoseKey from a CBORObject.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new CoseKey instance.</returns>
    internal static CoseKey FromCborObject(CBORObject cbor)
    {
        var key = new CoseKey
        {
            KeyType = (CoseKeyType)cbor[KeyTypeLabel].AsInt32()
        };

        if (key.KeyType == CoseKeyType.EC2)
        {
            key.Curve = (CoseCurve)cbor[CurveLabel].AsInt32();
            key.X = cbor[XCoordinateLabel].GetByteString();
            key.Y = cbor[YCoordinateLabel].GetByteString();

            if (cbor.ContainsKey(PrivateKeyLabel))
            {
                key.D = cbor[PrivateKeyLabel].GetByteString();
            }
        }

        return key;
    }

    /// <summary>
    /// Gets a copy of this key containing only the public key portion.
    /// </summary>
    /// <returns>A new CoseKey with only public key data.</returns>
    public CoseKey GetPublicKey()
    {
        return new CoseKey
        {
            KeyType = KeyType,
            Curve = Curve,
            X = (byte[])X.Clone(),
            Y = (byte[])Y.Clone(),
            D = null
        };
    }

    /// <summary>
    /// Converts this CoseKey to an ECDsa key.
    /// </summary>
    /// <returns>A new ECDsa instance.</returns>
    public ECDsa ToECDsa()
    {
        var curve = GetEcCurve(Curve);
        var ecDsa = ECDsa.Create();

        var parameters = new ECParameters
        {
            Curve = curve,
            Q = new ECPoint
            {
                X = X,
                Y = Y
            }
        };

        if (HasPrivateKey)
        {
            parameters.D = D;
        }

        ecDsa.ImportParameters(parameters);
        return ecDsa;
    }

    /// <inheritdoc/>
    public byte[] ToCbor()
    {
        return ToCborObject().EncodeToBytes();
    }

    /// <inheritdoc/>
    public CBORObject ToCborObject()
    {
        var cbor = CBORObject.NewMap();
        cbor.Add(KeyTypeLabel, (int)KeyType);
        cbor.Add(CurveLabel, (int)Curve);
        cbor.Add(XCoordinateLabel, X);
        cbor.Add(YCoordinateLabel, Y);

        if (HasPrivateKey)
        {
            cbor.Add(PrivateKeyLabel, D);
        }

        return cbor;
    }

    private static CoseCurve GetCoseCurve(ECCurve curve)
    {
        if (curve.Oid?.FriendlyName == ECCurve.NamedCurves.nistP256.Oid?.FriendlyName ||
            curve.Oid?.Value == "1.2.840.10045.3.1.7")
        {
            return CoseCurve.P256;
        }

        if (curve.Oid?.FriendlyName == ECCurve.NamedCurves.nistP384.Oid?.FriendlyName ||
            curve.Oid?.Value == "1.3.132.0.34")
        {
            return CoseCurve.P384;
        }

        if (curve.Oid?.FriendlyName == ECCurve.NamedCurves.nistP521.Oid?.FriendlyName ||
            curve.Oid?.Value == "1.3.132.0.35")
        {
            return CoseCurve.P521;
        }

        throw new NotSupportedException($"Curve {curve.Oid?.FriendlyName} is not supported.");
    }

    private static ECCurve GetEcCurve(CoseCurve curve)
    {
        return curve switch
        {
            CoseCurve.P256 => ECCurve.NamedCurves.nistP256,
            CoseCurve.P384 => ECCurve.NamedCurves.nistP384,
            CoseCurve.P521 => ECCurve.NamedCurves.nistP521,
            _ => throw new NotSupportedException($"Curve {curve} is not supported.")
        };
    }
}
