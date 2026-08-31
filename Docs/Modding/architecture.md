# Data-driven content architecture

## Goals

1. Add a supported enemy variant with one sprite sheet and one JSON definition, without rebuilding the game.
2. Treat shipped content as the required `core` content pack.
3. Permit additive content and explicit asset replacement without matching Unity filenames or GUIDs.
4. Keep old saves usable and preserve progress belonging to temporarily disabled mods.
5. Skip a broken external mod with a useful diagnostic instead of preventing the game from starting.
6. Migrate content incrementally while existing Unity prefabs continue working.

## Non-goals for Mod API v1

- Loading arbitrary C# assemblies.
- Replacing engine systems such as saves, input, physics or UI code.
- Creating behaviour that the engine does not expose as a supported module.
- External mods on Android or WebGL. Core must still load on every supported platform.
- Immediately converting every existing prefab into loose JSON and PNG files.

## Runtime layers

```text
Game engine
  Mod bootstrap and validation
  Content registry
  Asset resolver
  Runtime factories and prefab adapters
  Save-key translation
  UI, input, physics and reusable behaviours

Content
  core (required and read-only)
  external pack A
  external pack B
```

The registry resolves namespaced content IDs. The asset resolver resolves stable asset-slot IDs. Factories create fully data-driven content; adapters expose existing Unity prefabs through the same registry during migration.

## Load pipeline

1. Register the packaged Core manifest.
2. Discover enabled external manifests on supported desktop builds.
3. Validate manifest schema, IDs, paths and declared Mod API version.
4. Resolve required dependencies and deterministic load order.
5. Parse content definitions without creating Unity objects.
6. Validate references against the combined registry.
7. Register additive content.
8. Resolve explicit patches and report conflicts.
9. Build or bind runtime assets.
10. Expose the completed registry to libraries, menus, stages and the save system.

Runtime templates and their instantiated clones carry the same stable content identity component. Gameplay systems can therefore resolve `example.pack:enemy/name` or `core:clothing/name` directly instead of treating mutable Unity names or load-order-dependent numeric IDs as persistent keys.

An external pack that fails steps 3 through 8 is disabled for that session. Core failure is fatal and must produce a clear error because the game cannot run without it.

## Core migration

Migration is content-type-by-content-type:

1. Assign stable IDs to existing content.
2. Register existing prefabs through adapters, with no gameplay change.
3. Move simple metadata and statistics into Core definitions.
4. Route replaceable art and audio through stable asset slots.
5. Introduce runtime factories for content that can be created safely from data.
6. Retain prefab-backed Core entries for content that still needs bespoke Unity setup.

For example, `core:stage/field-day` may remain prefab-backed while a future external stage uses a supported level format. Both are still discovered through the stage registry and presented identically to menus and saves.

## Asset strategy

Public asset IDs are independent of source filenames:

```text
Public slot: core:player/body/head/pale
Unity source: Assets/.../an-existing-file-name.png
```

Core mappings can initially be generated or stored as Unity references. External assets use paths relative to their own pack. A mod may only replace a Core asset by declaring the target slot explicitly.

Sprite definitions must eventually include enough import data to reproduce Unity settings at runtime: frame dimensions, pixels per unit, pivot, filter mode and animation frame order.

## Enemy strategy

Current humanoid enemies are not single sprites. For example, the Gremlin prefab contains a multi-part rig with 20 sprite renderers and 19 rigid bodies/colliders. A flat frame-animation sheet cannot transparently replace that hierarchy.

The recommended first enemy implementation therefore extends a Core rig template and treats the supplied sprite sheet as an atlas of named body-part regions. This reuses the existing behaviour, hierarchy, colliders, inherited animations and interactions while replacing approved data and visual slots. It still satisfies the packaging goal of one PNG atlas plus one enemy JSON definition.

A later `frameAnimation` visual template can support simple enemies drawn as whole-character frames. It will have documented limitations until features such as per-body-part hits, ragdolls and special interactions are represented by that template.

Later enemy factories should compose supported modules:

- Movement: walker, flier, crawler, stationary.
- Targeting: chase, flee, keep-distance, ambush.
- Attacks: melee, projectile, leap, charge and grab.
- Effects: slow, webbed, hypnotized and knockback.
- Classification: normal, elite, miniboss and boss.

Content cannot request a module that the installed Mod API does not expose. Bespoke Core behaviour can be standardized into a module later.

## Saves

Existing numeric IDs remain valid for old saves. The migration adds stable content keys rather than renumbering old rows.

Required behaviour:

- Vanilla records map to `core:` keys.
- Mod records use their full namespaced ID.
- Removing or disabling a mod does not delete its records.
- Re-enabling the same mod reconnects its records.
- Missing definitions are not displayed as usable content but remain recoverable.
- Renaming a public ID requires an explicit alias/migration entry.

## Platform policy

- Windows desktop: packaged Core plus external content folders.
- Android and WebGL: packaged Core through the same registry, with external folder discovery disabled initially.
- A later distribution service such as mod.io is separate from the file format and registry. Downloading a pack must not change how the pack is parsed.

## Implementation phases

1. Specification, schemas and representative examples.
2. `ContentId`, manifest parser, validation report and registry unit tests.
3. Core adapters for existing enemy, stage, clothing, item and challenge libraries.
4. Save-key migration and missing-content handling.
5. Asset resolver and Core asset-slot catalog.
6. External enemy variant vertical slice using an inherited Core rig and a body-part atlas.
7. Clothing, cosmetic and item factories.
8. Rule profiles for difficulties, spawn modifiers and game modes.
9. Prefab-backed stage definitions followed by an external level format.
10. Mods menu, conflict resolution, SDK, validator and sample packs.

Each phase must leave normal Core-only gameplay working before the next phase begins.
