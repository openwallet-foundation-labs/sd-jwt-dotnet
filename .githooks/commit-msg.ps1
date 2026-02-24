#!/usr/bin/env pwsh
# PowerShell version of commit-msg hook for Windows users
# Automatically add Signed-off-by line to commit message if not present

param(
    [string]$CommitMsgFile
)

# Get the commit message
$commitMsg = Get-Content $CommitMsgFile -Raw

# Extract the subject line (first non-empty line without comments)
$subjectLine = (Get-Content $CommitMsgFile | Where-Object { $_ -notmatch '^\s*$' -and $_ -notmatch '^#' }) | Select-Object -First 1

# Regex to validate Conventional Commits
$regex = "^(feat|fix|docs|chore|test|refactor|perf|ci|build|revert|style)(\([a-z0-9_-]+\))?!?: .+"

if ($subjectLine -notmatch $regex) {
    Write-Host "ERROR: Commit message must follow Conventional Commits syntax." -ForegroundColor Red
    Write-Host "Expected format: <type>[optional scope]: <description>" -ForegroundColor Red
    Write-Host "Example: feat(api): add new endpoints" -ForegroundColor Red
    Write-Host "Allowed types: feat, fix, docs, chore, test, refactor, perf, ci, build, revert, style" -ForegroundColor Red
    Write-Host "Your commit message: $subjectLine" -ForegroundColor Red
    exit 1
}

# Get git user info
$name = git config user.name
$email = git config user.email

# Create sign-off line
$signOff = "Signed-off-by: $name <$email>"

# Check if sign-off already exists
if ($commitMsg -notmatch 'Signed-off-by:') {
    # Add sign-off line to the commit message
    Add-Content -Path $CommitMsgFile -Value "`n$signOff"
    Write-Host "✓ Added: $signOff" -ForegroundColor Green
}
