namespace SdJwt.Net.AgentTrust.Policy;

/// <summary>
/// Builder for delegation constraints.
/// </summary>
public class DelegationConstraintsBuilder
{
    private readonly List<string> _allowedActions = [];

    /// <summary>
    /// Initializes a new builder.
    /// </summary>
    public DelegationConstraintsBuilder(int maxDepth)
    {
        MaxDepth = maxDepth;
    }

    /// <summary>
    /// Max delegation depth.
    /// </summary>
    public int MaxDepth
    {
        get;
    }

    /// <summary>
    /// Allowed actions.
    /// </summary>
    public IReadOnlyList<string> AllowedActions => _allowedActions;

    /// <summary>
    /// Adds an allowed action.
    /// </summary>
    public DelegationConstraintsBuilder AllowAction(string action)
    {
        if (!string.IsNullOrWhiteSpace(action))
        {
            _allowedActions.Add(action);
        }

        return this;
    }
}
