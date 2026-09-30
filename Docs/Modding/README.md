# Captivity Reloaded modding specification

This directory defines the draft contract for the built-in, data-driven content system. The runtime validates IDs and manifests, discovers desktop packs, resolves required and optional dependencies, tracks disabled/conflicting/invalid packs, loads the packaged Core catalog, applies explicit asset patches, and constructs inherited enemies, clothing, weapons, usables, stages, challenges, difficulties, and game modes. The title-screen Mods panel persists enable/disable choices for the next restart.

The long-term model separates the game into two layers:

- The engine owns systems such as input, saves, UI, physics, navigation, reusable AI behaviours, rendering and content loading.
- Content packs currently provide enemies, stages, clothing, persistent player attachments, items, broad challenge objectives, difficulty profiles, selectable bounded game modes, and assets through stable public IDs.

The shipped game is represented by the required, read-only `core` content pack. External mods use the same registry and schemas as Core content. Core difficulties are now packaged JSON definitions; existing enemies, stages, clothing, and items remain backed by Unity prefabs during incremental migration.

## Documents

- [Architecture](architecture.md) describes the migration strategy and implementation phases.
- [Draft v1 contract](spec-v1.md) defines IDs, manifests, folders, dependencies, overrides and compatibility rules.
- [JSON Schemas](schemas) provide validation and autocomplete for every public v1 content contract, with machine-readable stable and experimental field classifications.
- [V1 design decisions](decisions.md) records the scope used by the draft implementation and still subject to the schema freeze review.
- [GitHub mod browser plan](github-mod-browser-plan.md) tracks the implemented browser, dependency installs, updates, rollback/recovery, remote previews, and remaining player-build verification.
- [Legacy `.capmod` conversion inventory](../Planning/legacy-capmod-conversion-inventory.md) maps every archived community release to its maintained source, duplicates, composition archives, parity gaps, and packaging status.
- [Enemy variant example](examples/acid-gremlin) demonstrates the target of one sprite sheet and one enemy JSON file.
- [Asset replacement example](examples/shaded-girl) demonstrates explicit player sprite-slot replacement.
- `ExampleMods/prey-bunny-girls` is the faithful 154-texture conversion of the legacy Prey/Bunny Girls overhaul; `prey-green-zombie` remains a separate enemy-authoring demonstration derived from one of its ideas.
- `ExampleMods/femboy-refitted-shirt` demonstrates an additive Core-template garment plus data-only artwork patches for 26 existing Core garments.
- `ExampleMods/additive-nerf-pistol` converts legacy replacement artwork into a separate Core-template weapon while preserving the original pistol.
- `ExampleMods/goblin-slayer-armor` faithfully replaces the four Jungle-earned Core Knight garments with legacy Goblin Slayer artwork.
- `ExampleMods/cod-wonderweapon` faithfully replaces the Core Schockgewehr sprite without changing its FER encounter or gameplay.
- `ExampleMods/raygun-revolver` faithfully replaces the equipped Core .44 Revolver base sprite without changing weapon behavior.
- `ExampleMods/start-with-no-gun` recreates the.apothem's no-starter-pistol and $500 game mode.
- `ExampleMods/small-tweaks` preserves the nine artwork replacements from the legacy Small Tweaks archive.
- `ExampleMods/cry-when-raped` preserves CAR's single crying face-layer replacement.
- `ExampleMods/smiling-blush-small-tweaks` layers the twelve legacy smiling-mouth replacements over `small-tweaks`.
- `ExampleMods/training-yard-stage` demonstrates a separate template-backed stage with JSON-defined waves and spawners.
- `ExampleMods/apothem-gun-game` recreates the legacy Gun Game mod with a data-driven kill-triggered weapon-progression rule.
- `ExampleMods/survival-sprint-mode` demonstrates spawn, wave, economy, and player rule modules.
- `ExampleMods/challenge-objectives-showcase` exercises general data-driven challenge objectives.
- `ExampleMods/tiled-training-yard` and `ModSDK/MapTemplates/TiledStage` demonstrate the experimental Tiled map pipeline.
- `ExampleMods/weapon-behavior-showcase` exercises experimental charge, beam, alternate-fire, melee, and sprite-animation modules.
- `ExampleMods/animation-reference-gallery` provides six non-spawning reference enemies, 72 portable Core-derived animation documents, and rig maps for zombie, quadruped hound, fly, maggot, musca, and gremlin body plans.

## Status

Specification status: **Draft 0.9, freeze review**

Runtime status: **The end-to-end data-only loader and all planned v1 content categories are represented. Every public example passes its structural schema, while several newer runtime modules still require gameplay verification before release.**

See the [public implementation status](Public/reference/status.md) and [v1 release checklist](Public/reference/v1-release-checklist.md) for the maintained capability and release-readiness summaries.

The schemas and examples may change before the first public Mod API release. Once Mod API v1 is released, existing public IDs and v1 fields should remain compatible for the lifetime of v1.

## Unity mod packager

Use **Captivity Reloaded > Modding > Create Mod...** to create a new loose mod folder, manifest, `content` and `assets` directories, and a matching Unity authoring profile. The wizard does not overwrite an existing mod folder. A data-only mod can be packaged immediately after adding valid content; Unity bundle builds need at least one descriptor-marked prefab.

Use **Captivity Reloaded > Modding > Create Original Enemy...** after creating the mod. Choose a humanoid, quadruped, flying, crawler, or blank topology; the enemy wizard creates a required atlas placeholder and region guide, a complete original rig with hit zones, idle/move/attack animation documents, and an optional one-enemy Field-Day test stage. Its Movement AI section provides runtime-backed ground assault, ground skirmisher, flying assault, and stationary guard presets, with controls for pursuit distance, retreat distance, reaction time, vision, acceleration, speed, and traction. The Attack Strategy section creates one or more weighted melee, hitscan, physical-projectile, area, charging, grab, or ordered multi-stage attacks and generates a matching animation document for every attack. Projectile strategies also add a dedicated projectile cell to the generated atlas. Balance controls cover health, bounty, wave scaling and spawn weight; behavior authoring covers regeneration, berserk, lifesteal, thorns, on-hit ragdoll, spawn-on-death and speed-pulse modules; and the drop editor accepts stable item content IDs. The grab preset also creates a paired DragonBones draft and connects it to a playable starter finisher that can be refined through the Paired Interaction tab. Movement AI and attack ranges are cross-validated so a pursuing enemy cannot stop outside every attack's range. The wizard refuses to replace generated files and immediately runs the normal `.capmod` validator. Replace the colored atlas cells with transparent artwork, adjust the generated bone pivots and hit zones, then refine the generated animation documents through the DragonBones bridge and the Unity animation/VFX preview.

Generated test stages now opt into an isolated **Mod Finisher Test** panel. In Play Mode it can spawn the authored enemy, knock down or expose the player, begin the real runtime finisher immediately, reset both actors without reloading the stage, and display the active phase, enemy/player clips, struggle meter, timer, and last outcome. The panel is absent from ordinary authored stages unless their experimental `testTools` block explicitly enables it.

The same window's **Import DragonBones** tab provides the safe return path. It diagnoses the edited armature, converts into temporary files, resolves destinations by content ID, validates a staged copy of the entire loose mod, and backs up every original animation outside the mod before replacement. Paired enemy/player sidecars are handled together and a failed live validation is rolled back automatically.

After importing a pair, use **Paired interaction** in the same window. It discovers only enemy animations already connected through that enemy's `animationRefs`, pairs them with pack-local player animations, and creates either a single continuous interaction or two named phases. Trigger distance, delay, struggle meter, input strength, decay, cooldown, and success/failure outcomes are exposed without manually editing JSON. Existing non-finisher behavior modules are preserved; an older `downedFinisher` is replaced after staging and validating the complete mod, with its enemy definition backed up under `ModAuthoringBackups`. **Open Pair in Preview** selects both documents in the existing animation/VFX preview.

For modded-enemy animation and effects, open **Captivity Reloaded > Modding > Player Animation Preview**, choose **Modded enemies**, and select the mod folder. The window discovers its enemy and `enemyAnimation` documents and groups animations by their target enemy. Existing motion data remains the source of animation; the editor is no longer intended to replace a dedicated animation package.

In **Modded enemies**, the Animation Source card shows the current enemy and clip. Press **Change** to open inline Enemy and Animation lists; these deliberately use scrollable buttons instead of floating dropdowns, so source selection cannot cover the editor controls. Catalog/export commands live in the collapsed tools section rather than occupying the animation workspace.

Switch to **VFX Timeline** to add presentation to the selected animation. The Effect Library creates, duplicates, and removes reusable particle, 2D light, motion-trail, and sprite-decal definitions. Effects may select a pack-local PNG, divide it into a bounded grid, and preview one selected region without importing the image into Unity. Scrub the dedicated Particle, Audio, Hitbox, Camera, Light, Trail, and Decal lanes to create frame-snapped markers. Audio, hitbox, and camera markers use the existing safe `sound`, `attackHit`, and `cameraShake` runtime events; the other lanes use the bounded effect-trigger scheduler. One definition may be triggered more than once, and deleting it removes orphaned triggers. **Copy** and **Paste** move portable effect JSON between clips, while **Starter preset** provides impact, smoke, sparks, glow, trail, and decal starting points. **Save JSON** validates the same portable data consumed on Windows, Android, and WebGL.

The preview validates animation references against the loaded enemy before saving. Missing rig bones, atlas regions, sound files, effect textures, and effect IDs are listed with their exact names. **Fix safe references** can retarget missing bones and regions and remove dangling trigger IDs; missing files are left for the author to replace through **Choose pack audio** or **Choose texture**, because the editor cannot safely guess the intended asset.

Gameplay callbacks remain in the safe event editor. Use those for hits, impulses, sounds, camera shake, sprite effects, and inert cues; use VFX Timeline for particle presentation. External packs still cannot import arbitrary Unity animation events or scripts.

The Unity Editor now provides **Captivity Reloaded > Modding > Captivity Mod Packager**. The initial packager slice validates an existing loose JSON/Tiled mod and exports a single `.capmod` archive. The archive is a deterministic ZIP container with a root `manifest.json`, so the existing catalog integrity, staged installation, recovery, and rollback pipeline remains usable.

The first engine-authored slice is also available. Create a **Captivity Reloaded > Mod Authoring Profile** asset, point it at a loose mod folder, assign prefab assets carrying a root `CapmodPrefabDescriptor`, select the target platforms, and use **Build Unity .capmod**. The packager builds LZ4 AssetBundles for Windows, Android, and WebGL, records their individual sizes and SHA-256 hashes in the generated manifest, and places them inside the same `.capmod` file. The source manifest must not declare `assetBundles`; that metadata is generated from the actual build output.

This initial prefab surface deliberately permits only Unity-native components plus `CapmodPrefabDescriptor`. New compiled gameplay behaviours will be added to the approved SDK surface incrementally. Loose folder mods remain supported for development and backward compatibility.
