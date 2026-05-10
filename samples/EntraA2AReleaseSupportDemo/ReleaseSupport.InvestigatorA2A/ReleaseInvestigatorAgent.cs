using ReleaseSupport.Shared;
using SdJwt.Net.AgentTrust.Core;

namespace ReleaseSupport.InvestigatorA2A;

/// <summary>
/// Agent handler that processes release investigation requests
/// using verified SD-JWT capability claims.
/// </summary>
public class ReleaseInvestigatorAgent
{
    private readonly ReleaseDiagnosisService _diagnosisService;
    private readonly ILogger<ReleaseInvestigatorAgent> _logger;

    /// <summary>
    /// Initializes the investigator agent.
    /// </summary>
    public ReleaseInvestigatorAgent(
        ReleaseDiagnosisService diagnosisService,
        ILogger<ReleaseInvestigatorAgent> logger)
    {
        _diagnosisService = diagnosisService ?? throw new ArgumentNullException(nameof(diagnosisService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes an investigation request using the verified capability.
    /// The resource field encodes scope as <c>owner/repo|packageId|version</c>.
    /// </summary>
    public async Task<ReleaseInvestigationResult> HandleAsync(
        A2AMessageEnvelope envelope,
        CapabilityClaim capability,
        CancellationToken ct = default)
    {
        var action = capability.Action;
        if (!string.Equals(action, Constants.Tools.InvestigateAction, StringComparison.Ordinal))
        {
            _logger.LogWarning("Action {Action} not allowed. Only {Allowed} is permitted.",
                action, Constants.Tools.InvestigateAction);
            return new ReleaseInvestigationResult
            {
                Success = false,
                Error = $"Action '{action}' is not allowed. Only '{Constants.Tools.InvestigateAction}' is permitted."
            };
        }

        // Parse the resource field: "owner/repo|packageId|version"
        var resource = capability.Resource ?? string.Empty;
        var parts = resource.Split('|');
        if (parts.Length != 3)
        {
            return new ReleaseInvestigationResult
            {
                Success = false,
                Error = "Invalid resource format. Expected 'owner/repo|packageId|version'."
            };
        }

        var repository = parts[0];
        var packageId = parts[1];
        var version = parts[2];

        _logger.LogInformation(
            "Processing investigation: repository={Repository}, package={PackageId}, version={Version}",
            repository, packageId, version);

        return await _diagnosisService.InvestigateAsync(
            new ReleaseInvestigationRequest
            {
                Repository = repository,
                PackageId = packageId,
                Version = version,
                Action = action
            }, ct);
    }
}
