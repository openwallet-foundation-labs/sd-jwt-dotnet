namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// Represents presentation definition information.
/// </summary>
public class PresentationDefinitionInfo
{
    /// <summary>
    /// Unique ID of the presentation definition.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Optional name.
    /// </summary>
    public string? Name
    {
        get; set;
    }

    /// <summary>
    /// Optional purpose description.
    /// </summary>
    public string? Purpose
    {
        get; set;
    }

    /// <summary>
    /// Input descriptors defining required credentials.
    /// </summary>
    public IReadOnlyList<InputDescriptorInfo> InputDescriptors { get; set; } = [];
}
