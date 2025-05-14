param (
    [string]$version  # Napr. 5.1.26.0
)

# Ziskani root cesty repository
$gitRoot = git rev-parse --show-toplevel

# Cesta ke config.json
$configPath = Join-Path $gitRoot "Tools.Binaries\Publisher\config.json"

Write-Host "Nacteni config.json: $configPath"
$config = Get-Content $configPath -Raw | ConvertFrom-Json

# Rozdeleni predane verze
$segments = $version -split '\.'

# Slozeni verze slozky (bez build)
$newFolderVersion = "$($segments[0]).$($segments[1]).$($segments[2])"

Write-Host "Nova verze: $version"
Write-Host "Nova slozka verze: $newFolderVersion"

# Aktualizace obou hodnot
$config.DMS.LastVersion = $version
$config.ADMIN.LastVersion = $version

Write-Host "Nastaveni cesty k instalacnimu balicku docu-x agenta"
$config.DMS.CopyFilesToOutputDirectoryPostPublish[0].SourcePath = "F:\GIT\Agents\$newFolderVersion\Win"

Write-Host "Ulozeni zmen zpet do config.json"
$config | ConvertTo-Json -Depth 10 | Set-Content -Encoding UTF8 $configPath

Write-Host "Verze DMS a ADMIN byly aktualizovany v config.json."

# Ziskani aktualni Git branch
$currentBranch = git -C $gitRoot rev-parse --abbrev-ref HEAD
Write-Host "Aktualni Git branch: $currentBranch"

# Nacteni tokenu z configu (muze byt i predany parametrem)
$gitToken = $config.Jit.GitApiToken

# Commit a push zmen
$commitMessage = "Navyseni verze $version a aktualizace config"
git -C $gitRoot add $configPath
git -C $gitRoot add -A
git -C $gitRoot commit -m "$commitMessage"

# Push s tokenem
$repoUrl = git -C $gitRoot remote get-url origin
$authGitUrl = $repoUrl -replace "https://", "https://$gitToken@"

git -C $gitRoot remote set-url origin $authGitUrl
git -C $gitRoot push origin $currentBranch
git -C $gitRoot remote set-url origin $repoUrl

Write-Host "Zmeny byly commitnuty a pushnuty na '$currentBranch'."
