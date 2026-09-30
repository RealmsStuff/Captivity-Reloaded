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
