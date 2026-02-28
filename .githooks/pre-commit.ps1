#!/usr/bin/env pwsh
# PowerShell version of pre-commit hook for Windows users
# Automatically fixes formatting issues before committing

Write-Host "Running pre-commit auto-formatting..."

# Auto-fix C# code formatting
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    Write-Host "Auto-formatting C# code..."

    $formatResult = dotnet format --verbosity quiet SdJwt.Net.sln 2>&1
    $formatExitCode = $LASTEXITCODE

    if ($formatExitCode -ne 0) {
        Write-Host ""
        Write-Host "ERROR: Failed to format C# code." -ForegroundColor Red
        Write-Host "Please check your code for syntax errors." -ForegroundColor Yellow
        exit 1
    }
    Write-Host "C# formatting applied." -ForegroundColor Green
}
else {
    Write-Host "Warning: dotnet not found, skipping C# format check." -ForegroundColor Yellow
}

# Auto-fix Markdown formatting for staged files
$stagedMdFiles = git diff --cached --name-only --diff-filter=ACM | Where-Object { $_ -match '\.md$' }

if ($stagedMdFiles) {
    Write-Host "Auto-formatting $($stagedMdFiles.Count) staged Markdown file(s)..."

    # Apply Prettier formatting
    $prettierResult = npx --yes prettier@3.2.5 --write $stagedMdFiles 2>&1
    $prettierExitCode = $LASTEXITCODE

    if ($prettierExitCode -ne 0) {
        Write-Host ""
        Write-Host "ERROR: Failed to format Markdown files." -ForegroundColor Red
        exit 1
    }

    # Re-stage the formatted files
    git add $stagedMdFiles
    Write-Host "Markdown formatting applied and files re-staged." -ForegroundColor Green
}
else {
    Write-Host "No staged Markdown files to format."
}

Write-Host "All formatting complete - code is ready for commit." -ForegroundColor Green
