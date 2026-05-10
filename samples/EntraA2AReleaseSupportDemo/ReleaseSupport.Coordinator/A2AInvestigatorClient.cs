using ReleaseSupport.Shared;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ReleaseSupport.Coordinator;

/// <summary>
/// HTTP client for communicating with the Release Investigator A2A server.
/// Discovers the agent card and sends investigation messages.
/// </summary>
public class A2AInvestigatorClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes the client.
    /// </summary>
    public A2AInvestigatorClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Discovers the investigator agent card.
    /// </summary>
    public async Task<A2AAgentCard?> DiscoverAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(Constants.Endpoints.AgentCard, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<A2AAgentCard>(cancellationToken: ct);
    }

    /// <summary>
    /// Sends an investigation message with the capability token as the Authorization bearer.
    /// </summary>
    public async Task<A2AMessageEnvelope?> SendAsync(
        string capabilityToken,
        string messageText,
        CancellationToken ct = default)
    {
        var envelope = new A2AMessageEnvelope
        {
            Message = new A2AMessage
            {
                Kind = "message",
                Role = "user",
                MessageId = Guid.NewGuid().ToString("N"),
                ContextId = Guid.NewGuid().ToString("N"),
                Parts =
                [
                    new A2AMessagePart
                    {
                        Kind = "text",
                        Text = messageText
                    }
                ]
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, Constants.Endpoints.MessageStream);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", capabilityToken);
        request.Content = JsonContent.Create(envelope);

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<A2AMessageEnvelope>(cancellationToken: ct);
    }
}
