# Setting Up Git Hooks for Automatic Sign-Off and Formatting

This project requires all commits to include a `Signed-off-by:` line for DCO (Developer Certificate of Origin) compliance and properly formatted code.

## One-Time Setup (Do This Now!)

To automatically add sign-off and format code on all your commits, run this command in your repository:

```bash
git config core.hooksPath .githooks
```

**That's it!** Every commit you make from now on will:

- Automatically include your sign-off
- Auto-format C# code with `dotnet format`
- Auto-format Markdown files with Prettier

## What This Does

After setup, when you commit:

```bash
git commit -m "Add new feature"
```

The pre-commit hook will:

1. **Auto-format** your C# code using `dotnet format`
2. **Auto-format** staged Markdown files using Prettier
3. **Re-stage** the formatted files
4. **Add sign-off** to your commit message

Your commit message automatically becomes:

```
Add new feature

Signed-off-by: Your Name <your.email@example.com>
```

## Verification

Test that it's working:

```bash
# Make a test commit
git commit --allow-empty -m "Test: verify sign-off hook"

# Check the last commit message
git log -1 --pretty=format:"%B"
```

You should see `Signed-off-by:` at the end of the message.

## For New Contributors

Please make sure to run the setup command above when you first clone this repository. This ensures:

- All your commits are DCO-compliant with automatic sign-off
- Your code is automatically formatted before commit
- Your PRs won't be blocked by formatting or DCO checks
- No need to remember formatting commands or the `-s` flag

**Note:** For bot-created PRs (Release Please, GitHub Copilot, etc.), the `auto-format.yml` workflow will automatically apply formatting fixes if needed.

## Alternative: Manual Formatting and Sign-Off

If you prefer not to use hooks, you can manually format and sign-off:

```bash
# Format code
dotnet format SdJwt.Net.sln
npx prettier@3.2.5 --write "**/*.md"

# Commit with sign-off
git commit -s -m "Your commit message"
```

But we **strongly recommend** using the automated hook to prevent forgotten sign-offs and formatting issues.

## CI Auto-Formatting for Bots

For PRs created by bots (Release Please, GitHub Copilot, Dependabot), the `.github/workflows/auto-format.yml` workflow automatically:

- Detects formatting issues
- Applies `dotnet format` and Prettier fixes
- Commits the changes back to the PR
- Adds a comment notifying that formatting was applied

This ensures CI checks pass even when bots generate files that don't match formatting rules.

## Troubleshooting

### Hook not working?

1. Verify hooks path is set:

   ```bash
   git config core.hooksPath
   ```

   Should output: `.githooks`

2. Re-run the setup command:

   ```bash
   git config core.hooksPath .githooks
   ```

### Already made commits without sign-off?

If you've already committed without sign-off, see [fix-signoff.ps1](fix-signoff.ps1) for a script that adds sign-off to existing commits.
