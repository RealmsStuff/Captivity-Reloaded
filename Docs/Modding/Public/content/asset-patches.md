# Asset patches

Use an `assetPatch` when the goal is to replace published PNG artwork without creating new gameplay content. A patch targets a stable content ID and maps its public slot names to pack-local PNG files. Mod API v1 does not expose Core audio through `assetPatch`; pack-local audio remains available on content types that explicitly declare audio fields.

## Minimal example

```json
{
  "schemaVersion": 1,
  "type": "assetPatch",
  "id": "yourname.my-pack:patch/starter-pistol",
  "target": "core:weapon/pistol",
  "replacements": {
    "body": "assets/pistol.png",
    "slide": "assets/slide.png",
    "base": "assets/base.png"
  }
}
```

The manifest must authorize replacement of each full slot ID:

```json
"overrides": [
  "core:weapon/pistol/body",
  "core:weapon/pistol/slide",
  "core:weapon/pistol/base"
]
```

This explicit declaration makes replacements reviewable and prevents matching filenames from changing unrelated content.

## Published slot families

The current Core adapter publishes slots for:

- weapon pieces and inventory sprites;
- player body and face artwork by semantic part and skin;
- clothing icons and semantic rig pieces;
- selected map machines and props;
- Core enemy body, rig, and anatomy artwork;
- reviewed interface, environment, effect, and presentation assets.

The [public asset-slot catalog](../reference/asset-slots.md) lists the exact IDs. Runtime lookup uses these public IDs, never Unity GUIDs or private hierarchy paths. Legacy-presentation slots are a compatibility layer over reviewed Core sprite names; prefer player, clothing, weapon, enemy, and stage semantic slots when both forms exist.

Every legacy-presentation slot requires its Core baseline to be loaded. The game log reports `Core legacy visual slots=<registered>/<advertised>` after binding and lists any advertised names unavailable in the current scene. A temporarily unavailable baseline is retried after scene loads and after relevant Core objects start.

## Resolution rules

- A replacement applies only when the target and slot exist, the file is valid, and the manifest authorizes the exact slot ID.
- A missing or invalid replacement leaves the previously resolved asset active.
- Two enabled packs targeting the same slot conflict unless the later pack is explicitly authorized under the load-order rules.
- Disabling a patch restores the baseline asset when the title scene reloads.
- Filename case and `/` separators must be portable across supported platforms.
- A PNG that is entirely black or transparent is still structurally valid. Inspect extracted legacy artwork before packaging; do not assume every changed Unity texture contains authored art.

## When not to use a patch

Use an additive `weapon`, `clothing`, `enemy`, or other content definition when the original and new content should coexist. Use a rule profile when behavior changes. Asset patches should not silently carry gameplay changes.

## Examples

- `ExampleMods/simple-nerf-gun`: three-piece pistol artwork.
- `ExampleMods/cod-wonderweapon`: one Core weapon presentation replacement.
- `ExampleMods/goblin-slayer-armor`: several Core clothing replacements.
- `ExampleMods/shaded-girl`: player artwork slots.
- `ExampleMods/futazombies`: Core enemy artwork slots.

See [Your first pack](../getting-started/first-pack.md) for a complete beginner workflow and `asset-patch.schema.json` in the [schema index](../reference/schemas.md) for the exact contract.
