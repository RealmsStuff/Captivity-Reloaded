# Draft Mod API v1 contract

Status: Draft 0.6. Manifest, ID, dependency, discovery, Core catalogs, save compatibility, typed asset slots, asset patches, enemy/clothing/weapon/usable factories, and discovery for template-backed stages are implemented. Other content-type schemas and gameplay factories remain draft work.

## Pack location

Desktop builds discover one pack per immediate child directory of `Mods` beside the game executable:

```text
Captivity Reloaded.exe
Mods/
  example.acid-gremlin/
    manifest.json
    content/
    assets/
```

The loader never reads a pack path outside that pack's directory. Absolute paths and `..` traversal are invalid.

## Manifest

Every pack has one UTF-8 `manifest.json`:

```json
{
  "schemaVersion": 1,
  "id": "example.acid-gremlin",
  "displayName": "Acid Gremlin",
  "version": "1.0.0",
  "modApiVersion": 1,
  "authors": ["Example Author"],
  "description": "Adds an Acid Gremlin enemy variant.",
  "dependencies": [
    {
      "id": "core",
      "version": ">=1.0.0"
    }
  ],
  "contentRoots": ["content"]
}
```

`core` is reserved, required and loaded first. External manifests cannot use the `core` pack ID or define content in the `core:` namespace.

Version strings use semantic versioning. The dependency range grammar will be finalized with the parser; v1 must at minimum support an exact version and `>=`.

## IDs

Pack IDs:

- Use lowercase ASCII letters, numbers, `.`, `_` and `-`.
- Begin with a letter or number.
- Do not contain `:` or `/`.
- Are compared ordinally and case-sensitively after lowercase validation.

Content and asset IDs have a namespace and path:

```text
<pack-id>:<category>/<name-or-path>

core:enemy/gremlin
core:stage/field-day
example.acid-gremlin:enemy/acid-gremlin
core:player/body/head/pale
```

The namespace must match the defining pack unless the file is an explicit patch. Path segments use lowercase letters, numbers and hyphens. Once released, an ID is permanent unless the pack supplies a migration alias.

## Pack layout

The recommended v1 layout is:

```text
manifest.json
content/
  enemies/
  clothing/
  items/
  stages/
  rules/
  patches/
assets/
  sprites/
  audio/
```

JSON files are discovered recursively inside declared content roots. Each content file declares its `type`; folder names are organizational and do not determine behaviour.

## Definition envelope

Every definition begins with:

```json
{
  "schemaVersion": 1,
  "type": "enemy",
  "id": "example.pack:enemy/example",
  "displayName": "Example"
}
```

Unknown `schemaVersion`, `type` or required fields invalidate that definition. A pack with invalid required definitions is disabled for the session. The validation report includes the pack, file, JSON path and human-readable reason.

## References and inheritance

Definitions reference other content by ID, never by Unity object name, asset filename or numeric database ID.

An `extends` field inherits from an existing compatible definition:

```json
"extends": "core:enemy/gremlin"
```

V1 permits only fields explicitly marked as overridable for that content type. Engine components, arbitrary C# class names and private prefab hierarchy paths are not valid fields.

### Enemy definition draft

The first supported enemy shape extends an existing Core enemy and supplies a `coreRigAtlas` visual. At runtime the loader clones that Core template, preserves its behaviour and rig, applies bounded stat overrides and replaces the declared sprite regions.

```json
{
  "schemaVersion": 1,
  "type": "enemy",
  "id": "example.enemies:enemy/acid-gremlin",
  "displayName": "Acid Gremlin",
  "extends": "core:enemy/gremlin",
  "description": "An atlas-driven Gremlin variant.",
  "stats": {
    "healthMax": 24,
    "speedAcceleration": 12,
    "speedMax": 4.5,
    "traction": 0.8,
    "bounty": 15,
    "healthIncreasePerWave": 2
  },
  "spawn": {
    "inheritTemplateSpawners": true
  },
  "visual": {
    "type": "coreRigAtlas",
    "atlas": "assets/enemies/acid-gremlin.png",
    "pixelsPerUnit": 32,
    "regions": {
      "body/head": { "x": 0, "y": 0, "width": 32, "height": 32 },
      "body/torso": { "x": 32, "y": 0, "width": 32, "height": 32 }
    }
  }
}
```

Stat values are bounded, atlas rectangles require a non-negative origin and positive dimensions, and region names are semantic paths. Unknown gameplay fields, component names, assembly names and unsupported visual types invalidate the definition.

Atlas coordinates use Unity's bottom-left origin. V1 exposes these regions for the current humanoid Core rig:

- `body/torso-lower`, `body/butt`, `body/hips`, `body/chest`, `body/neck`, `body/head`
- `body/arm-upper`, `body/arm-lower`, `body/hand`
- `body/leg-upper`, `body/leg-lower`, `body/foot-left`, `body/foot-right`

The shared arm, hand and leg regions are applied to both sides of the rig. The two feet remain separate because the Core zombie artwork uses distinct left and right sprites. A definition may replace only a subset and inherit the remaining artwork from its Core template.

`spawn.inheritTemplateSpawners` is optional and defaults to `false`. When enabled, the variant is added once to every Core stage spawner that can spawn its template, giving it the same selection weight as one existing entry in that spawner. Leaving it disabled registers and builds the enemy without changing Core stage encounters; a future custom stage can then reference it directly.

### Clothing definition draft

Additive clothing follows the same inheritance model. A definition extends one Core clothing template so attachment bones, offsets, sorting, colliders, tearing behaviour, category and compatibility rules remain intact. Its atlas replaces explicitly named template pieces:

```json
{
  "schemaVersion": 1,
  "type": "clothing",
  "id": "example.clothes:clothing/refitted-shirt",
  "displayName": "Refitted Shirt",
  "extends": "core:clothing/shirt-default",
  "unlockedByDefault": true,
  "visual": {
    "type": "coreClothingAtlas",
    "atlas": "assets/clothing/refitted-shirt.png",
    "pixelsPerUnit": 32,
    "regions": {
      "piece/shirt-spine": { "x": 0, "y": 0, "width": 32, "height": 32 },
      "piece/shirt-chest": { "x": 32, "y": 0, "width": 32, "height": 32 }
    }
  }
}
```

Runtime construction derives stable kebab-case piece slots from the inherited template's published `clp_` piece names (for example `clp_shirtChest` becomes `piece/shirt-chest`). The optional `icon` region replaces the wardrobe icon. Undeclared pieces and the icon are inherited. `unlockedByDefault` defaults to `false`; unlock and equipped flags for external clothing are stored under the full content ID rather than its temporary runtime number.

Region keys are validated during discovery against the published V1 clothing-slot catalog. The catalog is `icon` plus the semantic names derived from the Core wardrobe pieces: `arm-upper`, `belt`, `butt`, `chest`, `ear`, `glasses`, `hair`, `head`, `hips`, `l-arm-lower`, `l-arm-upper`, `l-foot`, `l-hand`, `l-leg-lower`, `l-leg-lower-armor`, `l-leg-lower-shoes`, `l-leg-lower-stocking`, `l-leg-upper`, `l-leg-upper-stocking`, `mask`, `neck`, their corresponding `r-` variants, `shirt-chest`, `shirt-collar`, `shirt-l-arm-lower`, `shirt-l-arm-upper`, `shirt-neck`, `shirt-r-arm-lower`, `shirt-r-arm-upper`, `shirt-spine`, `skirt-hips`, and `spine`, each prefixed with `piece/`. A published slot can still be unavailable on a particular inherited template; that template-specific mismatch is reported when the clothing is constructed.

### Weapon definition draft

The first additive weapon shape extends the Core pistol. It inherits the complete prefab, including firing behavior, statistics, ammunition, animation, audio, market availability, colliders, and attachment offsets. A definition supplies a new stable ID and may replace any subset of the three published pistol sprite slots:

```json
{
  "schemaVersion": 1,
  "type": "weapon",
  "id": "example.toys:item/weapon/toy-pistol",
  "displayName": "Toy Pistol",
  "extends": "core:item/weapon/pistol",
  "description": "A separate pistol variant.",
  "stats": {
    "damage": 3,
    "ammoMax": 120,
    "magazineSize": 12,
    "fireIntervalSeconds": 0.12,
    "recoil": 0.1
  },
  "visual": {
    "type": "coreWeaponSprites",
    "pixelsPerUnit": 32,
    "sprites": {
      "body": "assets/weapons/toy-body.png",
      "slide": "assets/weapons/toy-slide.png",
      "base": "assets/weapons/toy-base.png"
    }
  }
}
```

Each sprite is a separate safe pack-relative PNG. Valid V1 slots are `body`, `slide`, and `base`; undeclared slots retain Core artwork.

All statistics are optional and inherit the Core pistol value when omitted. Published overrides and bounds are: `damage` (0–10,000), `ammoMax` (1–100,000), `magazineSize` (1–100,000), `bulletsPerShot` (1–64), `penetration` (0–64), `rangeMultiplier` (0.05–1), `fireIntervalSeconds` (0.02–10), `recoil` (0–1), `movementRecoil` (0–1), and `knockbackX`/`knockbackY` (0–1,000). `ammoMax` and `magazineSize` must be supplied together, and the magazine cannot exceed total ammunition. Other weapon templates will be published after their rigs and gameplay assumptions are cataloged.

### Usable definition draft

Additive medicines and drugs inherit one packaged Core usable. The clone retains the Core effect implementation, usable type, audio, animation, equipment category, value, weight, stacking rules, market availability, and effect descriptions. JSON cannot select a class, status-effect type, method, or script:

```json
{
  "schemaVersion": 1,
  "type": "usable",
  "id": "example.medicine:item/usable/strong-aspirin",
  "displayName": "Strong Aspirin",
  "extends": "core:item/usable/aspirin",
  "description": "An additive Aspirin variant.",
  "visual": {
    "type": "coreUsableSprites",
    "icon": "assets/medicine/strong-aspirin.png",
    "world": "assets/medicine/strong-aspirin-world.png",
    "pixelsPerUnit": 32
  }
}
```

`extends` must be a known `core:item/usable/...` ID. `icon` is required. `world` is optional and defaults to the icon PNG, while still preserving the Core world sprite's pivot. Both are safe pack-relative PNG paths. Effect-parameter overrides are intentionally excluded until each reusable effect has a typed, bounded data contract.

### Stage definition draft

The first stage format separates encounter composition and level identity from Unity prefabs while inheriting a packaged Core layout, navigation map, lighting, ambience, vendors, and interactables. A later authored-layout type will replace geometry and navigation; it will be a new explicit `layout.type` rather than silently changing this contract.

```json
{
  "schemaVersion": 1,
  "type": "stage",
  "id": "example.stages:stage/training-yard",
  "displayName": "Training Yard",
  "extends": "core:stage/field-day",
  "description": "A template-backed custom encounter.",
  "layout": {
    "type": "coreStageLayout",
    "playerSpawn": { "x": 4, "y": 2 }
  },
  "waves": {
    "firstWaveEnemyCount": 5
  },
  "spawners": [
    {
      "id": "west-ground",
      "position": { "x": -12, "y": 3 },
      "enemies": [
        "core:enemy/zombie-1",
        "example.stages:enemy/training-zombie"
      ],
      "selectionWeight": 1,
      "minimumWave": 0,
      "delaySeconds": 1.5,
      "delayJitterSeconds": 0.25,
      "initialDelaySeconds": 1,
      "initialDelayJitterSeconds": 0.25,
      "spawnOutOfSight": true
    }
  ]
}
```

V1 `extends` references a non-hub Core stage. Positions use stage-local Unity units; coordinates must be finite and between -100,000 and 100,000. A stage has 1–256 uniquely named spawners; each contains 1–64 unique `enemy/...` content IDs. Enemy references are resolved only against enabled Core content and loaded mod dependencies. Selection weights are relative values from 0.001–1,000. `minimumWave` is 0–10,000, delays are bounded to 0–300 seconds, active spawn delays must be at least 0.02 seconds, and jitter cannot exceed its base delay. `firstWaveEnemyCount` is 1–1,000.

At runtime, the loader clones the selected Core stage, keeps its layout systems, disables inherited spawners, installs the JSON spawners, changes the player start and first-wave count, and adds a separate Locations entry. High scores are stored under the full stage content ID; temporary negative runtime numbers are never used as persistent identity. `ExampleMods/training-yard-stage` is a disabled working example. Its provisional spawn coordinates still require a hands-on gameplay pass.

## Assets

External paths are forward-slash paths relative to the pack root:

```json
"texture": "assets/sprites/acid-gremlin.png"
```

V1 initially targets PNG sprites and a documented subset of audio formats supported consistently by the selected Unity loaders. Executable files, scripts and arbitrary assemblies are rejected.

Asset replacement targets a stable public slot:

```json
{
  "schemaVersion": 1,
  "type": "assetPatch",
  "id": "example.skin:patch/player-art",
  "target": "core:player",
  "replacements": {
    "body/head/pale": "assets/sprites/head-pale.png"
  }
}
```

No replacement occurs merely because two files share a filename.

## Dependencies and load order

Required dependencies are loaded before the dependent pack. Missing or incompatible required dependencies disable the dependent pack.

After dependencies, ordering is deterministic by pack ID. Patches that target the same field or asset slot are conflicts. V1 reports the conflict and requires explicit user resolution; it must not silently choose based on filesystem enumeration order.

## Core content

Core uses the same logical registry and IDs, but may use Unity prefab adapters and packaged asset references while migration is incomplete. External packs cannot disable the Core pack.

## Error isolation

- Invalid external pack: disable that pack and dependent packs, then continue.
- Missing optional asset with a documented fallback: use the fallback and warn.
- Invalid explicit replacement: retain the previous resolved asset and report the error.
- Invalid Core definition: stop startup with a clear fatal report.

## Save compatibility

Save data uses full content IDs for modded content. Existing vanilla numeric IDs are preserved and mapped to stable `core:` IDs. Disabled or missing content remains in storage but is not instantiated.

## Security boundary

Mod API v1 is data-only. JSON values select supported engine behaviours; they do not execute expressions, reflection targets, shell commands, native libraries or arbitrary managed code.
