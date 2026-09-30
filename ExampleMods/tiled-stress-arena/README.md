# Tiled stress-test arena

The installed copy is configured as a runtime stress test: its 256-tile-wide arena contains 15 uniquely named enemy spawners and begins with 60 scheduled enemies. Captivity still enforces its normal 40-live-enemy cap, so remaining enemies enter as room becomes available.

Spawner object names must remain unique and must match an `id` in `content/stage.json`. Duplicating a Tiled spawner without renaming it makes the pack invalid.

The `room.tx`, `room-entry.tx`, and `room-transition.tx` templates provide optional in-stage multi-room camera and teleport progression without checkpoints.

This directory is the source-controlled release stress fixture and an editable Tiled example. Open `levels/starter-map.json` in Tiled. For a lightweight authoring starting point, use `ModSDK/MapTemplates/TiledStage` instead.

## Supported map settings

- Orientation must be **Orthogonal**.
- Infinite maps must be disabled.
- The example uses 32 by 32 tiles, `pixelsPerUnit: 32`, and a 256 by 96 tile canvas. That matches the approximate width of Space Station and exceeds Cave's vertical platform span, the widest and tallest existing combat-stage measurements.
- Save/export as JSON inside the mod pack. The stage schema currently requires a `.json` path.
- Tiled coordinates start at the upper-left and increase downward. The loader converts them to centered Unity coordinates automatically.

## Visual tile layers

Visible tile layers now render in their Tiled order. The V1 preview supports:

- Raw numeric tile arrays in JSON (Tiled's default JSON output); base64/compressed layer data is not supported.
- Embedded JSON tilesets and external `.json`/`.tsj` tilesets stored inside the same mod pack.
- PNG spritesheets with margin and spacing.
- Pack-local PNG image layers, including Tiled's horizontal and vertical repeat flags.
- Layer visibility, opacity, nested group offsets, and horizontal/vertical tile flips.
- Native Tiled tileset animations with 2 to 256 frames and per-frame durations from 16 to 60000 milliseconds. Animated tiles work in tile layers and tile-object decorations.

Export external tilesets as Tiled JSON (`.tsj`), not XML (`.tsx`). Tiles may be larger than the map grid and use Tiled's bottom-left alignment. Diagonal flips are experimental and currently render without their diagonal transform.

Set `layout.hideInheritedVisuals` in `content/stage.json` to `true` when the tile layers replace the Core template's artwork. Leave it false or omit it when the new layers should overlay inherited scenery.

## Gameplay objects

Create an object layer, conventionally named `Gameplay`, and use these classes/types:

| Class | Shape | Requirements |
| --- | --- | --- |
| `platform` | Rectangle | Positive width and height; becomes a `BoxCollider2D` and navigation platform. |
| `player-spawn` | Point | Exactly one per map. |
| `enemy-spawner` | Point | One or more; every name must be non-empty and unique. |
| `decoration` | Tile object | A non-empty tile object selected from a JSON/TSJ tileset; renders without collision or behavior. |
| `weapon-vendor` | Point | Clones the inherited stage's fully functional weapon vending machine at this position. |
| `usable-vendor` | Point | Clones the inherited stage's fully functional medicine/item vending machine at this position. |
| `weapon-case` | Point | Creates a purchasable weapon case. Set `weapon` to a Core or mod `item/weapon/...` content ID and `caseSize` to 1, 2, or 3. |
| `door` | Point | Clones a Core door. Give it a unique object name and set `doorType` to `standard`, `jacky`, or `roller`. |
| `door-switch` | Point | Toggles the doors named by its comma-separated `targetDoors` property. |
| `core-art` | Rectangle | Places and scales an approved Core background or visual prop selected by its `art` ID. |
| `note` | Point | Opens the Core reading HUD with custom `text` and `fontSize`. |
| `keypad` | Point | Opens the controller-compatible keypad HUD and opens the doors in `targetDoors` when its numeric `code` is entered. |
| `altar` | Point | Clones the complete Jungle altar, including fetish placement, wave activation, effects, and Altar Spirit release. |
| `interaction` | Point or rectangle | A use, touch, or shoot node with conditions and composable actions. Optional `visualArt` makes it a visible Core-art machine. |
| `machine` | Point/tile object | A readable alias for a use interaction. Use the purchase and healing templates as starting points. |
| `logic-switch` | Point/tile object | A readable alias for a use interaction that controls other named objects. |
| `easter-egg-step` | Point/tile object | A single-use step that can require earlier named interactions and activate later steps. |
| `light-bulb` | Point | A usable, shootable Core light with starting state and flicker controls. |
| `pickup` | Point | Spawns a registered weapon, usable, or consumable by full content ID. |
| `moving-platform` | Rectangle | A visible kinematic platform with pixel offset, travel/pause timing, and approved Core art. |
| `particle-emitter` | Point | Creates bounded looping particles with configurable color, rate, lifetime, speed, size, and radius. |
| `nav-node` | Point | Adds a named directed move/climb link, optionally marked for flying enemies. |
| `ambient-audio` | Point | Loads a pack-local WAV/OGG as non-positional ambience. |
| `audio-source` | Point | Loads a pack-local WAV/OGG as positional SFX with linear distance falloff. |

An `enemy-spawner` object's name must exactly equal one `spawners[].id` in `content/stage.json`. Every JSON spawner needs a Tiled point and every Tiled spawner point needs a JSON entry. Enemy lists, timing, minimum wave, weighting, and out-of-sight behavior remain in the stage JSON.

Platforms accept Boolean `climbableLeft` and `climbableRight` custom properties. Enable them on exposed ends that the player should grab. The starter leaves the long ground edges disabled and enables both ends of its three elevated platforms.
The moving-platform template enables both grab edges as well; override either property to `false` for a side that must not be climbable.

The `templates` directory contains reusable Tiled object templates. Vendors, machines, and switches use small redistributable preview PNGs, so they are visible and placeable as normal tile objects inside Tiled while still becoming their proper Core/runtime object in game. Native `.tx` template references are resolved at load time, including instance name, position, size, and property overrides. Templates must remain inside the same mod pack; cycles, missing files, and paths escaping the pack are rejected. You can also draw objects manually and set their Class field. Nested Tiled group offsets are supported.

For a decoration, create a Tile Object from a tileset and set its Class to `decoration`. Position, width/height scaling, clockwise rotation, visibility, object-layer opacity, draw order, and horizontal/vertical flips are carried into the game. The starter's `Decorations` layer includes a 64 by 64 lamp example.

Decorations accept an optional Boolean `repeat` property. When enabled, the sprite tiles across the object's rectangle instead of stretching; the 252-unit starter floor uses this. The optional `sortingLayer` string accepts `Background`, `Decoration`, or `Platform`.

Vendor points use the shared runtime vendor templates and the existing UI, including wave enable/disable behavior and the loaded mod item/weapon catalogs. The arena places both vendor types near its center without inheriting a playable Core stage.

Weapon cases use the shared runtime template's purchase and reload behavior. The arena includes a size-1 case containing `core:item/weapon/revolver-44`; change the `weapon` property to an installed additive weapon's full content ID to offer that weapon instead.

Doors use shared runtime templates. `standard` is the normal hinged door, `jacky` opens like its Core counterpart and applies the delayed curse, and `roller` uses the Space Station rolling animation. `initiallyOpen` defaults to false. An unlinked roller door opens automatically when an actor enters its `proximityRadius` (default 8); linking it from a `door-switch` makes it switch-controlled instead. A switch may target several doors, for example `west-door,east-door`.

Pack-local PNGs can reskin vendors and roller-door panels with `visualFile`, weapon cases with `visualFile` and `brokenVisualFile`, standard/Jacky doors with `closedVisualFile` and `openVisualFile`, and switches with `offVisualFile` and `onVisualFile`. Stateful pairs must both be supplied; `visualPixelsPerUnit` defaults to `32`. Light bulbs already support `visualFile` and `activatedVisualFile`.

The starter arena places all three door types and a separate switch for each one. These are runtime objects, not decorative tiles, and preserve their original sprites, sounds, collision, and interaction behavior.

The starter also places a readable code note and a keypad beside the standard door. Enter `1234` to open it. Both objects use their original game HUD and controller handling.

The paid secret button demonstrates a visible machine-style interaction graph using the FER brain-machine art. Beginning on wave 1, while no more than 20 enemies are alive, a player carrying the nearby revolver may spend 25 money to open the Jacky door, toggle a named light, queue two enemies at a named spawner, and receive an ammo box after a short delay. The revolver requirement is not consumed in this example. Set `consumeRequiredItem` to true for key items or ingredient exchanges; unsupported content IDs, challenge prerequisites, and interaction cycles are validated before play.

Interaction conditions also support `requiredMoney` (checked but not spent) and `requiredInteractions` (a comma-separated list of named steps that must already have fired). New actions include `restoreHealth`, `knockbackX`, `knockbackY`, and `ragdollSeconds`. This supports healing or purchase machines, launch pads, switches, ordered Easter eggs, and reward sequences without custom C#.

The starter also includes quiet ambient and positional machine-hum sources. Replace the small demonstration OGG files under `assets/audio` with your own WAV/OGG files. Positional sources load when the stage opens and stop when it closes.

The optional top-level `audio` object in `content/stage.json` supplies `ambience`, `entryMusic`, `waveMusic`, and `waveComplete` from pack-local WAV or OGG files. This neutral shell has no inherited clips, so omitted slots remain silent.

The Jungle altar example is at world X `64`. It is rebound to the authored stage's wave manager and global light instead of retaining references to the original Jungle stage.

`core-art` lets maps reuse stable, curated game artwork without copying the source PNG into the mod. Size the rectangle to the desired in-game footprint and select an ID from `reference/core-map-art.json`. Optional `sortingLayer` values are `Background`, `Decoration`, and `Platform`; `flipX` and `flipY` mirror the art. Core art is visual-only and does not create collision. The starter's far-right area demonstrates a Space Station bay background, poster, barrel, and chair.

For a background that must be visible while editing, add a normal Tiled Image Layer and select a PNG inside the mod pack. The starter uses `assets/space-bay-background.png`, aligned to the ground and repeated horizontally across the arena. Image layers render on Unity's `Background` sorting layer and honor visibility, opacity, group offsets, and `repeatx`/`repeaty`.

See `reference/core-stage-bounds.md` for the measured Core stage footprints used to size this template.

## Release test

1. Validate this directory with the schema and Tiled verification tools.
2. Install only this example and restart the game.
3. Select **Tiled Stress-Test Arena** and complete at least the opening wave.
4. Verify the 15 spawners, 60-enemy schedule, live-enemy cap, navigation links, moving platform, doors, machines, vendors, particles, audio, and scripted interaction.
5. Repeat in each release player build and record frame-rate, memory, exceptions, and gameplay failures.

The included map uses the neutral runtime shell and supplies its own repeated PNG background and ground art. Functional doors, vendors, and other objects come from the shared object library. Platform colliders remain authored separately in the `Gameplay` object layer, so painting a visual tile does not silently create collision.
