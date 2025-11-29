using System.Text.Json.Serialization;

namespace SdJwt.Net.Vc.Models;

/// <summary>
/// Represents common credential subject patterns for SD-JWT VCs.
/// This is a flexible model that can accommodate various subject structures.
/// </summary>
public class CredentialSubject
{
    /// <summary>
    /// Gets or sets the subject identifier.
    /// Optional. Identifier for the subject of the credential.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the given name of the subject.
    /// Optional. Can be selectively disclosed.
    /// </summary>
    [JsonPropertyName("given_name")]
    public string? GivenName { get; set; }

    /// <summary>
    /// Gets or sets the family name of the subject.
    /// Optional. Can be selectively disclosed.
    /// </summary>
    [JsonPropertyName("family_name")]
    public string? FamilyName { get; set; }

    /// <summary>
    /// Gets or sets the email address of the subject.
    /// Optional. Can be selectively disclosed.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the phone number of the subject.
    /// Optional. Can be selectively disclosed.
    /// </summary>
    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the birthdate of the subject.
    /// Optional. Can be selectively disclosed.
    /// </summary>
    [JsonPropertyName("birthdate")]
    public string? Birthdate { get; set; }

    /// <summary>
    /// Gets or sets the address of the subject.
    /// Optional. Can be selectively disclosed as a whole or individual components.
    /// </summary>
    [JsonPropertyName("address")]
    public object? Address { get; set; }

    /// <summary>
    /// Gets or sets any additional subject properties.
    /// These properties can be selectively disclosed based on credential type requirements.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData { get; set; }
}