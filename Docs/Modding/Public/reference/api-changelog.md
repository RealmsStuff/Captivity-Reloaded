# Mod API changelog

This page records changes to the public mod contract: JSON fields, content IDs, asset slots, packaging, validation behavior, and supported authoring workflows. Game changes that do not alter that contract do not belong here.

## Current line: Mod API v1

Mod API v1 is still being prepared for its first stable release. The schemas and runtime implementation are usable for development, but the [v1 release checklist](v1-release-checklist.md) remains the authority on unfinished verification.

### 2026-11-07: pack-authored player clip effects

- Player animation effects may now be the pack's own. Experimental: an effect's `source` may be `{ "authoring": "..." }` (the file or tool the pack made it with, 1 to 260 characters) instead of the Core `{ "unityPrefab", "hierarchyPath" }`; `trigger` may be any short name (a lowercase letter, then letters, digits or hyphens, at most 40) such as `drip` or `sweat`, not only `thrust`, `cumThrust` or `unique`; and an effect trigger's `source` may be a pack label with up to eight `+`-joined beat names, such as `pip:splash` or `pip:slam+cum+overflow`, not only the three `enemy:` Core events.
- The runtime still fires effects by trigger time only; these fields are labels.
- New validation codes `player-animation.effect-trigger-name`, `player-animation.effect-trigger-source` and `player-animation.effect-source` for values outside those shapes. The schema rejected them before, so valid packs are unaffected.

### 2026-11-07: finisher phases without struggle

- Added optional experimental `struggle` on `downedFinisher` phases. `false` ends the escape window: the struggle HUD closes, the `succumbed` status card appears, and the remaining phases play out without input before the failure outcome, like a Core grab after the player has lost.
- New validation codes `enemy.behavior.finisher-phase-struggle` (a struggling phase after a `struggle: false` one) and `enemy.behavior.finisher-phase-struggle-first` (the first phase has `struggle: false`).
- The HUD timer counts the struggling phases only. Finishers without the field behave as before.

### 2026-11-07: grab reactions for finisher phases

- Added the optional experimental phase field `reactions` to `downedFinisher` phases. It gives a phase what a Core grab animation sets up for its thrusts: thrust and cum-thrust gains, struggle knock-back, sounds, player and enemy voice lines and sound layers rolled by chance, the oral mouth, muting the player and a camera on the enemy's hips. See [Grab reactions](../content/enemies.md#grab-reactions-experimental).
- In a phase with `reactions`, the phase's clips (the enemy's and its participants') act on the cues `finisher.thrust` and `finisher.cum-thrust` (`amount` is the power, 0 to 3, which picks the Core camera shake), `audio.player-voice-random`, `audio.enemy-voice-random`, `audio.unique` (`index`) and `finisher.ko`. Each thrust also opens the player's mouth and flushes the screen as a Core thrust does. Cues stay inert in phases without `reactions`, so existing packs are unchanged.
- Reaction sounds are validated like `sound` event files (pack-relative WAV or OGG, at most 32 per list, at most 8 layers) and loaded with the enemy.
- New validation codes: `enemy.behavior.finisher-reactions-camera`, `enemy.behavior.finisher-reactions-layers`, the `enemy.behavior.finisher-reactions-<list>-sound` and `-sound-count` checks, and the `enemy.stats.finisher-reactions-<set>-pleasure`, `-libido`, `-struggle-damage`, `-chance` and `-volume` range checks.

### 2026-11-07: finisher start freeze

- Added optional experimental `downedFinisher.startFreezeSeconds` (0 to 1). When an original enemy's finisher starts, the game holds still for that long, like the half second before a Core grab.
- New validation code `enemy.stats.finisher-start-freeze` for values outside 0 to 1.
- Finishers without the field start as before.

### 2026-11-06: extra attacks on Core enemies

- Inherited enemies take an optional experimental `additionalAttacks` list (at most 8): new attacks in the original-enemy attack format (`melee`, `hitscan`, `projectile`, `area`, `multiStage`; not `grab`), each playing a clip from the enemy's `animationRefs` on its Core template's rig. Clips used only by additional attacks may carry `attackHit` and `impulse` events. On a Core rig `projectileRegion` names a bone, whose current sprite the projectile shows.
- The Core AI keeps its own attacks; at most once a second the enemy picks between its Core attack and the additional attacks in range by `chance`. An additional attack holds the Core AI while it runs, lands hits through the same delivery as original enemies' attacks, and shares one cooldown with the Core attacks. The Death Hound waits out its charge and the Goblin Marksman its shooting.
- New validation codes `enemy.additional-attacks.original`, `.limit`, `.entry`, `.id`, `.index`, `.grab`, `.type`, `.animation`, `.miss-animation`, `.hit-time`, and `core-extra-attack.hit-timing`, `.stage-events`, `.event-after-attack`, `.projectile-region`.
- Packs using `additionalAttacks` need a game build with this feature and do not load on 2.2.2.

### 2026-11-06: animating Core enemies

- Asset patches on a Core enemy (`core:enemy/<slug>`) take optional experimental `animations` (semantic clip name -> `enemyAnimation`) and `playerAnimations` (`finisher-phase-N` -> `playerAnimation`); `replacements` is now optional when either is present. They replace that enemy's clips everywhere it appears. Conflicts follow the asset patch rules with the slot IDs `<target>/animation/<semantic>` and `<target>/player-animation/finisher-phase-N`.
- Inherited enemies' `animationRefs` now play: each maps one of the template's semantic clip names to the variant's own `enemyAnimation`. Added optional experimental `playerAnimationRefs` (inherited enemies only) for the player's side of the template's grab phases.
- A pack's `enemyAnimation` may now set `enemy` to a Core enemy (`core:enemy/...`); its tracks name that Core rig's published bones.
- The Core enemy's Animator keeps running; after it, the override is sampled at the state's normalized time scaled to its own length and written over the bones (position, rotation, scale, sprite, sorting order, colour). The original clips' Unity events still fire. Overrides may carry `sound` and `cameraShake` events and normalized effects.
- New validation codes `core-animation.semantic`, `core-animation.phase`, `core-animation.bone`, `core-animation.event`, `core-animation.owner`, `core-animation.enemy`, `core-animation.length` (warning), `core-animation.player-events` (warning), `core-animation.audio-file`, `asset-patch.animation-target`, `asset-patch.animation-name`, `asset-patch.animation-id`, `asset-patch.player-animation-name`, `asset-patch.player-animation-id`, `enemy.player-animation-refs.original`, `enemy.player-animation-refs.name`, `enemy.player-animation-refs.id` and `enemy.player-animation-refs.limit`.
- Packs without these fields are unchanged. Inherited enemies with `animationRefs` used to fail with `enemy-animation.bone-missing`; they now load.

### 2026-11-06: several finishers per original enemy

- An original enemy may now have several `downedFinisher` behavior modules. Each takes an optional experimental `id` (required and unique when there is more than one) and `weight` (0.001–1000, default 1).
- Added optional experimental `finisher` to original-enemy attacks: the id of the finisher a `grab` attack (or an attack with a `grab` stage) starts. A grab without it picks a finisher by weight; a downed or exposing player gets one picked by weight among the finishers in range.
- The finishers share one cooldown: after any of them ends, none starts until that one's `cooldownSeconds` have passed.
- New validation codes `enemy.behavior.finisher-id`, `enemy.behavior.module-id`, `enemy.attacks.finisher`, `enemy.attacks.finisher-missing` and `enemy.stats.finisher-weight`. `enemy.behavior.module` now allows a repeated `downedFinisher`.
- Packs with one finisher are unchanged.

### 2026-11-02: finisher attachment and per-phase contacts

- Added the optional experimental `downedFinisher.attachment` block for original enemies. While attached, the runtime holds the enemy's root at `rootX`/`rootY` in a frame on a player bone (`target`, offset by `offsetX`/`offsetY`, turning with the bone by `followRotation`), easing in and out over `settleSeconds`. The finisher's usual side-by-side alignment does not run while an attachment is set.
- Attachment `contacts` hold an enemy hand or foot on a point in the attachment frame, or in a named player bone's frame, with two-bone IK after the clips play. `sortingOrders` and `playerSortingOrders` interleave the two actors' parts while the finisher runs.
- Finisher phases accept `attached` (`false` lets go, `true` holds on again) and their own `contacts`, which replace the attachment's for that phase. Contacts that start, stop or move ease over the phase's `blendSeconds` when set, otherwise over `settleSeconds`; a knee or elbow whose bend changes while held swings across instead of flipping. When the finisher ends, the enemy is released where its clip drew it.
- New validation codes: the range checks `enemy.stats.finisher-attachment-offset`, `enemy.stats.finisher-attachment-follow-rotation` and `enemy.stats.finisher-attachment-settle`; `enemy.behavior.finisher-attachment-target`, `-sorting`, `-player-sorting` and `-phase` (a phase sets `attached` or `contacts` without an attachment); and `enemy.behavior.finisher-contact-bone`, `-target` and `-value`.
- Finishers without `attachment` are unchanged.

### 2026-11-02: miss animations for original-enemy attacks

- Added optional experimental `missAnimation` to original-enemy attacks. When the attack's hit lands on nothing (out of range, out of sight, or a player who cannot be attacked right now), that clip takes over from the same point in its timeline and the attack runs until the clip ends; the attack's cooldown starts then.
- New validation codes `enemy.attacks.miss-animation-missing` (the clip is not one of the enemy's) and `enemy.attacks.miss-animation-inherited` (inherited Core attack overrides reject the field).
- Attacks without `missAnimation` are unchanged.

### 2026-10-24: Core animation reference in builds

- Standalone builds now include `StreamingAssets/CoreAnimationReference`: each Core enemy's `enemyRigReference` and packaged normalized clips, the `playerRig` and its reference clips, and one sprite atlas per rig (`sprites.png` with `sprites.json`). See `ModSDK/AnimationReference/README.md`.
- **Captivity Reloaded > Modding > Export Core Animation Reference...** writes the same folder from the editor.
- It is read-only reference data for tools outside Unity. The game does not read it, and no mod contract changes.

### 2026-10-18: original enemies in Core stage spawners

- Added optional experimental `spawn.joinCoreSpawners`: up to 32 entries, each naming a `core:enemy/...` ID with an optional `selectionWeight` (default `1`) and `minimumWave` (default `0`). The enemy joins every stage spawner that offers that Core enemy as an extra weighted choice from its minimum wave, the way inherited variants join their template's spawners.
- New validation codes `enemy.spawn.join-enemy`, `enemy.spawn.join-duplicate`, `enemy.spawn.join-minimum-wave` and `enemy.stats.spawn-join-selection-weight`; an entry whose Core enemy is not loaded is reported as `enemy.factory-join-template` at load.
- Spawners pick among entries that have reached their minimum wave; Core entries and inherited variants are never gated. Enemies without the field are unchanged.

### 2026-10-18: per-attack knockdown for original enemies

- Added optional experimental `ragdollSeconds` (0–30) to original-enemy attacks and `multiStage` stages. A landed hit knocks the player down for that long and counts as a KO; stages inherit the attack's value and projectiles carry it to their impact.
- Inherited (Core) attack overrides reject the field with `enemy.attacks.ragdoll-inherited`.
- Attacks without `ragdollSeconds` are unchanged.

### 2026-10-16: signal-triggered special waves and HUD identity

- Added optional experimental `triggerSignal` activation for special-wave rules, connected to stage-script signals from doors, buttons, pickups, and authored sequences.
- Signal activation replaces `everyNthWave`; minimum-wave, chance, and cooldown safeguards still apply.
- Special waves now receive a dedicated start banner and remain identified beside the wave number until they end.

### 2026-10-15: special-wave appearance variants

- Added experimental stage `specialWaves` rules with temporary weighted enemy rosters and wave cadence controls.
- Added the optional enemy-entry `appearance` field for copying sprites from an enemy with a compatible named-bone hierarchy while retaining the gameplay enemy's AI, attacks, animations, and finishers.
- Generated appearance templates exist only for the active wave and are destroyed afterward.
- Existing stages and special-wave entries without `appearance` are unchanged.

### 2026-10-15: positioned player climax emitters

- Added the opt-in `playerAttachment.orgasmEmitter` field.
- Emitter-only attachments may omit sprite fields and replay the normal player climax particles at their bone-relative position.
- The original player emitter remains active, allowing body mods to add rather than replace emission points.

### 2026-10-13: expanded Core visual replacement catalog (game 2.2.1)

- Expanded the reviewed legacy-presentation catalog from 352 to 458 Core sprite names.
- Published 417 canonical enemy-specific renderer slots across all 21 Core enemies and 72 compatibility aliases.
- Added isolated multipart feet, anatomy, asymmetric limbs, breast pieces, and animation-only variants so retextures do not need shared global sprite IDs.
- Added remaining high-value keypad/menu controls, doors and room pieces, backgrounds, floors, walls, platforms, furniture, projectiles, beams, fire, and shell artwork.
- Added stable asset-slot registry snapshots for diagnostics and deterministic authoring output.
- Added runtime coverage logging for advertised legacy visual slots and editor checks for duplicate IDs and missing shipped baselines.
- Clarified that Mod API v1 `assetPatch` replacements are PNG-only. General Core audio replacement remains outside the v1 contract.

This is additive: existing pack IDs and replacement slots are unchanged. Packs using the new IDs require game version 2.2.1 or newer.

### 2026-10-11: authoring and packaging clarification

- Clarified that pre-v1 mods must be validated against the exact target game release or source commit.
- Documented that unknown generated fields remain errors even when their names resemble features on another content type.
- Added explicit guidance for original-enemy spawning, player-animation event names, and current unsupported enemy fields.
- Added the required root layout for `.capmod` archives and warned that renaming a manually wrapped ZIP does not create a valid package.
- Added a validation checklist for AI-assisted and custom-generator output.

This clarification does not change the runtime API.

### 2026-10-08: authoring and activation additions

- Added `ruleProfile.activation`; `selectable` remains the default, while `pack` applies the profile whenever its owning pack is loaded without adding a game-mode entry.
- Added fully original `originalClothingAtlas` visuals with author-defined `piece/...` regions.
- Added bounded `idleAmplitude` and `idleFrequency` settings to procedural clothing sway.
- Added explicit `adaptive`, `alternate`, `rotate`, and `tap` finisher input patterns and device-appropriate prompts.
- The Mods panel can now apply pending install, update, enable, disable, rollback, and uninstall changes by reloading the title scene.
- Existing rule profiles, clothing, and finishers keep their previous defaults when these optional fields are omitted.

### 2026-10-08: cursed clothing and enemy attachment

- Added bounded clothing `escapePowerMultiplier` and `bountyMultiplier` effects.
- Applied escape modifiers to Core and modular finisher QTE input and bounty modifiers after rule-profile calculations.
- Added the `onHitEquipClothing` enemy behavior module with chance and cooldown controls.
- Added `equipClothing` lists to finisher success and failure outcomes.
- Forced garments use normal compatibility replacement, are unlocked and persisted, and remain removable in the wardrobe.
- Existing clothing and enemies retain their previous behavior when the new optional fields are omitted.

### 2026-10-07: independent finisher participants

- Added the optional `downedFinisher.participants` list for up to three independent secondary original-enemy instances.
- Added `startOnly` and phase-boundary late-join policies, participant approach/join ranges, shared-frame offsets and facing.
- Added per-phase `participantAnimations` mappings.
- The runtime now reserves participants, lets eligible late enemies approach open slots, synchronizes entry at phase boundaries, and restores AI, attacks, physics, collisions, position, facing, animation, and sorting state when the interaction ends.
- Existing finishers without `participants` retain their previous behavior; no pack migration is required.

### 2026-10-02: reference baseline

- Published field-by-field pages generated from all 19 JSON Schemas.
- Published a source-generated catalog of validation codes.
- Published the complete Core asset-slot catalog, including conditional enemy semantic slots.
- Documented implemented Android and WebGL storage, bundle, and download policies separately from hands-on release verification.
- Synchronized the enemy `inputPattern` contract between runtime parsing and the public schema.

These documentation changes do not increment `modApiVersion`; they describe the existing v1 implementation.

## Versioning policy

- Additive optional fields normally remain within the current API major version.
- A changed default, removed field, renamed content type, incompatible ID rule, or changed required behavior requires an explicit migration entry.
- Experimental fields can change before they are declared stable. Their stability is shown in the [field reference](api-fields.md).
- A pack's `modApiVersion` selects the contract expected by the game. It is not the same as the pack's own semantic `version`.

## Entry format for future changes

Each release entry must include:

1. Release date, game version, and Mod API version.
2. Added, changed, deprecated, and removed contracts.
3. Whether old packs continue to load unchanged.
4. Exact author action, with before-and-after JSON where a file must change.
5. New or retired validation codes.
6. A link to the corresponding [migration guide](migration-guides.md).

No later Mod API version is listed until it exists in the schemas and runtime. This avoids turning planned behavior into an accidental public promise.
