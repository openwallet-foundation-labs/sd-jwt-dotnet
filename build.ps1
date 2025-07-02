$ErrorActionPreference = "Stop"
$CoverageThreshold = 90 # Set the minimum code coverage percentage here

Write-Host "========================================" -ForegroundColor Green
Write-Host "  Building SD-JWT .NET Solution"
Write-Host "========================================"

# --- Define Paths ---
$SolutionFile = "SdJwt.Net.sln"
$TestResultsDirectory = "TestResults"
$TrxLogFile = Join-Path $TestResultsDirectory "test-results.trx"
$HtmlReportFile = Join-Path $TestResultsDirectory "test-report.html"

# 1. Clean Solution
Write-Host "`n[1/6] Cleaning solution..."
if (Test-Path $TestResultsDirectory) { Remove-Item $TestResultsDirectory -Recurse }
dotnet clean $SolutionFile --configuration Release
if ($LASTEXITCODE -ne 0) { throw "Clean failed." }

# 2. Restore Dependencies
Write-Host "`n[2/6] Restoring NuGet packages..."
dotnet restore $SolutionFile
if ($LASTEXITCODE -ne 0) { throw "Restore failed." }

# 3. Build Solution
Write-Host "`n[3/6] Building solution in Release mode..."
dotnet build $SolutionFile --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "Build failed." }

# 4. Run Tests, Collect Coverage, and Generate TRX Report
Write-Host "`n[4/6] Running tests and collecting code coverage..."
$testCommand = "dotnet test $SolutionFile --configuration Release --no-build --settings tests.runsettings --logger `"trx;LogFileName=$TrxLogFile`" /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=$CoverageThreshold /p.ThresholdType=line"

try {
    Invoke-Expression -Command $testCommand
}
catch {
    Write-Host "Test execution failed. Check logs for details." -ForegroundColor Red
    # Re-throw the exception to fail the script
    throw
}

Write-Host "Code coverage check passed (>= $CoverageThreshold%)." -ForegroundColor Green

# 5. Generate HTML Test Report from TRX file
Write-Host "`n[5/6] Generating HTML test report..."
try {
    # Check if trx-to-html is available, if not, provide instructions.
    if (-not (Get-Command trx-to-html -ErrorAction SilentlyContinue)) {
        Write-Host "Warning: 'trx-to-html' global tool not found. Skipping HTML report generation." -ForegroundColor Yellow
        Write-Host "Install it by running: dotnet tool install -g trx-to-html" -ForegroundColor Yellow
    }
    else {
        trx-to-html $TrxLogFile --output $HtmlReportFile
        Write-Host "HTML report generated at: $HtmlReportFile" -ForegroundColor Cyan
    }
}
catch {
    Write-Host "Failed to generate HTML report. Ensure 'trx-to-html' is installed and up-to-date." -ForegroundColor Yellow
}


# 6. Pack NuGet Package
Write-Host "`n[6/6] Packing NuGet package..."
if (Test-Path artifacts) { Remove-Item artifacts -Recurse }
$projectPath = "src/SdJwt.Net/SdJwt.Net.csproj"
dotnet pack $projectPath --configuration Release --no-build --output ./artifacts
if ($LASTEXITCODE -ne 0) { throw "Pack failed." }

$packageName = (Get-Item $projectPath).Name -replace ".csproj", ""
$version = (Select-Xml -Path $projectPath -XPath "/Project/PropertyGroup/Version").Node.InnerText
$packageFile = "artifacts/$packageName.$version.nupkg"

Write-Host "========================================"
Write-Host "  Build successful!"
Write-Host "  NuGet package created at: $packageFile" -ForegroundColor Green
Write-Host "========================================"