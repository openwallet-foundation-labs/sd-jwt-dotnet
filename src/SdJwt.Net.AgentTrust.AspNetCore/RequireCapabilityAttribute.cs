using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SdJwt.Net.AgentTrust.AspNetCore;

/// <summary>
/// Requires tool/action capability for endpoint execution.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireCapabilityAttribute : Attribute, IAuthorizationFilter
{
    /// <summary>
    /// Initializes attribute.
    /// </summary>
    public RequireCapabilityAttribute(string tool, string action)
    {
        Tool = tool;
        Action = action;
    }

    /// <summary>
    /// Required tool.
    /// </summary>
    public string Tool
    {
        get; set;
    }

    /// <summary>
    /// Required action.
    /// </summary>
    public string Action
    {
        get; set;
    }

    /// <summary>
    /// Optional required resource.
    /// </summary>
    public string? Resource
    {
        get; set;
    }

    /// <inheritdoc/>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var capability = context.HttpContext.GetVerifiedCapability();
        if (capability == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (!string.Equals(capability.Tool, Tool, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(capability.Action, Action, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new ForbidResult();
            return;
        }

        if (!string.IsNullOrWhiteSpace(Resource) &&
            !string.Equals(capability.Resource, Resource, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new ForbidResult();
        }
    }
}
