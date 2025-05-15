$ErrorActionPreference = 'Stop'

function Invoke-ScriptWithExitCheck {
    param (
        [string]$ScriptPath,
        [hashtable]$Arguments
    )

    $argList = $Arguments.GetEnumerator() | ForEach-Object { "-$($_.Key)", "`"$($_.Value)`"" }

    Write-Host "Spousteni: $ScriptPath"
    & {
        powershell -ExecutionPolicy Bypass -File $ScriptPath @argList
    }
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        Write-Host "Skript selhal: $ScriptPath (ExitCode: $exitCode)"
        exit $exitCode
    }
}

# ========================
# Ziskani root cesty repository
# ========================
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
Invoke-ScriptWithExitCheck -ScriptPath "$PSScriptRoot\GitNewBranchProcessing.ps1" -Arguments @{ version = $branchVersion }

# ========================
# SPUSTENI ConfigDevelopBranchProcessing
# ========================
Write-Host "Spousteni skriptu ConfigDevelopBranchProcessing.ps1"
Invoke-ScriptWithExitCheck -ScriptPath "$PSScriptRoot\ConfigDevelopBranchProcessing.ps1" -Arguments @{ version = $nextVersion }

# ========================
# SP SP_AdminCreateNewAppication
# ========================
Write-Host "Naplneni connection string a overeni pro sql prikazy"
$server   = "192.168.4.1"
$database = "DOCUX51_DEV_ADMIN"
$user     = "uzivatel"
$password = "heslo"

Write-Host "Spousteni SP_AdminCreateNewAppication pro zalozeni a zprocesovani nove aplikace"
$sqlQuery = "EXEC SP_AdminCreateNewAppication @NewVersion = N'$branchVersion'"
<#
try {
    sqlcmd -S $server `
           -d $database `
           -U $user `
           -P $password `
           -Q "`"$sqlQuery`"" `
           -h -1 -s ";" -W 2>&1 | Out-Null
} catch {
    Write-Host "Chyba pri volani SP_AdminCreateNewAppication"
    exit 1
}
#>
# ========================
# SPUSTENI GetApplicationInfo.ps1
# ========================
Write-Host "Spousteni SQL query pro ziskani informaci posledni aplikace pomoci sqlcmd"

$scriptPath = "$PSScriptRoot\GetApplicationInfo.ps1"
$cmd = "powershell -ExecutionPolicy Bypass -File `"$scriptPath`" -server `"$server`" -database `"$database`" -user `"$user`" -password `"$password`""

$values = Invoke-Expression $cmd
$exitCode = $LASTEXITCODE

if ($exitCode -ne 0) {
    Write-Host "Chyba ve skriptu GetApplicationInfo.ps1 (ExitCode: $exitCode)"
    exit $exitCode
}

$applicationUid   = $values[0]
$applicationName  = $values[1]
$applicationKey   = $values[2]
$licenceKey       = $values[3]
$applicationId    = $values[4]

Write-Host "Hodnoty posledni aplikace pro aktualizaci config.json a DmsDocuXContext.cs: Name=$applicationName, Key=$applicationKey, UID=$applicationUid, Licence=$licenceKey, ID=$applicationId"

# ========================
# SPUSTENI ConfigNewBranchProcessing.ps1
# ========================
Write-Host "Spousteni skriptu ConfigNewBranchProcessing.ps1"
Invoke-ScriptWithExitCheck -ScriptPath "$PSScriptRoot\ConfigNewBranchProcessing.ps1" -Arguments @{
    version = $branchVersion
    ApplicationKey = $applicationKey
    ApplicationUid = $applicationUid
    LicenceKey = $licenceKey
    ApplicationId = $applicationId
}

# ========================
# SPUSTENI CopyFiles.ps1
# ========================
Write-Host "Spousteni skriptu CopyFiles.ps1 pro kopirovani instalacniho balicku agenta"
Invoke-ScriptWithExitCheck -ScriptPath "$PSScriptRoot\CopyFiles.ps1" -Arguments @{ version = $branchVersion }

Write-Host "Vsechny skripty probehly uspesne."
