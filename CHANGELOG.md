# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- ?? **Version Alignment with Specifications**: Updated all package versions to align with their corresponding specification versions:
  - **SdJwt.Net**: 1.0.0 (RFC 9901 - Final Standard)
  - **SdJwt.Net.Vc**: 0.13.0 (draft-ietf-oauth-sd-jwt-vc-13)
  - **SdJwt.Net.StatusList**: 0.13.0 (draft-ietf-oauth-status-list-13)
  - **SdJwt.Net.Oid4Vci**: 1.0.0 (OID4VCI 1.0 Final)
  - **SdJwt.Net.Oid4Vp**: 1.0.0 (OID4VP 1.0 Final)

### Fixed
- ?? **CI/CD Pipeline**: Corrected solution file reference to `SdJwt.Net.sln`
- ?? **Documentation**: Updated all documentation to reflect correct package versions and specification compliance
- ?? **Platform Support**: Removed premature .NET 10 support (not yet stable), focusing on .NET 8, 9, and .NET Standard 2.1

---

## [1.0.0] - 2025-01-30 (SdJwt.Net Core)

### Added

#### ?? RFC 9901 Compliance - Final Standard
- **Complete RFC 9901 Implementation**: Full compliance with the final SD-JWT specification
- **JWS JSON Serialization Support**: Both Flattened and General JSON Serialization formats
- **Enhanced Security Features**: Algorithm validation, constant-time operations, and comprehensive input validation
- **Multi-Platform Support**: .NET 8, .NET 9, and .NET Standard 2.1 with platform-specific optimizations

#### ?? Core Features
- **SD-JWT Issuance**: Create SD-JWTs with selective disclosure capabilities
- **SD-JWT Verification**: Verify SD-JWTs with key binding and presentation support
- **SD-JWT Holders**: Create presentations with selective disclosure
- **Security Hardening**: Blocks weak algorithms (MD5, SHA-1), enforces SHA-2 family

### Security
- **Algorithm Enforcement**: Proactively blocks cryptographically weak algorithms
- **Timing Attack Protection**: Constant-time operations for sensitive comparisons
- **Cross-Platform Security**: Consistent security guarantees across all supported platforms

---

## [0.13.0] - 2025-01-30 (SdJwt.Net.Vc)

### Added

#### ?? draft-ietf-oauth-sd-jwt-vc-13 Compliance
- **Complete VC Support**: Full implementation of SD-JWT Verifiable Credentials specification
- **Type Safety**: Strongly-typed models for all VC components
- **Media Type Support**: Support for both `dc+sd-jwt` and legacy `vc+sd-jwt` media types
- **Status Claim Integration**: Built-in support for status claims and revocation checking

#### ?? VC-Specific Features
- **SdJwtVcIssuer**: Create SD-JWT VCs with proper VCT validation
- **SdJwtVcVerifier**: Verify SD-JWT VCs with comprehensive validation
- **SdJwtVcPayload**: Strongly-typed payload model with all VC claims
- **VCT Validation**: Collision-Resistant Name validation for VCT claims

### Enhanced
- **Validation**: Comprehensive VC structure validation according to draft-13
- **Error Handling**: Detailed error messages for VC-specific validation failures
- **Integration**: Seamless integration with StatusList for revocation checking

---

## [0.13.0] - 2025-01-30 (SdJwt.Net.StatusList)

### Added

#### ?? draft-ietf-oauth-status-list-13 Compliance
- **Complete Status List Support**: Full implementation of OAuth Status List specification
- **Multi-bit Status**: Support for multiple status types (Valid, Invalid, Suspended, UnderInvestigation)
- **Compression**: Efficient bit-level storage with GZIP compression
- **Optimistic Concurrency**: ETag-based versioning for safe concurrent updates

#### ?? Production Features
- **StatusListManager**: Create and manage status lists with enterprise features
- **HttpStatusListFetcher**: Production-ready HTTP fetcher with retry logic and caching
- **IStatusListStorage**: Pluggable storage abstraction with concurrency control
- **StatusListVerifier**: Verify credential status with comprehensive validation

### Enhanced
- **Performance**: Optimized for large-scale status list operations
- **Privacy**: Privacy-preserving status checking mechanisms
- **Caching**: Built-in caching for improved performance and reduced network load

---

## [1.0.0] - 2025-01-30 (SdJwt.Net.Oid4Vci)

### Added

#### ?? OID4VCI 1.0 Final Specification
- **Complete Protocol Support**: Full implementation of OpenID4VCI 1.0 final specification
- **Transport-Agnostic Design**: Pure data models that work with any HTTP framework
- **Multiple Grant Types**: Authorization Code, Pre-authorized Code, and custom grant support
- **Deferred Issuance**: Asynchronous credential delivery capabilities

#### ?? Protocol Features
- **CredentialOffer**: Complete credential offer generation with QR code support
- **Proof of Possession**: JWT, CWT, and LDP-VP proof format support
- **Token Management**: Full OAuth 2.0 token endpoint integration
- **Error Handling**: Comprehensive error responses per OID4VCI specification

### Enhanced
- **Modular Architecture**: Each model in its own file for better maintainability
- **Validation**: Comprehensive validation for all protocol parameters
- **Builder Patterns**: Fluent APIs for complex object construction

---

## [1.0.0] - 2025-01-30 (SdJwt.Net.Oid4Vp)

### Added

#### ?? OID4VP 1.0 Final Specification
- **Complete Protocol Support**: Full implementation of OpenID4VP 1.0 final specification
- **Presentation Exchange v2.0.0**: Complete DIF Presentation Exchange support
- **Cross-Device Flow**: QR code-based presentation flows using `response_mode=direct_post`
- **Security Validation**: Comprehensive validation including signature verification and key binding

#### ?? Verification Features
- **PresentationRequestBuilder**: Fluent API for creating complex presentation requests
- **VpTokenValidator**: Complete validation of VP token responses with security checks
- **AuthorizationRequestParser**: Parse and validate authorization request URIs
- **Status Constraints**: Integration with Status List for revocation checking

### Enhanced
- **Complex Requirements**: Support for "all", "pick N", and "pick range" submission rules
- **Field Constraints**: JSONPath-based field selection with JSON Schema validation
- **Error Handling**: Secure error responses that don't leak sensitive??
- **Transport Agnostic**: Works with any HTTP framework or transport mechanism

---

## [0.2.0] - 2024-07-01

### Added
- **`SdJwtParser` Utility**: Static utility class for parsing and inspecting raw SD-JWT issuance strings, presentations, and related artifacts
- **Status List Support**: Implemented `draft-ietf-oauth-status-list` for credential revocation checking
- **`StatusListManager`**: Helper class for Issuers to create, update, and sign Status List Credentials
- **Enhanced `SdJwtVcVerifier`**: Verifier can now perform status checks with in-memory caching for performance
- **SD-JWT-VC Support**: Implemented the `draft-ietf-oauth-sd-jwt-vc` specification
- **Multi-targeting**: Targets `.NET 9` and `.NET Standard 2.0`
- **Structured Logging**: Integrated `Microsoft.Extensions.Logging.Abstractions`
- **Security Hardening**: Constant-time comparison for `sd_hash` and strict algorithm policy

### Changed
- **SDK Redesign**: Complete redesign for improved robustness and maintainability
- **Dependencies**: Updated to latest stable versions for security and performance

---

## [0.1.0] - 2023

### Added
- Initial release with basic SD-JWT functionality
- Core issuance and verification capabilities
- Basic selective disclosure support

---

## Future Roadmap

### Planned for Next Releases
- **EdDSA Algorithm Support**: Native Ed25519 support for all target frameworks
- **Performance Benchmarks**: Comprehensive benchmarking suite
- **Advanced Caching**: Distributed caching options for enterprise deployments
- **Monitoring Integration**: Built-in metrics and telemetry support

### Under Consideration
- **.NET 10 Support**: When officially released and stable
- **Hardware Security Module (HSM)** integration
- **Quantum-resistant algorithms** future-proofing
- **WebAssembly (WASM)** support for client-side applications
- **Native AOT** compilation support for improved startup performance

---

## Version Alignment Summary

Our versioning strategy now aligns with specification maturity:

- **1.0.x**: Final, stable specifications (RFC 9901, OID4VCI 1.0, OID4VP 1.0)
- **0.13.x**: Draft specifications at version 13 (SD-JWT VC, Status List)

This provides clear indication of specification stability and helps developers make informed decisions about production readiness.

---

**For detailed migration guides and breaking change information, see the individual package README files.**