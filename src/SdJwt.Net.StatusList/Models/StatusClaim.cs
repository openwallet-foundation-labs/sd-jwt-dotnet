using System.Text.Json.Serialization;

namespace SdJwt.Net.StatusList.Models;

/// <summary>
/// Represents the 'status' claim structure as defined in draft-ietf-oauth-status-list-13.
/// This claim is included in referenced tokens to enable status checking (revocation, suspension, etc.).
/// </summary>
public class StatusClaim
{
    /// <summary>
    /// Gets or sets the status_list object containing reference to a Status List Token.
    /// Required when using the status list mechanism.
    /// </summary>
    [JsonPropertyName("status_list")]
    public StatusListReference? StatusList { get; set; }

    /// <summary>
    /// Validates the status claim according to draft-ietf-oauth-status-list-13.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation fails</exception>
    public void Validate()
    {
        if (StatusList == null)
            throw new InvalidOperationException("StatusList is required");

        StatusList.Validate();
    }
}