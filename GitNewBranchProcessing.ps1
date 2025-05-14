# Ziskani root cesty repository
$gitRoot = git rev-parse --show-toplevel

Write-Host "Nacteni config.json souboru"
# Slozeni cesty k config.json
$configPath = Join-Path $gitRoot "Tools.Binaries\Publisher\config.json"

Write-Host "Cteni config.json souboru"
# Precteni configu
$config = Get-Content $configPath -Raw | ConvertFrom-Json

Write-Host "Nastaveni Git Api tokenu z configu"
# Nastaveni API tokenu
$gitToken = $config.Jit.GitApiToken

Write-Host "Nacteni posledni verze pro nazev nove branch"
# Vytazeni posledni verze
$fullVersion = $config.DMS.LastVersion

# Nacteni bez build verze
$segments = ($fullVersion -split '\.')

# Poskladani verze zpet bez build cisla (no increment!)
$nextVersion = ($segments[0..2] -join '.')

Write-Host "Nazev nove verze (auto-incremented): $nextVersion"

# Pouzije se jako jmeno pro novou branch i slozku
$BranchName = $nextVersion

Write-Host "Kontrola zdali uz nove vytvarena branch neexistuje na Git"
# Kontrola: pokud branch uz existuje tak konec skriptu
$remoteBranchExists = git ls-remote --heads origin $BranchName
if ($remoteBranchExists) {
    Write-Host "Branch '$BranchName' uz existuje. Konec skriptu."
    exit 1
}

Write-Host "Nastaveni umisteni a nazvu noveho adresare pro klonovanou branch"
# Priprava slozky pro klon
$parentDir = Split-Path $gitRoot -Parent
$cloneDir = Join-Path $parentDir $BranchName

# Kontrola: pokud slozka uz existuje tak konec skriptu
if (Test-Path $cloneDir) {
    Write-Host "Error: Adresar '$cloneDir' uz existuje. Prejmenujte nebo odstrante adresar."
    exit 1
}

Write-Host "Kontrola ze je generovani nove branch pousteno na Develop branch"
# Kontrola: je spousteno na Develop branch
$currentBranch = git rev-parse --abbrev-ref HEAD
if ($currentBranch -ne "Develop") {
    Write-Host "Warning: Nejste na 'Develop' branch. Aktualni branch je: '$currentBranch'."
    exit 1
}

Write-Host "Kontrola zdali nejsou necommitnute zmenene soubory"
# Kontrola: nejsou necommitnute soubory
$hasUncommittedChanges = $(git status --porcelain)
if ($hasUncommittedChanges.Count -gt 0) {
    Write-Host "Warning: Na aktualni branch jsou zmenene a necommitnute soubory '$currentBranch'. Pred pokracovanim zmeny ulozte nebo vratte."
    exit 1
}

# Prepnuti do Develop branch
git checkout Develop
Write-Host "Prepnuto do Develop branch."

Write-Host "Vytvoreni nove branch $BranchName"

# Ziskani URL repository
$repoUrl = git remote get-url origin
Write-Host "URL repository: $repoUrl"

Write-Host "Pridani Git tokenu pro overeni"
# Pridani tokenu do URL
$authGitUrl = $repoUrl -replace "https://", "https://$gitToken@"

# Docasna zmena vzdaleneho URL repository pro push
$originalUrl = git remote get-url origin
git remote set-url origin $authGitUrl

# Vytvoreni nove branch a pushnuti na origin s tokenem
git checkout -b $BranchName
git push origin $BranchName

# Obnoveni puvodniho URL repository
git remote set-url origin $originalUrl

# Kontrola uspechu pushnuti
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Chyba pri pushnuti '$BranchName' na origin."
    exit 1
}

Write-Host "Branch $BranchName byla vytvorena a pushnuta na origin."

# Naklonovani branch na lokal
Write-Host "Stahovani nove branch na local $BranchName..."
git clone --single-branch --branch $BranchName $authGitUrl $cloneDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Chyba stahovani branch. Konec skriptu."
    exit 1
}

# Prepnuti do nove branch
Set-Location $cloneDir

# Prepnuti zpet do Develop branch
Set-Location $gitRoot

Write-Host "Prepnuti zpet do Develop branch"
# Odstraneni nove branch z lokalne trackovanych
git checkout Develop

Write-Host "Odstraneni nove branch: $BranchName z trackovanych v Develop repository..."
git branch -d $BranchName
git fetch --prune

Write-Host "Branch byla uspesne pripravena. Nova vetev sleduje vsechny vzdalene vetve a vetev Develop ji jiz nesleduje."
