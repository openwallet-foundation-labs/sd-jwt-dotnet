# Changelog

All notable changes to this project will be documented in this file.

## [0.2.0] - 2025-07-01
### Added

- **`SdJwtParser` Utility**: A new static utility class for parsing and inspecting raw SD-JWT issuance strings, presentations, and related artifacts.
- **Status List Support**: Implemented `draft-ietf-oauth-status-list` for credential revocation checking.
- **`StatusListManager`**: A new helper class for Issuers to create, update, and sign Status List Credentials.
- **Enhanced `SdJwtVcVerifier`**: The verifier can now perform status checks, including in-memory caching for performance.
- **SD-JWT-VC Support**: Implemented the `draft-ietf-oauth-sd-jwt-vc` specification.
- **Multi-targeting**: Targets `.NET 9` and `.NET Standard 2.0`.
- **Structured Logging**: Integrated `Microsoft.Extensions.Logging.Abstractions`.
- **Security Hardening**: Constant-time comparison for `sd_hash` and strict algorithm policy.
### Changed
- **Redesign SDK**: Redesign the SDK to make it more robust and maintainable


## [0.1.0] - 2023
- Initial release.