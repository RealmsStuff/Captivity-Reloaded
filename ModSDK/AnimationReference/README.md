# Core animation reference

Core enemies still play animation through their original Unity prefabs. The catalog exporter is the first migration step: it turns those opaque controllers and clips into a neutral, machine-readable reference without changing game behavior.

In Unity, choose **Captivity Reloaded > Modding > Export Core Animation Catalog**. The exporter writes `core-animation-catalog.json` beside this file.

Choose **Export All Core Enemy Animations** for migration builds. It writes every public semantic alias both to this SDK reference and to `Assets/Resources/Modding/Core/Content/Animations`, where packaged Core discovery validates and registers it. It also merges the resulting semantic-name-to-content-ID map into each owning `coreEnemy` document's `animationReferences`. Startup rejects missing clips and clips assigned to the wrong enemy. The representative export remains useful for quick preview work.

Catalog schema version 3 assigns reviewed semantic animation names across all 21 Core enemies and records them on states and reused clips. See [CORE-ENEMY-SEMANTICS.md](CORE-ENEMY-SEMANTICS.md) for the authoring contract.

Choose **Captivity Reloaded > Modding > Player Animation Preview** to open the graphical preview and authoring tool. It renders isolated copies of the real Core prefabs and has Standard player, Paired finishers, and Core enemies modes. Core enemies mode can inspect every catalogued enemy clip and export the selected motion into `NormalizedEnemies/<enemy>/<semantic-name>.json`. Each enemy directory also receives `rig.json`, which maps its legacy hierarchy to semantic bones and records untouched transforms plus every discovered sprite region, pivot, canvas size, sorting layer, and sorting order. The **Original Unity clip / Exported normalized JSON** switch samples either source at the same playhead, making transform, artwork, and sorting conversion errors directly visible. Every sample restores untouched player and enemy poses first, preventing unanimated bones, face sprites, and sorting values from leaking out of a previous clip. The Core clothing panel can equip compatible garments directly from their prefab metadata without changing a save or invoking gameplay effects. Playback supports five speeds, clip-rate frame stepping, optional looping, horizontal facing, and dynamic or animation-wide locked framing. Preview copies use an unlit material and cannot modify source prefabs.

The normalized timeline panel edits document metadata, individual numeric track keys, and ordered safe events or semantic cues. Changes are validated immediately, participate in Unity Undo/Redo, and can be copied into the `content` directory of a selected mod pack. Saving is restricted to packs inside the current project and rewrites Core namespaces to the target manifest namespace before validation; review the resulting document and owning enemy ID when adapting a Core reference into original mod content.

Diagnostic overlays show the animated player bone hierarchy, clothing attachment-to-pivot lines, actual sprite outlines, expected template canvases, semantic slot names, target bones, and sorting orders. Canvas outlines turn red when artwork exceeds its template dimensions. The panel also reports missing sprites, renderers, bones or pieces, automatic compatibility replacements, and missing explicit support for the selected `ModBodyVariant`. These warnings describe the isolated preview and do not change a garment or save.

The catalog covers all 21 Core enemies and the standard player controller. It records:

- the source NPC prefab and Animator controller;
- every controller state, motion, speed, and loop flag;
- every enemy animation clip;
- the player-side interaction clips associated with that enemy;
- explicit phase-by-phase enemy-state/player-clip pairs, excluding unused inherited poses;
- ordinary player poses such as idle, movement, crouching, damage, and weapon handling;
- clip duration, frame rate, animation events, numeric curve keyframes, and sprite/object-reference keyframes.

Player-side interaction clips are included deliberately. Those tracks animate the player skeleton and are the useful source for a future graphical clothing preview: the mod-maker can put a garment on the standard player rig, scrub a Core pose, and reveal clipping, bad pivots, or insufficient canvas bounds before launching the game.

## Stability

This catalog is an **experimental authoring format**, not a v1 runtime mod schema. Asset paths, Unity component property names, prefab state names, and automatically suggested roles may change while Core animation migration is in progress. Mods should not depend on this generated file at runtime yet.

Semantic names and rig maps are exported for every Core enemy and have been round-trip compared with their source clips. The version 1 enemy rig/animation shape is frozen; raw Unity catalog paths remain diagnostic and may still change with project asset organization.

## Normalized player reference

Choose **Captivity Reloaded > Modding > Export Normalized Player Reference** to export the stable semantic player-bone hierarchy and representative player tracks. See [NORMALIZED-FORMAT.md](NORMALIZED-FORMAT.md) for the frozen names, coordinate rules, and stable-versus-experimental boundary. External normalized player and enemy animation documents are registered runtime content. Complete Core enemy exports are also packaged and registered, while the copies under this SDK remain editable authoring references. Vanilla enemies retain their original Animator controller as a compatibility fallback until attacks and specialized AI can be moved without changing gameplay.
