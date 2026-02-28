# Git Hooks for SD-JWT .NET

This directory contains Git hooks that help maintain code quality and compliance.

## Available Hooks

### commit-msg - Automatic Sign-Off

The `commit-msg` hook automatically adds a `Signed-off-by:` line to every commit message, ensuring DCO (Developer Certificate of Origin) compliance. It also validates Conventional Commits format.

### pre-push - Markdown Formatting Check

The `pre-push` hook verifies that changed Markdown files pass Prettier formatting checks before pushing. This prevents CI failures due to formatting issues.

## Installation

### For All Contributors (Recommended)

Configure Git to use this hooks directory:

```bash
git config core.hooksPath .githooks
```

This command needs to be run once per clone of the repository.

### Manual Installation (Alternative)

If you prefer the traditional `.git/hooks` approach:

**On Linux/macOS:**

```bash
cp .githooks/commit-msg .git/hooks/commit-msg
chmod +x .git/hooks/commit-msg
```

**On Windows (PowerShell):**

```powershell
Copy-Item .githooks\commit-msg .git\hooks\commit-msg
```

## How It Works

When you run `git commit`, the hook automatically:

1. Checks if your commit message already has a `Signed-off-by:` line
2. If not, it adds `Signed-off-by: Your Name <your.email@example.com>` using your Git configuration
3. Shows a confirmation message

## Benefits

- **No more forgotten sign-offs**: Every commit is automatically DCO-compliant
- **No manual `-s` flag needed**: Just commit normally with `git commit -m "message"`
- **Catch formatting issues early**: Markdown formatting is checked before push
- **Team consistency**: All team members use the same process
- **PR approval**: Prevents DCO and formatting check failures in pull requests

## Testing

Test that the commit-msg hook works:

```bash
# Make a test commit
git commit --allow-empty -m "test: verify hooks"

# Verify the sign-off was added
git log -1 --format="%B"
```

Test that the pre-push hook works:

```bash
# Check markdown formatting manually
npx prettier@3.2.5 --check "**/*.md"

# Fix any issues
npx prettier@3.2.5 --write "**/*.md"
```

## Bypassing the Hook (Not Recommended)

In rare cases where you need to commit or push without running hooks:

```bash
git commit --no-verify -m "message"
git push --no-verify
```

However, this is **not recommended** as it will cause CI failures.
