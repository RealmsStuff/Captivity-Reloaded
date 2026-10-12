# Dependencies and load order

Required dependencies load first. A missing, incompatible, invalid, conflicting, or disabled required dependency disables the dependent pack with an explanation.

Optional dependencies affect ordering only when a compatible enabled pack is present. Their absence does not disable the consumer.

`loadAfter` and `loadBefore` are soft ordering hints. Explicit `conflicts` and `overrides` should be used when two packs cannot safely coexist or intentionally target the same public slot.

## Required and optional dependencies

```json
"dependencies": [
  { "id": "core", "version": ">=0.1.0" },
  { "id": "example.shared-library", "version": ">=1.2.0" }
],
"optionalDependencies": [
  { "id": "example.extra-art", "version": ">=1.0.0" }
]
```

Required dependencies are part of the pack's validity. Optional dependencies may enable compatibility or ordering but must have a safe absence path.

## Deterministic ordering

The resolver combines:

1. required dependency edges;
2. compatible optional dependency edges;
3. `loadAfter` and `loadBefore` hints;
4. numeric `priority`;
5. pack ID as the final deterministic tie breaker.

Filesystem enumeration order is never an API. Do not rely on folder names to decide which pack loads last.

## Conflicts and overrides

Use `conflicts` when two complete packs cannot safely run together. Use `overrides` for each exact content or asset-slot ID a pack intentionally replaces. An override declaration is authorization, not a wildcard and not proof that the replacement itself is valid.

```json
"conflicts": ["example.incompatible-pack"],
"overrides": ["core:weapon/pistol/body"]
```

If multiple enabled packs replace the same slot without a permitted resolution, the loader reports a conflict and preserves a deterministic safe baseline.

## Catalog installation

The catalog installer resolves compatible required dependencies before downloading and displays the full transaction. Optional dependencies are not installed automatically. Catalog dependency and conflict metadata must match the downloaded release manifest exactly.

See [Installing mods](../getting-started/installing-mods.md) for player behavior and [Publishing to the community catalog](../getting-started/publishing-to-catalog.md) for release metadata.
