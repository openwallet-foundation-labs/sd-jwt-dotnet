namespace SdJwt.Net.AgentTrust.Mcp;

/// <summary>
/// Result of an MCP trust interceptor operation.
/// </summary>
public record McpTrustResult
{
    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    public bool IsSuccess
    {
        get; private init;
    }

    /// <summary>
    /// The minted capability token, if successful.
    /// </summary>
    public string? Token
    {
        get; private init;
    }

    /// <summary>
    /// The token identifier, if successful.
    /// </summary>
    public string? TokenId
    {
        get; private init;
    }

    /// <summary>
    /// Error message, if the operation failed.
    /// </summary>
    public string? Error
    {
        get; private init;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static McpTrustResult Success(string token, string tokenId) => new()
    {
        IsSuccess = true,
        Token = token,
        TokenId = tokenId
    };

    /// <summary>
    /// Creates a denied result.
    /// </summary>
    public static McpTrustResult Denied(string reason) => new()
    {
        IsSuccess = false,
        Error = reason
    };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static McpTrustResult Failed(string error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}
