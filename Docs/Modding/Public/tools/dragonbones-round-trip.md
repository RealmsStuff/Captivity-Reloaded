# DragonBones round trip

The DragonBones bridge is an optional animation-authoring tool. Captivity Reloaded does not load the DragonBones runtime and `.capmod` files continue to contain normalized Captivity animation JSON.

The verified DragonBones Pro 5.6.3 installer is retained outside the Unity project at `../Tools/DragonBones/DragonBonesPro-v5.6.3.exe`. On Windows it installs the editor and its bundled Adobe AIR support to `C:/Program Files/Egret/DragonBonesPro/DragonBonesPro.exe`. DragonBones files are not imported into the Unity project, game, or `.capmod` runtime.

Open **Captivity Reloaded > Modding > DragonBones Round Trip**. **Core/reference rig** contains presets for the Zombie, Death Hound, Fly, Maggot, Musca, and Gremlin families. A custom reference export needs an `enemyRigReference` JSON, a directory containing animations for one enemy, an output directory, and the rig's pixels-per-unit value.

Choose **Original enemy atlas** for a data-defined enemy. Select its `.enemy.json`, animation directory, and output directory. This mode reads the atlas directly, extracts every declared region with nearest-neighbor scaling, preserves the bone hierarchy and sorting order, and converts each normalized bottom-left pivot into the corresponding centered DragonBones display offset. It does not need a prefab or Unity-imported sprites.

The **Create Original Enemy** wizard can perform this export automatically. Its projects are written outside the loose mod under `DragonBonesProjects/<pack-id>/<enemy-id>` so authoring files and enlarged working images are not included in the `.capmod`.

The exporter creates:

```text
<enemy>_ske.json
captivity-roundtrip.json
images/
  <display>.png
```

The skeleton uses DragonBones data version 5.5. It contains one armature and all animations found in the selected directory. Hermite curves are sampled at the exported frame rate so their visible motion survives the format conversion. Unity's upward Y axis and counter-clockwise rotation are converted to DragonBones coordinates. Animation-safe events appear as `cr.event.*` frame markers and particle triggers appear as `cr.vfx.*` markers.

The default pixel preview scale is 4. Exported images are enlarged with nearest-neighbor sampling and all DragonBones coordinates use the same scale, keeping small pixel sprites clearer in DragonBones without changing their size after round-trip import. Unity's non-centered sprite pivots are baked into display offsets around a conventional centered DragonBones pivot. Import corrected exports into a new DragonBones project; an existing project may retain its previously imported resource transforms.

For a future paired interaction, enable **Add a paired enemy + player draft project** in the enemy wizard. It creates matching `paired-draft` enemy and player animation documents and a `<enemy>-paired_ske.json` whose `enemy-*` and `player-*` bones share one playhead. The draft is deliberately not connected to gameplay; after authoring and importing both sides, reference those clips from a finisher phase or scripted interaction. The `enemy-only` subfolder retains the normal idle, move, and attack project.

Import `<enemy>_ske.json` and the PNG files from `images` into DragonBones. Keep bone and animation names intact. When exporting the edited armature, use DragonBones JSON data version **5.5** when available. The bridge accepts JSON skeleton data; do not select binary `.dbbin` output.

For a generated original enemy, return to **Create Original Enemy > Import DragonBones**. Select the loose mod, edited skeleton JSON, and its `captivity-roundtrip.json` or `captivity-paired-roundtrip.json`. **Analyze Only** converts into a temporary directory and reports removed or renamed bones, missing clips, unknown timeline targets, invalid durations, unmapped clips, and lost layer metadata without changing the mod.

**Validate, Back Up, and Import** matches converted animations to existing mod documents by content ID rather than filename. It applies them to a staged copy of the complete mod and runs normal `.capmod` validation before touching the source. If validation succeeds, originals are copied to `ModAuthoringBackups/<pack>/<armature>/<timestamp>` outside the loose mod and the validated files replace them. A failed live validation restores that backup automatically. The importer will not silently create or reconnect an unknown animation ID.

Then open **Paired interaction** in the Original Enemy window. Discover the clips, choose one continuous pair or two named phases, set the struggle and outcome values, and select **Validate and Configure**. This adds the runtime `downedFinisher` module while preserving unrelated behavior. **Open Pair in Preview** hands the chosen enemy and player documents directly to the existing animation/VFX preview for alignment and effects review.

The DragonBones bridge's lower-level import tab remains available for comparison-directory conversions. Use the original-enemy wizard's import mode when replacing real mod files.

The importer currently round-trips bone position, rotation, and scale timelines. Captivity-only events, effect definitions, effect triggers, source metadata, warnings, and existing sprite/object tracks are preserved from the sidecar. DragonBones event markers are currently visual references; changing their time does not yet change the preserved Captivity event. Slot color, display-switch, and z-order edits are not imported in this first slice.

The editor test suite includes a complete generated-original-enemy round trip. It exports enemy-only and paired projects, modifies a DragonBones transform, safely imports both paired documents, and verifies hierarchy, non-centered pivots, sprite ordering, all required animations, backup creation, and final `.capmod` validation. This is the regression gate for changes to either side of the bridge.

Older `captivityLoongBonesRoundTrip` sidecars remain accepted. They do not need to be regenerated before import.

After import, open **Player Animation Preview** to compare the result and use **VFX Timeline** to edit particle presentation.
