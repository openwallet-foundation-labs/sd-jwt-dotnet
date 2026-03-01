using SdJwt.Net.Wallet.Core;

namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// Represents a parsed presentation request.
/// </summary>
public class PresentationRequestInfo
{
    /// <summary>
    /// Unique request ID.
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// The verifier's client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The response URI where the presentation should be sent.
    /// </summary>
    public string ResponseUri { get; set; } = string.Empty;

    /// <summary>
    /// The nonce for replay protection.
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// Response mode (direct_post, direct_post.jwt, fragment, etc.).
    /// </summary>
    public string ResponseMode { get; set; } = string.Empty;

    /// <summary>
    /// Response type (vp_token, id_token, etc.).
    /// </summary>
    public string ResponseType { get; set; } = string.Empty;

    /// <summary>
    /// State value for correlation.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// The presentation definition or reference.
    /// </summary>
    public PresentationDefinitionInfo? PresentationDefinition { get; set; }

    /// <summary>
    /// Client ID scheme used.
    /// </summary>
    public string? ClientIdScheme { get; set; }

    /// <summary>
    /// Client metadata if provided.
    /// </summary>
    public IDictionary<string, object>? ClientMetadata { get; set; }

    /// <summary>
    /// DC API provider information if applicable.
    /// </summary>
    public string? DcApiProvider { get; set; }
}

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
    public string? Name { get; set; }

    /// <summary>
    /// Optional purpose description.
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// Input descriptors defining required credentials.
    /// </summary>
    public IReadOnlyList<InputDescriptorInfo> InputDescriptors { get; set; } = [];
}

/// <summary>
/// Represents an input descriptor.
/// </summary>
public class InputDescriptorInfo
{
    /// <summary>
    /// Unique ID of the descriptor.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Optional name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Optional purpose.
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// Accepted credential formats.
    /// </summary>
    public IReadOnlyList<string> Formats { get; set; } = [];

    /// <summary>
    /// Required claim paths.
    /// </summary>
    public IReadOnlyList<string> RequiredClaims { get; set; } = [];

    /// <summary>
    /// Optional claim paths.
    /// </summary>
    public IReadOnlyList<string> OptionalClaims { get; set; } = [];

    /// <summary>
    /// Constraints on the credential.
    /// </summary>
    public ConstraintInfo? Constraints { get; set; }
}

/// <summary>
/// Credential constraints information.
/// </summary>
public class ConstraintInfo
{
    /// <summary>
    /// Field constraints.
    /// </summary>
    public IReadOnlyList<FieldConstraintInfo> Fields { get; set; } = [];

    /// <summary>
    /// Limit disclosure policy.
    /// </summary>
    public string? LimitDisclosure { get; set; }
}

/// <summary>
/// Field constraint information.
/// </summary>
public class FieldConstraintInfo
{
    /// <summary>
    /// JSONPath to the field.
    /// </summary>
    public IReadOnlyList<string> Paths { get; set; } = [];

    /// <summary>
    /// Optional filter for the field value.
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// Whether intent to retain is requested.
    /// </summary>
    public bool IntentToRetain { get; set; }
}

/// <summary>
/// Represents a credential match for the request.
/// </summary>
public class CredentialMatch
{
    /// <summary>
    /// The input descriptor ID this matches.
    /// </summary>
    public string InputDescriptorId { get; set; } = string.Empty;

    /// <summary>
    /// The matching credential.
    /// </summary>
    public StoredCredential? Credential { get; set; }

    /// <summary>
    /// Claims that will be disclosed.
    /// </summary>
    public IReadOnlyList<string> DisclosedClaims { get; set; } = [];

    /// <summary>
    /// Whether all required claims are satisfied.
    /// </summary>
    public bool SatisfiesRequirements { get; set; }

    /// <summary>
    /// Missing required claims if any.
    /// </summary>
    public IReadOnlyList<string> MissingClaims { get; set; } = [];
}

/// <summary>
/// Result of submitting a presentation.
/// </summary>
public class PresentationSubmissionResult
{
    /// <summary>
    /// Whether the submission was successful.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Redirect URL if the verifier responded with one.
    /// </summary>
    public string? RedirectUri { get; set; }

    /// <summary>
    /// Error code if unsuccessful.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Error description.
    /// </summary>
    public string? ErrorDescription { get; set; }

    /// <summary>
    /// Response code for OAuth callback.
    /// </summary>
    public string? ResponseCode { get; set; }
}

/// <summary>
/// Options for presentation submission.
/// </summary>
public class PresentationSubmissionOptions
{
    /// <summary>
    /// The credential matches to include.
    /// </summary>
    public IReadOnlyList<CredentialMatch> Matches { get; set; } = [];

    /// <summary>
    /// The key manager for signing.
    /// </summary>
    public IKeyManager? KeyManager { get; set; }

    /// <summary>
    /// Additional state to include.
    /// </summary>
    public string? State { get; set; }
}

/// <summary>
/// Adapter interface for OpenID4VP presentation protocol.
/// </summary>
public interface IOid4VpAdapter
{
    /// <summary>
    /// Parses a presentation request from URI or JSON.
    /// </summary>
    /// <param name="request">The request URI, JSON, or same-device request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed presentation request information.</returns>
    Task<PresentationRequestInfo> ParseRequestAsync(
        string request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a request_uri to the actual request.
    /// </summary>
    /// <param name="requestUri">The request_uri to resolve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resolved presentation request.</returns>
    Task<PresentationRequestInfo> ResolveRequestUriAsync(
        string requestUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds matching credentials for the request.
    /// </summary>
    /// <param name="request">The presentation request.</param>
    /// <param name="availableCredentials">Available credentials to match against.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of credential matches.</returns>
    Task<IReadOnlyList<CredentialMatch>> FindMatchingCredentialsAsync(
        PresentationRequestInfo request,
        IReadOnlyList<StoredCredential> availableCredentials,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates and submits a presentation response.
    /// </summary>
    /// <param name="request">The original presentation request.</param>
    /// <param name="options">Submission options with matches and key manager.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Submission result.</returns>
    Task<PresentationSubmissionResult> SubmitPresentationAsync(
        PresentationRequestInfo request,
        PresentationSubmissionOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a presentation error response.
    /// </summary>
    /// <param name="request">The presentation request.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="errorDescription">Optional error description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Submission result with error sent.</returns>
    Task<PresentationSubmissionResult> SendErrorResponseAsync(
        PresentationRequestInfo request,
        string errorCode,
        string? errorDescription = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the client (verifier) using the specified scheme.
    /// </summary>
    /// <param name="request">The presentation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the client is valid.</returns>
    Task<bool> ValidateClientAsync(
        PresentationRequestInfo request,
        CancellationToken cancellationToken = default);
}
