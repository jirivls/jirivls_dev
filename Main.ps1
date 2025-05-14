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

# Spusteni skriptu pro vytvoreni branch
& "$PSScriptRoot\GitNewBranchProcessing.ps1" -version $branchVersion

# Spusteni skriptu pro upravu develop configu
& "$PSScriptRoot\ConfigDevelopBranchProcessing.ps1" -version $nextVersion
