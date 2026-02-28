namespace SdJwt.Net.Issuer;

/// <summary>
/// Defines advanced options for creating an SD-JWT.
/// </summary>
public class SdIssuanceOptions
{
    /// <summary>
    /// The number of decoy digests to add to the top-level _sd array.
    /// This helps to obscure the true number of disclosable claims. Defaults to 0.
    /// </summary>
    public int DecoyDigests { get; set; } = 0;

    /// <summary>
    /// A structured object that mirrors the claims payload, used to specify which claims are disclosable.
    /// To make a claim disclosable, set its corresponding property to `true`.
    /// For arrays, use a list of booleans to specify disclosability by index.
    /// Example: `new { user_details = new { email = true }, roles = new[] { true, false } }`
    /// </summary>
    public object? DisclosureStructure
    {
        get; set;
    }

    /// <summary>
    /// If true, the entire claims object will be wrapped in a disclosable structure.
    /// This is useful for making all claims selectively disclosable by default.
    /// When true, <see cref="DisclosureStructure"/> is ignored. Defaults to false.
    /// </summary>
    public bool MakeAllClaimsDisclosable { get; set; } = false;
}
