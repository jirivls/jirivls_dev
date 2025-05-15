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
$folderHasContent = $false

Write-Host "Kontrola na existenci cilove slozky a jejiho obsahu"
# Pokud slozka neexistuje, tak vytvarim a kopiruji jinak pouze kopiruji
if ($folderExists) {
    $items = Get-ChildItem -Path $agentPath -Recurse -Force -ErrorAction SilentlyContinue
    $folderHasContent = $items.Count -gt 0
}

if (-not $folderExists -or -not $folderHasContent) {
    if (-not $folderExists) {
        Write-Host "Slozka neexistuje. Vytvarim: $agentPath"
        New-Item -Path $agentPath -ItemType Directory -Force | Out-Null
    } else {
        Write-Host "Slozka existuje, ale je prazdna. Pokracuji v kopirovani."
    }

    Write-Host "Kopiruji obsah ze slozky develop: $sourcePath"
    Copy-Item -Path "$sourcePath\*" -Destination $agentPath -Recurse -Force
    Write-Host "Obsah byl zkopirovan do: $agentPath"
} else {
    Write-Host "Slozka existuje a neni prazdna. Kopirovani se neprovadi."
}
