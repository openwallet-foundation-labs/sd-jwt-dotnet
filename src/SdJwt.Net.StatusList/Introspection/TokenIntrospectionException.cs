namespace SdJwt.Net.StatusList.Introspection;

/// <summary>
/// Exception thrown when token introspection fails.
/// </summary>
public class TokenIntrospectionException : Exception
{
    /// <summary>
    /// HTTP status code returned by the introspection endpoint, if available.
    /// </summary>
    public int? HttpStatusCode
    {
        get;
    }

    /// <summary>
    /// The introspection endpoint that was called.
    /// </summary>
    public string? EndpointUri
    {
        get;
    }

    /// <summary>
    /// Error code returned by the introspection endpoint, if available.
    /// </summary>
    public string? ErrorCode
    {
        get;
    }

    /// <summary>
    /// Creates a new instance of TokenIntrospectionException.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TokenIntrospectionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new instance of TokenIntrospectionException with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TokenIntrospectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Creates a new instance of TokenIntrospectionException with HTTP details.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="httpStatusCode">The HTTP status code.</param>
    /// <param name="endpointUri">The endpoint URI that was called.</param>
    /// <param name="errorCode">Optional error code from the response.</param>
    public TokenIntrospectionException(
        string message,
        int httpStatusCode,
        string? endpointUri = null,
        string? errorCode = null) : base(message)
    {
        HttpStatusCode = httpStatusCode;
        EndpointUri = endpointUri;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates a new instance of TokenIntrospectionException with HTTP details and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="httpStatusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="endpointUri">The endpoint URI that was called.</param>
    /// <param name="errorCode">Optional error code from the response.</param>
    public TokenIntrospectionException(
        string message,
        int httpStatusCode,
        Exception innerException,
        string? endpointUri = null,
        string? errorCode = null) : base(message, innerException)
    {
        HttpStatusCode = httpStatusCode;
        EndpointUri = endpointUri;
        ErrorCode = errorCode;
    }
}
