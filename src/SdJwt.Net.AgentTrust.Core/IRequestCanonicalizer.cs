namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Canonicalizes HTTP requests and computes hashes for request binding validation.
/// Uses RFC 8785 JCS canonicalization for JSON payloads.
/// </summary>
public interface IRequestCanonicalizer
{
    /// <summary>
    /// Canonicalizes an HTTP request binding into a deterministic form.
    /// </summary>
    CanonicalRequest Canonicalize(HttpRequestBinding request);

    /// <summary>
    /// Computes the SHA-256 hash of raw body bytes.
    /// </summary>
    string ComputeBodyHash(ReadOnlySpan<byte> body);

    /// <summary>
    /// Computes the SHA-256 hash of a JSON value using RFC 8785 JCS canonicalization.
    /// </summary>
    string ComputeJsonHash<T>(T value);

    /// <summary>
    /// Computes the SHA-256 hash of MCP tool call arguments using JCS canonicalization.
    /// </summary>
    string ComputeMcpArgumentsHash(McpToolCallEnvelope envelope);
}
