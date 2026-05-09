using System.Security.Cryptography;
using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cose;
using SdJwt.Net.Mdoc.Handover;
using SdJwt.Net.Mdoc.Models;

namespace SdJwt.Net.Mdoc.Holder;

/// <summary>
/// Presents mdoc credentials with selective disclosure per ISO 18013-5 Section 8.3.2.1.2.2.
/// The holder selects which issuer-signed data elements to disclose and optionally
/// creates device authentication (DeviceSignature or DeviceMac).
/// </summary>
public class MdocPresenter
{
    private readonly ICoseCryptoProvider _cryptoProvider;

    /// <summary>
    /// Creates a new MdocPresenter with the default crypto provider.
    /// </summary>
    public MdocPresenter() : this(new DefaultCoseCryptoProvider())
    {
    }

    /// <summary>
    /// Creates a new MdocPresenter with a custom crypto provider.
    /// </summary>
    /// <param name="cryptoProvider">The crypto provider to use for device authentication.</param>
    public MdocPresenter(ICoseCryptoProvider cryptoProvider)
    {
        _cryptoProvider = cryptoProvider ?? throw new ArgumentNullException(nameof(cryptoProvider));
    }

    /// <summary>
    /// Creates a DeviceResponse by selecting elements from an issued document
    /// based on a DeviceRequest from the verifier.
    /// </summary>
    /// <param name="issuedDocument">The full issued document.</param>
    /// <param name="request">The device request specifying which elements to disclose.</param>
    /// <param name="deviceKey">Optional device private key for DeviceSignature.</param>
    /// <param name="algorithm">The signing algorithm for device authentication.</param>
    /// <param name="sessionTranscript">Session transcript for device authentication external AAD.</param>
    /// <returns>A DeviceResponse containing selectively disclosed elements.</returns>
    public async Task<DeviceResponse> PresentAsync(
        Document issuedDocument,
        DeviceRequest request,
        ECDsa? deviceKey = null,
        CoseAlgorithm algorithm = CoseAlgorithm.ES256,
        SessionTranscript? sessionTranscript = null)
    {
        if (issuedDocument == null)
            throw new ArgumentNullException(nameof(issuedDocument));
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var documents = new List<Document>();
        var documentErrors = new List<DocumentError>();

        foreach (var docRequest in request.DocRequests)
        {
            if (docRequest.ItemsRequest.DocType != issuedDocument.DocType)
            {
                documentErrors.Add(new DocumentError
                {
                    DocType = docRequest.ItemsRequest.DocType,
                    ErrorCode = 0 // no matching document
                });
                continue;
            }

            var document = CreateDisclosedDocument(
                issuedDocument,
                docRequest.ItemsRequest);

            // Add device authentication if device key is available
            if (deviceKey != null && sessionTranscript != null)
            {
                document.DeviceSigned = await CreateDeviceSignedAsync(
                    deviceKey,
                    algorithm,
                    sessionTranscript);
            }

            documents.Add(document);
        }

        return new DeviceResponse
        {
            Documents = documents,
            DocumentErrors = documentErrors
        };
    }

    /// <summary>
    /// Creates a DeviceResponse by manually selecting elements to disclose.
    /// </summary>
    /// <param name="issuedDocument">The full issued document.</param>
    /// <param name="disclosedElements">Elements to disclose: namespace to list of element identifiers.</param>
    /// <param name="deviceKey">Optional device private key for DeviceSignature.</param>
    /// <param name="algorithm">The signing algorithm for device authentication.</param>
    /// <param name="sessionTranscript">Session transcript for device authentication external AAD.</param>
    /// <returns>A DeviceResponse containing selectively disclosed elements.</returns>
    public async Task<DeviceResponse> PresentAsync(
        Document issuedDocument,
        Dictionary<string, List<string>> disclosedElements,
        ECDsa? deviceKey = null,
        CoseAlgorithm algorithm = CoseAlgorithm.ES256,
        SessionTranscript? sessionTranscript = null)
    {
        if (issuedDocument == null)
            throw new ArgumentNullException(nameof(issuedDocument));
        if (disclosedElements == null)
            throw new ArgumentNullException(nameof(disclosedElements));

        var document = new Document
        {
            DocType = issuedDocument.DocType,
            IssuerSigned = FilterIssuerSigned(issuedDocument.IssuerSigned, disclosedElements)
        };

        if (deviceKey != null && sessionTranscript != null)
        {
            document.DeviceSigned = await CreateDeviceSignedAsync(
                deviceKey,
                algorithm,
                sessionTranscript);
        }

        return new DeviceResponse
        {
            Documents = new List<Document> { document }
        };
    }

    private static Document CreateDisclosedDocument(
        Document issuedDocument,
        ItemsRequest itemsRequest)
    {
        var disclosedElements = new Dictionary<string, List<string>>();

        foreach (var (nameSpace, elements) in itemsRequest.NameSpaces)
        {
            disclosedElements[nameSpace] = elements.Keys.ToList();
        }

        return new Document
        {
            DocType = issuedDocument.DocType,
            IssuerSigned = FilterIssuerSigned(issuedDocument.IssuerSigned, disclosedElements)
        };
    }

    private static IssuerSigned FilterIssuerSigned(
        IssuerSigned fullIssuerSigned,
        Dictionary<string, List<string>> disclosedElements)
    {
        var filtered = new IssuerSigned
        {
            IssuerAuth = fullIssuerSigned.IssuerAuth
        };

        foreach (var (nameSpace, requestedElements) in disclosedElements)
        {
            if (!fullIssuerSigned.NameSpaces.TryGetValue(nameSpace, out var items))
            {
                continue;
            }

            var disclosedItems = items
                .Where(item => requestedElements.Contains(item.ElementIdentifier))
                .ToList();

            if (disclosedItems.Count > 0)
            {
                filtered.NameSpaces[nameSpace] = disclosedItems;
            }
        }

        return filtered;
    }

    private async Task<DeviceSigned> CreateDeviceSignedAsync(
        ECDsa deviceKey,
        CoseAlgorithm algorithm,
        SessionTranscript sessionTranscript)
    {
        // DeviceAuthentication = ["DeviceAuthentication", SessionTranscriptBytes, DocType, DeviceNameSpacesBytes]
        // For device signature, payload is empty and external_aad is SessionTranscript
        var coseKey = CoseKey.FromECDsa(deviceKey);

        var deviceSig = await CoseSign1.CreateAsync(
            Array.Empty<byte>(),
            coseKey,
            algorithm,
            _cryptoProvider,
            sessionTranscript.ToCbor());

        return new DeviceSigned
        {
            DeviceAuth = new DeviceAuth
            {
                DeviceSignature = deviceSig.ToCbor()
            }
        };
    }
}
