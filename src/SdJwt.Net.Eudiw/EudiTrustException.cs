namespace SdJwt.Net.Eudiw;

/// <summary>
/// Exception thrown for EUDI Trust validation failures.
/// </summary>
public class EudiTrustException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EudiTrustException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public EudiTrustException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EudiTrustException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public EudiTrustException(string message, Exception innerException)
        : base(message, innerException) { }
}
