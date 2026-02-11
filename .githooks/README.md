# Git Hooks for SD-JWT .NET

This directory contains Git hooks that help maintain code quality and compliance.

## Automatic Sign-Off Hook

The `commit-msg` hook automatically adds a `Signed-off-by:` line to every commit message, ensuring DCO (Developer Certificate of Origin) compliance.

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
- **Team consistency**: All team members use the same process
- **PR approval**: Prevents DCO check failures in pull requests

## Testing

Test that the hook works:

```bash
# Make a test commit
git commit --allow-empty -m "Test commit"

# Verify the sign-off was added
git log -1 --format="%B"
```

You should see your commit message with `Signed-off-by:` automatically appended.

## Bypassing the Hook (Not Recommended)

In rare cases where you need to commit without running hooks:
```bash
git commit --no-verify -m "message"
```

However, this is **not recommended** as it will cause DCO compliance failures.
