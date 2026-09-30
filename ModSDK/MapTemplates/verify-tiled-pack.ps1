param(
    [Parameter(Position = 0)]
    [string] $PackPath
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($PackPath)) {
    $PackPath = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) 'TiledStage'
}
$packRoot = [IO.Path]::GetFullPath($PackPath).TrimEnd('\', '/')
$packPrefix = $packRoot + [IO.Path]::DirectorySeparatorChar
$errors = [Collections.Generic.List[string]]::new()
$checkedReferences = 0

function Add-Error([string] $Message) {
    $script:errors.Add($Message)
}

function Resolve-PackReference([string] $Owner, [string] $Reference, [bool] $FromPackRoot) {
    if ([string]::IsNullOrWhiteSpace($Reference)) {
        Add-Error "$Owner contains an empty file reference."
        return
    }
    if ([IO.Path]::IsPathRooted($Reference)) {
        Add-Error "$Owner uses an absolute path: $Reference"
        return
    }

    $base = if ($FromPackRoot) { $packRoot } else { Split-Path -Parent $Owner }
    $resolved = [IO.Path]::GetFullPath((Join-Path $base $Reference))
    if (-not $resolved.StartsWith($packPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        Add-Error "$Owner references a path outside the pack: $Reference"
        return
    }
    if (-not (Test-Path -LiteralPath $resolved -PathType Leaf)) {
        Add-Error "$Owner references a missing file: $Reference"
        return
    }
    $script:checkedReferences++
}

function Visit-JsonNode($Node, [string] $Owner) {
    if ($null -eq $Node) { return }
    if ($Node -is [Collections.IEnumerable] -and $Node -isnot [string] -and
        $Node -isnot [Management.Automation.PSCustomObject]) {
        foreach ($item in $Node) { Visit-JsonNode $item $Owner }
        return
    }
    if ($Node -isnot [Management.Automation.PSCustomObject]) { return }

    if ($Node.PSObject.Properties['type'] -and $Node.type -eq 'file' -and
        $Node.PSObject.Properties['value']) {
        $optionalVisual = $Node.PSObject.Properties['name'] -and $Node.name -in @(
            'visualFile', 'brokenVisualFile', 'closedVisualFile', 'openVisualFile',
            'offVisualFile', 'onVisualFile')
        if (-not ($optionalVisual -and [string]::IsNullOrEmpty([string] $Node.value))) {
            Resolve-PackReference $Owner ([string] $Node.value) $true
        }
    }
    foreach ($property in $Node.PSObject.Properties) {
        $isFileReference = $property.Name -in @('template', 'image')
        if ($property.Name -eq 'source' -and $property.Value -is [string]) {
            $isFileReference = [IO.Path]::GetExtension([string] $property.Value) -in @('.json', '.tsj', '.tsx', '.tx')
        }
        if ($isFileReference -and $property.Value -is [string]) {
            Resolve-PackReference $Owner ([string] $property.Value) $false
        }
        Visit-JsonNode $property.Value $Owner
    }
}

if (-not (Test-Path -LiteralPath $packRoot -PathType Container)) {
    throw "Pack directory does not exist: $packRoot"
}

$jsonFiles = @(Get-ChildItem -LiteralPath $packRoot -Recurse -File |
    Where-Object { $_.Extension -in @('.json', '.tsj', '.tx') })
$documents = @{}
foreach ($file in $jsonFiles) {
    try {
        $document = Get-Content -Raw -LiteralPath $file.FullName | ConvertFrom-Json
        $documents[$file.FullName] = $document
        Visit-JsonNode $document $file.FullName
    }
    catch {
        Add-Error "$($file.FullName) is not valid JSON: $($_.Exception.Message)"
    }
}

$stageFiles = @($documents.Keys | Where-Object {
    $document = $documents[$_]
    $document.PSObject.Properties['type'] -and $document.type -eq 'stage'
})
if ($stageFiles.Count -eq 0) {
    Add-Error 'No stage definition was found.'
}

foreach ($stageFile in $stageFiles) {
    $stage = $documents[$stageFile]
    if (-not $stage.layout -or $stage.layout.type -ne 'tiledJson') { continue }
    Resolve-PackReference $stageFile ([string] $stage.layout.path) $true

    $mapPath = [IO.Path]::GetFullPath((Join-Path $packRoot ([string] $stage.layout.path)))
    if (-not $documents.ContainsKey($mapPath)) { continue }
    $map = $documents[$mapPath]
    if ($map.infinite) { Add-Error "$mapPath is infinite; v1 requires a finite map." }
    if ($map.orientation -ne 'orthogonal') { Add-Error "$mapPath is not orthogonal." }
    if ($map.width -le 0 -or $map.height -le 0) { Add-Error "$mapPath has invalid dimensions." }

    $stageSpawnerIds = @($stage.spawners | ForEach-Object { [string] $_.id })
    $mapSpawnerIds = @()
    $mapDoorIds = @()
    $playerSpawnCount = 0
    $objects = @($map.layers | Where-Object { $_.type -eq 'objectgroup' } | ForEach-Object { $_.objects })
    foreach ($object in $objects) {
        $class = if ($object.class) { [string] $object.class } else { [string] $object.type }
        if ($object.template) {
            $templatePath = [IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $mapPath) ([string] $object.template)))
            if ($documents.ContainsKey($templatePath)) {
                $templateObject = $documents[$templatePath].object
                $class = if ($templateObject.class) { [string] $templateObject.class } else { [string] $templateObject.type }
				if ([string]::IsNullOrWhiteSpace([string] $object.name)) { $objectName = [string] $templateObject.name }
            }
        }
		if (-not $objectName) { $objectName = [string] $object.name }
        if ($class -eq 'player-spawn') { $playerSpawnCount++ }
		if ($class -eq 'enemy-spawner') { $mapSpawnerIds += $objectName }
		$coreObjectKind = @($object.properties | Where-Object { $_.name -eq 'coreObjectKind' } |
			Select-Object -First 1 | ForEach-Object { [string] $_.value })
		if ($class -eq 'door' -or ($class -eq 'core-stage-object' -and $coreObjectKind -eq 'door')) {
			$mapDoorIds += $objectName
		}
		$objectName = $null
    }
    if ($playerSpawnCount -ne 1) { Add-Error "$mapPath has $playerSpawnCount player spawns; exactly one is required." }
    foreach ($id in $stageSpawnerIds) {
        if ($id -notin $mapSpawnerIds) { Add-Error "Stage spawner '$id' has no matching Tiled enemy-spawner." }
    }
    foreach ($id in $mapSpawnerIds) {
        if ($id -notin $stageSpawnerIds) { Add-Error "Tiled enemy-spawner '$id' has no matching stage spawner." }
    }
	foreach ($spawner in $stage.spawners) {
		foreach ($door in @($spawner.requiredOpenDoors) + @($spawner.requiredClosedDoors)) {
			if ([string]::IsNullOrWhiteSpace([string] $door)) { continue }
			if ([string] $door -notin $mapDoorIds) { Add-Error "Stage spawner '$($spawner.id)' references unknown Tiled door '$door'." }
		}
	}
}

if ($errors.Count -gt 0) {
    Write-Host "Tiled pack verification failed with $($errors.Count) error(s):" -ForegroundColor Red
    foreach ($message in $errors) { Write-Host " - $message" -ForegroundColor Red }
    exit 1
}

Write-Host "Tiled pack verification passed." -ForegroundColor Green
Write-Host "JSON documents: $($jsonFiles.Count)"
Write-Host "Resolved pack-local references: $checkedReferences"
Write-Host "Stage definitions: $($stageFiles.Count)"
