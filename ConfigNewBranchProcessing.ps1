param (
    [string]$version  # Napr. 5.1.25
)

# Odstraneni tecek z verze pro katalog
$catalogSuffix = $version -replace '\.', ''
$catalogName = "DOCUX_DEV_$catalogSuffix"

# Ziskani root cesty puvodniho repozitare
$gitRoot = git rev-parse --show-toplevel
$parentDir = Split-Path $gitRoot -Parent

# ========================
# UPRAVA DmsDocuXContext.cs
# ========================
$clonedPath = Join-Path $parentDir $version
$contextPath = Join-Path $clonedPath "BE\Dms\DocuX\Libraries\Socosit.Dms.DocuX.Dl\DmsDocuXContext.cs"

if (-not (Test-Path $contextPath)) {
    Write-Host "Error: Soubor nenalezen: $contextPath"
    exit 1
}

Write-Host "Uprava souboru: $contextPath"
$contextContent = Get-Content $contextPath -Raw
$contextContent = $contextContent -replace 'initial catalog=[^;"]+', "initial catalog=$catalogName"
Set-Content -Path $contextPath -Value $contextContent -Encoding UTF8
Write-Host "Katalog v DmsDocuXContext.cs byl upraven na: $catalogName"

# ========================
# UPRAVA globalAppsettings.json
# ========================
$appsettingsPath = Join-Path $clonedPath "BE\Configs\devConfigs.DEV.json"

if (-not (Test-Path $appsettingsPath)) {
    Write-Host "Error: Soubor nenalezen: $appsettingsPath"
    exit 1
}

Write-Host "Uprava souboru: $appsettingsPath"
$appsettingsContent = Get-Content $appsettingsPath -Raw
$appsettingsContent = $appsettingsContent -replace 'initial catalog=[^;"]+', "initial catalog=$catalogName"
Set-Content -Path $appsettingsPath -Value $appsettingsContent -Encoding UTF8
Write-Host "Katalog v globalAppsettings.json byl upraven na: $catalogName"
