# Setting Up Git Hooks for Automatic Sign-Off

This project requires all commits to include a `Signed-off-by:` line for DCO (Developer Certificate of Origin) compliance.

## One-Time Setup (Do This Now!)

To automatically add sign-off to all your commits, run this command in your repository:

```bash
git config core.hooksPath .githooks
```

**That's it!** Every commit you make from now on will automatically include your sign-off.

## What This Does

After setup, when you commit:

```bash
git commit -m "Add new feature"
```

The hook automatically transforms your commit message to:

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
- ✅ All your commits are DCO-compliant
- ✅ Your PRs won't be blocked by DCO checks
- ✅ No need to remember the `-s` flag

## Alternative: Manual Sign-Off

If you prefer not to use hooks, you can manually add sign-off using:

```bash
git commit -s -m "Your commit message"
```

But we **strongly recommend** using the automated hook to prevent forgotten sign-offs.

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
