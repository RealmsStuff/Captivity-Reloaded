# Asset patches

Use an `assetPatch` when the goal is to replace published artwork or audio without creating new gameplay content. A patch targets a stable content ID and maps its public slot names to pack-local files.

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
- reviewed interface and presentation assets.

The internal `Docs/Modding/asset-slots.md` inventory and the manifests of maintained conversion packs show the currently registered IDs. Runtime lookup uses these public IDs, never Unity GUIDs or private hierarchy paths.

## Resolution rules

- A replacement applies only when the target and slot exist, the file is valid, and the manifest authorizes the exact slot ID.
- A missing or invalid replacement leaves the previously resolved asset active.
- Two enabled packs targeting the same slot conflict unless the later pack is explicitly authorized under the load-order rules.
- Disabling a patch restores the baseline asset when the title scene reloads.
- Filename case and `/` separators must be portable across supported platforms.

## When not to use a patch

Use an additive `weapon`, `clothing`, `enemy`, or other content definition when the original and new content should coexist. Use a rule profile when behavior changes. Asset patches should not silently carry gameplay changes.

## Examples

- `ExampleMods/simple-nerf-gun`: three-piece pistol artwork.
- `ExampleMods/cod-wonderweapon`: one Core weapon presentation replacement.
- `ExampleMods/goblin-slayer-armor`: several Core clothing replacements.
- `ExampleMods/shaded-girl`: player artwork slots.
- `ExampleMods/futazombies`: Core enemy artwork slots.

See [Your first pack](../getting-started/first-pack.md) for a complete beginner workflow and `asset-patch.schema.json` in the [schema index](../reference/schemas.md) for the exact contract.
