using ReleaseSupport.Shared;

namespace ReleaseSupport.Coordinator;

/// <summary>
/// Orchestrates the 4-step release support workflow:
/// 1. Discover the investigator agent card
/// 2. Mint a scoped delegation token (identity embedded as iss claim)
/// 3. Send A2A investigation request with Authorization: Bearer capability
/// 4. Display results
/// </summary>
public class CoordinatorWorkflow
{
    private readonly A2AInvestigatorClient _client;
    private readonly DelegatedCapabilityIssuer _capabilityIssuer;
    private readonly string _coordinatorId;
    private readonly string _investigatorId;

    /// <summary>
    /// Initializes the coordinator workflow.
    /// </summary>
    public CoordinatorWorkflow(
        A2AInvestigatorClient client,
        DelegatedCapabilityIssuer capabilityIssuer,
        string coordinatorId,
        string investigatorId)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _capabilityIssuer = capabilityIssuer ?? throw new ArgumentNullException(nameof(capabilityIssuer));
        _coordinatorId = coordinatorId;
        _investigatorId = investigatorId;
    }

    /// <summary>
    /// Runs the full investigation workflow.
    /// </summary>
    public async Task RunAsync(
        string repository,
        string packageId,
        string version,
        string action,
        CancellationToken ct = default)
    {
        Console.WriteLine();
        Console.WriteLine("=== Release Support Coordinator ===");
        Console.WriteLine();

        // Step 1: Discover
        Console.WriteLine("[1] Discovering investigator agent...");
        var card = await _client.DiscoverAsync(ct);
        if (card == null)
        {
            Console.WriteLine("    FAILED: Could not discover agent card.");
            return;
        }
        Console.WriteLine($"    Found: {card.Name} v{card.Version}");
        Console.WriteLine($"    Trust required: {card.RequiresAgentTrust}");
        Console.WriteLine($"    Capabilities: {string.Join(", ", card.Capabilities ?? [])}");
        Console.WriteLine();

        // Step 2: Mint capability token (coordinator identity embedded as iss claim)
        Console.WriteLine("[2] Minting scoped delegation token...");
        Console.WriteLine($"    Issuer (identity): {_coordinatorId}");
        try
        {
            var tokenResult = await _capabilityIssuer.MintAsync(
                _coordinatorId, _investigatorId,
                repository, packageId, version, action, ct);

            Console.WriteLine($"    Token ID: {tokenResult.TokenId}");
            Console.WriteLine($"    Expires: {tokenResult.ExpiresAt:u}");
            Console.WriteLine($"    Scope: {Constants.Tools.ReleaseInvestigation}/{action}");
            Console.WriteLine($"    Resource: {repository}|{packageId}|{version}");
            Console.WriteLine();

            // Step 3: Send A2A message (capability token as Authorization: Bearer)
            Console.WriteLine("[3] Sending A2A investigation request...");
            var messageText = $"Investigate release status for {packageId} v{version} in {repository}";
            var response = await _client.SendAsync(tokenResult.Token, messageText, ct);

            if (response?.Message == null)
            {
                Console.WriteLine("    FAILED: No response from investigator.");
                return;
            }
            Console.WriteLine("    Response received.");
            Console.WriteLine();

            // Step 4: Display results
            Console.WriteLine("[4] Investigation Results:");
            Console.WriteLine("    ---");
            foreach (var part in response.Message.Parts ?? [])
            {
                if (part.Text != null)
                {
                    foreach (var line in part.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                    {
                        Console.WriteLine($"    {line}");
                    }
                }
            }
            Console.WriteLine("    ---");
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("Delegation denied", StringComparison.Ordinal))
        {
            Console.WriteLine($"    BLOCKED: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("[3] Skipped - delegation denied by policy.");
            Console.WriteLine("[4] No results - action not permitted.");
        }

        Console.WriteLine();
        Console.WriteLine("=== Workflow Complete ===");
    }
}
