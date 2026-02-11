#!/usr/bin/env pwsh
# PowerShell version of commit-msg hook for Windows users
# Automatically add Signed-off-by line to commit message if not present

param(
    [string]$CommitMsgFile
)

# Get the commit message
$commitMsg = Get-Content $CommitMsgFile -Raw

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
