param(
	[string]$ProjectRoot,
	[string]$OutputRoot
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
	$ProjectRoot = (Resolve-Path (Join-Path (Join-Path $PSScriptRoot '..') '..')).Path
}
$ProjectRoot = [IO.Path]::GetFullPath($ProjectRoot)
$publicRoot = [IO.Path]::GetFullPath((Join-Path $ProjectRoot 'Docs\Modding\Public'))
$tempRoot = [IO.Path]::GetFullPath((Join-Path $ProjectRoot 'Temp'))
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
	$OutputRoot = Join-Path $tempRoot 'GitHubWiki'
}
$OutputRoot = [IO.Path]::GetFullPath($OutputRoot)

$tempPrefix = $tempRoot.TrimEnd('\') + '\'
if (-not $OutputRoot.StartsWith($tempPrefix, [StringComparison]::OrdinalIgnoreCase)) {
	throw "For safety, wiki export output must be inside $tempRoot"
}
if (Test-Path -LiteralPath $OutputRoot) {
	Remove-Item -LiteralPath $OutputRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputRoot | Out-Null

$utf8NoBom = New-Object Text.UTF8Encoding($false)
$repositoryBase = 'https://github.com/RealmsStuff/Captivity-Reloaded/blob/main/'

function Get-RelativePath {
	param([string]$BasePath, [string]$TargetPath)
	$baseUri = [Uri]($BasePath.TrimEnd('\') + '\')
	$targetUri = [Uri]$TargetPath
	return [Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace('/', '\')
}

function Get-WikiPageName {
	param([string]$RelativePath)
	$normalized = $RelativePath.Replace('\', '/').TrimStart('/')
	if ($normalized -ieq 'README.md') { return 'Home.md' }
	if ($normalized.EndsWith('/README.md', [StringComparison]::OrdinalIgnoreCase)) {
		$normalized = $normalized.Substring(0, $normalized.Length - '/README.md'.Length)
	}
	else {
		$normalized = $normalized.Substring(0, $normalized.Length - '.md'.Length)
	}
	$slug = ($normalized -replace '[^A-Za-z0-9/_-]+', '-' -replace '[/_]+', '-' -replace '-+', '-').Trim('-').ToLowerInvariant()
	if ([string]::IsNullOrWhiteSpace($slug)) { throw "Cannot create wiki slug for $RelativePath" }
	return $slug + '.md'
}

$pages = @(Get-ChildItem -LiteralPath $publicRoot -Filter '*.md' -File -Recurse | Where-Object { $_.Name -ne 'SUMMARY.md' })
$pageMap = @{}
$outputNames = @{}
foreach ($page in $pages) {
	$relative = Get-RelativePath $publicRoot $page.FullName
	$outputName = Get-WikiPageName $relative
	if ($outputNames.ContainsKey($outputName)) {
		throw "GitHub Wiki filename collision: $relative and $($outputNames[$outputName]) both map to $outputName"
	}
	$pageMap[$page.FullName] = $outputName
	$outputNames[$outputName] = $relative
}

function Convert-GitBookHints {
	param([string]$Text)
	$pattern = '(?ms)^\{%\s+hint\s+style="(?<style>[^"]+)"\s+%\}\s*\r?\n(?<body>.*?)^\{%\s+endhint\s+%\}\s*'
	return [regex]::Replace($Text, $pattern, {
		param($match)
		$label = switch ($match.Groups['style'].Value.ToLowerInvariant()) {
			'warning' { 'Warning' }
			'danger' { 'Danger' }
			'success' { 'Success' }
			default { 'Note' }
		}
		$body = $match.Groups['body'].Value.TrimEnd()
		$quoted = (($body -split '\r?\n') | ForEach-Object { if ($_.Length) { '> ' + $_ } else { '>' } }) -join "`n"
		return "> **$label**`n>`n$quoted`n`n"
	})
}

function Convert-WikiLinks {
	param([string]$Text, [string]$SourcePath)
	$sourceDirectory = Split-Path $SourcePath -Parent
	$pattern = '(?<open>!?\[[^\]]*\]\()(?<target>[^)\s]+)(?<close>[^)]*\))'
	return [regex]::Replace($Text, $pattern, {
		param($match)
		$target = $match.Groups['target'].Value
		if ($target -match '^(?:https?://|mailto:|#|data:)') { return $match.Value }

		$fragment = ''
		$pathPart = $target
		$hashIndex = $target.IndexOf('#')
		if ($hashIndex -ge 0) {
			$pathPart = $target.Substring(0, $hashIndex)
			$fragment = $target.Substring($hashIndex)
		}
		if ([string]::IsNullOrWhiteSpace($pathPart)) { return $match.Value }

		$decoded = [Uri]::UnescapeDataString($pathPart)
		$resolved = [IO.Path]::GetFullPath((Join-Path $sourceDirectory $decoded))
		$newTarget = $null
		if ($pageMap.ContainsKey($resolved)) {
			$newTarget = [IO.Path]::GetFileNameWithoutExtension($pageMap[$resolved]) + $fragment
		}
		elseif ($resolved.StartsWith($publicRoot.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) {
			$assetRelative = (Get-RelativePath $publicRoot $resolved).Replace('\', '/')
			$newTarget = $assetRelative + $fragment
		}
		elseif ($resolved.StartsWith($ProjectRoot.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) {
			$repositoryRelative = (Get-RelativePath $ProjectRoot $resolved).Replace('\', '/')
			$newTarget = $repositoryBase + $repositoryRelative + $fragment
		}
		else {
			throw "Wiki link escapes the project: $SourcePath -> $target"
		}
		return $match.Groups['open'].Value + $newTarget + $match.Groups['close'].Value
	})
}

foreach ($page in $pages) {
	$text = Get-Content -LiteralPath $page.FullName -Raw -Encoding utf8
	$text = Convert-GitBookHints $text
	$text = Convert-WikiLinks $text $page.FullName
	[IO.File]::WriteAllText((Join-Path $OutputRoot $pageMap[$page.FullName]), $text.TrimEnd() + "`n", $utf8NoBom)
}

Get-ChildItem -LiteralPath $publicRoot -File -Recurse | Where-Object { $_.Extension -ne '.md' } | ForEach-Object {
	$relative = Get-RelativePath $publicRoot $_.FullName
	$destination = Join-Path $OutputRoot $relative
	New-Item -ItemType Directory -Force -Path (Split-Path $destination -Parent) | Out-Null
	Copy-Item -LiteralPath $_.FullName -Destination $destination -Force
}

$summaryPath = Join-Path $publicRoot 'SUMMARY.md'
$sidebar = Get-Content -LiteralPath $summaryPath -Raw -Encoding utf8
$sidebar = $sidebar -replace '(?m)^# Table of contents\s*', '## Captivity Reloaded wiki'
$sidebar = Convert-WikiLinks $sidebar $summaryPath
[IO.File]::WriteAllText((Join-Path $OutputRoot '_Sidebar.md'), $sidebar.TrimEnd() + "`n", $utf8NoBom)

$footer = @'
---

Published from [`Docs/Modding/Public`](https://github.com/RealmsStuff/Captivity-Reloaded/tree/main/Docs/Modding/Public). The repository documentation is the canonical source.
'@
[IO.File]::WriteAllText((Join-Path $OutputRoot '_Footer.md'), $footer.Trim() + "`n", $utf8NoBom)

$failures = New-Object System.Collections.Generic.List[string]
$exportedPages = @(Get-ChildItem -LiteralPath $OutputRoot -Filter '*.md' -File | Where-Object { $_.Name -notin @('_Sidebar.md', '_Footer.md') })
if ($exportedPages.Count -ne $pages.Count) {
	$failures.Add("Expected $($pages.Count) wiki pages, exported $($exportedPages.Count).")
}
foreach ($file in Get-ChildItem -LiteralPath $OutputRoot -Filter '*.md' -File -Recurse) {
	$text = Get-Content -LiteralPath $file.FullName -Raw -Encoding utf8
	if ($text -match '\{%\s*(?:hint|endhint)') {
		$failures.Add("Unconverted GitBook directive: $($file.FullName)")
	}
	foreach ($match in [regex]::Matches($text, '!?\[[^\]]*\]\((?<target>[^)\s]+)')) {
		$target = $match.Groups['target'].Value
		if ($target -match '^(?:https?://|mailto:|#|data:)') { continue }
		$pathPart = ($target -split '#')[0]
		if ([string]::IsNullOrWhiteSpace($pathPart)) { continue }
		if ($pathPart -match '^[a-z0-9-]+$') {
			if (-not (Test-Path -LiteralPath (Join-Path $OutputRoot ($pathPart + '.md')))) {
				$failures.Add("Missing exported page target: $($file.Name) -> $target")
			}
		}
		elseif (-not (Test-Path -LiteralPath (Join-Path $OutputRoot $pathPart))) {
			$failures.Add("Missing exported asset target: $($file.Name) -> $target")
		}
	}
}

if ($failures.Count -gt 0) {
	$failures | ForEach-Object { Write-Error $_ }
	exit 1
}

Write-Host "GitHub Wiki export passed: $($exportedPages.Count) pages in $OutputRoot"
