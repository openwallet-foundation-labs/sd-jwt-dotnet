using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cbor;

namespace SdJwt.Net.Mdoc.Models;

/// <summary>
/// Digest ID to element mapping.
/// </summary>
public class DigestIdMapping : ICborSerializable
{
    /// <summary>
    /// Mapping of digest ID to digest value.
    /// </summary>
    public Dictionary<int, byte[]> Digests { get; set; } = new();

    /// <inheritdoc/>
    public byte[] ToCbor()
    {
        return ToCborObject().EncodeToBytes();
    }

    /// <inheritdoc/>
    public CBORObject ToCborObject()
    {
        var cbor = CBORObject.NewMap();
        foreach (var (digestId, digest) in Digests)
        {
            cbor.Add(digestId, digest);
        }
        return cbor;
    }

    /// <summary>
    /// Creates a DigestIdMapping from CBOR.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new DigestIdMapping instance.</returns>
    public static DigestIdMapping FromCborObject(CBORObject cbor)
    {
        var mapping = new DigestIdMapping();
        foreach (var key in cbor.Keys)
        {
            mapping.Digests[key.AsInt32()] = cbor[key].GetByteString();
        }
        return mapping;
    }
}

/// <summary>
/// Device key information for holder binding.
/// </summary>
public class DeviceKeyInfo : ICborSerializable
{
    /// <summary>
    /// The device public key in COSE_Key format.
    /// </summary>
    public byte[] DeviceKey { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Optional key authorizations.
    /// </summary>
    public Dictionary<string, object>? KeyAuthorizations
    {
        get; set;
    }

    /// <summary>
    /// Optional key info.
    /// </summary>
    public Dictionary<string, object>? KeyInfo
    {
        get; set;
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
        cbor.Add("deviceKey", CBORObject.DecodeFromBytes(DeviceKey));

        if (KeyAuthorizations != null)
        {
            var authCbor = CBORObject.NewMap();
            foreach (var (key, value) in KeyAuthorizations)
            {
                authCbor.Add(key, CBORObject.FromObject(value));
            }
            cbor.Add("keyAuthorizations", authCbor);
        }

        return cbor;
    }

    /// <summary>
    /// Creates a DeviceKeyInfo from CBOR.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new DeviceKeyInfo instance.</returns>
    public static DeviceKeyInfo FromCborObject(CBORObject cbor)
    {
        var info = new DeviceKeyInfo
        {
            DeviceKey = cbor["deviceKey"].EncodeToBytes()
        };

        if (cbor.ContainsKey("keyAuthorizations"))
        {
            info.KeyAuthorizations = new Dictionary<string, object>();
            var auth = cbor["keyAuthorizations"];
            foreach (var key in auth.Keys)
            {
                info.KeyAuthorizations[key.AsString()] = auth[key];
            }
        }

        return info;
    }
}
