# AGENTS.md - AI Agent Guide for SD-JWT .NET

This file provides instructions and context for AI coding assistants working on this repository.

## Project Overview

SD-JWT .NET is a production-ready .NET ecosystem implementing:

- **SD-JWT** (Selective Disclosure JWT) per RFC 9901
- **OpenID4VCI / OpenID4VP** credential issuance and presentation protocols
- **DIF Presentation Exchange** v2.1.1
- **OpenID Federation** trust infrastructure
- **HAIP** (High Assurance Interoperability Profile) compliance

The project is hosted under the **OpenWallet Foundation Labs** organization.

## Repository Structure

```
sd-jwt-dotnet/
 src/                   # Library source projects (8 packages)
    SdJwt.Net/         # Core SD-JWT implementation (RFC 9901)
    SdJwt.Net.Vc/      # Verifiable Credentials
    SdJwt.Net.StatusList/   # Credential lifecycle / revocation
    SdJwt.Net.Oid4Vci/ # Credential issuance protocol
    SdJwt.Net.Oid4Vp/  # Presentation protocol
    SdJwt.Net.OidFederation/  # Trust management
    SdJwt.Net.PresentationExchange/  # DIF PEX v2.1.1
    SdJwt.Net.HAIP/    # High assurance security levels
 tests/                 # xUnit test projects (one per package)
 samples/               # Console demo application (not packaged)
 docs/                  # Developer documentation
    architecture-design.md
    developer-guide.md
    articles/          # Strategy and implementation articles
    specs/             # IETF and OpenID spec text files
 .github/
    workflows/
        ci-cd.yml         # CI pipeline (build, test, code quality, security)
        release-please.yml  # Automated release PR and changelog
        release.yml        # NuGet publish on GitHub Release
        reusable-build.yml # Shared build steps
        dependency-management.yml
 Directory.Build.props  # Shared MSBuild properties for all projects
 SdJwt.Net.sln          # Solution file
 CONTRIBUTING.md        # Contributor guide
 CHANGELOG.md           # Auto-updated by Release Please
 AGENTS.md              # This file
```

## Tech Stack

- **Language**: C# 12 with nullable reference types and implicit usings enabled.
- **Target Frameworks**: `net8.0`, `net9.0`, `netstandard2.1` (multi-targeted in each library).
- **Test Framework**: xUnit with `coverlet` for code coverage.
- **Auto-Versioning**: MinVer 4.3.0 - versions are derived from Git tags (e.g. `v1.2.0`).
- **Build System**: MSBuild with shared properties in `Directory.Build.props`.

## Coding Standards

All code must adhere to these standards without exception:

1. **No emojis** in any file (code or documentation).
2. **SOLID and DRY principles** - avoid duplication; use interfaces, abstractions, and well-named methods.
3. **XML documentation comments** on all public types and members.
4. **TreatWarningsAsErrors is enabled** - the build will fail on any C# warning.
5. **No weak cryptography** - MD5 and SHA-1 are blocked by the HAIP validator and must never be used.
6. **Constant-time comparisons** for all security-sensitive operations.
7. **DCO compliance** - every commit must have a `Signed-off-by:` line (configured via `.githooks`).

## Versioning Model (MinVer + Release Please)

Do NOT manually edit `<Version>` in any `.csproj` file. All versions are driven by Git tags.

### How it works

1. Merge commits to `main` following **Conventional Commits** format.
2. **Release Please** (`.github/workflows/release-please.yml`) opens or updates a "Release PR" containing the bumped version and an auto-generated `CHANGELOG.md`.
3. A maintainer reviews and merges the Release PR.
4. Release Please creates a GitHub Release and Git tag (e.g. `v1.2.0`).
5. The `release.yml` workflow triggers on the new release, builds packages, and publishes to NuGet via **Trusted Publishing (OIDC)** - no API keys required.

### Commit message format

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

| Type | Effect on semver |
|------|-----------------|
| `feat:` | Minor bump |
| `fix:` | Patch bump |
| `docs:`, `chore:`, `test:`, `ci:` | No version bump |
| `BREAKING CHANGE:` footer | Major bump |
| `feat!:` or `fix!:` | Major bump |

Examples:

```
feat(oid4vp): add audience validation support
fix: resolve nonce validation in KB-JWT freshness check
docs: update CONTRIBUTING.md branch model
feat!: redesign SdIssuer API for consistency
```

## Essential Commands

Before committing or raising a PR, always verify the following commands pass:

```pwsh
# Restore dependencies
dotnet restore SdJwt.Net.sln

# Build in Release mode (TreatWarningsAsErrors is on)
dotnet build SdJwt.Net.sln --configuration Release

# Run all tests
dotnet test SdJwt.Net.sln

# Verify code formatting (CI enforces this as a hard gate)
dotnet format --verify-no-changes SdJwt.Net.sln

# Check for vulnerable packages (CI enforces this as a hard gate)
dotnet list package --vulnerable --include-transitive
```

## CI/CD Pipeline Gates

The `ci-cd.yml` pipeline enforces the following **hard gates** (failure = PR blocked):

| Gate | What it checks |
|------|---------------|
| `dotnet format` | Code style - no unformatted files allowed |
| Vulnerability scan | `dotnet list package --vulnerable` must return clean |
| HAIP algorithm compliance | No MD5/SHA1 usage in src/ |
| All unit tests | All 8 test suites must pass |
| Package ecosystem validation | All 8 NuGet packages must be produced |

## NuGet Publishing

Publishing to NuGet.org uses **Trusted Publishing (OIDC)**:

- No `NUGET_API_KEY` secret is needed in the repository.
- The `production` GitHub Environment must have the trusted publisher policy configured on nuget.org.
- When a GitHub Release is published (created by Release Please), `release.yml` automatically publishes all packages.

## Architecture Notes

### Directory.Build.props

All shared MSBuild properties live here to avoid duplication across `.csproj` files:

- `MinVerTagPrefix`: set to `v` (tags like `v1.2.0` become version `1.2.0`)
- `TreatWarningsAsErrors`: `true`
- `Nullable`: `enable`
- `GenerateDocumentationFile`: `true` (for library projects)

### Package Relationships

All 8 packages are independent but `SdJwt.Net` is the foundational dependency:

```
SdJwt.Net (Core)
   SdJwt.Net.Vc
        SdJwt.Net.StatusList
   SdJwt.Net.Oid4Vci
   SdJwt.Net.Oid4Vp
        SdJwt.Net.PresentationExchange
   SdJwt.Net.OidFederation
   SdJwt.Net.HAIP
```

### Test Projects

Each `src/SdJwt.Net.*` has a corresponding `tests/SdJwt.Net.*.Tests` project. Test projects use:

- xUnit with `[Fact]` and `[Theory]` attributes.
- `IsPackable = false` (test projects are never published as NuGet packages).

## Docs Maintenance

When modifying documentation:

- Do not use emojis anywhere in `.md` files.
- Keep `docs/README.md` and `src/*/README.md` in sync with implementation status.
- Specification compliance docs live in `docs/specs/`.
- Strategy and implementation articles live in `docs/articles/`.

## Security Considerations

- Never introduce or weaken cryptographic validation.
- The `HaipCryptoValidator` class enforces algorithm compliance tiers (Level 1, 2, 3).
- Timing attacks must be mitigated with constant-time comparison operations.
- Replay attacks must be mitigated with nonce and `iat` freshness validation.
- If you discover a security vulnerability, report to `tldinteractive@gmail.com` - do not open a public issue.
