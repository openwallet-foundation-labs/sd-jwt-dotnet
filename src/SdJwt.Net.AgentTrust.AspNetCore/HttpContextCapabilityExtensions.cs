using Microsoft.AspNetCore.Http;
using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.AspNetCore;

/// <summary>
/// HttpContext extensions for capability access.
/// </summary>
public static class HttpContextCapabilityExtensions
{
    /// <summary>
    /// Gets the verified capability claim.
    /// </summary>
    public static CapabilityClaim? GetVerifiedCapability(this HttpContext context)
    {
        return context.Items.TryGetValue(AgentTrustContextKeys.Capability, out var value)
            ? value as CapabilityClaim
            : null;
    }

    /// <summary>
    /// Gets capability context.
    /// </summary>
    public static CapabilityContext? GetCapabilityContext(this HttpContext context)
    {
        return context.Items.TryGetValue(AgentTrustContextKeys.Context, out var value)
            ? value as CapabilityContext
            : null;
    }

    /// <summary>
    /// Gets token issuer id.
    /// </summary>
    public static string? GetAgentIssuer(this HttpContext context)
    {
        return context.Items.TryGetValue(AgentTrustContextKeys.Issuer, out var value)
            ? value as string
            : null;
    }
}
