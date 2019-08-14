Write-Host "Script was originally running in: " $(Get-Location)
Push-Location
Set-Location $(Join-Path $PSScriptRoot ..)
Write-Host "Script has changed location to: " $(Get-Location)

dotnet publish ../check-yo-self-indexer/check-yo-self-indexer.csproj -c Release -r linux-musl-x64
dotnet publish ./check-yo-self-indexer-sdk.csproj -o ./out
dotnet pack ./check-yo-self-indexer-sdk.csproj /p:PackageVersion=1.0.7-beta --configuration Debug --include-source --include-symbols --output ./nupkg --version-suffix "beta"

Pop-Location
