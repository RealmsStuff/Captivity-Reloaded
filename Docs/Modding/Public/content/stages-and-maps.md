# Stages and maps

Template-backed stages inherit a Core layout while replacing encounter data. The experimental authored-layout format reads finite orthogonal Tiled JSON and supports PNG tile/image layers, per-axis image parallax, viewport-aware camera bounds, animated tiles, native `.tx` templates, decorations, curated Core art, pack-local ambient/positional audio, explicit stage music overrides, vendors, weapon cases, three door types, notes, keypads, altars, lights, platforms, moving platforms, particles, loose registered-item pickups, named spawners, explicit navigation links, and validated interaction/action chains. Reusable visible templates cover machines, switches, and ordered Easter-egg steps.

## Multi-room progression

Multi-room stages remain one loaded stage and do not create checkpoints or mid-run saves. A `room` rectangle supplies camera bounds, a named `room-entry` point supplies a transition destination, and a `room-transition` rectangle moves the player to that entry. Entering a room volume also changes the camera bounds for seamless connected layouts.

- Room names, entry names, and transition names must each be unique.
- A room may set `initial: true`; otherwise the room containing the player becomes active.
- Each entry requires a `room` property naming its destination room.
- Each transition requires `destination`, naming an entry.
- `requireNoEnemies: true` prevents traversal while a living enemy remains active.
- `oneShot: true` disables the transition after its first successful traversal.

Transitions are deliberately transient: death, restarting, or reopening the stage uses the ordinary stage start rather than preserving the last room.

{% hint style="warning" %}
**Experimental:** the schema is not frozen. Arbitrary scripts and custom Unity components remain unsupported. Explicit navigation links, audio, and tile animations are available, but creators must still play-test them on every target platform and with each intended enemy movement type. Authored `ambient-audio` and `audio-source` objects preload their pack-local clip; set `playOnStart` to false when a stage script should control playback through `play-audio` and `stop-audio`. When `preserveInheritedStageObjects` is enabled for a migration pack, the inherited prefab hierarchy is translated so its original player start aligns with the authored Tiled player start; retained doors, actors, displays, and trigger zones consequently stay registered with the replacement layout.
{% endhint %}

Use `ExampleMods/training-yard-stage` for inheritance, `ExampleMods/tiled-training-yard` for the original authored-layout preview, and `ModSDK/MapTemplates/TiledStage` as the clean public starter kit. `ExampleMods/core-fer-tiled-port` is a migration reference exported from the shipped FER prefab. Version 0.3 exports its platforms, player start, spawners, vendors, camera bounds, and static SpriteRenderer artwork as pack-local PNG/TSJ assets with editable position, scale, rotation, flipping, tint, opacity, and sorting metadata. FER no longer enables the all-or-nothing `preserveInheritedStageObjects` option. Instead, each remaining specialized door, case, fuse, display actor, pose point, or linked lab object is represented by an explicit `core-stage-object` point with a validated source and authored position. This compatibility adapter is intended for conversion of shipped Core stages; ordinary maps should use the portable public objects and leave both compatibility flags false. FER's spawners are newly created mod components, and door-linked spawners receive explicit open-door requirements. Rebuild the reference with **Tools > Captivity Modding > Export FER as Tiled mod**.

## Special waves

The optional experimental `specialWaves` list temporarily changes a stage's enemy roster for an eligible wave. Selection is run once when the wave begins and is intentionally not persisted; restarting or leaving the stage starts a new run. Active special waves receive a large start announcement, and their display name remains beside the wave number until the wave ends.

```json
"specialWaves": [
  {
    "id": "zombie-rush",
    "displayName": "Zombie Rush",
    "minimumWave": 5,
    "everyNthWave": 5,
    "chance": 0.5,
    "cooldownWaves": 4,
    "enemyCountMultiplier": 1.25,
    "selectionMode": "singleRandom",
    "enemies": [
      { "enemy": "core:enemy/zombie-1", "appearance": "core:enemy/zombie-3", "weight": 2 },
      { "enemy": "core:enemy/zombie-2", "weight": 1 }
    ]
  }
]
```

To activate a rule from a door, button, pickup, or another scripted event, set `triggerSignal` instead of `everyNthWave`:

```json
{
  "id": "security-breach",
  "displayName": "Security Breach",
  "minimumWave": 3,
  "triggerSignal": "security-door-opened",
  "chance": 1,
  "cooldownWaves": 2,
  "selectionMode": "replace",
  "enemies": [
    { "enemy": "core:enemy/android", "weight": 1 }
  ]
}
```

The existing stage-script signal bus arms matching rules. A door-state or interaction sequence can use `send-signal` to emit the configured name. The armed rule is considered at the next wave that satisfies `minimumWave` and cooldown, and the signal is consumed when its `chance` is evaluated. `triggerSignal` cannot be combined with a positive `everyNthWave`. Signals emitted while a wave is active target a later wave; they never replace enemies already spawned in the current wave.

```json
{
  "id": "arm-security-breach",
  "trigger": "door-state",
  "objectId": "security-door",
  "state": "open",
  "once": true,
  "actions": [
    { "type": "send-signal", "signal": "security-door-opened" }
  ]
}
```

- `minimumWave` is the first eligible wave and defaults to 1.
- A positive `everyNthWave` restricts the rule to matching wave numbers; `0` permits every wave.
- `chance` is rolled once on each eligible wave. If multiple rules pass, one is selected using their chance values as relative weights.
- `cooldownWaves` prevents the same rule from repeating too soon within the current run.
- `enemyCountMultiplier` scales the ordinary enemy count for the selected wave.
- `replace` uses the listed roster, `add` combines it with each spawner's normal roster, and `singleRandom` selects one weighted entry for the entire wave.
- Optional `appearance` keeps the selected enemy's AI, attacks, and finishers but copies sprites from another enemy with a compatible named-bone hierarchy. Incompatible appearances are skipped with a warning.

Special-wave roster overrides and generated appearance templates are cleared before the following wave. Animations are not retargeted: `appearance` is intended only for enemies that already share the same bone layout.

### Shipped Core roster

Core stages include a conservative starter roster so ordinary waves remain the majority of a run. Chances are evaluated only on the listed cadence, and the same rule cannot repeat inside its cooldown.

| Stage | Special wave | First eligible cadence | Chance | Roster |
| --- | --- | ---: | ---: | --- |
| Shack | Rotting Horde | Every 6 waves | 65% | Zombie I, Zombie II, Zombie III |
| Shack | Grabber Night | Every 7 waves | 45% | Zombie Grabber, reduced enemy count |
| Cave | Buzzing Swarm | Every 5 waves | 70% | Fly and Musca |
| Cave | Brood Below | Every 7 waves | 50% | Maggot, Head Humper, Spider Mother |
| Jungle | Goblin Warband | Every 5 waves | 70% | Orc, Goblin Marksman, Goblin Trapper, Gremlin |
| Jungle | Hunter Pack | Every 7 waves | 50% | Hunter and Death Hound, reduced enemy count |
| Space Station | Boarding Party | Every 5 waves | 70% | Litigant, Sqoid, Head Humper |
| Space Station | Specimen Breach | Every 8 waves | 55% | Spider Mother, Sqoid, Musca, reduced enemy count |
| FER | Animatronic Spotlight | Every 5 waves | 100% | One of Abby, Jenny, or Sunny using the faceless android appearance |
| FER | Closing Shift | Every 8 waves | 65% | Sunny, Jenny, Abby, and Jacky |
| Field-Day | Target Practice | Every 3 waves | 75% | One of Zombie I, Fly, or Gremlin |
| Field-Day | Mixed Drill | Every 5 waves | 60% | Zombie Grabber, Goblin Marksman, Sqoid |

When two rules are eligible on the same wave, only one is selected. Passing candidates use their configured chance values as relative selection weights.
