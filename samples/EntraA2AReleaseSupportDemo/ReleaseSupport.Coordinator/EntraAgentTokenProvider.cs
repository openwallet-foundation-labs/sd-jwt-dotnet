namespace ReleaseSupport.Coordinator;

/// <summary>
/// Agent identity acquisition mode.
/// </summary>
public enum IdentityMode
{
    /// <summary>
    /// Uses a static agent URI for zero-dependency local development.
    /// No Entra registration or license required.
    /// </summary>
    DevFallbackStatic,

    /// <summary>
    /// Uses a standard Entra app registration with client credentials.
    /// Requires Azure AD app reg but not an E7 license.
    /// </summary>
    DevFallbackAppRegistration,

    /// <summary>
    /// Uses Microsoft Entra Agent ID for production workloads.
    /// Requires E7 license and Agent ID configuration.
    /// </summary>
    RealEntra
}

/// <summary>
/// Provides agent identity tokens for the coordinator based on the selected mode.
/// </summary>
public class EntraAgentTokenProvider
{
    private readonly IdentityMode _mode;
    private readonly string _agentUri;

    /// <summary>
    /// Initializes the token provider.
    /// </summary>
    public EntraAgentTokenProvider(IdentityMode mode, string agentUri)
    {
        _mode = mode;
        _agentUri = agentUri ?? throw new ArgumentNullException(nameof(agentUri));
    }

    /// <summary>
    /// Acquires an identity token for the coordinator agent.
    /// Returns a bearer token string and the agent identity URI.
    /// </summary>
    public Task<(string BearerToken, string AgentId)> AcquireTokenAsync(
        CancellationToken ct = default)
    {
        return _mode switch
        {
            IdentityMode.DevFallbackStatic => Task.FromResult((_agentUri, _agentUri)),

            IdentityMode.DevFallbackAppRegistration =>
                throw new NotSupportedException(
                    "DevFallbackAppRegistration requires Entra app registration configuration. " +
                    "Set --identity-mode DevFallbackStatic for local development."),

            IdentityMode.RealEntra =>
                throw new NotSupportedException(
                    "RealEntra requires Microsoft Entra Agent ID (E7 license). " +
                    "Set --identity-mode DevFallbackStatic for local development."),

            _ => throw new ArgumentOutOfRangeException(nameof(_mode))
        };
    }
}
