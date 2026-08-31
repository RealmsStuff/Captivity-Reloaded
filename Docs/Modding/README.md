# Captivity Reloaded modding specification

This directory defines the draft contract for the built-in, data-driven content system. The runtime validates IDs and manifests, discovers desktop packs, resolves dependencies, loads the packaged Core catalog, applies explicit PNG asset patches and constructs atlas-driven enemy variants from Core rigs.

The long-term model separates the game into two layers:

- The engine owns systems such as input, saves, UI, physics, navigation, reusable AI behaviours, rendering and content loading.
- Content packs provide enemies, stages, clothing, items, challenges, game modes and assets through stable public IDs.

The shipped game is represented by the required, read-only `core` content pack. External mods use the same registry and schemas as Core content, even while existing Core content remains backed by Unity prefabs during migration.

## Documents

- [Architecture](architecture.md) describes the migration strategy and implementation phases.
- [Draft v1 contract](spec-v1.md) defines IDs, manifests, folders, dependencies, overrides and compatibility rules.
- [Open design decisions](decisions.md) lists choices that need project-manager approval before runtime code fixes the public contract.
- [Enemy variant example](examples/acid-gremlin) demonstrates the target of one sprite sheet and one enemy JSON file.
- [Asset replacement example](examples/shaded-girl) demonstrates explicit player sprite-slot replacement.
- `ExampleMods/prey-green-zombie` is a working conversion of a legacy asset-bundle zombie overhaul into the atlas-driven enemy format.

## Status

Specification status: **Draft 0.4**

Runtime status: **Foundation, asset replacement, Core-rig enemies and clothing-definition discovery implemented**

The schemas and examples may change before the first public Mod API release. Once Mod API v1 is released, existing public IDs and v1 fields should remain compatible for the lifetime of v1.
