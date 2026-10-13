# Restore only automatic tracked changes outside the additive proof in the
# clean worktree created for this request. Never run in the other checkout.
$ErrorActionPreference='Stop'
$proofRestoreRoot=[IO.Path]::GetFullPath((Split-Path -Parent (Split-Path -Parent $PSScriptRoot)))
if((Get-Location).Path.TrimEnd('\') -ne $proofRestoreRoot.TrimEnd('\')){throw 'Run in the Energy Chamber worktree root.'}
if((& git branch --show-current) -ne 'art/energy-chamber-hybrid-proof-v1'){throw 'Wrong branch; refusing to restore.'}
$proofRestoreChanged=@(& git -c core.quotepath=false diff --name-only)
if($LASTEXITCODE -ne 0){throw 'Unable to inspect tracked changes.'}
$proofRestoreOutside=@($proofRestoreChanged | Where-Object {
    $_ -ne 'Assets/_Game/ArtProof.meta' -and
    -not $_.StartsWith('Assets/_Game/ArtProof/') -and
    -not $_.StartsWith('Tools/energy-chamber-hybrid-proof-v1/') -and
    -not $_.StartsWith('Tools/energy-chamber-visual-polish-v1/') -and
    -not $_.StartsWith('Tools/energy-chamber-vertical-nodes-v1/') -and
    -not $_.StartsWith('Tools/energy-chamber-final-polish-v1/') -and
    -not $_.StartsWith('art/energy-chamber-hybrid-proof-v1/') -and
    -not $_.StartsWith('art/energy-chamber-visual-polish-v1/') -and
    -not $_.StartsWith('art/energy-chamber-vertical-nodes-v1/') -and
    -not $_.StartsWith('docs/history/implementation-passes/energy-chamber-hybrid-proof-v1/') -and
    -not $_.StartsWith('docs/history/implementation-passes/energy-chamber-visual-polish-v1/') -and
    -not $_.StartsWith('docs/history/implementation-passes/energy-chamber-vertical-nodes-v1/') -and
    -not $_.StartsWith('docs/history/implementation-passes/energy-chamber-final-polish-v1/')
})
foreach($proofRestoreRelative in $proofRestoreOutside){
    $proofRestoreAbsolute=[IO.Path]::GetFullPath((Join-Path $proofRestoreRoot $proofRestoreRelative))
    if(-not $proofRestoreAbsolute.StartsWith($proofRestoreRoot.TrimEnd('\')+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Path escapes isolated worktree.'}
}
if($proofRestoreOutside.Count -gt 0){
    & git restore --source=HEAD --worktree -- $proofRestoreOutside
    if($LASTEXITCODE -ne 0){throw 'Restoring automatic Unity changes failed.'}
}
Write-Output ('RESTORED_AUTOMATIC_TRACKED_FILES='+$proofRestoreOutside.Count)
Write-Output 'Untracked generated settings/caches remain local; no recursive cleanup is performed.'
