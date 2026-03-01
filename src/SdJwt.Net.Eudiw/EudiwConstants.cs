namespace SdJwt.Net.Eudiw;

/// <summary>
/// Constants for EU Digital Identity Wallet ecosystem per eIDAS 2.0 ARF.
/// </summary>
public static class EudiwConstants
{
    /// <summary>
    /// Person Identification Data (PID) constants per EUDIW ARF.
    /// </summary>
    public static class Pid
    {
        /// <summary>
        /// DocType for PID credentials.
        /// </summary>
        public const string DocType = "eu.europa.ec.eudi.pid.1";

        /// <summary>
        /// Namespace for PID data elements.
        /// </summary>
        public const string Namespace = "eu.europa.ec.eudi.pid.1";

        /// <summary>
        /// Mandatory claims required in PID credentials per ARF.
        /// </summary>
        public static readonly string[] MandatoryClaims = new[]
        {
            "family_name",
            "given_name",
            "birth_date",
            "issuance_date",
            "expiry_date",
            "issuing_authority",
            "issuing_country"
        };

        /// <summary>
        /// Optional claims that may be present in PID credentials.
        /// </summary>
        public static readonly string[] OptionalClaims = new[]
        {
            "age_over_18",
            "age_over_21",
            "nationality",
            "resident_address",
            "birth_place",
            "gender",
            "portrait"
        };
    }

    /// <summary>
    /// Mobile Driving License (mDL) constants per EUDIW ARF.
    /// </summary>
    public static class Mdl
    {
        /// <summary>
        /// DocType for EU mDL credentials.
        /// </summary>
        public const string DocType = "org.iso.18013.5.1.mDL";

        /// <summary>
        /// Namespace for mDL data elements.
        /// </summary>
        public const string Namespace = "org.iso.18013.5.1";
    }

    /// <summary>
    /// Trust list constants for EU LOTL.
    /// </summary>
    public static class TrustList
    {
        /// <summary>
        /// EU List of Trusted Lists (LOTL) URL (XML format).
        /// </summary>
        public const string LotlUrl = "https://ec.europa.eu/tools/lotl/eu-lotl.xml";

        /// <summary>
        /// Alternative LOTL URL for JSON format.
        /// </summary>
        public const string LotlJsonUrl = "https://eudi.ec.europa.eu/trust/lotl.json";
    }

    /// <summary>
    /// ARF-mandated cryptographic algorithms.
    /// </summary>
    public static class Algorithms
    {
        /// <summary>
        /// Default signature algorithm (HAIP Level 2 minimum).
        /// </summary>
        public const string SignatureAlgorithm = "ES256";

        /// <summary>
        /// Digest algorithm.
        /// </summary>
        public const string DigestAlgorithm = "SHA-256";

        /// <summary>
        /// Supported signature algorithms per ARF/HAIP.
        /// </summary>
        public static readonly string[] SupportedAlgorithms = new[]
        {
            "ES256",
            "ES384",
            "ES512"
        };
    }

    /// <summary>
    /// EU Member State codes.
    /// </summary>
    public static class MemberStates
    {
        /// <summary>
        /// All 27 EU member state ISO 3166-1 alpha-2 codes.
        /// </summary>
        public static readonly string[] All = new[]
        {
            "AT", // Austria
            "BE", // Belgium
            "BG", // Bulgaria
            "HR", // Croatia
            "CY", // Cyprus
            "CZ", // Czech Republic
            "DK", // Denmark
            "EE", // Estonia
            "FI", // Finland
            "FR", // France
            "DE", // Germany
            "GR", // Greece
            "HU", // Hungary
            "IE", // Ireland
            "IT", // Italy
            "LV", // Latvia
            "LT", // Lithuania
            "LU", // Luxembourg
            "MT", // Malta
            "NL", // Netherlands
            "PL", // Poland
            "PT", // Portugal
            "RO", // Romania
            "SK", // Slovakia
            "SI", // Slovenia
            "ES", // Spain
            "SE"  // Sweden
        };
    }

    /// <summary>
    /// Qualified Electronic Attestation of Attributes (QEAA) constants.
    /// </summary>
    public static class Qeaa
    {
        /// <summary>
        /// Verifiable Credential Type prefix for QEAA.
        /// </summary>
        public const string VctPrefix = "urn:eu:europa:ec:eudi:qeaa:";
    }

    /// <summary>
    /// Electronic Attestation of Attributes (EAA) constants.
    /// </summary>
    public static class Eaa
    {
        /// <summary>
        /// Verifiable Credential Type prefix for EAA.
        /// </summary>
        public const string VctPrefix = "urn:eu:europa:ec:eudi:eaa:";
    }
}
