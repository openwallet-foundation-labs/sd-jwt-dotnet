namespace SdJwt.Net.AgentTrust.Policy.Opa;

/// <summary>
/// Configuration options for the OPA policy engine.
/// </summary>
public class OpaOptions
{
    /// <summary>
    /// Base URL of the OPA server (e.g., <c>http://localhost:8181</c>).
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:8181";

    /// <summary>
    /// Policy path for agent trust evaluation (e.g., <c>/v1/data/agenttrust/allow</c>).
    /// </summary>
    public string PolicyPath { get; set; } = "/v1/data/agenttrust/allow";

    /// <summary>
    /// Timeout for OPA HTTP requests. Defaults to 5 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Whether to deny when OPA is unreachable. Defaults to true (fail-closed).
    /// </summary>
    public bool DenyOnError { get; set; } = true;
}
