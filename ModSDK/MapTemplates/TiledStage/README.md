# Tiled stage starter kit

This directory is a ready-to-copy external stage pack and an editable Tiled example. Open `levels/starter-map.json` in Tiled.

## Supported map settings

- Orientation must be **Orthogonal**.
- Infinite maps must be disabled.
- The example uses 32 by 32 tiles, `pixelsPerUnit: 32`, and a 256 by 96 tile canvas. That matches the approximate width of Space Station and exceeds Cave's vertical platform span, the widest and tallest existing combat-stage measurements.
- Save/export as JSON inside the mod pack. The stage schema currently requires a `.json` path.
- Multi-room layouts use `room` camera rectangles, named `room-entry` points, and `room-transition` trigger rectangles. These transitions are not checkpoints and never change save respawn behavior.
- Tiled coordinates start at the upper-left and increase downward. The loader converts them to centered Unity coordinates automatically.

## Visual tile layers

Visible tile layers now render in their Tiled order. The V1 preview supports:

- Raw numeric tile arrays in JSON (Tiled's default JSON output); base64/compressed layer data is not supported.
- Embedded JSON tilesets and external `.json`/`.tsj` tilesets stored inside the same mod pack.
- PNG spritesheets with margin and spacing.
- Pack-local PNG image layers, including Tiled's horizontal and vertical repeat flags.
- Native image-layer parallax factors (`parallaxx` and `parallaxy`). `1` behaves like ordinary world art, lower values move more slowly, and `0` stays fixed relative to the camera on that axis.
- Layer visibility, opacity, nested group offsets, and horizontal/vertical tile flips.
- Native Tiled tileset animations with 2 to 256 frames and per-frame durations from 16 to 60000 milliseconds. Animated tiles work in tile layers and tile-object decorations.

Export external tilesets as Tiled JSON (`.tsj`), not XML (`.tsx`). Tiles may be larger than the map grid and use Tiled's bottom-left alignment. Diagonal flips are experimental and currently render without their diagonal transform.

Set `layout.hideInheritedVisuals` in `content/stage.json` to `true` when tile layers replace an inherited Core stage's artwork. It is harmless for the stage-independent `core:stage/mod-template` base, which has no inherited scenery.

## Gameplay objects

Create an object layer, conventionally named `Gameplay`, and use these classes/types:

| Class | Shape | Requirements |
| --- | --- | --- |
| `platform` | Rectangle | Positive width and height; becomes a `BoxCollider2D` and navigation platform. |
| `player-spawn` | Point | Exactly one per map. |
| `enemy-spawner` | Point | One or more; every name must be non-empty and unique. |
| `decoration` | Tile object | A non-empty tile object selected from a JSON/TSJ tileset; renders without collision or behavior. |
| `weapon-vendor` | Point | Instantiates the shared fully functional weapon vending-machine template at this position. |
| `usable-vendor` | Point | Instantiates the shared fully functional medicine/item vending-machine template at this position. |
| `weapon-case` | Point | Creates a purchasable weapon case. Set `weapon` to a Core or mod `item/weapon/...` content ID and `caseSize` to 1, 2, or 3. |
| `door` | Point | Clones a Core door. Give it a unique object name and set `doorType` to `standard`, `jacky`, or `roller`. |
| `door-switch` | Point | Toggles the doors named by its comma-separated `targetDoors` property. |
| `core-art` | Rectangle | Places and scales an approved Core background or visual prop selected by its `art` ID. |
| `note` | Point | Opens the Core reading HUD with custom `text` and `fontSize`. |
| `keypad` | Point | Opens the controller-compatible keypad HUD and opens the doors in `targetDoors` when its numeric `code` is entered. |
| `altar` | Point | Clones the complete Jungle altar, including fetish placement, wave activation, effects, and Altar Spirit release. |
| `interaction` | Point or rectangle | A use, touch, or shoot node with conditions and composable actions. Optional `visualArt` makes it a visible Core-art machine; paired pack-local `visualFile`/`activatedVisualFile` PNGs provide portable state changes. |
| `machine` | Point/tile object | A readable alias for a use interaction. Use the purchase and healing templates as starting points. |
| `logic-switch` | Point/tile object | A readable alias for a use interaction that controls other named objects. |
| `easter-egg-step` | Point/tile object | A single-use step that can require earlier named interactions and activate later steps. |
| `light-bulb` | Point | A usable, shootable Core light with starting state and flicker controls. |
| `proximity-light` | Point | A pack-local sprite and point/freeform 2D light that fades with nearby actors; can start inactive for stage-script activation. |
| `freeform-light` | Point | A static 2D light with authored polygon, falloff, color, and affected sorting layers. |
| `global-light` | Point | The stage-wide 2D light, rebinding global-light scripts to the JSON-created fixture. At most one per stage. |
| `point-light` | Point | A standalone 2D cone light with optional two-phase alarm pulse. |
| `pickup` | Point | Spawns a registered weapon, usable, or consumable by full content ID. |
| `moving-platform` | Rectangle | A visible kinematic platform with pixel offset, travel/pause timing, and approved Core art. |
| `particle-emitter` | Point | Creates bounded looping particles with configurable color, rate, lifetime, speed, size, and radius. |
| `nav-node` | Point | Adds a named move/climb link, optionally bidirectional or marked for flying enemies. |
| `ambient-audio` | Point | Loads a pack-local WAV/OGG as non-positional ambience; `playOnStart=false` reserves it for stage-script playback. |
| `audio-source` | Point | Loads a pack-local WAV/OGG as positional audio with linear distance falloff; `mixerGroup` can be `SFX` (default) or `Ambience`. `playOnStart=false` reserves it for stage-script playback. |
| `script-trigger` | Rectangle | Emits `player-enter` and `player-exit` stage-script triggers using its unique object name. |
| `stage-marker` | Point | A named, map-owned position for stage-script destinations, such as an actor's statue pose. |
| `stage-item` | Point | Creates a non-weapon/non-usable key item from pack-local PNG artwork and sends its `onPickupSignal` to stage scripts. |

An `enemy-spawner` object's name must exactly equal one `spawners[].id` in `content/stage.json`. Every JSON spawner needs a Tiled point and every Tiled spawner point needs a JSON entry. Enemy lists, timing, minimum wave, weighting, and out-of-sight behavior remain in the stage JSON.

Spawner entries may also list `requiredOpenDoors` and `requiredClosedDoors`. The values are Tiled door object names. A gated spawner is excluded from wave allocation until every named door has the required state, and already queued spawns pause if that state later changes. `requiredOpenDoors` is the normal choice for permanent player-opened doors; `requiredClosedDoors` is intended for scripted or automatic doors that can close again. Set experimental `enabled: false` when an interaction or stage script should activate a spawner later.

Platforms accept Boolean `climbableLeft` and `climbableRight` custom properties. Enable them on exposed ends that the player should grab. The starter leaves the long ground edges disabled and enables both ends of its three elevated platforms.

The `templates` directory contains reusable Tiled object templates. Vendors, machines, and switches use small redistributable preview PNGs, so they are visible and placeable as normal tile objects inside Tiled while still becoming their proper Core/runtime object in game. Native `.tx` template references are resolved at load time, including instance name, position, size, and property overrides. Templates must remain inside the same mod pack; cycles, missing files, and paths escaping the pack are rejected. You can also draw objects manually and set their Class field. Nested Tiled group offsets are supported.

For a decoration, create a Tile Object from a tileset and set its Class to `decoration`. Position, width/height scaling, clockwise rotation, visibility, object-layer opacity, draw order, and horizontal/vertical flips are carried into the game. Optional `sortingLayer`, `sortingOrder`, `opacity`, and `tintColor` properties provide explicit control when preserving artwork exported from an existing stage. The starter's `Decorations` layer includes a 64 by 64 lamp example.

Decorations accept an optional Boolean `repeat` property. When enabled, the sprite tiles across the object's rectangle instead of stretching; the 252-unit starter floor uses this. The optional `sortingLayer` string accepts `Background`, `Decoration`, or `Platform`.

Vendor points use shared runtime templates and the existing vendor UI, including wave enable/disable behavior and the loaded mod item/weapon catalogs. The starter places both vendor types near its center and does not require either vendor to exist in an inherited map.

Weapon cases use the existing purchase and reload behavior through a shared runtime template. The starter includes a size-1 case containing `core:item/weapon/revolver-44`; change the `weapon` property to an installed additive weapon's full content ID to offer that weapon instead.

Doors use the shared runtime object library, so all three types can be used without inheriting a playable Core map. `standard` is the normal hinged door, `jacky` opens like its Core counterpart and applies the delayed curse, and `roller` uses the Space Station rolling animation. `initiallyOpen` defaults to false. An unlinked roller door opens automatically when an actor enters its `proximityRadius` (default 8); linking it from a `door-switch` makes it switch-controlled instead. A switch may target several doors, for example `west-door,east-door`.

The starter arena places all three door types and a separate switch for each one. These are runtime objects, not decorative tiles, and preserve their original sprites, sounds, collision, and interaction behavior.

To reskin shared objects without editing a Unity prefab, add pack-local PNG paths to their Tiled properties. Vendors and roller-door panels use `visualFile`; weapon cases use `visualFile` and `brokenVisualFile`; standard and Jacky doors use `closedVisualFile` and `openVisualFile`; door switches use `offVisualFile` and `onVisualFile`. Stateful objects require both images so their appearance remains correct after interaction. Set `visualPixelsPerUnit` (default `32`, allowed `1`–`1024`) if the PNG uses another scale. Paths must stay inside the pack and PNGs must be at most 16 MiB. Light bulbs already support `visualFile` and `activatedVisualFile`. The roller panel retains its original scale-based opening motion.

The starter also places a readable code note and a keypad beside the standard door. Enter `1234` to open it. Both objects use their original game HUD and controller handling.

The paid secret button demonstrates a visible machine-style interaction graph using the FER brain-machine art. Beginning on wave 1, while no more than 20 enemies are alive, a player carrying the nearby revolver may spend 25 money to open the Jacky door, toggle a named light, queue two enemies at a named spawner, and receive an ammo box after a short delay. The revolver requirement is not consumed in this example. Set `consumeRequiredItem` to true for key items or ingredient exchanges; unsupported content IDs, challenge prerequisites, and interaction cycles are validated before play.

Interaction conditions also support `requiredMoney` (checked but not spent) and `requiredInteractions` (a comma-separated list of named steps that must already have fired). New actions include `restoreHealth`, `knockbackX`, `knockbackY`, and `ragdollSeconds`. This supports healing or purchase machines, launch pads, switches, ordered Easter eggs, and reward sequences without custom C#.

The starter also includes quiet ambient and positional machine-hum sources. Replace the small demonstration OGG files under `assets/audio` with your own WAV/OGG files. Positional sources load when the stage opens and stop when it closes.

The `script-volume-demo` rectangle demonstrates script-only touch boxes. Its object name is referenced by `content/touch-box-demo.stage-script.json`; entering and leaving it produce separate events, and sequence-level `once` controls whether either event may repeat. Resize or duplicate the rectangle in Tiled and give every copy a unique name.

The optional top-level `audio` object in `content/stage.json` supplies `ambience`, `entryMusic`, `waveMusic`, and `waveComplete` from pack-local WAV or OGG files. With a playable Core stage in `extends`, omitted slots retain inherited clips; with `core:stage/mod-template`, omitted slots remain silent.

The Jungle altar example is at world X `64`. It is rebound to the authored stage's wave manager and global light instead of retaining references to the original Jungle stage.

`core-art` lets maps reuse stable, curated game artwork without copying the source PNG into the mod. Size the rectangle to the desired in-game footprint and select an ID from `reference/core-map-art.json`. Optional `sortingLayer` values are `Background`, `Decoration`, and `Platform`; `flipX` and `flipY` mirror the art. Core art is visual-only and does not create collision. The starter's far-right area demonstrates a Space Station bay background, poster, barrel, and chair.

For a background that must be visible while editing, add a normal Tiled Image Layer and select a PNG inside the mod pack. The starter uses `assets/space-bay-background.png`, aligned to the ground and repeated horizontally across the arena. Image layers render on Unity's `Background` sorting layer and honor visibility, opacity, group offsets, and `repeatx`/`repeaty`.

Tiled layouts automatically constrain the camera to the complete map rectangle. Add `camera.bounds` to `content/stage.json` to use a smaller authored region or to include scenery outside the tile canvas. Bounds use stage-local Unity units and require `minX`, `minY`, `maxX`, and `maxY`; the camera accounts for its viewport size so it does not reveal space beyond those edges. The starter explicitly uses its full 256 by 96 unit map bounds.

See `reference/core-stage-bounds.md` for the measured Core stage footprints used to size this template.

## Test and customize

1. From `ModSDK/MapTemplates`, run `powershell -ExecutionPolicy Bypass -File .\verify-tiled-pack.ps1`. This checks JSON syntax, pack-local file references, finite orthogonal map requirements, the player spawn, and matching enemy-spawner IDs.
2. Copy this directory into the game's `Mods` directory.
3. Rename `manifest.template.json` to `manifest.json`.
4. Change the manifest namespace and the stage ID together.
5. Edit `starter-map.json` in Tiled without changing the required object classes.
6. Add, remove, or rename spawner points and make the same change in `content/stage.json`.
7. Restart the game and select **Tiled Starter Arena**.

The included map uses the neutral runtime shell and supplies its own repeated PNG background and ground art. Functional doors, vendors, cases, switches, lights, notes, keypads, and the altar come from the shared object library rather than Field Day. Platform colliders remain authored separately in the `Gameplay` object layer, so painting a visual tile does not silently create collision.
