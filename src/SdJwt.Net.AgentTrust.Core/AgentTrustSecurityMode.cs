namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Controls security strictness for Agent Trust verification.
/// </summary>
public enum AgentTrustSecurityMode
{
    /// <summary>
    /// Minimal security: symmetric keys, bearer tokens, in-memory replay.
    /// Suitable for demos and local development only.
    /// </summary>
    Demo,

    /// <summary>
    /// Moderate security: asymmetric keys recommended, replay prevention required.
    /// Suitable for internal pilot deployments.
    /// </summary>
    Pilot,

    /// <summary>
    /// Full security: asymmetric keys required, proof-of-possession required for
    /// privileged calls, distributed replay store, request binding for unsafe methods.
    /// Suitable for cross-boundary and production deployments.
    /// </summary>
    Enterprise,

    /// <summary>
    /// Maximum security: all Enterprise controls plus mandatory request binding,
    /// proof-of-possession for all calls, receipt chaining, and approval workflows.
    /// Suitable for regulated industries (financial, healthcare, government).
    /// </summary>
    Regulated
}
