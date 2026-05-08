using SdJwt.Net.Samples.Shared;
using SdJwt.Net.VcDm.Models;
using SdJwt.Net.VcDm.Serialization;
using SdJwt.Net.VcDm.Validation;
using System.Text.Json;

namespace SdJwt.Net.Samples.Intermediate;

/// <summary>
/// Tutorial 07: W3C Verifiable Credentials Data Model 2.0
///
/// LEARNING OBJECTIVES:
/// - Understand the difference between VCDM 2.0 (jwt_vc_json / ldp_vc) and SD-JWT VC (dc+sd-jwt)
/// - Build VerifiableCredential and VerifiablePresentation models
/// - Use credentialStatus (BitstringStatusListEntry)
/// - Read VCDM 1.1 backward-compatible properties
/// - Validate credential structure with VcDmValidator
///
/// TIME: ~15 minutes
/// </summary>
public static class W3cVerifiableCredentials
{
    public static Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 07: W3C Verifiable Credentials Data Model 2.0");

        Console.WriteLine("VCDM 2.0 is the W3C standard for credential structure.");
        Console.WriteLine("SdJwt.Net.VcDm provides typed models for jwt_vc_json and ldp_vc formats.");
        Console.WriteLine();

        // =====================================================================
        // STEP 1: VCDM 2.0 vs SD-JWT VC — the format map
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "Credential Format Map");

        Console.WriteLine("OID4VCI defines four credential formats:");
        Console.WriteLine();
        Console.WriteLine("  dc+sd-jwt    → SdJwt.Net.Vc     (IETF SD-JWT VC, no JSON-LD)");
        Console.WriteLine("  mso_mdoc     → SdJwt.Net.Mdoc   (ISO 18013-5, CBOR)");
        Console.WriteLine("  jwt_vc_json  → SdJwt.Net.VcDm   (W3C VCDM 2.0 payload in JWT)");
        Console.WriteLine("  ldp_vc       → SdJwt.Net.VcDm   (W3C VCDM 2.0 + Data Integrity proof)");
        Console.WriteLine();
        Console.WriteLine("KEY DISTINCTION:");
        Console.WriteLine("  dc+sd-jwt uses 'vct' (Verifiable Digital Credential Type)");
        Console.WriteLine("  jwt_vc_json uses '@context' + 'type[]' (JSON-LD semantics)");
        Console.WriteLine("  These are DIFFERENT specs with DIFFERENT data models.");

        // =====================================================================
        // STEP 2: Build a minimal VerifiableCredential
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "Build a minimal VerifiableCredential");

        var credential = new VerifiableCredential
        {
            // @context: first entry MUST be the VCDM 2.0 URL
            Context = [VcDmContexts.V2, "https://schema.org/"],

            // type: MUST contain "VerifiableCredential"
            Type = ["VerifiableCredential", "UniversityDegreeCredential"],

            // Unique credential identifier (optional)
            Id = "https://example.edu/credentials/3732",

            // Issuer: plain string URL or object with id/name/description
            Issuer = new Issuer("https://example.edu/issuers/14") { Name = "Example University" },

            // validFrom replaces VCDM 1.1 issuanceDate
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ValidUntil = new DateTimeOffset(2028, 1, 1, 0, 0, 0, TimeSpan.Zero),

            // credentialSubject: subject identifier + domain claims
            CredentialSubject =
            [
                new CredentialSubject
                {
                    Id = "did:example:ebfeb1f712ebc6f1c276e12ec21",
                    AdditionalClaims = new Dictionary<string, object>
                    {
                        ["degree"] = new { type = "BachelorDegree", name = "Bachelor of Science" },
                        ["alumniOf"] = "Example University"
                    }
                }
            ]
        };

        ConsoleHelpers.PrintSuccess("VerifiableCredential built");
        ConsoleHelpers.PrintKeyValue("@context[0]", credential.Context[0]);
        ConsoleHelpers.PrintKeyValue("type", string.Join(", ", credential.Type));
        ConsoleHelpers.PrintKeyValue("issuer.id", credential.Issuer.Id);
        ConsoleHelpers.PrintKeyValue("issuer.name", credential.Issuer.Name ?? "(none)");
        ConsoleHelpers.PrintKeyValue("validFrom", credential.ValidFrom?.ToString("O") ?? "(none)");

        // =====================================================================
        // STEP 3: Add credentialStatus (revocation)
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "Add BitstringStatusListEntry for revocation");

        credential.CredentialStatus =
        [
            new BitstringStatusListEntry
            {
                Id = "https://example.edu/status/3#94567",
                StatusPurpose = "revocation",
                StatusListIndex = "94567",
                StatusListCredential = "https://example.edu/status/3"
            }
        ];

        Console.WriteLine("BitstringStatusListEntry (VCDM 2.0) replaces StatusList2021Entry (VCDM 1.1).");
        Console.WriteLine();
        Console.WriteLine("  statusListCredential: URL of the bitstring status list VC");
        Console.WriteLine("  statusListIndex:      Index into the compressed bitstring");
        Console.WriteLine("  statusPurpose:        'revocation' or 'suspension'");

        // =====================================================================
        // STEP 4: Add credentialSchema, termsOfUse, evidence
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "Add credentialSchema, termsOfUse, and evidence");

        credential.CredentialSchema =
        [
            new CredentialSchema
            {
                Id = "https://example.edu/schemas/degree.json",
                Type = "JsonSchema"
            }
        ];

        credential.TermsOfUse =
        [
            new TermsOfUse
            {
                Type = "TrustFrameworkPolicy",
                Id = "https://policy.example.com/edu/v1"
            }
        ];

        credential.Evidence =
        [
            new Evidence
            {
                Id = "https://example.edu/evidence/001",
                Type = ["DocumentVerification"],
                AdditionalProperties = new Dictionary<string, object>
                {
                    ["verifier"] = "https://example.edu/registrar",
                    ["evidenceDocument"] = "DegreeApplication",
                    ["subjectPresence"] = "Physical"
                }
            }
        ];

        ConsoleHelpers.PrintSuccess("Optional fields added");

        // =====================================================================
        // STEP 5: Validate the credential structure
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "Validate with VcDmValidator");

        var validator = new VcDmValidator();
        var validResult = validator.Validate(credential);

        if (validResult.IsValid)
        {
            ConsoleHelpers.PrintSuccess("Credential passes structural validation");
        }
        else
        {
            Console.WriteLine("Validation errors:");
            foreach (var error in validResult.Errors)
                Console.WriteLine($"  - {error}");
        }

        // Show what a broken credential looks like
        var broken = new VerifiableCredential
        {
            Context = ["https://wrong-context.example.com"],  // wrong base context
            Type = ["SomeType"],                               // missing VerifiableCredential
            Issuer = null!,                                    // missing issuer
            CredentialSubject = []                             // empty subject
        };

        var brokenResult = validator.Validate(broken);
        Console.WriteLine();
        Console.WriteLine($"Broken credential has {brokenResult.Errors.Count} errors:");
        foreach (var error in brokenResult.Errors)
            Console.WriteLine($"  - {error}");

        // =====================================================================
        // STEP 6: Serialize to JSON
        // =====================================================================
        ConsoleHelpers.PrintStep(6, "Serialize to JSON with VcDmSerializerOptions");

        var json = JsonSerializer.Serialize(credential, new JsonSerializerOptions(VcDmSerializerOptions.Default)
        {
            WriteIndented = true
        });

        Console.WriteLine("Credential JSON (first 600 chars):");
        Console.WriteLine(json.Length > 600 ? json[..600] + "\n..." : json);

        // =====================================================================
        // STEP 7: Read a VCDM 1.1 credential (backward compatibility)
        // =====================================================================
        ConsoleHelpers.PrintStep(7, "Read a VCDM 1.1 credential (backward compatibility)");

        var vcdm11Json = """
            {
              "@context": ["https://www.w3.org/2018/credentials/v1"],
              "type": ["VerifiableCredential", "AlumniCredential"],
              "issuer": "https://example.edu",
              "issuanceDate": "2023-01-01T00:00:00Z",
              "expirationDate": "2027-01-01T00:00:00Z",
              "credentialSubject": {
                "id": "did:example:alice",
                "alumniOf": "Example University"
              }
            }
            """;

        var legacyCredential = JsonSerializer.Deserialize<VerifiableCredential>(
            vcdm11Json, VcDmSerializerOptions.Default);

        Console.WriteLine("VCDM 1.1 properties automatically mapped to VCDM 2.0:");
        ConsoleHelpers.PrintKeyValue("issuanceDate → validFrom", legacyCredential!.ValidFrom?.ToString("O") ?? "(none)");
        ConsoleHelpers.PrintKeyValue("expirationDate → validUntil", legacyCredential.ValidUntil?.ToString("O") ?? "(none)");
        Console.WriteLine();
        Console.WriteLine("Note: The library accepts v1 @context but warns — use v2 for new credentials.");

        // =====================================================================
        // STEP 8: Build a VerifiablePresentation (ldp_vc flow)
        // =====================================================================
        ConsoleHelpers.PrintStep(8, "Build a VerifiablePresentation for ldp_vc");

        var presentation = new VerifiablePresentation
        {
            Context = [VcDmContexts.V2],
            Type = ["VerifiablePresentation"],
            Id = "https://example.com/presentations/abc123",
            VerifiableCredential =
            [
                // In ldp_vc flow this would be a JSON-LD credential object (not a JWT string)
                // For demonstration we use a placeholder; in production a full VC object goes here
                "<<serialized-ldp-vc-json-object>>"
            ],
            // The proof binds this presentation to the verifier
            // challenge = OID4VP request nonce; domain = verifier client_id
            Proof =
            [
                new DataIntegrityProof
                {
                    Cryptosuite = "ecdsa-rdfc-2019",
                    ProofPurpose = "authentication",
                    VerificationMethod = "did:example:alice#key-1",
                    Challenge = "nonce-from-oid4vp-request",
                    Domain = "https://verifier.example.com",
                    Created = "2024-01-15T09:00:00Z",
                    ProofValue = "z3FXQjecWufY46yg..."
                }
            ]
        };

        var vpValidation = validator.Validate(presentation);
        Console.WriteLine();
        ConsoleHelpers.PrintSuccess(vpValidation.IsValid
            ? "VerifiablePresentation passes validation"
            : $"VP validation failed: {string.Join(", ", vpValidation.Errors)}");

        Console.WriteLine();
        Console.WriteLine("OID4VP VP Token encoding for ldp_vc:");
        Console.WriteLine("  vp_token = { \"my_credential_id\": [\"<serialized-VP-JSON>\"] }");
        Console.WriteLine("  where the key is the DCQL credential query id");

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 07: W3C Verifiable Credentials Data Model 2.0", new[]
        {
            "Understood the VCDM 2.0 vs SD-JWT VC format distinction",
            "Built a VerifiableCredential with all optional fields",
            "Added BitstringStatusListEntry for revocation",
            "Validated credential structure with VcDmValidator",
            "Serialized/deserialized to/from JSON",
            "Read VCDM 1.1 issuanceDate/expirationDate for backward compat",
            "Built a VerifiablePresentation with Data Integrity proof"
        });

        Console.WriteLine();
        Console.WriteLine("NEXT: Tutorial 08 — OID4VCI with multi-format credential issuance");
        return Task.CompletedTask;
    }
}
