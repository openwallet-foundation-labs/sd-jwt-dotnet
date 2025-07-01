$ErrorActionPreference = "Stop"
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Building SD-JWT .NET Solution"
Write-Host "========================================"
Write-Host "`n[1/5] Cleaning solution..."
dotnet clean --configuration Release
Write-Host "`n[2/5] Restoring NuGet packages..."
dotnet restore SdJwt.Net.sln
Write-Host "`n[3/5] Building solution in Release mode..."
dotnet build SdJwt.Net.sln --configuration Release --no-restore
Write-Host "`n[4/5] Running tests..."
dotnet test SdJwt.Net.sln --configuration Release --no-build
Write-Host "`n[5/5] Packing NuGet package..."
$projectPath = "src/SdJwt.Net/SdJwt.Net.csproj"
if (Test-Path artifacts) { Remove-Item artifacts -Recurse }
dotnet pack $projectPath --configuration Release --no-build --output ./artifacts
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Build successful! Package(s) in ./artifacts"
Write-Host "========================================"