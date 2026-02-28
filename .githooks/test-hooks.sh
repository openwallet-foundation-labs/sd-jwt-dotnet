#!/usr/bin/env bash
# Test script to verify Git hooks work correctly
# Run this to test pre-commit and pre-push hooks

set -e

echo "==> Testing Git Hooks Cross-Platform Compatibility"
echo ""

# Test 1: Pre-commit hook (bash version)
echo "Test 1: Pre-commit hook (bash)"
if bash .githooks/pre-commit; then
    echo "✓ Pre-commit (bash) passed"
else
    echo "✗ Pre-commit (bash) failed"
    exit 1
fi
echo ""

# Test 2: Pre-push hook (bash version)
echo "Test 2: Pre-push hook (bash)"
if bash .githooks/pre-push; then
    echo "✓ Pre-push (bash) passed"
else
    echo "✗ Pre-push (bash) failed"
    exit 1
fi
echo ""

# Test 3: Pre-commit hook (PowerShell version) - if pwsh is available
if command -v pwsh &> /dev/null; then
    echo "Test 3: Pre-commit hook (PowerShell)"
    if pwsh .githooks/pre-commit.ps1; then
        echo "✓ Pre-commit (PowerShell) passed"
    else
        echo "✗ Pre-commit (PowerShell) failed"
        exit 1
    fi
    echo ""

    echo "Test 4: Pre-push hook (PowerShell)"
    if pwsh .githooks/pre-push.ps1; then
        echo "✓ Pre-push (PowerShell) passed"
    else
        echo "✗ Pre-push (PowerShell) failed"
        exit 1
    fi
    echo ""
else
    echo "Skipping PowerShell tests (pwsh not found)"
    echo ""
fi

# Test 5: Verify hooks are installed
echo "Test 5: Verify hooks configuration"
hooks_path=$(git config core.hooksPath)
if [ "$hooks_path" = ".githooks" ]; then
    echo "✓ Hooks path configured: $hooks_path"
else
    echo "⚠ Warning: Hooks path not configured (expected: .githooks, got: $hooks_path)"
    echo "  Run: git config core.hooksPath .githooks"
fi
echo ""

echo "==> All tests passed!"
echo ""
echo "The Git hooks are working correctly on your platform."
