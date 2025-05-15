param (
    [string]$version,
    [string]$ApplicationKey,
    [string]$ApplicationUid,
    [string]$LicenceKey,
    [string]$ApplicationId
)

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
$contextContent = $contextContent -replace 'initial catalog=[^;"]+', "initial catalog=$ApplicationKey"
Set-Content -Path $contextPath -Value $contextContent -Encoding UTF8
Write-Host "Katalog v DmsDocuXContext.cs byl upraven na: $ApplicationKey"

# ========================
# UPRAVA globalAppsettings.json
# ========================
$appsettingsPath = Join-Path $clonedPath "BE\Configs\devConfigs.DEV.json"

if (-not (Test-Path $appsettingsPath)) {
    Write-Host "Error: Soubor nenalezen: $appsettingsPath"
    exit 1
}

# Nacteni obsahu JSON
$appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json

Write-Host "Aktualizace hodnot v DevConfigs"

$appsettings.DevConfigs.DmsConnectionString = $appsettings.DevConfigs.DmsConnectionString -replace 'initial catalog=[^;"]+', "initial catalog=$ApplicationKey"
$appsettings.DevConfigs.ApplicationId = [int]$ApplicationId
$appsettings.DevConfigs.Licence.ApplicationId = [int]$ApplicationId
$appsettings.DevConfigs.Licence.LicenceKey = $LicenceKey
$appsettings.DevConfigs.Application.Key = $ApplicationKey
$appsettings.DevConfigs.Application.Name = $ApplicationKey
$appsettings.DevConfigs.Application.Uid = $ApplicationUid

# Prevod zpet do JSON a ulozeni
$appsettings | ConvertTo-Json -Depth 10 | Set-Content -Encoding UTF8 $appsettingsPath

Write-Host "DevConfigs v $appsettingsPath byl aktualizovan."
