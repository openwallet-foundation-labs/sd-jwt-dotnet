namespace SdJwt.Net.AgentTrust.Maf;

/// <summary>
/// Abstract builder for wiring function-call middleware.
/// </summary>
public interface IAgentBuilder
{
    /// <summary>
    /// Registers middleware in the call pipeline.
    /// </summary>
    IAgentBuilder Use(Func<FunctionCallContext, Func<FunctionCallContext, Task>, Task> middleware);
}
