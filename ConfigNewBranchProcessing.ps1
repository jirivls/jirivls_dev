# Ziskani root cesty repository
$gitRoot = git rev-parse --show-toplevel

# Cesta ke config.json
$configPath = Join-Path $gitRoot "Tools.Binaries\Publisher\config.json"

Write-Host "Nacteni config.json: $configPath"
$config = Get-Content $configPath -Raw | ConvertFrom-Json

# Ziskani aktualni verze z DMS (napr. 5.1.25.10)
$currentVersion = $config.DMS.LastVersion
Write-Host "Aktualni verze DMS: $currentVersion"

# Rozdeleni verze
$segments = $currentVersion -split '\.'

Write-Host "Navyseni patch verze o +1 a vynulovani build verze"
# Zvyseni patch verze (treti segment)
$segments[2] = [int]$segments[2] + 1

# Reset build verze na 0 (ctvrty segment)
$segments[3] = 0

# Nova verze pro DMS a ADMIN
$newVersion = ($segments -join '.')
$newFolderVersion = "$($segments[0]).$($segments[1]).$($segments[2])"
Write-Host "Nova verze: $newVersion"

# Aktualizace obou hodnot
$config.DMS.LastVersion = $newVersion
$config.ADMIN.LastVersion = $newVersion

Write-Host "Nastaveni cesty k instalacnimu balicku docu-x agenta"
# Aktualizace cest v DMS.CopyFilesToOutputDirectoryPostPublish
$config.DMS.CopyFilesToOutputDirectoryPostPublish[0].SourcePath = "F:\GIT\Agents\$newFolderVersion\Win"

Write-Host "Ulozeni zmen zpet do config.json"
# Ulozeni zpet do config.json
$config | ConvertTo-Json -Depth 10 | Set-Content -Encoding UTF8 $configPath

Write-Host "Verze DMS a ADMIN byly aktualizovany v config.json."

# Nacteni aktualni branch
$currentBranch = git -C $gitRoot rev-parse --abbrev-ref HEAD
Write-Host "Aktualni Git branch: $currentBranch"


$commitMessage = "Navyseni verze $newVersion a aktualizace config"
# Pridani zmen do Git indexu
git -C $gitRoot add $configPath

# Commit zmen
git -C $gitRoot add -A
git -C $gitRoot commit -m "$commitMessage"

# Pridani tokenu do remote URL
$repoUrl = git -C $gitRoot remote get-url origin
$authGitUrl = $repoUrl -replace "https://", "https://$gitToken@"

# Docasna zmena remote s tokenem pro autentizaci
git -C $gitRoot remote set-url origin $authGitUrl

# Push na aktualni branch
git -C $gitRoot push origin $currentBranch

# Obnoveni puvodniho remote URL
git -C $gitRoot remote set-url origin $repoUrl

Write-Host "Zmeny byly commitnuty a pushnuty na '$currentBranch'."
