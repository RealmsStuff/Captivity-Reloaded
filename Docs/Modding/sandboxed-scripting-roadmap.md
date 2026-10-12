# Sandboxed mod scripting roadmap

Status: **future proposal; not part of Mod API v1**

This document records a possible post-v1 scripting layer for Captivity Reloaded. It is not a promise that script files are currently accepted by `.capmod`, and it does not change the v1 decision that public mods are data-only.

## Purpose

The scripting layer would cover behaviours that are too stateful or conditional for the existing JSON schemas, especially:

- more expressive enemy movement and combat decisions;
- custom game-mode objectives, scoring, rounds and win/loss conditions;
- coordinated enemies and dynamic participation in QTEs;
- stage encounters that react to gameplay events;
- small mod-specific systems without adding a new compiled engine module for every variation.

Straightforward content should continue to use JSON. Scripts are an optional higher layer, not a replacement for enemy definitions, rule profiles, stage scripts, Tiled maps, asset slots, or normalized animations.

## Proposed runtime

MoonSharp is the leading candidate for an initial prototype because it is a managed Lua interpreter for Unity, has explicit sandbox controls, and avoids loading arbitrary mod DLLs. Windows, Android and WebGL player builds must all pass a prototype before scripting becomes a supported public capability. WebGL is the highest-risk target and must not be assumed compatible without an actual player build and smoke test.

The intended flow is:

```text
.capmod archive
  -> existing archive and manifest validation
  -> existing dependency and enable-state resolution
  -> script-module definition and source validation
  -> one isolated Lua environment per enabled pack
  -> Captivity event bridge
  -> Lua callback
  -> restricted Captivity API proxies
  -> shared, validated engine commands
```

The existing loader remains authoritative. Installing a mod never executes its scripts. Scripts are considered only after the pack is enabled, its dependencies resolve, and all relevant content passes validation.

## Proposed pack shape

```text
example-mod/
  manifest.json
  content/
    enemy.json
    script-module.json
  scripts/
    main.lua
```

A future script-module definition could resemble:

```json
{
  "schemaVersion": 1,
  "type": "scriptModule",
  "id": "example-mod:script/main",
  "path": "scripts/main.lua",
  "scope": "gameplay",
  "apiVersion": 1
}
```

The exact schema is intentionally deferred. At minimum, validation must enforce a pack-local normalized path, file-size limit, supported scripting API version, allowed scope, and unique namespaced ID.

## Relationship to current stage scripts

The stage-script system already provides bounded triggers, conditions, variables, waits, sequences, state machines, spawning, object control, player actions, audio, particles and camera actions. Lua should use the same underlying implementations instead of creating a second privileged path.

The execution logic currently hosted by the external-stage runtime should gradually move behind a shared command layer, conceptually:

```text
JSON stage action --+
                    +-> ModCommandService -> game systems
Lua cr.* command ---+
```

Every public Lua command should therefore have the same validation, range limits, ownership checks and diagnostics as its JSON counterpart. JSON remains the preferred format when its existing triggers and actions are sufficient.

## Runtime components

The proposed engine-side components are:

- `ModScriptDefinition` and parser: discover and validate script-module metadata.
- `ModScriptRuntime`: create, start, suspend and dispose isolated pack environments.
- `ModEventBus`: publish a reviewed set of immutable gameplay events.
- `ModCommandService`: implement bounded operations shared by JSON and Lua.
- Safe proxy handles: expose stable IDs and selected operations without exposing Unity objects.
- `ModScriptScheduler`: service timers and resumable callbacks with execution budgets.
- Namespaced script state: optionally save bounded primitive values for a specific pack and API version.
- Script diagnostics: attribute parse errors, runtime errors and budget violations to the owning pack and source location.

Core does not require Lua, and a data-only pack does not create a Lua environment.

## Public event model

The first API should be event-driven and should not provide an unrestricted per-frame callback. Candidate events include:

- `game_started`
- `stage_opened`
- `wave_started` and `wave_ended`
- `enemy_spawned`, `enemy_damaged` and `enemy_killed`
- `player_damaged`, `player_climax` and `item_used`
- `interaction_started` and `interaction_ended`
- `qte_started`, `qte_participant_joined`, `qte_participant_left` and `qte_ended`
- named stage signals and bounded timers

Event data should contain primitive values and safe handles. Scripts must not retain a handle as proof that its target still exists; every operation rechecks validity.

## Enemy AI scripting

Lua should choose high-level intentions while the engine retains authority over pathfinding, collision, physics, animation and final movement. Appropriate commands include:

- pursue or retreat from a target;
- maintain an attack range;
- patrol, hold, flank, circle or seek cover;
- find a nearby platform, doorway, room or authored marker;
- request a jump, platform drop or supported traversal action;
- select from attacks already declared by the enemy definition;
- coordinate a role or target with nearby compatible enemies;
- request participation in an eligible QTE.

Scripts should not set a `Transform`, velocity or collider directly. An optional `ai_think` callback should run at a bounded low frequency, such as four to ten times per second, and the scheduler may reduce that rate under load. JSON movement presets remain the zero-script path for ordinary enemies.

Illustrative API only:

```lua
function on_ai_think(event)
    local enemy = event.enemy
    if event.player_distance < 2.0 then
        enemy:movement():retreat(3.0)
    elseif event.player_is_above then
        enemy:movement():seek_platform_above()
    else
        enemy:movement():pursue_player()
    end
end
```

## Custom game modes

Rule profiles remain the preferred data-only way to define bounded modes. A scripted mode could add stateful behaviour including:

- objectives, score and mode timers;
- round and wave transitions;
- stage and enemy selection policies;
- dynamic difficulty or enemy modifiers;
- starting inventory and player restrictions;
- rewards, drops and mode-specific status effects;
- custom QTE eligibility and participation rules;
- predefined HUD counters, objective text, progress bars and prompts;
- explicit completion and failure conditions.

The mode API should offer predefined UI widgets rather than arbitrary access to the game's canvases. It must not replace input, saves, menus, physics or other engine systems.

Illustrative API only:

```lua
function on_mode_started(event)
    cr.mode.set_objective("Survive for 10 minutes")
    cr.mode.start_timer("survival", 600)
end

function on_enemy_killed(event)
    cr.mode.add_score(100)
    if cr.mode.score() >= 5000 then
        cr.mode.complete()
    end
end
```

## Multi-enemy QTE integration

The scripting API can complement the engine's participant system without taking ownership of it. A script may query nearby eligible enemies and request that they join; the engine still validates range, capacity, compatibility, role availability, current actor state and animation support.

Late joiners can be handled through the same request while a QTE is active. On acceptance, the engine assigns an available participant role and synchronizes the enemy at the next supported entry point. Rejection returns a stable reason code instead of leaving the encounter partially modified.

Scripts cannot force an invalid actor into a QTE or bypass participant limits.

## Sandbox and security requirements

Downloaded scripts are untrusted. The supported catalog lane must enforce all of the following:

- use a minimal MoonSharp module set; do not expose `io`, operating-system functions, debug facilities, unrestricted loading, or unrestricted `require`;
- disable automatic CLR interop;
- never expose `GameObject`, `Component`, `MonoBehaviour`, reflection objects, delegates, streams, paths or arbitrary game classes;
- expose only owned proxy types with explicit allowlisted methods;
- restrict script loading to normalized files inside the owning pack;
- prohibit filesystem, network, process, environment-variable and thread access;
- enforce per-callback execution or instruction budgets;
- cap timers, coroutines, queued callbacks, recursion, stored state and high-impact commands;
- rate-limit spawning, audio, particles, notifications and other potentially disruptive operations;
- suspend only the offending script environment after a fault or budget violation;
- produce a searchable diagnostic with pack ID, module ID, event and source location.

An optional Windows-only unsafe DLL/Harmony lane would be a separate product decision. It must be disabled by default, clearly marked as unrestricted code, excluded from the safe catalog, and must not weaken `.capmod` guarantees.

## Compatibility and saves

The scripting API must be explicitly versioned independently of the content schema. Scripts target an `apiVersion`, and existing versions should remain available for the normal compatibility period.

Persistent state, if added, should be:

- namespaced by pack and module;
- limited to bounded strings, finite numbers, booleans and shallow collections;
- size-limited and validated before saving;
- preserved while a mod is disabled where practical;
- removable without corrupting the base save;
- passed through an explicit migration callback when a module changes its state version.

Runtime object handles are never serialized.

## Standard-mode guarantee

Standard mode must remain unchanged when no active content selects scripted behaviour:

- Core does not depend on the interpreter.
- No Lua environment is created for data-only packs.
- Only enabled and fully validated packs may start a script environment.
- A script cannot patch existing C# methods or globally intercept engine calls.
- Disabling a scripted pack removes its callbacks and commands after the normal reload/restart boundary.
- A script failure suspends that pack's scripting rather than changing Core defaults.

## Proposed implementation phases

### Phase 0: feasibility spike

1. Vendor a pinned MoonSharp version with license notices.
2. Load one pack-local Lua file through a custom in-memory loader.
3. Enable the hard sandbox and explicit proxy registration.
4. Dispatch `stage_opened` and implement a harmless `cr.notify` command.
5. Produce Windows, Android and WebGL development builds.
6. Measure startup cost, memory use and callback execution time.

Do not publish scripting support if the same script cannot run reliably on every supported target selected for Mod API support.

### Phase 1: shared commands and stage events

1. Extract reusable stage actions behind `ModCommandService`.
2. Add signals, variables, timers and stage lifecycle events.
3. Add source-attributed diagnostics and automatic suspension.
4. Add schema, archive and package-builder validation for script modules.

### Phase 2: enemy AI

1. Add safe actor and enemy handles.
2. Add high-level movement goals and declared-attack selection.
3. Add bounded `ai_think` scheduling and performance telemetry.
4. Add ownership, lifetime and stale-handle tests.

### Phase 3: scripted modes and QTEs

1. Add mode lifecycle, objectives, score, timers and completion state.
2. Add predefined HUD surfaces.
3. Add QTE observation and validated participant requests, including late joiners.
4. Add optional namespaced saved state and migrations.

### Phase 4: public release readiness

1. Freeze Scripting API v1 names and semantics.
2. Publish field, event, command, handle and error-code references.
3. Ship example mods for a stage event, custom enemy AI and custom mode.
4. Add malicious-script, infinite-loop, stress, save-compatibility and platform test suites.
5. Define catalog review and capability-display policy.

## Feasibility acceptance criteria

The prototype is successful only if:

- the same `.capmod` script behaves consistently on Windows, Android and WebGL;
- Standard mode and data-only mods behave identically without starting MoonSharp;
- scripts cannot obtain Unity, CLR, filesystem, network or process access;
- an infinite loop or repeated error is stopped without freezing or terminating the game;
- one broken script does not stop Core or unrelated mods;
- script load and runtime failures are visible in the existing mod diagnostics;
- event ordering and saved state are deterministic enough to document and test;
- the engine can reject invalid AI and QTE requests without leaving actors in a partial state.

## Open decisions

- Whether scripting ships on WebGL if platform storage or execution limits require a reduced feature set.
- Whether environments are one per pack or one per module; one per pack is simpler, while one per module provides stronger fault isolation.
- Which events and commands belong in the first stable API.
- Whether scripted mods require an explicit player-facing capability warning even under the sandbox.
- Whether persistent script state is necessary for the first release.
- How script CPU and memory budgets should scale on mobile devices.
- Whether script APIs are allowed in multiplayer or deterministic/replay contexts if those systems are introduced later.

