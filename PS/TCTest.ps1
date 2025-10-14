[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$commitSubject = git log -1 --pretty=%s
$commitBody    = git log -1 --pretty=%b
$webhookUri    = $env:webhookUri

if ([string]::IsNullOrWhiteSpace($commitBody)) {
    $commitBody = ""
}

$payload = @{
    title = $commitSubject.Trim()
    text  = $commitBody.Trim()
}

# Split lines for later use
$lines = $payload.text.Split([Environment]::NewLine)

# You can still use it for display or processing
Write-Host "Lines:"
$lines | ForEach-Object { Write-Host $_ }

# Optionally, rejoin for sending or JSON payload
$payload.text = $lines -join "`n"

$json = $payload | ConvertTo-Json -Depth 10
$utf8Bytes = [System.Text.Encoding]::UTF8.GetBytes($json)

Write-Host "Title: $($payload.title)"
Write-Host "Text: $($payload.text)"
Write-Host "URI: $webhookUri"
