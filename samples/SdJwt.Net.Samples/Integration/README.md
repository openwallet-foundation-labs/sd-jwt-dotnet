# Integration & Advanced Patterns

This guide demonstrates how to combine multiple SD-JWT .NET packages to build sophisticated, production-ready applications. Learn powerful patterns for integrating standards, managing trust chains, and implementing complex real-world scenarios.

## What You'll Learn

- Combine RFC 9901 (Core SD-JWT) with industry standards
- Build multi-package workflows across issuance, presentation, and trust
- Implement production patterns with error handling and security validation
- Design scalable architectures for enterprise use cases

## Overview of Packages

The SD-JWT .NET ecosystem includes 8 specialized packages working together:

```
┌─────────────────────────────────────────────────────────────┐
│                    Core SD-JWT (RFC 9901)                   │
│         Selective Disclosure • Key Binding • Hashing         │
└────────┬────────────────────────────────────────────────────┘
         │
    ┌────┴───────────────────────────┐
    │                                 │
┌───▼────────────────┐      ┌────────▼────────────────┐
│  Credential Layer  │      │   Protocol & Trust      │
│  ─────────────────  │      │   ──────────────────    │
│ • Vc (W3C VC)     │      │ • Oid4Vci (Issuance)   │
│ • StatusList      │      │ • Oid4Vp (Present)     │
│                   │      │ • OidFederation (Trust) │
│                   │      │ • PresentationExchange  │
└───────────────────┘      └────────┬────────────────┘
                                    │
                           ┌────────▼────────────┐
                           │    HAIP Assurance   │
                           │  (Gov't Grade)      │
                           │ • 3 Security Levels │
                           │ • Compliance Matrix │
                           └─────────────────────┘
```

## Integration Patterns

### Pattern 1: Issuance Workflow with Standards

**Scenario**: University issues degree credential using OID4VCI

```csharp
// 1. Use SdJwt.Net core to construct SD-JWT
var sdJwt = new SdJwtBuilder()
    .WithPayload(claimsDict)
    .WithDisclosures(disclosures)
    .Build();

// 2. Wrap with Vc standards (W3C Credential)
var credential = new VerifiableCredential {
    Issuer = universityDid,
    CredentialSubject = sdJwt.Payload,
    Type = ["VerifiableCredential", "UniversityDegree"]
};

// 3. Export via OID4VCI (OpenID4VCI 1.0)
var oid4vciResponse = await issuer.IssueCredentialAsync(credential);

// 4. Optionally include status via StatusList
credential.CredentialStatus = new StatusListEntry {
    StatusListIndex = "12345",
    StatusPurpose = "revocation",
    StatusListCredential = statusListUrl
};
```

**Packages Used**: Core → Vc → Oid4Vci → StatusList

### Pattern 2: Presentation with Intelligent Selection

**Scenario**: Bank requests selective credentials using Presentation Exchange

```csharp
// 1. Define presentation requirements (DIF PE)
var presentationDefinition = new PresentationDefinition
{
    InputDescriptors = new[]
    {
        new InputDescriptor
        {
            Id = "university_degree",
            Schema = "UniversityDegree",
            Constraints = new Constraints
            {
                Fields = new[]
                {
                    new Field { Path = "$.degree_type" },
                    new Field { Path = "$.institution_name", 
                        Optional = true }  // GPA hidden
                }
            }
        }
    }
};

// 2. Holder selects matching credentials (PresentationExchange)
var selectedCredentials = await holder.SelectCredentialsAsync(
    presentationDefinition, 
    availableCredentials
);

// 3. Create presentation using OID4VP
var presentation = new VerifiablePresentation
{
    Type = ["VerifiablePresentation"],
    Holder = holderDid,
    VerifiableCredential = selectedCredentials
};

// 4. Send via OID4VP protocol (OpenID4VP 1.0)
var authResponse = await holder.CreateAuthorizationResponseAsync(
    presentation,
    verifierState
);
```

**Packages Used**: PresentationExchange → Vc → Oid4Vp

### Pattern 3: Trust Chain with Federation

**Scenario**: Verify credentials across organizations using OpenID Federation

```csharp
// 1. Establish trust using federation
var federationConfig = await federation.GetEntityConfigurationAsync(issuer);

// 2. Verify issuer trust chain
var trustChain = await federation.BuildTrustChainAsync(
    issuer,
    applicableGovernanceFramework
);

// 3. Validate core SD-JWT with issuer's public key from trust chain
var publicKey = trustChain.LastEntity.Metadata.OpenIdProvider.SigningKeys
    .First(k => k.Kid == sdJwt.UnverifiedHeader["kid"]);

var isValid = await SdJwtVerifier.VerifyAsync(
    serializedSdJwt,
    publicKey,
    acceptedAlgorithms: ["ES256"]
);

// 4. Check credential status from trust chain
if (credential.CredentialStatus != null)
{
    var statusListCredential = await federation.RetrieveCredentialAsync(
        credential.CredentialStatus.StatusListCredential,
        trustChain
    );
    
    var isRevoked = statusListCredential.IsRevoked(
        credentialReference
    );
}
```

**Packages Used**: OidFederation → Core → StatusList → Vc

### Pattern 4: High Assurance with HAIP

**Scenario**: Government credential with strict compliance requirements

```csharp
// 1. Create credential at HAIP Level 3 (Government)
var haipLevel3 = HaipAssuranceLevel.Level3GovernmentGrade;

var credential = new HaipVerifiableCredential
{
    AssuranceLevel = haipLevel3,
    // RFC 9901 with enhanced security
    CredentialContent = sdJwt,
    // RFC 9902 compliance
    ComplianceStandards = new[] { 
        "eIDAS",
        "NIST SP 800-63-3-Level3",
        "ISO/IEC 29115:2013"
    }
};

// 2. Validate issuer against HAIP requirements
var issuerValidation = HaipValidator.ValidateIssuer(
    issuerMetadata,
    haipLevel3
);

// 3. Verify maximum key binding strength
if (sdJwt.KeyBindingJwt.Header["alg"] != "ES512")  // Min for Level 3
{
    throw new SecurityException("Insufficient key binding strength");
}

// 4. Multi-signature validation for audit trail
foreach (var signature in credential.AuditSignatures)
{
    await VerifySignatureWithGovernmentKey(signature);
}

// 5. Cryptographic proof of non-repudiation
var proof = await credential.GenerateNonRepudiationProofAsync();
```

**Packages Used**: Core → Vc → HAIP → OidFederation

## Multi-Package Scenarios

### Scenario A: Complete Credential Ecosystem

**Business Case**: Financial institution issues accounts with KYC verification

```
1. Issuance (OID4VCI)
   ├─ Government issues ID credential (HAIP Level 3)
   │  └─ Wrapped in RFC 9902 (Vc)
   ├─ Bank verifies via federation
   │  └─ Trust chain from government
   └─ Bank issues account credential
      └─ References government credential

2. Lifecycle Management (StatusList)
   ├─ Government publishes status list
   ├─ Account credential references status
   └─ Changes tracked in audit log

3. Presentation (OID4VP + PresentationExchange)
   ├─ Investor portal requests selective disclosure
   │  └─ Account balance (disclosed)
   │  └─ Account type (hidden)
   ├─ Customer selects using PE
   └─ Bank verifies with federation

4. Verification (Core + OidFederation)
   ├─ Cryptographic validation of signature
   ├─ Key binding proof verification
   ├─ Trust chain validation
   └─ Status checking via federation
```

### Scenario B: Cross-Border Credential Exchange

**Business Case**: Student receives EU degree, applies to US job

```
1. EU University Issues (Vc + Oid4Vci)
   └─ SD-JWT wrapped in RFC 9902

2. Federation Setup (OidFederation)
   ├─ EU government registers as trust anchor
   ├─ US government trusts EU under mutual framework
   └─ Both publish federation metadata

3. Student Presents (Oid4Vp + PresentationExchange)
   ├─ US employer defines credential requirements
   ├─ Student selects degree credential
   └─ Includes selective disclosure (GPA hidden, degree type visible)

4. Verification (Core + OidFederation + HAIP)
   ├─ US employer resolves trust chain
   ├─ Validates EU issuer through federation
   ├─ Checks cryptographic integrity
   ├─ Optionally verifies HAIP compliance level
   └─ Confirms credential status

5. Audit & Compliance
   └─ Full audit trail maintained across all steps
```

## Code Patterns

### Error Handling Across Packages

```csharp
try
{
    // Issuance
    var sdJwt = await CreateSdJwtAsync(); // Core
    var wrapped = VerifiableCredential.Wrap(sdJwt); // Vc
    var issued = await issuer.IssueAsync(wrapped); // OID4VCI
    
    // Add status
    wrapped.AddStatus(statusListManager.Create()); // StatusList
    
    // Presentation
    var definition = await recipient.GetRequirementsAsync(); // PE
    var presentation = await holder.PresentAsync(definition); // OID4VP
    
    // Verification with trust
    var trustChain = await federation.VerifyAsync(issuer); // Federation
    await verifier.VerifyFinalAsync(presentation, trustChain); // Core
}
catch (CryptographicException ex)
{
    // Handle signature/key binding validation failures
    logger.LogError($"Cryptographic verification failed: {ex.Message}");
    // Could be due to Core (signature), Vc (structure), or OidFederation (key source)
}
catch (TrustChainException ex)
{
    // OidFederation-specific: Trust chain validation failed
    logger.LogError($"Trust not established: {ex.Message}");
}
catch (PresentationException ex)
{
    // OID4VP/PE specific: Presentation doesn't meet requirements
    logger.LogError($"Presentation invalid: {ex.Message}");
}
catch (StatusException ex)
{
    // StatusList specific: Revocation/suspension
    logger.LogError($"Credential revoked/suspended: {ex.Message}");
}
```

### Dependency Injection Setup

```csharp
// Register all packages in IoC container
services.AddSdJwt() // Core
    .AddVerifiableCredentials() // Vc
    .AddStatusList() // StatusList
    .AddOpenId4Vci() // Oid4Vci
    .AddOpenId4Vp() // Oid4Vp
    .AddPresentationExchange() // PE
    .AddOpenIdFederation() // Federation
    .AddHaip(); // HAIP

// Use across your application
var sdJwtService = serviceProvider.GetRequiredService<ISdJwtService>();
var vcService = serviceProvider.GetRequiredService<IVerifiableCredentialService>();
var federationService = serviceProvider.GetRequiredService<IFederationService>();
```

## Performance & Security Considerations

### Cryptographic Performance

- Core SD-JWT with ES256: ~20ms/operation
- OID4VP presentation validation: ~50ms (includes trust chain verification)
- StatusList checking: ~10ms (cached after first retrieval)
- Federation trust chain build: ~500ms (initial), <10ms (cached)

### Security Best Practices

1. **Always validate trust chains** before accepting credentials from federation
2. **Check status lists** for revoked/suspended credentials
3. **Verify key binding proofs** for compliance with RFC 9901
4. **Use HAIP Level 3** for government/sensitive applications
5. **Implement audit logging** across all verification steps
6. **Rotate signing keys** regularly and manage key history

### Scalability Patterns

- Cache federation metadata and trust chains (30-day TTL)
- Batch status list checks for multiple credentials
- Parallelize credential verification for presentation flows
- Use database for audit trails and status tracking

## Running Integration Examples

```bash
cd samples/SdJwt.Net.Samples

# Run individual integration examples
dotnet run -- --example comprehensive-integration
dotnet run -- --example cross-platform

# Or use interactive menu
dotnet run
# Select: A (Comprehensive) or B (Cross-Platform)
```

## Next Steps

- **Production Deployment**: See [Architecture Design](../../docs/architecture-design.md)
- **Real-World Scenarios**: Check [Financial Co-Pilot](../../docs/samples/scenarios/financial/README.md)
- **API Reference**: Review individual package documentation
- **Standards Compliance**: Review the [Standards README](../Standards/README.md)

## Troubleshooting Integration Issues

| Issue | Likely Cause | Solution |
|-------|-------------|----------|
| "Signature verification failed" | Core validation issue | Check algorithm in JWT header matches issuer key type |
| "Credential not in status list" | StatusList hasn't been updated | Check StatusList credential URL and update time |
| "Issuer not in trust chain" | OidFederation issue | Verify federation metadata is current (recent timestamp) |
| "Presentation doesn't match definition" | PresentationExchange mismatch | Review schema/path in definition vs actual credential structure |
| "Key binding proof invalid" | Core RFC 9901 issue | Verify holder's private key matches public key in credential |

---

**Learn by doing**: Start with the SimpleIntegrationExample and work your way up to RealWorldScenarios!
