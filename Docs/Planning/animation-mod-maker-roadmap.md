# Animation and mod-maker roadmap

This work is deliberately ordered around one shared normalized format. Unity remains the reference renderer and VFX/event authoring tool; animation motion is imported or retained from the selected animation rather than authored in a custom Unity rig editor.

## Current direction

- [x] Use DragonBones as the optional external bone-animation editor while keeping normalized Captivity JSON as the runtime format.
- [x] Replace the custom bone-animation workspace entry point with an Animation VFX Timeline.
- [x] Add an effect library, bone attachment, particle properties, live scrubbing, frame-snapped triggers, validation, and JSON saving.
- [x] Add pack-local particle textures and texture-sheet region preview.
- [x] Add dedicated audio, hitbox, camera, light, trail, and decal lanes alongside particles.
- [x] Add copy/paste presets and a small library of safe starter effects.
- [x] Add an automated original-enemy and paired-animation DragonBones round-trip regression test.
- [x] Add an opt-in generated gameplay test stage with repeatable spawn, knockdown, Expose, finisher, reset, and live runtime diagnostics.

## 1. Core semantic inventory

- [x] Inventory all 21 Core enemy Animator controllers and interaction sets.
- [x] Assign reviewed semantic names to every state without renaming Unity assets.
- [x] Preserve layered animation meaning and mark empty controller plumbing as non-authorable.
- [ ] Regenerate and visually review the schema-version-3 catalog in Unity.

## 2. Representative normalized exports

- [x] Add selected-clip normalized export from the preview.
- [x] Add batch export for representative idle, movement, primary attack, layered, and first-finisher clips.
- [x] Preserve supported transform, sprite, color, sorting, attack-event, source, and warning data.
- [ ] Review unresolved bone names and Unity events in the generated files.
- [ ] Freeze normalized Core enemy rig maps after representative exports expose the differences between rigs.

## 3. Unity authoring window

- [x] Add a Core enemies browsing and playback mode.
- [x] Show semantic names beside raw Unity clip names.
- [x] Export the selected enemy clip from the graphical window.
- [x] Add editable keyframes, event/effect markers, validation, undo, and save-as-pack workflow.
- [x] Preview imported external animation JSON directly rather than only Unity clips.
- [x] Discover a selected pack's enemies and linked animations, create starter clips, and author numeric and sprite-region tracks from rig-aware dropdowns.
- [x] Add a Blender-inspired design workspace with an Outliner, interactive rig viewport, transform inspector, keying shortcuts, and multi-lane Dope Sheet.

## 4. Standalone graphical mod-maker

- [ ] Choose the packaging shell after the normalized rig review. The renderer must support pixel-perfect sprites, hierarchy transforms, sorting, scrubbing, and garment overlays.
- [ ] Build pack creation, manifest editing, content forms, asset import, validation, and ZIP packaging around the same public schemas.
- [ ] Add enemy/player animation timelines only after Unity and runtime round-trip tests agree.
- [ ] Keep LibreSprite and Tiled as dedicated sprite/map editors and link their templates instead of recreating them.

## 5. In-game gallery

- [ ] Add a developer/gallery entry that uses real runtime loaders and the existing Captivity UI.
- [ ] Browse player, Core enemy, paired-finisher, and loaded-mod animations.
- [ ] Support clothing/body variants, playback controls, facing, and diagnostic labels.
- [ ] Treat this as final runtime verification, not as the primary animation editor.
