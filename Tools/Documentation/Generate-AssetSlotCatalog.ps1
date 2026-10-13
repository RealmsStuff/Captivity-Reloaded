param([string]$ProjectRoot)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
	$ProjectRoot = (Resolve-Path (Join-Path (Join-Path $PSScriptRoot '..') '..')).Path
}

$binderPath = Join-Path $ProjectRoot 'Assets/Scripts/Assembly-CSharp/CoreAssetSlotBinder.cs'
$binder = Get-Content -LiteralPath $binderPath -Raw -Encoding utf8
$legacyCatalogPath = Join-Path $ProjectRoot 'Assets/Scripts/Assembly-CSharp/CoreLegacyVisualSpriteCatalog.cs'
$legacyCatalog = Get-Content -LiteralPath $legacyCatalogPath -Raw -Encoding utf8
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

$slots = @($slots | Sort-Object @{ Expression = { Get-OrdinalSortKey $_.Id } } -Unique)
$lines = New-Object System.Collections.Generic.List[string]
$lines.Add('# Public asset-slot catalog')
$lines.Add('')
$lines.Add('This searchable catalog is generated from `CoreAssetSlotBinder.cs`, `CoreLegacyVisualSpriteCatalog.cs`, packaged Core weapon definitions, and `core-clothing.json`. Regenerate it with `Tools/Documentation/Generate-AssetSlotCatalog.ps1`.')
$lines.Add('')
$lines.Add('A slot is registered only when its baseline Unity sprite or renderer is available. This matters for legacy weapons and enemy rigs whose prefabs do not all expose the same pieces. An unknown target is rejected instead of guessing a private Unity name.')
$lines.Add('')
$lines.Add('## Enemy semantic slots')
$lines.Add('')
$lines.Add('For a Core enemy ID `core:enemy/<enemy>`, the binder publishes the following suffixes when that named renderer exists:')
$lines.Add('')
foreach ($part in $enemyParts) { $lines.Add('- `core:enemy/<enemy>/' + $part + '`') }
$lines.Add('')
$lines.Add('Paired limbs intentionally share semantic slots except for left and right feet. Use the in-game validation result to confirm availability on a particular legacy enemy.')
$lines.Add('')
$lines.Add('## Exact registered slot IDs')
$lines.Add('')
$lines.Add('| Slot ID | Family | Meaning | Availability |')
$lines.Add('| --- | --- | --- | --- |')
foreach ($slot in $slots) {
	$lines.Add('| `' + $slot.Id + '` | ' + $slot.Family + ' | ' + $slot.Meaning.Replace('|','\|') + ' | ' + $slot.Availability.Replace('|','\|') + ' |')
}
$lines.Add('')
$lines.Add('## Using a slot')
$lines.Add('')
$lines.Add('Declare the exact slot ID in the manifest `overrides` list, then target its owner in an `assetPatch` and use the suffix relative to that owner in `replacements`. See [Asset patches](../content/asset-patches.md) for examples and resolution rules.')

[IO.File]::WriteAllLines($output, [string[]]$lines, (New-Object Text.UTF8Encoding($false)))
Write-Host "Generated $($slots.Count) exact asset-slot entries and $($enemyParts.Count) enemy semantic patterns."
