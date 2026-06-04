$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$localSettingsPath = Join-Path $repoRoot "Fotografia.Api\appsettings.Local.json"
$createLocalSettingsScript = Join-Path $PSScriptRoot "create-appsettings-local.ps1"
$projectPath = Join-Path $repoRoot "Fotografia.Api\Fotografia.Api.csproj"

if (-not (Test-Path -LiteralPath $localSettingsPath)) {
    & $createLocalSettingsScript
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

dotnet run --project $projectPath
