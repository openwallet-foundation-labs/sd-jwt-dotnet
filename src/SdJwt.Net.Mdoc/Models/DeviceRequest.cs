using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cbor;

namespace SdJwt.Net.Mdoc.Models;

/// <summary>
/// DeviceRequest per ISO 18013-5 Section 8.3.2.1.2.1.
/// Contains one or more document requests from a reader/verifier.
/// </summary>
public class DeviceRequest : ICborSerializable
{
    /// <summary>
    /// Version of the DeviceRequest structure.
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Document requests, one per document type.
    /// </summary>
    public List<DocRequest> DocRequests { get; set; } = new();

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

        var docRequestsArray = CBORObject.NewArray();
        foreach (var docRequest in DocRequests)
        {
            docRequestsArray.Add(docRequest.ToCborObject());
        }
        cbor.Add("docRequests", docRequestsArray);

        return cbor;
    }

    /// <summary>
    /// Creates a DeviceRequest from CBOR bytes.
    /// </summary>
    /// <param name="cbor">The CBOR bytes.</param>
    /// <returns>A new DeviceRequest instance.</returns>
    public static DeviceRequest FromCbor(byte[] cbor)
    {
        return FromCborObject(CBORObject.DecodeFromBytes(cbor));
    }

    /// <summary>
    /// Creates a DeviceRequest from a CBOR object.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new DeviceRequest instance.</returns>
    public static DeviceRequest FromCborObject(CBORObject cbor)
    {
        var request = new DeviceRequest();

        if (cbor.ContainsKey("version"))
        {
            request.Version = cbor["version"].AsString();
        }

        if (cbor.ContainsKey("docRequests"))
        {
            foreach (var docReqCbor in cbor["docRequests"].Values)
            {
                request.DocRequests.Add(DocRequest.FromCborObject(docReqCbor));
            }
        }

        return request;
    }
}

/// <summary>
/// DocRequest per ISO 18013-5 Section 8.3.2.1.2.1.
/// A single document type request containing items to be returned and optional reader authentication.
/// </summary>
public class DocRequest : ICborSerializable
{
    /// <summary>
    /// The items request (encoded as tag-24 wrapped CBOR).
    /// </summary>
    public ItemsRequest ItemsRequest { get; set; } = new();

    /// <summary>
    /// Optional reader authentication (COSE_Sign1 over ReaderAuthenticationBytes).
    /// </summary>
    public byte[]? ReaderAuth
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

        // ItemsRequest is tag-24 wrapped
        cbor.Add("itemsRequest", CBORObject.FromObjectAndTag(ItemsRequest.ToCbor(), 24));

        if (ReaderAuth != null)
        {
            cbor.Add("readerAuth", CBORObject.DecodeFromBytes(ReaderAuth));
        }

        return cbor;
    }

    /// <summary>
    /// Creates a DocRequest from a CBOR object.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new DocRequest instance.</returns>
    public static DocRequest FromCborObject(CBORObject cbor)
    {
        var docRequest = new DocRequest();

        if (cbor.ContainsKey("itemsRequest"))
        {
            var itemsReqCbor = cbor["itemsRequest"];
            if (itemsReqCbor.HasMostOuterTag(24))
            {
                itemsReqCbor = CBORObject.DecodeFromBytes(itemsReqCbor.GetByteString());
            }
            docRequest.ItemsRequest = ItemsRequest.FromCborObject(itemsReqCbor);
        }

        if (cbor.ContainsKey("readerAuth"))
        {
            docRequest.ReaderAuth = cbor["readerAuth"].EncodeToBytes();
        }

        return docRequest;
    }
}

/// <summary>
/// ItemsRequest per ISO 18013-5 Section 8.3.2.1.2.1.
/// Specifies which data elements the reader is requesting.
/// </summary>
public class ItemsRequest : ICborSerializable
{
    /// <summary>
    /// Document type being requested (e.g., "org.iso.18013.5.1.mDL").
    /// </summary>
    public string DocType { get; set; } = string.Empty;

    /// <summary>
    /// Requested namespaces and data elements.
    /// Key: namespace, Value: dictionary of element identifier to intent-to-retain flag.
    /// </summary>
    public Dictionary<string, Dictionary<string, bool>> NameSpaces { get; set; } = new();

    /// <summary>
    /// Optional request info (additional request metadata).
    /// </summary>
    public Dictionary<string, object>? RequestInfo
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
        cbor.Add("docType", DocType);

        var nameSpacesCbor = CBORObject.NewMap();
        foreach (var (nameSpace, elements) in NameSpaces)
        {
            var elementsCbor = CBORObject.NewMap();
            foreach (var (elementId, intentToRetain) in elements)
            {
                elementsCbor.Add(elementId, intentToRetain);
            }
            nameSpacesCbor.Add(nameSpace, elementsCbor);
        }
        cbor.Add("nameSpaces", nameSpacesCbor);

        if (RequestInfo != null)
        {
            var requestInfoCbor = CBORObject.NewMap();
            foreach (var (key, value) in RequestInfo)
            {
                requestInfoCbor.Add(key, CBORObject.FromObject(value));
            }
            cbor.Add("requestInfo", requestInfoCbor);
        }

        return cbor;
    }

    /// <summary>
    /// Creates an ItemsRequest from a CBOR object.
    /// </summary>
    /// <param name="cbor">The CBOR object.</param>
    /// <returns>A new ItemsRequest instance.</returns>
    public static ItemsRequest FromCborObject(CBORObject cbor)
    {
        var request = new ItemsRequest();

        if (cbor.ContainsKey("docType"))
        {
            request.DocType = cbor["docType"].AsString();
        }

        if (cbor.ContainsKey("nameSpaces"))
        {
            var nameSpacesCbor = cbor["nameSpaces"];
            foreach (var nsKey in nameSpacesCbor.Keys)
            {
                var elements = new Dictionary<string, bool>();
                var elementsCbor = nameSpacesCbor[nsKey];
                foreach (var elemKey in elementsCbor.Keys)
                {
                    elements[elemKey.AsString()] = elementsCbor[elemKey].AsBoolean();
                }
                request.NameSpaces[nsKey.AsString()] = elements;
            }
        }

        return request;
    }
}
