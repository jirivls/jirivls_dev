# Ziskani root cesty repository
$gitRoot = git rev-parse --show-toplevel

# Slozeni cesty k config.json
$configPath = Join-Path $gitRoot "Tools.Binaries\Publisher\config.json"

# Precteni configu
$config = Get-Content $configPath -Raw | ConvertFrom-Json

# Nastaveni API tokenu
$gitToken = "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"

# Vytazeni posledni verze
$fullVersion = $config.DMS.LastVersion

# Nacteni bez build verze
$segments = ($fullVersion -split '\.')

# Zvyseni PATCH verze o 1
$segments[2] = ([int]$segments[2]) + 1

# Poskladani verze zpet bez build cisla
$nextVersion = ($segments[0..2] -join '.')

Write-Host "New version (auto-incremented): $nextVersion"

# Pouzije se jako jmeno pro novou branch i slozku
$BranchName = $nextVersion

# Prepnuti do Develop branch
git checkout Develop
Write-Host "Switched to Develop branch."

# Vytvoreni nove branch
git checkout -b $BranchName
git push origin $BranchName
Write-Host "Branch $BranchName created and pushed to remote."

# Ziskani repo URL
$repoUrl = git remote get-url origin
Write-Host "Remote repository URL: $repoUrl"

# Pridani tokenu
$authGitUrl = $repoUrl -replace "https://", "https://$gitToken@"

# Naklonovani branch na lokal
$parentDir = Split-Path $gitRoot -Parent
$cloneDir = Join-Path $parentDir $BranchName
Write-Host "Cloning repository with only branch $BranchName..."
git clone --single-branch --branch $BranchName $authGitUrl $cloneDir

# Prepnuti do nove branch
Set-Location $cloneDir

# Prepnuti zpet do Develop branch
Set-Location $gitRoot

# Odstraneni nove branch z lokalne trackovanych
git checkout Develop
Write-Host "Switched back to Develop branch."

Write-Host "Removing local tracking of branch $BranchName in Develop repository..."
git branch -d $BranchName
git fetch --prune

Write-Host "Branch setup completed successfully. The new branch tracks all remote branches, and Develop no longer tracks it."