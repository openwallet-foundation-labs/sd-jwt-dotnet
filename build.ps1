# Stop script on first error
$ErrorActionPreference = "Stop"
$CoverageThreshold = 90 # Set the minimum code coverage percentage here

Write-Host "========================================"
Write-Host "  Building SD-JWT .NET Solution"
Write-Host "========================================" -ForegroundColor Green

# --- Define Paths ---
$SolutionFile = "SdJwt.Net.sln"
$TestResultsDirectory = "TestResults"
$HtmlReportFile = Join-Path $TestResultsDirectory "test-report.html"

# 1. Clean Solution
Write-Host "`n[1/5] Cleaning solution..."
if (Test-Path artifacts) { Remove-Item artifacts -Recurse -Force }
if (Test-Path $TestResultsDirectory) { Remove-Item $TestResultsDirectory -Recurse -Force }
dotnet clean $SolutionFile --configuration Release
if ($LASTEXITCODE -ne 0) { throw "Clean failed." }

# 2. Restore Dependencies
Write-Host "`n[2/5] Restoring NuGet packages..."
dotnet restore $SolutionFile
if ($LASTEXITCODE -ne 0) { throw "Restore failed." }

# 3. Build Solution
Write-Host "`n[3/5] Building solution in Release mode..."
dotnet build $SolutionFile --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "Build failed." }

# 4. Run Tests, Collect Coverage, and Generate HTML Report
Write-Host "`n[4/5] Running tests, collecting coverage, and generating report..."

# Using the call operator (&) with an argument list is safer and more reliable than Invoke-Expression.
# It avoids complex quoting issues and potential script injection vulnerabilities.
$testArgs = @(
    "test",
    $SolutionFile,
    "--configuration", "Release",
    "--no-build",
    "--settings", "tests.runsettings",
    "--logger", "xunit;LogFilePath=$HtmlReportFile;ReportType=html",
    "/p:CollectCoverage=true",
    "/p:CoverletOutputFormat=cobertura",
    "/p:Threshold=$CoverageThreshold",
    "/p:ThresholdType=line"
)

try {
    & dotnet $testArgs
}
catch {
    Write-Host "Test execution or coverage check failed. Check logs for details." -ForegroundColor Red
    if (Test-Path $HtmlReportFile) {
        Write-Host "An HTML report may have been generated for the failed run at: $HtmlReportFile" -ForegroundColor Yellow
    }
    # Re-throw the exception to fail the script
    throw
}

Write-Host "Code coverage check passed (>= $CoverageThreshold%)." -ForegroundColor Green
Write-Host "HTML test report generated at: $HtmlReportFile" -ForegroundColor Cyan


# 5. Pack NuGet Package
Write-Host "`n[5/5] Packing NuGet package..."
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