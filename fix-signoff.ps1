#!/usr/bin/env pwsh
# Script to add Signed-off-by to commits without rebasing conflicts
# This works by rewriting the commit graph with updated messages only

param(
    [int]$CommitCount = 75
)

Write-Host "Adding Signed-off-by to last $CommitCount commits..." -ForegroundColor Cyan

# Get git user info
$gitName = git config user.name
$gitEmail = git config user.email
$signoffLine = "Signed-off-by: $gitName <$gitEmail>"

Write-Host "Using: $signoffLine" -ForegroundColor Yellow

# Ensure we're on develop branch and clean
$branch = git rev-parse --abbrev-ref HEAD
if ($branch -ne "develop") {
    Write-Error "Must be on develop branch. Currently on: $branch"
    exit 1
}

# Check for uncommitted changes to tracked files (ignore untracked)
$status = git diff --name-only
$stagedStatus = git diff --cached --name-only
if ($status -or $stagedStatus) {
    Write-Error "Working directory has uncommitted changes. Please commit or stash changes first."
    exit 1
}

# Create a backup branch just in case
$backupBranch = "develop-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
git branch $backupBranch
Write-Host "Created backup branch: $backupBranch" -ForegroundColor Green

# Use git interpret-trailers with filter-branch for Windows compatibility
# This is the most reliable method for adding trailers without code conflicts
Write-Host "`nProcessing commits..." -ForegroundColor Cyan

$env:FILTER_BRANCH_SQUELCH_WARNING = "1"

# Create a temporary script file for the msg-filter
$tempScript = Join-Path $env:TEMP "git-msg-filter-$(Get-Date -Format 'yyyyMMddHHmmss').ps1"
@'
$msg = $input | Out-String
if ($msg -notmatch 'Signed-off-by:') {
    $msg = $msg.TrimEnd()
    $msg + "`n`nSigned-off-by: {0} <{1}>" -f $env:GIT_AUTHOR_NAME, $env:GIT_AUTHOR_EMAIL
} else {
    $msg
}
'@ | Set-Content -Path $tempScript -Encoding UTF8

try {
    # Run filter-branch with PowerShell script as msg-filter
    $result = git filter-branch -f --msg-filter "pwsh -NoProfile -File `"$tempScript`"" "HEAD~$CommitCount..HEAD" 2>&1

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✓ Successfully added Signed-off-by to commits!" -ForegroundColor Green

        # Verify a few commits
        Write-Host "`nVerifying commits..." -ForegroundColor Cyan
        $recentCommits = git log --format="%h %s" -5
        $recentCommits | ForEach-Object { Write-Host "  $_" }

        # Check one commit message in detail
        Write-Host "`nSample commit message:" -ForegroundColor Cyan
        git log -1 --format="%B" HEAD | Write-Host -ForegroundColor Gray

        Write-Host "`nTo push these changes, run:" -ForegroundColor Yellow
        Write-Host "  git push --force-with-lease origin develop" -ForegroundColor White
        Write-Host "`nIf something went wrong, restore with:" -ForegroundColor Yellow
        Write-Host "  git reset --hard $backupBranch" -ForegroundColor White
    }
    else {
        Write-Error "git filter-branch failed. Output: $result"
        Write-Host "Restoring from backup..." -ForegroundColor Yellow
        git reset --hard $backupBranch
        exit 1
    }
}
finally {
    # Cleanup temp script
    if (Test-Path $tempScript) {
        Remove-Item $tempScript -Force
    }
}
