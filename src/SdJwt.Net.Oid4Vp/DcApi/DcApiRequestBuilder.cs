using SdJwt.Net.Oid4Vp.DcApi.Models;
using SdJwt.Net.Oid4Vp.Models;

namespace SdJwt.Net.Oid4Vp.DcApi;

/// <summary>
/// Builder for creating DC API compatible OpenID4VP requests.
/// </summary>
public class DcApiRequestBuilder
{
    private DcApiRequestType _requestType = DcApiRequestType.Unsigned;
    private string _clientId = string.Empty;
    private string _clientIdScheme = DcApiConstants.WebOriginScheme;
    private string _nonce = string.Empty;
    private string _responseType = "vp_token";
    private DcApiResponseMode _responseMode = DcApiResponseMode.DcApi;
    private string[]? _expectedOrigins;
    private PresentationDefinition? _presentationDefinition;

    /// <summary>
    /// Configures the builder to create an unsigned OpenID4VP DC API request.
    /// </summary>
    /// <returns>This builder for chaining.</returns>
    public DcApiRequestBuilder AsUnsignedRequest()
    {
        _requestType = DcApiRequestType.Unsigned;
        return this;
    }

    /// <summary>
    /// Configures the builder to create a signed OpenID4VP DC API request.
    /// </summary>
    /// <returns>This builder for chaining.</returns>
    public DcApiRequestBuilder AsSignedRequest()
    {
        _requestType = DcApiRequestType.Signed;
        return this;
    }

    /// <summary>
    /// Sets the client identifier (typically the verifier's origin URL).
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <returns>This builder for chaining.</returns>
    public DcApiRequestBuilder WithClientId(string clientId)
    {
        _clientId = clientId;
        return this;
    }

    /// <summary>
    /// Sets the client ID scheme. Defaults to "web-origin".
    /// </summary>
    /// <param name="scheme">The client ID scheme.</param>
    /// <returns>This builder for chaining.</returns>
    public DcApiRequestBuilder WithClientIdScheme(string scheme)
    {
        _clientIdScheme = scheme;
        return this;
    }

    /// <summary>
    /// Sets the expected verifier origins for signed DC API requests.
    /// </summary>
    /// <param name="origins">Expected verifier origins.</param>
    /// <returns>This builder for chaining.</returns>
    public DcApiRequestBuilder WithExpectedOrigins(params string[] origins)
    {
        _expectedOrigins = origins;
        return this;
    }

    /// <summary>
    /// Sets the nonce for replay protection.
    /// </summary>
    /// <param name="nonce">The nonce value.</param>
    /// <returns>This builder for chaining.</returns>
    public DcApiRequestBuilder WithNonce(string nonce)
    {
        _nonce = nonce;
        return this;
    }

    /// <summary>
    /// Sets the response type. Defaults to "vp_token".
    /// </summary>
    /// <param name="responseType">The response type.</param>
    /// <returns>This builder for chaining.</returns>
    public DcApiRequestBuilder WithResponseType(string responseType)
    {
        _responseType = responseType;
        return this;
    }

    /// <summary>
    /// Sets the presentation definition describing required credentials.
    /// </summary>
    /// <param name="definition">The presentation definition.</param>
    /// <returns>This builder for chaining.</returns>
    public DcApiRequestBuilder WithPresentationDefinition(PresentationDefinition definition)
    {
        _presentationDefinition = definition;
        return this;
    }

    /// <summary>
    /// Sets the response mode. Use DcApiJwt for encrypted responses.
    /// </summary>
    /// <param name="mode">The response mode.</param>
    /// <returns>This builder for chaining.</returns>
    public DcApiRequestBuilder WithResponseMode(DcApiResponseMode mode)
    {
        _responseMode = mode;
        return this;
    }

    /// <summary>
    /// Builds the DC API request.
    /// </summary>
    /// <returns>A configured DC API request.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required parameters are missing or invalid.
    /// </exception>
    public DcApiRequest Build()
    {
        ValidateRequiredParameters();

        return new DcApiRequest
        {
            Digital = new DigitalCredentialRequestOptions
            {
                Requests = new[]
                {
                    new DigitalCredentialGetRequest
                    {
                        Protocol = MapProtocol(_requestType),
                        Data = new DcApiAuthorizationRequest
                        {
                            ClientId = _requestType == DcApiRequestType.Unsigned ? null : _clientId,
                            ClientIdScheme = _requestType == DcApiRequestType.Unsigned ? null : _clientIdScheme,
                            ExpectedOrigins = _requestType == DcApiRequestType.Signed ? _expectedOrigins : null,
                            ResponseType = _responseType,
                            ResponseMode = MapResponseMode(_responseMode),
                            Nonce = _nonce,
                            PresentationDefinition = _presentationDefinition
                        }
                    }
                }
            }
        };
    }

    private void ValidateRequiredParameters()
    {
        if (_requestType == DcApiRequestType.Signed && string.IsNullOrWhiteSpace(_clientId))
        {
            throw new InvalidOperationException(
                "ClientId is required. Use WithClientId() to set it.");
        }

        if (_requestType == DcApiRequestType.Signed &&
            (_expectedOrigins == null || _expectedOrigins.Length == 0 || _expectedOrigins.Any(string.IsNullOrWhiteSpace)))
        {
            throw new InvalidOperationException(
                "ExpectedOrigins is required for signed DC API requests. Use WithExpectedOrigins() to set it.");
        }

        if (string.IsNullOrWhiteSpace(_nonce))
        {
            throw new InvalidOperationException(
                "Nonce is required. Use WithNonce() to set it.");
        }

        if (_presentationDefinition is null)
        {
            throw new InvalidOperationException(
                "PresentationDefinition is required. Use WithPresentationDefinition() to set it.");
        }
    }

    private static string MapResponseMode(DcApiResponseMode mode)
    {
        return mode switch
        {
            DcApiResponseMode.DcApi => DcApiConstants.ResponseModes.DcApi,
            DcApiResponseMode.DcApiJwt => DcApiConstants.ResponseModes.DcApiJwt,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown response mode")
        };
    }

    private static string MapProtocol(DcApiRequestType requestType)
    {
        return requestType switch
        {
            DcApiRequestType.Unsigned => DcApiConstants.Protocols.OpenId4VpV1Unsigned,
            DcApiRequestType.Signed => DcApiConstants.Protocols.OpenId4VpV1Signed,
            DcApiRequestType.Multisigned => DcApiConstants.Protocols.OpenId4VpV1Multisigned,
            _ => throw new ArgumentOutOfRangeException(nameof(requestType), requestType, "Unknown request type")
        };
    }
}

/// <summary>
/// OpenID4VP request types supported by the Digital Credentials API.
/// </summary>
public enum DcApiRequestType
{
    /// <summary>
    /// Unsigned OpenID4VP request.
    /// </summary>
    Unsigned,

    /// <summary>
    /// Signed OpenID4VP request.
    /// </summary>
    Signed,

    /// <summary>
    /// Multisigned OpenID4VP request.
    /// </summary>
    Multisigned
}
