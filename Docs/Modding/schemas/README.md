# Mod API v1 JSON Schemas

These Draft 2020-12 schemas describe every public Mod API v1 document:

- `manifest.schema.json`
- `asset-patch.schema.json`
- `challenge.schema.json`
- `clothing.schema.json`
- `difficulty.schema.json`
- `enemy.schema.json`
- `enemy-animation.schema.json`
- `enemy-rig-reference.schema.json`
- `player-attachment.schema.json`
- `rule-profile.schema.json`
- `stage.schema.json`
- `tiled-map.schema.json`
- `tiled-tileset.schema.json`
- `usable.schema.json`
- `weapon.schema.json`

The normalized editor/mod-maker contracts are also described here:

- `player-rig.schema.json`
- `player-animation.schema.json`

`enemy-rig-reference.schema.json` records the semantic bone map, default transforms, sprite regions, pivots, and sorting values extracted from every Core enemy prefab. Its version 1 shape is frozen as an editor reference and round-trip diagnostic contract; it is not a runtime pack content type.

They normalize Unity's actor hierarchies and clips for tooling. `playerAnimation` documents are referenced by finishers through `playerAnimationRef`; `enemyAnimation` documents are assigned semantic clip names through an enemy's `animationRefs`. Runtime playback applies bounded transforms, atlas-region sprite swaps, color, sorting order, safe events, and reviewed particle definitions. Enemy rig and animation schema version 1 is frozen; player-side presentation fields retain their separately annotated stability.

The custom `x-stability` annotation is either `stable` or `experimental`. Stable fields are compatibility candidates for the v1 freeze. Experimental fields remain supported but may change before promotion. Runtime validation remains authoritative where a rule depends on the selected Core template, another content ID, Tiled template expansion, or relationships between several files.

All schemas use `additionalProperties: false` for loader-owned content documents because the runtime parser rejects unknown fields. The Tiled schema deliberately remains open at Tiled-owned object and layer boundaries so ordinary editor metadata is not rejected.

Run `dotnet run --project Tools/ModSchemaValidator -- .` from the repository root to validate every public example manifest, content document, Tiled map, and external JSON/TSJ tileset. Dependencies restore under the repository's `Temp` directory.

Pass a pack directory after the repository path to validate an installed or in-progress pack directly:

```powershell
dotnet run --project Tools/ModSchemaValidator -- . Mods/tiled-starter-kit
```
