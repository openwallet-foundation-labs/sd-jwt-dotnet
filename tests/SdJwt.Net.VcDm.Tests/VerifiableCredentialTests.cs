using FluentAssertions;
using SdJwt.Net.VcDm.Models;
using SdJwt.Net.VcDm.Serialization;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.VcDm.Tests;

public class VerifiableCredentialTests
{
    // -----------------------------------------------------------------------
    // Serialization roundtrip
    // -----------------------------------------------------------------------

    [Fact]
    public void Serialize_MinimalCredential_ProducesCorrectJson()
    {
        var vc = BuildMinimal();
        var json = JsonSerializer.Serialize(vc, VcDmSerializerOptions.Default);

        json.Should().Contain("\"@context\"");
        json.Should().Contain(VcDmContexts.V2);
        json.Should().Contain("\"VerifiableCredential\"");
        json.Should().Contain("\"issuer\"");
        json.Should().Contain("\"credentialSubject\"");
    }

    [Fact]
    public void Deserialize_MinimalCredential_Roundtrips()
    {
        var original = BuildMinimal();
        var json = JsonSerializer.Serialize(original, VcDmSerializerOptions.Default);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential>(json, VcDmSerializerOptions.Default);

        deserialized.Should().NotBeNull();
        deserialized!.Context.Should().Contain(VcDmContexts.V2);
        deserialized.Type.Should().Contain("VerifiableCredential");
        deserialized.Issuer.Id.Should().Be("https://example.edu/issuers/14");
        deserialized.CredentialSubject.Should().HaveCount(1);
        deserialized.CredentialSubject[0].Id.Should().Be("did:example:ebfeb1f712ebc6f1c276e12ec21");
    }

    [Fact]
    public void Serialize_ValidFrom_IsIso8601String()
    {
        var vc = BuildMinimal();
        vc.ValidFrom = new DateTimeOffset(2024, 1, 15, 9, 0, 0, TimeSpan.Zero);

        var json = JsonSerializer.Serialize(vc, VcDmSerializerOptions.Default);

        json.Should().Contain("\"validFrom\"");
        json.Should().Contain("2024-01-15T09:00:00Z");
    }

    [Fact]
    public void Serialize_DoesNotWrite_IssuanceDateOrExpirationDate()
    {
        var vc = BuildMinimal();
        vc.ValidFrom = DateTimeOffset.UtcNow;
        vc.ValidUntil = DateTimeOffset.UtcNow.AddYears(1);

        var json = JsonSerializer.Serialize(vc, VcDmSerializerOptions.Default);

        json.Should().NotContain("issuanceDate");
        json.Should().NotContain("expirationDate");
        json.Should().Contain("validFrom");
        json.Should().Contain("validUntil");
    }

    [Fact]
    public void Deserialize_Vcdm11IssuanceDate_MapsToValidFrom()
    {
        var json = """
            {
              "@context": ["https://www.w3.org/ns/credentials/v2"],
              "type": ["VerifiableCredential"],
              "issuer": "https://example.com",
              "credentialSubject": {},
              "issuanceDate": "2023-06-01T00:00:00Z"
            }
            """;

        var vc = JsonSerializer.Deserialize<VerifiableCredential>(json, VcDmSerializerOptions.Default);

        vc!.ValidFrom.Should().NotBeNull();
        vc.ValidFrom!.Value.Year.Should().Be(2023);
    }

    [Fact]
    public void Deserialize_Vcdm11ExpirationDate_MapsToValidUntil()
    {
        var json = """
            {
              "@context": ["https://www.w3.org/ns/credentials/v2"],
              "type": ["VerifiableCredential"],
              "issuer": "https://example.com",
              "credentialSubject": {},
              "expirationDate": "2025-12-31T23:59:59Z"
            }
            """;

        var vc = JsonSerializer.Deserialize<VerifiableCredential>(json, VcDmSerializerOptions.Default);

        vc!.ValidUntil.Should().NotBeNull();
        vc.ValidUntil!.Value.Year.Should().Be(2025);
    }

    // -----------------------------------------------------------------------
    // CredentialStatus
    // -----------------------------------------------------------------------

    [Fact]
    public void Serialize_BitstringStatusListEntry_HasCorrectType()
    {
        var vc = BuildMinimal();
        vc.CredentialStatus =
        [
            new BitstringStatusListEntry
            {
                Id = "https://example.com/status/3#94567",
                StatusPurpose = "revocation",
                StatusListIndex = "94567",
                StatusListCredential = "https://example.com/status/3"
            }
        ];

        var json = JsonSerializer.Serialize(vc, VcDmSerializerOptions.Default);

        json.Should().Contain("BitstringStatusListEntry");
        json.Should().Contain("94567");
    }

    [Fact]
    public void Deserialize_BitstringStatusListEntry_RoundTrips()
    {
        var json = """
            {
              "@context": ["https://www.w3.org/ns/credentials/v2"],
              "type": ["VerifiableCredential"],
              "issuer": "https://example.com",
              "credentialSubject": {},
              "credentialStatus": {
                "id": "https://example.com/status/1#42",
                "type": "BitstringStatusListEntry",
                "statusPurpose": "revocation",
                "statusListIndex": "42",
                "statusListCredential": "https://example.com/status/1"
              }
            }
            """;

        var vc = JsonSerializer.Deserialize<VerifiableCredential>(json, VcDmSerializerOptions.Default);

        vc!.CredentialStatus.Should().HaveCount(1);
        vc.CredentialStatus![0].Should().BeOfType<BitstringStatusListEntry>();
        var entry = (BitstringStatusListEntry)vc.CredentialStatus[0];
        entry.StatusListIndex.Should().Be("42");
        entry.StatusPurpose.Should().Be("revocation");
    }

    [Fact]
    public void Deserialize_StatusList2021Entry_MapsToLegacyType()
    {
        var json = """
            {
              "@context": ["https://www.w3.org/ns/credentials/v2"],
              "type": ["VerifiableCredential"],
              "issuer": "https://example.com",
              "credentialSubject": {},
              "credentialStatus": {
                "type": "StatusList2021Entry",
                "statusPurpose": "suspension",
                "statusListIndex": "7",
                "statusListCredential": "https://example.com/status/old"
              }
            }
            """;

        var vc = JsonSerializer.Deserialize<VerifiableCredential>(json, VcDmSerializerOptions.Default);

#pragma warning disable CS0618
        vc!.CredentialStatus![0].Should().BeOfType<StatusList2021Entry>();
#pragma warning restore CS0618
    }

    [Fact]
    public void Deserialize_UnknownStatusType_MapsToUnknownCredentialStatus()
    {
        var json = """
            {
              "@context": ["https://www.w3.org/ns/credentials/v2"],
              "type": ["VerifiableCredential"],
              "issuer": "https://example.com",
              "credentialSubject": {},
              "credentialStatus": { "type": "CustomStatusType2099", "foo": "bar" }
            }
            """;

        var vc = JsonSerializer.Deserialize<VerifiableCredential>(json, VcDmSerializerOptions.Default);

        vc!.CredentialStatus![0].Should().BeOfType<UnknownCredentialStatus>();
        vc.CredentialStatus[0].Type.Should().Be("CustomStatusType2099");
    }

    // -----------------------------------------------------------------------
    // Issuer (string vs object)
    // -----------------------------------------------------------------------

    [Fact]
    public void Deserialize_IssuerAsString_CreatesSimpleIssuer()
    {
        var json = """
            {
              "@context": ["https://www.w3.org/ns/credentials/v2"],
              "type": ["VerifiableCredential"],
              "issuer": "https://example.com/issuers/1",
              "credentialSubject": {}
            }
            """;

        var vc = JsonSerializer.Deserialize<VerifiableCredential>(json, VcDmSerializerOptions.Default);

        vc!.Issuer.Id.Should().Be("https://example.com/issuers/1");
        vc.Issuer.IsSimpleUrl.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_IssuerAsObject_CreatesRichIssuer()
    {
        var json = """
            {
              "@context": ["https://www.w3.org/ns/credentials/v2"],
              "type": ["VerifiableCredential"],
              "issuer": { "id": "https://example.com/issuers/1", "name": "Example University" },
              "credentialSubject": {}
            }
            """;

        var vc = JsonSerializer.Deserialize<VerifiableCredential>(json, VcDmSerializerOptions.Default);

        vc!.Issuer.Id.Should().Be("https://example.com/issuers/1");
        vc.Issuer.Name.Should().Be("Example University");
        vc.Issuer.IsSimpleUrl.Should().BeFalse();
    }

    [Fact]
    public void Serialize_SimpleIssuer_WritesStringNotObject()
    {
        var vc = BuildMinimal();
        var json = JsonSerializer.Serialize(vc, VcDmSerializerOptions.Default);

        // Should write "issuer": "https://..." not "issuer": { "id": ... }
        json.Should().Contain("\"issuer\":\"https://example.edu/issuers/14\"");
    }

    [Fact]
    public void Serialize_RichIssuer_WritesObject()
    {
        var vc = BuildMinimal();
        vc.Issuer = new Issuer("https://example.com") { Name = "Example Corp" };
        var json = JsonSerializer.Serialize(vc, VcDmSerializerOptions.Default);

        json.Should().Contain("\"id\":\"https://example.com\"");
        json.Should().Contain("\"name\":\"Example Corp\"");
    }

    // -----------------------------------------------------------------------
    // CredentialSubject single vs array
    // -----------------------------------------------------------------------

    [Fact]
    public void Deserialize_SingleCredentialSubject_NormalizesToArray()
    {
        var json = """
            {
              "@context": ["https://www.w3.org/ns/credentials/v2"],
              "type": ["VerifiableCredential"],
              "issuer": "https://example.com",
              "credentialSubject": { "id": "did:example:abc", "name": "Alice" }
            }
            """;

        var vc = JsonSerializer.Deserialize<VerifiableCredential>(json, VcDmSerializerOptions.Default);

        vc!.CredentialSubject.Should().HaveCount(1);
        vc.CredentialSubject[0].Id.Should().Be("did:example:abc");
    }

    [Fact]
    public void Deserialize_ArrayCredentialSubject_ParsesAll()
    {
        var json = """
            {
              "@context": ["https://www.w3.org/ns/credentials/v2"],
              "type": ["VerifiableCredential"],
              "issuer": "https://example.com",
              "credentialSubject": [
                { "id": "did:example:alice" },
                { "id": "did:example:bob" }
              ]
            }
            """;

        var vc = JsonSerializer.Deserialize<VerifiableCredential>(json, VcDmSerializerOptions.Default);

        vc!.CredentialSubject.Should().HaveCount(2);
    }

    // -----------------------------------------------------------------------
    // Full credential with all optional fields
    // -----------------------------------------------------------------------

    [Fact]
    public void Serialize_FullCredential_AllFieldsPresent()
    {
        var vc = BuildFull();
        var json = JsonSerializer.Serialize(vc, VcDmSerializerOptions.Default);

        json.Should().Contain("credentialStatus");
        json.Should().Contain("credentialSchema");
        json.Should().Contain("termsOfUse");
        json.Should().Contain("evidence");
        json.Should().Contain("UniversityDegreeCredential");
    }

    [Fact]
    public void Deserialize_FullCredential_Roundtrips()
    {
        var original = BuildFull();
        var json = JsonSerializer.Serialize(original, VcDmSerializerOptions.Default);
        var restored = JsonSerializer.Deserialize<VerifiableCredential>(json, VcDmSerializerOptions.Default);

        restored.Should().NotBeNull();
        restored!.Type.Should().Contain("UniversityDegreeCredential");
        restored.CredentialStatus.Should().HaveCount(1);
        restored.CredentialSchema.Should().HaveCount(1);
        restored.TermsOfUse.Should().HaveCount(1);
        restored.Evidence.Should().HaveCount(1);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static VerifiableCredential BuildMinimal() => new()
    {
        Context = [VcDmContexts.V2],
        Type = ["VerifiableCredential"],
        Issuer = new Issuer("https://example.edu/issuers/14"),
        CredentialSubject =
        [
            new CredentialSubject { Id = "did:example:ebfeb1f712ebc6f1c276e12ec21" }
        ]
    };

    private static VerifiableCredential BuildFull() => new()
    {
        Context = [VcDmContexts.V2, "https://schema.org/"],
        Type = ["VerifiableCredential", "UniversityDegreeCredential"],
        Id = "https://example.edu/credentials/3732",
        Issuer = new Issuer("https://example.edu/issuers/14") { Name = "Example University" },
        ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
        ValidUntil = new DateTimeOffset(2028, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CredentialSubject =
        [
            new CredentialSubject
            {
                Id = "did:example:ebfeb1f712ebc6f1c276e12ec21",
                AdditionalClaims = new Dictionary<string, object>
                {
                    ["degree"] = "BachelorDegree"
                }
            }
        ],
        CredentialStatus =
        [
            new BitstringStatusListEntry
            {
                Id = "https://example.edu/status/1#42",
                StatusPurpose = "revocation",
                StatusListIndex = "42",
                StatusListCredential = "https://example.edu/status/1"
            }
        ],
        CredentialSchema =
        [
            new CredentialSchema
            {
                Id = "https://example.edu/schemas/degree.json",
                Type = "JsonSchema"
            }
        ],
        TermsOfUse =
        [
            new TermsOfUse { Type = "TrustFrameworkPolicy", Id = "https://policy.example.com/1" }
        ],
        Evidence =
        [
            new Evidence
            {
                Id = "https://example.edu/evidence/001",
                Type = ["DocumentVerification"]
            }
        ]
    };
}
