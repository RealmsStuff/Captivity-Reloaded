param([string]$ProjectRoot)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
	$ProjectRoot = (Resolve-Path (Join-Path (Join-Path $PSScriptRoot '..') '..')).Path
}

$binderPath = Join-Path $ProjectRoot 'Assets/Scripts/Assembly-CSharp/CoreAssetSlotBinder.cs'
$binder = Get-Content -LiteralPath $binderPath -Raw -Encoding utf8
$legacyCatalogPath = Join-Path $ProjectRoot 'Assets/Scripts/Assembly-CSharp/CoreLegacyVisualSpriteCatalog.cs'
$legacyCatalog = Get-Content -LiteralPath $legacyCatalogPath -Raw -Encoding utf8
$enemyCatalogPath = Join-Path $ProjectRoot 'Assets/Resources/Modding/Core/enemy-asset-slots.json'
$enemyCatalog = Get-Content -LiteralPath $enemyCatalogPath -Raw -Encoding utf8 | ConvertFrom-Json
$output = Join-Path $ProjectRoot 'Docs/Modding/Public/reference/asset-slots.md'
$slots = New-Object System.Collections.Generic.List[object]

function Add-Slot([string]$Id, [string]$Family, [string]$Meaning, [string]$Availability = 'runtime baseline required') {
	$slots.Add([pscustomobject]@{ Id=$Id; Family=$Family; Meaning=$Meaning; Availability=$Availability })
}

function Get-OrdinalSortKey([string]$Value) {
	if ($null -eq $Value) { return '' }
	return [BitConverter]::ToString([Text.Encoding]::UTF8.GetBytes($Value)).Replace('-', '')
}

function Slug([string]$Name) {
	$result = New-Object System.Text.StringBuilder
	for ($index=0; $index -lt $Name.Length; $index++) {
		$character = $Name[$index]
		if ($character -eq '_') { $character = '-' }
		if ([char]::IsUpper($character) -and $index -gt 0 -and $result.Length -gt 0 -and $result[$result.Length-1] -ne '-') { [void]$result.Append('-') }
		[void]$result.Append([char]::ToLowerInvariant($character))
	}
	return $result.ToString()
}

function Get-CatalogSection($Slot) {
	if ($Slot.Family -eq 'clothing') { return 'Clothing' }
	if ($Slot.Family -eq 'player' -or $Slot.Family -eq 'player-mouth' -or $Slot.Id.StartsWith('core:player/')) { return 'Player' }
	if ($Slot.Family -eq 'enemy') { return 'Core enemy-specific slots' }
	if ($Slot.Family -eq 'weapon' -or $Slot.Family -eq 'weapon-effect') { return 'Weapons and weapon effects' }
	if ($Slot.Id.StartsWith('core:enemy-anatomy/')) { return 'Enemy anatomy' }
	if ($Slot.Id.StartsWith('core:stage-art/') -or $Slot.Id.StartsWith('core:map-art/')) { return 'Stages and map art' }
	if ($Slot.Id.StartsWith('core:sprite/')) {
		$name = $Slot.Id.Substring('core:sprite/'.Length)
		$first = [char]::ToUpperInvariant($name[0])
		if ($first -le 'D') { return 'Legacy compatibility (A-D)' }
		if ($first -le 'H') { return 'Legacy compatibility (E-H)' }
		if ($first -le 'L') { return 'Legacy compatibility (I-L)' }
		if ($first -le 'P') { return 'Legacy compatibility (M-P)' }
		if ($first -le 'T') { return 'Legacy compatibility (Q-T)' }
		return 'Legacy compatibility (U-Z)'
	}
	return 'Other'
}

$owners = @{}
foreach ($match in [regex]::Matches($binder, 'ContentId\s+(?<name>\w+)\s*=\s*ContentId\.Parse\("(?<id>[^"]+)"\)')) {
	$owners[$match.Groups['name'].Value] = $match.Groups['id'].Value
}

# Player body and face slots.
$partBlock = [regex]::Match($binder, '(?s)PlayerPartNames\s*=.*?\{(?<body>.*?)\n\s*\};').Groups['body'].Value
$parts = @([regex]::Matches($partBlock, '\{\s*"[^"]+"\s*,\s*"(?<part>[^"]+)"\s*\}') |
	ForEach-Object {$_.Groups['part'].Value} | Sort-Object @{ Expression = { Get-OrdinalSortKey ([string]$_) } } -Unique)
$skinBlock = [regex]::Match($binder, 'SkinColors\s*=\s*\{(?<body>[^}]+)\}').Groups['body'].Value
$skins = @([regex]::Matches($skinBlock, 'SkinColor\.(?<skin>\w+)') | ForEach-Object {$_.Groups['skin'].Value.ToLowerInvariant()})
foreach ($part in $parts) { foreach ($skin in $skins) {
	$path = if ($part.StartsWith('face/')) { $part } else { 'body/' + $part }
	Add-Slot "core:player/$path/$skin" 'player' "Player $part artwork for $skin skin" 'registered when the shipped skin sprite exists'
} }

# Player mouth and muzzle-flash switch tables.
foreach ($match in [regex]::Matches($binder, 'case\s+"[^"]+"\s*:\s*o_suffix\s*=\s*"(?<suffix>[^"]+)"')) {
	Add-Slot ('core:player/face/mouth/' + $match.Groups['suffix'].Value) 'player-mouth' 'Player mouth expression' 'registered when a scene mouth manager supplies the sprite'
}
foreach ($index in 1..3) { Add-Slot "core:weapon-effects/muzzle-flashes/$index" 'weapon-effect' "Core muzzle flash $index" 'registered when used by a shipped gun' }

# Core weapons.
$contentRoot = Join-Path $ProjectRoot 'Assets/Resources/Modding/Core/Content'
foreach ($file in Get-ChildItem -LiteralPath $contentRoot -Filter '*.json' -File) {
	try { $document = Get-Content -LiteralPath $file.FullName -Raw -Encoding utf8 | ConvertFrom-Json } catch { continue }
	if ($document.type -ne 'coreWeapon') { continue }
	$name = ([string]$document.id).Substring('core:item/weapon/'.Length)
	Add-Slot "core:weapon/$name/body" 'weapon' (([string]$document.displayName) + ' inventory/body sprite') 'registered when the shipped icon exists'
	Add-Slot "core:weapon/$name/base" 'weapon' (([string]$document.displayName) + ' base renderer') 'registered when the shipped base renderer exists'
}
Add-Slot 'core:weapon/pistol/slide' 'weapon' 'Pistol slide renderer' 'registered when the shipped slide renderer exists'

# Core clothing catalog.
$clothing = Get-Content -LiteralPath (Join-Path $contentRoot 'core-clothing.json') -Raw -Encoding utf8 | ConvertFrom-Json
foreach ($entry in $clothing.entries) {
	if ($null -ne $entry.icon) { Add-Slot ([string]$entry.id + '/icon') 'clothing' 'Wardrobe icon' 'registered from the reconstructed garment' }
	foreach ($piece in @($entry.pieces)) {
		Add-Slot ([string]$entry.id + '/' + [string]$piece.slot) 'clothing' ('Garment ' + [string]$piece.slot) 'registered from the reconstructed garment'
	}
}

# Enemy semantic family. These are patterns because varied legacy rigs do not all contain every part.
$enemyPartBlock = [regex]::Match($binder, '(?s)CoreEnemyPartNames\s*=.*?\{(?<body>.*?)\n\s*\};').Groups['body'].Value
$enemyParts = @([regex]::Matches($enemyPartBlock, '\{\s*"(?<part>body/[^"]+)"') |
	ForEach-Object {$_.Groups['part'].Value} | Sort-Object @{ Expression = { Get-OrdinalSortKey ([string]$_) } } -Unique)

# Runtime-authoritative Core enemy slots and their compatibility aliases.
$enemyCanonicalCount = 0
$enemyAliasCount = 0
foreach ($enemy in @($enemyCatalog.enemies)) {
	foreach ($record in @($enemy.slots)) {
		$spriteName = if (![string]::IsNullOrWhiteSpace([string]$record.animatedSpriteName)) { [string]$record.animatedSpriteName } else { [string]$record.spriteName }
		$usage = if (![string]::IsNullOrWhiteSpace([string]$record.animatedSpriteName)) { 'animation variant' } else { 'rig renderer' }
		Add-Slot ([string]$enemy.id + '/' + [string]$record.slot) 'enemy' (([string]$enemy.id).Substring('core:enemy/'.Length) + ' ' + [string]$record.bone + ' (' + $spriteName + ', ' + $usage + ')') 'registered from the generated Core enemy rig catalog'
		$enemyCanonicalCount++
	}
	foreach ($alias in @($enemy.legacyAliases)) {
		Add-Slot ([string]$enemy.id + '/' + [string]$alias.slot) 'enemy' (([string]$enemy.id).Substring('core:enemy/'.Length) + ' compatibility alias for ' + (@($alias.targets) -join ', ')) 'registered when at least one target renderer is available'
		$enemyAliasCount++
	}
}
if ([int]$enemyCatalog.enemyCount -ne @($enemyCatalog.enemies).Count) { throw 'Core enemy asset-slot catalog enemyCount is stale.' }
if ([int]$enemyCatalog.slotCount -ne $enemyCanonicalCount) { throw 'Core enemy asset-slot catalog slotCount is stale.' }

# Direct named bindings resolve owner constants and suffixes.
foreach ($match in [regex]::Matches($binder, 'BindNamedCoreSpriteSlot\("(?<sprite>[^"]+)",\s*(?<owner>\w+),\s*"(?<suffix>[^"]+)"\)')) {
	$owner = $owners[$match.Groups['owner'].Value]
	if ($owner) { Add-Slot ($owner + '/' + $match.Groups['suffix'].Value) 'named-core-sprite' ('Core sprite ' + $match.Groups['sprite'].Value) }
}

# Indirect legacy visual catalog.
$legacyBlock = [regex]::Match($legacyCatalog, '(?s)Names\s*=\s*\{(?<body>.*?)\};').Groups['body'].Value
foreach ($match in [regex]::Matches($legacyBlock, '"(?<name>[^"]+)"')) {
	$name = $match.Groups['name'].Value
	Add-Slot ('core:sprite/' + (Slug $name) + '/image') 'legacy-presentation' ('Legacy Core sprite ' + $name)
}

# Fixed usable-vendor slot.
Add-Slot 'core:map-art/usable-vendor/sprite' 'map-art' 'Usable vendor world artwork' 'registered when the Core template or a Tiled vendor is available'

$duplicateSlotIds = @($slots | Group-Object Id | Where-Object Count -gt 1)
if ($duplicateSlotIds.Count -gt 0) { throw ('Duplicate public asset-slot IDs: ' + (($duplicateSlotIds | ForEach-Object Name) -join ', ')) }
$slots = @($slots | Sort-Object @{ Expression = { Get-OrdinalSortKey $_.Id } })
$lines = New-Object System.Collections.Generic.List[string]
$lines.Add('# Public asset-slot catalog')
$lines.Add('')
$lines.Add('This searchable catalog is generated from `CoreAssetSlotBinder.cs`, `CoreLegacyVisualSpriteCatalog.cs`, `enemy-asset-slots.json`, packaged Core weapon definitions, and `core-clothing.json`. Regenerate it with `Tools/Documentation/Generate-AssetSlotCatalog.ps1`.')
$lines.Add('')
$lines.Add('A slot is registered only when its baseline Unity sprite or renderer is available. This matters for legacy weapons and enemy rigs whose prefabs do not all expose the same pieces. An unknown target is rejected instead of guessing a private Unity name.')
$lines.Add('')
$lines.Add('Prefer the player, enemy, clothing, weapon, and stage sections when a semantic slot exists. The `core:sprite/...` sections are a compatibility layer for artwork that does not yet have a more specific public owner; those replacements can affect every Core object that shares the same original sprite name.')
$lines.Add('')
$lines.Add('## Find the right slot')
$lines.Add('')
$lines.Add('- Replacing one Core enemy body part: use [Core enemy-specific slots](#core-enemy-specific-slots). The [detailed enemy catalog](enemy-asset-slots.md) also shows rig bones, original sprite names, animation-only variants, and aliases by enemy.')
$lines.Add('- Replacing player artwork: use [Player](#player).')
$lines.Add('- Retexturing clothing: use [Clothing](#clothing).')
$lines.Add('- Retexturing guns or muzzle flashes: use [Weapons and weapon effects](#weapons-and-weapon-effects).')
$lines.Add('- Replacing FER or map artwork: use [Stages and map art](#stages-and-map-art).')
$lines.Add('- Replacing shared legacy body pieces, props, UI, backgrounds, or effects: search the [Legacy compatibility](#legacy-compatibility-a-d) sections by original sprite name.')
$lines.Add('')
$lines.Add('## Enemy compatibility patterns')
$lines.Add('')
$lines.Add('The generated enemy catalog below is authoritative. For older development projects without that generated catalog, the binder can fall back to these broad semantic patterns when the named renderer exists:')
$lines.Add('')
foreach ($part in $enemyParts) { $lines.Add('- `core:enemy/<enemy>/' + $part + '`') }
$lines.Add('')
$lines.Add('Some generated enemies retain compatibility aliases for these broad names. New retextures should use the exact enemy-specific paths below, especially for multipart feet, anatomy, asymmetric limbs, and animation-only artwork.')
$lines.Add('')
$lines.Add('## Using a slot')
$lines.Add('')
$lines.Add('Declare the exact slot ID in the manifest `overrides` list, then target its owner in an `assetPatch` and use the suffix relative to that owner in `replacements`. See [Asset patches](../content/asset-patches.md) for examples and resolution rules.')
$lines.Add('')
$lines.Add('## Exact registered slot IDs')
$lines.Add('')
$sectionOrder = @(
	'Player',
	'Core enemy-specific slots',
	'Enemy anatomy',
	'Clothing',
	'Weapons and weapon effects',
	'Stages and map art',
	'Legacy compatibility (A-D)',
	'Legacy compatibility (E-H)',
	'Legacy compatibility (I-L)',
	'Legacy compatibility (M-P)',
	'Legacy compatibility (Q-T)',
	'Legacy compatibility (U-Z)',
	'Other'
)
$groupedSlots = @{}
foreach ($slot in $slots) {
	$section = Get-CatalogSection $slot
	if (!$groupedSlots.ContainsKey($section)) { $groupedSlots[$section] = New-Object System.Collections.Generic.List[object] }
	$groupedSlots[$section].Add($slot)
}
$lines.Add('| Section | Entries |')
$lines.Add('| --- | ---: |')
foreach ($section in $sectionOrder) {
	if (!$groupedSlots.ContainsKey($section)) { continue }
	$anchor = $section.ToLowerInvariant().Replace(' ', '-').Replace('(', '').Replace(')', '')
	$lines.Add('| [' + $section + '](#' + $anchor + ') | ' + $groupedSlots[$section].Count + ' |')
}
$lines.Add('')
foreach ($section in $sectionOrder) {
	if (!$groupedSlots.ContainsKey($section)) { continue }
	$lines.Add('### ' + $section)
	$lines.Add('')
	if ($section.StartsWith('Legacy compatibility')) {
		$lines.Add('These slots follow original Core sprite names and may affect multiple objects that reuse the same sprite. Prefer a semantic slot above when one exists.')
		$lines.Add('')
	}
	$lines.Add('| Slot ID | Meaning | Availability |')
	$lines.Add('| --- | --- | --- |')
	foreach ($slot in $groupedSlots[$section]) {
		$lines.Add('| `' + $slot.Id + '` | ' + $slot.Meaning.Replace('|','\|') + ' | ' + $slot.Availability.Replace('|','\|') + ' |')
	}
	$lines.Add('')
}

if ($lines.Count -gt 0 -and $lines[$lines.Count - 1] -eq '') { $lines.RemoveAt($lines.Count - 1) }

[IO.File]::WriteAllLines($output, [string[]]$lines, (New-Object Text.UTF8Encoding($false)))
Write-Host "Generated $($slots.Count) exact asset-slot entries and $($enemyParts.Count) enemy semantic patterns."
