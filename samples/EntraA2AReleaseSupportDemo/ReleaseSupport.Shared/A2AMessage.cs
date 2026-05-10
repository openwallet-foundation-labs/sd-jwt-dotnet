using System.Text.Json.Serialization;

namespace ReleaseSupport.Shared;

/// <summary>
/// A2A protocol message envelope.
/// </summary>
public record A2AMessageEnvelope
{
    /// <summary>
    /// The message payload.
    /// </summary>
    [JsonPropertyName("message")]
    public A2AMessage Message { get; init; } = new();
}

/// <summary>
/// A2A protocol message.
/// </summary>
public record A2AMessage
{
    /// <summary>
    /// Message kind. Typically <c>message</c>.
    /// </summary>
    [JsonPropertyName("kind")]
    public string Kind { get; init; } = "message";

    /// <summary>
    /// Message role (e.g., <c>user</c>, <c>agent</c>).
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; init; } = "user";

    /// <summary>
    /// Message parts.
    /// </summary>
    [JsonPropertyName("parts")]
    public List<A2AMessagePart> Parts { get; init; } = [];

    /// <summary>
    /// Unique message identifier.
    /// </summary>
    [JsonPropertyName("messageId")]
    public string MessageId { get; init; } = string.Empty;

    /// <summary>
    /// Context identifier for correlation.
    /// </summary>
    [JsonPropertyName("contextId")]
    public string ContextId { get; init; } = string.Empty;
}

/// <summary>
/// A part of an A2A message.
/// </summary>
public record A2AMessagePart
{
    /// <summary>
    /// Part kind (e.g., <c>text</c>).
    /// </summary>
    [JsonPropertyName("kind")]
    public string Kind { get; init; } = "text";

    /// <summary>
    /// Text content.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}

/// <summary>
/// A2A agent card for discovery.
/// </summary>
public record A2AAgentCard
{
    /// <summary>
    /// Agent name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Agent description.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Agent version.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Supported capabilities.
    /// </summary>
    [JsonPropertyName("capabilities")]
    public List<string> Capabilities { get; init; } = [];

    /// <summary>
    /// A2A protocol version.
    /// </summary>
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; init; } = "1.0";

    /// <summary>
    /// Whether SD-JWT Agent Trust capability tokens are required.
    /// </summary>
    [JsonPropertyName("requiresAgentTrust")]
    public bool RequiresAgentTrust { get; init; } = true;
}
