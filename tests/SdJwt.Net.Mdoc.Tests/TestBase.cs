using System.Security.Cryptography;

namespace SdJwt.Net.Mdoc.Tests;

/// <summary>
/// Base class for mdoc tests with shared test infrastructure.
/// </summary>
public abstract class TestBase : IDisposable
{
    /// <summary>
    /// EC P-256 issuer signing key for tests.
    /// </summary>
    protected ECDsa IssuerSigningKey { get; }

    /// <summary>
    /// EC P-256 device key for holder binding tests.
    /// </summary>
    protected ECDsa DeviceKey { get; }

    /// <summary>
    /// EC P-384 issuer signing key for ES384 tests.
    /// </summary>
    protected ECDsa IssuerSigningKeyEs384 { get; }

    /// <summary>
    /// Fixed test document type for mDL.
    /// </summary>
    protected const string MdlDocType = "org.iso.18013.5.1.mDL";

    /// <summary>
    /// Fixed test namespace for mDL.
    /// </summary>
    protected const string MdlNamespace = "org.iso.18013.5.1";

    protected TestBase()
    {
        IssuerSigningKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        DeviceKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        IssuerSigningKeyEs384 = ECDsa.Create(ECCurve.NamedCurves.nistP384);
    }

    /// <summary>
    /// Creates test mDL data elements.
    /// </summary>
    protected static Dictionary<string, object> CreateTestMdlElements()
    {
        return new Dictionary<string, object>
        {
            ["family_name"] = "Doe",
            ["given_name"] = "John",
            ["birth_date"] = "1990-01-15",
            ["issue_date"] = "2024-01-01",
            ["expiry_date"] = "2029-01-01",
            ["issuing_country"] = "US",
            ["issuing_authority"] = "State DMV",
            ["document_number"] = "DL123456789",
            ["driving_privileges"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["vehicle_category_code"] = "B",
                    ["issue_date"] = "2024-01-01",
                    ["expiry_date"] = "2029-01-01"
                }
            },
            ["age_over_18"] = true,
            ["age_over_21"] = true
        };
    }

    public void Dispose()
    {
        IssuerSigningKey.Dispose();
        DeviceKey.Dispose();
        IssuerSigningKeyEs384.Dispose();
        GC.SuppressFinalize(this);
    }
}
