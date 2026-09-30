# Clothing

Inherited clothing extends a Core wardrobe item so its existing slot layout remains a useful starting point. Mods can replace published clothing-piece sprites, set a category, declare incompatible categories, override each supplied piece's attachment/pivot/tearing settings, and add bounded equipped effects.

LibreSprite kits for the default shirt, hazmat suit, and scientist/lab-coat set are available under `ModSDK/ClothingTemplates`. They cover a small two-piece garment and two multi-slot outfits. Every named layer corresponds to a published `piece/...` region; left and right slots remain separate even when Core uses the same source art for both.

The supplied templates use 32 by 32 cells because that is the size of their Core source art, not because the loader culls larger sprites. A `coreClothingAtlas` region may have any validated width and height inside the PNG, and a `coreClothingSprites` file uses the entire PNG. This makes extended skirts, capes, oversized sleeves, and other overhanging art possible. Enlarge the region and atlas cell together, keep regions from overlapping, and use attachment pivots/offsets to align the art. Atlas JSON uses a bottom-left origin, while LibreSprite uses a top-left origin.

Sprite renderers use the full rectangular sprite mesh and do not inherit the old sprite's 32 by 32 bounds. Clothing still inherits an existing logical rig piece, so larger artwork is supported while completely new skeletal slots remain experimental/future work.

## Attachment overrides

Add `visual.attachments` entries keyed by a supplied `piece/...` slot. Each entry may set `bone`, `offsetX`, `offsetY`, `rotation`, normalized `pivotX`/`pivotY`, `sortingOffset`, `attachToBone`, `hideBodyPart`, `droppable`, `destroyable`, `dropOnOralThrust`, and `destroyOnOralThrust`. Omitted values remain inherited. This allows a replacement to correct alignment and define its own safe tearing response without editing a prefab.

```json
"attachments": {
  "piece/shirt-chest": {
    "bone": "Chest",
    "offsetX": 0.1,
    "offsetY": -0.2,
    "pivotX": 0.5,
    "pivotY": 0.5,
    "sortingOffset": 2,
    "droppable": true
  }
}
```

## Equipped effects

The optional top-level `effects.damageTakenMultiplier` provides armor or vulnerability (0.05–2). `effects.statModifiers` can modify up to eight reviewed player stats while the garment is equipped: `HealthMax`, `SpeedAccel`, `SpeedMax`, `Traction`, `DamageMultiplierGun`, `SpeedSprint`, `PowerJump`, and `PowerDash`. Modifiers are removed on unequip. Multiple garments combine multiplicatively for incoming damage and additively for player stats.

## Fully original clothing rigs

Use experimental `originalClothingSprites` without `extends` to construct a garment entirely from PNG files. An icon, category, and at least one `piece/...` sprite are required. Piece names are author-defined and every piece needs an attachment specifying its player bone. This permits back attachments, tails, jewelry, anatomy, layered armor, and other slots that do not exist on a Core garment.

```json
"visual": {
  "type": "originalClothingSprites",
  "pixelsPerUnit": 32,
  "sprites": {
    "icon": "assets/icon.png",
    "piece/back": "assets/back.png",
    "piece/custom-charm": "assets/charm.png"
  },
  "attachments": {
    "piece/back": {
      "bone": "Spine", "attachToBone": true,
      "offsetX": 0, "offsetY": 0,
      "pivotX": 0.5, "pivotY": 0.85,
      "sortingOffset": -3,
      "physics": {
        "mode": "sway", "spring": 45, "damping": 9,
        "gravity": 0.25, "motionInfluence": 1, "maxAngle": 30
      }
    },
    "piece/custom-charm": { "bone": "Hips", "attachToBone": true }
  }
}
```

`physics.mode: "sway"` is a bounded procedural spring intended for hanging pieces such as capes, straps, hair, and accessories. It does not add colliders or joints to the player and disables itself if the piece is torn off.

## Persistent player attachments

Experimental `playerAttachment` definitions add an always-present sprite to a published player bone without treating it as wardrobe clothing. They support a default PNG, per-skin PNG overrides, runtime skin tinting, positioning, sorting, and the same bounded sway module used by original clothing.

The optional `pregnancyGrowth` module selects a transform stage from the player's current fetus count and smoothly transitions when that count changes. Every stage declares a unique `minimumFetuses`, offset, and scale, including a required zero-fetus stage. `breakSpineClothing` can reproduce expanding-body presentations by tearing pieces attached to the Spine after a bounded delay. The [Original Dev Extras](../../../../ExampleMods/original-dev-extras/README.md) pack demonstrates the module with the exact five belly stages and four skin sprites recovered from v1.0.5bDebug.

## Body variants

`visual.bodyVariants` replaces selected piece sprites without duplicating the garment. A variant key can match the current player prefab name (for example `alex`) or the ID of a loaded body pack. An explicit `ModBodyVariant` PlayerPrefs value takes priority for future character selectors. The ordinary `visual.sprites` entry is always the fallback.

```json
"bodyVariants": {
  "my.body-pack": {
    "piece/back": "assets/back-for-my-body.png"
  },
  "alex": {
    "piece/custom-charm": "assets/charm-alex.png"
  }
}
```

The ready-to-copy skeleton is under `ModSDK/ClothingTemplates/AdvancedOriginal`. These fields remain experimental while attachment presets and body-pack identity conventions are tested.
