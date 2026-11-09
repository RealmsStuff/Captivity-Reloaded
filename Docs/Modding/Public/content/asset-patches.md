# Asset patches

Use an `assetPatch` when the goal is to replace published PNG artwork without creating new gameplay content. A patch targets a stable content ID and maps its public slot names to pack-local PNG files. Mod API v1 does not expose Core audio through `assetPatch`; pack-local audio remains available on content types that explicitly declare audio fields.

> **Fast path:** inside the Unity project, use **Captivity Reloaded > Modding > Create Simple Retexture...**. The [Simple retexture wizard](../tools/simple-retextures.md) generates this JSON, copies the PNGs, adds the exact manifest overrides, recognizes Core enemy export filenames, and corrects DragonBones export scale automatically.

## Minimal example

```json
{
  "schemaVersion": 1,
  "type": "assetPatch",
  "id": "yourname.my-pack:patch/starter-pistol",
  "target": "core:weapon/pistol",
  "replacements": {
    "body": "assets/pistol.png",
    "slide": "assets/slide.png",
    "base": "assets/base.png"
  }
}
```

The manifest must authorize replacement of each full slot ID:

```json
"overrides": [
  "core:weapon/pistol/body",
  "core:weapon/pistol/slide",
  "core:weapon/pistol/base"
]
```

This explicit declaration makes replacements reviewable and prevents matching filenames from changing unrelated content.

## Multipart enemy example

Target the enemy owner and use its relative body slots when retexturing a Core enemy. Do not use shared `core:sprite/...` compatibility slots when an enemy-specific slot exists.

```json
{
  "schemaVersion": 1,
  "type": "assetPatch",
  "id": "yourname.jenny-retexture:patch/body",
  "target": "core:enemy/jenny",
  "replacements": {
    "body/l-foot-base": "assets/jenny/left-foot-base.png",
    "body/r-foot-base": "assets/jenny/right-foot-base.png",
    "body/l-foot-end": "assets/jenny/left-foot-end.png",
    "body/r-foot-end": "assets/jenny/right-foot-end.png",
    "body/breast": "assets/jenny/breast.png",
    "body/breast-bg": "assets/jenny/breast-background.png"
  }
}
```

Authorize the corresponding full IDs in `manifest.json`, such as `core:enemy/jenny/body/l-foot-base`. The [Core enemy asset-slot catalog](../reference/enemy-asset-slots.md) lists every exact body part, original sprite name, animation-only variant, and compatibility alias for all 21 Core enemies.

The complete enemy-specific catalog is available in game version 2.2.1 and newer. Packs intended for older builds must use only the slots actually published by that build.

## Published slot families

The current Core adapter publishes slots for:

- weapon pieces and inventory sprites;
- player body and face artwork by semantic part and skin;
- clothing icons and semantic rig pieces;
- selected map machines and props;
- Core enemy body, rig, and anatomy artwork;
- reviewed interface, environment, effect, and presentation assets.

The [public asset-slot catalog](../reference/asset-slots.md) lists the exact IDs. Runtime lookup uses these public IDs, never Unity GUIDs or private hierarchy paths. Legacy-presentation slots are a compatibility layer over reviewed Core sprite names; prefer player, clothing, weapon, enemy, and stage semantic slots when both forms exist.

Every legacy-presentation slot requires its Core baseline to be loaded. The game log reports `Core legacy visual slots=<registered>/<advertised>` after binding and lists any advertised names unavailable in the current scene. A temporarily unavailable baseline is retried after scene loads and after relevant Core objects start.

## Resolution rules

- A replacement applies only when the target and slot exist, the file is valid, and the manifest authorizes the exact slot ID.
- A missing or invalid replacement leaves the previously resolved asset active.
- Two enabled packs targeting the same slot conflict unless the later pack is explicitly authorized under the load-order rules.
- Disabling a patch restores the baseline asset when the title scene reloads.
- Filename case and `/` separators must be portable across supported platforms.
- A PNG that is entirely black or transparent is still structurally valid. Inspect extracted legacy artwork before packaging; do not assume every changed Unity texture contains authored art.

## Replacing a Core enemy's animations (experimental)

An asset patch on a Core enemy (`core:enemy/<slug>`) may also replace its animation clips. `animations` maps a semantic clip name from the Core enemy semantics (`ModSDK/AnimationReference/CORE-ENEMY-SEMANTICS.md`) (`idle`, `move`, `attack-primary`, `finisher-phase-3`, ...) to an `enemyAnimation` document; `playerAnimations` maps `finisher-phase-N` to a `playerAnimation` for the player's side of that grab phase. `replacements` becomes optional when either is present.

```json
{
  "schemaVersion": 1,
  "type": "assetPatch",
  "id": "your.pack:patch/zombie-head-tilt",
  "target": "core:enemy/zombie-1",
  "animations": { "idle": "your.pack:enemy-animation/zombie-1-idle" },
  "playerAnimations": { "finisher-phase-1": "your.pack:player-animation/zombie-1-phase-1" }
}
```

- The `enemyAnimation` sets `"enemy": "core:enemy/<slug>"` and animates that Core rig's bones by their published names (each enemy's `ModSDK/AnimationReference/NormalizedEnemies/<slug>/rig.json`). Sprite keys name Core sprites by `name`, or load a pack-local PNG with `asset`. The `playerAnimation` uses `core:player-rig/alex`.
- The game keeps running the enemy's own Animator: its states, transitions, attack hits and grab steps are unchanged, and the Unity animation events on the original clips still fire. Each frame, after the Animator, the replacement is sampled at the state's normalized time scaled to the replacement's own length (wrapped when it loops, clamped when it does not) and its bones' position, rotation, scale, sprite, sorting order and colour are written over the Animator's. Bones and properties it does not animate keep the Animator's pose. During the enemy's grab the player's side is timed by the player's Animator the same way.
- A replacement may carry cosmetic `sound` and `cameraShake` events and normalized effects. Gameplay events (`attackHit`, `impulse`) and `spriteEffect` are rejected: the Core clip's own events still do that work. A player clip's events are ignored with a warning, because the Core grab keeps its own pleasure and damage.
- Every instance of the Core enemy plays the replacement, and so do its inherited variants unless they give their own clip for that name (see [Enemies](enemies.md#animating-a-core-enemy-experimental)).
- Two packs replacing the same clip follow the rules above, with the slot IDs `<target>/animation/<semantic>` and `<target>/player-animation/finisher-phase-N`: the later pack wins only when its manifest lists that slot in `overrides`; otherwise both are dropped and the Core clip stays.
- Validation errors: `core-animation.semantic` (not one of that enemy's clips), `core-animation.phase` (not one of its grab phases), `core-animation.bone` (not a bone of its rig), `core-animation.event`, `core-animation.owner`, `asset-patch.animation-target` (the target is not a Core enemy). The warning `core-animation.length` reports a replacement whose length differs from the original state's by more than 5%; it still plays, stretched to the state's timing.

Packs using `animations`, `playerAnimations` or `playerAnimationRefs` need a game build with this feature and do not load on 2.2.2.

## When not to use a patch

Use an additive `weapon`, `clothing`, `enemy`, or other content definition when the original and new content should coexist. Use a rule profile when behavior changes. Asset patches should not silently carry gameplay changes.

## Examples

- `ExampleMods/simple-nerf-gun`: three-piece pistol artwork.
- `ExampleMods/cod-wonderweapon`: one Core weapon presentation replacement.
- `ExampleMods/goblin-slayer-armor`: several Core clothing replacements.
- `ExampleMods/shaded-girl`: player artwork slots.
- `ExampleMods/futazombies`: Core enemy artwork slots.

See [Your first pack](../getting-started/first-pack.md) for a complete beginner workflow and `asset-patch.schema.json` in the [schema index](../reference/schemas.md) for the exact contract.
