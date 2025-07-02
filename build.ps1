$ErrorActionPreference = "Stop"
$CoverageThreshold = 90 # Set the minimum code coverage percentage here
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Building SD-JWT .NET Solution"
Write-Host "========================================"
Write-Host "`n[1/5] Cleaning solution..."
dotnet clean --configuration Release
Write-Host "`n[2/5] Restoring NuGet packages..."
dotnet restore SdJwt.Net.sln
Write-Host "`n[3/5] Building solution in Release mode..."
dotnet build SdJwt.Net.sln --configuration Release --no-restore
Write-Host "`n[4/5] Running tests and collecting code coverage..."
$testCommand = "dotnet test SdJwt.Net.sln --configuration Release --no-build --settings tests.runsettings /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=$CoverageThreshold /p:ThresholdType=line"
# The command will automatically fail if the threshold is not met.
Invoke-Expression -Command $testCommand
Write-Host "Code coverage check passed (>= $CoverageThreshold%)." -ForegroundColor Green
Write-Host "`n[5/5] Packing NuGet package..."
$projectPath = "src/SdJwt.Net/SdJwt.Net.csproj"
if (Test-Path artifacts) { Remove-Item artifacts -Recurse }
dotnet pack $projectPath --configuration Release --no-build --output ./artifacts
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Build successful! Package(s) in ./artifacts"
Write-Host "========================================"