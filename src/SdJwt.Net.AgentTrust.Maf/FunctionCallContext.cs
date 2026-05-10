using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.Maf;

/// <summary>
/// Generic function call context for middleware interception.
/// </summary>
public class FunctionCallContext
{
    /// <summary>
    /// Tool name.
    /// </summary>
    public required string ToolName
    {
        get; init;
    }

    /// <summary>
    /// Action name.
    /// </summary>
    public required string ActionName
    {
        get; init;
    }

    /// <summary>
    /// Optional argument payload.
    /// </summary>
    public IDictionary<string, object>? Arguments
    {
        get; init;
    }

    /// <summary>
    /// Correlation context.
    /// </summary>
    public required CapabilityContext Context
    {
        get; init;
    }

    /// <summary>
    /// Call metadata (headers, trace info).
    /// </summary>
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>(StringComparer.Ordinal);

    /// <summary>
    /// Optional request binding to bind the token to this specific invocation.
    /// </summary>
    public RequestBinding? RequestBinding
    {
        get; init;
    }
}
