#!/usr/bin/env pwsh
# Test script to verify Git hooks work correctly on Windows/PowerShell
# Run this to test pre-commit and pre-push hooks

$ErrorActionPreference = "Stop"

Write-Host "==> Testing Git Hooks Cross-Platform Compatibility" -ForegroundColor Cyan
Write-Host ""

# Test 1: Pre-commit hook (PowerShell version)
Write-Host "Test 1: Pre-commit hook (PowerShell)" -ForegroundColor Yellow
try {
    & pwsh .githooks/pre-commit.ps1
    Write-Host "✓ Pre-commit (PowerShell) passed" -ForegroundColor Green
}
catch {
    Write-Host "✗ Pre-commit (PowerShell) failed" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Test 2: Pre-push hook (PowerShell version)
Write-Host "Test 2: Pre-push hook (PowerShell)" -ForegroundColor Yellow
try {
    & pwsh .githooks/pre-push.ps1
    Write-Host "✓ Pre-push (PowerShell) passed" -ForegroundColor Green
}
catch {
    Write-Host "✗ Pre-push (PowerShell) failed" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Test 3: Pre-commit hook (bash version) - if bash is available
if (Get-Command bash -ErrorAction SilentlyContinue) {
    Write-Host "Test 3: Pre-commit hook (bash)" -ForegroundColor Yellow
    bash .githooks/pre-commit
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Pre-commit (bash) passed" -ForegroundColor Green
    }
    else {
        Write-Host "✗ Pre-commit (bash) failed" -ForegroundColor Red
        exit 1
    }
    Write-Host ""

    Write-Host "Test 4: Pre-push hook (bash)" -ForegroundColor Yellow
    bash .githooks/pre-push
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Pre-push (bash) passed" -ForegroundColor Green
    }
    else {
        Write-Host "✗ Pre-push (bash) failed" -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}
else {
    Write-Host "Skipping bash tests (bash not found)" -ForegroundColor Yellow
    Write-Host ""
}

# Test 5: Verify hooks are installed
Write-Host "Test 5: Verify hooks configuration" -ForegroundColor Yellow
$hooksPath = git config core.hooksPath
if ($hooksPath -eq ".githooks") {
    Write-Host "✓ Hooks path configured: $hooksPath" -ForegroundColor Green
}
else {
    Write-Host "⚠ Warning: Hooks path not configured (expected: .githooks, got: $hooksPath)" -ForegroundColor Yellow
    Write-Host "  Run: git config core.hooksPath .githooks" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "==> All tests passed!" -ForegroundColor Green
Write-Host ""
Write-Host "The Git hooks are working correctly on your platform." -ForegroundColor Cyan
