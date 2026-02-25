# Contributing to SD-JWT .NET Ecosystem

Thank you for your interest in contributing to the SD-JWT .NET Ecosystem! We welcome contributions from the community to help improve and grow the project. Please read the following guidelines to ensure a smooth contribution process.

## How to Contribute

1. **Fork the Repository**
   - Click the "Fork" button at the top right of the repository page to create your own copy.

2. **Clone Your Fork**
   - Clone your forked repository to your local machine:

     ```pwsh
     git clone https://github.com/YOUR_USERNAME/sd-jwt-dotnet.git
     cd sd-jwt-dotnet
     ```

3. **Setup Git Hooks (Required for DCO Compliance)**
   - Configure Git to automatically add sign-off to all commits:

     ```pwsh
     git config core.hooksPath .githooks
     ```

   This prevents DCO check failures. See [SETUP_HOOKS.md](SETUP_HOOKS.md) for details.

4. **Create a Branch**
   - Create a new branch for your feature or bugfix:

     ```pwsh
     git checkout -b feature/your-feature-name
     ```

5. **Make Changes**
   - Implement your changes following the project's coding standards and best practices.
   - Add or update tests as needed.
   - Follow [Conventional Commits](https://www.conventionalcommits.org/) for your commit messages (e.g., `feat: add nonce validation`, `fix: resolve replay attack`). This is used to automatically generate `CHANGELOG.md` entries.

6. **Test Your Changes**
   - Run the tests to ensure your changes do not break existing functionality:

     ```pwsh
     dotnet test
     ```

   - Verify code formatting:

     ```pwsh
     dotnet format --verify-no-changes SdJwt.Net.sln
     ```

7. **Commit and Push**
   - Commit your changes with a clear message (sign-off is added automatically):

     ```pwsh
     git commit -am "feat: describe your change"
     git push origin feature/your-feature-name
     ```

   *Note: If you set up the git hooks in step 3, `Signed-off-by:` is automatically added. Otherwise, use `git commit -s` to manually add sign-off.*

8. **Create a Pull Request**
   - Go to the original repository and open a Pull Request targeting the **`main` branch**.
   - Fill out the PR template and describe your changes clearly.

## Developer Certificate of Origin (DCO)

All commits must include a `Signed-off-by:` line to comply with the [Developer Certificate of Origin](https://developercertificate.org/).

**Automatic Sign-Off (Recommended):**
Run this once after cloning:

```pwsh
git config core.hooksPath .githooks
```

**Manual Sign-Off (Alternative):**
Add `-s` flag to each commit:

```pwsh
git commit -s -m "Your commit message"
```

Pull requests with unsigned commits will be blocked until resolved.

## Branching Model

This project uses a simple, single-trunk model:

- **`main`** - The primary branch. All PRs should target `main` directly.
- **Feature/bugfix branches** - Create branches from `main` and merge back to `main` via PR.
- **Release tags** - Releases are cut from `main` via git tags (e.g., `v1.2.0`).

## Conventional Commits Reference

This project uses [Conventional Commits](https://www.conventionalcommits.org/) to automate changelog generation:

| Type | Use For | Version Bump |
|------|---------|-------------|
| `feat:` | New features | Minor (`1.1.0`) |
| `fix:` | Bug fixes | Patch (`1.0.1`) |
| `docs:` | Documentation only | None |
| `chore:` | Tooling, CI, deps | None |
| `refactor:` | Code restructuring | None |
| `test:` | Adding/fixing tests | None |
| `BREAKING CHANGE:` | Breaking API change | Major (`2.0.0`) |

**Examples:**

```
feat: add OID4VP nonce validation
fix: resolve replay attack in KB-JWT freshness check
docs: update CONTRIBUTING.md with branching model
feat!: redesign SdIssuer API for consistency
```

## Code Style and Standards

- Follow the .NET coding conventions and C# 12 language features.
- Adhere to **SOLID** and **DRY** principles.
- Write clear, concise, and well-documented code with XML documentation comments.
- Include unit and integration tests for new features and bug fixes.
- Ensure all tests pass before submitting a PR.
- Do not use weak cryptographic algorithms (MD5, SHA-1) - these are blocked by the HAIP validator.

## Reporting Issues

- Use the [Issues](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/issues) tab to report bugs or request features.
- Provide as much detail as possible, including steps to reproduce, expected behavior, and screenshots if applicable.

## Community and Support

- Be respectful and constructive in all communications.
- For questions, discussions, or help, use GitHub Discussions or reach out to maintainers listed in `MAINTAINERS.md`.

## License

By contributing, you agree that your contributions will be licensed under the terms of the project's [LICENSE.txt](LICENSE.txt).

---

Thank you for helping make SD-JWT .NET Ecosystem better!
