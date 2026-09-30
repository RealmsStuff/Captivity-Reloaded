# Rig animation roadmap

Custom rig animations are incremental work. The current inherited-enemy and inherited-clothing formats replace artwork while retaining the Core prefab's bones, pivots, animation clips, controller, colliders, and gameplay events.

The planned authoring path is:

1. A Unity Editor exporter records Core controller states, exact transform curves, sprite-reference curves, and event markers in a neutral JSON catalog. This raw catalog is implemented for all 21 enemies, their associated player interaction clips, and the standard player controller.
2. Human-reviewed mappings replace the exporter's provisional role suggestions with stable semantic names.
3. The reviewed export contains position, rotation, and scale keyframes plus safe event markers such as attack impact, footsteps, and sounds.
4. A graphical preview applies Core player/enemy clips to their paired rigs and currently selected clothing. Playback, frame stepping, facing, locked framing, sorting correction, diagnostic overlays, and finisher VFX preview are implemented. It may later produce a preview sprite sheet or GIF for artists; previews are not runtime animation data.
5. A first runtime importer is implemented for normalized player transform curves referenced by data-driven enemy finishers. It converts bounded curves and safe events into the existing finisher animation runner without exposing arbitrary Unity events. General Animator clip/controller import remains future work.
6. Validation rejects unknown bones, unsafe event calls, invalid time ranges, and missing required states.

Arbitrary Unity animation events will not be accepted from external packs. Event names must come from a small published allowlist so a data pack cannot invoke unrelated game code.

Full-frame LibreSprite animations may be explored later as an experimental visual mode. They are not the primary rig format because flattening the actor would discard the current body-part hitboxes, physics, clothing attachments, and dismemberment behavior.

The raw catalog exporter is implemented under **Captivity Reloaded > Modding > Export Core Animation Catalog**. The graphical preview, normalized player-rig/animation reference exporter, Core enemy semantic rig exporter, source-versus-JSON comparison, and bounded runtime importers are implemented. The enemy rig and animation schema version 1 is frozen after review across all 21 Core enemies. The player animation format and a general Animator-controller importer remain separate future work.
