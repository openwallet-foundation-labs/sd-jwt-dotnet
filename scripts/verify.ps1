param(
    [string]$Solution = "SdJwt.Net.sln",
    [string]$Configuration = "Release",
    [switch]$SkipFormat,
    [switch]$SkipVulnerabilityScan
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

function Invoke-VerificationStep {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [scriptblock]$Action
    )

    Write-Host ""
    Write-Host "==> $Name"
    & $Action
}

Invoke-VerificationStep -Name "Restore dependencies" -Action {
    dotnet restore $Solution
}

Invoke-VerificationStep -Name "Build solution ($Configuration)" -Action {
    dotnet build $Solution --configuration $Configuration --no-restore
}

Invoke-VerificationStep -Name "Run tests" -Action {
    dotnet test $Solution --configuration $Configuration --no-build --verbosity normal
}

if (-not $SkipFormat) {
    Invoke-VerificationStep -Name "Verify code formatting" -Action {
        dotnet format --verify-no-changes --verbosity normal $Solution
    }

    Invoke-VerificationStep -Name "Verify Markdown formatting" -Action {
        $mdFiles = Get-ChildItem -Path . -Filter '*.md' -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -notmatch '[\\\/](node_modules|.git)[\\\/]' }

        if ($mdFiles) {
            Write-Host "Checking $($mdFiles.Count) Markdown files..."
            $relativePaths = $mdFiles | ForEach-Object { $_.FullName.Replace((Get-Location).Path + [IO.Path]::DirectorySeparatorChar, '') }
            npx --yes prettier@3.2.5 --check $relativePaths
        }
        else {
            Write-Host "No Markdown files found"
        }
    }
}

if (-not $SkipVulnerabilityScan) {
    Invoke-VerificationStep -Name "Scan for vulnerable packages" -Action {
        $vulnerabilityOutput = dotnet list $Solution package --vulnerable --include-transitive | Out-String
        Write-Host $vulnerabilityOutput

        if ($vulnerabilityOutput -match "has the following vulnerable packages") {
            throw "Vulnerable packages detected."
        }
    }
}

Write-Host ""
Write-Host "Verification completed successfully."
