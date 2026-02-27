using SdJwt.Net.Oid4Vci.Models;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

#if NETSTANDARD2_1
using System.Web;
#else
using System.Web;
#endif

namespace SdJwt.Net.Oid4Vci.Client;

/// <summary>
/// Exception thrown when credential offer parsing fails.
/// </summary>
public class CredentialOfferParseException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CredentialOfferParseException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public CredentialOfferParseException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CredentialOfferParseException"/> class with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CredentialOfferParseException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Static utility class for parsing credential offers from QR codes and URIs.
/// </summary>
public static class CredentialOfferParser
{
    private const int MaxCredentialOfferBytes = 256 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
#if NET6_0_OR_GREATER
        ,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
#endif
    };

    /// <summary>
    /// Parses a credential offer from a URI string.
    /// </summary>
    /// <param name="uri">The credential offer URI (e.g., from QR code)</param>
    /// <returns>The parsed credential offer</returns>
    /// <exception cref="CredentialOfferParseException">Thrown when parsing fails</exception>
    public static CredentialOffer Parse(string uri)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(uri, nameof(uri));
#else
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(uri));
#endif

        try
        {
            // Check if it starts with the correct scheme
            if (!uri.StartsWith($"{Oid4VciConstants.CredentialOfferScheme}://", StringComparison.OrdinalIgnoreCase))
            {
                throw new CredentialOfferParseException($"URI must start with '{Oid4VciConstants.CredentialOfferScheme}://'");
            }

            // Parse the URI
            var parsedUri = new Uri(uri);

#if NETSTANDARD2_1
            var query = parsedUri.Query.TrimStart('?');
            var queryParams = HttpUtility.ParseQueryString(query);
#else
            var queryParams = HttpUtility.ParseQueryString(parsedUri.Query);
#endif

            // Check for direct credential offer
            var credentialOfferParam = queryParams["credential_offer"];
            if (!string.IsNullOrEmpty(credentialOfferParam))
            {
                return ParseCredentialOfferJson(credentialOfferParam);
            }

            // Check for credential offer URI
            var credentialOfferUri = queryParams["credential_offer_uri"];
            if (!string.IsNullOrEmpty(credentialOfferUri))
            {
                throw new CredentialOfferParseException(
                    "credential_offer_uri requires asynchronous retrieval. Use ParseAsync(uri, httpClient, cancellationToken).");
            }

            throw new CredentialOfferParseException("URI must contain either 'credential_offer' or 'credential_offer_uri' parameter");
        }
        catch (CredentialOfferParseException)
        {
            throw;
        }
        catch (NotSupportedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CredentialOfferParseException("Failed to parse credential offer URI", ex);
        }
    }

    /// <summary>
    /// Parses a credential offer from a URI string, including asynchronous retrieval from <c>credential_offer_uri</c>.
    /// </summary>
    /// <param name="uri">The credential offer URI (e.g., from QR code).</param>
    /// <param name="httpClient">HTTP client used to retrieve remote credential offers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parsed credential offer.</returns>
    /// <exception cref="CredentialOfferParseException">Thrown when parsing fails.</exception>
    public static async Task<CredentialOffer> ParseAsync(
        string uri,
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(uri, nameof(uri));
        ArgumentNullException.ThrowIfNull(httpClient);
#else
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(uri));
        if (httpClient == null)
            throw new ArgumentNullException(nameof(httpClient));
#endif

        try
        {
            if (!uri.StartsWith($"{Oid4VciConstants.CredentialOfferScheme}://", StringComparison.OrdinalIgnoreCase))
            {
                throw new CredentialOfferParseException($"URI must start with '{Oid4VciConstants.CredentialOfferScheme}://'");
            }

            var parsedUri = new Uri(uri);
#if NETSTANDARD2_1
            var query = parsedUri.Query.TrimStart('?');
            var queryParams = HttpUtility.ParseQueryString(query);
#else
            var queryParams = HttpUtility.ParseQueryString(parsedUri.Query);
#endif

            var credentialOfferParam = queryParams["credential_offer"];
            if (!string.IsNullOrEmpty(credentialOfferParam))
            {
                return ParseCredentialOfferJson(credentialOfferParam);
            }

            var credentialOfferUri = queryParams["credential_offer_uri"];
            if (!string.IsNullOrEmpty(credentialOfferUri))
            {
                return await FetchCredentialOfferAsync(credentialOfferUri, httpClient, cancellationToken).ConfigureAwait(false);
            }

            throw new CredentialOfferParseException("URI must contain either 'credential_offer' or 'credential_offer_uri' parameter");
        }
        catch (CredentialOfferParseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CredentialOfferParseException("Failed to parse credential offer URI", ex);
        }
    }

    /// <summary>
    /// Parses a credential offer from JSON string.
    /// </summary>
    /// <param name="json">The credential offer JSON</param>
    /// <returns>The parsed credential offer</returns>
    /// <exception cref="CredentialOfferParseException">Thrown when parsing fails</exception>
    public static CredentialOffer ParseJson(string json)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));
#else
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(json));
#endif

        try
        {
            return ParseCredentialOfferJson(json);
        }
        catch (Exception ex)
        {
            throw new CredentialOfferParseException("Failed to parse credential offer JSON", ex);
        }
    }

    /// <summary>
    /// Creates a credential offer URI from a credential offer object.
    /// </summary>
    /// <param name="credentialOffer">The credential offer to encode</param>
    /// <returns>The credential offer URI</returns>
    public static string CreateUri(CredentialOffer credentialOffer)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(credentialOffer);
#else
        if (credentialOffer == null)
            throw new ArgumentNullException(nameof(credentialOffer));
#endif

        var json = JsonSerializer.Serialize(credentialOffer, JsonOptions);
        var encodedJson = HttpUtility.UrlEncode(json);

        return $"{Oid4VciConstants.CredentialOfferScheme}://?credential_offer={encodedJson}";
    }

    private static async Task<CredentialOffer> FetchCredentialOfferAsync(
        string credentialOfferUri,
        HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(credentialOfferUri, UriKind.Absolute, out var offerUri) ||
            !string.Equals(offerUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !string.IsNullOrEmpty(offerUri.Fragment))
        {
            throw new CredentialOfferParseException("credential_offer_uri must be an absolute HTTPS URI without fragment.");
        }

        using var response = await httpClient.GetAsync(offerUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new CredentialOfferParseException($"credential_offer_uri returned HTTP {(int)response.StatusCode}.");
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
        {
            throw new CredentialOfferParseException("credential_offer_uri response must be application/json.");
        }

        var payload = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        if (payload.Length > MaxCredentialOfferBytes)
        {
            throw new CredentialOfferParseException("credential_offer_uri response exceeds size limit.");
        }

        var json = System.Text.Encoding.UTF8.GetString(payload);
        return ParseCredentialOfferJson(json);
    }

    private static CredentialOffer ParseCredentialOfferJson(string json)
    {
        // URL decode if needed
        var decodedJson = HttpUtility.UrlDecode(json);

        var credentialOffer = JsonSerializer.Deserialize<CredentialOffer>(decodedJson, JsonOptions);

        if (credentialOffer == null)
        {
            throw new CredentialOfferParseException("Failed to deserialize credential offer JSON");
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(credentialOffer.CredentialIssuer))
        {
            throw new CredentialOfferParseException("credential_issuer is required");
        }

        if (credentialOffer.CredentialConfigurationIds == null || credentialOffer.CredentialConfigurationIds.Length == 0)
        {
            throw new CredentialOfferParseException("credential_configuration_ids is required and must not be empty");
        }

        return credentialOffer;
    }
}
