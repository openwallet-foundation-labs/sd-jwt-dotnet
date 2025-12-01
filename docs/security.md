# Security Policy

## Supported Versions

The following versions of the SdJwt.Net ecosystem are currently being supported with security updates:

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

We take the security of SdJwt.Net seriously. If you discover a security vulnerability, please follow these steps:

1. **Do not open a public issue.**
2. Email the security team at `security@openwallet.foundation`.
3. Include a detailed description of the vulnerability, steps to reproduce, and any potential impact.

We will acknowledge your report within 48 hours and provide an estimated timeline for a fix.

## Security Best Practices

### Key Management

- Always use secure storage mechanisms for private keys (e.g., Azure Key Vault, AWS KMS, Hardware Security Modules).
- Never hardcode keys in your application code.
- Rotate keys regularly.

### Algorithm Selection

- Use strong algorithms like ES256 (ECDSA using P-256 and SHA-256) or stronger.
- Avoid deprecated algorithms like RS256 unless strictly required for legacy compatibility.
- The library enforces secure defaults, but ensure you do not explicitly override them with insecure options.

### Randomness

- Ensure that the random number generator used for nonces and salts is cryptographically secure.
- The library uses `System.Security.Cryptography.RandomNumberGenerator` by default.

### Validation

- Always validate the signature of incoming SD-JWTs.
- Verify the issuer (`iss`), audience (`aud`), and expiration (`exp`) claims.
- Ensure that the disclosures match the expected structure and content.

## Compliance

This library is designed to be compliant with:

- [RFC 9901](https://tools.ietf.org/rfc/rfc9901.txt) (Selective Disclosure for JWTs)
- [OpenID for Verifiable Credential Issuance](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html)
- [OpenID for Verifiable Presentations](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html)

Please refer to the specific package documentation for compliance details.
