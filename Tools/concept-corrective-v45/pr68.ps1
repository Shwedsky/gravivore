[CmdletBinding()]
param([ValidateSet('Read','Update')][string]$Action='Read',[string]$BodyFile)
$ErrorActionPreference='Stop'
$correctiveCredentialInput="protocol=https`nhost=github.com`n`n"
$correctiveCredentialLines=$correctiveCredentialInput | git credential fill
if($LASTEXITCODE){throw 'GitHub credential helper failed'}
$correctiveCredentials=@{}
foreach($line in $correctiveCredentialLines){$split=$line.IndexOf('=');if($split -ge 0){$correctiveCredentials[$line.Substring(0,$split)]=$line.Substring($split+1)}}
if(-not $correctiveCredentials['password']){throw 'GitHub credential is unavailable'}
$correctiveHeaders=@{Authorization='Bearer '+$correctiveCredentials['password'];Accept='application/vnd.github+json';'X-GitHub-Api-Version'='2022-11-28'}
$correctiveUri='https://api.github.com/repos/Shwedsky/gravivore/pulls/68'
$correctivePr=Invoke-RestMethod -Uri $correctiveUri -Headers $correctiveHeaders
if($correctivePr.state -ne 'open' -or -not $correctivePr.draft -or $correctivePr.head.ref -ne 'art/chapter01-visual-replacement-v3'){throw 'PR #68 must remain open/draft on the specified branch'}
if($Action -eq 'Update'){
 if(-not $BodyFile){throw 'BodyFile required'}
 $correctiveBody=Get-Content -LiteralPath $BodyFile -Raw
 $correctivePayload=@{title='Chapter 01 concept fidelity corrective pass and V45 DEV APK';body=$correctiveBody} | ConvertTo-Json
 $correctivePr=Invoke-RestMethod -Uri $correctiveUri -Headers $correctiveHeaders -Method Patch -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes($correctivePayload))
}
[ordered]@{url=$correctivePr.html_url;state=$correctivePr.state;draft=$correctivePr.draft;branch=$correctivePr.head.ref;head=$correctivePr.head.sha;title=$correctivePr.title} | ConvertTo-Json
