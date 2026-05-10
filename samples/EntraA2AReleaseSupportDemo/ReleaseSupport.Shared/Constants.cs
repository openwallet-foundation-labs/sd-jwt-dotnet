namespace ReleaseSupport.Shared;

/// <summary>
/// Shared constants for the Release Support A2A demo.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Agent identity URIs.
    /// </summary>
    public static class Agents
    {
        /// <summary>
        /// Coordinator agent identity pattern. Use <c>string.Format</c> with tenant ID.
        /// </summary>
        public const string CoordinatorPattern = "agent://entra/{0}/release-support-coordinator-dev";

        /// <summary>
        /// Investigator agent identity pattern. Use <c>string.Format</c> with tenant ID.
        /// </summary>
        public const string InvestigatorPattern = "agent://entra/{0}/release-investigator-dev";

        /// <summary>
        /// Default tenant ID for dev-fallback mode.
        /// </summary>
        public const string DevTenantId = "dev-local";
    }

    /// <summary>
    /// Tool and action identifiers.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Release investigation tool identifier.
        /// </summary>
        public const string ReleaseInvestigation = "release-investigation";

        /// <summary>
        /// Allowed investigation action.
        /// </summary>
        public const string InvestigateAction = "InvestigatePackageRelease";

        /// <summary>
        /// Blocked action for governance demos.
        /// </summary>
        public const string RerunWorkflowAction = "RerunWorkflow";
    }

    /// <summary>
    /// A2A endpoint paths.
    /// </summary>
    public static class Endpoints
    {
        /// <summary>
        /// A2A agent card path.
        /// </summary>
        public const string AgentCard = "/a2a/release-investigator/v1/card";

        /// <summary>
        /// A2A message stream path.
        /// </summary>
        public const string MessageStream = "/a2a/release-investigator/v1/message:stream";
    }

    /// <summary>
    /// Default capability lifetime.
    /// </summary>
    public static readonly TimeSpan DefaultCapabilityLifetime = TimeSpan.FromMinutes(5);
}
