#!/usr/bin/env pwsh
# PowerShell version of pre-push hook for Windows users
# Verifies Markdown formatting with Prettier before pushing

Write-Host "Running pre-push checks..."

# Find all markdown files in the repository (excluding node_modules and .git)
$mdFiles = Get-ChildItem -Path . -Filter '*.md' -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '[\\/](node_modules|.git)[\\/]' }

if ($mdFiles) {
    Write-Host "Checking Markdown formatting for $($mdFiles.Count) files..."

    $relativePaths = $mdFiles | ForEach-Object { $_.FullName.Replace((Get-Location).Path + [IO.Path]::DirectorySeparatorChar, '') }

    $prettierResult = npx --yes prettier@3.2.5 --check $relativePaths 2>&1
    $prettierExitCode = $LASTEXITCODE

    if ($prettierExitCode -ne 0) {
        Write-Host ""
        Write-Host "ERROR: Markdown formatting issues detected." -ForegroundColor Red
        Write-Host "Run: npx prettier@3.2.5 --write '**/*.md'" -ForegroundColor Yellow
        Write-Host "Or run: ./scripts/verify.ps1 (includes all checks)" -ForegroundColor Yellow
        exit 1
    }

    Write-Host "Markdown formatting check passed." -ForegroundColor Green
}
else {
    Write-Host "No Markdown files found."
}

Write-Host "Pre-push checks passed." -ForegroundColor Green
