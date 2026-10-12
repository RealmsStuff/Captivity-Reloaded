# Inherited clothing templates

These kits create layered LibreSprite files for three Core clothing layouts:

| Kit | Extends | Piece slots | Canvas |
| --- | --- | ---: | --- |
| Default shirt | `core:clothing/shirt-default` | 2 | 96 by 32 |
| Scientist/lab coat | `core:clothing/scientist-set` | 12 | 128 by 128 |
| Hazmat suit | `core:clothing/hazmat-suit` | 21 | 160 by 160 |

Copy `build-clothing-templates.js` to LibreSprite's scripts directory, rescan scripts, and run it once. It writes each `.aseprite` source into its matching directory and the flattened PNG under that kit's `assets/clothing` folder. When LibreSprite warns that PNG cannot preserve layers, choose **Yes**: the `.aseprite` file retains the editable layers and the PNG is the runtime atlas.

The supplied Core art starts in 32 by 32 cells, but **32 by 32 is not a runtime limit**. The loader creates each piece from the complete PNG or atlas region and uses a full rectangular mesh, so skirts, capes, oversized sleeves, and similar art may extend beyond the old cell without being cropped. In a copied generator descriptor, set `cellWidth` and `cellHeight` (for example, `64` and `64`), enlarge the matching `visual.regions` width/height and coordinates, then use `visual.attachments` pivot and offset fields to align the larger piece. Keep neighboring atlas regions from overlapping.

Do not trim the exported canvas unless you also update every atlas coordinate. Duplicate-looking left/right layers are deliberate because those are independently addressable mod slots. Individual-PNG `coreClothingSprites` definitions are often simpler for unusually large pieces because every file becomes its full sprite automatically.

For fully original layered garments and hair, `originalClothingAtlas` keeps author-defined `piece/...` regions in one PNG and gives every region its own attachment, pivot, sorting offset, and optional sway settings. `ExampleMods/authored-hairstyles` instead demonstrates the separate-PNG workflow when exact native canvas alignment matters.

The three `clothing-variant.json` files are ready-to-copy additive definition examples. Change their namespace, IDs, display text, and atlas path for a real pack.

`DefaultShirt/content/clothing-override.json` demonstrates the second workflow: replacing the existing Core shirt in place with individual PNG pieces. It does not add a wardrobe entry and preserves the Core shirt's save identity. When copying it into a pack, keep the referenced files inside that pack and adjust the paths to match their location relative to the pack root.

Use a `playerAttachment` document for artwork that should always be part of the player rig rather than an equipable garment. The installed Advanced Clothing Showcase contains a working hips-attached example with normalized pivots and sorting.
