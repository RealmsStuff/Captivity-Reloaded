# Mod API changelog

This page records changes to the public mod contract: JSON fields, content IDs, asset slots, packaging, validation behavior, and supported authoring workflows. Game changes that do not alter that contract do not belong here.

## Current line: Mod API v1

Mod API v1 is still being prepared for its first stable release. The schemas and runtime implementation are usable for development, but the [v1 release checklist](v1-release-checklist.md) remains the authority on unfinished verification.

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
