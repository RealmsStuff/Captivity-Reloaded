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

The adapter currently locates those renderers inside the packaged Pistol prefab. That lookup is private to Core and can change without breaking a mod that uses the public slot IDs.

External PNG decoding and JSON patch discovery are the next layer. The registry and runtime bindings deliberately land first so file-loaded assets pass through validation and deterministic conflict handling instead of directly mutating renderers.
