using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SdJwt.Net.AgentTrust.Policy.Opa;

/// <summary>
/// Policy engine that delegates evaluation to an Open Policy Agent (OPA) server via HTTP.
/// </summary>
public class OpaHttpPolicyEngine : IPolicyEngine
{
    private readonly HttpClient _httpClient;
    private readonly OpaOptions _options;
    private readonly ILogger<OpaHttpPolicyEngine> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new OPA policy engine.
    /// </summary>
    /// <param name="httpClientFactory">HTTP client factory.</param>
    /// <param name="options">OPA configuration options.</param>
    /// <param name="logger">Optional logger.</param>
    public OpaHttpPolicyEngine(
        IHttpClientFactory httpClientFactory,
        IOptions<OpaOptions> options,
        ILogger<OpaHttpPolicyEngine>? logger = null)
    {
        if (httpClientFactory == null)
        {
            throw new ArgumentNullException(nameof(httpClientFactory));
        }

        _httpClient = httpClientFactory.CreateClient("AgentTrustOpa");
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger<OpaHttpPolicyEngine>.Instance;
        _httpClient.Timeout = _options.Timeout;
    }

    /// <inheritdoc/>
    public async Task<PolicyDecision> EvaluateAsync(PolicyRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var input = new OpaInput
        {
            AgentId = request.AgentId,
            Tool = request.Tool,
            Action = request.Action,
            Resource = request.Resource,
            DelegationDepth = request.DelegationChain?.Depth,
            DelegationMaxDepth = request.DelegationChain?.MaxDepth,
            TenantId = request.Context?.TenantId
        };

        try
        {
            var url = $"{_options.BaseUrl.TrimEnd('/')}{_options.PolicyPath}";
            var response = await _httpClient.PostAsJsonAsync(
                url,
                new OpaRequest { Input = input },
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OPA returned {StatusCode} for {AgentId}/{Tool}/{Action}",
                    response.StatusCode, request.AgentId, request.Tool, request.Action);

                return _options.DenyOnError
                    ? PolicyDecision.Deny("OPA returned error.", "opa_error")
                    : PolicyDecision.Permit();
            }

            var result = await response.Content.ReadFromJsonAsync<OpaResponse>(JsonOptions, cancellationToken);

            if (result?.Result?.Allow == true)
            {
                return PolicyDecision.Permit(result.Result.Constraints);
            }

            return PolicyDecision.Deny(
                result?.Result?.DenialReason ?? "Policy denied by OPA.",
                "opa_denied");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogError(ex, "Failed to evaluate policy via OPA for {AgentId}/{Tool}/{Action}",
                request.AgentId, request.Tool, request.Action);

            return _options.DenyOnError
                ? PolicyDecision.Deny($"OPA unreachable: {ex.Message}", "opa_unreachable")
                : PolicyDecision.Permit();
        }
    }

    private sealed class OpaRequest
    {
        public OpaInput Input { get; set; } = new();
    }

    private sealed class OpaInput
    {
        public string AgentId { get; set; } = string.Empty;
        public string Tool { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Resource
        {
            get; set;
        }
        public int? DelegationDepth
        {
            get; set;
        }
        public int? DelegationMaxDepth
        {
            get; set;
        }
        public string? TenantId
        {
            get; set;
        }
    }

    private sealed class OpaResponse
    {
        public OpaResult? Result
        {
            get; set;
        }
    }

    private sealed class OpaResult
    {
        public bool Allow
        {
            get; set;
        }
        public string? DenialReason
        {
            get; set;
        }
        public PolicyConstraints? Constraints
        {
            get; set;
        }
    }
}
