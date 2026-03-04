namespace SdJwt.Net.Eudiw.TrustFramework;

/// <summary>
/// EU List of Trusted Lists (LOTL) structure.
/// </summary>
public class ListOfTrustedLists
{
    /// <summary>
    /// Sequence number/version of the LOTL.
    /// </summary>
    public int SequenceNumber
    {
        get; set;
    }

    /// <summary>
    /// Issue date of this LOTL version.
    /// </summary>
    public DateTimeOffset IssueDate
    {
        get; set;
    }

    /// <summary>
    /// Next scheduled update date.
    /// </summary>
    public DateTimeOffset NextUpdate
    {
        get; set;
    }

    /// <summary>
    /// Pointers to member state Trusted Lists.
    /// </summary>
    public IReadOnlyList<TrustedListPointer> TrustedLists { get; set; } = Array.Empty<TrustedListPointer>();
}
