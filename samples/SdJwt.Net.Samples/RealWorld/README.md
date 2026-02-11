# Real-World Scenarios

This guide showcases complete, production-ready implementations of SD-JWT in actual business contexts. Each scenario demonstrates how to solve real problems with the complete SD-JWT .NET ecosystem, combining all 8 packages in realistic workflows.

## Featured Scenarios

### Scenario 1: University to Bank Loan

**Business Challenge**: Streamline mortgage application by verifying education and employment history while protecting sensitive information.

**Timeline**: 5 minutes to run end-to-end example

#### The Problem

- Banks manually verify 100+ documents per application
- Applicants reluctant to disclose salary and expenses
- Each institution requests same information differently
- Manual verification takes 3+ weeks

#### The Solution

```
┌──────────────────────────────────────────────────────────┐
│                   Student Life Journey                    │
├──────────────────────────────────────────────────────────┤
│                                                            │
│  STAGE 1: Education (University)                         │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━                          │
│  ├─ University issues degree via OID4VCI                │
│  ├─ Includes GPA, honors, graduation date               │
│  ├─ Wrapped in W3C Verifiable Credential (RFC 9902)    │
│  └─ Core RFC 9901 SD-JWT enables selective disclosure  │
│                                                            │
│  STAGE 2: Employment (TechCorp)                          │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━                         │
│  ├─ Employer issues employment record via OID4VCI       │
│  ├─ Includes salary, position, tenure                   │
│  ├─ Wrapped in W3C Verifiable Credential (RFC 9902)    │
│  └─ Core RFC 9901 SD-JWT hides compensation details    │
│                                                            │
│  STAGE 3: Loan Application (Bank)                        │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━                     │
│  ├─ Bank defines requirements via DIF Presentation       │
│  │  └─ Wants: Institution, degree type, current tenure  │
│  │  └─ Hides: GPA, salary amounts                       │
│  ├─ Student presents using selective disclosure          │
│  ├─ Bank verifies trust chain via OpenID Federation     │
│  └─ Loan decision in 3 hours (vs 3 weeks!)              │
│                                                            │
│  STAGE 4: Status Tracking                                 │
│  ━━━━━━━━━━━━━━━━━━━━━━━━                               │
│  └─ Loan status tracked via Draft-13 Status List        │
│     (pending → approved → funded)                        │
│                                                            │
└──────────────────────────────────────────────────────────┘
```

#### Key Features Demonstrated

- **RFC 9901 (Core)**: Selective disclosure to hide salary, GPA
- **RFC 9902 (Vc)**: Wrapping SD-JWT in W3C Credential format  
- **OID4VCI 1.0**: University and employer issue credentials
- **DIF PE v2.1.1**: Bank's intelligent credential requirements
- **OID4VP 1.0**: Student presents credentials securely
- **Draft-13 Status List**: Track loan application lifecycle
- **OID Federation 1.0**: Verify university and employer are trusted
- **HAIP**: Optional government-grade verification (employment verification)

#### Running the Example

```bash
cd samples/SdJwt.Net.Samples/RealWorld
dotnet run -- --scenario university-to-bank

# Detailed output shows all steps
```

#### Privacy Benefits

- University degree visible, but GPA stays private
- Employer confirmed, but salary amount hidden
- Bank verifies employment fact without seeing compensation
- Full audit trail without exposing sensitive data

---

### Scenario 2: Defense Contractor Background Check

**Business Challenge**: Verify security clearance and education for sensitive government contract without exposing classified clearance details.

**Timeline**: 3 minutes to run end-to-end example

#### The Problem

- Multiple government agencies hold different clearance information
- Contractors need to prove certifications without revealing specifics
- Each prime contractor verifies differently from subcontractors
- Risk of information leakage about actual clearance level

#### The Solution

```
┌──────────────────────────────────────────────────────────┐
│              Background Verification Flow                 │
├──────────────────────────────────────────────────────────┤
│                                                            │
│  STAGE 1: Security Clearance (Government)                │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━                  │
│  ├─ Government agency issues clearance as SD-JWT        │
│  │  └─ Includes: Has clearance, level, valid dates      │
│  │  └─ Hidden: Investigation agency, detail level       │
│  ├─ HAIP Level 3 for government-grade security          │
│  └─ Status List for expiration tracking                  │
│                                                            │
│  STAGE 2: Education Verification (MIT)                   │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━                     │
│  ├─ MIT issues degree credential                        │
│  ├─ Advanced degree confirmed, specific major hidden    │
│  └─ Cross-referenced with government database           │
│                                                            │
│  STAGE 3: Contractor Verification (Prime Company)        │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━            │
│  ├─ Contractor shows \"has clearance\" without details  │
│  ├─ Education verified through federation              │
│  ├─ Multiple signatures validate through trust chain   │
│  └─ Contractor hired without leaking security details   │
│                                                            │
│  STAGE 4: Subcontractor Verification (Compliance)        │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━               │
│  ├─ Prime references worker's current clearance status │
│  ├─ Subcontractor verifies expiration (via Status List) │
│  └─ Hiring decision without exposing clearance details  │
│                                                            │
└──────────────────────────────────────────────────────────┘
```

#### Key Features Demonstrated

- **RFC 9901 (Core)**: Hide investigation specifics
- **RFC 9902 (Vc)**: Standardize clearance credential format
- **OID4VCI 1.0**: Government issues clearance
- **OID4VP 1.0**: Contractor presents to prime company
- **OID Federation 1.0**: Verify government issuer legitimacy
- **Draft-13 Status List**: Track clearance expiration
- **HAIP Level 3**: Government-grade cryptographic assurance
- **DIF PE v2.1.1**: Prime company's verification requirements

#### Running the Example

```bash
cd samples/SdJwt.Net.Samples/RealWorld
dotnet run -- --scenario defense-background-check

# Shows multi-credential workflow with classified protection
```

#### Security Benefits

- Clearance existence confirmed without exposure
- Investigation details remain confidential
- Expiration dates tracked automatically
- Cross-organization verification without leaking details
- Audit trail for compliance

---

### Scenario 3: Healthcare Record Sharing

**Business Challenge**: Enable patient-controlled medical record sharing across providers while maintaining HIPAA compliance and privacy.

**Timeline**: 4 minutes to run end-to-end example

#### The Problem

- Patients frustrated by repeated medical histories
- Providers lack complete patient context
- HIPAA creates liability for any data exposure
- Patients want control without sacrificing care quality

#### The Solution

```
┌──────────────────────────────────────────────────────────┐
│           Patient-Initiated Care Coordination            │
├──────────────────────────────────────────────────────────┤
│                                                            │
│  STAGE 1: Initial Medical Summary (Hospital)            │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━                  │
│  ├─ Hospital creates medical summary as SD-JWT          │
│  │  └─ Includes: Diagnoses, allergies, medications      │
│  │  └─ Hides: Insurance info, SSN, billing              │
│  ├─ Wrapped in W3C Verifiable Credential               │
│  └─ HIPAA-compliant selective disclosure               │
│                                                            │
│  STAGE 2: Patient Consent (Patient Portal)              │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━                  │
│  ├─ Patient reviews and consents to share               │
│  ├─ Patient specifies which provider can access         │
│  ├─ DIF Presentation Exchange defines what's shared     │
│  └─ Audit trail created for compliance                  │
│                                                            │
│  STAGE 3: Specialist Consultation (Cardiology)          │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━                │
│  ├─ Patient presents medical summary to specialist      │
│  ├─ Only relevant information visible (cardiac data)    │
│  ├─ Insurance/billing data stays hidden                 │
│  ├─ Specialist verifies doctor credentials              │
│  └─ Improved diagnosis with better context              │
│                                                            │
│  STAGE 4: Audit & Compliance (Office of Compliance)     │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━            │
│  ├─ HIPAA audit trail: who accessed what, when         │
│  ├─ Status List tracks revocation of consent            │
│  ├─ Federation verifies all providers are legitimate    │
│  └─ Patient can revoke consent at any time              │
│                                                            │
└──────────────────────────────────────────────────────────┘
```

#### Key Features Demonstrated

- **RFC 9901 (Core)**: Selective disclosure of sensitive data
- **RFC 9902 (Vc)**: Healthcare credential standard
- **OID4VCI 1.0**: Hospital issues medical summary
- **OID4VP 1.0**: Secure presentation to specialist
- **DIF PE v2.1.1**: Define what data specialist can see
- **Draft-13 Status List**: Track consent revocation
- **OID Federation 1.0**: Verify provider credentials
- **Audit Trail**: Full HIPAA compliance logging

#### Running the Example

```bash
cd samples/SdJwt.Net.Samples/RealWorld
dotnet run -- --scenario healthcare-sharing

# Demonstrates HIPAA-compliant credential sharing
```

#### Compliance Benefits

- Selective disclosure ensures HIPAA compliance
- Audit trail satisfies regulatory requirements
- Patient consent explicit and revocable
- Insurance info remains confidential
- SSN never shared unnecessarily
- Complete traceability for investigators

---

### Scenario 4: Government Service Access

**Business Challenge**: Citizens access multiple government services (DMV, tax office, benefits) with single credential while protecting privacy across jurisdictions.

**Timeline**: 6 minutes to run end-to-end example

#### The Problem

- Citizens manage separate credentials for each agency
- Age verification requires full driver's license scan
- Government agencies don't trust each other's systems
- Privacy concerns with multiple lookups of same person

#### The Solution

```
┌──────────────────────────────────────────────────────────┐
│          Unified Government Digital Identity              │
├──────────────────────────────────────────────────────────┤
│                                                            │
│  STAGE 1: National ID Issuance (Federal)                 │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━                  │
│  ├─ DMV issues digital driver's license as SD-JWT       │
│  ├─ HAIP Level 3 certification (meets eIDAS)            │
│  ├─ Includes personal details with strict protection    │
│  └─ Selectively disclosable: age, residency status      │
│                                                            │
│  STAGE 2: Citizen Verification (Tax Office)              │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━                   │
│  ├─ Citizen presents digital ID via OID4VP             │
│  ├─ Tax office requests only age, residency (via PE)   │
│  ├─ Full SSN/address stays private                     │
│  ├─ Federation verifies DMV legitimacy                 │
│  └─ Tax filing completed without data exposure         │
│                                                            │
│  STAGE 3: Benefits Eligibility (Social Services)         │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━               │
│  ├─ Citizen applies for veterans benefits              │
│  ├─ Different information revealed (veteran status)    │
│  ├─ Same digital ID used, different disclosures       │
│  ├─ Reduced fraud by verified government source       │
│  └─ Faster processing than traditional documents       │
│                                                            │
│  STAGE 4: Cross-Agency Audit (Inspector General)         │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━               │
│  ├─ Inspector traces all citizen interactions          │
│  ├─ Federation proves all agencies legitimate          │
│  ├─ Audit trail shows exactly what was accessed        │
│  ├─ Privacy preserved - only revealed data is logged   │
│  └─ Fraud detection without violating citizen privacy  │
│                                                            │
└──────────────────────────────────────────────────────────┘
```

#### Key Features Demonstrated

- **RFC 9901 (Core)**: Age verification without full SSN
- **RFC 9902 (Vc)**: Standardized government credential
- **OID4VCI 1.0**: DMV issues digital ID
- **OID4VP 1.0**: Citizen presents to any agency
- **DIF PE v2.1.1**: Agency-specific disclosure requirements
- **OID Federation 1.0**: Trust between federal/state agencies
- **Draft-13 Status List**: Credential revocation (ID expires)
- **HAIP Level 3**: eIDAS-approved (EU spec, US adoptable)

#### Running the Example

```bash
cd samples/SdJwt.Net.Samples/RealWorld
dotnet run -- --scenario government-services

# Shows cross-agency credential usage
```

#### Government Benefits

- Single credential for multiple services
- Age verification without identity theft risk
- Faster service delivery
- Reduced fraud
- Privacy by design
- Constitutional-grade audit trail
- International standards compliance (eIDAS)

---

## Running All Scenarios

### Interactive Mode (Recommended for Learning)

```bash
cd samples/SdJwt.Net.Samples
dotnet run

# Then select: C (Real-World Use Cases)
```

### Command Line Mode

```bash
# Run specific scenario
dotnet run -- --scenario university-to-bank
dotnet run -- --scenario defense-background-check
dotnet run -- --scenario healthcare-sharing
dotnet run -- --scenario government-services

# Run all scenarios
dotnet run -- --scenario all

# Verbose output for debugging
dotnet run -- --scenario university-to-bank --verbose
```

## The Financial Co-Pilot: AI-Powered Real-World Scenario

Beyond the basic scenarios above, the Financial Co-Pilot demonstrates how to integrate AI with SD-JWT for intelligence privacy-preserving financial guidance.

**See**: [Financial Co-Pilot Deep Dive](../../docs/samples/scenarios/financial/README.md)

**What It Shows**:

- ChatGPT integration with privacy
- Selective disclosure to AI services
- Cryptographic audit trail of AI interactions
- Compliance with financial regulations
- User control over AI access to data

---

## Key Takeaways from Real-World Scenarios

### Privacy Without Compromise

- Share only necessary information
- Rest remains cryptographically protected
- Recipient can't access what wasn't disclosed
- Holder maintains full control

### Efficiency Gains

- 75% faster mortgage applications (week → day)
- Same-day government ID verification
- Instant medical record access (with consent)
- Automated clearance verification

### Compliance & Security

- HIPAA-compliant healthcare sharing
- NIST 800-63-3 government authentication
- eIDAS-approved digital identity
- SOX-ready financial audit trails

### Trust & Interoperability

- Works across different organizations
- Doesn't require shared infrastructure
- Leverages open standards (RFC 9901, W3C VC, OID4VC)
- Government and enterprise ready

---

## Next Steps

### Explore Individual Components

- [Core Concepts](../Core/README.md)
- [Protocol Standards](../Standards/README.md)
- [Integration Patterns](../Integration/README.md)

### Go Deeper

- [Architecture & Design](../../docs/architecture-design.md)
- [Financial Co-Pilot](../../docs/samples/scenarios/financial/README.md)
- [Developer Guide](../../docs/developer-guide.md)

### Get Involved

- Contribute new scenarios to GitHub
- Share your use cases with the community
- Report issues and improvements

---

**These scenarios prove that privacy-preserving digital credentials are not theoretical — they're practical, standard-based, and ready for production!**
