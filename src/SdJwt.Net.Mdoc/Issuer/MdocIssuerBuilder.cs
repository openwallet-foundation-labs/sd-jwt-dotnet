using System.Security.Cryptography;
using SdJwt.Net.Mdoc.Cose;
using SdJwt.Net.Mdoc.Models;
using SdJwt.Net.Mdoc.Namespaces;

namespace SdJwt.Net.Mdoc.Issuer;

/// <summary>
/// Fluent builder for creating mdoc credentials.
/// </summary>
public class MdocIssuerBuilder
{
    private readonly MdocIssuerOptions _options = new();
    private ECDsa? _issuerKey;
    private CoseAlgorithm _algorithm = CoseAlgorithm.ES256;
    private CoseKey? _deviceKey;
    private readonly Dictionary<string, Dictionary<string, object>> _claims = new();
    private byte[]? _issuerCertificate;
    private ICoseCryptoProvider? _cryptoProvider;

    /// <summary>
    /// Sets the document type.
    /// </summary>
    /// <param name="docType">The document type identifier.</param>
    /// <returns>The builder for chaining.</returns>
    public MdocIssuerBuilder WithDocType(string docType)
    {
        _options.DocType = docType;
        return this;
    }

    /// <summary>
    /// Sets the issuer signing key.
    /// </summary>
    /// <param name="key">The ECDsa private key.</param>
    /// <returns>The builder for chaining.</returns>
    public MdocIssuerBuilder WithIssuerKey(ECDsa key)
    {
        _issuerKey = key;
        return this;
    }

    /// <summary>
    /// Sets the issuer signing key from a COSE key.
    /// </summary>
    /// <param name="key">The COSE key containing the private key.</param>
    /// <returns>The builder for chaining.</returns>
    public MdocIssuerBuilder WithIssuerKey(CoseKey key)
    {
        _issuerKey = key.ToECDsa();
        return this;
    }

    /// <summary>
    /// Sets the signing algorithm.
    /// </summary>
    /// <param name="algorithm">The COSE algorithm.</param>
    /// <returns>The builder for chaining.</returns>
    public MdocIssuerBuilder WithAlgorithm(CoseAlgorithm algorithm)
    {
        _algorithm = algorithm;
        return this;
    }

    /// <summary>
    /// Sets the device key for holder binding.
    /// </summary>
    /// <param name="deviceKey">The device's public key.</param>
    /// <returns>The builder for chaining.</returns>
    public MdocIssuerBuilder WithDeviceKey(CoseKey deviceKey)
    {
        _deviceKey = deviceKey;
        return this;
    }

    /// <summary>
    /// Sets the issuer certificate for x5chain header.
    /// </summary>
    /// <param name="certificate">The issuer's X.509 certificate bytes.</param>
    /// <returns>The builder for chaining.</returns>
    public MdocIssuerBuilder WithIssuerCertificate(byte[] certificate)
    {
        _issuerCertificate = certificate;
        return this;
    }

    /// <summary>
    /// Sets the validity period.
    /// </summary>
    /// <param name="validFrom">Start of validity period.</param>
    /// <param name="validUntil">End of validity period.</param>
    /// <returns>The builder for chaining.</returns>
    public MdocIssuerBuilder WithValidity(DateTimeOffset validFrom, DateTimeOffset validUntil)
    {
        _options.ValidFrom = validFrom;
        _options.ValidUntil = validUntil;
        return this;
    }

    /// <summary>
    /// Sets the expected update timestamp.
    /// </summary>
    /// <param name="expectedUpdate">Expected update timestamp.</param>
    /// <returns>The builder for chaining.</returns>
    public MdocIssuerBuilder WithExpectedUpdate(DateTimeOffset expectedUpdate)
    {
        _options.ExpectedUpdate = expectedUpdate;
        return this;
    }

    /// <summary>
    /// Adds a claim to a namespace.
    /// </summary>
    /// <param name="nameSpace">The namespace.</param>
    /// <param name="elementIdentifier">The element identifier.</param>
    /// <param name="value">The claim value.</param>
    /// <returns>The builder for chaining.</returns>
    public MdocIssuerBuilder AddClaim(string nameSpace, string elementIdentifier, object value)
    {
        if (!_claims.ContainsKey(nameSpace))
        {
            _claims[nameSpace] = new Dictionary<string, object>();
        }
        _claims[nameSpace][elementIdentifier] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple claims to a namespace.
    /// </summary>
    /// <param name="nameSpace">The namespace.</param>
    /// <param name="claims">Dictionary of element identifiers to values.</param>
    /// <returns>The builder for chaining.</returns>
    public MdocIssuerBuilder AddClaims(string nameSpace, Dictionary<string, object> claims)
    {
        foreach (var (key, value) in claims)
        {
            AddClaim(nameSpace, key, value);
        }
        return this;
    }

    /// <summary>
    /// Adds an mDL data element to the mDL namespace.
    /// </summary>
    /// <param name="element">The mDL data element.</param>
    /// <param name="value">The claim value.</param>
    /// <returns>The builder for chaining.</returns>
    public MdocIssuerBuilder AddMdlElement(MdlDataElement element, object value)
    {
        var elementName = element.ToElementIdentifier();
        return AddClaim(MdlNamespace.Namespace, elementName, value);
    }

    /// <summary>
    /// Sets a custom crypto provider.
    /// </summary>
    /// <param name="provider">The crypto provider.</param>
    /// <returns>The builder for chaining.</returns>
    public MdocIssuerBuilder WithCryptoProvider(ICoseCryptoProvider provider)
    {
        _cryptoProvider = provider;
        return this;
    }

    /// <summary>
    /// Builds the MdocIssuer instance.
    /// </summary>
    /// <returns>A configured MdocIssuer.</returns>
    public MdocIssuer Build()
    {
        if (_issuerKey == null)
        {
            throw new InvalidOperationException("Issuer key is required.");
        }

        if (string.IsNullOrEmpty(_options.DocType))
        {
            throw new InvalidOperationException("DocType is required.");
        }

        return new MdocIssuer(
            _issuerKey,
            _algorithm,
            _options,
            _deviceKey,
            _claims,
            _issuerCertificate,
            _cryptoProvider ?? new DefaultCoseCryptoProvider());
    }

    /// <summary>
    /// Builds and issues an mdoc asynchronously.
    /// </summary>
    /// <param name="cryptoProvider">The crypto provider to use.</param>
    /// <returns>The issued document.</returns>
    public Task<Document> BuildAsync(ICoseCryptoProvider cryptoProvider)
    {
        _cryptoProvider = cryptoProvider;
        var issuer = Build();
        return issuer.IssueAsync();
    }
}
