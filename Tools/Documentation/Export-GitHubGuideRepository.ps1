param(
	[string]$ProjectRoot,
	[string]$OutputRoot
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
	$ProjectRoot = (Resolve-Path (Join-Path (Join-Path $PSScriptRoot '..') '..')).Path
}
$ProjectRoot = [IO.Path]::GetFullPath($ProjectRoot)
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
	$OutputRoot = Join-Path $ProjectRoot 'Temp\GitHubGuide'
}
$OutputRoot = [IO.Path]::GetFullPath($OutputRoot)

& (Join-Path $PSScriptRoot 'Export-GitHubWiki.ps1') -ProjectRoot $ProjectRoot -OutputRoot $OutputRoot

$utf8NoBom = New-Object Text.UTF8Encoding($false)
$pageNames = @{}
foreach ($page in Get-ChildItem -LiteralPath $OutputRoot -Filter '*.md' -File) {
	$pageNames[[IO.Path]::GetFileNameWithoutExtension($page.Name)] = $page.Name
}

foreach ($file in Get-ChildItem -LiteralPath $OutputRoot -Filter '*.md' -File) {
	$text = Get-Content -LiteralPath $file.FullName -Raw -Encoding utf8
	$text = [regex]::Replace($text, '(?<open>!?\[[^\]]*\]\()(?<target>[A-Za-z0-9-]+)(?<fragment>#[^)]*)?(?<close>\))', {
		param($match)
		$target = $match.Groups['target'].Value
		if (-not $pageNames.ContainsKey($target)) { return $match.Value }
		return $match.Groups['open'].Value + $pageNames[$target] + $match.Groups['fragment'].Value + $match.Groups['close'].Value
	})
	[IO.File]::WriteAllText($file.FullName, $text, $utf8NoBom)
}

Copy-Item -LiteralPath (Join-Path $OutputRoot 'Home.md') -Destination (Join-Path $OutputRoot 'README.md') -Force

$failures = New-Object System.Collections.Generic.List[string]
foreach ($file in Get-ChildItem -LiteralPath $OutputRoot -Filter '*.md' -File) {
	$text = Get-Content -LiteralPath $file.FullName -Raw -Encoding utf8
	foreach ($match in [regex]::Matches($text, '!?\[[^\]]*\]\((?<target>[^)\s]+)')) {
		$target = $match.Groups['target'].Value
		if ($target -match '^(?:https?://|mailto:|#|data:)') { continue }
		$pathPart = ($target -split '#')[0]
		if (-not (Test-Path -LiteralPath (Join-Path $OutputRoot $pathPart))) {
			$failures.Add("Missing repository guide target: $($file.Name) -> $target")
		}
	}
}
if ($failures.Count -gt 0) {
	$failures | ForEach-Object { Write-Error $_ }
	exit 1
}

Write-Host "GitHub guide repository export passed: $(@(Get-ChildItem -LiteralPath $OutputRoot -Filter '*.md' -File).Count) Markdown files in $OutputRoot"
