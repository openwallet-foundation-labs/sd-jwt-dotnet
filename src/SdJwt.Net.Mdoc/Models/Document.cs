using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cbor;

namespace SdJwt.Net.Mdoc.Models;

/// <summary>
/// DeviceSigned structure with device authentication.
/// </summary>
public class DeviceSigned : ICborSerializable
{
    /// <summary>
    /// Namespaced data elements signed by the device.
    /// </summary>
    public Dictionary<string, List<IssuerSignedItem>> NameSpaces { get; set; } = new();

    /// <summary>
    /// Device authentication (deviceSignature or deviceMac).
    /// </summary>
    public DeviceAuth? DeviceAuth { get; set; }

    /// <inheritdoc/>
    public byte[] ToCbor()
    {
        return ToCborObject().EncodeToBytes();
    }

    /// <inheritdoc/>
    public CBORObject ToCborObject()
    {
        var cbor = CBORObject.NewMap();

        var nameSpacesCbor = CBORObject.NewMap();
        foreach (var (nameSpace, items) in NameSpaces)
        {
            var itemsArray = CBORObject.NewArray();
            foreach (var item in items)
            {
                itemsArray.Add(CBORObject.FromObjectAndTag(item.ToCbor(), 24));
            }
            nameSpacesCbor.Add(nameSpace, itemsArray);
        }
        cbor.Add("nameSpaces", CBORObject.FromObjectAndTag(nameSpacesCbor.EncodeToBytes(), 24));

        if (DeviceAuth != null)
        {
            cbor.Add("deviceAuth", DeviceAuth.ToCborObject());
        }

        return cbor;
    }

    /// <summary>
    /// Creates a DeviceSigned from a CBOR object.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new DeviceSigned instance.</returns>
    public static DeviceSigned FromCborObject(CBORObject cbor)
    {
        var deviceSigned = new DeviceSigned();

        if (cbor.ContainsKey("nameSpaces"))
        {
            var nameSpacesCbor = cbor["nameSpaces"];
            if (nameSpacesCbor.HasMostOuterTag(24))
            {
                nameSpacesCbor = CBORObject.DecodeFromBytes(nameSpacesCbor.GetByteString());
            }

            foreach (var key in nameSpacesCbor.Keys)
            {
                var items = new List<IssuerSignedItem>();
                foreach (var itemCbor in nameSpacesCbor[key].Values)
                {
                    var actualCbor = itemCbor.HasMostOuterTag(24)
                        ? CBORObject.DecodeFromBytes(itemCbor.GetByteString())
                        : itemCbor;
                    items.Add(IssuerSignedItem.FromCborObject(actualCbor));
                }
                deviceSigned.NameSpaces[key.AsString()] = items;
            }
        }

        if (cbor.ContainsKey("deviceAuth"))
        {
            deviceSigned.DeviceAuth = DeviceAuth.FromCborObject(cbor["deviceAuth"]);
        }

        return deviceSigned;
    }
}

/// <summary>
/// Device authentication (deviceSignature or deviceMac).
/// </summary>
public class DeviceAuth : ICborSerializable
{
    /// <summary>
    /// Device signature (COSE_Sign1).
    /// </summary>
    public byte[]? DeviceSignature { get; set; }

    /// <summary>
    /// Device MAC (COSE_Mac0).
    /// </summary>
    public byte[]? DeviceMac { get; set; }

    /// <inheritdoc/>
    public byte[] ToCbor()
    {
        return ToCborObject().EncodeToBytes();
    }

    /// <inheritdoc/>
    public CBORObject ToCborObject()
    {
        var cbor = CBORObject.NewMap();

        if (DeviceSignature != null)
        {
            cbor.Add("deviceSignature", CBORObject.DecodeFromBytes(DeviceSignature));
        }
        else if (DeviceMac != null)
        {
            cbor.Add("deviceMac", CBORObject.DecodeFromBytes(DeviceMac));
        }

        return cbor;
    }

    /// <summary>
    /// Creates a DeviceAuth from a CBOR object.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new DeviceAuth instance.</returns>
    public static DeviceAuth FromCborObject(CBORObject cbor)
    {
        var auth = new DeviceAuth();

        if (cbor.ContainsKey("deviceSignature"))
        {
            auth.DeviceSignature = cbor["deviceSignature"].EncodeToBytes();
        }
        else if (cbor.ContainsKey("deviceMac"))
        {
            auth.DeviceMac = cbor["deviceMac"].EncodeToBytes();
        }

        return auth;
    }
}

/// <summary>
/// Document error information.
/// </summary>
public class DocumentError
{
    /// <summary>
    /// Document type that caused the error.
    /// </summary>
    public string DocType { get; set; } = string.Empty;

    /// <summary>
    /// Error code.
    /// </summary>
    public int ErrorCode { get; set; }
}

/// <summary>
/// Individual document within a DeviceResponse.
/// </summary>
public class Document : ICborSerializable
{
    /// <summary>
    /// Document type (e.g., "org.iso.18013.5.1.mDL").
    /// </summary>
    public string DocType { get; set; } = string.Empty;

    /// <summary>
    /// Issuer-signed data including namespaces and MSO.
    /// </summary>
    public IssuerSigned IssuerSigned { get; set; } = new();

    /// <summary>
    /// Device-signed data proving possession.
    /// </summary>
    public DeviceSigned? DeviceSigned { get; set; }

    /// <summary>
    /// Errors for specific data elements that could not be returned.
    /// </summary>
    public Dictionary<string, Dictionary<string, int>>? Errors { get; set; }

    /// <inheritdoc/>
    public byte[] ToCbor()
    {
        return ToCborObject().EncodeToBytes();
    }

    /// <inheritdoc/>
    public CBORObject ToCborObject()
    {
        var cbor = CBORObject.NewMap();
        cbor.Add("docType", DocType);
        cbor.Add("issuerSigned", IssuerSigned.ToCborObject());

        if (DeviceSigned != null)
        {
            cbor.Add("deviceSigned", DeviceSigned.ToCborObject());
        }

        if (Errors != null && Errors.Count > 0)
        {
            var errorsCbor = CBORObject.NewMap();
            foreach (var (ns, nsErrors) in Errors)
            {
                var nsErrorsCbor = CBORObject.NewMap();
                foreach (var (elem, code) in nsErrors)
                {
                    nsErrorsCbor.Add(elem, code);
                }
                errorsCbor.Add(ns, nsErrorsCbor);
            }
            cbor.Add("errors", errorsCbor);
        }

        return cbor;
    }

    /// <summary>
    /// Creates a Document from a CBOR object.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new Document instance.</returns>
    public static Document FromCborObject(CBORObject cbor)
    {
        var doc = new Document
        {
            DocType = cbor["docType"].AsString(),
            IssuerSigned = IssuerSigned.FromCborObject(cbor["issuerSigned"])
        };

        if (cbor.ContainsKey("deviceSigned"))
        {
            doc.DeviceSigned = DeviceSigned.FromCborObject(cbor["deviceSigned"]);
        }

        return doc;
    }
}
