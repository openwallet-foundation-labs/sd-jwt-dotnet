using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.A2A;

/// <summary>
/// Validates that delegated capabilities are properly attenuated per spec Section 18.2.
/// Child capabilities must be equal or narrower than parent capabilities.
/// </summary>
public static class AttenuationValidator
{
    /// <summary>
    /// Validates that a child capability is properly attenuated relative to its parent.
    /// </summary>
    /// <param name="parent">Parent capability token options.</param>
    /// <param name="child">Child capability token options.</param>
    /// <returns>Validation result with details of any violations.</returns>
    public static AttenuationValidationResult Validate(
        CapabilityTokenOptions parent,
        CapabilityTokenOptions child)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        if (child == null)
        {
            throw new ArgumentNullException(nameof(child));
        }

        var violations = new List<string>();

        // child.exp <= parent.exp
        if (child.Lifetime > parent.Lifetime)
        {
            violations.Add("Child lifetime exceeds parent lifetime.");
        }

        // child.cap.toolId equals or narrows parent.cap.toolId
        if (child.Capability != null && parent.Capability != null)
        {
            if (!string.Equals(child.Capability.Tool, parent.Capability.Tool, StringComparison.Ordinal))
            {
                violations.Add($"Child tool '{child.Capability.Tool}' differs from parent tool '{parent.Capability.Tool}'.");
            }

            // child.cap.action equals or narrows parent.cap.action
            if (!string.Equals(child.Capability.Action, parent.Capability.Action, StringComparison.Ordinal)
                && !string.Equals(parent.Capability.Action, "*", StringComparison.Ordinal))
            {
                violations.Add($"Child action '{child.Capability.Action}' is not within parent action '{parent.Capability.Action}'.");
            }

            // child.cap.resource equals or narrows parent.cap.resource
            if (child.Capability.Resource != null && parent.Capability.Resource != null
                && !IsNarrowedResource(parent.Capability.Resource, child.Capability.Resource))
            {
                violations.Add($"Child resource '{child.Capability.Resource}' is not narrower than parent resource '{parent.Capability.Resource}'.");
            }
        }

        // child.ctx.tenantId == parent.ctx.tenantId
        if (child.Context?.TenantId != null && parent.Context?.TenantId != null
            && !string.Equals(child.Context.TenantId, parent.Context.TenantId, StringComparison.Ordinal))
        {
            violations.Add("Child tenant differs from parent tenant.");
        }

        // Delegation depth checks
        if (child.Delegation != null && parent.Delegation != null)
        {
            var expectedDepth = parent.Delegation.Depth + 1;
            if (child.Delegation.Depth != expectedDepth)
            {
                violations.Add($"Child delegation depth {child.Delegation.Depth} must be {expectedDepth}.");
            }

            if (child.Delegation.Depth > child.Delegation.MaxDepth)
            {
                violations.Add($"Child delegation depth {child.Delegation.Depth} exceeds max depth {child.Delegation.MaxDepth}.");
            }

            // child.delegation.rootIssuer == rootIssuer
            if (!string.IsNullOrEmpty(parent.Delegation.RootIssuer)
                && !string.Equals(child.Delegation.RootIssuer, parent.Delegation.RootIssuer, StringComparison.Ordinal))
            {
                violations.Add("Child root issuer does not match parent root issuer.");
            }
        }
        else if (child.Delegation != null && parent.Delegation == null)
        {
            // Parent has no delegation claim: parentDepth = 0 and rootIssuer = parent.iss
            if (child.Delegation.Depth != 1)
            {
                violations.Add($"Child delegation depth must be 1 when parent has no delegation claim, got {child.Delegation.Depth}.");
            }

            if (!string.IsNullOrEmpty(parent.Issuer)
                && !string.Equals(child.Delegation.RootIssuer, parent.Issuer, StringComparison.Ordinal))
            {
                violations.Add("Child root issuer must equal parent issuer when parent has no delegation claim.");
            }
        }

        if (violations.Count > 0)
        {
            return AttenuationValidationResult.Invalid(violations);
        }

        return AttenuationValidationResult.Valid();
    }

    private static bool IsNarrowedResource(string parentResource, string childResource)
    {
        // Exact match is always valid
        if (string.Equals(parentResource, childResource, StringComparison.Ordinal))
        {
            return true;
        }

        // Wildcard parent permits any child
        if (string.Equals(parentResource, "*", StringComparison.Ordinal))
        {
            return true;
        }

        // Prefix narrowing: parent "docs/" allows child "docs/private/"
        if (parentResource.EndsWith("/", StringComparison.Ordinal)
            && childResource.StartsWith(parentResource, StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
