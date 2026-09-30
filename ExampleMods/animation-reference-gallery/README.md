# Original Enemy Animation Reference Gallery

This example contains portable copies of the complete normalized Core animation sets for six substantially different enemy rigs:

| Reference | Body plan | Bones | Clips | Useful comparisons |
| --- | --- | ---: | ---: | --- |
| Zombie I | Full humanoid | 21 | 16 | Walking, climbing, jumping, recovery, attacks, and an eight-phase paired sequence |
| Death Hound | Quadruped | 23 | 13 | Four-legged locomotion, charge preparation, leap attack, and an eight-phase paired sequence |
| Fly | Small airborne | 18 | 8 | Wing motion, charge preparation/attack, and compact airborne pairing |
| Maggot | Compact crawler | 5 | 6 | Minimal bone set, crawling, jumping, leap attack, and a short paired sequence |
| Musca | Large airborne carrier | 21 | 10 | Separate travel/wing motion, carrying, attack motion, and a six-phase paired sequence |
| Gremlin | Small humanoid | 22 | 19 | Tool attacks, climbing, recovery, locomotion, and an eleven-phase paired sequence |

## Viewing the examples

1. Open **Captivity Reloaded > Modding > Player Animation Preview**.
2. Select **Modded enemies**.
3. Choose this `animation-reference-gallery` folder.
4. Pick a reference enemy and animation.
5. Use **Animation Preview** to inspect its motion or **VFX Timeline** to make a temporary effect edit.

The six enemy definitions extend Core enemies only so the preview has matching artwork and a runtime hierarchy. They set `inheritTemplateSpawners` to `false`, so installing the gallery does not add them to ordinary waves. The copied `enemyAnimation` documents themselves use this pack's IDs and are valid examples of the portable animation format.

The editor test suite parses all 72 animation documents, validates every referenced bone against the six published `rig.json` files, checks pack-local presentation assets, and builds each document into its runtime clip representation. This makes the gallery the compatibility reference for animation and future enemy-AI authoring work.

## What to compare

- `reference/rigs/*/rig.json`: published names, parent relationships, default transforms, sprite pivots, and sorting orders.
- `content/animations/*/*.enemy-animation.json`: duration, looping, transform curves, sprite tracks, safe events, particle definitions, and timed effect triggers.
- Locomotion clips: how many bones are actually animated for a readable silhouette.
- Attack clips: anticipation, contact timing, recovery, and the relationship between motion and safe events.
- Finisher phases: how longer paired sequences are divided into small semantic clips.

Do not copy the `prefab`, `controller`, `unityPath`, or `asset` values from a reference rig into a new original enemy. Those fields document the Core source hierarchy for comparison. A fully original enemy should define its public visual bones, atlas regions, hit zones, AI, attacks, and semantic animation references in its own enemy JSON.

These are reference ports, not new artwork. Replace the Core-backed enemy definition with an `originalSkeletonAtlas` visual when turning an example into an original enemy.

## Exporting to DragonBones

Open **Captivity Reloaded > Modding > DragonBones Round Trip**, choose this enemy from the Reference Gallery presets, and export. Import the generated `*_ske.json` and PNG files from the `images` folder into DragonBones. Export edited work as DragonBones JSON data version 5.5, then use the bridge's import tab to convert its bone timelines back into Captivity animation JSON.
