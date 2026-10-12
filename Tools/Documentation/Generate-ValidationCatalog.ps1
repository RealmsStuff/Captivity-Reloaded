param([string]$ProjectRoot)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
	$ProjectRoot = (Resolve-Path (Join-Path (Join-Path $PSScriptRoot '..') '..')).Path
}

$roots = @(
	(Join-Path $ProjectRoot 'Assets/Scripts')
	(Join-Path $ProjectRoot 'Assets/Editor/Modding')
	(Join-Path $ProjectRoot 'Tools')
)
$records = New-Object System.Collections.Generic.List[object]
$pattern = '(?s)(?:Add|AddIssue)\s*\(\s*ValidationSeverity\.(?<severity>\w+)\s*,\s*"(?<code>[a-z0-9][a-z0-9.-]+)"\s*,\s*"(?<message>(?:\\.|[^"\\])*)"'

foreach ($root in $roots) {
	if (-not (Test-Path -LiteralPath $root)) { continue }
	foreach ($file in Get-ChildItem -LiteralPath $root -Filter '*.cs' -File -Recurse) {
		$text = Get-Content -LiteralPath $file.FullName -Raw -Encoding utf8
		foreach ($match in [regex]::Matches($text, $pattern)) {
			$records.Add([pscustomobject]@{
				Code = $match.Groups['code'].Value
				Severity = $match.Groups['severity'].Value
				Message = $match.Groups['message'].Value.Replace('\"', '"')
				Source = $file.FullName.Substring($ProjectRoot.Length + 1).Replace('\', '/')
				Offset = $match.Index
			})
		}
	}
}

function Escape-Cell([string]$Value) {
	if ($null -eq $Value) { return '' }
	return $Value.Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function Get-OrdinalSortKey([string]$Value) {
	if ($null -eq $Value) { return '' }
	return [BitConverter]::ToString([Text.Encoding]::UTF8.GetBytes($Value)).Replace('-', '')
}

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add('# Validation and error-code catalog')
$lines.Add('')
$lines.Add('This searchable catalog is generated from literal validation calls in the runtime, editor tools, and repository validators. Use your browser search for an exact code shown by the Mods panel or authoring tool.')
$lines.Add('')
$lines.Add('Regenerate it with `Tools/Documentation/Generate-ValidationCatalog.ps1`. Codes assembled dynamically at runtime may not appear here; the displayed message and source remain authoritative.')
$lines.Add('')
$lines.Add('## Severity')
$lines.Add('')
$lines.Add('- **Error:** the document, operation, or pack cannot proceed safely.')
$lines.Add('- **Warning:** content may load with a fallback or requires author attention.')
$lines.Add('- **Info:** state or platform information that does not itself invalidate content.')

$unique = @($records | Group-Object Code | ForEach-Object {
	$first = $_.Group | Sort-Object @{ Expression = { Get-OrdinalSortKey $_.Source } }, Offset | Select-Object -First 1
	[pscustomobject]@{
		Code = $_.Name
		Category = ($_.Name -split '\.')[0]
		Severity = (@($_.Group.Severity | Sort-Object @{ Expression = { Get-OrdinalSortKey ([string]$_) } } -Unique) -join '/')
		Message = $first.Message
		Sources = (@($_.Group.Source | Sort-Object @{ Expression = { Get-OrdinalSortKey ([string]$_) } } -Unique) -join ', ')
	}
} | Sort-Object @{ Expression = { Get-OrdinalSortKey $_.Category } }, @{ Expression = { Get-OrdinalSortKey $_.Code } })

foreach ($category in $unique | Group-Object Category) {
	$lines.Add('')
	$lines.Add('## `' + $category.Name + '.*`')
	$lines.Add('')
	$lines.Add('| Code | Severity | Message prefix | Source |')
	$lines.Add('| --- | --- | --- | --- |')
	foreach ($record in $category.Group) {
		$lines.Add('| `' + (Escape-Cell $record.Code) + '` | ' + (Escape-Cell $record.Severity) + ' | ' +
			(Escape-Cell $record.Message) + ' | `' + (Escape-Cell $record.Sources) + '` |')
	}
}

$output = Join-Path $ProjectRoot 'Docs/Modding/Public/reference/error-codes.md'
[IO.File]::WriteAllLines($output, [string[]]$lines, (New-Object Text.UTF8Encoding($false)))
Write-Host "Generated $($unique.Count) validation-code entries from $($records.Count) literal calls."
