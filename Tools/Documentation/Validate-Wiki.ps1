param([string]$ProjectRoot)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
	$ProjectRoot = (Resolve-Path (Join-Path (Join-Path $PSScriptRoot '..') '..')).Path
}

$publicRoot = Join-Path $ProjectRoot 'Docs/Modding/Public'
$summaryPath = Join-Path $publicRoot 'SUMMARY.md'
if (-not (Test-Path -LiteralPath $summaryPath)) { throw "Wiki summary not found: $summaryPath" }

$failures = New-Object System.Collections.Generic.List[string]
$markdownFiles = @(Get-ChildItem -LiteralPath $ProjectRoot -Filter '*.md' -File -Recurse | Where-Object {
	$relative = $_.FullName.Substring($ProjectRoot.Length + 1)
	$relative -notmatch '^(Library|Temp|obj|bin|Logs|UserSettings|\.git)[\\/]'
})

$relativeLinks = 0
$mojibakePattern = ([char]0x00E2).ToString() + [char]0x20AC + '|' + [char]0x00C3 + '|' + [char]0x00C2 + '\s'
foreach ($file in $markdownFiles) {
	$text = Get-Content -LiteralPath $file.FullName -Raw -Encoding utf8
	$display = $file.FullName.Substring($ProjectRoot.Length + 1)
	if ($file.FullName.StartsWith($publicRoot, [StringComparison]::OrdinalIgnoreCase)) {
		$headings = @([regex]::Matches($text, '(?m)^#\s+\S'))
		if ($file.Name -ne 'SUMMARY.md' -and $headings.Count -ne 1) {
			$failures.Add("Public page must contain exactly one H1: $display ($($headings.Count) found)")
		}
		$hintStarts = @([regex]::Matches($text, '\{%\s+hint\b')).Count
		$hintEnds = @([regex]::Matches($text, '\{%\s+endhint\s+%\}')).Count
		if ($hintStarts -ne $hintEnds) {
			$failures.Add("Unbalanced GitBook hint block: $display ($hintStarts start, $hintEnds end)")
		}
		if ($text -match $mojibakePattern) {
			$failures.Add("Probable UTF-8 mojibake in $display")
		}
	}
	foreach ($match in [regex]::Matches($text, '\[[^\]]+\]\(([^)]+)\)')) {
		$target = $match.Groups[1].Value.Trim()
		if ($target -match '^(https?://|mailto:|#|\{)') { continue }
		$pathPart = ($target -split '#')[0]
		if ([string]::IsNullOrWhiteSpace($pathPart)) { continue }
		$relativeLinks++
		$decoded = [uri]::UnescapeDataString($pathPart)
		$resolved = Join-Path $file.DirectoryName $decoded
		if (-not (Test-Path -LiteralPath $resolved)) {
			$failures.Add("Broken link: $display -> $target")
		}
	}
}

$summary = Get-Content -LiteralPath $summaryPath -Raw -Encoding utf8
$publicPages = @(Get-ChildItem -LiteralPath $publicRoot -Filter '*.md' -File -Recurse | Where-Object { $_.Name -ne 'SUMMARY.md' })
foreach ($page in $publicPages) {
	$relative = $page.FullName.Substring($publicRoot.Length + 1).Replace('\', '/')
	if ($summary -notmatch [regex]::Escape($relative)) {
		$failures.Add("Public page is missing from SUMMARY.md: $relative")
	}
}

$publicMarkdown = ($publicPages | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw -Encoding utf8 }) -join "`n"
$screenshotRoot = Join-Path $publicRoot 'assets/screenshots'
if (Test-Path -LiteralPath $screenshotRoot) {
	foreach ($screenshot in Get-ChildItem -LiteralPath $screenshotRoot -Filter '*.png' -File) {
		if ($publicMarkdown -notmatch [regex]::Escape($screenshot.Name)) {
			$failures.Add("Public screenshot is not referenced by any page: assets/screenshots/$($screenshot.Name)")
		}
	}
}

$literalPathPattern = '`((?:Docs|ExampleMods|ModSDK|Tools|ModCatalog)/[^`]+)`'
foreach ($file in $markdownFiles) {
	$text = Get-Content -LiteralPath $file.FullName -Raw -Encoding utf8
	foreach ($match in [regex]::Matches($text, $literalPathPattern)) {
		$literal = $match.Groups[1].Value
		if ($literal -match '[*{}<>]') { continue }
		$resolved = Join-Path $ProjectRoot ($literal.Replace('/', [IO.Path]::DirectorySeparatorChar))
		if (-not (Test-Path -LiteralPath $resolved)) {
			$display = $file.FullName.Substring($ProjectRoot.Length + 1)
			$failures.Add("Missing repository path: $display -> $literal")
		}
	}
}

$exampleRoot = Join-Path $ProjectRoot 'ExampleMods'
$packDirectories = @(Get-ChildItem -LiteralPath $exampleRoot -Directory | Where-Object {
	Test-Path -LiteralPath (Join-Path $_.FullName 'manifest.json')
})
foreach ($pack in $packDirectories) {
	if (-not (Test-Path -LiteralPath (Join-Path $pack.FullName 'README.md'))) {
		$failures.Add("Example pack has no README.md: $($pack.Name)")
	}
}

if ($failures.Count -gt 0) {
	$failures | ForEach-Object { Write-Error $_ }
	exit 1
}

Write-Host "Wiki validation passed: $($publicPages.Count) public pages, $relativeLinks relative links, $($packDirectories.Count) documented example packs."
