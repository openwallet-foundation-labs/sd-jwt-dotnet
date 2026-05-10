using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Binds a capability token to the actual HTTP or MCP request.
/// Enables the verifier to confirm the token is being used for the exact approved operation.
/// </summary>
public record RequestBinding
{
    /// <summary>
    /// HTTP method (GET, POST, PUT, DELETE) or MCP method (tools/call).
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Canonicalized target URI (without query string for hashing).
    /// </summary>
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of the canonicalized request body or MCP arguments.
    /// </summary>
    public string? BodyHash
    {
        get; set;
    }

    /// <summary>
    /// MCP tool identifier when the request targets an MCP server.
    /// </summary>
    public string? McpToolId
    {
        get; set;
    }

    /// <summary>
    /// SHA-256 hash of the MCP tool JSON schema for tamper detection.
    /// </summary>
    public string? McpToolSchemaHash
    {
        get; set;
    }

    /// <summary>
    /// SHA-256 hash of the MCP tool call arguments.
    /// </summary>
    public string? McpArgumentsHash
    {
        get; set;
    }

    /// <summary>
    /// Content type of the request body (e.g., application/json).
    /// </summary>
    public string? ContentType
    {
        get; set;
    }

    /// <summary>
    /// Idempotency key for sensitive write operations.
    /// </summary>
    public string? IdempotencyKey
    {
        get; set;
    }

    /// <summary>
    /// Computes the SHA-256 hash of a byte payload.
    /// </summary>
    /// <param name="payload">The raw bytes to hash.</param>
    /// <returns>Base64url-encoded SHA-256 hash.</returns>
    public static string ComputeHash(byte[] payload)
    {
        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

#pragma warning disable CA1850
        using var sha256 = SHA256.Create();
#pragma warning restore CA1850
        var hash = sha256.ComputeHash(payload);
        return Base64UrlEncode(hash);
    }

    /// <summary>
    /// Computes the SHA-256 hash of a JSON-serialized object using stable property ordering.
    /// </summary>
    /// <param name="value">The object to serialize and hash.</param>
    /// <returns>Base64url-encoded SHA-256 hash.</returns>
    public static string ComputeJsonHash(object value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
        return ComputeHash(Encoding.UTF8.GetBytes(json));
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
