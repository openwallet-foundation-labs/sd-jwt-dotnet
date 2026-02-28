using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cbor;

namespace SdJwt.Net.Mdoc.Models;

/// <summary>
/// Mobile Security Object as defined in ISO 18013-5 Section 9.1.2.4.
/// Contains the issuer-signed metadata and digest values for verifying IssuerSigned items.
/// </summary>
public class MobileSecurityObject : ICborSerializable
{
    /// <summary>
    /// Version of the MSO structure. Must be "1.0" per ISO 18013-5.
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Digest algorithm used. Must be "SHA-256" or "SHA-384" or "SHA-512".
    /// </summary>
    public string DigestAlgorithm { get; set; } = "SHA-256";

    /// <summary>
    /// Mapping of digest values per namespace.
    /// Key: nameSpace, Value: DigestIdMapping
    /// </summary>
    public Dictionary<string, DigestIdMapping> ValueDigests { get; set; } = new();

    /// <summary>
    /// Device key information for holder binding.
    /// </summary>
    public DeviceKeyInfo DeviceKeyInfo { get; set; } = new();

    /// <summary>
    /// Document type (e.g., "org.iso.18013.5.1.mDL").
    /// </summary>
    public string DocType { get; set; } = string.Empty;

    /// <summary>
    /// Validity information for the credential.
    /// </summary>
    public ValidityInfo ValidityInfo { get; set; } = new();

    /// <inheritdoc/>
    public byte[] ToCbor()
    {
        return ToCborObject().EncodeToBytes();
    }

    /// <inheritdoc/>
    public CBORObject ToCborObject()
    {
        var cbor = CBORObject.NewMap();
        cbor.Add("version", Version);
        cbor.Add("digestAlgorithm", DigestAlgorithm);
        cbor.Add("docType", DocType);

        var valueDigestsCbor = CBORObject.NewMap();
        foreach (var (nameSpace, digests) in ValueDigests)
        {
            valueDigestsCbor.Add(nameSpace, digests.ToCborObject());
        }
        cbor.Add("valueDigests", valueDigestsCbor);

        // Only include deviceKeyInfo if we have a device key
        if (DeviceKeyInfo.DeviceKey.Length > 0)
        {
            cbor.Add("deviceKeyInfo", DeviceKeyInfo.ToCborObject());
        }
        cbor.Add("validityInfo", ValidityInfo.ToCborObject());

        return cbor;
    }

    /// <summary>
    /// Creates a MobileSecurityObject from CBOR bytes.
    /// </summary>
    /// <param name="cborData">The CBOR-encoded data.</param>
    /// <returns>A new MobileSecurityObject instance.</returns>
    public static MobileSecurityObject FromCbor(byte[] cborData)
    {
        if (cborData == null) throw new ArgumentNullException(nameof(cborData));
        var cbor = CBORObject.DecodeFromBytes(cborData);
        return FromCborObject(cbor);
    }

    /// <summary>
    /// Creates a MobileSecurityObject from a CBOR object.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new MobileSecurityObject instance.</returns>
    public static MobileSecurityObject FromCborObject(CBORObject cbor)
    {
        var mso = new MobileSecurityObject
        {
            Version = cbor["version"].AsString(),
            DigestAlgorithm = cbor["digestAlgorithm"].AsString(),
            DocType = cbor["docType"].AsString()
        };

        var valueDigestsCbor = cbor["valueDigests"];
        foreach (var key in valueDigestsCbor.Keys)
        {
            mso.ValueDigests[key.AsString()] = DigestIdMapping.FromCborObject(valueDigestsCbor[key]);
        }

        if (cbor.ContainsKey("deviceKeyInfo"))
        {
            mso.DeviceKeyInfo = DeviceKeyInfo.FromCborObject(cbor["deviceKeyInfo"]);
        }

        if (cbor.ContainsKey("validityInfo"))
        {
            mso.ValidityInfo = ValidityInfo.FromCborObject(cbor["validityInfo"]);
        }

        return mso;
    }
}
