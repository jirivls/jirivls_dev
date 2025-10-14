param (
    [string]$server = "192.168.4.1",
    [string]$database = "DOCUX51_DEV_ADMIN",
    [string]$user = "",
    [string]$password = "",
    [string]$applicationUid
)

Write-Host "Spusteni SQL query do DB"
$sqlQuery = "SET NOCOUNT ON; SELECT [Uid], [Name], [Key], [LicenceKey], [Id] FROM [dbo].[Applications] WHERE [Uid] = '$applicationUid'"

try {
    # Spusteni prikazu s potlacenou hlavickou a definovanym oddelovacem
    $result = sqlcmd -S $server -d $database -U $user -P $password -Q "`"$sqlQuery`"" -h -1 -s ";" -W 2>&1
} catch {
    Write-Host "Chyba: sqlcmd selhal (napr. chyba pripojeni nebo neplatne prihlaseni)"
    exit 1
}

# Kontrola chybove hlasky ve vysledku
if ($LASTEXITCODE -ne 0 -or $result -match "Sqlcmd:" -or $result -match "Msg \d+") {
    Write-Host "Chyba: SQL dotaz selhal nebo vratil chybu"
    Write-Host $result
    exit 1
}

Write-Host "Parsovani vysledku pro predani do parametru"
$parsed = $result | Where-Object {
    $_ -and ($_ -notmatch "^-+$") -and ($_ -notmatch "^\s*$")
} | Select-Object -First 1

if (-not $parsed) {
    Write-Host "Chyba: SQL dotaz nevratil radek s validnimi daty."
    exit 1
}

$columns = $parsed -split ";" | ForEach-Object { $_.Trim() }

$applicationUid  = $columns[0]
$applicationName = $columns[1]
$applicationKey  = $columns[2]
$licenceKey      = $columns[3]
$applicationId   = $columns[4]

Write-Host "Name: $applicationName, Key: $applicationKey, Uid: $applicationUid, LicenceKey: $licenceKey, Id: $applicationId"
Write-Host "Konec SQL query a vraceni vysledku zpet do Main.ps1"

# Vystup jako jednotlive radky
Write-Output $applicationUid
Write-Output $applicationName
Write-Output $applicationKey
Write-Output $licenceKey
Write-Output $applicationId
