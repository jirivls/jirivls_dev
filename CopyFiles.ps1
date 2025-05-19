param (
    [string]$version  # Napr. "5.1.25"
)

# =============================
# Kontrola a vytvoreni slozky Agentu
# =============================

$agentPath = "C:\GithubSocos\Agents\$version\Win"
$sourcePath = "C:\GithubSocos\jirivls_dev\Tools.Binaries\Agents\Win"

Write-Host "Kontrola slozky agentu: $agentPath"

$folderExists = Test-Path $agentPath

if (-not $folderExists) {
    Write-Host "Slozka neexistuje. Vytvarim: $agentPath"
    New-Item -Path $agentPath -ItemType Directory -Force | Out-Null
}

Write-Host "Kontrola poctu souboru ve slozce: $agentPath"
$files = Get-ChildItem -Path $agentPath -Recurse -Force -File -ErrorAction SilentlyContinue
$fileCount = $files.Count

if ($fileCount -ne 1) {
    Write-Host "Ocekaval jsem presne 1 soubor, ale bylo nalezeno: $fileCount"
    Write-Host "Kopiruji obsah ze slozky develop: $sourcePath"
    Copy-Item -Path "$sourcePath\*" -Destination $agentPath -Recurse -Force
    Write-Host "Obsah byl zkopirovan do: $agentPath"

    # Kontrola znovu po kopirovani
    $filesAfterCopy = Get-ChildItem -Path $agentPath -Recurse -Force -File -ErrorAction SilentlyContinue
    $fileCountAfter = $filesAfterCopy.Count

    if ($fileCountAfter -ne 1) {
        Write-Host "Po kopirovani je ve slozce stale $fileCountAfter souboru. Ocekavan je presne 1." -ForegroundColor Red
    }
} else {
    Write-Host "Slozka obsahuje presne jeden soubor. Kopirovani se neprovadi." -ForegroundColor Red
}
