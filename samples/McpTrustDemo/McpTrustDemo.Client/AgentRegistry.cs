namespace McpTrustDemo.Client;

/// <summary>
/// Represents an AI agent with bounded capabilities.
/// </summary>
/// <param name="AgentId">Unique agent identifier URI.</param>
/// <param name="DisplayName">Human-readable name.</param>
/// <param name="AllowedTools">Tools this agent is configured to use.</param>
public record AgentProfile(string AgentId, string DisplayName, string[] AllowedTools);

/// <summary>
/// Registry of available agent profiles.
/// </summary>
public class AgentRegistry
{
    private readonly IReadOnlyDictionary<string, AgentProfile> _agents;

    public AgentRegistry(IReadOnlyDictionary<string, AgentProfile> agents)
    {
        _agents = agents;
    }

    public AgentProfile Get(string name) => _agents[name];

    public IEnumerable<AgentProfile> All => _agents.Values;
}
