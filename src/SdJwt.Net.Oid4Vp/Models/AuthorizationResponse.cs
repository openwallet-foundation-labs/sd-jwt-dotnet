using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models;

/// <summary>
/// Represents an Authorization Response according to OID4VP 1.0.
/// This is the wallet's response containing the VP tokens and presentation submission.
/// </summary>
public class AuthorizationResponse
{
    /// <summary>
    /// Gets or sets the VP token.
    /// CONDITIONAL. Contains the Verifiable Presentation(s) as SD-JWT string.
    /// Can be a single string or array of strings.
    /// </summary>
    [JsonPropertyName("vp_token")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public object? VpToken
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the presentation submission.
    /// CONDITIONAL. The Presentation Submission mapping object.
    /// </summary>
    [JsonPropertyName("presentation_submission")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public PresentationSubmission? PresentationSubmission
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the SIOPv2 ID Token returned for combined <c>vp_token id_token</c> flows.
    /// CONDITIONAL. Present when the authorization request response type included <c>id_token</c>.
    /// </summary>
    [JsonPropertyName("id_token")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? IdToken
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the state parameter.
    /// OPTIONAL. The state parameter from the authorization request.
    /// </summary>
    [JsonPropertyName("state")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? State
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the error code.
    /// CONDITIONAL. Present when the authorization request could not be completed.
    /// </summary>
    [JsonPropertyName("error")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Error
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the error description.
    /// OPTIONAL. Human-readable error description.
    /// </summary>
    [JsonPropertyName("error_description")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? ErrorDescription
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the error URI.
    /// OPTIONAL. URI with more information about the error.
    /// </summary>
    [JsonPropertyName("error_uri")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? ErrorUri
    {
        get; set;
    }

    /// <summary>
    /// Creates a successful authorization response with a single VP token.
    /// </summary>
    /// <param name="vpToken">The SD-JWT VP token</param>
    /// <param name="presentationSubmission">The presentation submission</param>
    /// <param name="state">Optional state parameter</param>
    /// <returns>A new AuthorizationResponse instance</returns>
    public static AuthorizationResponse Success(
        string vpToken,
        PresentationSubmission presentationSubmission,
        string? state = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(vpToken);
        ArgumentNullException.ThrowIfNull(presentationSubmission);
#else
        if (string.IsNullOrWhiteSpace(vpToken))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(vpToken));
        if (presentationSubmission == null)
            throw new ArgumentNullException(nameof(presentationSubmission));
#endif

        return new AuthorizationResponse
        {
            VpToken = vpToken,
            PresentationSubmission = presentationSubmission,
            State = state
        };
    }

    /// <summary>
    /// Creates a successful authorization response for a DCQL-based presentation.
    /// Per OID4VP 1.0 Section 8, the VP Token is a JSON object keyed by DCQL credential query
    /// <c>id</c> values, each mapping to an array of presentation strings.
    /// </summary>
    /// <param name="dcqlVpToken">
    /// Dictionary keyed by DCQL credential query <c>id</c>; each value is an array of
    /// presentation strings (typically one element when <c>multiple</c> is <see langword="false"/>).
    /// </param>
    /// <param name="state">Optional state parameter.</param>
    /// <returns>A new <see cref="AuthorizationResponse"/> instance.</returns>
    public static AuthorizationResponse SuccessWithDcql(
        Dictionary<string, string[]> dcqlVpToken,
        string? state = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(dcqlVpToken);
#else
        if (dcqlVpToken == null)
            throw new ArgumentNullException(nameof(dcqlVpToken));
#endif

        if (dcqlVpToken.Count == 0)
            throw new ArgumentException("DCQL VP token must contain at least one entry.", nameof(dcqlVpToken));

        return new AuthorizationResponse
        {
            VpToken = dcqlVpToken,
            State = state
        };
    }

    /// <summary>
    /// Creates a successful authorization response with a VP token and SIOPv2 ID Token.
    /// </summary>
    /// <param name="vpToken">The SD-JWT VP token.</param>
    /// <param name="presentationSubmission">The presentation submission.</param>
    /// <param name="idToken">The SIOPv2 subject-signed ID Token.</param>
    /// <param name="state">Optional state parameter.</param>
    /// <returns>A new <see cref="AuthorizationResponse"/> instance.</returns>
    public static AuthorizationResponse SuccessWithIdToken(
        string vpToken,
        PresentationSubmission presentationSubmission,
        string idToken,
        string? state = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(idToken);
#else
        if (string.IsNullOrWhiteSpace(idToken))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(idToken));
#endif

        var response = Success(vpToken, presentationSubmission, state);
        response.IdToken = idToken;
        return response;
    }

    /// <summary>
    /// Creates a successful authorization response with multiple VP tokens.
    /// </summary>
    /// <param name="vpTokens">Array of SD-JWT VP tokens</param>
    /// <param name="presentationSubmission">The presentation submission</param>
    /// <param name="state">Optional state parameter</param>
    /// <returns>A new AuthorizationResponse instance</returns>
    public static AuthorizationResponse Success(
        string[] vpTokens,
        PresentationSubmission presentationSubmission,
        string? state = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(vpTokens);
        ArgumentNullException.ThrowIfNull(presentationSubmission);
#else
        if (vpTokens == null)
            throw new ArgumentNullException(nameof(vpTokens));
        if (presentationSubmission == null)
            throw new ArgumentNullException(nameof(presentationSubmission));
#endif

        if (vpTokens.Length == 0)
            throw new ArgumentException("At least one VP token is required", nameof(vpTokens));

        foreach (var token in vpTokens)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("VP tokens cannot be null or whitespace", nameof(vpTokens));
        }

        return new AuthorizationResponse
        {
            VpToken = vpTokens,
            PresentationSubmission = presentationSubmission,
            State = state
        };
    }

    /// <summary>
    /// Creates an error authorization response.
    /// </summary>
    /// <param name="error">The error code</param>
    /// <param name="errorDescription">Optional error description</param>
    /// <param name="errorUri">Optional error URI</param>
    /// <param name="state">Optional state parameter</param>
    /// <returns>A new AuthorizationResponse instance representing an error</returns>
    public static AuthorizationResponse CreateError(
        string error,
        string? errorDescription = null,
        string? errorUri = null,
        string? state = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
#else
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(error));
#endif

        return new AuthorizationResponse
        {
            Error = error,
            ErrorDescription = errorDescription,
            ErrorUri = errorUri,
            State = state
        };
    }

    /// <summary>
    /// Gets the VP tokens as an array of strings.
    /// </summary>
    /// <returns>Array of VP token strings</returns>
    public string[] GetVpTokens()
    {
        if (VpToken == null)
            return Array.Empty<string>();

        if (VpToken is string singleToken)
            return new[] { singleToken };

        if (VpToken is string[] tokenArray)
            return tokenArray;

        if (VpToken is Dictionary<string, string[]> dcqlDictionary)
            return dcqlDictionary.Values.SelectMany(tokens => tokens).ToArray();

        if (VpToken is IReadOnlyDictionary<string, string[]> readOnlyDcqlDictionary)
            return readOnlyDcqlDictionary.Values.SelectMany(tokens => tokens).ToArray();

        if (VpToken is System.Text.Json.JsonElement element)
        {
            if (element.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var token = element.GetString();
                return token != null ? new[] { token } : Array.Empty<string>();
            }

            if (element.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var tokens = new List<string>();
                foreach (var item in element.EnumerateArray())
                {
                    var token = item.GetString();
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        tokens.Add(token);
                    }
                }
                return tokens.ToArray();
            }

            if (element.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                return ReadDcqlVpTokenObject(element)
                    .Values
                    .SelectMany(tokens => tokens)
                    .ToArray();
            }
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Gets the VP token as a DCQL response object keyed by credential query id.
    /// </summary>
    /// <returns>A dictionary of DCQL credential query id to presentation strings.</returns>
    public Dictionary<string, string[]> GetDcqlVpTokens()
    {
        if (VpToken == null)
            return new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (VpToken is Dictionary<string, string[]> dcqlDictionary)
            return new Dictionary<string, string[]>(dcqlDictionary, StringComparer.Ordinal);

        if (VpToken is IReadOnlyDictionary<string, string[]> readOnlyDcqlDictionary)
            return readOnlyDcqlDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

        if (VpToken is System.Text.Json.JsonElement element && element.ValueKind == System.Text.Json.JsonValueKind.Object)
            return ReadDcqlVpTokenObject(element);

        var flatTokens = GetVpTokens();
        return flatTokens.Length == 0
            ? new Dictionary<string, string[]>(StringComparer.Ordinal)
            : new Dictionary<string, string[]>(StringComparer.Ordinal) { ["$"] = flatTokens };
    }

    /// <summary>
    /// Gets whether this response represents an error.
    /// </summary>
    /// <returns>True if this is an error response</returns>
    public bool IsError => !string.IsNullOrWhiteSpace(Error);

    /// <summary>
    /// Gets whether this response contains VP tokens.
    /// </summary>
    /// <returns>True if VP tokens are present</returns>
    public bool HasVpTokens => VpToken != null && GetVpTokens().Length > 0;

    /// <summary>
    /// Validates this authorization response according to OID4VP 1.0 requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the response is invalid</exception>
    public void Validate()
    {
        // For DCQL responses, VpToken is present but PresentationSubmission is absent (not used with DCQL).
        // For PE responses, both VpToken and PresentationSubmission are present.
        var hasSuccess = HasVpTokens;
        var hasError = !string.IsNullOrWhiteSpace(Error);

        if (!hasSuccess && !hasError)
        {
            throw new InvalidOperationException("Response must contain either VP tokens or an error");
        }

        if (hasSuccess && hasError)
        {
            throw new InvalidOperationException("Response cannot contain both VP tokens and error");
        }

        if (IdToken != null && string.IsNullOrWhiteSpace(IdToken))
        {
            throw new InvalidOperationException("id_token must not be empty when provided");
        }

        // Validate presentation submission if present (PE flow only)
        PresentationSubmission?.Validate();
    }

    private static Dictionary<string, string[]> ReadDcqlVpTokenObject(System.Text.Json.JsonElement element)
    {
        var result = new Dictionary<string, string[]>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var token = property.Value.GetString();
                if (!string.IsNullOrWhiteSpace(token))
                    result[property.Name] = new[] { token };
            }
            else if (property.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var tokens = new List<string>();
                foreach (var item in property.Value.EnumerateArray())
                {
                    var token = item.GetString();
                    if (!string.IsNullOrWhiteSpace(token))
                        tokens.Add(token);
                }

                result[property.Name] = tokens.ToArray();
            }
        }

        return result;
    }
}
