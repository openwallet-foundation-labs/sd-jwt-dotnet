using System.Text;
using System.Text.Json;
using SdJwt.Net.Oid4Vp.Models;

namespace SdJwt.Net.Oid4Vp.Verifier;

/// <summary>
/// Builder for creating OID4VP authorization requests with presentation definitions.
/// Provides a fluent API for constructing complex presentation requirements.
/// </summary>
public class PresentationRequestBuilder
{
    private readonly AuthorizationRequest _request;
    private readonly PresentationDefinition _definition;
    private readonly List<InputDescriptor> _inputDescriptors;
    private readonly List<SubmissionRequirement> _submissionRequirements;

    private PresentationRequestBuilder(string clientId, string responseUri)
    {
        var definitionId = Guid.NewGuid().ToString();
        var nonce = GenerateNonce();

        _definition = new PresentationDefinition
        {
            Id = definitionId
        };

        _request = new AuthorizationRequest
        {
            ClientId = clientId,
            ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
            ResponseMode = Oid4VpConstants.ResponseModes.DirectPost,
            ResponseUri = responseUri,
            Nonce = nonce,
            PresentationDefinition = _definition
        };

        _inputDescriptors = new List<InputDescriptor>();
        _submissionRequirements = new List<SubmissionRequirement>();
    }

    /// <summary>
    /// Creates a new presentation request builder for cross-device flow.
    /// </summary>
    /// <param name="clientId">The verifier's client identifier</param>
    /// <param name="responseUri">The URI where the wallet should POST the response</param>
    /// <returns>A new PresentationRequestBuilder instance</returns>
    public static PresentationRequestBuilder Create(string clientId, string responseUri)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(responseUri);
#else
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(clientId));
        if (string.IsNullOrWhiteSpace(responseUri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(responseUri));
#endif

        return new PresentationRequestBuilder(clientId, responseUri);
    }

    /// <summary>
    /// Sets the presentation definition name.
    /// </summary>
    /// <param name="name">Human-friendly name</param>
    /// <returns>This builder for method chaining</returns>
    public PresentationRequestBuilder WithName(string name)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
#else
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
#endif

        _definition.Name = name;
        return this;
    }

    /// <summary>
    /// Sets the presentation definition purpose.
    /// </summary>
    /// <param name="purpose">Purpose description</param>
    /// <returns>This builder for method chaining</returns>
    public PresentationRequestBuilder WithPurpose(string purpose)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
#else
        if (string.IsNullOrWhiteSpace(purpose))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(purpose));
#endif

        _definition.Purpose = purpose;
        return this;
    }

    /// <summary>
    /// Sets a custom nonce value.
    /// </summary>
    /// <param name="nonce">The nonce value</param>
    /// <returns>This builder for method chaining</returns>
    public PresentationRequestBuilder WithNonce(string nonce)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(nonce);
#else
        if (string.IsNullOrWhiteSpace(nonce))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(nonce));
#endif

        _request.Nonce = nonce;
        return this;
    }

    /// <summary>
    /// Sets the state parameter.
    /// </summary>
    /// <param name="state">The state value</param>
    /// <returns>This builder for method chaining</returns>
    public PresentationRequestBuilder WithState(string state)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
#else
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(state));
#endif

        _request.State = state;
        return this;
    }

    /// <summary>
    /// Configures the request to use the <c>direct_post.jwt</c> response mode.
    /// </summary>
    /// <returns>This builder for method chaining.</returns>
    public PresentationRequestBuilder UseDirectPostJwtResponseMode()
    {
        _request.ResponseMode = Oid4VpConstants.ResponseModes.DirectPostJwt;
        return this;
    }

    /// <summary>
    /// Requests a credential of a specific type.
    /// </summary>
    /// <param name="credentialType">The credential type (vct value)</param>
    /// <param name="purpose">Optional purpose for requesting this credential</param>
    /// <param name="name">Optional human-friendly name for the input descriptor</param>
    /// <returns>This builder for method chaining</returns>
    public PresentationRequestBuilder RequestCredential(
        string credentialType,
        string? purpose = null,
        string? name = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialType);
#else
        if (string.IsNullOrWhiteSpace(credentialType))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(credentialType));
#endif

        var descriptorId = $"input_{_inputDescriptors.Count + 1}";
        var descriptor = InputDescriptor.CreateForCredentialType(
            descriptorId, credentialType, name, purpose)
            .WithSdJwtFormat();

        _inputDescriptors.Add(descriptor);
        return this;
    }

    /// <summary>
    /// Requests a credential from a specific issuer.
    /// </summary>
    /// <param name="credentialType">The credential type (vct value)</param>
    /// <param name="issuer">The required issuer</param>
    /// <param name="purpose">Optional purpose for requesting this credential</param>
    /// <param name="name">Optional human-friendly name for the input descriptor</param>
    /// <returns>This builder for method chaining</returns>
    public PresentationRequestBuilder RequestCredentialFromIssuer(
        string credentialType,
        string issuer,
        string? purpose = null,
        string? name = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialType);
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
#else
        if (string.IsNullOrWhiteSpace(credentialType))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(credentialType));
        if (string.IsNullOrWhiteSpace(issuer))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(issuer));
#endif

        var descriptorId = $"input_{_inputDescriptors.Count + 1}";
        var descriptor = InputDescriptor.CreateForCredentialType(
            descriptorId, credentialType, name, purpose)
            .WithSdJwtFormat()
            .WithField(Field.CreateForIssuer(issuer, "Verify credential issuer"));

        _inputDescriptors.Add(descriptor);
        return this;
    }

    /// <summary>
    /// Adds a custom input descriptor.
    /// </summary>
    /// <param name="inputDescriptor">The input descriptor to add</param>
    /// <returns>This builder for method chaining</returns>
    public PresentationRequestBuilder WithInputDescriptor(InputDescriptor inputDescriptor)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(inputDescriptor);
#else
        if (inputDescriptor == null)
            throw new ArgumentNullException(nameof(inputDescriptor));
#endif

        _inputDescriptors.Add(inputDescriptor);
        return this;
    }

    /// <summary>
    /// Adds a custom field constraint to the last added input descriptor.
    /// </summary>
    /// <param name="field">The field constraint to add</param>
    /// <returns>This builder for method chaining</returns>
    public PresentationRequestBuilder WithField(Field field)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(field);
#else
        if (field == null)
            throw new ArgumentNullException(nameof(field));
#endif

        if (_inputDescriptors.Count == 0)
            throw new InvalidOperationException("No input descriptors available to add field to");

        var lastDescriptor = _inputDescriptors[_inputDescriptors.Count - 1];
        lastDescriptor.WithField(field);
        return this;
    }

    /// <summary>
    /// Requires all input descriptors to be satisfied.
    /// </summary>
    /// <param name="name">Optional name for the submission requirement</param>
    /// <param name="purpose">Optional purpose for the submission requirement</param>
    /// <returns>This builder for method chaining</returns>
    public PresentationRequestBuilder RequireAll(string? name = null, string? purpose = null)
    {
        if (_inputDescriptors.Count == 0)
            throw new InvalidOperationException("No input descriptors available to require");

        var ids = _inputDescriptors.Select(d => d.Id).ToArray();
        var requirement = SubmissionRequirement.RequireAll(ids, name, purpose);
        _submissionRequirements.Add(requirement);
        return this;
    }

    /// <summary>
    /// Requires a specific number of input descriptors to be satisfied.
    /// </summary>
    /// <param name="count">Number of descriptors to pick</param>
    /// <param name="name">Optional name for the submission requirement</param>
    /// <param name="purpose">Optional purpose for the submission requirement</param>
    /// <returns>This builder for method chaining</returns>
    public PresentationRequestBuilder RequirePick(int count, string? name = null, string? purpose = null)
    {
        if (_inputDescriptors.Count == 0)
            throw new InvalidOperationException("No input descriptors available to pick from");

        if (count <= 0 || count > _inputDescriptors.Count)
            throw new ArgumentException($"Count must be between 1 and {_inputDescriptors.Count}", nameof(count));

        var ids = _inputDescriptors.Select(d => d.Id).ToArray();
        var requirement = SubmissionRequirement.RequirePick(ids, count, name, purpose);
        _submissionRequirements.Add(requirement);
        return this;
    }

    /// <summary>
    /// Requires a range of input descriptors to be satisfied.
    /// </summary>
    /// <param name="min">Minimum number of descriptors</param>
    /// <param name="max">Maximum number of descriptors</param>
    /// <param name="name">Optional name for the submission requirement</param>
    /// <param name="purpose">Optional purpose for the submission requirement</param>
    /// <returns>This builder for method chaining</returns>
    public PresentationRequestBuilder RequirePickRange(
        int min,
        int max,
        string? name = null,
        string? purpose = null)
    {
        if (_inputDescriptors.Count == 0)
            throw new InvalidOperationException("No input descriptors available to pick from");

        if (min <= 0 || max > _inputDescriptors.Count || min > max)
            throw new ArgumentException("Invalid min/max range", nameof(min));

        var ids = _inputDescriptors.Select(d => d.Id).ToArray();
        var requirement = SubmissionRequirement.RequirePickRange(ids, min, max, name, purpose);
        _submissionRequirements.Add(requirement);
        return this;
    }

    /// <summary>
    /// Adds SD-JWT format restrictions to the presentation definition.
    /// </summary>
    /// <param name="algorithms">Optional array of allowed signing algorithms</param>
    /// <returns>This builder for method chaining</returns>
    public PresentationRequestBuilder WithSdJwtFormat(string[]? algorithms = null)
    {
        _definition.WithSdJwtFormat(algorithms);
        return this;
    }

    /// <summary>
    /// Builds the authorization request object.
    /// </summary>
    /// <returns>The completed AuthorizationRequest</returns>
    public AuthorizationRequest Build()
    {
        if (_inputDescriptors.Count == 0)
            throw new InvalidOperationException("At least one input descriptor is required");

        // Finalize the presentation definition
        _definition.InputDescriptors = _inputDescriptors.ToArray();

        if (_submissionRequirements.Count > 0)
        {
            _definition.SubmissionRequirements = _submissionRequirements.ToArray();
        }

        // Validate the request before returning
        _request.Validate();

        return _request;
    }

    /// <summary>
    /// Builds the authorization request as a URI suitable for QR codes.
    /// </summary>
    /// <returns>The OID4VP URI</returns>
    public string BuildUri()
    {
        var request = Build();
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var encodedRequest = Uri.EscapeDataString(json);
        return $"{Oid4VpConstants.AuthorizationRequestScheme}://?request={encodedRequest}";
    }

    /// <summary>
    /// Builds the authorization request as a URI suitable for QR codes using request_uri parameter.
    /// </summary>
    /// <param name="requestUri">URI where the request object can be retrieved</param>
    /// <returns>The OID4VP URI</returns>
    public string BuildUriWithRequestUri(string requestUri)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(requestUri);
#else
        if (string.IsNullOrWhiteSpace(requestUri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(requestUri));
#endif

        var encodedRequestUri = Uri.EscapeDataString(requestUri);
        return $"{Oid4VpConstants.AuthorizationRequestScheme}://?request_uri={encodedRequestUri}";
    }

    /// <summary>
    /// Gets the current nonce value.
    /// </summary>
    /// <returns>The nonce value</returns>
    public string GetNonce() => _request.Nonce;

    /// <summary>
    /// Gets the current state value.
    /// </summary>
    /// <returns>The state value, or null if not set</returns>
    public string? GetState() => _request.State;

    /// <summary>
    /// Generates a cryptographically secure nonce.
    /// </summary>
    /// <param name="length">Length in bytes (default: 32)</param>
    /// <returns>A base64url encoded nonce</returns>
    private static string GenerateNonce(int length = 32)
    {
        if (length <= 0)
            throw new ArgumentException("Length must be positive", nameof(length));

        var bytes = new byte[length];
#if NET6_0_OR_GREATER
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
#else
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
#endif

        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
