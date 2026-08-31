# Public asset slots

Asset slots are stable names for replaceable Unity assets. Mods target these IDs explicitly; Unity filenames, GUIDs, hierarchy names and bundle object names are implementation details and are not part of the public API.

## Resolution rules

- Every slot has a Core or content-pack baseline asset and an expected Unity asset type.
- A replacement applies only when its patch ID belongs to the declaring pack, its target slot exists, and its asset type matches.
- If multiple enabled patches target one slot, the baseline remains active and the loader reports `asset-patch.conflict`.
- Missing targets and invalid replacements also retain the baseline.
- Resolution starts from the baseline each time, so disabling a mod restores the original asset.

## Core weapon slots

The first Core adapter publishes the three visual pieces changed by the legacy Simple Nerf Gun mod:

| Public slot | Type | Meaning |
| --- | --- | --- |
| `core:weapon/pistol/body` | `Sprite` | Main pistol body |
| `core:weapon/pistol/slide` | `Sprite` | Moving upper slide |
| `core:weapon/pistol/base` | `Sprite` | Lower grip/base |

## Core player body slots

Naked player artwork uses `core:player/body/<part>/<skin>`. Supported parts are `torso-lower`, `butt`, `hips`, `chest`, `neck`, `head`, `ear`, `arm-upper`, `arm-lower`, `hand`, `leg-upper`, `leg-lower`, and `foot`. Supported skins are `pale`, `white`, `tan`, and `black`, producing 52 stable slots.

Left and right limbs intentionally share a slot because Core uses the same source sprite on both sides. The runtime binding updates the serialized skin variants as well as the currently visible renderer, so replacements survive skin changes and apply to gameplay and wardrobe instances loaded later.

The adapter currently locates those renderers inside the packaged Pistol prefab. That lookup is private to Core and can change without breaking a mod that uses the public slot IDs.

## External patch example

Place a patch definition under one of the pack's declared `contentRoots` and keep its PNGs inside the same pack:

```json
{
  "schemaVersion": 1,
  "type": "assetPatch",
  "id": "example.nerf:patch/starter-pistol",
  "target": "core:weapon/pistol",
  "replacements": {
    "body": "assets/pistol/body.png",
    "slide": "assets/pistol/slide.png",
    "base": "assets/pistol/base.png"
  }
}
```

On Windows, the loader discovers these definitions recursively, accepts PNG files up to 32 MiB, and creates runtime sprites using the Core sprite's pixels-per-unit, normalized pivot, border, filter mode and source rectangle when it fits the replacement texture. This lets same-sized legacy texture swaps keep their original alignment. Paths are resolved within the defining pack; absolute paths and traversal outside it are rejected.
