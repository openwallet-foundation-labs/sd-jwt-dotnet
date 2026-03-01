using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SdJwt.Net.StatusList.Introspection;

/// <summary>
/// OAuth 2.0 Token Introspection client per RFC 7662.
/// Provides real-time token status checking as an alternative to Status List tokens.
/// </summary>
public class TokenIntrospectionClient : ITokenIntrospectionClient
{
    private readonly HttpClient _httpClient;
    private readonly string _introspectionEndpoint;
    private readonly TokenIntrospectionOptions _options;
    private readonly bool _ownsHttpClient;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    /// <summary>
    /// Creates a new Token Introspection client.
    /// </summary>
    /// <param name="introspectionEndpoint">The introspection endpoint URI.</param>
    /// <param name="httpClient">Optional HTTP client (creates own if null).</param>
    /// <param name="options">Optional configuration options.</param>
    /// <exception cref="ArgumentException">Thrown when endpoint is null, empty, or invalid URI.</exception>
    public TokenIntrospectionClient(
        string introspectionEndpoint,
        HttpClient? httpClient = null,
        TokenIntrospectionOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(introspectionEndpoint))
        {
            throw new ArgumentException("Introspection endpoint cannot be null or empty.", nameof(introspectionEndpoint));
        }

        _options = options ?? new TokenIntrospectionOptions();

        if (_options.ValidateEndpointUri && !Uri.TryCreate(introspectionEndpoint, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("Introspection endpoint must be a valid absolute URI.", nameof(introspectionEndpoint));
        }

        _introspectionEndpoint = introspectionEndpoint;

        if (httpClient == null)
        {
            _httpClient = new HttpClient { Timeout = _options.Timeout };
            _ownsHttpClient = true;
        }
        else
        {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }
    }

    /// <inheritdoc/>
    public async Task<IntrospectionResult> IntrospectAsync(
        string token,
        string? tokenTypeHint = null,
        CancellationToken cancellationToken = default)
    {
        if (token == null)
        {
            throw new ArgumentNullException(nameof(token));
        }
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be empty.", nameof(token));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var request = BuildRequest(token, tokenTypeHint);
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new TokenIntrospectionException(
                $"Failed to send introspection request to {_introspectionEndpoint}: {ex.Message}",
                ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new TokenIntrospectionException(
                $"Introspection request failed with status code {(int)response.StatusCode}",
                (int)response.StatusCode,
                _introspectionEndpoint);
        }

        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return ParseResponse(responseContent);
    }

    private HttpRequestMessage BuildRequest(string token, string? tokenTypeHint)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _introspectionEndpoint);

        // Build form content
        var formParams = new List<KeyValuePair<string, string>>
        {
            new("token", token)
        };

        if (!string.IsNullOrEmpty(tokenTypeHint))
        {
            formParams.Add(new("token_type_hint", tokenTypeHint));
        }

        // Add client credentials based on auth method
        if (_options.AuthMethod == ClientAuthMethod.ClientSecretBasic &&
            !string.IsNullOrEmpty(_options.ClientId) &&
            !string.IsNullOrEmpty(_options.ClientSecret))
        {
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }
        else if (_options.AuthMethod == ClientAuthMethod.ClientSecretPost)
        {
            if (!string.IsNullOrEmpty(_options.ClientId))
            {
                formParams.Add(new("client_id", _options.ClientId));
            }
            if (!string.IsNullOrEmpty(_options.ClientSecret))
            {
                formParams.Add(new("client_secret", _options.ClientSecret));
            }
        }

        request.Content = new FormUrlEncodedContent(formParams);

        // Add additional headers
        if (_options.AdditionalHeaders != null)
        {
            foreach (var header in _options.AdditionalHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return request;
    }

    private IntrospectionResult ParseResponse(string responseContent)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(responseContent);
        }
        catch (JsonException ex)
        {
            throw new TokenIntrospectionException(
                $"Failed to parse introspection response: {ex.Message}",
                ex);
        }

        var root = document.RootElement;
        var result = new IntrospectionResult
        {
            RetrievedAt = DateTimeOffset.UtcNow
        };

        // Required field
        if (root.TryGetProperty("active", out var activeElement))
        {
            result.IsActive = activeElement.GetBoolean();
        }

        // Standard optional fields per RFC 7662
        if (root.TryGetProperty("scope", out var scopeElement))
        {
            result.Scope = scopeElement.GetString();
        }

        if (root.TryGetProperty("client_id", out var clientIdElement))
        {
            result.ClientId = clientIdElement.GetString();
        }

        if (root.TryGetProperty("username", out var usernameElement))
        {
            result.Username = usernameElement.GetString();
        }

        if (root.TryGetProperty("token_type", out var tokenTypeElement))
        {
            result.TokenType = tokenTypeElement.GetString();
        }

        if (root.TryGetProperty("exp", out var expElement) && expElement.TryGetInt64(out var exp))
        {
            result.ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(exp);
        }

        if (root.TryGetProperty("iat", out var iatElement) && iatElement.TryGetInt64(out var iat))
        {
            result.IssuedAt = DateTimeOffset.FromUnixTimeSeconds(iat);
        }

        if (root.TryGetProperty("nbf", out var nbfElement) && nbfElement.TryGetInt64(out var nbf))
        {
            result.NotBefore = DateTimeOffset.FromUnixTimeSeconds(nbf);
        }

        if (root.TryGetProperty("sub", out var subElement))
        {
            result.Subject = subElement.GetString();
        }

        if (root.TryGetProperty("aud", out var audElement))
        {
            result.Audience = audElement.ValueKind == JsonValueKind.Array
                ? string.Join(" ", audElement.EnumerateArray().Select(e => e.GetString()))
                : audElement.GetString();
        }

        if (root.TryGetProperty("iss", out var issElement))
        {
            result.Issuer = issElement.GetString();
        }

        if (root.TryGetProperty("jti", out var jtiElement))
        {
            result.JwtId = jtiElement.GetString();
        }

        // Extension: status field
        if (root.TryGetProperty("status", out var statusElement))
        {
            result.Status = statusElement.GetString();
        }

        // Collect additional claims
        var standardClaims = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "active", "scope", "client_id", "username", "token_type",
            "exp", "iat", "nbf", "sub", "aud", "iss", "jti", "status"
        };

        foreach (var property in root.EnumerateObject())
        {
            if (!standardClaims.Contains(property.Name))
            {
                result.AdditionalClaims[property.Name] = ParseJsonElement(property.Value);
            }
        }

        return result;
    }

    private static object ParseJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString()!,
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => element.EnumerateArray().Select(ParseJsonElement).ToList(),
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(p => p.Name, p => ParseJsonElement(p.Value)),
            _ => element.GetRawText()
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing && _ownsHttpClient)
        {
            _httpClient.Dispose();
        }

        _disposed = true;
    }
}
