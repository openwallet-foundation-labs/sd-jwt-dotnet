#!/usr/bin/env pwsh
# PowerShell version of pre-commit hook for Windows users
# Verifies code and Markdown formatting before committing

Write-Host "Running pre-commit formatting checks..."

# Check C# code formatting
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    Write-Host "Checking C# code formatting..."

    $formatResult = dotnet format --verify-no-changes --verbosity quiet SdJwt.Net.sln 2>&1
    $formatExitCode = $LASTEXITCODE

    if ($formatExitCode -ne 0) {
        Write-Host ""
        Write-Host "ERROR: C# formatting issues detected." -ForegroundColor Red
        Write-Host "Run: dotnet format SdJwt.Net.sln" -ForegroundColor Yellow
        exit 1
    }
    Write-Host "C# formatting check passed." -ForegroundColor Green
}
else {
    Write-Host "Warning: dotnet not found, skipping C# format check." -ForegroundColor Yellow
}

# Check Markdown formatting for staged files
$stagedMdFiles = git diff --cached --name-only --diff-filter=ACM | Where-Object { $_ -match '\.md$' }

if ($stagedMdFiles) {
    Write-Host "Checking Markdown formatting for $($stagedMdFiles.Count) staged file(s)..."

    $prettierResult = npx --yes prettier@3.2.5 --check $stagedMdFiles 2>&1
    $prettierExitCode = $LASTEXITCODE

    if ($prettierExitCode -ne 0) {
        Write-Host ""
        Write-Host "ERROR: Markdown formatting issues detected." -ForegroundColor Red
        Write-Host "Run: npx prettier@3.2.5 --write '**/*.md'" -ForegroundColor Yellow
        Write-Host "Then re-stage your files with: git add <files>" -ForegroundColor Yellow
        exit 1
    }
    Write-Host "Markdown formatting check passed." -ForegroundColor Green
}
else {
    Write-Host "No staged Markdown files to check."
}

Write-Host "All formatting checks passed." -ForegroundColor Green
