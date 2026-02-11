# Standards & Protocol Compliance Examples

This directory contains examples demonstrating implementations of industry standards and protocols for verifiable credentials and identity systems.

## Directory Structure

```
Standards/
├── VerifiableCredentials/     # SD-JWT VC and Status List (IETF drafts)
├── OpenId/                    # OpenID Foundation protocols
└── PresentationExchange/      # DIF Presentation Exchange
```

## Examples by Category

### Verifiable Credentials (IETF Standards)

#### [VerifiableCredentials/VerifiableCredentialsExample.cs](VerifiableCredentials/VerifiableCredentialsExample.cs)

**Standard**: [draft-ietf-oauth-sd-jwt-vc-13](https://datatracker.ietf.org/doc/draft-ietf-oauth-sd-jwt-vc/)

**Learning Objectives**:

- Create W3C-compliant verifiable credentials using SD-JWT
- Understand SD-JWT VC structure and required claims
- Implement credential type definitions
- Validate VCs according to specification

**Key Features**:

- `vct` (verifiable credential type) claim
- `iss` (issuer) claim validation
- Collision-resistant claim names
- Credential schemas and types

**Use Cases**:

- University degrees
- Professional certifications
- Medical credentials
- Employment verification

#### [VerifiableCredentials/StatusListExample.cs](VerifiableCredentials/StatusListExample.cs)

**Standard**: [draft-ietf-oauth-status-list-13](https://datatracker.ietf.org/doc/draft-ietf-oauth-status-list/)

**Learning Objectives**:

- Implement credential lifecycle management
- Handle revocation and suspension
- Work with compressed status lists
- Perform real-time status verification

**Key Features**:

- Multi-bit status encoding
- Gzip compression for privacy
- Optimistic concurrency control
- Herd privacy protection

**Use Cases**:

- Credential revocation (driver's licenses)
- Temporary suspension (access badges)
- Status verification before acceptance
- Privacy-preserving status checks

---

### OpenID Foundation Protocols

#### [OpenId/OpenId4VciExample.cs](OpenId/OpenId4VciExample.cs)

**Standard**: [OpenID for Verifiable Credential Issuance 1.0](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html)

**Learning Objectives**:

- Implement standardized credential issuance flows
- Handle credential offer creation and delivery
- Support pre-authorized and authorization code flows
- Manage deferred credential issuance

**Key Features**:

- Pre-authorized code flow with PIN
- Authorization code flow
- Batch credential issuance
- Deferred issuance for complex credentials
- QR code generation for mobile wallets

**Use Cases**:

- Digital ID card issuance by government
- Employee badge provisioning
- Educational credential distribution
- Mobile driver's license issuance

#### [OpenId/OpenId4VpExample.cs](OpenId/OpenId4VpExample.cs)

**Standard**: [OpenID for Verifiable Presentations 1.0](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html)

**Learning Objectives**:

- Implement cross-device presentation flows
- Create and validate presentation requests
- Handle authorization requests and responses
- Validate VP tokens with SD-JWT VCs

**Key Features**:

- Cross-device presentation via QR codes
- Direct post response mode
- Presentation definition support
- SD-JWT VC validation
- Key binding JWT verification
- Nonce-based replay protection

**Use Cases**:

- Age verification at point of sale
- Border control credential presentation
- Access control with verifiable credentials
- Login with verifiable credentials

#### [OpenId/OpenIdFederationExample.cs](OpenId/OpenIdFederationExample.cs)

**Standard**: [OpenID Federation 1.0](https://openid.net/specs/openid-federation-1_0.html)

**Learning Objectives**:

- Build trust chains between entities
- Validate entity configurations
- Resolve trust anchors
- Implement trust chain validation

**Key Features**:

- Entity configuration creation
- Trust chain resolution
- Authority hint processing
- Federation metadata validation
- Multi-level trust hierarchies

**Use Cases**:

- Government PKI federation
- Cross-border identity systems
- Multi-organization trust networks
- Educational credential ecosystems

---

### DIF Presentation Exchange

#### [PresentationExchange/PresentationExchangeExample.cs](PresentationExchange/PresentationExchangeExample.cs)

**Standard**: [DIF Presentation Exchange v2.1.1](https://identity.foundation/presentation-exchange/spec/v2.1.1/)

**Learning Objectives**:

- Define presentation requirements declaratively
- Implement intelligent credential selection
- Handle complex constraint matching
- Generate compliant presentation submissions

**Key Features**:

- Presentation definitions with input descriptors
- Field constraints with JSONPath
- Submission requirements (all, pick patterns)
- Automated credential selection
- Privacy-preserving predicates

**Use Cases**:

- Multi-credential verification workflows
- Conditional credential requirements
- Age verification without revealing exact age
- Income verification without exact amounts
- Citizenship verification without revealing country

## Complete Integration Example

Here's how these standards work together in a real-world scenario:

```
┌─────────────────────────────────────────────────────────────┐
│                    COMPLETE WORKFLOW                         │
└─────────────────────────────────────────────────────────────┘

1. ISSUANCE (OpenID4VCI)
   University ─[OID4VCI]→ Student receives degree credential

2. TRUST (OpenID Federation)  
   University is validated through education trust chain

3. STATUS (Status List)
   Credential marked as "valid" (not revoked/suspended)

4. PRESENTATION REQUEST (OpenID4VP + Presentation Exchange)
   Employer requests: "Prove you have a Computer Science degree"
   
5. CREDENTIAL SELECTION (Presentation Exchange)
   Wallet finds matching credential automatically
   
6. PRESENTATION (OpenID4VP)
   Student presents degree with key binding
   
7. VERIFICATION (SD-JWT VC + Status List + Federation)
   Employer validates:
   ✓ Cryptographic signature
   ✓ University is trusted (federation check)
   ✓ Credential not revoked (status check)
   ✓ Student possesses credential (key binding)
```

## Running the Examples

### Interactive Menu

```bash
cd samples/SdJwt.Net.Samples
dotnet run

# Select from menu:
# 4 - Verifiable Credentials
# 5 - Status Lists
# 6 - OpenID4VCI
# 7 - OpenID4VP
# 8 - OpenID Federation
# 9 - Presentation Exchange
```

### Direct Execution

Each example can be studied independently, but they're designed to work together for complete workflows.

## Learning Path

### Level 1: Foundation (2-3 hours)

1. **Verifiable Credentials** - Understand VC structure
2. **Status Lists** - Learn lifecycle management

### Level 2: Protocols (3-4 hours)

3. **OpenID4VCI** - Master credential issuance
2. **OpenID4VP** - Master credential presentation
3. **Presentation Exchange** - Learn request/response patterns

### Level 3: Advanced (2-3 hours)

6. **OpenID Federation** - Build trust infrastructure
2. **Complete Integration** - Combine all standards

## Standards Comparison

| Standard | Purpose | When to Use |
|----------|---------|-------------|
| **SD-JWT VC** | Credential format | Always (foundation) |
| **Status List** | Lifecycle management | When revocation needed |
| **OID4VCI** | Standardized issuance | Multi-vendor issuance |
| **OID4VP** | Standardized presentation | Cross-device verification |
| **OpenID Federation** | Trust management | Multi-organization trust |
| **Presentation Exchange** | Complex requirements | Intelligent selection needed |

## Package Dependencies

Each example demonstrates a specific package:

| Example | Primary Package | Additional Packages |
|---------|----------------|---------------------|
| Verifiable Credentials | `SdJwt.Net.Vc` | `SdJwt.Net` |
| Status Lists | `SdJwt.Net.StatusList` | `SdJwt.Net.Vc` |
| OpenID4VCI | `SdJwt.Net.Oid4Vci` | `SdJwt.Net.Vc` |
| OpenID4VP | `SdJwt.Net.Oid4Vp` | `SdJwt.Net.Vc` |
| OpenID Federation | `SdJwt.Net.OidFederation` | `SdJwt.Net` |
| Presentation Exchange | `SdJwt.Net.PresentationExchange` | `SdJwt.Net.Vc` |

## Key Takeaways

### Interoperability

- Standards enable vendor-neutral implementations
- Credentials issued by one system work with any compliant verifier
- Trust can be established across organizational boundaries

### Completeness

- Each standard addresses a specific need
- Combined together, they provide complete credential lifecycle
- Missing pieces can break interoperability

### Security

- Standards define security requirements
- Implementations must follow specifications exactly
- Compliance ensures trust in the ecosystem

## Next Steps

After mastering these standards:

- Review [Integration examples](../Integration/) for multi-package patterns
- Explore [Real-World scenarios](../RealWorld/) for production implementations
- Study [HAIP examples](../HAIP/) for high-assurance use cases

## Related Documentation

- **[Developer Guide](../../../docs/developer-guide.md)** - Comprehensive development guide
- **[Architecture Design](../../../docs/architecture-design.md)** - System architecture patterns
- **Package Documentation**:
  - [SdJwt.Net.Vc](../../../src/SdJwt.Net.Vc/README.md)
  - [SdJwt.Net.StatusList](../../../src/SdJwt.Net.StatusList/README.md)
  - [SdJwt.Net.Oid4Vci](../../../src/SdJwt.Net.Oid4Vci/README.md)
  - [SdJwt.Net.Oid4Vp](../../../src/SdJwt.Net.Oid4Vp/README.md)
  - [SdJwt.Net.OidFederation](../../../src/SdJwt.Net.OidFederation/README.md)
  - [SdJwt.Net.PresentationExchange](../../../src/SdJwt.Net.PresentationExchange/README.md)

---

**Last Updated**: February 11, 2026
