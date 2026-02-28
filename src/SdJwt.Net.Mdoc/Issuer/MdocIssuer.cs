using System.Security.Cryptography;
using PeterO.Cbor;
using SdJwt.Net.Mdoc.Cose;
using SdJwt.Net.Mdoc.Models;

namespace SdJwt.Net.Mdoc.Issuer;

/// <summary>
/// Issues mdoc credentials per ISO 18013-5.
/// </summary>
public class MdocIssuer
{
    private readonly ECDsa _issuerKey;
    private readonly CoseAlgorithm _algorithm;
    private readonly MdocIssuerOptions _options;
    private readonly CoseKey? _deviceKey;
    private readonly Dictionary<string, Dictionary<string, object>> _claims;
    private readonly byte[]? _issuerCertificate;
    private readonly ICoseCryptoProvider _cryptoProvider;

    internal MdocIssuer(
        ECDsa issuerKey,
        CoseAlgorithm algorithm,
        MdocIssuerOptions options,
        CoseKey? deviceKey,
        Dictionary<string, Dictionary<string, object>> claims,
        byte[]? issuerCertificate,
        ICoseCryptoProvider cryptoProvider)
    {
        _issuerKey = issuerKey;
        _algorithm = algorithm;
        _options = options;
        _deviceKey = deviceKey;
        _claims = claims;
        _issuerCertificate = issuerCertificate;
        _cryptoProvider = cryptoProvider;
    }

    /// <summary>
    /// Creates a new MdocIssuerBuilder.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public static MdocIssuerBuilder CreateBuilder()
    {
        return new MdocIssuerBuilder();
    }

    /// <summary>
    /// Issues the mdoc credential.
    /// </summary>
    /// <returns>The issued IssuerSigned structure.</returns>
    public IssuerSigned Issue()
    {
        var issuerSigned = new IssuerSigned();
        var valueDigestsByNamespace = new Dictionary<string, DigestIdMapping>();

        // Generate IssuerSignedItems for each namespace
        foreach (var (nameSpace, elements) in _claims)
        {
            var items = new List<IssuerSignedItem>();
            var digests = new DigestIdMapping();
            var digestId = 0;

            foreach (var (elementId, value) in elements)
            {
                var item = new IssuerSignedItem
                {
                    DigestId = digestId,
                    Random = GenerateRandom(),
                    ElementIdentifier = elementId,
                    ElementValue = value
                };
                items.Add(item);

                // Compute digest of the tagged CBOR
                var itemBytes = CBORObject.FromObjectAndTag(item.ToCbor(), 24).EncodeToBytes();
                var digest = ComputeSha256(itemBytes);
                digests.Digests[digestId] = digest;

                digestId++;
            }

            issuerSigned.NameSpaces[nameSpace] = items;
            valueDigestsByNamespace[nameSpace] = digests;
        }

        // Build MSO
        var mso = new MobileSecurityObject
        {
            Version = "1.0",
            DigestAlgorithm = "SHA-256",
            DocType = _options.DocType,
            ValueDigests = valueDigestsByNamespace,
            ValidityInfo = new ValidityInfo
            {
                Signed = _options.ValidFrom,
                ValidFrom = _options.ValidFrom,
                ValidUntil = _options.ValidUntil,
                ExpectedUpdate = _options.ExpectedUpdate
            }
        };

        // Add device key if provided
        if (_deviceKey != null)
        {
            mso.DeviceKeyInfo = new DeviceKeyInfo
            {
                DeviceKey = _deviceKey.ToCbor()
            };
        }

        // Sign MSO as COSE_Sign1
        var msoBytes = mso.ToCbor();
        var coseSign1 = new CoseSign1(_algorithm, msoBytes);

        // Add x5chain if certificate provided
        if (_issuerCertificate != null)
        {
            coseSign1.UnprotectedHeaders["x5chain"] = _issuerCertificate;
        }

        issuerSigned.IssuerAuth = _cryptoProvider.Sign(coseSign1, _issuerKey);

        return issuerSigned;
    }

    /// <summary>
    /// Issues the mdoc credential asynchronously.
    /// </summary>
    /// <returns>The issued Document.</returns>
    public Task<Document> IssueAsync()
    {
        var issuerSigned = Issue();
        var document = new Document
        {
            DocType = _options.DocType,
            IssuerSigned = issuerSigned
        };
        return Task.FromResult(document);
    }

    private static byte[] GenerateRandom()
    {
        var random = new byte[16];
        RandomNumberGenerator.Fill(random);
        return random;
    }

    private static byte[] ComputeSha256(byte[] data)
    {
#if NETSTANDARD2_1
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(data);
#else
        return SHA256.HashData(data);
#endif
    }
}
