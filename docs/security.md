# Security Policy

## Supported Versions

The following versions of the SdJwt.Net ecosystem are currently being supported with security updates:

| Version | Supported          | Release Date | End of Support |
| ------- | ------------------ | ------------ | -------------- |
| 1.0.x   | :white_check_mark: | 2025-01-30   | Active LTS     |
| 0.13.x  | :white_check_mark: | 2024-12-01   | 2025-06-30     |
| < 0.13  | :x:                | -            | Deprecated     |

## Reporting a Vulnerability

We take the security of SdJwt.Net seriously. If you discover a security vulnerability, please follow these steps:

### For Critical Security Issues
1. **Do not open a public issue or discussion.**
2. Email the security team directly at `security@thomastran.dev`
3. Use the subject line: `[SECURITY] SD-JWT .NET - [Brief Description]`
4. Include a detailed description of the vulnerability:
   - Steps to reproduce
   - Potential impact and severity assessment
   - Affected versions and components
   - Any proof-of-concept code (if available)

### Response Timeline
- **Acknowledgment**: Within 24 hours for critical issues, 48 hours for others
- **Initial Assessment**: Within 72 hours
- **Progress Updates**: Every 5 business days until resolution
- **Resolution**: Target 30 days for critical issues, 90 days for others
- **Public Disclosure**: Coordinated disclosure after fix is available

### Security Advisory Process
1. We will work with you to understand and validate the issue
2. We'll develop and test a fix
3. We'll prepare a security advisory and release
4. We'll credit you in the advisory (unless you prefer to remain anonymous)

## Security Best Practices

### Key Management (Updated 2025)

#### Production Key Storage
- **Hardware Security Modules (HSM)**: Required for HAIP Level 3 (Sovereign) compliance
- **Azure Key Vault**: Recommended for Azure-based deployments
- **AWS KMS/CloudHSM**: Recommended for AWS-based deployments
- **Google Cloud HSM**: Recommended for GCP-based deployments
- **Never hardcode keys**: Use environment variables or secure configuration

#### Key Rotation Best Practices
```csharp
// Automatic key rotation with overlap periods
builder.Services.AddSdJwtKeyRotation(options =>
{
    options.RotationInterval = TimeSpan.FromDays(90);
    options.OverlapPeriod = TimeSpan.FromDays(7);
    options.UseHardwareProtectedKeys = true; // For HAIP Level 2+
});
```

### Algorithm Selection (2025 Standards)

#### Recommended Algorithms (Security Order)
1. **ES512** (ECDSA P-521 + SHA-512) - Maximum security, required for HAIP Level 3
2. **ES384** (ECDSA P-384 + SHA-384) - High security, required for HAIP Level 2
3. **ES256** (ECDSA P-256 + SHA-256) - Good security, minimum for HAIP Level 1
4. **EdDSA** (Ed25519/Ed448) - Modern alternative, all HAIP levels
5. **PS512** (RSA-PSS 4096+ + SHA-512) - RSA alternative, HAIP Level 3
6. **PS384** (RSA-PSS 3072+ + SHA-384) - RSA alternative, HAIP Level 2
7. **PS256** (RSA-PSS 2048+ + SHA-256) - RSA alternative, HAIP Level 1

#### Deprecated/Blocked Algorithms
- ❌ **RS256/RS384/RS512**: PKCS#1 v1.5 padding (vulnerable to attacks)
- ❌ **HS256/HS384/HS512**: Symmetric algorithms (not suitable for VCs)
- ❌ **MD5**: Cryptographically broken
- ❌ **SHA-1**: Cryptographically weak
- ❌ **RSA < 2048 bits**: Insufficient key strength

```csharp
// The library automatically blocks weak algorithms
var haipValidator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh);
var result = haipValidator.ValidateAlgorithm("RS256"); // Will fail validation
```

### Cryptographic Randomness

#### Secure Random Generation
- **System.Security.Cryptography.RandomNumberGenerator**: Default and recommended
- **Hardware RNG**: When available, provides additional entropy
- **Nonce/Salt Generation**: Always use cryptographically secure randomness

```csharp
// Secure random generation for salts and nonces
using var rng = RandomNumberGenerator.Create();
var salt = new byte[32];
rng.GetBytes(salt);
var saltString = Convert.ToBase64String(salt);
```

### Validation & Verification

#### Comprehensive Verification Process
```csharp
public async Task<VerificationResult> SecureVerifyAsync(string sdJwt)
{
    var verifier = new SdJwtVerifier(new SdJwtVerifierOptions
    {
        // Strict validation settings
        RequireSignature = true,
        RequireIssuer = true,
        RequireAudience = true,
        RequireExpirationTime = true,
        ClockSkewTolerance = TimeSpan.FromMinutes(5),
        
        // HAIP compliance validation
        EnforceHaipCompliance = true,
        MinimumHaipLevel = HaipLevel.Level2_VeryHigh,
        
        // Trust chain validation
        RequireTrustChain = true,
        TrustAnchors = GetTrustedIssuers(),
        
        // Status validation
        CheckCredentialStatus = true,
        StatusCheckTimeout = TimeSpan.FromSeconds(30)
    });
    
    return await verifier.VerifyAsync(sdJwt);
}
```

#### Essential Validation Checks
1. **Signature Validation**: Cryptographic signature verification
2. **Trust Chain**: Issuer authenticity through federation or certificates
3. **Temporal Validity**: Check `iat`, `nbf`, and `exp` claims
4. **Audience Validation**: Verify `aud` claim matches expected audience
5. **Status Validation**: Check credential hasn't been revoked/suspended
6. **HAIP Compliance**: Verify algorithmic and protocol requirements
7. **Disclosure Validation**: Verify selective disclosure integrity

### HAIP Compliance Levels (2025)

#### Level 1: High Assurance
**Target**: Standard business, education, consumer applications
- **Algorithms**: ES256+, PS256+, EdDSA
- **Key Management**: Secure software-based storage
- **Protocols**: Basic proof of possession
- **Transport**: HTTPS required
- **Audit**: Standard logging

#### Level 2: Very High Assurance  
**Target**: Financial services, healthcare, regulated industries
- **Algorithms**: ES384+, PS384+, EdDSA
- **Key Management**: Hardware-protected storage
- **Protocols**: Wallet attestation + DPoP tokens
- **Transport**: Mutual TLS, certificate pinning
- **Audit**: Enhanced logging with retention
- **Additional**: PAR (Pushed Authorization Requests)

#### Level 3: Sovereign
**Target**: Government, critical infrastructure, national security
- **Algorithms**: ES512+, PS512+, EdDSA
- **Key Management**: HSM backing required
- **Protocols**: Enhanced device attestation
- **Transport**: Government-grade encryption
- **Audit**: Comprehensive compliance logging
- **Additional**: Qualified electronic signatures, multi-factor authentication

### Transport Security

#### TLS Configuration
```csharp
// Enforce strong TLS configuration
builder.Services.Configure<HttpsRedirectionOptions>(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    options.HttpsPort = 443;
});

builder.Services.Configure<HstsOptions>(options =>
{
    options.IncludeSubdomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
    options.Preload = true;
});
```

#### Certificate Validation
```csharp
// Certificate pinning for trust anchors
builder.Services.AddHttpClient("federation", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "SdJwt.Net/1.0");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) =>
    {
        // Implement certificate pinning logic
        return ValidateCertificatePin(certificate, chain, errors);
    }
});
```

### Privacy Protection

#### Selective Disclosure Best Practices
1. **Minimize Disclosure**: Only reveal necessary claims
2. **Audience-Specific**: Tailor disclosures to specific verifiers
3. **Progressive Disclosure**: Reveal information incrementally
4. **Session Boundaries**: Don't persist disclosed data across sessions
5. **User Consent**: Always obtain explicit user consent for disclosure

#### Data Protection Compliance
- **GDPR**: Data minimization and purpose limitation
- **CCPA**: Consumer privacy rights
- **HIPAA**: Healthcare data protection (where applicable)
- **PCI DSS**: Payment card data protection (where applicable)

## Compliance Standards (2025)

### Supported Standards
- **[IETF RFC 9901](https://datatracker.ietf.org/doc/rfc9901/)** - Selective Disclosure for JWTs
- **[draft-ietf-oauth-sd-jwt-vc-13](https://datatracker.ietf.org/doc/draft-ietf-oauth-sd-jwt-vc/)** - SD-JWT Verifiable Credentials
- **[draft-ietf-oauth-status-list-13](https://datatracker.ietf.org/doc/draft-ietf-oauth-status-list/)** - OAuth Status Lists
- **[OpenID4VCI 1.0](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html)** - Credential Issuance
- **[OpenID4VP 1.0](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html)** - Verifiable Presentations
- **[OpenID Federation 1.0](https://openid.net/specs/openid-federation-1_0.html)** - Trust Infrastructure
- **[DIF PE v2.1.1](https://identity.foundation/presentation-exchange/spec/v2.1.1/)** - Presentation Exchange
- **[HAIP 1.0](https://openid.net/specs/openid4vc-high-assurance-interoperability-profile-sd-jwt-vc-1_0.html)** - High Assurance Profile

### Regulatory Compliance
- **eIDAS 2.0**: European Digital Identity regulation
- **NIST SP 800-63**: Digital Identity Guidelines
- **ISO/IEC 18013-5**: Mobile Driving License standard
- **FIDO Alliance**: Strong authentication standards

## Security Testing

### Automated Security Testing
```bash
# Security-focused test execution
dotnet test --configuration Release --logger:trx \
  --collect:"XPlat Code Coverage" \
  --filter "Category=Security|Category=HAIP|Category=Cryptography"

# Security-specific test categories
dotnet test --filter "TestCategory=SecurityValidation"
dotnet test --filter "TestCategory=CryptographicSecurity"
dotnet test --filter "TestCategory=HaipCompliance"
```

### Security Benchmarks
- **Penetration Testing**: Regular third-party security assessments
- **Vulnerability Scanning**: Automated dependency and code scanning
- **Performance Security**: Timing attack resistance validation
- **Compliance Testing**: HAIP level validation and certification

## Incident Response

### Security Incident Classification
- **Critical**: Remote code execution, private key exposure, authentication bypass
- **High**: Privilege escalation, sensitive data exposure, denial of service
- **Medium**: Information disclosure, input validation flaws
- **Low**: Minor configuration issues, documentation problems

### Response Procedures
1. **Assessment**: Impact and severity evaluation
2. **Containment**: Immediate risk mitigation
3. **Investigation**: Root cause analysis
4. **Remediation**: Fix development and testing
5. **Communication**: User and stakeholder notification
6. **Recovery**: Deployment and monitoring
7. **Lessons Learned**: Process improvement

### Emergency Contacts
- **Security Team**: security@thomastran.dev
- **Critical Issues**: 24/7 response for severity 1 issues
- **Community**: [GitHub Security Advisories](https://github.com/thomas-tran/sd-jwt-dotnet/security)

---

**Last Updated**: January 2025
**Security Policy Version**: 1.1
**Next Review**: April 2025

For specific security questions or clarifications, please contact the security team at security@thomastran.dev.
