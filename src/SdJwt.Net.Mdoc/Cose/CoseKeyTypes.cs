namespace SdJwt.Net.Mdoc.Cose;

/// <summary>
/// COSE key type identifiers per RFC 8152.
/// </summary>
public enum CoseKeyType
{
    /// <summary>Octet Key Pair (OKP) - Ed25519, X25519.</summary>
    OKP = 1,

    /// <summary>Elliptic Curve Keys with x,y coordinates - ECDSA.</summary>
    EC2 = 2,

    /// <summary>Symmetric key.</summary>
    Symmetric = 4
}

/// <summary>
/// COSE curve identifiers per RFC 8152.
/// </summary>
public enum CoseCurve
{
    /// <summary>NIST P-256 curve.</summary>
    P256 = 1,

    /// <summary>NIST P-384 curve.</summary>
    P384 = 2,

    /// <summary>NIST P-521 curve.</summary>
    P521 = 3,

    /// <summary>X25519 for ECDH.</summary>
    X25519 = 4,

    /// <summary>X448 for ECDH.</summary>
    X448 = 5,

    /// <summary>Ed25519 for EdDSA signatures.</summary>
    Ed25519 = 6,

    /// <summary>Ed448 for EdDSA signatures.</summary>
    Ed448 = 7
}
