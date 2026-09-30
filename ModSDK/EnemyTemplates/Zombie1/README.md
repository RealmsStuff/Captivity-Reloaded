# Zombie I inherited-enemy template

The included definition uses `spawn.selectionWeight: 0.25`. When a spawner originally contains one Zombie I choice, that gives this variant a 20% share and the Core Zombie I an 80% share. Increase the weight to make the variant more common, or disable `inheritTemplateSpawners` and place its full enemy ID directly in a custom stage spawner.

This kit creates an editable LibreSprite document for an enemy extending `core:enemy/zombie-1`. It uses the exact 32×32 Core body-part PNGs, arranged as a 128×128 atlas with one named layer per public rig slot.

The `source-parts` directory contains clearly named copies of those Core PNGs. These aliases prevent LibreSprite from mistaking Unity filenames such as `TorsoLower_5.png` for numbered animation sequences.

The generator copies pixels directly into one full-canvas layer per body part. It does not depend on clipboard paste behavior or on LibreSprite creating cels for empty layers.

## Build the LibreSprite file

1. Install LibreSprite.
2. Check the `projectRoot` value near the top of `build-zombie-template.js`. Change it if your repository is elsewhere.
3. In LibreSprite, choose **Scripts → Open Scripts Folder** and copy `build-zombie-template.js` there.
4. Choose **Scripts → Rescan Scripts**, then run **build-zombie-template** from the Scripts menu.
5. The script creates `zombie-1-template.aseprite` and `zombie-1-template.png` in this SDK directory, not in LibreSprite's AppData folder.
6. Open the generated `.aseprite` file and edit the named layers without moving artwork outside its assigned 32×32 cell.
7. Export a non-trimmed 128×128 PNG. Copy `manifest.template.json` to your pack as `manifest.json`, copy `zombie-variant.json` into its content folder, and update their example IDs and text.

Before testing an export, confirm its canvas is exactly 128×128. A larger canvas changes Unity's bottom-origin atlas coordinates even when the artwork appears to occupy the upper-left 128×128 area.

The checked-in script points at this checkout because LibreSprite runs copied scripts from its AppData scripts directory and does not provide the original repository location. Only the generated template and atlas are written by the script, both inside `ModSDK/EnemyTemplates/Zombie1`.

## Layout

The table uses LibreSprite's top-left cell coordinates. The JSON file uses the equivalent Unity bottom-left pixel rectangles.

| Row | Column 1 | Column 2 | Column 3 | Column 4 |
| --- | --- | --- | --- | --- |
| 1 | torso lower | butt | hips | chest |
| 2 | neck | head | arm upper | arm lower |
| 3 | hand | leg upper | leg lower | left foot |
| 4 | right foot | unused | unused | unused |

Shared arm, hand, and leg art is applied to both sides of the inherited rig. The feet remain separate. Zombie I's extra genital sprite is prefab-owned and is not a published v1 rig-atlas slot.

This is an inherited-rig art template, not a new skeleton. The Core prefab continues to own its bones, pivots, animation clips, colliders, attacks, and specialized components unless a field is explicitly supported by the enemy schema.
