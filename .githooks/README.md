# Git Hooks for SD-JWT .NET

This directory contains Git hooks that help maintain code quality and compliance across all platforms (Windows, macOS, Linux).

## Available Hooks

### commit-msg - Automatic Sign-Off

The `commit-msg` hook automatically adds a `Signed-off-by:` line to every commit message, ensuring DCO (Developer Certificate of Origin) compliance. It also validates Conventional Commits format.

### pre-commit - Auto-Format Code and Markdown

The `pre-commit` hook **automatically formats your code** before committing:

- **C# code formatting**: Runs `dotnet format` to auto-fix C# style issues
- **Markdown formatting**: Runs `prettier --write` to auto-fix Markdown formatting on staged files

**This hook will automatically fix formatting issues and re-stage the files** - your commit will never fail due to formatting!

**Available scripts:**

- `pre-commit` - Bash script (default, works on all platforms via Git Bash)
- `pre-commit.ps1` - PowerShell script (optional, for native Windows PowerShell users)

### pre-push - Markdown Formatting Check

The `pre-push` hook verifies that all Markdown files pass Prettier formatting checks before pushing. This provides a final safety net for formatting issues.

**Available scripts:**

- `pre-push` - Bash script (default, works on all platforms via Git Bash)
- `pre-push.ps1` - PowerShell script (optional, for native Windows PowerShell users)

## Installation

### Recommended: For All Contributors (Cross-Platform)

Configure Git to use this hooks directory:

```bash
git config core.hooksPath .githooks
```

This command works on **all platforms** (Windows, macOS, Linux) and needs to be run once per clone.

- **On Windows:** Git Bash (installed with Git for Windows) will automatically use the bash scripts.
- **On macOS/Linux:** The bash scripts run natively.

### Manual Installation (Traditional Approach)

If you prefer the traditional `.git/hooks` approach:

**On Linux/macOS:**

```bash
cp .githooks/commit-msg .git/hooks/commit-msg
cp .githooks/pre-commit .git/hooks/pre-commit
cp .githooks/pre-push .git/hooks/pre-push
chmod +x .git/hooks/commit-msg
chmod +x .git/hooks/pre-commit
chmod +x .git/hooks/pre-push
```

**On Windows (PowerShell - uses PowerShell scripts):**

```powershell
Copy-Item .githooks\commit-msg .git\hooks\commit-msg
Copy-Item .githooks\pre-commit.ps1 .git\hooks\pre-commit
Copy-Item .githooks\pre-push.ps1 .git\hooks\pre-push
```

**On Windows (Git Bash - uses bash scripts):**

When you run `git push`, the hook:

1. **Pre-push**: Checks all Markdown files for formatting issues
2. If any issues are found, the push is blocked with instructions to fix

## Platform Compatibility

### Windows

- **Git Bash** (installed with Git for Windows): Uses bash scripts automatically
- **PowerShell/PowerShell Core**: Can use `.ps1` scripts (copy them manually)
- **Command Prompt**: Not supported for hooks (use Git Bash or PowerShell)

### macOS

- **zsh/bash**: Uses bash scripts natively
- **PowerShell Core**: Can use `.ps1` scripts if installed

### Linux

- **Cross-platform**: Works consistently on Windows, macOS, and Linux
- **bash/sh**: Uses bash scripts natively
- **PowerShell Core**: Can use `.ps1` scripts if installed

## Requirements

on your platform:

```bash
# Test pre-commit formatting check
echo "# Test" >> TEST.md
git add TEST.md
git commit -m "test: verify hooks"  # Will check formatting first

# Verify the sign-off was added by commit-msg hook
git log -1 --format="%B"

# Reset the test
git reset HEAD~1
rm TEST.md
```

Manually check formatting:

```bash
# Check C# formatting
dotnet format --verify-no-changes SdJwt.Net.sln

# Check Markdown formatting
npx prettier@3.2.5 --check "**/*.md"

# Fix any issues
dotnet format SdJwt.Net.sln
npx prettier@3.2.5 --write "**/*.md"
```

## Troubleshooting

### "dotnet: command not found" on Windows Git Bash

If you see this warning but have .NET installed, it means dotnet is not in your Git Bash PATH. The hook will show a warning but continue. To fix:

**Option 1**: Add dotnet to Git Bash PATH:

```bash
# Add to ~/.bashrc
export PATH="$PATH:/c/Program Files/dotnet"
```

**Option 2**: Use PowerShell hooks instead (they automatically find dotnet):

```powershell
Copy-Item .githooks\pre-commit.ps1 .git\hooks\pre-commit
Copy-Item .githooks\pre-push.ps1 .git\hooks\pre-push
```

### Hook not executing

Verify hooks are configured:

```bash
git config core.hooksPath
# Should output: .githooks
```

Check file permissions (Unix/macOS):

```bash
ls -la .githooks/
# pre-commit, pre-push, commit-msg should have execute permission (x)
```

### Line ending issues

The `.gitattributes` file ensures hooks always use LF line endings. If you manually edited hooks on Windows, normalize them:

```bash
git add --renormalize .githooks/
```

## Bypassing Hooksissues early\*\*: C# and Markdown formatting checked before commit

- **Fast pre-commit checks**: Only staged files are checked for Markdown formatting
- **Team consistency**: All team members use the same process
- **PR approval**: Prevents DCO and formatting check failures in pull requests

## Testing

### Automated Cross-Platform Tests

Run the automated test suite to verify hooks work on your platform:

**On Linux/macOS/Git Bash:**

```bash
bash .githooks/test-hooks.sh
```

**On Windows PowerShell:**

```powershell
pwsh .githooks/test-hooks.ps1
```

The test suite will:

- Test both bash and PowerShell versions of hooks (if available)
- Verify C# and Markdown formatting checks work
- Confirm hooks are properly configured
- Report detailed results for each test

### Manual Testing

Test that the hooks work:

```bash
# Test pre-commit formatting check
echo "test" >> README.md
git add README.md
git commit -m "test: verify hooks" # Will check formatting first

# Verify the sign-off was added by commit-msg hook
git log -1 --format="%B"

# Reset the test
git reset HEAD~1
git checkout README.md
```

Manually check formatting:

```bash
# Check C# formatting
dotnet format --verify-no-changes SdJwt.Net.sln

# Check Markdown formatting
npx prettier@3.2.5 --check "**/*.md"

# Fix any issues
dotnet format SdJwt.Net.sln
npx prettier@3.2.5 --write "**/*.md"
```

## Troubleshooting

### "dotnet: command not found" on Windows Git Bash

If you see this warning but have .NET installed, it means dotnet is not in your Git Bash PATH. The hook will show a warning but continue. To fix:

**Option 1**: Add dotnet to Git Bash PATH:

```bash
# Add to ~/.bashrc
export PATH="$PATH:/c/Program Files/dotnet"
```

**Option 2**: Use PowerShell hooks instead (they automatically find dotnet):

```powershell
Copy-Item .githooks\pre-commit.ps1 .git\hooks\pre-commit
Copy-Item .githooks\pre-push.ps1 .git\hooks\pre-push
```

### Hook not executing

Verify hooks are configured:

```bash
git config core.hooksPath
# Should output: .githooks
```

Check file permissions (Unix/macOS):

```bash
ls -la .githooks/
# pre-commit, pre-push, commit-msg should have execute permission (x)
```

### Line ending issues

The `.gitattributes` file ensures hooks always use LF line endings. If you manually edited hooks on Windows, normalize them:

```bash
git add --renormalize .githooks/
```

## Bypassing the Hook (Not Recommended)

In rare cases where you need to commit or push without running hooks:

```bash
git commit --no-verify -m "message"
git push --no-verify
```

However, this is **not recommended** as it will cause CI failures.
