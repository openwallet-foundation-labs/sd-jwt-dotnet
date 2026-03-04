namespace SdJwt.Net.Eudiw.Credentials;

/// <summary>
/// Exception thrown when PID credential validation fails.
/// </summary>
public class PidValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PidValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public PidValidationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PidValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public PidValidationException(string message, Exception innerException) : base(message, innerException) { }
}
