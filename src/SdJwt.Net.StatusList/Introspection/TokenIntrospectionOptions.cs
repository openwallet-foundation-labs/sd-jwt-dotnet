namespace SdJwt.Net.StatusList.Introspection;

/// <summary>
/// Options for configuring the Token Introspection client.
/// </summary>
public class TokenIntrospectionOptions
{
    /// <summary>
    /// OAuth 2.0 client identifier for authenticating with the introspection endpoint.
    /// Required when the introspection endpoint requires client authentication.
    /// </summary>
    public string? ClientId
    {
        get; set;
    }

    /// <summary>
    /// OAuth 2.0 client secret for authenticating with the introspection endpoint.
    /// Used with client_secret_basic or client_secret_post authentication.
    /// </summary>
    public string? ClientSecret
    {
        get; set;
    }

    /// <summary>
    /// Client authentication method to use.
    /// Default is client_secret_basic (HTTP Basic Authentication).
    /// </summary>
    public ClientAuthMethod AuthMethod { get; set; } = ClientAuthMethod.ClientSecretBasic;

    /// <summary>
    /// Timeout for introspection requests.
    /// Default is 10 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Whether to validate the endpoint URI strictly.
    /// Default is true.
    /// </summary>
    public bool ValidateEndpointUri { get; set; } = true;

    /// <summary>
    /// Additional headers to include in introspection requests.
    /// </summary>
    public IDictionary<string, string>? AdditionalHeaders
    {
        get; set;
    }
}

/// <summary>
/// OAuth 2.0 client authentication methods.
/// </summary>
public enum ClientAuthMethod
{
    /// <summary>
    /// HTTP Basic Authentication using client_id and client_secret.
    /// </summary>
    ClientSecretBasic,

    /// <summary>
    /// Client credentials included in the request body.
    /// </summary>
    ClientSecretPost,

    /// <summary>
    /// No client authentication (for public introspection endpoints).
    /// </summary>
    None
}
