param(
	[string]$ProjectRoot,
	[string]$UnityRoot
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
	$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
}
if ([string]::IsNullOrWhiteSpace($UnityRoot)) {
	$UnityRoot = Join-Path (Split-Path $ProjectRoot -Parent) 'Unity-2021.3.45f1'
}

$script:Checks = New-Object System.Collections.Generic.List[object]

function Add-ReleaseCheck {
	param([string]$Name, [bool]$Passed, [string]$Detail)
	$script:Checks.Add([pscustomobject]@{
		Status = if ($Passed) { 'PASS' } else { 'BLOCKED' }
		Check = $Name
		Detail = $Detail
	})
}

function Get-GitLines {
	param([string[]]$Arguments)
	$lines = & git -C $ProjectRoot @Arguments 2>&1
	if ($LASTEXITCODE -ne 0) {
		throw "git $($Arguments -join ' ') failed: $($lines -join [Environment]::NewLine)"
	}
	return @($lines)
}

if (-not (Test-Path (Join-Path $ProjectRoot 'ProjectSettings\ProjectVersion.txt'))) {
	throw "Not a Unity project: $ProjectRoot"
}

$branch = (Get-GitLines @('branch', '--show-current')) -join ''
$head = (Get-GitLines @('rev-parse', '--short', 'HEAD')) -join ''
$status = @(Get-GitLines @('status', '--porcelain=v1'))
Add-ReleaseCheck 'Source snapshot is clean' ($status.Count -eq 0) "$($status.Count) changed or untracked entries on $branch at $head."

$tracked = @(Get-GitLines @('ls-files'))
$generatedPattern = '^(Logs|TestResults|Builds?|UserSettings|obj|Library|Temp|\.vs)/|^(debug\.log|UpgradeLog\.htm)$|^[^/]+\.(csproj|sln|suo|user|pdb)$'
$trackedGenerated = @($tracked | Where-Object { $_ -match $generatedPattern })
Add-ReleaseCheck 'No generated files are tracked' ($trackedGenerated.Count -eq 0) "$($trackedGenerated.Count) tracked generated/local files."

$playbackRoot = Join-Path $UnityRoot 'Editor\Data\PlaybackEngines'
$requiredModules = @('windowsstandalonesupport', 'AndroidPlayer', 'WebGLSupport')
$installedModules = @()
if (Test-Path $playbackRoot) {
	$installedModules = @(Get-ChildItem -LiteralPath $playbackRoot -Directory | ForEach-Object Name)
}
foreach ($module in $requiredModules) {
	Add-ReleaseCheck "Unity module: $module" ($installedModules -contains $module) $playbackRoot
}

$editAssembly = Join-Path $ProjectRoot 'Assets\Tests\Editor\Modding\CaptivityReloaded.Modding.Tests.asmdef'
$playAssembly = Join-Path $ProjectRoot 'Assets\Tests\PlayMode\Modding\CaptivityReloaded.Modding.PlayModeTests.asmdef'
$editTests = @()
$playTests = @()
if (Test-Path (Join-Path $ProjectRoot 'Assets\Tests\Editor')) {
	$editTests = @(Get-ChildItem (Join-Path $ProjectRoot 'Assets\Tests\Editor') -Filter '*.cs' -File -Recurse)
}
if (Test-Path (Join-Path $ProjectRoot 'Assets\Tests\PlayMode')) {
	$playTests = @(Get-ChildItem (Join-Path $ProjectRoot 'Assets\Tests\PlayMode') -Filter '*.cs' -File -Recurse)
}
$editCases = @($editTests | Select-String -Pattern '\[(?:UnityTest|Test|TestCase)(?:\(|\])')
$playCases = @($playTests | Select-String -Pattern '\[(?:UnityTest|Test|TestCase)(?:\(|\])')
Add-ReleaseCheck 'EditMode suite exists' ((Test-Path $editAssembly) -and $editCases.Count -gt 0) "$($editCases.Count) declared test attributes in $($editTests.Count) source files."
# Nine PlayMode cases existed in the last archived passing result. A smaller discovered source suite is a regression.
Add-ReleaseCheck 'PlayMode suite baseline is present' ((Test-Path $playAssembly) -and $playCases.Count -ge 9) "$($playCases.Count) declared test attributes; baseline is 9."

$exampleRoot = Join-Path $ProjectRoot 'ExampleMods'
$examples = @()
if (Test-Path $exampleRoot) {
	$examples = @(Get-ChildItem -LiteralPath $exampleRoot -Directory | Where-Object { Test-Path (Join-Path $_.FullName 'manifest.json') })
}
Add-ReleaseCheck 'Repository examples are discoverable' ($examples.Count -gt 0) "$($examples.Count) example packs with manifests."

$stressCandidates = @()
foreach ($root in @((Join-Path $ProjectRoot 'ExampleMods'), (Join-Path $ProjectRoot 'ModSDK'))) {
	if (-not (Test-Path $root)) { continue }
	foreach ($file in Get-ChildItem -LiteralPath $root -Filter '*.json' -File -Recurse) {
		try {
			$document = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json
			if ($null -ne $document.waves -and [int]$document.waves.firstWaveEnemyCount -ge 60 -and @($document.spawners).Count -ge 15) {
				$stressCandidates += $file.FullName.Substring($ProjectRoot.Length + 1)
			}
		} catch {
			# Invalid JSON is handled by the schema suite; it is not a stress-stage candidate.
		}
	}
}
Add-ReleaseCheck 'Stress arena is source-controlled input' ($stressCandidates.Count -gt 0) $(if ($stressCandidates.Count) { $stressCandidates -join ', ' } else { 'The 60-enemy/15-spawner arena currently exists only under local Mods.' })

$catalogFiles = @(
	Join-Path $ProjectRoot 'Assets\Scripts\Assembly-CSharp\CoreContentAdapter.cs'
	Join-Path $ProjectRoot 'Assets\Resources\Modding\Catalog\catalog-v1.json'
	Join-Path $ProjectRoot 'ModCatalog\catalog-v1.json'
)
$temporaryReferences = New-Object System.Collections.Generic.List[string]
foreach ($file in $catalogFiles) {
	if (-not (Test-Path $file)) { continue }
	if (Select-String -LiteralPath $file -SimpleMatch 'cheaterbob/CR-MODS-TEST' -Quiet) {
		$temporaryReferences.Add($file.Substring($ProjectRoot.Length + 1))
	}
}
Add-ReleaseCheck 'Permanent catalog configured' ($temporaryReferences.Count -eq 0) $(if ($temporaryReferences.Count) { $temporaryReferences -join ', ' } else { 'No temporary catalog references in runtime/catalog sources.' })

$licensePath = Join-Path $ProjectRoot 'LICENSE'
$policyPath = Join-Path $ProjectRoot 'Docs\Modding\Public\reference\licensing-and-redistribution.md'
$noticesPath = Join-Path $ProjectRoot 'THIRD_PARTY_NOTICES.md'
Add-ReleaseCheck 'Repository license exists' (Test-Path $licensePath) $licensePath
Add-ReleaseCheck 'Redistribution policy exists' (Test-Path $policyPath) $policyPath
$noticesComplete = (Test-Path $noticesPath) -and (Select-String -LiteralPath $noticesPath -Pattern 'Release provenance approval:\s*\*\*yes\*\*' -Quiet)
Add-ReleaseCheck 'Third-party notices are complete' $noticesComplete $noticesPath

$script:Checks | Format-Table -AutoSize -Wrap
$blocked = @($script:Checks | Where-Object Status -eq 'BLOCKED')
Write-Host ""
Write-Host "Release audit: $($script:Checks.Count - $blocked.Count) passed, $($blocked.Count) blocked."
if ($blocked.Count -gt 0) { exit 1 }
