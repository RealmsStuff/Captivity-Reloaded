# Inherited weapon templates

These kits cover three published Core weapon layouts:

| Kit | Extends | Published sprites | Canvas |
| --- | --- | ---: | --- |
| Pistol | `core:item/weapon/pistol` | 4 | 32 by 32 |
| Tenelli SO3 shotgun | `core:item/weapon/tenelli-so3` | 4 | 48 by 32 |
| Revolver .44 | `core:item/weapon/revolver-44` | 10 | 32 by 32 |

Copy `build-weapon-templates.js` to LibreSprite's scripts directory, rescan scripts, and run it. The script creates one layered `.aseprite` document per weapon. All layers intentionally overlap because weapon definitions load one PNG per semantic slot rather than one atlas.

To export an edited part, show only that named layer and export the full, untrimmed canvas over the matching file in `assets/weapon`. Transparent canvas pixels and the original dimensions must be preserved. The `body` slot is the inventory/drop icon; it is not an ejected bullet. Magazine replacements are applied to every matching magazine renderer in the inherited prefab.

Each directory is also a ready-to-copy pack skeleton. Rename `manifest.template.json` to `manifest.json`, then change the namespace, IDs, authors, display text, and statistics. The checked-in PNGs reproduce the Core appearance so placement can be verified before drawing custom art.

Inheritance retains the Core prefab's Animator, colliders, muzzle position, and specialized mechanics unless a reviewed JSON field replaces them. The optional `behavior` block supports burst fire, tracer presentation, pack-local WAV/OGG audio, muzzle sprites, casing sprites/ejection, and physical or explosive sprite projectiles. See `Docs/Modding/Public/content/weapons-and-items.md` for fields and bounds. Arbitrary Unity scripts and custom animation graphs are not loaded.
