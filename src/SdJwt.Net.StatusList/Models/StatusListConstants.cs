namespace SdJwt.Net.StatusList.Models;

/// <summary>
/// Constants for Token Status List processing.
/// </summary>
public static class StatusListConstants
{
    /// <summary>
    /// The implemented Token Status List draft version.
    /// </summary>
    public const string SpecificationVersion = "draft-ietf-oauth-status-list-20";

    /// <summary>
    /// JWT media type for a Status List Token.
    /// </summary>
    public const string JwtMediaType = "application/statuslist+jwt";

    /// <summary>
    /// CWT media type for a Status List Token.
    /// </summary>
    public const string CwtMediaType = "application/statuslist+cwt";

    /// <summary>
    /// JWT type header for a Status List Token.
    /// </summary>
    public const string JwtType = "statuslist+jwt";
}
