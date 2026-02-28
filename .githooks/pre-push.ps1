#!/usr/bin/env pwsh
# PowerShell version of pre-push hook for Windows users
# Verifies Markdown formatting with Prettier before pushing

Write-Host "Running pre-push checks..."

# Get changed markdown files between local and remote
$changedMd = $null
try {
    $changedMd = git diff --name-only '@{u}..HEAD' -- '*.md' 2>$null
}
catch {
    # Fallback if upstream not set
}

if (-not $changedMd) {
    try {
        $changedMd = git diff --name-only HEAD~1 HEAD -- '*.md' 2>$null
    }
    catch {
        # No commits to compare
    }
}

if ($changedMd) {
    $files = $changedMd -split "`n" | Where-Object { $_ -and (Test-Path $_) }

    if ($files) {
        Write-Host "Checking Markdown formatting: $($files -join ', ')"

        $prettierResult = npx --yes prettier@3.2.5 --check $files 2>&1
        $prettierExitCode = $LASTEXITCODE

        if ($prettierExitCode -ne 0) {
            Write-Host ""
            Write-Host "ERROR: Markdown formatting issues detected." -ForegroundColor Red
            Write-Host "Run: npx prettier@3.2.5 --write <files>" -ForegroundColor Yellow
            Write-Host "Or run: ./scripts/verify.ps1 (includes all checks)" -ForegroundColor Yellow
            exit 1
        }

        Write-Host "Markdown formatting check passed." -ForegroundColor Green
    }
}
else {
    Write-Host "No changed Markdown files to check."
}

Write-Host "Pre-push checks passed." -ForegroundColor Green
