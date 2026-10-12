param([string]$ProjectRoot)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
	$ProjectRoot = (Resolve-Path (Join-Path (Join-Path $PSScriptRoot '..') '..')).Path
}

$schemaRoot = Join-Path $ProjectRoot 'Docs/Modding/schemas'
$outputRoot = Join-Path $ProjectRoot 'Docs/Modding/Public/reference/api'
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null

function Escape-Cell([object]$Value) {
	if ($null -eq $Value) { return '' }
	return ([string]$Value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function Get-OrdinalSortKey([string]$Value) {
	if ($null -eq $Value) { return '' }
	return [BitConverter]::ToString([Text.Encoding]::UTF8.GetBytes($Value)).Replace('-', '')
}

function Schema-Type([object]$Node) {
	if ($null -eq $Node) { return '' }
	if ($null -ne $Node.'$ref') { return 'ref: `' + (($Node.'$ref' -split '/')[-1]) + '`' }
	if ($null -ne $Node.const) { return 'const: `' + [string]$Node.const + '`' }
	if ($null -ne $Node.enum) { return 'enum: ' + ((@($Node.enum) | ForEach-Object { '`' + [string]$_ + '`' }) -join ', ') }
	if ([string]$Node.type -eq 'array' -and $null -ne $Node.items) { return 'array of ' + (Schema-Type $Node.items) }
	if ([string]$Node.type -eq 'object' -and $null -ne $Node.additionalProperties -and $Node.additionalProperties -isnot [bool]) {
		return 'object map of ' + (Schema-Type $Node.additionalProperties)
	}
	if ($null -ne $Node.type) { return [string]$Node.type }
	if ($null -ne $Node.oneOf) { return 'oneOf' }
	if ($null -ne $Node.anyOf) { return 'anyOf' }
	if ($null -ne $Node.allOf) { return 'allOf' }
	return 'schema'
}

function Schema-Notes([object]$Node) {
	$notes = New-Object System.Collections.Generic.List[string]
	foreach ($pair in @(
		@('minimum', 'min'), @('exclusiveMinimum', 'exclusive min'), @('maximum', 'max'),
		@('exclusiveMaximum', 'exclusive max'), @('minLength', 'min length'), @('maxLength', 'max length'),
		@('minItems', 'min items'), @('maxItems', 'max items'), @('minProperties', 'min properties'),
		@('maxProperties', 'max properties'), @('default', 'default')
	)) {
		$value = $Node.($pair[0])
		if ($null -ne $value) { $notes.Add($pair[1] + ': `' + [string]$value + '`') }
	}
	if ($null -ne $Node.uniqueItems -and [bool]$Node.uniqueItems) { $notes.Add('unique items') }
	if ($null -ne $Node.pattern) { $notes.Add('pattern: `' + [string]$Node.pattern + '`') }
	if ($null -ne $Node.description) { $notes.Add([string]$Node.description) }
	return Escape-Cell ($notes -join '; ')
}

function Add-PropertyTable([System.Collections.Generic.List[string]]$Lines, [object]$ObjectSchema) {
	$required = @{}
	foreach ($name in @($ObjectSchema.required)) { if ($null -ne $name) { $required[[string]$name] = $true } }
	$Lines.Add('| Field | Type | Required | Stability | Constraints and meaning |')
	$Lines.Add('| --- | --- | --- | --- | --- |')
	if ($null -eq $ObjectSchema.properties) {
		$Lines.Add('| _No named properties_ | | | | This definition is composed through references or conditional schemas. |')
		return
	}
	foreach ($property in $ObjectSchema.properties.PSObject.Properties) {
		$node = $property.Value
		$stability = if ($null -ne $node.'x-stability') { [string]$node.'x-stability' } else { '' }
		$Lines.Add('| `' + (Escape-Cell $property.Name) + '` | ' + (Escape-Cell (Schema-Type $node)) + ' | ' +
			$(if ($required.ContainsKey($property.Name)) { 'yes' } else { 'no' }) + ' | ' +
			(Escape-Cell $stability) + ' | ' + (Schema-Notes $node) + ' |')
	}
}

function Add-NestedTables([System.Collections.Generic.List[string]]$Lines, [object]$Node, [string]$Path) {
	if ($null -eq $Node) { return }
	if ($null -ne $Node.properties) {
		foreach ($property in $Node.properties.PSObject.Properties) {
			$child = $property.Value
			$childPath = if ([string]::IsNullOrWhiteSpace($Path)) { $property.Name } else { $Path + '.' + $property.Name }
			foreach ($shape in @(
				[pscustomobject]@{ Node=$child; Path=$childPath },
				[pscustomobject]@{ Node=$child.items; Path=$childPath + '[]' },
				[pscustomobject]@{ Node=$(if ($child.additionalProperties -isnot [bool]) { $child.additionalProperties }); Path=$childPath + '{}'}
			)) {
				if ($null -eq $shape.Node) { continue }
				if ($null -ne $shape.Node.properties) {
					$Lines.Add('')
					$Lines.Add('### Fields under `' + $shape.Path + '`')
					$Lines.Add('')
					Add-PropertyTable $Lines $shape.Node
				}
				Add-NestedTables $Lines $shape.Node $shape.Path
			}
		}
	}
	foreach ($keyword in @('allOf', 'oneOf', 'anyOf')) {
		$alternatives = @($Node.$keyword)
		for ($index = 0; $index -lt $alternatives.Count; $index++) {
			$alternative = $alternatives[$index]
			$alternativePath = $Path + ' (' + $keyword + ' ' + ($index + 1) + ')'
			if ($null -ne $alternative.properties) {
				$Lines.Add('')
				$Lines.Add('### Fields for `' + $alternativePath + '`')
				$Lines.Add('')
				Add-PropertyTable $Lines $alternative
			}
			Add-NestedTables $Lines $alternative $alternativePath
		}
	}
}

$index = New-Object System.Collections.Generic.List[string]
$index.Add('# Field-by-field API reference')
$index.Add('')
$index.Add('These pages are generated from the Draft 2020-12 schemas in `Docs/Modding/schemas`. Regenerate them with `Tools/Documentation/Generate-ApiReference.ps1`; do not edit generated field tables by hand.')
$index.Add('')
$index.Add('| Contract | Reference |')
$index.Add('| --- | --- |')

foreach ($file in Get-ChildItem -LiteralPath $schemaRoot -Filter '*.schema.json' |
	Sort-Object @{ Expression = { Get-OrdinalSortKey $_.Name } }) {
	$schema = Get-Content -LiteralPath $file.FullName -Raw -Encoding utf8 | ConvertFrom-Json
	$slug = $file.Name.Replace('.schema.json', '')
	$title = if ($schema.title) { [string]$schema.title } else { $slug }
	$lines = New-Object System.Collections.Generic.List[string]
	$lines.Add('# ' + $title)
	$lines.Add('')
	$lines.Add('> Generated from `' + $file.Name + '`. Runtime validation remains authoritative for cross-file and engine-dependent rules.')
	$lines.Add('')
	if ($schema.description) { $lines.Add([string]$schema.description); $lines.Add('') }
	$lines.Add('## Root fields')
	$lines.Add('')
	Add-PropertyTable $lines $schema
	Add-NestedTables $lines $schema 'root'

	if ($null -ne $schema.'$defs') {
		foreach ($definition in $schema.'$defs'.PSObject.Properties) {
			$lines.Add('')
			$lines.Add('## Definition: `' + $definition.Name + '`')
			$lines.Add('')
			$node = $definition.Value
			$lines.Add('Schema form: ' + (Schema-Type $node) + '.')
			if ($node.description) { $lines.Add(''); $lines.Add([string]$node.description) }
			$definitionNotes = Schema-Notes $node
			if (-not [string]::IsNullOrWhiteSpace($definitionNotes)) { $lines.Add(''); $lines.Add('Constraints: ' + $definitionNotes + '.') }
			$lines.Add('')
			Add-PropertyTable $lines $node
			Add-NestedTables $lines $node $definition.Name
		}
	}

	[IO.File]::WriteAllLines((Join-Path $outputRoot ($slug + '.md')), [string[]]$lines, (New-Object Text.UTF8Encoding($false)))
	$index.Add('| ' + (Escape-Cell $title) + ' | [' + $file.Name + '](api/' + $slug + '.md) |')
}

[IO.File]::WriteAllLines((Join-Path $ProjectRoot 'Docs/Modding/Public/reference/api-fields.md'), [string[]]$index, (New-Object Text.UTF8Encoding($false)))
Write-Host "Generated $(@(Get-ChildItem -LiteralPath $outputRoot -Filter '*.md').Count) API reference pages."
