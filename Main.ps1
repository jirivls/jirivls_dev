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

Write-Host "Spousteni skriptu GitNewBranchProcessing.ps1"
# Spusteni skriptu pro vytvoreni branch
#& "$PSScriptRoot\GitNewBranchProcessing.ps1" -version $branchVersion

Write-Host "Spousteni skriptu ConfigDevelopBranchProcessing.ps1"
# Spusteni skriptu pro upravu develop configu
#& "$PSScriptRoot\ConfigDevelopBranchProcessing.ps1" -version $nextVersion

# Spustit SP pro novou verzi v adminovi

# Spusteni query pro ziskani hodnot posledne zalozene aplikace
Write-Host "Spousteni SQL prikazu pomoci sqlcmd"

# Parametry connection string
$server = "192.168.4.1"
$database = "DOCUX51_DEV_ADMIN"
$user = "uzivatel"
$password = "heslo"

Write-Host "Slozeni SQL query pro ziskani hodnot posledne zalozene aplikace v dbo.Applications"
# Slozeni SQL dotazu
$sqlQuery = "SELECT TOP 1 [Uid], [Name], [Key], [LicenceKey] FROM [dbo].[Applications] ORDER BY ID DESC"

Write-Host "Spusteni SQL query do DB"
# Spusteni prikazu a ulozeni vysledku
$result = sqlcmd -S $server -d $database -U $user -P $password -Q $sqlQuery -s ";" -W

Write-Host "Zobrazeni vysledku:"
$result

Write-Host "Parsovani vysledku pro predani do parametru"
# Parsovani vysledku
$parsed = $result | Where-Object { $_ -and ($_ -notmatch "Uid") } | Select-Object -First 1
$columns = $parsed -split ";"

$applicationUid = $columns[0].Trim()
$applicationName = $columns[1].Trim()
$applicationKey = $columns[2].Trim()
$licenceKey = $columns[3].Trim()

Write-Host "Name: $applicationName, Key: $applicationKey, Uid: $applicationUid, LicenceKey: $licenceKey"

Write-Host "Spousteni skriptu ConfigNewBranchProcessing.ps1"
# Spusteni skriptu pro upravu nove branch configu
& "$PSScriptRoot\ConfigNewBranchProcessing.ps1" `
    -version $branchVersion `
    -ApplicationKey $applicationKey `
    -ApplicationUid $applicationUid `
    -LicenceKey $licenceKey
