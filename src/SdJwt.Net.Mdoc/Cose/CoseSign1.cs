using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cbor;

namespace SdJwt.Net.Mdoc.Cose;

/// <summary>
/// COSE_Sign1 structure per RFC 8152.
/// </summary>
public class CoseSign1 : ICborSerializable
{
    private const int CoseSign1Tag = 18;
    private const string ContextString = "Signature1";

    /// <summary>
    /// Creates a new empty COSE_Sign1 structure.
    /// </summary>
    public CoseSign1()
    {
    }

    /// <summary>
    /// Creates a new COSE_Sign1 structure with algorithm and payload.
    /// </summary>
    /// <param name="algorithm">The signing algorithm.</param>
    /// <param name="payload">The payload to sign.</param>
    public CoseSign1(CoseAlgorithm algorithm, byte[]? payload)
    {
        Algorithm = algorithm;
        Payload = payload ?? Array.Empty<byte>();
    }

    /// <summary>
    /// Gets or sets the protected headers.
    /// </summary>
    public byte[] ProtectedHeaders { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the unprotected headers (keyed by string for convenience, converted to int on serialization).
    /// </summary>
    public Dictionary<string, object> UnprotectedHeaders { get; set; } = new();

    /// <summary>
    /// Gets or sets the payload.
    /// </summary>
    public byte[]? Payload { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the signature.
    /// </summary>
    public byte[]? Signature { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the algorithm.
    /// </summary>
    public CoseAlgorithm Algorithm { get; set; } = CoseAlgorithm.ES256;

    /// <summary>
    /// Creates a new COSE_Sign1 structure with a signature.
    /// </summary>
    /// <param name="payload">The payload to sign.</param>
    /// <param name="privateKey">The private key for signing.</param>
    /// <param name="algorithm">The signing algorithm.</param>
    /// <param name="cryptoProvider">The crypto provider.</param>
    /// <param name="externalAad">Optional external additional authenticated data.</param>
    /// <returns>A new CoseSign1 instance.</returns>
    public static async Task<CoseSign1> CreateAsync(
        byte[] payload,
        CoseKey privateKey,
        CoseAlgorithm algorithm,
        ICoseCryptoProvider cryptoProvider,
        byte[]? externalAad = null)
    {
        if (payload == null) throw new ArgumentNullException(nameof(payload));
        if (privateKey == null) throw new ArgumentNullException(nameof(privateKey));
        if (cryptoProvider == null) throw new ArgumentNullException(nameof(cryptoProvider));

        var coseSign1 = new CoseSign1
        {
            Payload = payload,
            Algorithm = algorithm
        };

        // Create protected headers with algorithm
        var protectedCbor = CBORObject.NewMap();
        protectedCbor.Add(1, (int)algorithm); // alg parameter
        coseSign1.ProtectedHeaders = protectedCbor.EncodeToBytes();

        // Create Sig_structure for signing
        var sigStructure = CreateSigStructure(
            coseSign1.ProtectedHeaders,
            externalAad ?? Array.Empty<byte>(),
            payload);

        // Sign
        coseSign1.Signature = await cryptoProvider.SignAsync(
            sigStructure,
            privateKey,
            algorithm);

        return coseSign1;
    }

    /// <summary>
    /// Verifies the signature.
    /// </summary>
    /// <param name="publicKey">The public key for verification.</param>
    /// <param name="cryptoProvider">The crypto provider.</param>
    /// <param name="externalAad">Optional external additional authenticated data.</param>
    /// <returns>True if the signature is valid.</returns>
    public async Task<bool> VerifyAsync(
        CoseKey publicKey,
        ICoseCryptoProvider cryptoProvider,
        byte[]? externalAad = null)
    {
        if (publicKey == null) throw new ArgumentNullException(nameof(publicKey));
        if (cryptoProvider == null) throw new ArgumentNullException(nameof(cryptoProvider));

        var sigStructure = CreateSigStructure(
            ProtectedHeaders,
            externalAad ?? Array.Empty<byte>(),
            Payload ?? Array.Empty<byte>());

        return await cryptoProvider.VerifyAsync(
            sigStructure,
            Signature ?? Array.Empty<byte>(),
            publicKey,
            Algorithm);
    }

    /// <summary>
    /// Creates a CoseSign1 from CBOR bytes.
    /// </summary>
    /// <param name="cborData">The CBOR-encoded data.</param>
    /// <returns>A new CoseSign1 instance.</returns>
    public static CoseSign1 FromCbor(byte[] cborData)
    {
        if (cborData == null) throw new ArgumentNullException(nameof(cborData));

        var cbor = CBORObject.DecodeFromBytes(cborData);

        // Handle tagged or untagged
        if (cbor.HasMostOuterTag(CoseSign1Tag))
        {
            cbor = cbor.UntagOne();
        }

        var coseSign1 = new CoseSign1
        {
            ProtectedHeaders = cbor[0].GetByteString(),
            Payload = cbor[2].IsNull ? Array.Empty<byte>() : cbor[2].GetByteString(),
            Signature = cbor[3].GetByteString()
        };

        // Parse algorithm from protected headers
        if (coseSign1.ProtectedHeaders.Length > 0)
        {
            var protectedCbor = CBORObject.DecodeFromBytes(coseSign1.ProtectedHeaders);
            if (protectedCbor.ContainsKey(1))
            {
                coseSign1.Algorithm = (CoseAlgorithm)protectedCbor[1].AsInt32();
            }
        }

        // Parse unprotected headers
        var unprotected = cbor[1];
        if (!unprotected.IsNull && unprotected.Type == CBORType.Map)
        {
            foreach (var key in unprotected.Keys)
            {
                var keyInt = key.AsInt32();
                var headerName = GetHeaderName(keyInt);
                if (keyInt == 33) // x5chain
                {
                    coseSign1.UnprotectedHeaders[headerName] = unprotected[key].GetByteString();
                }
                else
                {
                    coseSign1.UnprotectedHeaders[headerName] = unprotected[key];
                }
            }
        }

        return coseSign1;
    }

    /// <inheritdoc/>
    public byte[] ToCbor()
    {
        return ToCborObject().EncodeToBytes();
    }

    /// <inheritdoc/>
    public CBORObject ToCborObject()
    {
        var array = CBORObject.NewArray();
        array.Add(ProtectedHeaders);

        var unprotectedCbor = CBORObject.NewMap();
        foreach (var (key, value) in UnprotectedHeaders)
        {
            var headerLabel = GetHeaderLabel(key);
            if (value is byte[] bytes)
            {
                unprotectedCbor.Add(headerLabel, bytes);
            }
            else
            {
                unprotectedCbor.Add(headerLabel, CBORObject.FromObject(value));
            }
        }
        array.Add(unprotectedCbor);

        array.Add(Payload ?? Array.Empty<byte>());
        array.Add(Signature ?? Array.Empty<byte>());

        return CBORObject.FromObjectAndTag(array, CoseSign1Tag);
    }

    private static int GetHeaderLabel(string name)
    {
        return name switch
        {
            "alg" => 1,
            "crit" => 2,
            "content_type" or "cty" => 3,
            "kid" => 4,
            "x5chain" => 33,
            _ => throw new NotSupportedException($"Unknown header: {name}")
        };
    }

    private static string GetHeaderName(int label)
    {
        return label switch
        {
            1 => "alg",
            2 => "crit",
            3 => "content_type",
            4 => "kid",
            33 => "x5chain",
            _ => $"unknown_{label}"
        };
    }

    private static byte[] CreateSigStructure(byte[] protectedHeaders, byte[] externalAad, byte[] payload)
    {
        // Sig_structure = [
        //   context : "Signature1",
        //   body_protected : empty_or_serialized_map,
        //   external_aad : bstr,
        //   payload : bstr
        // ]
        var sigStructure = CBORObject.NewArray();
        sigStructure.Add(ContextString);
        sigStructure.Add(protectedHeaders);
        sigStructure.Add(externalAad);
        sigStructure.Add(payload);

        return sigStructure.EncodeToBytes();
    }
}
