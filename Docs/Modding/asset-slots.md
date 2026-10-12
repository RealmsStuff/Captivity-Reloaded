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
| `core:weapon/pistol/body` | `Sprite` | Pistol inventory icon |
| `core:weapon/pistol/slide` | `Sprite` | Moving upper slide |
| `core:weapon/pistol/base` | `Sprite` | Lower grip/base |

## Core player body slots

Naked player artwork uses `core:player/body/<part>/<skin>`. Supported parts are `torso-lower`, `butt`, `hips`, `chest`, `neck`, `head`, `ear`, `arm-upper`, `arm-lower`, `hand`, `leg-upper`, `leg-lower`, and `foot`. The lower eye-cover layer uses `core:player/face/eyelid-lower/<skin>`. Supported skins are `pale`, `white`, `tan`, and `black`, producing 56 stable body/face slots.

Left and right limbs intentionally share a slot because Core uses the same source sprite on both sides. The runtime binding updates the serialized skin variants as well as the currently visible renderer, so replacements survive skin changes and apply to gameplay and wardrobe instances loaded later.

## Core clothing slots

Every reconstructed Core garment publishes its icon and all of its semantic pieces. The slot ID is the garment content ID followed by `icon` or the exported piece slot:

```text
core:clothing/shirt-default/icon
core:clothing/shirt-default/piece/shirt-chest
core:clothing/shirt-default/piece/shirt-spine
```

An `assetPatch` whose target is `core:clothing/shirt-default` can therefore replace the original wardrobe entry in place. The patch keeps Core attachment points, pivots, sorting, tearing, compatibility, unlock state, and save identity. PNGs may be larger than the old 32-by-32 canvas; the source pivot is normalized onto the replacement dimensions so extended artwork is not culled.

All lookups of Core renderers and reconstructed garment pieces are private implementation details. Mods only depend on the public semantic slot IDs.

## Core map-object slots

The usable-item vending machine publishes `core:map-art/usable-vendor/sprite`. Replacing it changes every loaded copy of the machine—including copies instantiated by a Tiled stage—without replacing its catalog, controls, purchase rules, or other `Vendor` behavior. `ExampleMods/cod-perk-machine` demonstrates this compatibility route with legacy artwork.

## Core enemy anatomy slots

The 32 explicit anatomy sprites changed by the legacy CNR/SFW archive are published below `core:enemy-anatomy/sprite/...`. These slots preserve the distinct original sprite canvases and pivots. Runtime substitutions are reapplied after Animator evaluation so animated Core enemies cannot restore an unpatched frame. The converted reference pack is not included in the current `ExampleMods` set.

## Core enemy rig slots

Packaged Core enemies publish semantic rig artwork below their enemy ID. The current body slots are `body/torso-lower`, `body/butt`, `body/hips`, `body/chest`, `body/neck`, `body/head`, `body/arm-upper`, `body/arm-lower`, `body/hand`, `body/leg-upper`, `body/leg-lower`, `body/foot-left`, `body/foot-right`, and `body/penis` when that part exists. For example, Zombie I exposes `core:enemy/zombie-1/body/head`.

Paired limbs share one semantic slot except for feet, whose asymmetric artwork remains independently replaceable. These slots patch the shipped enemy template in place, so clones retain the original AI, stats, colliders, animations, spawner membership, and stable save identity. `ExampleMods/futazombies` demonstrates a complete three-enemy legacy replacement.

## Curated legacy presentation slots

Scene-only actors and props that do not belong to a reusable gameplay template
use explicit `core:sprite/.../image` compatibility slots. The published set is:

```text
arm-lower-2, arm-upper-18, arm-upper-20, blush, butt-15, butt-5, butt-6,
chest-5, chest-7, eyelid, face, foot, hand-16, hand-2, head-8, head1, head2,
head3, head4, hip-3, hip-5, lady-statue, leg-lower-11, leg-lower-15,
leg-upper-1, leg-upper-3, medic-body, neck-5, neck-8, painting, rontgen,
torso-lower-0, torso-lower-8
```

These names are stable IDs, not a promise that arbitrary private sprite names are
patchable. They cover the placed/background characters and scenery changed by
the legacy Prey/Bunny Girls release. Zombie artwork from that release targets
the semantic Core enemy rig slots instead, ensuring later spawned enemies also
receive the replacement.

## FER presentation slots

FER's named character-state and gore sprites are published below `core:stage-art/fer/actor/...` and `core:stage-art/fer/effects/...`. The actor group covers alternate powered-off heads, selected body variants, broken/bloody limbs, and other FER-specific presentation frames. The effect group covers the two large blood pools, three wall splashes, four small splashes, and the long blood platform. General anatomy and already-public placed sprites retain their existing owners instead of receiving duplicate FER aliases. `ExampleMods/tweaked-fer` is the complete reference patch.

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

For an in-place clothing replacement, use the same shape with a clothing target:

```json
{
  "schemaVersion": 1,
  "type": "assetPatch",
  "id": "example.refit:patch/default-shirt",
  "target": "core:clothing/shirt-default",
  "replacements": {
    "piece/shirt-chest": "assets/default-shirt/chest.png",
    "piece/shirt-spine": "assets/default-shirt/spine.png"
  }
}
```

On Windows, the loader discovers these definitions recursively, accepts PNG files up to 32 MiB, and creates runtime sprites using the Core sprite's pixels-per-unit, normalized pivot, border, filter mode and source rectangle when it fits the replacement texture. This lets same-sized legacy texture swaps keep their original alignment. Paths are resolved within the defining pack; absolute paths and traversal outside it are rejected.
