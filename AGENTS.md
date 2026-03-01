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
 src/                   # Library source projects (9 packages)
    SdJwt.Net/         # Core SD-JWT implementation (RFC 9901)
    SdJwt.Net.Vc/      # Verifiable Credentials
    SdJwt.Net.StatusList/   # Credential lifecycle / revocation
    SdJwt.Net.Oid4Vci/ # Credential issuance protocol
    SdJwt.Net.Oid4Vp/  # Presentation protocol
    SdJwt.Net.OidFederation/  # Trust management
    SdJwt.Net.PresentationExchange/  # DIF PEX v2.1.1
    SdJwt.Net.HAIP/    # High assurance security levels
    SdJwt.Net.Mdoc/    # ISO 18013-5 mDL/mdoc support
 tests/                 # xUnit test projects (one per package)
 samples/               # Console demo application (not packaged)
 docs/                  # Developer documentation
    architecture-design.md
    developer-guide.md
    use-cases/         # Industry use cases and patterns
    specs/             # IETF and OpenID spec text files
 .github/
    workflows/
        ci-validation.yml      # CI pipeline (build, test, quality, security)
        draft-release-pr.yml   # Automated release PR and changelog
        publish-nuget.yml      # NuGet publish on GitHub Release
        reusable-build-pack.yml # Shared build and packaging steps
        scan-dependencies.yml
 Directory.Build.props  # Shared MSBuild properties for all projects
 SdJwt.Net.sln          # Solution file
 CONTRIBUTING.md        # Contributor guide
 CHANGELOG.md           # Auto-updated by Release Please
 AGENTS.md              # This file
```

## Tech Stack

- **Language**: C# 12 with nullable reference types and implicit usings enabled.
- **Target Frameworks**: `net8.0`, `net9.0`, `net10.0`, `netstandard2.1` (multi-targeted in each library).
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

## Test-Driven Development (TDD)

This project follows **Test-Driven Development** methodology. All new features and bug fixes must follow the TDD workflow.

### TDD Workflow (Red-Green-Refactor)

1. **RED**: Write a failing test first that defines the expected behavior.
2. **GREEN**: Write the minimal code necessary to make the test pass.
3. **REFACTOR**: Improve the code while keeping all tests passing.

### TDD Requirements

- **Tests First**: Never write implementation code without a corresponding failing test.
- **One Behavior Per Test**: Each test should verify a single behavior or scenario.
- **Descriptive Names**: Test method names should clearly describe the scenario and expected outcome.
- **Arrange-Act-Assert (AAA)**: Structure tests with clear separation of setup, execution, and verification.
- **Edge Cases**: Include tests for boundary conditions, null inputs, and error scenarios.
- **Coverage Threshold**: Aim for minimum 90% code coverage; critical paths require 100%.

### Test Naming Convention

```
MethodName_Scenario_ExpectedBehavior
```

Examples:

```csharp
[Fact]
public void Verify_WithExpiredCredential_ReturnsFalse()

[Fact]
public void Issue_WithValidClaims_CreatesSignedMdoc()

[Theory]
[InlineData(null)]
[InlineData("")]
public void Constructor_WithInvalidInput_ThrowsArgumentException(string? input)
```

### Test Project Structure

```
tests/SdJwt.Net.{PackageName}.Tests/
    {ClassName}Tests.cs         # Unit tests for specific class
    Integration/                # Integration tests
    TestFixtures/               # Shared test data and fixtures
    TestBase.cs                 # Common test infrastructure
```

### Test Categories

| Category    | Purpose                            | Execution   |
| ----------- | ---------------------------------- | ----------- |
| `[Fact]`    | Unit tests for single behaviors    | Every build |
| `[Theory]`  | Parameterized tests for variations | Every build |
| Integration | Cross-component tests              | CI pipeline |
| End-to-End  | Full workflow validation           | CI pipeline |

### Test Dependencies

All test projects use these packages:

- `xunit` - Test framework
- `FluentAssertions` - Readable assertions
- `coverlet.collector` - Code coverage
- `Microsoft.NET.Test.Sdk` - Test runner integration

### Running Tests

```pwsh
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/SdJwt.Net.Mdoc.Tests/

# Run tests matching a filter
dotnet test --filter "FullyQualifiedName~MdocVerifier"
```

## Versioning Model (MinVer + Release Please)

Do NOT manually edit `<Version>` in any `.csproj` file. All versions are driven by Git tags.

### How it works

1. Merge commits to `main` following **Conventional Commits** format.
2. **Release Please** (`.github/workflows/draft-release-pr.yml`) opens or updates a "Release PR" containing the bumped version and an auto-generated `CHANGELOG.md`.
3. A maintainer reviews and merges the Release PR.
4. Release Please creates a GitHub Release and Git tag (e.g. `v1.2.0`).
5. The `publish-nuget.yml` workflow triggers on the new release, builds packages, and publishes to NuGet via **Trusted Publishing (OIDC)** - no long-lived API keys required.

### Commit message format

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

| Type                              | Effect on semver |
| --------------------------------- | ---------------- |
| `feat:`                           | Minor bump       |
| `fix:`                            | Patch bump       |
| `docs:`, `chore:`, `test:`, `ci:` | No version bump  |
| `BREAKING CHANGE:` footer         | Major bump       |
| `feat!:` or `fix!:`               | Major bump       |

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
# Single command that mirrors CI hard gates
./scripts/verify.ps1
```

```bash
# Linux/macOS equivalent
bash ./scripts/verify.sh
```

## CI/CD Pipeline Gates

The `ci-validation.yml` pipeline enforces the following **hard gates** (failure = PR blocked):

| Gate                         | What it checks                                       |
| ---------------------------- | ---------------------------------------------------- |
| `scripts/verify.*`           | Restore, build, test, formatting, vulnerability scan |
| Vulnerability scan           | `dotnet list package --vulnerable` must return clean |
| HAIP algorithm compliance    | No MD5/SHA1 usage in src/                            |
| All unit tests               | All 9 test suites must pass                          |
| Package ecosystem validation | All 9 NuGet packages must be produced                |

## NuGet Publishing

Publishing to NuGet.org uses **Trusted Publishing (OIDC)**:

- No `NUGET_API_KEY` secret is needed in the repository.
- Configure `NUGET_USERNAME` as a repository or organization variable for OIDC token exchange in workflow.
- The `production` GitHub Environment must have the trusted publisher policy configured on nuget.org.
- When a GitHub Release is published (created by Release Please), `publish-nuget.yml` automatically publishes all packages.

## Architecture Notes

### Directory.Build.props

All shared MSBuild properties live here to avoid duplication across `.csproj` files:

- `MinVerTagPrefix`: set to `v` (tags like `v1.2.0` become version `1.2.0`)
- `TreatWarningsAsErrors`: `true`
- `Nullable`: `enable`
- `GenerateDocumentationFile`: `true` (for library projects)

### Package Relationships

All 9 packages are independent but `SdJwt.Net` is the foundational dependency:

```
SdJwt.Net (Core)
   SdJwt.Net.Vc
        SdJwt.Net.StatusList
   SdJwt.Net.Oid4Vci
   SdJwt.Net.Oid4Vp
        SdJwt.Net.PresentationExchange
   SdJwt.Net.OidFederation
   SdJwt.Net.HAIP
   SdJwt.Net.Mdoc          # ISO 18013-5 mDL/mdoc support
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
- Industry use cases and patterns live in `docs/use-cases/`.

## Security Considerations

- Never introduce or weaken cryptographic validation.
- The `HaipCryptoValidator` class enforces algorithm compliance tiers (Level 1, 2, 3).
- Timing attacks must be mitigated with constant-time comparison operations.
- Replay attacks must be mitigated with nonce and `iat` freshness validation.
- If you discover a security vulnerability, report to `tldinteractive@gmail.com` - do not open a public issue.
