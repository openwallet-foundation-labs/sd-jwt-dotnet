# Auto-Versioning and Release Strategy for SD-JWT .NET

As the SD-JWT .NET ecosystem grows, manually managing version strings in `.csproj` files and hand-writing `CHANGELOG.md` becomes error-prone and tedious. To ensure a smooth, automated, and error-free release cycle, adopting robust auto-versioning and release strategies is highly recommended.

Below is an analysis of the current state, alongside the recommended industry-standard strategies for .NET OSS libraries.

## 1. Current State Analysis

* **Versioning**: All versions are currently hardcoded as `<Version>1.0.0</Version>` in every `src/*.csproj`. This leads to missed version bumps, or instances where a release is cut but the package metadata retains older versions, producing the warnings seen in CI.
* **Changelog**: Hand-written. While it adheres to "Keep a Changelog", maintaining it manually guarantees drift and merge conflicts when multiple PRs are generated simultaneously.
* **Releasing**: Handled manually via GitHub. The tag and release must be perfectly synced with the code state.

## 2. Recommended Strategy: MinVer + Release Please

The absolute best-in-class workflow for modern .NET ecosystem libraries combines two powerful, lightweight tools: **MinVer** and Google's **Release Please**.

### A. Auto-Versioning: `MinVer`

[MinVer](https://github.com/adamralph/minver) is a minimalist tool that automatically versions your .NET assemblies and NuGet packages based purely on Git tags.

**How it works:**

1. Add the `MinVer` package to `Directory.Build.props`.
2. Remove all `<Version>` tags and hardcoded metadata from all `.csproj` files.
3. When you build, `MinVer` finds the latest Git tag starting with `v` (e.g., `v1.1.0`), calculates distance if there are subsequent commits, and sets the `AssemblyVersion`, `FileVersion`, `PackageVersion`, and `Version` properties dynamically during build.

**Benefits:**
* Single source of truth (Git tags).
* No more commit noise just to bump a `.csproj` version.
* Excellent support for pre-releases (`v1.2.0-beta.1`).

### B. Automated Changelog & Release: `Release Please`

[Release Please](https://github.com/googleapis/release-please) is a GitHub action that automates semantic versioning based on "Conventional Commits" (e.g., `feat: Add OID4VP support`, `fix: Patch nonce validation`).

**How it works:**

1. As PRs are merged to `main`, Release Please analyzes the commit titles.
2. It opens an automated "Release PR" (e.g., `chore: release 1.2.0`). This PR updates `CHANGELOG.md` automatically by grouping up all features, fixes, and breaking changes.
3. When a maintainer is ready to release, they simply merge this Release PR.
4. Release Please then automatically creates the GitHub Release, assigns the Git tag (`v1.2.0`), and triggers your CI pipeline.
5. Your CI pipeline runs, `MinVer` picks up the new `v1.2.0` tag, builds the NuGet packages with version `1.2.0`, and uses **NuGet Trusted Publishing (OIDC)** to securely push to NuGet.org.

**Benefits:**
* Completely eliminates manual `CHANGELOG.md` editing.
* Mathematically determines semantic version changes (Major/Minor/Patch) based on commit types.
* Provides a clean, visible governance model (the Release PR acts as the final sign-off before publishing).

## 3. Alternative Approaches

### Release Drafter + MinVer

Instead of Release Please and Conventional Commits, you can use **Release Drafter**, which creates GitHub Draft Releases based on PR labels (e.g., `bug`, `enhancement`).
* *Pros*: Doesn't enforce rigid commit naming standards on contributors.
* *Cons*: Still requires a human to manually publish the draft release, and manually update the `CHANGELOG.md` file in the repo.

### Nerdbank.GitVersioning (NBGV)

An alternative to MinVer. It uses a `version.json` file to track versions and calculates a unique build number per commit.
* *Pros*: Extremely powerful, tracks branch versions explicitly.
* *Cons*: Highly pervasive, modifies git history, and arguably too heavy for a mostly linear OSS project compared to MinVer.

## 4. Proposed Implementation Steps if Approved

If you choose to adopt the **MinVer + Release Please** recommendation, here are the steps we would take:

1. **Introduce Conventional Commits**: Update `CONTRIBUTING.md` to request PRs follow conventional commits (or use PR titles).
2. **Add MinVer**: Add `<PackageReference Include="MinVer" Version="4.3.0" PrivateAssets="all" />` to `Directory.Build.props` and delete all `<Version>` tags in the SDKs.
3. **Configure Release Please**: Add `.github/workflows/release-please.yml` to set up the automated PR/Changelog creation.
4. **Refactor Release CI**: Update the existing `release.yml` so that it triggers exclusively on newly published GitHub Releases (which Release Please creates), invoking `MinVer` and Trusted Publishing gracefully.
