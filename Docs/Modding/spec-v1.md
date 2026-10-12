# Draft Mod API v1 contract

Status: Draft 0.9, freeze review. Pack discovery, dependencies, Core catalogs, stable IDs, asset patches, save keys, all public content parsers, runtime factories, the Mods panel, selectable game modes, and the experimental Tiled layout pipeline are implemented. Draft 2020-12 JSON Schemas cover every public v1 content contract—including external Tiled JSON/TSJ tilesets—and label stable versus experimental fields with `x-stability`. All repository examples pass the schema validator. Remaining work is runtime verification, clean-build/release testing, and final licensing. Newer weapon, enemy, clothing, usable, rule-profile, and Tiled modules remain experimental until their gameplay behavior is verified broadly enough for promotion.

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
  "optionalDependencies": [
    {
      "id": "example.optional-integration",
      "version": ">=1.0.0"
    }
  ],
  "contentRoots": ["content"]
}
```

`core` is reserved, required and loaded first. External manifests cannot use the `core` pack ID or define content in the `core:` namespace.

Optional `previewImages` is an ordered list of up to eight pack-relative PNG paths, for example `"previewImages": ["assets/previews/1.png", "assets/previews/2.png"]`. The Mods panel shows a blank preview box if the list is absent and loops through multiple images every three seconds. Put the actual image files in the distributed pack. `description` is shown in the selected pack's details.

Packaged Unity-authored mods may also contain an experimental `assetBundles` list with one generated payload for each of `windows`, `android`, and `webgl`. Each entry records its safe pack-relative `.bundle` path, byte size, and SHA-256. The Captivity Mod Packager owns this field; authors configure prefab assets and platforms through a Unity authoring profile rather than editing bundle metadata by hand.

Version strings use semantic versioning. The dependency range grammar will be finalized with the parser; v1 must at minimum support an exact version and `>=`.

Optional manifest ordering and conflict fields are now supported:

```json
{
  "loadAfter": ["example.framework"],
  "loadBefore": [],
  "priority": 10,
  "conflicts": ["example.incompatible-pack"],
  "overrides": ["core:player/body/head/pale"]
}
```

Required `dependencies` are topologically ordered and version-checked. If one is missing, incompatible, invalid, conflicting, or disabled by the user, the dependent pack is disabled with a direct explanation. `optionalDependencies` affect ordering when a compatible enabled pack is present, but never prevent the consumer from loading; missing or disabled optional integrations are informational. A pack ID may appear in only one dependency list.

`loadAfter` and `loadBefore` are soft ordering edges used only when both packs exist. Lower priorities load first. `conflicts` disables the lower-priority pack, with pack ID as the deterministic tie breaker. A later sprite patch may win a collision only when its manifest lists the exact public slot ID in `overrides`; otherwise the Core asset is retained. Validation errors are queued as top-left Captivity status notifications when gameplay begins; pack state remains visible in the Mods screen.

The title screen's **MODS** panel colors loaded packs green, disabled packs red, and invalid or conflicting packs yellow; it also shows the selected pack's description and preview images. External packs are enabled by default. The player can persistently enable or disable them there; changes apply on the next game restart so runtime objects are never removed halfway through a session. Core cannot be disabled. Browse reads a validated GitHub catalog and supports staged installs and updates, rollback, recoverable removal, and version-specific required-dependency batches. The catalog's required dependencies and conflicts must match the ZIP manifest; all batch archives are validated before installed folders change. Catalog installs keep an external ledger of bounded file-tree fingerprints. Edited or untracked installs require explicit update confirmation and are copied to separate recovery storage before replacement.

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

The first supported enemy shape extends an existing Core enemy. Its `coreRigAtlas` visual is optional: omitting it preserves the complete Core appearance, while supplying it replaces only the declared sprite regions. At runtime the loader clones that Core template and preserves its behaviour and rig while applying bounded data overrides.

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

`spawn.inheritTemplateSpawners` is optional and defaults to `false`. When enabled, the variant is added once to every Core stage spawner that can spawn its template, giving it the same selection weight as one existing entry in that spawner. Leaving it disabled registers and builds the enemy without changing Core stage encounters; authored and template-backed stages can reference it directly.

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

Reconstructed Core garments also publish these semantic sprites as asset-patch slots beneath their content ID. For example, an in-place refit of the default shirt targets `core:clothing/shirt-default` and replaces `piece/shirt-chest` and `piece/shirt-spine`. This preserves the original garment's unlock, equipment, compatibility, and save identity instead of adding another wardrobe entry.

Experimental `playerAttachment` documents add persistent artwork to an existing semantic player bone without participating in wardrobe categories or saves. They support one default sprite, optional per-tone sprites, automatic palette matching through `tintWithSkin`, offsets, pivot, sorting, and bounded sway. Core exposes Pale, White, Tan, Black, Olive, Brown, and Deep tones. Attachments extend rendering only: v1 attachments do not introduce new animated bones, physics colliders, or gameplay anatomy.

### Weapon definition draft

An additive weapon extends a Core gun whose sprite layout has been published. It inherits the complete prefab, including firing behavior, statistics, ammunition, animation, audio, market availability, colliders, and attachment offsets. Pistol, Tenelli SO3, and Revolver .44 layouts are currently published. A definition supplies a new stable ID and may replace any subset of that template's slots:

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

Each sprite is a separate safe pack-relative PNG. Pistol slots are `body`, `slide`, `magazine`, and `base`. Tenelli SO3 slots are `body`, `base`, `magazine`, and `shell`. Revolver .44 slots are `body`, `hammer`, `chamber`, `base`, `bullet`, and `bullet-1` through `bullet-5`. Undeclared slots retain Core artwork. The `body` slot is the item icon rather than an ejected projectile.

All statistics are optional and inherit the selected Core gun's value when omitted. Published overrides and bounds are: `damage` (0–10,000), `ammoMax` (1–100,000), `magazineSize` (1–100,000), `bulletsPerShot` (1–64), `penetration` (0–64), `rangeMultiplier` (0.05–1), `fireIntervalSeconds` (0.02–10), `recoil` (0–1), `movementRecoil` (0–1), and `knockbackX`/`knockbackY` (0–1,000). `ammoMax` and `magazineSize` must be supplied together, and the magazine cannot exceed total ammunition. The experimental `behavior` object exposes bounded burst and charge fire, beams, alternate fire, melee overlap attacks, tracer presentation, custom WAV/OGG fire and reload sounds, muzzle sprites, casing sprites/ejection, physical projectile sprites with optional radial damage, and timed PNG sprite clips for idle/equip/fire/reload. Other weapon templates will be published after their rigs and gameplay assumptions are cataloged.

Fully original `originalWeaponSprites` definitions omit `extends` and construct up to 16 independently positioned, rotated, scaled, and sorted PNG parts without a Core gun prefab. Reviewed behavior also includes alternate/charge sprite clips, alternate-fire bursts and recoil, charge feedback, and ammunition modifiers for penetration, weak points, damage-over-time, and slowing.

### Usable definition draft

Medicines and drugs may inherit one packaged Core usable. The clone retains the Core effect implementation, usable type, audio, animation, equipment category, value, weight, stacking rules, market availability, and effect descriptions. JSON cannot select a class, status-effect type, method, or script:

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

When supplied, `extends` must be a known `core:item/usable/...` ID. `icon` is required. `world` is optional and defaults to the icon PNG. A prefab-free usable instead omits `extends`, uses `originalUsableSprites`, provides its own pivot/collider/sorting values, and uses `effectMode: "replace"`. Both visual types use safe pack-relative PNG paths.

Usables may declare `effectMode` as `inherit` (default), `add`, or `replace`. The bounded `effects` list supports `restoreHealth`, `restoreStrength`, `restoreStamina`, `reducePleasure`, `ragdoll`, `invulnerability`, timed `statModifier`, `refillAmmo`, `fillAllAmmo`, `repairClothing`, and `gainMoney` entries. Stat modifiers use a fixed whitelist and every amount and duration is range-checked; arbitrary scripts, classes, and method names remain forbidden.

### Expanded gameplay overrides

All new gameplay fields are optional and inherit the selected Core template when absent.

- Clothing publishes `category` and `incompatibleCategories`. Valid category names are `Hair`, `Upper`, `Lower`, `Shoes`, `Hat`, `Sleeves`, `Stockings`, and `Other`. Inherited visuals may use one `coreClothingAtlas` or map separate PNG files with `coreClothingSprites`; fully original rigs use `originalClothingAtlas` or `originalClothingSprites`. Experimental attachment entries may override bone placement, pivot, sorting, body-part hiding, tearing flags, and bounded sway physics; equipped effects provide bounded armor and player-stat modifiers.
- Enemies publish `attacks` (selected by zero-based inherited attack index), `behavior.visionRange`, `behavior.ignoreWave`, `animation.speedMultiplier`, and `drops` containing a bounded chance and stable item IDs. Attack overrides include chance, damage, knockback, cooldown, initiate/hit ranges, movement, and duration. Fully original attack and finisher clips may publish ordered `attackHit`, facing-relative `impulse`, pack-local positional `sound`, `cameraShake`, atlas-backed `spriteEffect`, and inert semantic `cue` events. Cues preserve authoring markers such as `finisher.thrust`, `audio.unique`, or `attack.projectile` without invoking Unity methods and may appear on looping clips; gameplay events remain restricted to referenced non-looping attack or finisher clips. Event-driven hits replace the legacy single `hitTimeSeconds` fallback and permit bounded multi-hit attacks. Reusable behavior modules include `regeneration`, `berserk`, `lifesteal`, `thorns`, `onHitRagdoll`, `spawnOnDeath`, and `speedPulse`. Fully original enemies may also use the experimental `downedFinisher` module. It triggers near a ragdolled player or through the normal voluntary `Expose` input and uses the Core struggle HUD and normal keyboard, controller, or mobile struggle controls. Packs configure its trigger delay/range, meter, cooldown, and either one presentation or 2–8 ordered phases with phase-specific enemy/player animations, timed events, input power, decay, and an `adaptive`, `alternate`, `rotate`, or `tap` input pattern. Success and failure outcomes independently combine health, strength, pleasure, libido, recovery, enemy stun, and player ragdoll effects. Its optional `statuses` text table customizes active, mating, infusion, defeat, orgasm, pregnancy, labor, birth, and mind-break cards with bounded text and `{enemy}`, `{fetus}`, and `{child}` substitutions. Procedural player-bone poses are restored afterward, and looping player-side phase clips repeat their events.
- Guns additionally publish `reloadSeconds`, `equipSeconds`, `weight`, `value`, `infiniteAmmo`, `semiAutomatic`, `marketable`, `holdType`, and `reloadType`.
- Usables publish weight, value, equip duration, market availability, displayed good/bad effect descriptions, and the bounded typed effects described above.

Challenge definitions can inherit any registered challenge through `extends`, preserving the template's complete objective component while replacing its ID, text, rewards, and optionally its stage. This exposes all 79 Core behaviors without attempting to flatten specialized Unity references into unsafe JSON. General data objectives include kills, weapon-filtered kills, reached/survived/flawless waves, pickups, item use, successful interactions, shots, damage events, births, impregnations, rape events, orgasms, and mind breaks. Supported enemy and item allowlists use stable IDs. Experimental ordered `steps` compose these objectives into a sequence that resets on activation and must finish within one run. Experimental reward bundles grant bounded currency, items, one-copy weapons, and persistent content entitlements; legacy clothing rewards remain stable. Omitting `stage` creates a General challenge. Stable namespaced IDs are converted to deterministic private legacy database keys so completion survives restarts without consuming Core challenge IDs.

The converted Femboy shirt, challenge showcase, Prey green zombie, and additive Nerf pistol examples exercise these fields. Goblin Slayer Armor separately demonstrates a faithful multi-garment Core artwork replacement.

Rule profiles default to mutually selectable game modes. The runtime exposes valid loaded selectable profiles in Options, persists the chosen stable ID, and falls back to Standard if that pack disappears or becomes invalid. A profile with `activation: "pack"` instead follows its owning pack and is omitted from the game-mode list; this is intended for inseparable accessibility or safety behavior. Published bounded modules are `weaponProgression`, `spawnModifiers`, `waveRules`, `economyRules`, `playerRules`, `consumableOverride`, `eventReward`, and `experimentScaling`. They cover starter/rotating weapons, spawn pacing and caps, wave growth/intermissions/finite completion, economy, player and weapon behavior, reviewed Core consumable changes, bounded event rewards, and progression from completed experiments.

## Difficulty profiles

Difficulty profiles are selectable rule definitions. Enabled profiles are added to the existing Options dropdown and persist by stable content ID:

```json
{
  "schemaVersion": 1,
  "type": "difficulty",
  "id": "example.rules:difficulty/nightmare",
  "displayName": "Nightmare",
  "sortOrder": 400,
  "enemyHealthMultiplier": 1.5,
  "playerDamageTakenMultiplier": 1.5,
  "escapeStrengthMultiplier": 0.25
}
```

All three multipliers must be finite values from 0.1 through 10. Lower `sortOrder` values appear first. IDs and display names must be unique among enabled profiles. Casual, Normal, and Hard are registered as Core profiles; legacy saves containing those display names continue to resolve. `ExampleMods/dakozan-extended-difficulties` recreates @DakoZan's Very Hard and Nightmare behavior without a replacement DLL.

## Rule profiles

Rule profiles compose reviewed gameplay modules without loading external scripts. `weaponProgression` reacts to `enemyKilled`, selects a registered weapon by stable content ID using `random` or `ordered` selection, and may replace the player's existing guns while preserving non-weapon inventory. A profile may also declare the weapon used when entering a combat stage.

Valid loaded rule profiles appear under **Options > Game mode**. Only the selected profile is active, the choice persists by stable content ID, and Standard is restored if the selected pack becomes unavailable. `ExampleMods/apothem-gun-game` reproduces Gun Game, while `ExampleMods/survival-sprint-mode` combines general spawn, wave, economy, player, scoring, and match-goal rules.

The second module is `spawnModifiers`. It composes bounded multipliers and additive offsets for wave population and spawner pacing:

```json
{
  "type": "spawnModifiers",
  "waveSpawnMultiplier": 1.25,
  "waveSpawnAdd": 2,
  "maxStageEnemiesMultiplier": 1.5,
  "maxRoomEnemiesAdd": 5,
  "spawnerChanceMultiplier": 1,
  "spawnDelayMultiplier": 0.75,
  "initialSpawnDelayMultiplier": 0.5
}
```

All fields are optional, but at least one must be present. Multipliers and additive values are bounded by the schema. Experimental `playerRules` also cover maximum hearts and arousal, retained climax buffs, clothing/strength damage, forced automatic fire, reload-on-empty, weapon range, and camera framing. `consumableOverride` can alter only the six reviewed Core drugs; it cannot invoke arbitrary item code. `eventReward` responds only to published gameplay events and `experimentScaling` applies bounded percentage modifiers from completed Core experiments. This is the portable replacement for legacy DLLs that patched `Player`, `Gun`, consumables, or cameras directly. A selected profile may combine these with scoring and match goals, but multiple installed profiles do not apply simultaneously.

### Stage definition draft

The template-backed stage format separates encounter composition and level identity from Unity prefabs. It can inherit a packaged Core layout, while the experimental `tiledJson` layout type can instead author geometry, presentation, navigation, audio, and reviewed gameplay objects on a neutral shell.

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
  "audio": {
    "ambience": "assets/audio/ambience.ogg",
    "entryMusic": "assets/audio/entry.wav",
    "waveMusic": "assets/audio/wave.ogg",
    "waveComplete": "assets/audio/wave-complete.wav"
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

V1 `extends` references a non-hub Core stage. A fully Tiled-authored stage may instead use the reserved `core:stage/mod-template`, which supplies only the neutral runtime shell (wave manager, actor/item roots, spawn waypoint, navigation root, and global light) and does not inherit a playable Core map. Positions use stage-local Unity units; coordinates must be finite and between -100,000 and 100,000. A stage has 1–256 uniquely named spawners; each contains 1–64 unique `enemy/...` content IDs. Enemy references are resolved only against enabled Core content and loaded mod dependencies. Selection weights are relative values from 0.001–1,000. `minimumWave` is 0–10,000, delays are bounded to 0–300 seconds, active spawn delays must be at least 0.02 seconds, and jitter cannot exceed its base delay. Experimental `enabled`, `requiredOpenDoors`, and `requiredClosedDoors` fields allow stage logic and named Tiled door states to gate allocation and queued spawning. `firstWaveEnemyCount` is 1–1,000.

At runtime, the loader clones either the selected Core stage or the neutral shell, installs the JSON spawners, changes the player start and first-wave count, and adds a separate Locations entry. Core-backed stages keep their inherited layout systems; neutral-shell stages receive only their Tiled-authored layout and shared object-library templates. The optional `audio` object's `ambience`, `entryMusic`, `waveMusic`, and `waveComplete` fields accept safe pack-relative WAV/OGG paths; omitted slots retain an inherited Core clip only on Core-backed stages and are silent on the neutral shell. High scores are stored under the full stage content ID; temporary negative runtime numbers are never used as persistent identity. `ExampleMods/training-yard-stage` is a disabled Core-backed working example. Its provisional spawn coordinates still require a hands-on gameplay pass.

The authored-layout preview uses Tiled JSON exported from an orthogonal, finite map. A stage selects it with:

```json
"layout": {
  "type": "tiledJson",
  "path": "levels/training-yard.json",
  "pixelsPerUnit": 32
}
```

Gameplay objects are placed in Tiled object layers. Rectangle objects with type `platform` become collision platforms. Exactly one point object with type `player-spawn` defines the player start. Point objects with type `enemy-spawner` must have unique names matching the stage definition's spawner IDs. Tiled uses a top-left, downward-positive pixel coordinate system; the loader centers the map horizontally and converts it to upward-positive Unity units using `pixelsPerUnit`.

The authored-layout pipeline supports PNG tile and image layers, animated tiles, decorations, curated Core art and presentation props, pack-local ambience and positional audio, explicit stage music, platforms and moving platforms, particles, pickups, named spawners, navigation links, vendors, cases, doors, scripted actors, notes, keypads, altars, lights, and validated interaction/action graphs. Doors can preserve price, single-use and initial-interaction state, a required stage-item ID, and named Core open/closed sprites. A `scripted-actor` point references an enemy content ID and can begin dormant for a later `spawn-actor` action. A `core-prop` point references a stable `core:stage-prop/...` ID for complex shipped presentation hierarchies such as articulated displays, masks, Animator-driven scenery, and coupled light/particle effects; the Core adapter owns the version-specific prefab path rather than the external mod. Reusable interactions cover machines, switches, ordered Easter-egg steps, and shoot/touch/use triggers. The format remains experimental: explicit navigation, audio, animation, large maps, and stage reopening still require broader gameplay testing. See `ExampleMods/tiled-training-yard` and `ModSDK/MapTemplates/TiledStage`.

Experimental `stageScript` documents target a stage by stable content ID. The reviewed runtime supports stage, wave, signal, interaction, player-enter/player-exit touch-box, timer, door-state, object-state, enemy-count, and manual triggers; numeric variables; wave, filtered-enemy, door, and object-state selectors; world-state and synchronization waits; local sequence composition; and bounded notification, variable, signal, object activation/motion, door, light, spawner and interaction enable/disable, queued-spawn, player teleport, camera, global-light, audio, particle, and validated Animator-state actions. Scripts cannot name Unity classes or invoke arbitrary methods. The FER Tiled reference now expresses its fuse laboratory activation, completion cue, delayed Jacky-room curse, display poses, and encounter activation through this format while retaining inherited objects as compatibility fallbacks. The format remains experimental until additional stage sequences establish the required vocabulary.

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

Required dependencies are loaded before the dependent pack. Missing, incompatible, conflicting, invalid, or user-disabled required dependencies disable the dependent pack. Optional dependencies load first when present and compatible, but do not disable their consumer when unavailable.

Load order is resolved from required and compatible optional dependencies, `loadAfter`/`loadBefore` edges, numeric priority, and finally pack ID as the deterministic tie breaker. Patches that target the same field or asset slot are conflicts unless the later pack explicitly declares that public slot in `overrides`. V1 reports unresolved conflicts and lets the player change pack enable state from the Mods panel for the next restart. Filesystem enumeration order never decides the winner.

## Core content

Packaged Core definitions use the same validation pipeline as external data. Core weapon migration uses a temporary `coreWeapon` hybrid definition: the JSON owns the stable ID, legacy prefab selector, display metadata, economy fields, and supported weapon statistics, while the adapter supplies Unity-specific artwork, audio, animation, colliders, and components. All 22 vanilla guns use this hybrid format. Once every weapon shape has published slots and behavior modules, the adapter format can be folded into the general weapon schema without breaking stable content IDs.

Core enemy migration uses the corresponding temporary `coreEnemy` hybrid definition. It owns the stable ID, legacy numeric selector, display name, base actor statistics, bounty, per-wave health growth, vision range, and wave participation. Omitted descriptions and prefab-owned attack, drop, animation, visual, and specialized AI data remain unchanged. All 21 vanilla enemies use this hybrid format.

External enemies may omit `extends` and use `originalSkeletonAtlas`. This standalone path constructs a bounded custom bone hierarchy, atlas sprites, box/circle hit zones, body collision, procedural named animation clips, melee, hitscan, physical-projectile, area, grab, and ordered multi-stage attacks, the reviewed `groundChase` AI, and an optional data-driven finisher QTE without instantiating a Core enemy prefab. Animation clips may be split into normalized `enemyAnimation` documents referenced by semantic name; the runtime applies transform, atlas or pack-local PNG sprite, color, sorting, safe-event, and bounded particle tracks. The normalized enemy rig and animation document shapes are frozen at schema version 1; additions require optional fields or a future schema version. Inherited `coreRigAtlas` enemies retain their existing contract.

Core item migration uses `coreItem` definitions for the 10 vanilla usables and Ammo Box. These definitions own identity, economy, equip timing, marketability, and effect-description text. Aspirin, Morphine, and Hyper additionally own their reviewed effect-module lists; other concrete medicine and ammunition effects remain supplied by bespoke prefab logic. Sprites, sounds, animations, and component types remain supplied by the prefab adapter.

Core stage migration uses `coreStage` definitions for all seven vanilla stages. These definitions own identity, display metadata, and the initial wave enemy count. Existing prefabs continue to supply layout geometry, navigation, spawners, ambience and wave audio, actors and items, and specialized Hub, Jungle, and F.E.R. scripting during the hybrid migration.

Core clothing migration uses a `coreClothingCatalog` document containing all 98 vanilla clothing records. Each record owns its stable ID, legacy numeric adapter selector, and equip category. Existing clothing objects continue to supply icons, rigged sprite pieces, category incompatibilities, and explicit compatible/incompatible clothing relationships during the hybrid migration.

Core challenge migration uses a `coreChallengeCatalog` document containing all 79 vanilla challenge records. Each record owns its stable ID, legacy numeric adapter selector, and a validated objective-adapter kind. Existing challenge components continue to supply objective parameters, display text, stage and reward references, completion flags, and specialized tracking behavior during the hybrid migration.

Core uses the same logical registry and IDs, but may use Unity prefab adapters and packaged asset references while migration is incomplete. External packs cannot disable the Core pack.

## Error isolation

- Invalid external pack: disable that pack and dependent packs, then continue.
- Omitted optional asset field: retain the documented inherited or previously resolved asset.
- Explicit asset path that is missing, unsafe, empty, oversized, or undecodable: reject that definition and report an error.
- Invalid explicit replacement: retain the previous resolved asset and report the error.
- Invalid Core definition: stop startup with a clear fatal report.

## Save compatibility

Save data uses full content IDs for modded content. Existing vanilla numeric IDs are preserved and mapped to stable `core:` IDs. Disabled or missing content remains in storage but is not instantiated.

## Security boundary

Mod API v1 is data-only. JSON values select supported engine behaviours; they do not execute expressions, reflection targets, shell commands, native libraries or arbitrary managed code.
