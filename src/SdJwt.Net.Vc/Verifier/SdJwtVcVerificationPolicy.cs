using SdJwt.Net.Vc.Metadata;

namespace SdJwt.Net.Vc.Verifier;

/// <summary>
/// Policy options for SD-JWT VC verification beyond base RFC 9901 checks.
/// </summary>
public class SdJwtVcVerificationPolicy
{
    /// <summary>
    /// Gets or sets whether legacy <c>vc+sd-jwt</c> is accepted as typ value.
    /// </summary>
    public bool AcceptLegacyTyp { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional expected VCT value.
    /// </summary>
    public string? ExpectedVctType
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether type metadata resolution is required.
    /// </summary>
    public bool RequireTypeMetadata { get; set; } = false;

    /// <summary>
    /// Gets or sets whether status claim validation is required when a status claim is present.
    /// </summary>
    public bool RequireStatusCheck { get; set; } = false;

    /// <summary>
    /// Gets or sets the type metadata resolver.
    /// </summary>
    public ITypeMetadataResolver? TypeMetadataResolver
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the status validator.
    /// </summary>
    public ISdJwtVcStatusValidator? StatusValidator
    {
        get; set;
    }
}

/// <summary>
/// Validates credential status claim content.
/// </summary>
public interface ISdJwtVcStatusValidator
{
    /// <summary>
    /// Validates a deserialized status claim object.
    /// </summary>
    /// <param name="statusClaim">Status claim value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> if status is acceptable; otherwise <see langword="false"/>.</returns>
    Task<bool> ValidateAsync(object statusClaim, CancellationToken cancellationToken = default);
}
