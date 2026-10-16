# Codex suggestions and possible next work

Status: planning document, not a release commitment.

This document collects engineering and design suggestions proposed during development. Community-authored requests remain in [player-suggestions.md](player-suggestions.md). Items here should move into an issue, milestone, or implementation plan before development begins.

## Planning principles

- Finish and test existing systems before adding another broad framework.
- Prefer optional behavior that preserves existing saves, stages, and Mod API v1 packs.
- Add reusable authoring support alongside major runtime features.
- Treat Windows, Android, and WebGL as separate verification targets.
- Keep accessibility and low-end-device performance in the acceptance criteria.
- Do not plan around mid-run saving. The current runtime cannot safely serialize an active wave, spawned actors, paired interactions, physics, or transient mod state.

## Recommended order

1. Merge and smoke-test PR #14 on desktop and Android.
2. Add enough authored special waves for the system to affect normal play outside FER.
3. Complete Android control layout and safe-area support.
4. Add optional combat diagnostics for players and mod authors.
5. Build a special-wave authoring interface and compatibility report.
6. Expand vanilla animation selection and fallback behavior.
7. Add seeded Randomizer runs and useful end-of-run summaries.
8. Improve mod conflict reporting and release automation.

## Immediate release and verification work

### Complete PR #14 verification

**Goal:** Verify the special-wave, Android-input, and Core-character-export changes before treating them as release-ready.

**Work:**

- Trigger FER's Animatronic Spotlight naturally and through the Developer Toolkit.
- Verify wave-number, chance, cooldown, and signal-trigger paths independently.
- Confirm Abby, Jenny, and Sunny appearances load without missing-bone warnings.
- Confirm ordinary wave generation resumes after the special wave.
- Verify the announcement and active-wave HUD text at supported aspect ratios.
- Run **Export Every Core Character to DragonBones** and inspect all 21 combat enemies, Betsy, and Alex.
- Test an Android build with no controller, with a connected controller, after disconnecting it, and after returning from the background.

**Acceptance criteria:** No empty special wave, no stuck wave state, no missing mobile controls, and every exported character has a skeleton, sidecar, images, animations, and index entry.

**Risk/effort:** Low implementation risk; medium hands-on testing effort.

### Release-candidate checklist

**Goal:** Make release promotion repeatable instead of relying on memory.

**Work:**

- Produce Windows, Android, and WebGL artifacts from the same commit.
- Run archive-content checks to exclude debug symbols, Unity caches, local mods, authoring exports, and user saves.
- Record hashes, test results, known limitations, and tester devices in release evidence.
- Verify the update prompt from both the stable and testing channels.
- Confirm clean install, upgrade, rollback, and first-launch behavior.

## Special-wave expansion

### Stage-specific wave library

**Status:** Initial two-wave Core roster implemented; hands-on balance and performance tuning remain.

**Goal:** Turn the special-wave framework into visible gameplay content.

**Proposed content:**

- Shack: close-range rush, low-ammunition defense, or a single durable attacker.
- Cave: crawler swarm, darkness/limited-visibility wave, or alternating tunnel spawns.
- Jungle: flying swarm, ambush wave, or ranged-enemy pressure.
- Space Station: security lockdown, ranged squad, or hazard-assisted wave.
- FER: Animatronic Spotlight, single-character showcase, and escalating full-cast event.
- Field-Day: controlled tutorial variants used to test one mechanic at a time.

**Implementation:** Keep stage definitions data-driven. Reuse appearance overrides where rigs are compatible, and add explicit presentation fields for announcement text, sound, lighting, and reward rather than hard-coding stage names.

**Acceptance criteria:** Every standard stage has at least two special waves; the same special wave cannot repeat inside its cooldown; empty or incompatible entries fall back to an ordinary wave.

**Risk/effort:** Medium. Content tuning and low-end performance are larger risks than the trigger system.

### Special-wave rewards and modifiers

**Goal:** Make special waves desirable as well as dangerous.

**Possible fields:** Bonus currency, ammunition drop chance, healing drop chance, score multiplier, enemy speed/health/damage modifiers, restricted spawn areas, and a completion reward message.

**Constraints:** Rewards must be capped, deterministic after wave selection, and unable to duplicate through pause/reload transitions.

### Special-wave authoring window

**Goal:** Let authors build and test waves without hand-editing JSON.

**Work:**

- Dropdowns for stage, trigger type, signal, selection mode, enemy, and appearance.
- Inline validation for missing content IDs, incompatible rigs, invalid weights, and impossible cadence settings.
- Estimated enemy count and performance warning based on the stage's normal wave curve.
- Buttons for queueing the entire wave, one selected entry, or a signal trigger.
- Read-only preview of the resulting JSON before saving.

**Acceptance criteria:** A new valid special wave can be created, tested, saved, and packaged without manually editing its definition.

## Randomizer improvements

### Seeded and shareable runs

**Goal:** Allow players and testers to reproduce the same randomized run.

**Implementation:** Generate a run seed when the mode starts and derive loadout selection and optional modifiers from a dedicated deterministic random stream. Display and copy the seed from the pause and results screens.

**Acceptance criteria:** The same game version, enabled content set, rules, and seed produce the same starting loadout and scheduled modifiers.

### Run configuration

**Options:** Timer versus wave objective, difficulty preset, reroll count, loadout refresh per round, weapon-category exclusions, guaranteed healing, and optional enemy modifiers.

**Guardrails:** Always produce at least one usable weapon and a valid ammunition path. Never select disabled, developer-only, or unavailable content.

### End-of-run summary

**Display:** Seed, waves survived, elapsed time, starting and final loadouts, rerolls used, damage dealt/taken, enemies defeated, accuracy where measurable, and special waves completed.

**Value:** Supports balance reports and turns failed runs into useful feedback.

## Android and input improvements

### Customizable touch layout

**Goal:** Support different hand sizes, screens, and accessibility needs.

**Work:**

- Edit mode for dragging and resizing touch controls.
- Reset-to-default and left-handed presets.
- Independent opacity and scale controls.
- Safe-area clamping for notches, rounded corners, gesture bars, and cutouts.
- Store normalized anchors rather than raw pixels so layouts survive resolution changes.

**Acceptance criteria:** Controls cannot be moved entirely off-screen, remain usable after orientation/resolution changes, and can always be reset without editing files.

### Input-source transitions

**Goal:** Make touch/controller switching predictable.

**Work:**

- Track actual controller activity rather than device presence.
- Restore touch controls immediately after controller disconnect or touch interaction.
- Prevent stick drift from permanently claiming the active-input state.
- Keep QTE prompts synchronized with the last meaningful input.

### Low-end mobile preset

**Options:** Reduced particles, light count, ragdoll duration, simultaneous decals, animation sampling, post-processing, and maximum active enemies.

**Acceptance criteria:** The preset changes presentation rather than game rules and does not make waves impossible to complete.

## Combat information and balancing

### Optional damage numbers

**Goal:** Help players understand weapons and help modders validate balance changes.

**Options:** Off, player damage only, all combat damage, healing, critical/status indicators, aggregation of rapid hits, and adjustable lifetime/scale.

**Implementation notes:** Pool UI objects, cap simultaneous labels, combine automatic-weapon hits within a short window, and avoid world-space canvases per enemy.

### Enemy health display

**Possible presentation:** Targeted-enemy bar, compact bars above damaged enemies, boss bar for special waves, or developer-only detailed overlay.

**Acceptance criteria:** Off by default for the classic presentation, readable on mobile, and no measurable allocation spike during dense waves.

### Balance telemetry without data collection

Add a local run report that can be copied voluntarily. Include content IDs, calculated damage, modifiers, and wave details, but exclude usernames, personal paths, hardware identifiers, saves, and background telemetry.

## Animation and interaction work

### Multiple vanilla animation variants

**Goal:** Reduce repetition and support alternate grabs without replacing an enemy's entire controller.

**Data model:** Semantic action, eligible player/enemy states, approach direction, stage tags, weight, cooldown, required bones, and fallback semantic.

**Runtime behavior:** Filter incompatible variants, avoid immediate repeats, select deterministically when required, and return to the existing vanilla clip if no variant is valid.

### Jacky alternate interaction

Treat this as the first validation case for the general variant system rather than a one-off branch. It should exercise conditional selection, authoring preview, required-bone validation, and fallback behavior.

### Rig compatibility report

**Goal:** Explain whether one appearance can safely use another actor's animations.

**Report:** Matching bones, missing required bones, extra harmless bones, renderer matches, paired-animation anchors, attachment points, sorting metadata, and clips that will fall back.

**Use cases:** Faceless mannequins, retextures, alternate anatomy, Betsy, Alex, and future player replacements.

### Player and NPC asset-slot catalogs

Extend the searchable public slot catalog beyond combat enemies. Cover Alex, Betsy, clothing variants, anatomy, effects, animation-only sprites, duplicate renderers, and paired-animation-specific artwork.

## Mod management and authoring

### Conflict and load-order viewer

**Goal:** Replace silent last-writer-wins behavior with an understandable report.

**Display:** Conflicting packs, shared content ID or asset slot, selected winner, reason, dependency relationship, and whether reordering is safe.

**Controls:** Reorder independent packs, disable one pack, copy diagnostics, and return to the last known valid order.

### Better diagnostics

Add a **Copy diagnostic report** action containing the game version, platform, enabled pack IDs/versions, validation issues, and sanitized stack information. Remove user directory names, tokens, save content, and unrelated system information.

### Audio replacement support

Define stable slots for enemy voices, weapons, ambience, music cues, and UI sounds. Add preview, duration/channel limits, supported-format validation, and volume guidance. Avoid allowing arbitrary streamed URLs at runtime.

### Map-authoring components

Add reusable authored objects for doors, buttons, traps, checkpoints, wave signals, hazards, spawn regions, lighting zones, and ambience. Provide Tiled templates and validation so object names do not become undocumented runtime APIs.

### One-click mod test session

From an authoring profile, validate the pack, copy it into an isolated test location, launch the requested stage/mode, and show the content IDs loaded. Never overwrite the author's source pack or normal player saves.

## Additional gameplay systems

### Elite variants

Create data-driven modifiers such as armored, fast, regenerating, volatile, support, or resistant. Reuse existing actors with strong visual indicators. Define mutually exclusive combinations and strict multiplier caps.

### Boss and miniboss waves

Build these on special-wave presentation and phase triggers. Require telegraphed attacks, bounded add spawning, clear completion conditions, and recovery if a phase actor fails to spawn.

### Challenge modes

Potential modes include one-weapon, limited ammunition, no healing, endless survival, time attack, stage gauntlet, and daily seeded Randomizer. Keep them as base-game modes when they change progression or scoring; use mods for optional content packs.

### Training room

**Features:** Spawn any registered enemy, queue a wave, select an appearance, toggle invulnerability/ammunition, play QTEs, view hit zones, display damage, slow time, and reset the room.

**Constraint:** Training actions must not write normal progression, achievements, or survival records.

### Environmental hazards

Support bounded definitions for electrified floors, crushing doors, gas zones, alarms, moving platforms, and stage traps. Hazards should declare damage, cadence, warning time, affected layers, and optional special-wave signals.

### Expanded enemy behaviors

Possible modules include ally support, healing, shielding, retreat/reposition, hazard avoidance, noise reaction, coordinated attacks, and stage-specific patrols. Add one module at a time with deterministic tests rather than creating an unrestricted scripting surface.

## Presentation and accessibility

### UI and control accessibility

- Full controller rebinding and conflict detection.
- Stick sensitivity, aim acceleration, and independent deadzones.
- Hold/toggle choices for sustained actions.
- UI scale independent of game resolution.
- Reduced flashing, reduced screen shake, and high-contrast prompts.
- Color-independent status and rarity indicators.
- Subtitle/caption support for important audio cues.

### Localization foundation

Move player-facing runtime strings into stable keys, add font fallback, test text expansion, and keep mod-provided localization namespaced by pack. Do not begin translation until menus can handle longer strings without overlap.

### Wardrobe and appearance improvements

Improve filtering, categories, previews, color selection, compatibility explanations, and controller/mobile navigation. Custom colors should use bounded palettes or explicit channels so recoloring does not damage shaded pixel art.

### New hair and appearance art

The animation support exists, but new longer/moving hairstyles still require authored sprites, pivots, sorting tests, and clothing/headgear compatibility. Treat new fur patterns and skin tones similarly: artwork and compatibility testing are the remaining work, not only code.

## Platform, performance, and maintenance

### Linux support

**Work:** Produce a native build, correct case-sensitive paths, verify executable permissions and ZIP structure, test persistent mod storage, validate controller mappings, and run the full smoke matrix on at least one common distribution.

### Xbox Developer Mode investigation

Begin with a feasibility audit covering Unity target availability, filesystem restrictions, package signing, controller-only navigation, mod installation limitations, and distribution policy. Do not advertise support until deployment and persistence work on real hardware.

### Performance budgets

Establish budgets for active enemies, sprite memory, particles, lights, decals, audio voices, and UI labels. Add warnings to authoring tools and profile FER/special waves on representative low-end Android hardware.

### Save resilience

Although active mid-run saving is out of scope, improve between-run safety with automatic backups, schema/version diagnostics, recoverable migration, corrupt-save detection, and a user-visible restore flow.

### Automated release pipeline

Build and validate Windows, Android, and WebGL artifacts from the same commit. Generate hashes and evidence, inspect archive contents, publish testing-channel metadata, and require manual smoke-test signoff before stable promotion.

### Stable and testing update channels

Let users explicitly choose stable or testing builds. Display the channel in the update banner, prevent silent downgrades, and provide clear recovery when release metadata or downloads fail validation.

## Lower-priority research

- Local daily challenges without requiring an online account.
- Replayable run-event logs for debugging desynchronization or balance reports.
- Achievement/challenge definitions that remain isolated from modded runs.
- More expressive but still bounded stage sequencing beyond signals and special waves.
- Optional allied-enemy behaviors with clear ownership, targeting, reward, and save rules.
- A safe numerical status-effect framework shared by weapons, hazards, clothing, and enemies.

These research items should not delay content, Android usability, or release verification.
