# Implementation status

## Implemented and exercised

* Pack discovery, stable IDs, dependencies, ordering, conflicts, and enable state
* Asset-slot patches
* Inherited enemies, clothing, weapons, and usables
* All 79 Core challenge behaviors through inheritance, 15 general data-driven challenge objectives, difficulty profiles, and selectable game modes with weapon, spawn, wave, economy, and player rules
* Hybrid Core catalogs backed by existing Unity prefabs

## Implemented but experimental or awaiting broader runtime verification

* The Tiled authoring pipeline: visual layers, animated tiles, pack-local audio, gameplay templates, loose pickups, moving platforms, particles, explicit navigation links, visible machine art, and validated interaction graphs
* Enemy `spawnOnDeath` and `speedPulse` behavior modules have passed their initial gameplay confirmation but remain experimental for v1
* Original-enemy animation event tracks support animation-timed hits, multi-hit attacks, bounded facing-relative impulses, positional audio, camera shake, atlas sprite effects, and inert semantic authoring cues
* Weapon charge, beam, alternate-fire, melee, physical-projectile, presentation, and sprite-animation modules; core mechanics have been exercised, while the revised charge/recoil feedback needs a regression pass
* Clothing attachment, tearing, armor, and equipped-stat overrides
* Normalized player and original-enemy animation import, including transform, sprite, color, sorting, safe-event, and bounded particle tracks; enemy rig/animation schema version 1 is frozen
* Unity Core-enemy preview and original-skeleton mod-enemy preview, source/JSON comparison, pack discovery, clip creation, rig-aware numeric/sprite tracks, event/effect markers, live validation, Undo/Redo, and guarded save-to-mod tooling

## Outside the stable v1 scope or still in migration

* Fully loose Core artwork, audio, colliders, projectiles, and specialized mechanics
* Core enemy rigs and all public semantic clips can be exported as packaged normalized definitions; vanilla runtime enemies still use prefab controllers as a compatibility fallback while specialized attack and AI parity is completed
* Arbitrary new scripted machine behavior and custom Unity components
* A standalone graphical mod-maker

See the [v1 release checklist](v1-release-checklist.md) for the verification and publication work that remains. This page describes capability, not a release promise.
