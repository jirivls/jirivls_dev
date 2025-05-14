# Ziskani root cesty repository
$gitRoot = git rev-parse --show-toplevel

# Slozeni cesty k config.json
$configPath = Join-Path $gitRoot "Tools.Binaries\Publisher\config.json"

# Precteni configu
$config = Get-Content $configPath -Raw | ConvertFrom-Json

# Nastaveni API tokenu
$gitToken = $config.Jit.GitApiToken

# Vytazeni posledni verze
$fullVersion = $config.DMS.LastVersion

# Nacteni bez build verze
$segments = ($fullVersion -split '\.')

# Poskladani verze zpet bez build cisla (no increment!)
$nextVersion = ($segments[0..2] -join '.')

Write-Host "New version (auto-incremented): $nextVersion"

# Pouzije se jako jmeno pro novou branch i slozku
$BranchName = $nextVersion

# Kontrola: pokud branch uz existuje tak konec skriptu
$remoteBranchExists = git ls-remote --heads origin $BranchName
if ($remoteBranchExists) {
    Write-Host "Branch '$BranchName' already exists on remote. Exiting script."
    exit 1
}

# Priprava slozky pro klon
$parentDir = Split-Path $gitRoot -Parent
$cloneDir = Join-Path $parentDir $BranchName

# Kontrola: pokud slozka uz existuje tak konec skriptu
if (Test-Path $cloneDir) {
    Write-Host "Error: Directory '$cloneDir' already exists. Remove or rename it first."
    exit 1
}

# Kontrola: je spousteno na Develop branch
$currentBranch = git rev-parse --abbrev-ref HEAD
if ($currentBranch -ne "Develop") {
    Write-Host "Warning: You are not on 'Develop' branch. You are on '$currentBranch'."
    exit 1
}

# Kontrola: nejsou necommitnute soubory
$hasUncommittedChanges = $(git status --porcelain)
if ($hasUncommittedChanges.Count -gt 0) {
    Write-Host "Warning: You have uncommitted changes on branch '$currentBranch'. Commit or stash them before continuing."
    exit 1
}

# Prepnuti do Develop branch
git checkout Develop
Write-Host "Switched to Develop branch."

# Vytvoreni nove branch
git checkout -b $BranchName
git push origin $BranchName

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to push branch '$BranchName' to origin."
    exit 1
}

Write-Host "Branch $BranchName created and pushed to remote."

# Ziskani repo URL
$repoUrl = git remote get-url origin
Write-Host "Remote repository URL: $repoUrl"

# Pridani tokenu
$authGitUrl = $repoUrl -replace "https://", "https://$gitToken@"

# Naklonovani branch na lokal
Write-Host "Cloning repository with only branch $BranchName..."
git clone --single-branch --branch $BranchName $authGitUrl $cloneDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Git clone failed. Exiting script."
    exit 1
}

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

nsatavit cofnig verzi