param(
    [string]$Configuration = "Release",
    [string]$GameDir = "E:\SOFT\STEAM\steamapps\common\Chill with You Lo-Fi Story"
)

dotnet build "$PSScriptRoot\ChillPatcherLite.csproj" `
    -c $Configuration `
    -p:GameDir=$GameDir `
    --nologo

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Built: $PSScriptRoot\bin\$Configuration\ChillPatcherLite.dll"
}
