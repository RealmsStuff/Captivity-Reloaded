# Defaults and compatibility

This page defines the Mod API v1 behavior when a pack omits data, supplies unknown data, disappears, or changes between saves. Numeric and collection bounds are normative in the JSON Schemas under `Docs/Modding/schemas`; runtime validation is authoritative for rules involving Core templates, asset files, or other content IDs.

## General rules

* Unknown fields in loader-owned manifests and content documents are errors. The affected pack is disabled; unrelated valid packs continue loading.
* Tiled-owned map, layer, object, and property metadata remains open so Tiled can preserve editor data. Only recognized Captivity object types and properties affect gameplay.
* Missing required fields, invalid enum values, unsafe paths, non-finite numbers, and values outside published bounds are errors. Values are never silently clamped during loading.
* An omitted optional scalar uses the default shown in its schema or inherits from the selected Core template. An omitted optional list is empty unless the content page states that the Core list is inherited.
* Paths are pack-relative, use `/`, cannot be absolute, and cannot traverse outside the pack. Omitting an optional asset field retains the inherited or previously resolved asset. Supplying a path makes that file required; a missing, empty, oversized, unsafe, or undecodable referenced file is an error.
* An invalid explicit asset patch does not erase the earlier valid asset. The previously resolved slot remains active and the loader reports the rejected patch.

## Content fallbacks

| Content | Omitted data and fallback |
| --- | --- |
| Manifest | Optional lists are empty; `priority` is `0`. Required dependency failures disable the consumer. Missing or incompatible optional dependencies only warn. |
| Enemy | Omitted statistics, visuals, attacks, drops, animation, and behavior retain the inherited Core enemy. `spawn.selectionWeight` defaults to `1`; experimental behavior-module defaults are recorded in `enemy.schema.json`. |
| Clothing | Omitted visuals retain the inherited garment. Attachments default to zero offset/rotation, centered pivot, unit scale, and zero sorting offset. Effects default to no armor and no stat changes. |
| Weapon | Omitted statistics and presentation retain the inherited gun. Behavior defaults to one shot, normal sound and muzzle flash, no custom projectile/melee/beam/charge/alternate fire, and the Core animation states. Module-specific defaults are in `weapon.schema.json`. |
| Usable | `effectMode` defaults to `inherit`; an omitted effects list therefore keeps the Core usable effect. Optional visuals and statistics inherit from the Core item. |
| Stage | Description defaults to empty. Core-layout stages use the declared spawn. Tiled stages use their `player-spawn` object. Optional audio slots retain the inherited stage audio. Spawner jitter and initial delay default to `0`, minimum wave to `0`, and out-of-sight spawning to `false`. |
| Challenge | Inherited challenges retain the Core objective when no replacement objective is supplied. New challenges must provide a supported objective, count, and clothing rewards. Optional filters are empty. |
| Difficulty | All declared multipliers are required. If a selected external difficulty is unavailable, selection falls back to `core:difficulty/normal`. |
| Rule profile | Omitted modules do nothing. Module defaults preserve ordinary game values: multipliers `1`, additions/money `0`, wave growth `2`, intermission `30`, unlimited waves `0`, and ammo-drop chance `0.07`. If the selected profile disappears, the game falls back to Standard. |
| Asset patch | Every listed slot is explicit. Invalid or missing replacement files leave the previous slot value intact. |

## Stable IDs and saves

Mod API v1 does not provide automatic aliases for renamed content IDs. A published stable ID is permanent within its pack. Renaming an ID is treated as removal of the old content plus addition of new content.

Save records use full content IDs. If a pack is disabled or missing, its stored records are retained but its content is not instantiated. Re-enabling the same pack and IDs reconnects that data. Authors who must rename content should keep a deprecated definition at the old ID for the duration of v1; automatic aliasing is reserved for a future API revision.

Adding optional fields does not require a schema-version increase when their defaults preserve existing behavior. Removing or renaming stable fields, changing their meaning, or making validation stricter requires an explicit migration and normally a new major Mod API version. Experimental fields may change before promotion and do not carry the stable-v1 compatibility promise.

## Schema and runtime validation

The schemas provide fast structural feedback for authoring tools and continuous integration. The runtime then performs contextual checks such as namespace ownership, referenced content existence, Core-template compatibility, asset dimensions, safe filesystem containment, and cross-field relationships. A document must pass both layers.
