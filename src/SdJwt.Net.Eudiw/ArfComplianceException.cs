namespace SdJwt.Net.Eudiw;

/// <summary>
/// Exception thrown for ARF compliance violations.
/// </summary>
public class ArfComplianceException : Exception
{
    /// <summary>
    /// Gets the list of ARF violations.
    /// </summary>
    public IReadOnlyList<string> Violations
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArfComplianceException"/> class.
    /// </summary>
    /// <param name="violations">The list of ARF compliance violations.</param>
    public ArfComplianceException(IReadOnlyList<string> violations)
        : base($"ARF compliance violations: {string.Join(", ", violations)}")
    {
        Violations = violations;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArfComplianceException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ArfComplianceException(string message)
        : base(message)
    {
        Violations = new[] { message };
    }
}
