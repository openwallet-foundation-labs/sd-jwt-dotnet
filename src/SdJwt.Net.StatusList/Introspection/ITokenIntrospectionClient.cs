namespace SdJwt.Net.StatusList.Introspection;

/// <summary>
/// Interface for OAuth 2.0 Token Introspection per RFC 7662.
/// </summary>
public interface ITokenIntrospectionClient : IDisposable
{
    /// <summary>
    /// Introspects a token to determine its current state.
    /// </summary>
    /// <param name="token">The token to introspect.</param>
    /// <param name="tokenTypeHint">Optional hint about the type of the token (e.g., "access_token", "refresh_token").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The introspection result indicating token status and metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when token is null.</exception>
    /// <exception cref="ArgumentException">Thrown when token is empty.</exception>
    /// <exception cref="TokenIntrospectionException">Thrown when introspection fails.</exception>
    Task<IntrospectionResult> IntrospectAsync(
        string token,
        string? tokenTypeHint = null,
        CancellationToken cancellationToken = default);
}
