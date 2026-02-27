# HAIP (High Assurance Interoperability Profile) Examples

This directory contains examples demonstrating the three assurance levels defined in the [HAIP Profile for SD-JWT VC](https://openid.net/specs/openid4vc-high-assurance-interoperability-profile-sd-jwt-vc-1_0.html).

## Understanding HAIP Levels

HAIP defines three assurance levels for SD-JWT Verifiable Credentials, each with increasing security requirements:

| Level | Name | Use Cases | Security Requirements |
|-------|------|-----------|----------------------|
| **Level 1** | Basic | Education, Professional Credentials | Standard JWT security, basic key management |
| **Level 2** | Very High | Financial Services, Healthcare | Enhanced cryptography, hardware key storage |
| **Level 3** | Sovereign | Government ID, ePassports, Critical Infrastructure | Hardware security modules, certified cryptography |

## Examples in This Directory

### 1. BasicHaipExample.cs (Level 1)

**Use Cases**: University degrees, professional certifications, training certificates

**Features Demonstrated**:

- Standard HAIP validation
- Basic cryptographic requirements
- SD-JWT selective disclosure
- Standard key binding

**Security Level**:

- ES256 or RS256 signatures
- Software key storage acceptable
- Standard token lifetimes
- Basic issuer trust

**When to Use**:

- Educational credentials
- Professional memberships
- Non-sensitive verifiable data
- Proof of completion certificates

### 2. EnterpriseHaipExample.cs (Level 2 - Very High)

**Use Cases**: Banking KYC, healthcare records, insurance claims, legal documents

**Features Demonstrated**:

- Enhanced cryptographic requirements
- Hardware-backed key storage
- Stricter validation rules
- Additional security checks

**Security Level**:

- ES256 with P-256 curve or stronger
- Hardware key storage (TPM, Secure Enclave)
- Reduced token lifetimes
- Enhanced issuer verification
- Status list validation required

**When to Use**:

- Financial services
- Healthcare information
- Legal agreements
- High-value transactions
- Regulated industries

### 3. GovernmentHaipExample.cs (Level 3 - Sovereign)

**Use Cases**: National ID cards, ePassports, government services, critical infrastructure access

**Features Demonstrated**:

- Maximum security requirements
- Certified cryptographic modules
- Strict compliance validation
- Government-grade security

**Security Level**:

- ES384/ES512 or PS384/PS512 required
- FIPS 140-2 Level 2+ HSM mandatory
- Very short token lifetimes (minutes)
- Full trust chain validation
- Mandatory status checking
- Certified implementation required

**When to Use**:

- Government-issued identity
- Border control systems
- Critical infrastructure
- National security applications
- Sovereign identity systems

## Security Comparison Matrix

| Feature | Level 1 (Basic) | Level 2 (Very High) | Level 3 (Sovereign) |
|---------|----------------|---------------------|---------------------|
| **Cryptography** | ES256/RS256 | ES256+ | ES384/ES512/PS384/PS512 |
| **Key Storage** | Software OK | Hardware (TPM) | HSM (FIPS 140-2 L2+) |
| **Token Lifetime** | Hours | Minutes-Hours | Minutes |
| **Status Validation** | Optional | Recommended | Mandatory |
| **Trust Chain** | Basic | Enhanced | Full validation |
| **Issuer Requirements** | Standard | Verified | Government/Certified |
| **Key Binding** | Optional | Recommended | Mandatory |
| **Certification** | None | Optional | Mandatory |

## Running the Examples

### Prerequisites

All examples require:

- .NET 8.0 or later
- SdJwt.Net.HAIP package

Additional requirements by level:

- **Level 2**: Access to TPM or Secure Enclave
- **Level 3**: Hardware Security Module (HSM) with FIPS 140-2 Level 2+ certification

### Run from Sample Menu

```bash
cd samples/SdJwt.Net.Samples
dotnet run
```

Select the HAIP examples from the menu to explore each assurance level.

From the interactive menu, use:
- `H` for Basic HAIP
- `E` for Enterprise HAIP
- `G` for Government HAIP

## Implementation Guidelines

### Choosing the Right Level

**Use Level 1 when**:

- Personal data is not highly sensitive
- Financial impact of fraud is low
- Regulatory requirements are minimal
- Cost optimization is important

**Use Level 2 when**:

- Handling sensitive personal data
- Financial transactions involved
- Regulatory compliance required (GDPR, HIPAA)
- Moderate to high fraud risk

**Use Level 3 when**:

- Government-issued credentials
- National security implications
- Critical infrastructure access
- Compliance with eIDAS, NIST 800-63-3 Level 3, or similar

### Common Integration Patterns

Each example demonstrates:

1. **Credential Issuance** - Creating HAIP-compliant SD-JWT VCs
2. **Selective Disclosure** - Presenting only required claims
3. **Verification** - HAIP-level specific validation
4. **Status Checking** - Revocation and suspension handling

## Compliance Standards

HAIP aligns with multiple international standards:

- **eIDAS** (EU): Levels align with LoA Substantial (L2) and High (L3)
- **NIST 800-63-3** (US): Levels map to IAL2 (L2) and IAL3 (L3)
- **ISO/IEC 29115**: Authentication assurance framework
- **FIDO Alliance**: Hardware authenticator requirements

## Security Best Practices

### Level 1 (Basic)

- Use recent cryptographic libraries
- Implement basic input validation
- Regular key rotation (yearly)
- Monitor for common attacks

### Level 2 (Very High)

- Hardware-backed key storage mandatory
- Implement comprehensive logging
- Regular security audits
- Key rotation (quarterly)
- Real-time status validation

### Level 3 (Sovereign)

- FIPS-validated cryptographic modules
- Continuous monitoring and alerting
- Frequent security assessments
- Certified implementation required
- Immediate status checking
- Short-lived credentials only

## Related Documentation

- **[HAIP Package Documentation](../../../src/SdJwt.Net.HAIP/README.md)** - Complete API reference
- **[Developer Guide](../../../docs/README.md)** - Implementation patterns
- **[Security Guidelines](../../../SECURITY.md)** - Security best practices
- **[HAIP Specification](https://openid.net/specs/openid4vc-high-assurance-interoperability-profile-sd-jwt-vc-1_0.html)** - Official standard

## Support

For questions or issues specific to HAIP implementation:

- Review the [specification](https://openid.net/specs/openid4vc-high-assurance-interoperability-profile-sd-jwt-vc-1_0.html)
- Check [GitHub Issues](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/issues)
- Join [GitHub Discussions](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/discussions)

---

**Last Updated**: February 11, 2026
**HAIP Version**: 1.0

