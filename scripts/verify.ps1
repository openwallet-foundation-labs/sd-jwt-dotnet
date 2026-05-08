param(
    [string]$Solution = "SdJwt.Net.sln",
    [string]$Configuration = "Release",
    [switch]$SkipFormat,
    [switch]$SkipVulnerabilityScan
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
$DotNetStabilityArgs = @(
    "-m:1",
    "-p:BuildInParallel=false",
    "-p:UseSharedCompilation=false",
    "-p:GeneratePackageOnBuild=false"
)

function Invoke-DotNet {
    param(
        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]]$Arguments
    )

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Get-SolutionProjects {
    $projects = & dotnet sln $Solution list
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet sln $Solution list failed with exit code $LASTEXITCODE."
    }

    return $projects |
        Where-Object { $_ -like "*.csproj" } |
        ForEach-Object { Join-Path (Get-Location) $_ } |
        Sort-Object
}

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
    Invoke-DotNet restore $Solution
}

Invoke-VerificationStep -Name "Build solution ($Configuration)" -Action {
    $projects = Get-SolutionProjects

    foreach ($project in $projects) {
        Write-Host "Building $project"
        Invoke-DotNet build $project --configuration $Configuration --no-restore @DotNetStabilityArgs
    }
}

Invoke-VerificationStep -Name "Run tests" -Action {
    $testProjects = Get-SolutionProjects |
        Where-Object { $_ -match "[\\\/]tests[\\\/].*\.csproj$" }

    foreach ($testProject in $testProjects) {
        Write-Host "Testing $testProject"
        Invoke-DotNet test $testProject --configuration $Configuration --no-build --framework net10.0 --verbosity normal @DotNetStabilityArgs
    }
}

if (-not $SkipFormat) {
    Invoke-VerificationStep -Name "Verify code formatting" -Action {
        try {
            Invoke-DotNet format --verify-no-changes --verbosity normal $Solution
            Write-Host ".NET code is properly formatted." -ForegroundColor Green
        }
        catch {
            Write-Host ""
            Write-Host ".NET code formatting issues detected!" -ForegroundColor Red
            Write-Host ""
            Write-Host "To fix locally, run:"
            Write-Host "  dotnet format $Solution" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "Or commit with the pre-commit hook enabled (it auto-formats)."
            throw
        }
    }

    Invoke-VerificationStep -Name "Verify Markdown formatting" -Action {
        $mdFiles = Get-ChildItem -Path . -Filter '*.md' -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -notmatch '[\\\/](node_modules|.git)[\\\/]' }

        if ($mdFiles) {
            Write-Host "Checking $($mdFiles.Count) Markdown files..."
            $relativePaths = $mdFiles | ForEach-Object { $_.FullName.Replace((Get-Location).Path + [IO.Path]::DirectorySeparatorChar, '') }
            
            try {
                npx --yes prettier@3.2.5 --check $relativePaths
                if ($LASTEXITCODE -ne 0) {
                    throw "Markdown formatting check failed with exit code $LASTEXITCODE."
                }
                Write-Host "All Markdown files are properly formatted." -ForegroundColor Green
            }
            catch {
                Write-Host ""
                Write-Host "Markdown formatting issues detected!" -ForegroundColor Red
                Write-Host ""
                Write-Host "To fix locally, run:"
                Write-Host "  npx prettier@3.2.5 --write `"**/*.md`"" -ForegroundColor Yellow
                Write-Host ""
                Write-Host "Or commit with the pre-commit hook enabled (it auto-formats)."
                throw
            }
        }
        else {
            Write-Host "No Markdown files found"
        }
    }
}

if (-not $SkipVulnerabilityScan) {
    Invoke-VerificationStep -Name "Scan for vulnerable packages" -Action {
        $vulnerabilityOutput = & dotnet list $Solution package --vulnerable --include-transitive | Out-String
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet list $Solution package --vulnerable --include-transitive failed with exit code $LASTEXITCODE."
        }
        Write-Host $vulnerabilityOutput

        if ($vulnerabilityOutput -match "has the following vulnerable packages") {
            throw "Vulnerable packages detected."
        }
    }
}

Write-Host ""
Write-Host "Verification completed successfully."
