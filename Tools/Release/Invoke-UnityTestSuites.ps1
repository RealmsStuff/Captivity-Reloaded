param(
	[string]$ProjectRoot,
	[string]$UnityExe,
	[string]$Label = 'candidate'
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
	$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
}
if ([string]::IsNullOrWhiteSpace($UnityExe)) {
	$UnityExe = Join-Path (Split-Path $ProjectRoot -Parent) 'Unity-2021.3.45f1\Editor\Unity.exe'
}
if (-not (Test-Path $UnityExe)) { throw "Unity executable not found: $UnityExe" }
if ($Label -notmatch '^[A-Za-z0-9._-]+$') { throw 'Label contains unsafe characters.' }

$resultRoot = Join-Path $ProjectRoot "TestResults\Release\$Label"
$logRoot = Join-Path $ProjectRoot "Logs\Release\$Label"
New-Item -ItemType Directory -Path $resultRoot -Force | Out-Null
New-Item -ItemType Directory -Path $logRoot -Force | Out-Null

$failures = New-Object System.Collections.Generic.List[string]
foreach ($platform in @('EditMode', 'PlayMode')) {
	$resultPath = Join-Path $resultRoot "$platform.xml"
	$logPath = Join-Path $logRoot "$platform.log"
	if (Test-Path $resultPath) { Remove-Item -LiteralPath $resultPath -Force }

	& $UnityExe -batchmode -nographics -projectPath $ProjectRoot -runTests `
		-testPlatform $platform -testResults $resultPath -logFile $logPath -quit
	$exitCode = $LASTEXITCODE
	if ($exitCode -ne 0 -or -not (Test-Path $resultPath)) {
		$failures.Add("$platform did not produce a valid result (Unity exit $exitCode). See $logPath")
		continue
	}

	[xml]$document = Get-Content -LiteralPath $resultPath
	$run = $document.'test-run'
	$total = [int]$run.total
	$failed = [int]$run.failed
	Write-Host "${platform}: result=$($run.result), total=$total, passed=$($run.passed), failed=$failed, skipped=$($run.skipped)"
	if ($total -le 0) { $failures.Add("$platform discovered zero tests.") }
	if ($failed -gt 0 -or $run.result -notlike 'Passed*') { $failures.Add("$platform failed. See $resultPath") }
}

if ($failures.Count -gt 0) {
	$failures | ForEach-Object { Write-Error $_ }
	exit 1
}

Write-Host "Both Unity suites passed. Results: $resultRoot"
