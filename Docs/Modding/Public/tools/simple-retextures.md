# Simple retexture wizard

Use **Captivity Reloaded > Modding > Create Simple Retexture...** when a mod only replaces published PNG artwork. The wizard removes the error-prone parts of an `assetPatch`: it copies the PNGs into the pack, writes the content document, adds the exact manifest `overrides`, adds the Core dependency when needed, and validates the finished loose mod.

## Retexture a Core enemy

1. Create a mod with **Captivity Reloaded > Modding > Create Mod...**, or choose an existing mod folder containing `manifest.json`.
2. Open **Create Simple Retexture...** and select the Core enemy.
3. Choose the folder containing the edited PNGs.
4. Select **Scan folder and match slots**.
5. Review the matched public slots and uncheck anything that should remain unchanged.
6. Select **Create or update retexture**.
7. Open **Captivity Mod Packager**, validate once more, and build the `.capmod`.

The scanner recognizes both original Unity sprite names such as `Head_11.png` and the filenames in a Core DragonBones export. Files ending in `-2`, `-3`, and so on are assigned to separate renderers that use the same original artwork, such as left and right limbs. It uses the generated [Core enemy asset-slot catalog](../reference/enemy-asset-slots.md), including animation-only variants, instead of guessing legacy global slots.

## DragonBones export scale

Core DragonBones exports enlarge pixel art so it is easier to edit. Their `captivity-roundtrip.json` records that scale, normally `4`. When the selected `images` folder sits beside that file, **Auto-detect** and **Scan folder** select the correct scale automatically. The wizard uses nearest-neighbor downscaling while copying the PNGs into the mod, so a 4x editing image returns to its native game dimensions.

Do not manually shrink the PNG and also leave **4x** selected. If the artwork is already at native size, choose **1x (native)**.

## Weapons, player art, clothing, and stages

Enable **Use a custom public target**, copy the target owner from the [public asset-slot catalog](../reference/asset-slots.md), and enter the relative slot separately. For example:

- target: `core:weapon/pistol`
- relative slot: `body`

Choose **Add PNG...** for each replacement. The list can contain several slots under the same target. A single generated patch intentionally has one target; create a second patch name when replacing artwork owned by another target.

## What the wizard does not guess

- It does not publish private Unity asset paths as public mod IDs.
- It does not add gameplay or animation changes to a retexture.
- It does not package the mod until you explicitly use **Captivity Mod Packager**.
- It cannot decide whether artwork is licensed for redistribution.

If an enemy PNG is reported as unrecognized, compare its original name with the detailed enemy catalog. Renamed files can be added through custom-target mode by entering the enemy target and exact relative slot.
