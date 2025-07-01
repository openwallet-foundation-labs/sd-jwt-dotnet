using System.Text.Json.Serialization;

namespace SdJwt.Net.Models;

/// <summary>
/// Represents the 'status' claim within a Verifiable Credential,
/// pointing to an entry in a Status List for revocation checking,
/// as defined in `draft-ietf-oauth-status-list`.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="StatusClaim"/> class.
/// </remarks>
/// <param name="statusListCredential">The URI of the Status List Credential.</param>
/// <param name="statusListIndex">The index for this credential's status.</param>
public class StatusClaim(string statusListCredential, int statusListIndex)
{
    /// <summary>
    /// A URI that can be dereferenced to retrieve the Status List Credential.
    /// </summary>
    [JsonPropertyName("status_list_credential")]
    public string StatusListCredential { get; set; } = statusListCredential;

    /// <summary>
    /// The index of this credential's status within the bitstring of the Status List.
    /// </summary>
    [JsonPropertyName("status_list_index")]
    public int StatusListIndex { get; set; } = statusListIndex;
}