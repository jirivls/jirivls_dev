param (
    [string]$BranchName # Name of the new branch
)

# Nastaveni API tokenu
$gitToken = "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"

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
$cloneDir = $BranchName -replace "\.", ""
Write-Host "Cloning repository with only branch $BranchName..."
git clone --single-branch --branch $BranchName $authGitUrl $cloneDir

# Prepnuti do nove branch
Set-Location $cloneDir

# Natahnuti vsech remote branch pro novou
Write-Host "Fetching all remote branches..."
git fetch --all
Write-Host "Setting remote-tracking branches..."
git remote set-branches origin "*"
git fetch origin
git branch -r

# Prepnuti zpet do Develop repository
Set-Location ..

# Odstraneni nove branch z lokalne trackovanych
Set-Location (Get-Location).Path
git checkout Develop
Write-Host "Switched back to Develop branch."

Write-Host "Removing local tracking of branch $BranchName in Develop repository..."
git branch --unset-upstream $BranchName 2>$null
git branch -d $BranchName
git fetch --prune

Write-Host "Branch setup completed successfully. The new branch tracks all remote branches, and Develop no longer tracks it."
