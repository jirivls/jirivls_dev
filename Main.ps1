# Ziskani root cesty repository
$gitRoot = git rev-parse --show-toplevel
$configPath = Join-Path $gitRoot "Tools.Binaries\Publisher\config.json"

Write-Host "Cteni config.json pro ziskani verze"
$config = Get-Content $configPath -Raw | ConvertFrom-Json
$fullVersion = $config.DMS.LastVersion
Write-Host "Aktualni verze z config.json: $fullVersion"

# Rozdeleni verze
$segments = $fullVersion -split '\.'
$major = [int]$segments[0]
$minor = [int]$segments[1]
$patch = [int]$segments[2]
$build = [int]$segments[3]

# Verze pro branch (soucasna verze bez build)
$branchVersion = "$major.$minor.$patch"

# Verze nova (patch +1, build = 0)
$nextVersion = "$major.$minor." + ($patch + 1) + ".0"

Write-Host "Verze pro novou branch: $branchVersion"
Write-Host "Nova verze pro develop: $nextVersion"

# ========================
# SPUSTENI GitNewBranchProcessing
# ========================

Write-Host "Spousteni skriptu GitNewBranchProcessing.ps1"
# Spusteni skriptu pro vytvoreni branch
& "$PSScriptRoot\GitNewBranchProcessing.ps1" -version $branchVersion

# ========================
# SPUSTENI GitNewBranchProcessing
# ========================
Write-Host "Spousteni skriptu ConfigDevelopBranchProcessing.ps1"
# Spusteni skriptu pro upravu develop configu
& "$PSScriptRoot\ConfigDevelopBranchProcessing.ps1" -version $nextVersion

# ========================
# SPUSTENI SP SP_AdminCreateNewAppication,GetApplicationInfo.ps1,ConfigDevelopBranchProcessing.ps1
# ========================

# Spustit SP pro novou verzi v adminovi
Write-Host "Naplneni connection string a overeni pro sql prikazy"
# Spusteni query pro ziskani hodnot posledne zalozene aplikace
$server   = "192.168.4.1"
$database = "DOCUX51_DEV_ADMIN"
$user     = "uzivatel"
$password = "heslo"

Write-Host "Spousteni SP_AdminCreateNewAppication pro zalozeni a zprocesovani nove aplikace"
# Command v DOCUX51_DEV_ADMIN pro zalozeni nove aplikace
$sqlQuery = "EXEC SP_AdminCreateNewAppication @NewVersion = N'$branchVersion'"

sqlcmd -S $server `
                 -d $database `
                 -U $user `
                 -P $password `
                 -Q "`"$sqlQuery`"" `
                 -h -1 -s ";" -W 2>&1
                 
Write-Host "Spousteni SQL query pro ziskani informaci posledni aplikace pomoci sqlcmd"
# Query DOCUX51_DEV_ADMIN.dbo.Applications pro ziskani informaci a posledne zalozene aplikaci pro update config a dbContext souboru

$values = & "$PSScriptRoot\GetApplicationInfo.ps1" -server $server -database $database -user $user -password $password

$applicationUid   = $values[0]
$applicationName  = $values[1]
$applicationKey   = $values[2]
$licenceKey       = $values[3]
$applicationId    = $values[4]

Write-Host "Hodnoty posledni aplikace pro aktualizaci config.json a DmsDocuXContext.cs: Name=$applicationName, Key=$applicationKey, UID=$applicationUid, Licence=$licenceKey, ID=$applicationId"

Write-Host "Spousteni skriptu ConfigNewBranchProcessing.ps1"
# Spusteni skriptu pro upravu nove branch configu
& "$PSScriptRoot\ConfigNewBranchProcessing.ps1" `
    -version $branchVersion `
    -ApplicationKey $applicationKey `
    -ApplicationUid $applicationUid `
    -LicenceKey $licenceKey `
    -ApplicationId $applicationId

# ========================
# SPUSTENI CopyFiles.ps1
# ========================

Write-Host "Spousteni skriptu CopyFiles.ps1 pro kopirovani instalacniho balicku agenta"
# Prekopirovani instalcniho balicku agenta do nove odlite verze z Develop
& "$PSScriptRoot\CopyFiles.ps1" -version $branchVersion