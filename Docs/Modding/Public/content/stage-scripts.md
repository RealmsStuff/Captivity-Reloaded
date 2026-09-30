# Stage scripts

Experimental `stageScript` definitions add safe event sequences to any external stage without executable code. A script targets a stage content ID and contains named sequences.

The supported triggers are `stage-open`, `wave-start`, `wave-end`, named `signal` events, activation of a named Tiled `interaction`, `player-enter` and `player-exit` events from a named Tiled `script-trigger` rectangle, `timer`, `door-state`, `object-state`, `enemy-count`, and `manual`. Manual sequences run only when another sequence calls or starts them. World triggers are edge-driven: a door, object, or enemy-count sequence runs when its predicate changes from false to true. Timers use `seconds` and repeat unless `once` is true. Enemy counts can optionally be restricted with an `enemy` content ID. Touch boxes track all colliders belonging to the player as one occupant, so crossing their edge does not emit duplicate events for individual body parts.

Conditions support numeric variables by default and the explicit sources `wave`, `enemy-count`, `door-open`, and `object-active`. Every source uses the same `operator` and numeric `value`; boolean sources compare as 0 or 1. Object sources use `objectId`, and enemy counts accept the optional `enemy` filter. This permits portable room/wave/door conditions without exposing Unity components or hierarchy paths.

Sequences can wait for a duration, a named door state, an active-enemy threshold, or until the player is farther than `value` units from one or two named objects; show a normal game notification; set or add numeric variables; send another signal; enable, disable, move, or rotate a named Tiled/runtime object; control spawners, interactions, doors, and lights; activate another interaction; queue enemies; teleport the player to a named point; apply an allow-listed player status; shake or zoom the existing gameplay camera; change global-light intensity; start or stop a named authored audio source; play particles; and play a validated Animator state. Reviewed cutscene actions can also lock or unlock player input, show or hide the existing HUD, set player facing, remove equipped clothing, play a player animation, and kill the player. `move-object-to` copies the world position of `destinationId`, optionally tweening for `seconds`. Motion using explicit coordinates remains stage-local. A wait action with `seconds: 0` has no timeout. Disabling a spawner safely returns its queued enemies to the wave for reassignment. `set-interaction-enabled` changes whether a named interaction can be used without activating it. Variable comparisons can guard a sequence, and `once` prevents it from repeating during that stage run.

`wait-until` accepts the same `conditions` array as a sequence and polls until every condition is true. Its optional `seconds` value is a timeout; zero waits indefinitely while the stage remains open. `repeat` runs its nested `actions` a fixed `amount` from 1 to 100. Repeat blocks may be nested four levels deep, contain at most 32 actions each, and a sequence may expand to at most 4096 executed actions. These bounds make repeated lights, sounds, movements, and puzzle polling possible without permitting an infinite JSON loop.

```json
{
  "type": "repeat",
  "amount": 3,
  "actions": [
    { "type": "set-light", "objectId": "warning-light", "state": "toggle" },
    { "type": "wait", "seconds": 0.2 }
  ]
},
{
  "type": "wait-until",
  "seconds": 30,
  "conditions": [
    { "source": "enemy-count", "operator": "==", "value": 0 }
  ]
}
```

## Reusable machine states

A script may define up to 32 local `machines`. Each machine binds a logical ID to a named Tiled `objectId`, declares an `initialState`, and supplies up to 16 named states. Entering a state runs its reviewed action list, so one definition can consistently coordinate artwork objects, lights, audio, particles, doors, and interactions. `set-machine-state` changes state, and the `machine-state` trigger reacts after that state's actions finish. Setting the current state again is a no-op, preventing duplicate effects.

```json
"machines": [{
  "id": "generator",
  "objectId": "generator-console",
  "initialState": "off",
  "states": [
    { "id": "off", "actions": [
      { "type": "set-light", "objectId": "generator-light", "state": "off" }
    ] },
    { "id": "active", "actions": [
      { "type": "set-light", "objectId": "generator-light", "state": "on" },
      { "type": "play-audio", "objectId": "generator-hum" }
    ] }
  ]
}]
```

Machine and state names are local to one stage-script document. Their actions use the same nesting and 4096-action safety budget as ordinary sequences. The Tiled starter kit connects `paid-secret-button`, `secret-machine-hum`, and `flickering-light` as a complete example.

Player animation actions use `controller: "current"` for a state on the player's active controller or `controller: "finisher"` for an allow-listed state on the game's shared finisher controller. Input locks, HUD hiding, and controller replacement are restored when the authored stage closes, preventing an interrupted cutscene from leaking those transient states into another stage. Clothing removal and death are intentional gameplay changes and are not rolled back.

`apply-player-status` exposes reviewed effect modules rather than CLR class names. The first supported module is `jacky-curse`, configured with `durationSeconds`, `ticksPerSecond`, initial `chance`, per-tick `chanceIncrease`, and `maxActive`. Applying it again is harmless because the status is non-stackable. The FER reference uses it after a 15-second delay when the Jacky-room door opens.

Large events can be split into local named sequences:

- `run-sequence` runs a sequence inline and waits for it to finish.
- `start-sequence` starts it in parallel and continues immediately.
- `cancel-sequence` stops a parallel/top-level run of that sequence.
- `wait-for-sequence` joins a parallel sequence, while `wait-for-signal` waits for the next matching signal.

Sequence actions use `"sequence": "local-sequence-id"`; signal actions use `"signal": "signal-id"`. Wait actions accept an optional `seconds` timeout from 0 to 300, where 0 means no timeout. References—including references inside repeat blocks—cannot cross stage-script files, missing targets fail validation, and call/start/wait cycles are rejected. A called sequence still observes its own conditions and `once` setting. Conditional branching uses the existing safe signal model: update variables, `send-signal`, and place different conditions on sequences listening for that signal. World-state branches can first use `wait-until`, `wait-for-door`, or `wait-for-enemy-count` before sending the signal.

This is intentionally a reviewed action library rather than arbitrary Unity method invocation. Later modules can add doors, lights, actors, dialogue, cameras, vendors, encounter control, and room progression without allowing mod JSON to execute unrestricted code.

See `ExampleMods/tiled-training-yard/content/stage-opening.stage-script.json` for the smallest working example. The FER reference includes the laboratory fuse chain with a pack-local completion cue, wave-driven display poses, door/display encounter activation, and the delayed Jacky-room curse as separate editable scripts. The current format remains experimental while more FER sequences verify and expand it.
