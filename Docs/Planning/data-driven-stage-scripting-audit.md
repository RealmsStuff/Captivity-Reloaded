# Data-driven stage scripting audit

Status: implementation in progress. The deterministic runner, scoped numeric variables, signals, interaction/player-volume/wave/stage/timer/door/object/enemy-count/machine-state triggers, wave/enemy/door/object selectors, manual helper sequences, synchronous and parallel composition, joins, cancellation, cycle/execution guards, bounded repeat blocks, general condition waits, reusable machine states, normalized named-object resolution, doors, lights, spawner and interaction state, actor activation, player-distance waits, target-to-target movement, player teleport, camera effects, global lighting, audio, particles, validated Animator states, allow-listed player status, and reviewed player cutscene controls are implemented. FER's laboratory chain and completion audio, delayed Jacky-room curse, twelve wave-driven statue moves, linked display actors, and locked-room spawner activation are represented by stage scripts. Its Tiled port now explicitly selects each remaining Core-only gameplay object rather than retaining the entire inherited hierarchy. Remaining bespoke callbacks in other stages remain.

## Goal

Use validated, namespaced JSON event graphs to let map authors build encounters, machines, puzzles, secrets, and scripted sequences without executable mod DLLs. Tiled objects provide stable map object IDs and simple trigger bindings; larger graphs live in separate `stage-script` JSON documents.

## Core-stage coverage audit

- Shack: doors, lights, vendors, weapon cases, and wave spawners.
- Field Day: waves, spawners, vendors, weapon cases, and particles.
- Cave: generator/button chains, connected work lights, looping audio, and flashlight placement.
- Jungle: a four-item altar puzzle, placed-item visuals, wave-dependent activation, spirit release, global-light transitions, and a pursuing effect entity.
- Furry Entertainment Robotics: a three-fuse all-of puzzle, enabling lab objects and interactions, keycard doors, delayed status effects, wave-driven statue poses, offscreen movement, scripted display actors, and encounter activation.
- Space Station: proximity roller doors, touch-driven status effects, corpse interaction, animation-driven enemy release, particles, and sounds.
- Hub: generated keypad codes, note-text substitution, challenge-completion conditions, repeated-switch secrets, world-theme changes, wardrobe/terminal menus, player positioning and animation, input locking, HUD transitions, and scripted sequences.

## Required runtime model

Each graph contains triggers, conditions, actions, variables, selectors, signals, and explicit execution policy. Scripts must be cancelled when their stage closes and must never address Unity hierarchy paths or component methods directly.

### Triggers

- Stage open/close and wave start/end.
- Player or actor entering/exiting a volume.
- Interactable activation, door unlock, and object damage/destruction.
- Enemy spawn/death and filtered enemy-count changes.
- Animation cues, timers, variable changes, and named signals.

### Conditions

- All/any/not composition and numeric comparisons.
- Inventory, money, challenge, wave, difficulty, and game-mode checks.
- Object state and prior sequence completion.
- Entity counts filtered by object ID, tag, content ID, spawn group, room, or state.
- Distance, camera visibility, player-facing, animation state, and player status/body/clothing checks.
- Exact activation counts and completion of a set of distinct items.

### Actions

- Set/increment variables and send signals.
- Enable, disable, show, hide, destroy, or change interaction/collider state.
- Open, close, lock, and unlock doors.
- Spawn/despawn actors, items, particles, and reusable machines.
- Change sprites, tint, sorting, lights, animation, audio, music, and object transforms.
- Move, rotate, scale, teleport, tween, follow, home, patrol, and attach to semantic bones.
- Apply reviewed status, damage, healing, force, ragdoll, inventory, reward, and body-effect modules.
- Lock player input, control approved HUD/menu surfaces, adjust camera focus/zoom/bounds, and complete/fail a stage.
- Generate values and substitute them safely into notes or messages.

### Sequence control

- Ordered steps, delays, conditional signal branches, parallel starts, joins, cancellation, `wait-for-signal`, bounded `repeat` blocks, general `wait-until` conditions, and bounded world-state waits are implemented.
- Cancellation, cooldowns, once-per-run/save flags, and re-entry policies (`ignore`, `restart`, `queue`, or `parallel`).
- Local, stage, save, and pack-namespaced variable scopes.

## Reusable definitions

Machines should expose named states such as `off`, `starting`, `active`, and `broken`, with state-owned visuals, animation, light, audio, colliders, and interactions. Reusable logic prefabs should cover vendors, perk/upgrade machines, switches, traps, puzzle steps, and encounter controllers.

Per-frame behavior should use reviewed modules such as `follow-target`, `move-to`, `orbit-target`, `patrol`, `flee-target`, `home-on-target`, `attach-to-bone`, and `wait-until-offscreen`, rather than JSON loops.

## Suggested implementation order

1. Create a deterministic sequence runner, object-ID resolver, scoped variables, signals, validation, cancellation, and execution limits.
2. Recreate Cave's generator/lights and FER's fuse/lab/door sequences.
3. Add selectors, encounter waits, spawning, object states, audio, lights, particles, and normalized animation actions.
4. Recreate the Jungle altar and Space Station corpse release.
5. Add generated values, text interpolation, camera/HUD/input actions, and recreate the Hub secret flows.
6. Add reusable machine/state definitions and graphical node authoring that saves ordinary JSON.

Advanced sandboxed scripting can be reconsidered only if reviewed event/action modules prove insufficient. It is not required for the first public format.
