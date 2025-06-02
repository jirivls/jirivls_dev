param (
    [string]$server,
    [string]$database,
    [string]$user,
    [string]$password,
    [string]$version,
    [ref]$applicationUidOut
)

$ErrorActionPreference = 'Stop'

Write-Host "Spousteni SP_AdminCreateNewAppication pro zalozeni a zprocesovani nove aplikace"
$sqlQuery = @"
DECLARE @LicenceKeyOut NVARCHAR(36);
EXEC SP_AdminCreateNewAppication 
    @NewVersion = N'$version',
    @NewLicenceKeyOut = @LicenceKeyOut OUTPUT;
SELECT UID FROM [dbo].[Applications] WHERE Id = SCOPE_IDENTITY();
"@

try {
    $result = sqlcmd -S $server `
                     -d $database `
                     -U $user `
                     -P $password `
                     -Q "$sqlQuery" `
                     -h -1 -s ";" -W

    if (-not $result) {
        throw "Prazdny vysledek, UID nebyl vracen."
    }

    $applicationUidOut.Value = $result.Trim()
} catch {
    Write-Host "Chyba pri volani SP_AdminCreateNewAppication"
    Write-Host "test commit pro TC"
    exit 1
}
