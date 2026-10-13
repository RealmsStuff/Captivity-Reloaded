# Public asset-slot catalog

This searchable catalog is generated from `CoreAssetSlotBinder.cs`, `CoreLegacyVisualSpriteCatalog.cs`, `enemy-asset-slots.json`, packaged Core weapon definitions, and `core-clothing.json`. Regenerate it with `Tools/Documentation/Generate-AssetSlotCatalog.ps1`.

A slot is registered only when its baseline Unity sprite or renderer is available. This matters for legacy weapons and enemy rigs whose prefabs do not all expose the same pieces. An unknown target is rejected instead of guessing a private Unity name.

Prefer the player, enemy, clothing, weapon, and stage sections when a semantic slot exists. The `core:sprite/...` sections are a compatibility layer for artwork that does not yet have a more specific public owner; those replacements can affect every Core object that shares the same original sprite name.

## Find the right slot

- Replacing one Core enemy body part: use [Core enemy-specific slots](#core-enemy-specific-slots). The [detailed enemy catalog](enemy-asset-slots.md) also shows rig bones, original sprite names, animation-only variants, and aliases by enemy.
- Replacing player artwork: use [Player](#player).
- Retexturing clothing: use [Clothing](#clothing).
- Retexturing guns or muzzle flashes: use [Weapons and weapon effects](#weapons-and-weapon-effects).
- Replacing FER or map artwork: use [Stages and map art](#stages-and-map-art).
- Replacing shared legacy body pieces, props, UI, backgrounds, or effects: search the [Legacy compatibility](#legacy-compatibility-a-d) sections by original sprite name.

## Enemy compatibility patterns

The generated enemy catalog below is authoritative. For older development projects without that generated catalog, the binder can fall back to these broad semantic patterns when the named renderer exists:

- `core:enemy/<enemy>/body/arm-lower`
- `core:enemy/<enemy>/body/arm-upper`
- `core:enemy/<enemy>/body/butt`
- `core:enemy/<enemy>/body/chest`
- `core:enemy/<enemy>/body/foot-left`
- `core:enemy/<enemy>/body/foot-right`
- `core:enemy/<enemy>/body/hand`
- `core:enemy/<enemy>/body/head`
- `core:enemy/<enemy>/body/hips`
- `core:enemy/<enemy>/body/leg-lower`
- `core:enemy/<enemy>/body/leg-upper`
- `core:enemy/<enemy>/body/neck`
- `core:enemy/<enemy>/body/penis`
- `core:enemy/<enemy>/body/torso-lower`

Some generated enemies retain compatibility aliases for these broad names. New retextures should use the exact enemy-specific paths below, especially for multipart feet, anatomy, asymmetric limbs, and animation-only artwork.

## Using a slot

Declare the exact slot ID in the manifest `overrides` list, then target its owner in an `assetPatch` and use the suffix relative to that owner in `replacements`. See [Asset patches](../content/asset-patches.md) for examples and resolution rules.

## Exact registered slot IDs

| Section | Entries |
| --- | ---: |
| [Player](#player) | 69 |
| [Core enemy-specific slots](#core-enemy-specific-slots) | 489 |
| [Enemy anatomy](#enemy-anatomy) | 32 |
| [Clothing](#clothing) | 413 |
| [Weapons and weapon effects](#weapons-and-weapon-effects) | 48 |
| [Stages and map art](#stages-and-map-art) | 28 |
| [Legacy compatibility (A-D)](#legacy-compatibility-a-d) | 192 |
| [Legacy compatibility (E-H)](#legacy-compatibility-e-h) | 77 |
| [Legacy compatibility (I-L)](#legacy-compatibility-i-l) | 30 |
| [Legacy compatibility (M-P)](#legacy-compatibility-m-p) | 90 |
| [Legacy compatibility (Q-T)](#legacy-compatibility-q-t) | 56 |
| [Legacy compatibility (U-Z)](#legacy-compatibility-u-z) | 20 |

### Player

| Slot ID | Meaning | Availability |
| --- | --- | --- |
| `core:player/body/arm-lower/black` | Player arm-lower artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/arm-lower/pale` | Player arm-lower artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/arm-lower/tan` | Player arm-lower artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/arm-lower/white` | Player arm-lower artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/arm-upper/black` | Player arm-upper artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/arm-upper/pale` | Player arm-upper artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/arm-upper/tan` | Player arm-upper artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/arm-upper/white` | Player arm-upper artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/butt/black` | Player butt artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/butt/pale` | Player butt artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/butt/tan` | Player butt artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/butt/white` | Player butt artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/chest/black` | Player chest artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/chest/pale` | Player chest artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/chest/tan` | Player chest artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/chest/white` | Player chest artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/ear/black` | Player ear artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/ear/pale` | Player ear artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/ear/tan` | Player ear artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/ear/white` | Player ear artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/foot/black` | Player foot artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/foot/pale` | Player foot artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/foot/tan` | Player foot artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/foot/white` | Player foot artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/hand/black` | Player hand artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/hand/pale` | Player hand artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/hand/tan` | Player hand artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/hand/white` | Player hand artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/head/black` | Player head artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/head/pale` | Player head artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/head/tan` | Player head artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/head/white` | Player head artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/hips/black` | Player hips artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/hips/pale` | Player hips artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/hips/tan` | Player hips artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/hips/white` | Player hips artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-lower/black` | Player leg-lower artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-lower/pale` | Player leg-lower artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-lower/tan` | Player leg-lower artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-lower/white` | Player leg-lower artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-upper/black` | Player leg-upper artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-upper/pale` | Player leg-upper artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-upper/tan` | Player leg-upper artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-upper/white` | Player leg-upper artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/neck/black` | Player neck artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/neck/pale` | Player neck artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/neck/tan` | Player neck artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/neck/white` | Player neck artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/torso-lower/black` | Player torso-lower artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/torso-lower/pale` | Player torso-lower artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/torso-lower/tan` | Player torso-lower artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/torso-lower/white` | Player torso-lower artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/face/blush/image` | Core sprite Blush_0 | runtime baseline required |
| `core:player/face/eyelid-lower/black` | Player face/eyelid-lower artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/face/eyelid-lower/pale` | Player face/eyelid-lower artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/face/eyelid-lower/tan` | Player face/eyelid-lower artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/face/eyelid-lower/white` | Player face/eyelid-lower artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/face/mouth/mouth-10` | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-12` | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-2` | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-3` | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-4` | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-5` | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-6` | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-7` | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-8` | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-9` | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-oral-3` | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-tired-2` | Player mouth expression | registered when a scene mouth manager supplies the sprite |

### Core enemy-specific slots

| Slot ID | Meaning | Availability |
| --- | --- | --- |
| `core:enemy/abby/body/arm-left-lower` | abby arm-left-lower (ArmLower_8, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/arm-left-upper` | abby arm-left-upper (ArmUpper_16, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/arm-lower` | abby compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/abby/body/arm-right-lower` | abby arm-right-lower (ArmLowerBloody, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/arm-right-upper` | abby arm-right-upper (ArmUpper_16, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/arm-upper` | abby compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/abby/body/breast` | abby breast (Breast, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/breast-bg` | abby breast-bg (BreastBg_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/butt` | abby butt (Butt_12, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/chest` | abby chest (Chest_12, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/hand` | abby compatibility alias for body/hand-left, body/hand-right | registered when at least one target renderer is available |
| `core:enemy/abby/body/hand-left` | abby hand-left (Hand_18, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/hand-right` | abby hand-right (HandBroken, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/head` | abby head (Head_17, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/hips` | abby hips (Hips_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/l-ear` | abby l-ear (EarBroken, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/l-foot-base` | abby l-foot-base (FootBase_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/l-foot-end` | abby l-foot-end (FootEnd_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/leg-left-lower` | abby leg-left-lower (LegLowerBroken, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/leg-left-upper` | abby leg-left-upper (LegUpper_17, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/leg-lower` | abby compatibility alias for body/leg-left-lower, body/leg-right-lower | registered when at least one target renderer is available |
| `core:enemy/abby/body/leg-right-lower` | abby leg-right-lower (LegLower_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/leg-right-upper` | abby leg-right-upper (LegUpper_17, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/leg-upper` | abby compatibility alias for body/leg-left-upper, body/leg-right-upper | registered when at least one target renderer is available |
| `core:enemy/abby/body/neck` | abby neck (Neck_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/penis-base` | abby penis-base (PenisBase_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/penis-end` | abby penis-end (PenisEnd_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/penis-rod` | abby penis-rod (PenisRod_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/r-ear` | abby r-ear (Ear, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/r-foot-base` | abby r-foot-base (FootBase_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/r-foot-end` | abby r-foot-end (FootEnd_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/abby/body/torso-lower` | abby spine (Spine_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/head` | death-hound head (Head_9, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/hips` | death-hound hips (TorsoLower_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/jaw` | death-hound jaw (Jaw, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/l-foot-back` | death-hound l-foot-back (FootBack, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/l-foot-front` | death-hound l-foot-front (FootFront, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/l-leg-back-lower` | death-hound l-leg-back-lower (LegBackLower, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/l-leg-back-middle` | death-hound l-leg-back-middle (LegBackMiddle, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/l-leg-back-upper` | death-hound l-leg-back-upper (LegBackUpper, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/l-leg-front-lower` | death-hound l-leg-front-lower (LegFrontLower, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/l-leg-front-upper` | death-hound l-leg-front-upper (LegFrontUpper_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/neck` | death-hound neck (Neck_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/penis` | death-hound penis (Penis_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/r-foot-back` | death-hound r-foot-back (FootBack, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/r-foot-front` | death-hound r-foot-front (FootFront, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/r-leg-back-lower` | death-hound r-leg-back-lower (LegBackLower, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/r-leg-back-middle` | death-hound r-leg-back-middle (LegBackMiddle, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/r-leg-back-upper` | death-hound r-leg-back-upper (LegBackUpper, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/r-leg-front-lower` | death-hound r-leg-front-lower (LegFrontLower, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/r-leg-front-upper` | death-hound r-leg-front-upper (LegFrontUpper_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/tail` | death-hound tail (Tail, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/death-hound/body/torso-lower` | death-hound spine (TorsoUpper, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/chest` | fly chest (Chest_20, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/head` | fly head (Head_20, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/hips` | fly hips (Hips_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/leg-back-lower` | fly leg-back-lower (LegLower_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/leg-back-mid` | fly leg-back-mid (LegMid, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/leg-back-upper` | fly leg-back-upper (LegUpper_11, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/leg-front-lower` | fly leg-front-lower (LegLower_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/leg-front-mid` | fly leg-front-mid (LegMid, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/leg-front-upper` | fly leg-front-upper (LegUpper_11, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/leg-mid-lower` | fly leg-mid-lower (LegLower_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/leg-mid-mid` | fly leg-mid-mid (LegMid, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/leg-mid-upper` | fly leg-mid-upper (LegUpper_11, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/penis-base` | fly penis-base (PenisBase_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/penis-mid` | fly penis-mid (PenisMid, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/penis-tip` | fly penis-tip (PenisTip_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/fly/body/wing` | fly wing (Wing, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/arm-left-lower` | goblin-marksman arm-left-lower (ArmLower_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/arm-left-upper` | goblin-marksman arm-left-upper (ArmUpper_15, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/arm-lower` | goblin-marksman compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/goblin-marksman/body/arm-right-lower` | goblin-marksman arm-right-lower (ArmLower_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/arm-right-upper` | goblin-marksman arm-right-upper (ArmUpper_15, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/arm-upper` | goblin-marksman compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/goblin-marksman/body/chest` | goblin-marksman chest (Chest_21, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/foot-left` | goblin-marksman foot-left (Foot_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/foot-right` | goblin-marksman foot-right (Foot_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/hand` | goblin-marksman compatibility alias for body/hand-left, body/hand-right | registered when at least one target renderer is available |
| `core:enemy/goblin-marksman/body/hand-left` | goblin-marksman hand-left (Hand_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/hand-right` | goblin-marksman hand-right (Hand_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/head` | goblin-marksman head (Head_7, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/hips` | goblin-marksman hips (Hips, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/l-weapon` | goblin-marksman l-weapon (Knife_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/leg-left-lower` | goblin-marksman leg-left-lower (LegLower_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/leg-left-upper` | goblin-marksman leg-left-upper (LegUpper, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/leg-lower` | goblin-marksman compatibility alias for body/leg-left-lower, body/leg-right-lower | registered when at least one target renderer is available |
| `core:enemy/goblin-marksman/body/leg-right-lower` | goblin-marksman leg-right-lower (LegLower_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/leg-right-upper` | goblin-marksman leg-right-upper (LegUpper, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/leg-upper` | goblin-marksman compatibility alias for body/leg-left-upper, body/leg-right-upper | registered when at least one target renderer is available |
| `core:enemy/goblin-marksman/body/neck` | goblin-marksman neck (Neck_13, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/penis` | goblin-marksman penis (Penis, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/r-weapon` | goblin-marksman r-weapon (Blowgun, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-marksman/body/torso-lower` | goblin-marksman spine (Spine_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/arm-left-lower` | goblin-trapper arm-left-lower (ArmLower_7, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/arm-left-upper` | goblin-trapper arm-left-upper (ArmUpper_11, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/arm-lower` | goblin-trapper compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/goblin-trapper/body/arm-right-lower` | goblin-trapper arm-right-lower (ArmLower_7, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/arm-right-upper` | goblin-trapper arm-right-upper (ArmUpper_11, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/arm-upper` | goblin-trapper compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/goblin-trapper/body/chest` | goblin-trapper chest (Chest_18, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/foot-left` | goblin-trapper foot-left (Foot_7, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/foot-right` | goblin-trapper foot-right (Foot_7, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/hand` | goblin-trapper compatibility alias for body/hand-left, body/hand-right | registered when at least one target renderer is available |
| `core:enemy/goblin-trapper/body/hand-left` | goblin-trapper hand-left (Hand_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/hand-right` | goblin-trapper hand-right (Hand_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/head` | goblin-trapper head (Head_12, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/hips` | goblin-trapper hips (Hips_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/l-weapon` | goblin-trapper l-weapon (Club, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/leg-left-lower` | goblin-trapper leg-left-lower (LegLower_7, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/leg-left-upper` | goblin-trapper leg-left-upper (LegUpper_20, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/leg-lower` | goblin-trapper compatibility alias for body/leg-left-lower, body/leg-right-lower | registered when at least one target renderer is available |
| `core:enemy/goblin-trapper/body/leg-right-lower` | goblin-trapper leg-right-lower (LegLower_7, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/leg-right-upper` | goblin-trapper leg-right-upper (LegUpper_20, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/leg-upper` | goblin-trapper compatibility alias for body/leg-left-upper, body/leg-right-upper | registered when at least one target renderer is available |
| `core:enemy/goblin-trapper/body/neck` | goblin-trapper neck (Neck_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/penis` | goblin-trapper penis (Penis_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/r-weapon` | goblin-trapper r-weapon (Trap, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/goblin-trapper/body/torso-lower` | goblin-trapper spine (Spine, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/arm-left-lower` | gremlin arm-left-lower (ArmLower_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/arm-left-upper` | gremlin arm-left-upper (ArmUpper_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/arm-lower` | gremlin compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/gremlin/body/arm-right-lower` | gremlin arm-right-lower (ArmLower_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/arm-right-upper` | gremlin arm-right-upper (ArmUpper_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/arm-upper` | gremlin compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/gremlin/body/chest` | gremlin chest (Chest_17, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/foot-left` | gremlin foot-left (Foot_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/foot-right` | gremlin foot-right (Foot_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/hand` | gremlin compatibility alias for body/hand-left, body/hand-right | registered when at least one target renderer is available |
| `core:enemy/gremlin/body/hand-left` | gremlin hand-left (Hand_13, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/hand-right` | gremlin hand-right (Hand_13, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/head` | gremlin head (Head_14, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/hips` | gremlin hips (Hips_8, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/l-weapon` | gremlin l-weapon (Syringe, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/leg-left-lower` | gremlin leg-left-lower (LegLower_17, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/leg-left-upper` | gremlin leg-left-upper (LegUpper_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/leg-lower` | gremlin compatibility alias for body/leg-left-lower, body/leg-right-lower | registered when at least one target renderer is available |
| `core:enemy/gremlin/body/leg-right-lower` | gremlin leg-right-lower (LegLower_17, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/leg-right-upper` | gremlin leg-right-upper (LegUpper_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/leg-upper` | gremlin compatibility alias for body/leg-left-upper, body/leg-right-upper | registered when at least one target renderer is available |
| `core:enemy/gremlin/body/neck` | gremlin neck (Neck_15, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/penis` | gremlin penis (Penis_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/r-weapon` | gremlin r-weapon (Stick, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/gremlin/body/torso-lower` | gremlin spine (Spine_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/hips` | head-humper hips (Hips_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/l-leg-back-base` | head-humper l-leg-back-base (LegBackBase, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/l-leg-back-lower` | head-humper l-leg-back-lower (LegBackLower_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/l-leg-back-mid` | head-humper l-leg-back-mid (LegBackMid, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/l-leg-front-base` | head-humper l-leg-front-base (LegFrontBase, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/l-leg-front-lower` | head-humper l-leg-front-lower (LegFrontLower_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/l-leg-front-mid` | head-humper l-leg-front-mid (LegFrontMid, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/penis-base` | head-humper penis-base (Penis_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/penis-tip` | head-humper penis-tip (Penis_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/penis1` | head-humper penis1 (Penis_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/penis2` | head-humper penis2 (Penis_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/penis3` | head-humper penis3 (Penis_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/r-leg-back-base` | head-humper r-leg-back-base (LegBackBase, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/r-leg-back-lower` | head-humper r-leg-back-lower (LegBackLower_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/r-leg-back-mid` | head-humper r-leg-back-mid (LegBackMid, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/r-leg-front-base` | head-humper r-leg-front-base (LegFrontBase, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/r-leg-front-lower` | head-humper r-leg-front-lower (LegFrontLower_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/r-leg-front-mid` | head-humper r-leg-front-mid (LegFrontMid, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/sack` | head-humper sack (Sack, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/torso-back` | head-humper torso-back (TorsoBack, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/head-humper/body/torso-front` | head-humper torso-front (TorsoFront, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/antenna-l` | hunter antenna-l (AntennaL, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/antenna-r` | hunter antenna-r (AntennaR, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/arm-left-lower` | hunter arm-left-lower (ArmLower_11, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/arm-left-upper` | hunter arm-left-upper (ArmUpper_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/arm-lower` | hunter compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/hunter/body/arm-right-lower` | hunter arm-right-lower (ArmLower_11, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/arm-right-upper` | hunter arm-right-upper (ArmUpper_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/arm-upper` | hunter compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/hunter/body/balls` | hunter balls (Balls, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/chest` | hunter chest (Chest_9, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/foot-left` | hunter foot-left (Foot_12, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/foot-right` | hunter foot-right (Foot_12, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/hand` | hunter compatibility alias for body/hand-left, body/hand-right | registered when at least one target renderer is available |
| `core:enemy/hunter/body/hand-left` | hunter hand-left (Hand_12, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/hand-right` | hunter hand-right (Hand_12, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/head` | hunter head (Head_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/hips` | hunter hips (Hip_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/leg-left-lower` | hunter leg-left-lower (LegLower_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/leg-left-upper` | hunter leg-left-upper (LegUpper_9, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/leg-lower` | hunter compatibility alias for body/leg-left-lower, body/leg-right-lower | registered when at least one target renderer is available |
| `core:enemy/hunter/body/leg-right-lower` | hunter leg-right-lower (LegLower_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/leg-right-upper` | hunter leg-right-upper (LegUpper_9, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/leg-upper` | hunter compatibility alias for body/leg-left-upper, body/leg-right-upper | registered when at least one target renderer is available |
| `core:enemy/hunter/body/neck` | hunter neck (Neck_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/hunter/body/penis` | hunter penis (Penis_7, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/arm-left-lower` | jacky arm-left-lower (ArmLower_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/arm-left-upper` | jacky arm-left-upper (ArmUpper_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/arm-lower` | jacky compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/jacky/body/arm-right-lower` | jacky arm-right-lower (ArmLower_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/arm-right-upper` | jacky arm-right-upper (ArmUpper_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/arm-upper` | jacky compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/jacky/body/breast` | jacky breast (Breast_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/breast-bg` | jacky breast-bg (BreastBg, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/butt` | jacky butt (Butt_7, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/chest` | jacky chest (Chest_16, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/hand` | jacky compatibility alias for body/hand-left, body/hand-right | registered when at least one target renderer is available |
| `core:enemy/jacky/body/hand-left` | jacky hand-left (Hand_19, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/hand-right` | jacky hand-right (Hand_19, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/head` | jacky head (Head_19, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/head/variant/head-fear` | jacky head (HeadFear, animation variant) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/hips` | jacky hips (Hips_7, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/knife` | jacky knife (knife, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/l-foot-base` | jacky l-foot-base (FootBase_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/l-foot-end` | jacky l-foot-end (FootEnd_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/leg-left-lower` | jacky leg-left-lower (LegLower_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/leg-left-upper` | jacky leg-left-upper (LegUpper_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/leg-lower` | jacky compatibility alias for body/leg-left-lower, body/leg-right-lower | registered when at least one target renderer is available |
| `core:enemy/jacky/body/leg-right-lower` | jacky leg-right-lower (LegLower_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/leg-right-upper` | jacky leg-right-upper (LegUpper_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/leg-upper` | jacky compatibility alias for body/leg-left-upper, body/leg-right-upper | registered when at least one target renderer is available |
| `core:enemy/jacky/body/neck` | jacky neck (Neck_11, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/penis-base` | jacky penis-base (PenisBase_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/penis-end` | jacky penis-end (PenisEnd_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/penis-rod` | jacky penis-rod (PenisRod, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/r-foot-base` | jacky r-foot-base (FootBase_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/r-foot-end` | jacky r-foot-end (FootEnd_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jacky/body/torso-lower` | jacky spine (Spine_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/arm-left-lower` | jenny arm-left-lower (ArmLower_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/arm-left-upper` | jenny arm-left-upper (ArmUpper_17, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/arm-lower` | jenny compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/jenny/body/arm-right-lower` | jenny arm-right-lower (ArmLower_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/arm-right-upper` | jenny arm-right-upper (ArmUpper_17, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/arm-upper` | jenny compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/jenny/body/breast` | jenny breast (Breast_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/breast-bg` | jenny breast-bg (BreastBg_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/butt` | jenny butt (Butt_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/chest` | jenny chest (Chest_11, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/hand` | jenny compatibility alias for body/hand-left, body/hand-right | registered when at least one target renderer is available |
| `core:enemy/jenny/body/hand-left` | jenny hand-left (Hand_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/hand-right` | jenny hand-right (Hand_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/head` | jenny head (Head_18, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/hips` | jenny hips (Hips_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/l-foot-base` | jenny l-foot-base (FootBase_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/l-foot-end` | jenny l-foot-end (FootEnd, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/leg-left-lower` | jenny leg-left-lower (LegLower_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/leg-left-upper` | jenny leg-left-upper (LegUpper_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/leg-lower` | jenny compatibility alias for body/leg-left-lower, body/leg-right-lower | registered when at least one target renderer is available |
| `core:enemy/jenny/body/leg-right-lower` | jenny leg-right-lower (LegLower_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/leg-right-upper` | jenny leg-right-upper (LegUpper_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/leg-upper` | jenny compatibility alias for body/leg-left-upper, body/leg-right-upper | registered when at least one target renderer is available |
| `core:enemy/jenny/body/neck` | jenny neck (Neck_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/penis-base` | jenny penis-base (PenisBase_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/penis-end` | jenny penis-end (PenisEnd, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/penis-rod` | jenny penis-rod (PenisRod_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/r-foot-base` | jenny r-foot-base (FootBase_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/r-foot-end` | jenny r-foot-end (FootEnd, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/jenny/body/torso-lower` | jenny spine (Spine_9, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/arm-left-lower` | litigant arm-left-lower (ArmLower_17, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/arm-left-upper` | litigant arm-left-upper (ArmUpper_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/arm-lower` | litigant compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/litigant/body/arm-right-lower` | litigant arm-right-lower (ArmLower_17, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/arm-right-upper` | litigant arm-right-upper (ArmUpper_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/arm-upper` | litigant compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/litigant/body/chest` | litigant chest (Chest_19, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/foot-left` | litigant foot-left (Foot_9, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/foot-right` | litigant foot-right (Foot_9, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/head` | litigant head (Head_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/hips` | litigant hips (Hips_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/l-hand-base` | litigant l-hand-base (HandBase, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/l-hand-end` | litigant l-hand-end (HandEnd, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/l-toe` | litigant l-toe (Toe, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/leg-left-lower` | litigant leg-left-lower (LegLower_16, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/leg-left-upper` | litigant leg-left-upper (LegUpper_16, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/leg-lower` | litigant compatibility alias for body/leg-left-lower, body/leg-right-lower | registered when at least one target renderer is available |
| `core:enemy/litigant/body/leg-right-lower` | litigant leg-right-lower (LegLower_16, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/leg-right-upper` | litigant leg-right-upper (LegUpper_16, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/leg-upper` | litigant compatibility alias for body/leg-left-upper, body/leg-right-upper | registered when at least one target renderer is available |
| `core:enemy/litigant/body/neck` | litigant neck (Neck_12, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/penis-base` | litigant penis-base (PenisMiddle, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/penis-tip` | litigant penis-tip (PenisTip_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/penis1` | litigant penis1 (PenisMiddle, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/penis2` | litigant penis2 (PenisMiddle, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/penis3` | litigant penis3 (PenisMiddle, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/penis4` | litigant penis4 (PenisMiddle, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/penis5` | litigant penis5 (PenisMiddle, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/penis6` | litigant penis6 (PenisMiddle, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/r-hand-base` | litigant r-hand-base (HandBase, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/r-hand-end` | litigant r-hand-end (HandEnd, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/r-toe` | litigant r-toe (Toe, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/litigant/body/torso-lower` | litigant spine (Spine_8, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/maggot/body/back` | maggot back (Back, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/maggot/body/front` | maggot front (Front, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/maggot/body/hips` | maggot hips (Mid, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/arm-bottom-lower` | musca arm-bottom-lower (ArmLower_14, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/arm-bottom-upper` | musca arm-bottom-upper (ArmUpper_8, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/arm-mid-lower` | musca arm-mid-lower (ArmLower_14, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/arm-mid-upper` | musca arm-mid-upper (ArmUpper_8, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/arm-top-lower` | musca arm-top-lower (ArmLower_14, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/arm-top-upper` | musca arm-top-upper (ArmUpper_8, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/balls` | musca balls (Balls_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/chest` | musca chest (Chest_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/hand-bottom` | musca hand-bottom (Hand_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/hand-mid` | musca hand-mid (Hand_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/hand-top` | musca hand-top (Hand_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/head` | musca head (Head_15, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/hips` | musca hips (Hips_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/neck` | musca neck (Neck_16, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/penis-base` | musca penis-base (PenisBase_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/penis-mid` | musca penis-mid (PenisMid_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/penis-tip` | musca penis-tip (PenisTip, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/torso-lower` | musca spine (Spine_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/musca/body/wings` | musca wings (Wing_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/arm-left-lower` | orc arm-left-lower (ArmLower_13, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/arm-left-upper` | orc arm-left-upper (ArmUpper_13, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/arm-lower` | orc compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/orc/body/arm-right-lower` | orc arm-right-lower (ArmLower_13, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/arm-right-upper` | orc arm-right-upper (ArmUpper_13, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/arm-upper` | orc compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/orc/body/butt` | orc butt (Butt_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/chest` | orc chest (Chest_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/foot-left` | orc foot-left (Foot_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/foot-right` | orc foot-right (Foot_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/hand` | orc compatibility alias for body/hand-left, body/hand-right | registered when at least one target renderer is available |
| `core:enemy/orc/body/hand-left` | orc hand-left (Hand_15, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/hand-right` | orc hand-right (Hand_15, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/head` | orc head (Head_21, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/hips` | orc hips (Hips_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/leg-left-lower` | orc leg-left-lower (LegLower_9, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/leg-left-upper` | orc leg-left-upper (LegUpper_18, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/leg-lower` | orc compatibility alias for body/leg-left-lower, body/leg-right-lower | registered when at least one target renderer is available |
| `core:enemy/orc/body/leg-right-lower` | orc leg-right-lower (LegLower_9, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/leg-right-upper` | orc leg-right-upper (LegUpper_18, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/leg-upper` | orc compatibility alias for body/leg-left-upper, body/leg-right-upper | registered when at least one target renderer is available |
| `core:enemy/orc/body/machete` | orc machete (Machete, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/neck` | orc neck (Neck_9, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/penis-base` | orc penis-base (PenisBase, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/penis-mid` | orc penis-mid (PenisMid_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/penis-tip` | orc penis-tip (PenisTip_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/shield` | orc shield (Shield, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/orc/body/torso-lower` | orc spine (Spine_7, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/arm-left-lower` | spider-mother arm-left-lower (ArmLower_12, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/arm-left-upper` | spider-mother arm-left-upper (ArmUpper, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/arm-lower` | spider-mother compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/spider-mother/body/arm-right-lower` | spider-mother arm-right-lower (ArmLower_12, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/arm-right-upper` | spider-mother arm-right-upper (ArmUpper, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/arm-upper` | spider-mother compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/spider-mother/body/breast` | spider-mother breast (Breast_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/butt` | spider-mother butt (Butt_13, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/chest` | spider-mother chest (Chest_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/chest-inner` | spider-mother chest-inner (ChestInner, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/hand` | spider-mother compatibility alias for body/hand-left, body/hand-right | registered when at least one target renderer is available |
| `core:enemy/spider-mother/body/hand-left` | spider-mother hand-left (Hand_14, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/hand-right` | spider-mother hand-right (Hand_14, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/head` | spider-mother head (Head_13, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/hips` | spider-mother hips (Hips_9, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/l-leg-back-base` | spider-mother l-leg-back-base (LegBackBase_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/l-leg-back-upper` | spider-mother l-leg-back-upper (LegBackUpper_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/l-leg-front-base` | spider-mother l-leg-front-base (LegFrontBase_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/l-leg-front-upper` | spider-mother l-leg-front-upper (LegFrontUpper, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/neck` | spider-mother neck (Neck_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/r-leg-back-base` | spider-mother r-leg-back-base (LegBackBase_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/r-leg-back-upper` | spider-mother r-leg-back-upper (LegBackUpper_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/r-leg-front-base` | spider-mother r-leg-front-base (LegFrontBase_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/spider-mother/body/r-leg-front-upper` | spider-mother r-leg-front-upper (LegFrontUpper, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sqoid/body/background` | sqoid background (bg, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sqoid/body/brain` | sqoid brain (brain, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sqoid/body/hips` | sqoid hips (hips_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/arm-left-lower` | sunny arm-left-lower (ArmLower_19, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/arm-left-upper` | sunny arm-left-upper (ArmUpper_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/arm-lower` | sunny compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/sunny/body/arm-right-lower` | sunny arm-right-lower (ArmLower_19, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/arm-right-upper` | sunny arm-right-upper (ArmUpper_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/arm-upper` | sunny compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/sunny/body/breast` | sunny breast (Breast_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/breast-bg` | sunny breast-bg (BreastBg_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/butt` | sunny butt (Butt_8, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/chest` | sunny chest (Chest_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/hand` | sunny compatibility alias for body/hand-left, body/hand-right | registered when at least one target renderer is available |
| `core:enemy/sunny/body/hand-left` | sunny hand-left (Hand_8, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/hand-right` | sunny hand-right (Hand_8, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/head` | sunny head (Head_11, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/hips` | sunny hips (Hips_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/l-foot-base` | sunny l-foot-base (ShoeBase, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/l-foot-end` | sunny l-foot-end (ShoeEnd, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/leg-left-lower` | sunny leg-left-lower (LegLower_13, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/leg-left-upper` | sunny leg-left-upper (LegUpper_12, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/leg-lower` | sunny compatibility alias for body/leg-left-lower, body/leg-right-lower | registered when at least one target renderer is available |
| `core:enemy/sunny/body/leg-right-lower` | sunny leg-right-lower (LegLowerBloody_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/leg-right-upper` | sunny leg-right-upper (LegUpperBloody, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/leg-upper` | sunny compatibility alias for body/leg-left-upper, body/leg-right-upper | registered when at least one target renderer is available |
| `core:enemy/sunny/body/neck` | sunny neck (Neck_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/penis-base` | sunny penis-base (PenisBase_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/penis-end` | sunny penis-end (PenisEnd_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/penis-rod` | sunny penis-rod (PenisRod_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/r-foot-base` | sunny r-foot-base (FootBase, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/r-foot-end` | sunny r-foot-end (FootEnd_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/skirt` | sunny skirt (Skirt, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/sunny/body/torso-lower` | sunny spine (Spine_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/arm-left-lower` | zombie-1 arm-left-lower (ArmLower_21, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/arm-left-upper` | zombie-1 arm-left-upper (ArmUpper_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/arm-lower` | zombie-1 compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/zombie-1/body/arm-right-lower` | zombie-1 arm-right-lower (ArmLower_21, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/arm-right-upper` | zombie-1 arm-right-upper (ArmUpper_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/arm-upper` | zombie-1 compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/zombie-1/body/butt` | zombie-1 butt (Butt, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/chest` | zombie-1 chest (Chest, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/foot-left` | zombie-1 foot-left (Foot_11, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/foot-right` | zombie-1 foot-right (Foot2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/hand` | zombie-1 compatibility alias for body/hand-left, body/hand-right | registered when at least one target renderer is available |
| `core:enemy/zombie-1/body/hand-left` | zombie-1 hand-left (Hand_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/hand-right` | zombie-1 hand-right (Hand_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/head` | zombie-1 head (Head, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/hips` | zombie-1 hips (Hip, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/leg-left-lower` | zombie-1 leg-left-lower (LegLower_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/leg-left-upper` | zombie-1 leg-left-upper (LegUpper_19, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/leg-lower` | zombie-1 compatibility alias for body/leg-left-lower, body/leg-right-lower | registered when at least one target renderer is available |
| `core:enemy/zombie-1/body/leg-right-lower` | zombie-1 leg-right-lower (LegLower_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/leg-right-upper` | zombie-1 leg-right-upper (LegUpper_19, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/leg-upper` | zombie-1 compatibility alias for body/leg-left-upper, body/leg-right-upper | registered when at least one target renderer is available |
| `core:enemy/zombie-1/body/neck` | zombie-1 neck (Neck_18, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/penis` | zombie-1 penis (penis_0, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-1/body/torso-lower` | zombie-1 spine (TorsoLower_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/arm-left-lower` | zombie-2 arm-left-lower (ArmLower_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/arm-left-upper` | zombie-2 arm-left-upper (ArmUpper_19, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/arm-lower` | zombie-2 compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/zombie-2/body/arm-right-lower` | zombie-2 arm-right-lower (ArmLower_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/arm-right-upper` | zombie-2 arm-right-upper (ArmUpper_19, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/arm-upper` | zombie-2 compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/zombie-2/body/butt` | zombie-2 butt (Butt_2, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/chest` | zombie-2 chest (Chest_14, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/foot-left` | zombie-2 foot-left (Foot_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/foot-right` | zombie-2 foot-right (Foot_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/hand` | zombie-2 compatibility alias for body/hand-left, body/hand-right | registered when at least one target renderer is available |
| `core:enemy/zombie-2/body/hand-left` | zombie-2 hand-left (Hand_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/hand-right` | zombie-2 hand-right (Hand_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/head` | zombie-2 head (Head_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/hips` | zombie-2 hips (Hip_6, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/leg-left-lower` | zombie-2 leg-left-lower (LegLower_19, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/leg-left-upper` | zombie-2 leg-left-upper (LegUpper_8, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/leg-lower` | zombie-2 compatibility alias for body/leg-left-lower, body/leg-right-lower | registered when at least one target renderer is available |
| `core:enemy/zombie-2/body/leg-right-lower` | zombie-2 leg-right-lower (LegLower_19, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/leg-right-upper` | zombie-2 leg-right-upper (LegUpper_8, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/leg-upper` | zombie-2 compatibility alias for body/leg-left-upper, body/leg-right-upper | registered when at least one target renderer is available |
| `core:enemy/zombie-2/body/neck` | zombie-2 neck (Neck_14, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/penis` | zombie-2 penis (penis_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-2/body/torso-lower` | zombie-2 spine (TorsoLower_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/arm-left-lower` | zombie-3 arm-left-lower (ArmLower_21 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/arm-left-upper` | zombie-3 arm-left-upper (ArmUpper_1 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/arm-lower` | zombie-3 compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/zombie-3/body/arm-right-lower` | zombie-3 arm-right-lower (ArmLower_21 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/arm-right-upper` | zombie-3 arm-right-upper (ArmUpper_1 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/arm-upper` | zombie-3 compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/zombie-3/body/butt` | zombie-3 butt (Butt, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/chest` | zombie-3 chest (Chest 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/foot-left` | zombie-3 foot-left (Foot_11 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/foot-right` | zombie-3 foot-right (Foot_11 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/hand` | zombie-3 compatibility alias for body/hand-left, body/hand-right | registered when at least one target renderer is available |
| `core:enemy/zombie-3/body/hand-left` | zombie-3 hand-left (Hand_6 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/hand-right` | zombie-3 hand-right (Hand_6 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/head` | zombie-3 head (Head, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/hips` | zombie-3 hips (Hip 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/leg-left-lower` | zombie-3 leg-left-lower (LegLower_2 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/leg-left-upper` | zombie-3 leg-left-upper (LegUpper_19 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/leg-lower` | zombie-3 compatibility alias for body/leg-left-lower, body/leg-right-lower | registered when at least one target renderer is available |
| `core:enemy/zombie-3/body/leg-right-lower` | zombie-3 leg-right-lower (LegLower_2 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/leg-right-upper` | zombie-3 leg-right-upper (LegUpper_19 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/leg-upper` | zombie-3 compatibility alias for body/leg-left-upper, body/leg-right-upper | registered when at least one target renderer is available |
| `core:enemy/zombie-3/body/neck` | zombie-3 neck (Neck_18 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/penis` | zombie-3 penis (penis_0 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-3/body/torso-lower` | zombie-3 spine (TorsoLower_5 1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/arm-left-lower` | zombie-grabber arm-left-lower (ArmLower_18, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/arm-left-upper` | zombie-grabber arm-left-upper (ArmUpper_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/arm-lower` | zombie-grabber compatibility alias for body/arm-left-lower, body/arm-right-lower | registered when at least one target renderer is available |
| `core:enemy/zombie-grabber/body/arm-right-lower` | zombie-grabber arm-right-lower (ArmLower_18, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/arm-right-upper` | zombie-grabber arm-right-upper (ArmUpper_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/arm-upper` | zombie-grabber compatibility alias for body/arm-left-upper, body/arm-right-upper | registered when at least one target renderer is available |
| `core:enemy/zombie-grabber/body/butt` | zombie-grabber butt (Butt_9, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/chest` | zombie-grabber chest (Chest_1, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/foot-left` | zombie-grabber foot-left (Foot_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/foot-right` | zombie-grabber foot-right (Foot_3, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/hand` | zombie-grabber compatibility alias for body/hand-left, body/hand-right | registered when at least one target renderer is available |
| `core:enemy/zombie-grabber/body/hand-left` | zombie-grabber hand-left (Hand_17, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/hand-right` | zombie-grabber hand-right (Hand_17, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/head` | zombie-grabber head (Head_4, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/hips` | zombie-grabber hips (Hip_8, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/leg-left-lower` | zombie-grabber leg-left-lower (LegLower_12, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/leg-left-upper` | zombie-grabber leg-left-upper (LegUpper_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/leg-lower` | zombie-grabber compatibility alias for body/leg-left-lower, body/leg-right-lower | registered when at least one target renderer is available |
| `core:enemy/zombie-grabber/body/leg-right-lower` | zombie-grabber leg-right-lower (LegLower_12, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/leg-right-upper` | zombie-grabber leg-right-upper (LegUpper_10, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/leg-upper` | zombie-grabber compatibility alias for body/leg-left-upper, body/leg-right-upper | registered when at least one target renderer is available |
| `core:enemy/zombie-grabber/body/neck` | zombie-grabber neck (Neck_17, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/penis` | zombie-grabber penis (penis_5, rig renderer) | registered from the generated Core enemy rig catalog |
| `core:enemy/zombie-grabber/body/torso-lower` | zombie-grabber spine (TorsoLower_4, rig renderer) | registered from the generated Core enemy rig catalog |

### Enemy anatomy

| Slot ID | Meaning | Availability |
| --- | --- | --- |
| `core:enemy-anatomy/sprite/penis` | Core sprite Penis | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-0` | Core sprite penis_0 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-1` | Core sprite penis_1 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-2` | Core sprite Penis_2 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-3` | Core sprite Penis_3 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-4` | Core sprite Penis_4 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-5` | Core sprite penis_5 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-6` | Core sprite Penis_6 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-7` | Core sprite Penis_7 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base` | Core sprite PenisBase | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base-0` | Core sprite PenisBase_0 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base-1` | Core sprite PenisBase_1 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base-2` | Core sprite PenisBase_2 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base-3` | Core sprite PenisBase_3 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base-4` | Core sprite PenisBase_4 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base-5` | Core sprite PenisBase_5 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base-6` | Core sprite PenisBase_6 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-end` | Core sprite PenisEnd | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-end-0` | Core sprite PenisEnd_0 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-end-1` | Core sprite PenisEnd_1 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-end-2` | Core sprite PenisEnd_2 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-end-3` | Core sprite PenisEnd_3 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-mid` | Core sprite PenisMid | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-mid-0` | Core sprite PenisMid_0 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-mid-1` | Core sprite PenisMid_1 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-middle` | Core sprite PenisMiddle | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-rod` | Core sprite PenisRod | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-rod-0` | Core sprite PenisRod_0 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-tip` | Core sprite PenisTip | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-tip-0` | Core sprite PenisTip_0 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-tip-1` | Core sprite PenisTip_1 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-tip-2` | Core sprite PenisTip_2 | runtime baseline required |

### Clothing

| Slot ID | Meaning | Availability |
| --- | --- | --- |
| `core:clothing/arm-wrap/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/arm-wrap/piece/r-arm-upper` | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/bandana/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/bandana/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/bikini-jewelry/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/bikini-jewelry/piece/shirt-chest` | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/bikini-marine/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/bikini-marine/piece/shirt-chest` | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/bikini-orange/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/bikini-orange/piece/shirt-chest` | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/bikini-pink/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/bikini-pink/piece/shirt-chest` | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/bikini-white/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/bikini-white/piece/shirt-chest` | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/black-top/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/black-top/piece/shirt-chest` | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/christy-skirt/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/christy-skirt/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/christy-skirt/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/christy-skirt/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/christy-skirt/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/christy-skirt/piece/skirt-hips` | Garment piece/skirt-hips | registered from the reconstructed garment |
| `core:clothing/crown/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/crown/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/crying-mask/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/crying-mask/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/dark-purple-top/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/dark-purple-top/piece/shirt-chest` | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/dog-collar/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/dog-collar/piece/neck` | Garment piece/neck | registered from the reconstructed garment |
| `core:clothing/dog-ears/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/dog-ears/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/dress-ada/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/dress-ada/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/dress-ada/piece/chest` | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/dress-ada/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/dress-ada/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/dress-ada/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/dress-ada/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/eye-wrap/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/eye-wrap/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/feet-jewelry/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/feet-jewelry/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/feet-jewelry/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/feet-jewelry/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/feet-jewelry/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/glasses-1/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/glasses-1/piece/glasses` | Garment piece/glasses | registered from the reconstructed garment |
| `core:clothing/glasses-2/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/glasses-2/piece/glasses` | Garment piece/glasses | registered from the reconstructed garment |
| `core:clothing/golden-arm-bracelet/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/golden-arm-bracelet/piece/r-arm-upper` | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/hair-2/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-2/piece/hair` | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-ada/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-ada/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/hair-asian/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-asian/piece/hair` | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-blonde-bowl-red-line/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-blonde-bowl-red-line/piece/hair` | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-blonde-pony-tail/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-blonde-pony-tail/piece/hair` | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-brown-big/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-brown-big/piece/hair` | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-brown-pony-tail/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-brown-pony-tail/piece/hair` | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-default/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-default/piece/hair` | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-punk/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-punk/piece/hair` | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-white-blonde/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-white-blonde/piece/hair` | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hand-jill/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hand-jill/piece/l-hand` | Garment piece/l-hand | registered from the reconstructed garment |
| `core:clothing/hand-jill/piece/r-hand` | Garment piece/r-hand | registered from the reconstructed garment |
| `core:clothing/hard-hat/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hard-hat/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/hat-christy/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hat-christy/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/hat-jill/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hat-jill/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/chest` | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/l-arm-lower` | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/l-arm-upper` | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/l-hand` | Garment piece/l-hand | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/l-leg-lower-shoes` | Garment piece/l-leg-lower-shoes | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/mask` | Garment piece/mask | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/neck` | Garment piece/neck | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/r-arm-lower` | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/r-arm-upper` | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/r-hand` | Garment piece/r-hand | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/r-leg-lower-shoes` | Garment piece/r-leg-lower-shoes | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/hockey-mask/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hockey-mask/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/jeans-blue/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/jeans-blue/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/jeans-blue/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/jeans-blue/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/jeans-blue/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/jeans-blue/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/jeans-blue/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/jeans-blue/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/jeans-white/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/jeans-white/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/jeans-white/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/jeans-white/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/jeans-white/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/jeans-white/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/jeans-white/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/jeans-white/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/jewelry-ear/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/jewelry-ear/piece/ear` | Garment piece/ear | registered from the reconstructed garment |
| `core:clothing/jewelry-sleeve/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/jewelry-sleeve/piece/r-arm-lower` | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/jewelry-sleeve/piece/r-hand` | Garment piece/r-hand | registered from the reconstructed garment |
| `core:clothing/knight-boots/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/l-leg-lower-armor` | Garment piece/l-leg-lower-armor | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/l-leg-lower-stocking` | Garment piece/l-leg-lower-stocking | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/l-leg-upper-stocking` | Garment piece/l-leg-upper-stocking | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/r-leg-lower-armor` | Garment piece/r-leg-lower-armor | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/r-leg-lower-stocking` | Garment piece/r-leg-lower-stocking | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/r-leg-upper-stocking` | Garment piece/r-leg-upper-stocking | registered from the reconstructed garment |
| `core:clothing/knight-breast-plate/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/knight-breast-plate/piece/arm-upper` | Garment piece/arm-upper | registered from the reconstructed garment |
| `core:clothing/knight-breast-plate/piece/chest` | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/knight-helmet/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/knight-helmet/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/knight-pants/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/knight-pants/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/knight-pants/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/knight-pants/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/knight-pants/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/knight-pants/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/latex-gloves-black/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/latex-gloves-black/piece/l-arm-lower` | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/latex-gloves-black/piece/l-arm-upper` | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/latex-gloves-black/piece/l-hand` | Garment piece/l-hand | registered from the reconstructed garment |
| `core:clothing/latex-gloves-black/piece/r-arm-lower` | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/latex-gloves-black/piece/r-arm-upper` | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/latex-gloves-black/piece/r-hand` | Garment piece/r-hand | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-purple/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-purple/piece/l-arm-lower` | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-purple/piece/l-arm-upper` | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-purple/piece/l-hand` | Garment piece/l-hand | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-purple/piece/r-arm-lower` | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-purple/piece/r-arm-upper` | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-purple/piece/r-hand` | Garment piece/r-hand | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-red/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-red/piece/l-arm-lower` | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-red/piece/l-arm-upper` | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-red/piece/l-hand` | Garment piece/l-hand | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-red/piece/r-arm-lower` | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-red/piece/r-arm-upper` | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-red/piece/r-hand` | Garment piece/r-hand | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/lingerie-black-upper/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/lingerie-black-upper/piece/chest` | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/lingerie-white-upper/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/lingerie-white-upper/piece/chest` | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/pants-default/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/pants-default/piece/belt` | Garment piece/belt | registered from the reconstructed garment |
| `core:clothing/pants-default/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/pants-default/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/pants-default/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/pants-default/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/pants-jill/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/pants-jill/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/pants-jill/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/pants-jill/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/pants-jill/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/pants-jill/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/pants-jill/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/pants-jill/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/pants-underwear/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/pants-underwear/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/pants-underwear/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/pants-underwear/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/pants-underwear/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/pants-underwear/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-body/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-body/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-body/piece/chest` | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-body/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-body/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-ears/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-ears/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-sleeves/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-sleeves/piece/l-arm-lower` | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-sleeves/piece/r-arm-lower` | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/programming-socks-blue-white/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/programming-socks-blue-white/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/programming-socks-blue-white/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/programming-socks-blue-white/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/programming-socks-blue-white/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/programming-socks-blue-white/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/programming-socks-blue-white/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/programming-socks-jacky-style/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/programming-socks-jacky-style/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/programming-socks-jacky-style/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/programming-socks-jacky-style/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/programming-socks-jacky-style/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/programming-socks-jacky-style/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/programming-socks-jacky-style/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/programming-socks-pink-white/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/programming-socks-pink-white/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/programming-socks-pink-white/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/programming-socks-pink-white/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/programming-socks-pink-white/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/programming-socks-pink-white/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/programming-socks-pink-white/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/scientist-set/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/shirt-chest` | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/shirt-l-arm-lower` | Garment piece/shirt-l-arm-lower | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/shirt-l-arm-upper` | Garment piece/shirt-l-arm-upper | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/shirt-r-arm-lower` | Garment piece/shirt-r-arm-lower | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/shirt-r-arm-upper` | Garment piece/shirt-r-arm-upper | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/shirt-spine` | Garment piece/shirt-spine | registered from the reconstructed garment |
| `core:clothing/security-hat/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/security-hat/piece/shirt-collar` | Garment piece/shirt-collar | registered from the reconstructed garment |
| `core:clothing/security-pants/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/security-pants/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/security-pants/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/security-pants/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/security-pants/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/security-pants/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/security-pants/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/security-pants/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/security-shirt/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/security-shirt/piece/shirt-chest` | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/security-shirt/piece/shirt-collar` | Garment piece/shirt-collar | registered from the reconstructed garment |
| `core:clothing/security-shirt/piece/shirt-l-arm-lower` | Garment piece/shirt-l-arm-lower | registered from the reconstructed garment |
| `core:clothing/security-shirt/piece/shirt-l-arm-upper` | Garment piece/shirt-l-arm-upper | registered from the reconstructed garment |
| `core:clothing/security-shirt/piece/shirt-r-arm-lower` | Garment piece/shirt-r-arm-lower | registered from the reconstructed garment |
| `core:clothing/security-shirt/piece/shirt-r-arm-upper` | Garment piece/shirt-r-arm-upper | registered from the reconstructed garment |
| `core:clothing/security-shirt/piece/shirt-spine` | Garment piece/shirt-spine | registered from the reconstructed garment |
| `core:clothing/security-shoes/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/security-shoes/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/security-shoes/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/shirt-1/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-1/piece/chest` | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/shirt-1/piece/l-arm-upper` | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-1/piece/r-arm-upper` | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-1/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/shirt-2/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-2/piece/chest` | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/shirt-2/piece/l-arm-upper` | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-2/piece/r-arm-upper` | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-2/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/shirt-3/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-3/piece/chest` | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/shirt-3/piece/l-arm-upper` | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-3/piece/r-arm-upper` | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-3/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/shirt-christy/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-christy/piece/l-arm-lower` | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/shirt-christy/piece/l-arm-upper` | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-christy/piece/r-arm-lower` | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/shirt-christy/piece/r-arm-upper` | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-christy/piece/shirt-chest` | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/shirt-christy/piece/shirt-neck` | Garment piece/shirt-neck | registered from the reconstructed garment |
| `core:clothing/shirt-christy/piece/shirt-spine` | Garment piece/shirt-spine | registered from the reconstructed garment |
| `core:clothing/shirt-damaged/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-damaged/piece/chest` | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/shirt-damaged/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/shirt-default/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-default/piece/shirt-chest` | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/shirt-default/piece/shirt-spine` | Garment piece/shirt-spine | registered from the reconstructed garment |
| `core:clothing/shirt-jill/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-jill/piece/chest` | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/shirt-jill/piece/l-arm-upper` | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-jill/piece/r-arm-upper` | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-jill/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/shirt-top-blue/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-top-blue/piece/shirt-chest` | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/shirt-top-orange/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-top-orange/piece/shirt-chest` | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/shoes-ada/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shoes-ada/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/shoes-ada/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/shoes-christy/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shoes-christy/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/shoes-christy/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/shoes-default/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shoes-default/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/shoes-default/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/shoes-jill/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shoes-jill/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/shoes-jill/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/shoes-jill/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/shoes-jill/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-blue/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-blue/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-blue/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-blue/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-blue/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-blue/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-white/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-white/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-white/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-white/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-white/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-white/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/silver-arm-bracelet/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/silver-arm-bracelet/piece/r-arm-upper` | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/sports-sneakers-black/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/sports-sneakers-black/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/sports-sneakers-black/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/sports-sneakers-white/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/sports-sneakers-white/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/sports-sneakers-white/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/stockings-ada/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/stockings-ada/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/stockings-ada/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/stockings-ada/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/stockings-ada/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/stockings-ada/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/stockings-ada/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/stockings-default/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/stockings-default/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/stockings-default/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/stockings-default/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/stockings-default/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/stockings-default/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/stockings-default/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/stockings-punk/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/stockings-punk/piece/l-foot` | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/stockings-punk/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/stockings-punk/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/stockings-punk/piece/r-foot` | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/stockings-punk/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/stockings-punk/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/string-jewelry/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/string-jewelry/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/string-marine/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/string-marine/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/string-orange/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/string-orange/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/string-pink/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/string-pink/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/string-white/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/string-white/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/summer-hat/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/summer-hat/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/sweater-1/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/sweater-1/piece/chest` | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/sweater-1/piece/l-arm-lower` | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/sweater-1/piece/l-arm-upper` | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/sweater-1/piece/r-arm-lower` | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/sweater-1/piece/r-arm-upper` | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/sweater-1/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/sweater-2/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/sweater-2/piece/chest` | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/sweater-2/piece/l-arm-lower` | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/sweater-2/piece/l-arm-upper` | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/sweater-2/piece/r-arm-lower` | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/sweater-2/piece/r-arm-upper` | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/sweater-2/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/tiara/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/tiara/piece/head` | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/white-top/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/white-top/piece/shirt-chest` | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/piece/spine` | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/icon` | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/piece/butt` | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/piece/hips` | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/piece/l-leg-lower` | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/piece/l-leg-upper` | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/piece/r-leg-lower` | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/piece/r-leg-upper` | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/piece/spine` | Garment piece/spine | registered from the reconstructed garment |

### Weapons and weapon effects

| Slot ID | Meaning | Availability |
| --- | --- | --- |
| `core:weapon-effects/muzzle-flashes/1` | Core muzzle flash 1 | registered when used by a shipped gun |
| `core:weapon-effects/muzzle-flashes/2` | Core muzzle flash 2 | registered when used by a shipped gun |
| `core:weapon-effects/muzzle-flashes/3` | Core muzzle flash 3 | registered when used by a shipped gun |
| `core:weapon/css446/base` | CSS446 base renderer | registered when the shipped base renderer exists |
| `core:weapon/css446/body` | CSS446 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/halman-785/base` | Halman 785 base renderer | registered when the shipped base renderer exists |
| `core:weapon/halman-785/body` | Halman 785 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/harrington-model-1892/base` | Harrington Model 1892 base renderer | registered when the shipped base renderer exists |
| `core:weapon/harrington-model-1892/body` | Harrington Model 1892 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/lk-235/base` | LK-235 base renderer | registered when the shipped base renderer exists |
| `core:weapon/lk-235/body` | LK-235 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/m4b1/base` | M4B1 base renderer | registered when the shipped base renderer exists |
| `core:weapon/m4b1/body` | M4B1 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/mg32/base` | MG32 base renderer | registered when the shipped base renderer exists |
| `core:weapon/mg32/body` | MG32 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/muger-p08/base` | Muger P08 base renderer | registered when the shipped base renderer exists |
| `core:weapon/muger-p08/body` | Muger P08 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/np-40/base` | NP-40 base renderer | registered when the shipped base renderer exists |
| `core:weapon/np-40/body` | NP-40 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/np-9/base` | NP-9 base renderer | registered when the shipped base renderer exists |
| `core:weapon/np-9/body` | NP-9 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/pistol/base` | Pistol base renderer | registered when the shipped base renderer exists |
| `core:weapon/pistol/body` | Pistol inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/pistol/slide` | Pistol slide renderer | registered when the shipped slide renderer exists |
| `core:weapon/qlock-17-a/base` | Qlock 17-A base renderer | registered when the shipped base renderer exists |
| `core:weapon/qlock-17-a/body` | Qlock 17-A inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/qlock-17-s/base` | Qlock 17-S base renderer | registered when the shipped base renderer exists |
| `core:weapon/qlock-17-s/body` | Qlock 17-S inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/revolver-44/base` | Revolver .44 base renderer | registered when the shipped base renderer exists |
| `core:weapon/revolver-44/body` | Revolver .44 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/ronigsberg-543/base` | Ronigsberg 543 base renderer | registered when the shipped base renderer exists |
| `core:weapon/ronigsberg-543/body` | Ronigsberg 543 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/sawed-off/base` | Sawed-Off base renderer | registered when the shipped base renderer exists |
| `core:weapon/sawed-off/body` | Sawed-Off inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/schockgewehr/base` | Schockgewehr base renderer | registered when the shipped base renderer exists |
| `core:weapon/schockgewehr/body` | Schockgewehr inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/sl-defender/base` | S&L Defender base renderer | registered when the shipped base renderer exists |
| `core:weapon/sl-defender/body` | S&L Defender inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/snub-revolver-32/base` | Snub Revolver .32 base renderer | registered when the shipped base renderer exists |
| `core:weapon/snub-revolver-32/body` | Snub Revolver .32 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/ssph-42/base` | SSPH-42 base renderer | registered when the shipped base renderer exists |
| `core:weapon/ssph-42/body` | SSPH-42 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/tenelli-m5/base` | Tenelli M5 base renderer | registered when the shipped base renderer exists |
| `core:weapon/tenelli-m5/body` | Tenelli M5 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/tenelli-so3/base` | Tenelli SO3 base renderer | registered when the shipped base renderer exists |
| `core:weapon/tenelli-so3/body` | Tenelli SO3 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/usi/base` | Usi base renderer | registered when the shipped base renderer exists |
| `core:weapon/usi/body` | Usi inventory/body sprite | registered when the shipped icon exists |

### Stages and map art

| Slot ID | Meaning | Availability |
| --- | --- | --- |
| `core:map-art/usable-vendor/sprite` | Usable vendor world artwork | registered when the Core template or a Tiled vendor is available |
| `core:stage-art/fer/actor/butt-12` | Core sprite Butt_12 | runtime baseline required |
| `core:stage-art/fer/actor/chest-12` | Core sprite Chest_12 | runtime baseline required |
| `core:stage-art/fer/actor/chest-15` | Core sprite Chest_15 | runtime baseline required |
| `core:stage-art/fer/actor/hand-broken` | Core sprite HandBroken | runtime baseline required |
| `core:stage-art/fer/actor/head-11` | Core sprite Head_11 | runtime baseline required |
| `core:stage-art/fer/actor/head-17` | Core sprite Head_17 | runtime baseline required |
| `core:stage-art/fer/actor/head-18` | Core sprite Head_18 | runtime baseline required |
| `core:stage-art/fer/actor/head-turned-off` | Core sprite HeadTurnedOff | runtime baseline required |
| `core:stage-art/fer/actor/head-turned-off-alt` | Core sprite HeadTurnedOff_0 | runtime baseline required |
| `core:stage-art/fer/actor/leg-lower-bloody` | Core sprite LegLowerBloody_0 | runtime baseline required |
| `core:stage-art/fer/actor/leg-upper-17` | Core sprite LegUpper_17 | runtime baseline required |
| `core:stage-art/fer/actor/leg-upper-4` | Core sprite LegUpper_4 | runtime baseline required |
| `core:stage-art/fer/actor/leg-upper-6` | Core sprite LegUpper_6 | runtime baseline required |
| `core:stage-art/fer/actor/leg-upper-bloody` | Core sprite LegUpperBloody | runtime baseline required |
| `core:stage-art/fer/actor/spine-3` | Core sprite Spine_3 | runtime baseline required |
| `core:stage-art/fer/actor/spine-5` | Core sprite Spine_5 | runtime baseline required |
| `core:stage-art/fer/actor/spine-9` | Core sprite Spine_9 | runtime baseline required |
| `core:stage-art/fer/effects/blood-big-1` | Core sprite BloodBig1 | runtime baseline required |
| `core:stage-art/fer/effects/blood-big-2` | Core sprite BloodBig2 | runtime baseline required |
| `core:stage-art/fer/effects/blood-platform` | Core sprite BloodPlatform | runtime baseline required |
| `core:stage-art/fer/effects/blood-small-1` | Core sprite bloodSmall1 | runtime baseline required |
| `core:stage-art/fer/effects/blood-small-2` | Core sprite bloodSmall2 | runtime baseline required |
| `core:stage-art/fer/effects/blood-small-3` | Core sprite bloodSmall3 | runtime baseline required |
| `core:stage-art/fer/effects/blood-small-4` | Core sprite bloodSmall4 | runtime baseline required |
| `core:stage-art/fer/effects/blood-wall-1` | Core sprite BloodSplashesWall1 | runtime baseline required |
| `core:stage-art/fer/effects/blood-wall-2` | Core sprite BloodSplashesWall2 | runtime baseline required |
| `core:stage-art/fer/effects/blood-wall-3` | Core sprite BloodSplashesWall3 | runtime baseline required |

### Legacy compatibility (A-D)

These slots follow original Core sprite names and may affect multiple objects that reuse the same sprite. Prefer a semantic slot above when one exists.

| Slot ID | Meaning | Availability |
| --- | --- | --- |
| `core:sprite/ammo-box/image` | Legacy Core sprite AmmoBox | runtime baseline required |
| `core:sprite/anaphrodisiac/image` | Legacy Core sprite anaphrodisiac | runtime baseline required |
| `core:sprite/android-parts-cabinet/image` | Legacy Core sprite AndroidPartsCabinet | runtime baseline required |
| `core:sprite/arm-lower-1/image` | Legacy Core sprite ArmLower_1 | runtime baseline required |
| `core:sprite/arm-lower-10/image` | Legacy Core sprite ArmLower_10 | runtime baseline required |
| `core:sprite/arm-lower-15/image` | Legacy Core sprite ArmLower_15 | runtime baseline required |
| `core:sprite/arm-lower-19/image` | Legacy Core sprite ArmLower_19 | runtime baseline required |
| `core:sprite/arm-lower-2/image` | Legacy Core sprite ArmLower_2 | runtime baseline required |
| `core:sprite/arm-lower-5/image` | Legacy Core sprite ArmLower_5 | runtime baseline required |
| `core:sprite/arm-lower-8/image` | Legacy Core sprite ArmLower_8 | runtime baseline required |
| `core:sprite/arm-lower-bloody-0/image` | Legacy Core sprite ArmLowerBloody_0 | runtime baseline required |
| `core:sprite/arm-lower-bloody/image` | Legacy Core sprite ArmLowerBloody | runtime baseline required |
| `core:sprite/arm-upper-0/image` | Legacy Core sprite ArmUpper_0 | runtime baseline required |
| `core:sprite/arm-upper-10/image` | Legacy Core sprite ArmUpper_10 | runtime baseline required |
| `core:sprite/arm-upper-16/image` | Legacy Core sprite ArmUpper_16 | runtime baseline required |
| `core:sprite/arm-upper-17/image` | Legacy Core sprite ArmUpper_17 | runtime baseline required |
| `core:sprite/arm-upper-18/image` | Legacy Core sprite ArmUpper_18 | runtime baseline required |
| `core:sprite/arm-upper-20/image` | Legacy Core sprite ArmUpper_20 | runtime baseline required |
| `core:sprite/arm-upper-5/image` | Legacy Core sprite ArmUpper_5 | runtime baseline required |
| `core:sprite/aspirin/image` | Legacy Core sprite Aspirin | runtime baseline required |
| `core:sprite/b-prec/image` | Legacy Core sprite BPrec | runtime baseline required |
| `core:sprite/background/image` | Legacy Core sprite Background | runtime baseline required |
| `core:sprite/bar-big/image` | Legacy Core sprite barBig | runtime baseline required |
| `core:sprite/bar-small/image` | Legacy Core sprite barSmall | runtime baseline required |
| `core:sprite/bar/image` | Legacy Core sprite Bar | runtime baseline required |
| `core:sprite/barrel-0/image` | Legacy Core sprite barrel_0 | runtime baseline required |
| `core:sprite/barrel-1/image` | Legacy Core sprite barrel_1 | runtime baseline required |
| `core:sprite/barrel-pusher/image` | Legacy Core sprite barrelPusher | runtime baseline required |
| `core:sprite/barrel/image` | Legacy Core sprite barrel | runtime baseline required |
| `core:sprite/base-0/image` | Legacy Core sprite base_0 | runtime baseline required |
| `core:sprite/base-1/image` | Legacy Core sprite base_1 | runtime baseline required |
| `core:sprite/base-10/image` | Legacy Core sprite base_10 | runtime baseline required |
| `core:sprite/base-11/image` | Legacy Core sprite base_11 | runtime baseline required |
| `core:sprite/base-12/image` | Legacy Core sprite base_12 | runtime baseline required |
| `core:sprite/base-13/image` | Legacy Core sprite base_13 | runtime baseline required |
| `core:sprite/base-14/image` | Legacy Core sprite base_14 | runtime baseline required |
| `core:sprite/base-15/image` | Legacy Core sprite base_15 | runtime baseline required |
| `core:sprite/base-16/image` | Legacy Core sprite base_16 | runtime baseline required |
| `core:sprite/base-17/image` | Legacy Core sprite base_17 | runtime baseline required |
| `core:sprite/base-18/image` | Legacy Core sprite base_18 | runtime baseline required |
| `core:sprite/base-19/image` | Legacy Core sprite base_19 | runtime baseline required |
| `core:sprite/base-2/image` | Legacy Core sprite base_2 | runtime baseline required |
| `core:sprite/base-20/image` | Legacy Core sprite base_20 | runtime baseline required |
| `core:sprite/base-21/image` | Legacy Core sprite base_21 | runtime baseline required |
| `core:sprite/base-3/image` | Legacy Core sprite base_3 | runtime baseline required |
| `core:sprite/base-4/image` | Legacy Core sprite base_4 | runtime baseline required |
| `core:sprite/base-5/image` | Legacy Core sprite base_5 | runtime baseline required |
| `core:sprite/base-6/image` | Legacy Core sprite base_6 | runtime baseline required |
| `core:sprite/base-7/image` | Legacy Core sprite base_7 | runtime baseline required |
| `core:sprite/base-8/image` | Legacy Core sprite base_8 | runtime baseline required |
| `core:sprite/base-9/image` | Legacy Core sprite base_9 | runtime baseline required |
| `core:sprite/base/image` | Legacy Core sprite base | runtime baseline required |
| `core:sprite/battery/image` | Legacy Core sprite battery | runtime baseline required |
| `core:sprite/bay-bg/image` | Legacy Core sprite BayBg | runtime baseline required |
| `core:sprite/bay-wall-left/image` | Legacy Core sprite BayWallLeft | runtime baseline required |
| `core:sprite/beam-0/image` | Legacy Core sprite Beam_0 | runtime baseline required |
| `core:sprite/beam/image` | Legacy Core sprite Beam | runtime baseline required |
| `core:sprite/bed-0/image` | Legacy Core sprite Bed_0 | runtime baseline required |
| `core:sprite/bed/image` | Legacy Core sprite Bed | runtime baseline required |
| `core:sprite/bg-0/image` | Legacy Core sprite bg_0 | runtime baseline required |
| `core:sprite/bg-1/image` | Legacy Core sprite bg_1 | runtime baseline required |
| `core:sprite/bg-2/image` | Legacy Core sprite bg_2 | runtime baseline required |
| `core:sprite/bg-3/image` | Legacy Core sprite bg_3 | runtime baseline required |
| `core:sprite/bg-4/image` | Legacy Core sprite bg_4 | runtime baseline required |
| `core:sprite/bg-5/image` | Legacy Core sprite bg_5 | runtime baseline required |
| `core:sprite/bg-basement/image` | Legacy Core sprite bgBasement | runtime baseline required |
| `core:sprite/bg-blue/image` | Legacy Core sprite bgBlue | runtime baseline required |
| `core:sprite/bg-corridor/image` | Legacy Core sprite bgCorridor | runtime baseline required |
| `core:sprite/bg-green-animated1/image` | Legacy Core sprite BgGreenAnimated1 | runtime baseline required |
| `core:sprite/bg-green-animated10/image` | Legacy Core sprite BgGreenAnimated10 | runtime baseline required |
| `core:sprite/bg-green-animated11/image` | Legacy Core sprite BgGreenAnimated11 | runtime baseline required |
| `core:sprite/bg-green-animated12/image` | Legacy Core sprite BgGreenAnimated12 | runtime baseline required |
| `core:sprite/bg-green-animated13/image` | Legacy Core sprite BgGreenAnimated13 | runtime baseline required |
| `core:sprite/bg-green-animated14/image` | Legacy Core sprite BgGreenAnimated14 | runtime baseline required |
| `core:sprite/bg-green-animated15/image` | Legacy Core sprite BgGreenAnimated15 | runtime baseline required |
| `core:sprite/bg-green-animated16/image` | Legacy Core sprite BgGreenAnimated16 | runtime baseline required |
| `core:sprite/bg-green-animated17/image` | Legacy Core sprite BgGreenAnimated17 | runtime baseline required |
| `core:sprite/bg-green-animated2/image` | Legacy Core sprite BgGreenAnimated2 | runtime baseline required |
| `core:sprite/bg-green-animated3/image` | Legacy Core sprite BgGreenAnimated3 | runtime baseline required |
| `core:sprite/bg-green-animated4/image` | Legacy Core sprite BgGreenAnimated4 | runtime baseline required |
| `core:sprite/bg-green-animated5/image` | Legacy Core sprite BgGreenAnimated5 | runtime baseline required |
| `core:sprite/bg-green-animated6/image` | Legacy Core sprite BgGreenAnimated6 | runtime baseline required |
| `core:sprite/bg-green-animated7/image` | Legacy Core sprite BgGreenAnimated7 | runtime baseline required |
| `core:sprite/bg-green-animated8/image` | Legacy Core sprite BgGreenAnimated8 | runtime baseline required |
| `core:sprite/bg-green-animated9/image` | Legacy Core sprite BgGreenAnimated9 | runtime baseline required |
| `core:sprite/bg-ground/image` | Legacy Core sprite bgGround | runtime baseline required |
| `core:sprite/bg-lab/image` | Legacy Core sprite bgLab | runtime baseline required |
| `core:sprite/bg-office/image` | Legacy Core sprite bgOffice | runtime baseline required |
| `core:sprite/bg-purple/image` | Legacy Core sprite bgPurple | runtime baseline required |
| `core:sprite/bg-trees/image` | Legacy Core sprite BgTrees | runtime baseline required |
| `core:sprite/bg/image` | Legacy Core sprite bg | runtime baseline required |
| `core:sprite/bg1/image` | Legacy Core sprite bg1 | runtime baseline required |
| `core:sprite/blood-big1/image` | Legacy Core sprite BloodBig1 | runtime baseline required |
| `core:sprite/blood-big2/image` | Legacy Core sprite BloodBig2 | runtime baseline required |
| `core:sprite/blood-platform/image` | Legacy Core sprite BloodPlatform | runtime baseline required |
| `core:sprite/blood-small1/image` | Legacy Core sprite bloodSmall1 | runtime baseline required |
| `core:sprite/blood-small2/image` | Legacy Core sprite bloodSmall2 | runtime baseline required |
| `core:sprite/blood-small3/image` | Legacy Core sprite bloodSmall3 | runtime baseline required |
| `core:sprite/blood-small4/image` | Legacy Core sprite bloodSmall4 | runtime baseline required |
| `core:sprite/blood-splashes-wall1/image` | Legacy Core sprite BloodSplashesWall1 | runtime baseline required |
| `core:sprite/blood-splashes-wall2/image` | Legacy Core sprite BloodSplashesWall2 | runtime baseline required |
| `core:sprite/blood-splashes-wall3/image` | Legacy Core sprite BloodSplashesWall3 | runtime baseline required |
| `core:sprite/blueprint/image` | Legacy Core sprite blueprint | runtime baseline required |
| `core:sprite/blush-0/image` | Legacy Core sprite Blush_0 | runtime baseline required |
| `core:sprite/blush/image` | Legacy Core sprite Blush | runtime baseline required |
| `core:sprite/brain-0/image` | Legacy Core sprite brain_0 | runtime baseline required |
| `core:sprite/brain-bottle-wood/image` | Legacy Core sprite brainBottleWood | runtime baseline required |
| `core:sprite/brain-bottle/image` | Legacy Core sprite brainBottle | runtime baseline required |
| `core:sprite/brain-glass-overlay/image` | Legacy Core sprite brainGlassOverlay | runtime baseline required |
| `core:sprite/brain-machine-base/image` | Legacy Core sprite brainMachineBase | runtime baseline required |
| `core:sprite/breast-1/image` | Legacy Core sprite Breast_1 | runtime baseline required |
| `core:sprite/breast-2/image` | Legacy Core sprite Breast_2 | runtime baseline required |
| `core:sprite/breast-3/image` | Legacy Core sprite Breast_3 | runtime baseline required |
| `core:sprite/breast-4/image` | Legacy Core sprite Breast_4 | runtime baseline required |
| `core:sprite/breast-clean/image` | Legacy Core sprite BreastClean | runtime baseline required |
| `core:sprite/breast/image` | Legacy Core sprite Breast | runtime baseline required |
| `core:sprite/brow-0/image` | Legacy Core sprite Brow_0 | runtime baseline required |
| `core:sprite/btn-keypad-idle/image` | Legacy Core sprite BtnKeypadIdle | runtime baseline required |
| `core:sprite/btn-keypad-pressed/image` | Legacy Core sprite BtnKeypadPressed | runtime baseline required |
| `core:sprite/btn-menu-exit/image` | Legacy Core sprite BtnMenuExit | runtime baseline required |
| `core:sprite/btn-menu-top-bar-open/image` | Legacy Core sprite BtnMenuTopBarOpen | runtime baseline required |
| `core:sprite/btn-menu/image` | Legacy Core sprite BtnMenu | runtime baseline required |
| `core:sprite/btn-pressed/image` | Legacy Core sprite BtnPressed | runtime baseline required |
| `core:sprite/bubbles-mask/image` | Legacy Core sprite bubblesMask | runtime baseline required |
| `core:sprite/bubbles/image` | Legacy Core sprite bubbles | runtime baseline required |
| `core:sprite/buffout/image` | Legacy Core sprite buffout | runtime baseline required |
| `core:sprite/bullet-0/image` | Legacy Core sprite bullet_0 | runtime baseline required |
| `core:sprite/bullet-1/image` | Legacy Core sprite bullet_1 | runtime baseline required |
| `core:sprite/bullet-2/image` | Legacy Core sprite bullet_2 | runtime baseline required |
| `core:sprite/bullet-3/image` | Legacy Core sprite bullet_3 | runtime baseline required |
| `core:sprite/bullet-4/image` | Legacy Core sprite bullet_4 | runtime baseline required |
| `core:sprite/bullet-5/image` | Legacy Core sprite bullet_5 | runtime baseline required |
| `core:sprite/bullet-6/image` | Legacy Core sprite bullet_6 | runtime baseline required |
| `core:sprite/bullet-7/image` | Legacy Core sprite bullet_7 | runtime baseline required |
| `core:sprite/bullet/image` | Legacy Core sprite bullet | runtime baseline required |
| `core:sprite/butt-11/image` | Legacy Core sprite Butt_11 | runtime baseline required |
| `core:sprite/butt-12/image` | Legacy Core sprite Butt_12 | runtime baseline required |
| `core:sprite/butt-15/image` | Legacy Core sprite Butt_15 | runtime baseline required |
| `core:sprite/butt-2/image` | Core sprite Butt_2 | runtime baseline required |
| `core:sprite/butt-5/image` | Core sprite Butt_5 | runtime baseline required |
| `core:sprite/butt-6/image` | Core sprite Butt_6 | runtime baseline required |
| `core:sprite/butt-7/image` | Legacy Core sprite Butt_7 | runtime baseline required |
| `core:sprite/butt-8/image` | Legacy Core sprite Butt_8 | runtime baseline required |
| `core:sprite/butt-9/image` | Legacy Core sprite Butt_9 | runtime baseline required |
| `core:sprite/butt/image` | Core sprite Butt | runtime baseline required |
| `core:sprite/c-s446/image` | Legacy Core sprite CS446 | runtime baseline required |
| `core:sprite/camp-fire/image` | Legacy Core sprite CampFire | runtime baseline required |
| `core:sprite/case1/image` | Legacy Core sprite Case1 | runtime baseline required |
| `core:sprite/chair-0/image` | Legacy Core sprite Chair_0 | runtime baseline required |
| `core:sprite/chair-1/image` | Legacy Core sprite Chair_1 | runtime baseline required |
| `core:sprite/chair-2/image` | Legacy Core sprite Chair_2 | runtime baseline required |
| `core:sprite/chair/image` | Legacy Core sprite Chair | runtime baseline required |
| `core:sprite/challenge-item-hud-border/image` | Legacy Core sprite ChallengeItemHudBorder | runtime baseline required |
| `core:sprite/chamber-0/image` | Legacy Core sprite chamber_0 | runtime baseline required |
| `core:sprite/chamber/image` | Legacy Core sprite chamber | runtime baseline required |
| `core:sprite/checkmark/image` | Legacy Core sprite Checkmark | runtime baseline required |
| `core:sprite/chest-11/image` | Legacy Core sprite Chest_11 | runtime baseline required |
| `core:sprite/chest-12/image` | Legacy Core sprite Chest_12 | runtime baseline required |
| `core:sprite/chest-15/image` | Legacy Core sprite Chest_15 | runtime baseline required |
| `core:sprite/chest-16/image` | Legacy Core sprite Chest_16 | runtime baseline required |
| `core:sprite/chest-4/image` | Legacy Core sprite Chest_4 | runtime baseline required |
| `core:sprite/chest-5/image` | Core sprite Chest_5 | runtime baseline required |
| `core:sprite/chest-7/image` | Core sprite Chest_7 | runtime baseline required |
| `core:sprite/chest-clean/image` | Legacy Core sprite ChestClean | runtime baseline required |
| `core:sprite/circle-saw/image` | Legacy Core sprite circleSaw | runtime baseline required |
| `core:sprite/door-bg-0/image` | Legacy Core sprite doorBg_0 | runtime baseline required |
| `core:sprite/door-bg/image` | Legacy Core sprite doorBg | runtime baseline required |
| `core:sprite/door-closed-0/image` | Legacy Core sprite DoorClosed_0 | runtime baseline required |
| `core:sprite/door-closed-left/image` | Legacy Core sprite DoorClosedLeft | runtime baseline required |
| `core:sprite/door-closed-right/image` | Legacy Core sprite DoorClosedRight | runtime baseline required |
| `core:sprite/door-closed/image` | Legacy Core sprite doorClosed | runtime baseline required |
| `core:sprite/door-floor/image` | Legacy Core sprite DoorFloor | runtime baseline required |
| `core:sprite/door-hole/image` | Legacy Core sprite doorHole | runtime baseline required |
| `core:sprite/door-office/image` | Legacy Core sprite DoorOffice | runtime baseline required |
| `core:sprite/door-open-left/image` | Legacy Core sprite DoorOpenLeft | runtime baseline required |
| `core:sprite/door-open-right-0/image` | Legacy Core sprite DoorOpenRight_0 | runtime baseline required |
| `core:sprite/door-open-right/image` | Legacy Core sprite DoorOpenRight | runtime baseline required |
| `core:sprite/door-open/image` | Legacy Core sprite doorOpen | runtime baseline required |
| `core:sprite/door-rollable/image` | Legacy Core sprite DoorRollable | runtime baseline required |
| `core:sprite/door-roller/image` | Legacy Core sprite DoorRoller | runtime baseline required |
| `core:sprite/door-visitor-closed-left/image` | Legacy Core sprite DoorVisitorClosedLeft | runtime baseline required |
| `core:sprite/door-visitor-closed-right/image` | Legacy Core sprite DoorVisitorClosedRight | runtime baseline required |
| `core:sprite/door-visitor-open-left/image` | Legacy Core sprite DoorVisitorOpenLeft | runtime baseline required |
| `core:sprite/door-visitor-open-right/image` | Legacy Core sprite DoorVisitorOpenRight | runtime baseline required |
| `core:sprite/door1/image` | Legacy Core sprite Door1 | runtime baseline required |
| `core:sprite/door2/image` | Legacy Core sprite Door2 | runtime baseline required |
| `core:sprite/double-trigger/image` | Legacy Core sprite DoubleTrigger | runtime baseline required |
| `core:sprite/drawing-jenny/image` | Legacy Core sprite DrawingJenny | runtime baseline required |
| `core:sprite/drawing-sunny/image` | Legacy Core sprite DrawingSunny | runtime baseline required |
| `core:sprite/drop-down-bar/image` | Legacy Core sprite DropDownBar | runtime baseline required |
| `core:sprite/drop-down-check-mark/image` | Legacy Core sprite DropDownCheckMark | runtime baseline required |
| `core:sprite/dropdown-arrow/image` | Legacy Core sprite DropdownArrow | runtime baseline required |

### Legacy compatibility (E-H)

These slots follow original Core sprite names and may affect multiple objects that reuse the same sprite. Prefer a semantic slot above when one exists.

| Slot ID | Meaning | Availability |
| --- | --- | --- |
| `core:sprite/ear-broken/image` | Legacy Core sprite EarBroken | runtime baseline required |
| `core:sprite/ear/image` | Legacy Core sprite Ear | runtime baseline required |
| `core:sprite/eye-white-0/image` | Legacy Core sprite EyeWhite_0 | runtime baseline required |
| `core:sprite/eyelid-upper-mask-0/image` | Legacy Core sprite EyelidUpperMask_0 | runtime baseline required |
| `core:sprite/eyelid-upper-mask/image` | Legacy Core sprite EyelidUpperMask | runtime baseline required |
| `core:sprite/eyelid-upper/image` | Legacy Core sprite EyelidUpper | runtime baseline required |
| `core:sprite/eyelid/image` | Legacy Core sprite Eyelid | runtime baseline required |
| `core:sprite/eyewhite/image` | Legacy Core sprite Eyewhite | runtime baseline required |
| `core:sprite/face/image` | Legacy Core sprite face | runtime baseline required |
| `core:sprite/fence-0/image` | Legacy Core sprite fence_0 | runtime baseline required |
| `core:sprite/fence-end/image` | Legacy Core sprite fenceEnd | runtime baseline required |
| `core:sprite/fence/image` | Legacy Core sprite Fence | runtime baseline required |
| `core:sprite/fetus-hud-fill-mask/image` | Legacy Core sprite FetusHudFillMask | runtime baseline required |
| `core:sprite/fetus-hud-fill/image` | Legacy Core sprite FetusHudFill | runtime baseline required |
| `core:sprite/flash-light/image` | Legacy Core sprite FlashLight | runtime baseline required |
| `core:sprite/floor-0/image` | Legacy Core sprite floor_0 | runtime baseline required |
| `core:sprite/floor-1/image` | Legacy Core sprite floor_1 | runtime baseline required |
| `core:sprite/floor-2/image` | Legacy Core sprite floor_2 | runtime baseline required |
| `core:sprite/floor-3/image` | Legacy Core sprite floor_3 | runtime baseline required |
| `core:sprite/floor-4/image` | Legacy Core sprite floor_4 | runtime baseline required |
| `core:sprite/floor-bend/image` | Legacy Core sprite floorBend | runtime baseline required |
| `core:sprite/floor-bg/image` | Legacy Core sprite floorBg | runtime baseline required |
| `core:sprite/floor-climable/image` | Legacy Core sprite floorClimable | runtime baseline required |
| `core:sprite/floor-climbable/image` | Legacy Core sprite floorClimbable | runtime baseline required |
| `core:sprite/floor/image` | Legacy Core sprite floor | runtime baseline required |
| `core:sprite/floor1/image` | Legacy Core sprite floor1 | runtime baseline required |
| `core:sprite/floorbg-0/image` | Legacy Core sprite floorbg_0 | runtime baseline required |
| `core:sprite/floordouble/image` | Legacy Core sprite floordouble | runtime baseline required |
| `core:sprite/floorlower/image` | Legacy Core sprite floorlower | runtime baseline required |
| `core:sprite/foot-base-0/image` | Legacy Core sprite FootBase_0 | runtime baseline required |
| `core:sprite/foot-base-1/image` | Legacy Core sprite FootBase_1 | runtime baseline required |
| `core:sprite/foot-base-2/image` | Legacy Core sprite FootBase_2 | runtime baseline required |
| `core:sprite/foot-base-3/image` | Legacy Core sprite FootBase_3 | runtime baseline required |
| `core:sprite/foot-base/image` | Legacy Core sprite FootBase | runtime baseline required |
| `core:sprite/foot-bloody/image` | Legacy Core sprite FootBloody | runtime baseline required |
| `core:sprite/foot-end-0/image` | Legacy Core sprite FootEnd_0 | runtime baseline required |
| `core:sprite/foot-end-1/image` | Legacy Core sprite FootEnd_1 | runtime baseline required |
| `core:sprite/foot-end-2/image` | Legacy Core sprite FootEnd_2 | runtime baseline required |
| `core:sprite/foot-end-3/image` | Legacy Core sprite FootEnd_3 | runtime baseline required |
| `core:sprite/foot-end/image` | Legacy Core sprite FootEnd | runtime baseline required |
| `core:sprite/foot/image` | Legacy Core sprite Foot | runtime baseline required |
| `core:sprite/generator/image` | Legacy Core sprite Generator | runtime baseline required |
| `core:sprite/ground-seperator/image` | Legacy Core sprite groundSeperator | runtime baseline required |
| `core:sprite/ground/image` | Legacy Core sprite ground | runtime baseline required |
| `core:sprite/gun-rig/image` | Legacy Core sprite gunRig | runtime baseline required |
| `core:sprite/halman/image` | Legacy Core sprite halman | runtime baseline required |
| `core:sprite/hand-16/image` | Legacy Core sprite Hand_16 | runtime baseline required |
| `core:sprite/hand-18/image` | Legacy Core sprite Hand_18 | runtime baseline required |
| `core:sprite/hand-19/image` | Legacy Core sprite Hand_19 | runtime baseline required |
| `core:sprite/hand-2/image` | Legacy Core sprite Hand_2 | runtime baseline required |
| `core:sprite/hand-4/image` | Legacy Core sprite Hand_4 | runtime baseline required |
| `core:sprite/hand-8/image` | Legacy Core sprite Hand_8 | runtime baseline required |
| `core:sprite/hand-9/image` | Legacy Core sprite Hand_9 | runtime baseline required |
| `core:sprite/hand-broken/image` | Legacy Core sprite HandBroken | runtime baseline required |
| `core:sprite/harrington/image` | Legacy Core sprite harrington | runtime baseline required |
| `core:sprite/head-11/image` | Legacy Core sprite Head_11 | runtime baseline required |
| `core:sprite/head-16/image` | Legacy Core sprite Head_16 | runtime baseline required |
| `core:sprite/head-17/image` | Legacy Core sprite Head_17 | runtime baseline required |
| `core:sprite/head-18/image` | Legacy Core sprite Head_18 | runtime baseline required |
| `core:sprite/head-19/image` | Legacy Core sprite Head_19 | runtime baseline required |
| `core:sprite/head-8/image` | Legacy Core sprite Head_8 | runtime baseline required |
| `core:sprite/head-fear/image` | Legacy Core sprite HeadFear | runtime baseline required |
| `core:sprite/head-turned-off-0/image` | Legacy Core sprite HeadTurnedOff_0 | runtime baseline required |
| `core:sprite/head-turned-off/image` | Legacy Core sprite HeadTurnedOff | runtime baseline required |
| `core:sprite/head-w-i-p/image` | Legacy Core sprite HeadWIP | runtime baseline required |
| `core:sprite/head1/image` | Legacy Core sprite Head1 | runtime baseline required |
| `core:sprite/head2/image` | Legacy Core sprite Head2 | runtime baseline required |
| `core:sprite/head3/image` | Legacy Core sprite Head3 | runtime baseline required |
| `core:sprite/head4/image` | Legacy Core sprite Head4 | runtime baseline required |
| `core:sprite/heart-dead/image` | Legacy Core sprite HeartDead | runtime baseline required |
| `core:sprite/heart-healthy/image` | Legacy Core sprite HeartHealthy | runtime baseline required |
| `core:sprite/hip-3/image` | Legacy Core sprite Hip_3 | runtime baseline required |
| `core:sprite/hip-5/image` | Legacy Core sprite Hip_5 | runtime baseline required |
| `core:sprite/hips-1/image` | Legacy Core sprite Hips_1 | runtime baseline required |
| `core:sprite/hips-7/image` | Legacy Core sprite Hips_7 | runtime baseline required |
| `core:sprite/hut-bg/image` | Legacy Core sprite HutBg | runtime baseline required |
| `core:sprite/hut-door/image` | Legacy Core sprite HutDoor | runtime baseline required |

### Legacy compatibility (I-L)

These slots follow original Core sprite names and may affect multiple objects that reuse the same sprite. Prefer a semantic slot above when one exists.

| Slot ID | Meaning | Availability |
| --- | --- | --- |
| `core:sprite/icon-0/image` | Legacy Core sprite Icon_0 | runtime baseline required |
| `core:sprite/icon-6/image` | Legacy Core sprite Icon_6 | runtime baseline required |
| `core:sprite/icon-7/image` | Legacy Core sprite Icon_7 | runtime baseline required |
| `core:sprite/icon-9/image` | Legacy Core sprite Icon_9 | runtime baseline required |
| `core:sprite/jacky-door-light/image` | Legacy Core sprite JackyDoorLight | runtime baseline required |
| `core:sprite/keypad/image` | Legacy Core sprite Keypad | runtime baseline required |
| `core:sprite/knife/image` | Legacy Core sprite knife | runtime baseline required |
| `core:sprite/lab/image` | Legacy Core sprite Lab | runtime baseline required |
| `core:sprite/lady-statue/image` | Legacy Core sprite LadyStatue | runtime baseline required |
| `core:sprite/leg-lower-10/image` | Legacy Core sprite LegLower_10 | runtime baseline required |
| `core:sprite/leg-lower-11/image` | Legacy Core sprite LegLower_11 | runtime baseline required |
| `core:sprite/leg-lower-13/image` | Legacy Core sprite LegLower_13 | runtime baseline required |
| `core:sprite/leg-lower-15/image` | Legacy Core sprite LegLower_15 | runtime baseline required |
| `core:sprite/leg-lower-3/image` | Legacy Core sprite LegLower_3 | runtime baseline required |
| `core:sprite/leg-lower-4/image` | Legacy Core sprite LegLower_4 | runtime baseline required |
| `core:sprite/leg-lower-8/image` | Legacy Core sprite LegLower_8 | runtime baseline required |
| `core:sprite/leg-lower-bloody-0/image` | Legacy Core sprite LegLowerBloody_0 | runtime baseline required |
| `core:sprite/leg-lower-bloody/image` | Legacy Core sprite LegLowerBloody | runtime baseline required |
| `core:sprite/leg-lower-broken/image` | Legacy Core sprite LegLowerBroken | runtime baseline required |
| `core:sprite/leg-upper-1/image` | Legacy Core sprite LegUpper_1 | runtime baseline required |
| `core:sprite/leg-upper-12/image` | Legacy Core sprite LegUpper_12 | runtime baseline required |
| `core:sprite/leg-upper-17/image` | Legacy Core sprite LegUpper_17 | runtime baseline required |
| `core:sprite/leg-upper-3/image` | Legacy Core sprite LegUpper_3 | runtime baseline required |
| `core:sprite/leg-upper-4/image` | Legacy Core sprite LegUpper_4 | runtime baseline required |
| `core:sprite/leg-upper-5/image` | Legacy Core sprite LegUpper_5 | runtime baseline required |
| `core:sprite/leg-upper-6/image` | Legacy Core sprite LegUpper_6 | runtime baseline required |
| `core:sprite/leg-upper-bloody/image` | Legacy Core sprite LegUpperBloody | runtime baseline required |
| `core:sprite/libido-heart/image` | Legacy Core sprite LibidoHeart | runtime baseline required |
| `core:sprite/light-switch-off/image` | Legacy Core sprite lightSwitchOff | runtime baseline required |
| `core:sprite/lk235/image` | Legacy Core sprite lk235 | runtime baseline required |

### Legacy compatibility (M-P)

These slots follow original Core sprite names and may affect multiple objects that reuse the same sprite. Prefer a semantic slot above when one exists.

| Slot ID | Meaning | Availability |
| --- | --- | --- |
| `core:sprite/m4-b1/image` | Legacy Core sprite M4B1 | runtime baseline required |
| `core:sprite/magazine-0/image` | Legacy Core sprite magazine_0 | runtime baseline required |
| `core:sprite/magazine-1/image` | Legacy Core sprite magazine_1 | runtime baseline required |
| `core:sprite/magazine-10/image` | Legacy Core sprite magazine_10 | runtime baseline required |
| `core:sprite/magazine-11/image` | Legacy Core sprite magazine_11 | runtime baseline required |
| `core:sprite/magazine-12/image` | Legacy Core sprite magazine_12 | runtime baseline required |
| `core:sprite/magazine-3/image` | Legacy Core sprite magazine_3 | runtime baseline required |
| `core:sprite/magazine-4/image` | Legacy Core sprite magazine_4 | runtime baseline required |
| `core:sprite/magazine-5/image` | Legacy Core sprite magazine_5 | runtime baseline required |
| `core:sprite/magazine-6/image` | Legacy Core sprite magazine_6 | runtime baseline required |
| `core:sprite/magazine-7/image` | Legacy Core sprite magazine_7 | runtime baseline required |
| `core:sprite/magazine-9/image` | Legacy Core sprite magazine_9 | runtime baseline required |
| `core:sprite/magazine/image` | Legacy Core sprite magazine | runtime baseline required |
| `core:sprite/medic-body/image` | Legacy Core sprite MedicBody | runtime baseline required |
| `core:sprite/mg32/image` | Legacy Core sprite mg32 | runtime baseline required |
| `core:sprite/morphine/image` | Legacy Core sprite Morphine | runtime baseline required |
| `core:sprite/mouth-oral1/image` | Legacy Core sprite MouthOral1 | runtime baseline required |
| `core:sprite/mouth-oral2/image` | Legacy Core sprite MouthOral2 | runtime baseline required |
| `core:sprite/mouth-oral3/image` | Legacy Core sprite MouthOral3 | runtime baseline required |
| `core:sprite/mouth-tired1/image` | Legacy Core sprite MouthTired1 | runtime baseline required |
| `core:sprite/mouth-tired2/image` | Legacy Core sprite MouthTired2 | runtime baseline required |
| `core:sprite/mouth10/image` | Legacy Core sprite Mouth10 | runtime baseline required |
| `core:sprite/mouth12/image` | Legacy Core sprite Mouth12 | runtime baseline required |
| `core:sprite/mouth2/image` | Legacy Core sprite Mouth2 | runtime baseline required |
| `core:sprite/mouth3/image` | Legacy Core sprite Mouth3 | runtime baseline required |
| `core:sprite/mouth4/image` | Legacy Core sprite Mouth4 | runtime baseline required |
| `core:sprite/mouth5/image` | Legacy Core sprite Mouth5 | runtime baseline required |
| `core:sprite/mouth6/image` | Legacy Core sprite Mouth6 | runtime baseline required |
| `core:sprite/mouth7/image` | Legacy Core sprite Mouth7 | runtime baseline required |
| `core:sprite/mouth8/image` | Legacy Core sprite Mouth8 | runtime baseline required |
| `core:sprite/mouth9/image` | Legacy Core sprite Mouth9 | runtime baseline required |
| `core:sprite/muger/image` | Legacy Core sprite muger | runtime baseline required |
| `core:sprite/muzzle-flash1/image` | Legacy Core sprite MuzzleFlash1 | runtime baseline required |
| `core:sprite/muzzle-flash2/image` | Legacy Core sprite MuzzleFlash2 | runtime baseline required |
| `core:sprite/muzzle-flash3/image` | Legacy Core sprite MuzzleFlash3 | runtime baseline required |
| `core:sprite/n-p40/image` | Legacy Core sprite NP40 | runtime baseline required |
| `core:sprite/neck-11/image` | Legacy Core sprite Neck_11 | runtime baseline required |
| `core:sprite/neck-4/image` | Legacy Core sprite Neck_4 | runtime baseline required |
| `core:sprite/neck-5/image` | Legacy Core sprite Neck_5 | runtime baseline required |
| `core:sprite/neck-8/image` | Legacy Core sprite Neck_8 | runtime baseline required |
| `core:sprite/no-entry/image` | Legacy Core sprite noEntry | runtime baseline required |
| `core:sprite/note-keycode/image` | Legacy Core sprite noteKeycode | runtime baseline required |
| `core:sprite/note-wall/image` | Legacy Core sprite noteWall | runtime baseline required |
| `core:sprite/np9/image` | Legacy Core sprite np9 | runtime baseline required |
| `core:sprite/overlay-hole/image` | Legacy Core sprite OverlayHole | runtime baseline required |
| `core:sprite/overlay/image` | Legacy Core sprite Overlay | runtime baseline required |
| `core:sprite/painting/image` | Legacy Core sprite painting | runtime baseline required |
| `core:sprite/penis-0/image` | Legacy Core sprite penis_0 | runtime baseline required |
| `core:sprite/penis-1/image` | Legacy Core sprite penis_1 | runtime baseline required |
| `core:sprite/penis-2/image` | Legacy Core sprite Penis_2 | runtime baseline required |
| `core:sprite/penis-3/image` | Legacy Core sprite Penis_3 | runtime baseline required |
| `core:sprite/penis-4/image` | Legacy Core sprite Penis_4 | runtime baseline required |
| `core:sprite/penis-5/image` | Legacy Core sprite penis_5 | runtime baseline required |
| `core:sprite/penis-6/image` | Legacy Core sprite Penis_6 | runtime baseline required |
| `core:sprite/penis-base-0/image` | Legacy Core sprite PenisBase_0 | runtime baseline required |
| `core:sprite/penis-base-1/image` | Legacy Core sprite PenisBase_1 | runtime baseline required |
| `core:sprite/penis-base-2/image` | Legacy Core sprite PenisBase_2 | runtime baseline required |
| `core:sprite/penis-base-3/image` | Legacy Core sprite PenisBase_3 | runtime baseline required |
| `core:sprite/penis-base-4/image` | Legacy Core sprite PenisBase_4 | runtime baseline required |
| `core:sprite/penis-base-5/image` | Legacy Core sprite PenisBase_5 | runtime baseline required |
| `core:sprite/penis-base-6/image` | Legacy Core sprite PenisBase_6 | runtime baseline required |
| `core:sprite/penis-base/image` | Legacy Core sprite PenisBase | runtime baseline required |
| `core:sprite/penis-end-0/image` | Legacy Core sprite PenisEnd_0 | runtime baseline required |
| `core:sprite/penis-end-1/image` | Legacy Core sprite PenisEnd_1 | runtime baseline required |
| `core:sprite/penis-end-2/image` | Legacy Core sprite PenisEnd_2 | runtime baseline required |
| `core:sprite/penis-end-3/image` | Legacy Core sprite PenisEnd_3 | runtime baseline required |
| `core:sprite/penis-end/image` | Legacy Core sprite PenisEnd | runtime baseline required |
| `core:sprite/penis-mid-0/image` | Legacy Core sprite PenisMid_0 | runtime baseline required |
| `core:sprite/penis-mid-1/image` | Legacy Core sprite PenisMid_1 | runtime baseline required |
| `core:sprite/penis-mid/image` | Legacy Core sprite PenisMid | runtime baseline required |
| `core:sprite/penis-middle/image` | Legacy Core sprite PenisMiddle | runtime baseline required |
| `core:sprite/penis-rod-0/image` | Legacy Core sprite PenisRod_0 | runtime baseline required |
| `core:sprite/penis-rod/image` | Legacy Core sprite PenisRod | runtime baseline required |
| `core:sprite/penis-tip-0/image` | Legacy Core sprite PenisTip_0 | runtime baseline required |
| `core:sprite/penis-tip-1/image` | Legacy Core sprite PenisTip_1 | runtime baseline required |
| `core:sprite/penis-tip-2/image` | Legacy Core sprite PenisTip_2 | runtime baseline required |
| `core:sprite/penis-tip/image` | Legacy Core sprite PenisTip | runtime baseline required |
| `core:sprite/penis/image` | Legacy Core sprite Penis | runtime baseline required |
| `core:sprite/pipe-fire/image` | Legacy Core sprite pipeFire | runtime baseline required |
| `core:sprite/pistol/image` | Legacy Core sprite pistol | runtime baseline required |
| `core:sprite/plate-of-food/image` | Legacy Core sprite plateOfFood | runtime baseline required |
| `core:sprite/platform-0/image` | Legacy Core sprite platform_0 | runtime baseline required |
| `core:sprite/platform-climable-left/image` | Legacy Core sprite platformClimableLeft | runtime baseline required |
| `core:sprite/platform-climable-right/image` | Legacy Core sprite platformClimableRight | runtime baseline required |
| `core:sprite/platform-climable/image` | Legacy Core sprite platformClimable | runtime baseline required |
| `core:sprite/platform/image` | Legacy Core sprite platform | runtime baseline required |
| `core:sprite/poster-wife-bedroom/image` | Legacy Core sprite PosterWifeBedroom | runtime baseline required |
| `core:sprite/poster/image` | Legacy Core sprite poster | runtime baseline required |
| `core:sprite/psycho/image` | Legacy Core sprite psycho | runtime baseline required |
| `core:sprite/pump/image` | Legacy Core sprite pump | runtime baseline required |

### Legacy compatibility (Q-T)

These slots follow original Core sprite names and may affect multiple objects that reuse the same sprite. Prefer a semantic slot above when one exists.

| Slot ID | Meaning | Availability |
| --- | --- | --- |
| `core:sprite/qlock17-a/image` | Legacy Core sprite Qlock17A | runtime baseline required |
| `core:sprite/qlock17/image` | Legacy Core sprite Qlock17 | runtime baseline required |
| `core:sprite/reload-light/image` | Legacy Core sprite reloadLight | runtime baseline required |
| `core:sprite/revolver/image` | Legacy Core sprite revolver | runtime baseline required |
| `core:sprite/ronigsberg/image` | Legacy Core sprite ronigsberg | runtime baseline required |
| `core:sprite/rontgen/image` | Legacy Core sprite rontgen | runtime baseline required |
| `core:sprite/s-i-i-d-21/image` | Legacy Core sprite SIID-21 | runtime baseline required |
| `core:sprite/s-s-p-h42/image` | Legacy Core sprite SSPH42 | runtime baseline required |
| `core:sprite/saw/image` | Legacy Core sprite saw | runtime baseline required |
| `core:sprite/sawed-off/image` | Legacy Core sprite SawedOff | runtime baseline required |
| `core:sprite/schockgewehr/image` | Legacy Core sprite schockgewehr | runtime baseline required |
| `core:sprite/shack-front-door-roof/image` | Legacy Core sprite shackFrontDoorRoof | runtime baseline required |
| `core:sprite/shell/image` | Legacy Core sprite shell | runtime baseline required |
| `core:sprite/shitty-table/image` | Legacy Core sprite shittyTable | runtime baseline required |
| `core:sprite/shoe-base/image` | Legacy Core sprite ShoeBase | runtime baseline required |
| `core:sprite/shoe-end/image` | Legacy Core sprite ShoeEnd | runtime baseline required |
| `core:sprite/skirt/image` | Legacy Core sprite Skirt | runtime baseline required |
| `core:sprite/sldefender/image` | Legacy Core sprite sldefender | runtime baseline required |
| `core:sprite/slide-0/image` | Legacy Core sprite slide_0 | runtime baseline required |
| `core:sprite/slide-1/image` | Legacy Core sprite slide_1 | runtime baseline required |
| `core:sprite/slide-2/image` | Legacy Core sprite slide_2 | runtime baseline required |
| `core:sprite/slide/image` | Legacy Core sprite slide | runtime baseline required |
| `core:sprite/smg/image` | Legacy Core sprite smg | runtime baseline required |
| `core:sprite/snub/image` | Legacy Core sprite snub | runtime baseline required |
| `core:sprite/spine-0/image` | Legacy Core sprite Spine_0 | runtime baseline required |
| `core:sprite/spine-2/image` | Legacy Core sprite Spine_2 | runtime baseline required |
| `core:sprite/spine-3/image` | Legacy Core sprite Spine_3 | runtime baseline required |
| `core:sprite/spine-5/image` | Legacy Core sprite Spine_5 | runtime baseline required |
| `core:sprite/spine-9/image` | Legacy Core sprite Spine_9 | runtime baseline required |
| `core:sprite/stage/image` | Legacy Core sprite Stage | runtime baseline required |
| `core:sprite/stamina-bar-danger/image` | Legacy Core sprite StaminaBarDanger | runtime baseline required |
| `core:sprite/stamina-bar-empty/image` | Legacy Core sprite StaminaBarEmpty | runtime baseline required |
| `core:sprite/stamina-bar-fill/image` | Legacy Core sprite StaminaBarFill | runtime baseline required |
| `core:sprite/status-player-hud-item/image` | Legacy Core sprite StatusPlayerHudItem | runtime baseline required |
| `core:sprite/surgery-bed-jacky/image` | Legacy Core sprite surgeryBedJacky | runtime baseline required |
| `core:sprite/surgery-bed/image` | Legacy Core sprite surgeryBed | runtime baseline required |
| `core:sprite/table-0/image` | Legacy Core sprite table_0 | runtime baseline required |
| `core:sprite/table-1/image` | Legacy Core sprite table_1 | runtime baseline required |
| `core:sprite/table-right/image` | Legacy Core sprite tableRight | runtime baseline required |
| `core:sprite/table-wall/image` | Legacy Core sprite tableWall | runtime baseline required |
| `core:sprite/table/image` | Legacy Core sprite Table | runtime baseline required |
| `core:sprite/table1/image` | Legacy Core sprite Table1 | runtime baseline required |
| `core:sprite/table2/image` | Legacy Core sprite Table2 | runtime baseline required |
| `core:sprite/tenelli/image` | Legacy Core sprite tenelli | runtime baseline required |
| `core:sprite/tenelliso3/image` | Legacy Core sprite tenelliso3 | runtime baseline required |
| `core:sprite/theatre-back-background/image` | Legacy Core sprite theatreBackBackground | runtime baseline required |
| `core:sprite/theatre-bg/image` | Legacy Core sprite TheatreBg | runtime baseline required |
| `core:sprite/theatre-seat/image` | Legacy Core sprite theatreSeat | runtime baseline required |
| `core:sprite/title-bg/image` | Legacy Core sprite titleBg | runtime baseline required |
| `core:sprite/torso-lower-0/image` | Core sprite TorsoLower_0 | runtime baseline required |
| `core:sprite/torso-lower-8/image` | Legacy Core sprite TorsoLower_8 | runtime baseline required |
| `core:sprite/tree-mid/image` | Legacy Core sprite treeMid | runtime baseline required |
| `core:sprite/tree-platform-0/image` | Legacy Core sprite treePlatform_0 | runtime baseline required |
| `core:sprite/tree-platform/image` | Legacy Core sprite treePlatform | runtime baseline required |
| `core:sprite/tree-root/image` | Legacy Core sprite treeRoot | runtime baseline required |
| `core:sprite/tv-chair/image` | Legacy Core sprite tvChair | runtime baseline required |

### Legacy compatibility (U-Z)

These slots follow original Core sprite names and may affect multiple objects that reuse the same sprite. Prefer a semantic slot above when one exists.

| Slot ID | Meaning | Availability |
| --- | --- | --- |
| `core:sprite/u-i-check-mark/image` | Legacy Core sprite UICheckMark | runtime baseline required |
| `core:sprite/upper-floor/image` | Legacy Core sprite upperFloor | runtime baseline required |
| `core:sprite/usable-vendor/image` | Legacy Core sprite UsableVendor | runtime baseline required |
| `core:sprite/vegetationfloor/image` | Legacy Core sprite vegetationfloor | runtime baseline required |
| `core:sprite/wall-0/image` | Legacy Core sprite wall_0 | runtime baseline required |
| `core:sprite/wall-1/image` | Legacy Core sprite Wall_1 | runtime baseline required |
| `core:sprite/wall-2/image` | Legacy Core sprite Wall_2 | runtime baseline required |
| `core:sprite/wall-3/image` | Legacy Core sprite Wall_3 | runtime baseline required |
| `core:sprite/wall-4/image` | Legacy Core sprite wall_4 | runtime baseline required |
| `core:sprite/wall-5/image` | Legacy Core sprite wall_5 | runtime baseline required |
| `core:sprite/wall-bottom-closed/image` | Legacy Core sprite WallBottomClosed | runtime baseline required |
| `core:sprite/wall-light/image` | Legacy Core sprite wallLight | runtime baseline required |
| `core:sprite/wall-pillar/image` | Legacy Core sprite WallPillar | runtime baseline required |
| `core:sprite/wall-scratches-medic-room/image` | Legacy Core sprite WallScratchesMedicRoom | runtime baseline required |
| `core:sprite/wall/image` | Legacy Core sprite wall | runtime baseline required |
| `core:sprite/wall2/image` | Legacy Core sprite Wall2 | runtime baseline required |
| `core:sprite/wardrobe/image` | Legacy Core sprite Wardrobe | runtime baseline required |
| `core:sprite/weapon-vendor/image` | Legacy Core sprite WeaponVendor | runtime baseline required |
| `core:sprite/whiteboard/image` | Legacy Core sprite whiteboard | runtime baseline required |
| `core:sprite/yard-grass-platform/image` | Legacy Core sprite YardGrassPlatform | runtime baseline required |
