# Core enemy asset slots

This page is generated from the same normalized rig metadata used by the runtime. Do not edit it by hand. Run **Captivity Reloaded > Modding > Refresh Core Enemy Asset Slot Catalog** after changing a Core rig.

The catalog currently publishes 417 enemy-specific sprite slots across 21 Core enemies. Target the enemy ID in an `assetPatch` and use the relative slot shown below. Enemy-specific multipart anatomy slots are preferred over the older global `core:enemy-anatomy` aliases. A `/variant/` slot replaces artwork assigned only by an animation.

## `core:enemy/abby`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hips_1` | Default |
| `body/butt` | `butt` | `Butt_12` | Default |
| `body/leg-left-upper` | `leg-left-upper` | `LegUpper_17` | Default |
| `body/penis-rod` | `penis-rod` | `PenisRod_0` | Default |
| `body/leg-right-upper` | `leg-right-upper` | `LegUpper_17` | Default |
| `body/torso-lower` | `spine` | `Spine_5` | Default |
| `body/leg-left-lower` | `leg-left-lower` | `LegLowerBroken` | Default |
| `body/penis-base` | `penis-base` | `PenisBase_0` | Default |
| `body/leg-right-lower` | `leg-right-lower` | `LegLower_4` | Default |
| `body/chest` | `chest` | `Chest_12` | Default |
| `body/l-foot-base` | `l-foot-base` | `FootBase_1` | Default |
| `body/penis-end` | `penis-end` | `PenisEnd_0` | Default |
| `body/r-foot-base` | `r-foot-base` | `FootBase_1` | Default |
| `body/breast` | `breast` | `Breast` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper_16` | Default |
| `body/neck` | `neck` | `Neck_4` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper_16` | Default |
| `body/l-foot-end` | `l-foot-end` | `FootEnd_1` | Default |
| `body/r-foot-end` | `r-foot-end` | `FootEnd_1` | Default |
| `body/breast-bg` | `breast-bg` | `BreastBg_0` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_8` | Default |
| `body/head` | `head` | `Head_17` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLowerBloody` | Default |
| `body/hand-left` | `hand-left` | `Hand_18` | Default |
| `body/hand-right` | `hand-right` | `HandBroken` | Default |
| `body/l-ear` | `l-ear` | `EarBroken` | Default |
| `body/r-ear` | `r-ear` | `Ear` | Default |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/hand` replaces: `body/hand-left`, `body/hand-right`
- `body/leg-upper` replaces: `body/leg-left-upper`, `body/leg-right-upper`
- `body/leg-lower` replaces: `body/leg-left-lower`, `body/leg-right-lower`

## `core:enemy/death-hound`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `TorsoLower_2` | Default |
| `body/l-leg-back-upper` | `l-leg-back-upper` | `LegBackUpper` | Default |
| `body/penis` | `penis` | `Penis_2` | Default |
| `body/r-leg-back-upper` | `r-leg-back-upper` | `LegBackUpper` | Default |
| `body/torso-lower` | `spine` | `TorsoUpper` | Default |
| `body/tail` | `tail` | `Tail` | Default |
| `body/l-leg-back-middle` | `l-leg-back-middle` | `LegBackMiddle` | Default |
| `body/r-leg-back-middle` | `r-leg-back-middle` | `LegBackMiddle` | Default |
| `body/l-leg-front-upper` | `l-leg-front-upper` | `LegFrontUpper_0` | Default |
| `body/neck` | `neck` | `Neck_3` | Default |
| `body/r-leg-front-upper` | `r-leg-front-upper` | `LegFrontUpper_0` | Default |
| `body/l-leg-back-lower` | `l-leg-back-lower` | `LegBackLower` | Default |
| `body/r-leg-back-lower` | `r-leg-back-lower` | `LegBackLower` | Default |
| `body/l-leg-front-lower` | `l-leg-front-lower` | `LegFrontLower` | Default |
| `body/head` | `head` | `Head_9` | Default |
| `body/r-leg-front-lower` | `r-leg-front-lower` | `LegFrontLower` | Default |
| `body/l-foot-back` | `l-foot-back` | `FootBack` | Default |
| `body/r-foot-back` | `r-foot-back` | `FootBack` | Default |
| `body/l-foot-front` | `l-foot-front` | `FootFront` | Default |
| `body/jaw` | `jaw` | `Jaw` | Default |
| `body/r-foot-front` | `r-foot-front` | `FootFront` | Default |

## `core:enemy/fly`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hips_4` | Default |
| `body/chest` | `chest` | `Chest_20` | Default |
| `body/leg-back-upper` | `leg-back-upper` | `LegUpper_11` | Default |
| `body/penis-base` | `penis-base` | `PenisBase_4` | Default |
| `body/head` | `head` | `Head_20` | Default |
| `body/leg-front-upper` | `leg-front-upper` | `LegUpper_11` | Default |
| `body/leg-mid-upper` | `leg-mid-upper` | `LegUpper_11` | Default |
| `body/wing` | `wing` | `Wing` | Default |
| `body/leg-back-mid` | `leg-back-mid` | `LegMid` | Default |
| `body/penis-mid` | `penis-mid` | `PenisMid` | Default |
| `body/leg-front-mid` | `leg-front-mid` | `LegMid` | Default |
| `body/leg-mid-mid` | `leg-mid-mid` | `LegMid` | Default |
| `body/leg-back-lower` | `leg-back-lower` | `LegLower_5` | Default |
| `body/penis-tip` | `penis-tip` | `PenisTip_2` | Default |
| `body/leg-front-lower` | `leg-front-lower` | `LegLower_5` | Default |
| `body/leg-mid-lower` | `leg-mid-lower` | `LegLower_5` | Default |

## `core:enemy/goblin-marksman`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hips` | Default |
| `body/leg-left-upper` | `leg-left-upper` | `LegUpper` | Default |
| `body/penis` | `penis` | `Penis` | Default |
| `body/leg-right-upper` | `leg-right-upper` | `LegUpper` | Default |
| `body/torso-lower` | `spine` | `Spine_4` | Default |
| `body/leg-left-lower` | `leg-left-lower` | `LegLower_6` | Default |
| `body/leg-right-lower` | `leg-right-lower` | `LegLower_6` | Default |
| `body/chest` | `chest` | `Chest_21` | Default |
| `body/foot-left` | `foot-left` | `Foot_2` | Default |
| `body/foot-right` | `foot-right` | `Foot_2` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper_15` | Default |
| `body/neck` | `neck` | `Neck_13` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper_15` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_6` | Default |
| `body/head` | `head` | `Head_7` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLower_6` | Default |
| `body/hand-left` | `hand-left` | `Hand_10` | Default |
| `body/hand-right` | `hand-right` | `Hand_10` | Default |
| `body/l-weapon` | `l-weapon` | `Knife_0` | Default |
| `body/r-weapon` | `r-weapon` | `Blowgun` | Default |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/hand` replaces: `body/hand-left`, `body/hand-right`
- `body/leg-upper` replaces: `body/leg-left-upper`, `body/leg-right-upper`
- `body/leg-lower` replaces: `body/leg-left-lower`, `body/leg-right-lower`

## `core:enemy/goblin-trapper`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hips_2` | Default |
| `body/leg-left-upper` | `leg-left-upper` | `LegUpper_20` | Default |
| `body/penis` | `penis` | `Penis_3` | Default |
| `body/leg-right-upper` | `leg-right-upper` | `LegUpper_20` | Default |
| `body/torso-lower` | `spine` | `Spine` | Default |
| `body/leg-left-lower` | `leg-left-lower` | `LegLower_7` | Default |
| `body/leg-right-lower` | `leg-right-lower` | `LegLower_7` | Default |
| `body/chest` | `chest` | `Chest_18` | Default |
| `body/foot-left` | `foot-left` | `Foot_7` | Default |
| `body/foot-right` | `foot-right` | `Foot_7` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper_11` | Default |
| `body/neck` | `neck` | `Neck_1` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper_11` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_7` | Default |
| `body/head` | `head` | `Head_12` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLower_7` | Default |
| `body/hand-left` | `hand-left` | `Hand_1` | Default |
| `body/hand-right` | `hand-right` | `Hand_1` | Default |
| `body/l-weapon` | `l-weapon` | `Club` | Default |
| `body/r-weapon` | `r-weapon` | `Trap` | Default |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/hand` replaces: `body/hand-left`, `body/hand-right`
- `body/leg-upper` replaces: `body/leg-left-upper`, `body/leg-right-upper`
- `body/leg-lower` replaces: `body/leg-left-lower`, `body/leg-right-lower`

## `core:enemy/gremlin`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hips_8` | Default |
| `body/leg-left-upper` | `leg-left-upper` | `LegUpper_0` | Default |
| `body/penis` | `penis` | `Penis_4` | Default |
| `body/leg-right-upper` | `leg-right-upper` | `LegUpper_0` | Default |
| `body/torso-lower` | `spine` | `Spine_6` | Default |
| `body/leg-left-lower` | `leg-left-lower` | `LegLower_17` | Default |
| `body/leg-right-lower` | `leg-right-lower` | `LegLower_17` | Default |
| `body/chest` | `chest` | `Chest_17` | Default |
| `body/foot-left` | `foot-left` | `Foot_10` | Default |
| `body/foot-right` | `foot-right` | `Foot_10` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper_3` | Default |
| `body/neck` | `neck` | `Neck_15` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper_3` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_5` | Default |
| `body/head` | `head` | `Head_14` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLower_5` | Default |
| `body/hand-left` | `hand-left` | `Hand_13` | Default |
| `body/hand-right` | `hand-right` | `Hand_13` | Default |
| `body/l-weapon` | `l-weapon` | `Syringe` | Default |
| `body/r-weapon` | `r-weapon` | `Stick` | Default |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/hand` replaces: `body/hand-left`, `body/hand-right`
- `body/leg-upper` replaces: `body/leg-left-upper`, `body/leg-right-upper`
- `body/leg-lower` replaces: `body/leg-left-lower`, `body/leg-right-lower`

## `core:enemy/head-humper`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hips_5` | Default |
| `body/l-leg-back-base` | `l-leg-back-base` | `LegBackBase` | Default |
| `body/penis-base` | `penis-base` | `Penis_6` | Default |
| `body/r-leg-back-base` | `r-leg-back-base` | `LegBackBase` | Default |
| `body/sack` | `sack` | `Sack` | Default |
| `body/torso-front` | `torso-front` | `TorsoFront` | Default |
| `body/torso-back` | `torso-back` | `TorsoBack` | Default |
| `body/l-leg-back-mid` | `l-leg-back-mid` | `LegBackMid` | Default |
| `body/penis1` | `penis1` | `Penis_6` | Default |
| `body/r-leg-back-mid` | `r-leg-back-mid` | `LegBackMid` | Default |
| `body/l-leg-front-base` | `l-leg-front-base` | `LegFrontBase` | Default |
| `body/r-leg-front-base` | `r-leg-front-base` | `LegFrontBase` | Default |
| `body/l-leg-back-lower` | `l-leg-back-lower` | `LegBackLower_0` | Default |
| `body/penis2` | `penis2` | `Penis_6` | Default |
| `body/r-leg-back-lower` | `r-leg-back-lower` | `LegBackLower_0` | Default |
| `body/l-leg-front-mid` | `l-leg-front-mid` | `LegFrontMid` | Default |
| `body/r-leg-front-mid` | `r-leg-front-mid` | `LegFrontMid` | Default |
| `body/penis3` | `penis3` | `Penis_6` | Default |
| `body/l-leg-front-lower` | `l-leg-front-lower` | `LegFrontLower_0` | Default |
| `body/r-leg-front-lower` | `r-leg-front-lower` | `LegFrontLower_0` | Default |
| `body/penis-tip` | `penis-tip` | `Penis_6` | Default |

## `core:enemy/hunter`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hip_1` | Default |
| `body/balls` | `balls` | `Balls` | Default |
| `body/chest` | `chest` | `Chest_9` | Default |
| `body/leg-left-upper` | `leg-left-upper` | `LegUpper_9` | Default |
| `body/leg-right-upper` | `leg-right-upper` | `LegUpper_9` | Default |
| `body/penis` | `penis` | `Penis_7` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper_2` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper_2` | Default |
| `body/neck` | `neck` | `Neck_10` | Default |
| `body/leg-left-lower` | `leg-left-lower` | `LegLower_1` | Default |
| `body/leg-right-lower` | `leg-right-lower` | `LegLower_1` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_11` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLower_11` | Default |
| `body/head` | `head` | `Head_2` | Default |
| `body/foot-left` | `foot-left` | `Foot_12` | Default |
| `body/foot-right` | `foot-right` | `Foot_12` | Default |
| `body/hand-left` | `hand-left` | `Hand_12` | Default |
| `body/hand-right` | `hand-right` | `Hand_12` | Default |
| `body/antenna-l` | `antenna-l` | `AntennaL` | Default |
| `body/antenna-r` | `antenna-r` | `AntennaR` | Default |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/hand` replaces: `body/hand-left`, `body/hand-right`
- `body/leg-upper` replaces: `body/leg-left-upper`, `body/leg-right-upper`
- `body/leg-lower` replaces: `body/leg-left-lower`, `body/leg-right-lower`

## `core:enemy/jacky`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hips_7` | Default |
| `body/butt` | `butt` | `Butt_7` | Default |
| `body/leg-left-upper` | `leg-left-upper` | `LegUpper_5` | Default |
| `body/penis-rod` | `penis-rod` | `PenisRod` | Default |
| `body/leg-right-upper` | `leg-right-upper` | `LegUpper_5` | Default |
| `body/torso-lower` | `spine` | `Spine_0` | Default |
| `body/leg-left-lower` | `leg-left-lower` | `LegLower_10` | Default |
| `body/penis-base` | `penis-base` | `PenisBase_3` | Default |
| `body/leg-right-lower` | `leg-right-lower` | `LegLower_10` | Default |
| `body/chest` | `chest` | `Chest_16` | Default |
| `body/l-foot-base` | `l-foot-base` | `FootBase_2` | Default |
| `body/penis-end` | `penis-end` | `PenisEnd_1` | Default |
| `body/r-foot-base` | `r-foot-base` | `FootBase_2` | Default |
| `body/breast` | `breast` | `Breast_1` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper_10` | Default |
| `body/neck` | `neck` | `Neck_11` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper_10` | Default |
| `body/l-foot-end` | `l-foot-end` | `FootEnd_2` | Default |
| `body/r-foot-end` | `r-foot-end` | `FootEnd_2` | Default |
| `body/breast-bg` | `breast-bg` | `BreastBg` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_10` | Default |
| `body/head` | `head` | `Head_19` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLower_10` | Default |
| `body/hand-left` | `hand-left` | `Hand_19` | Default |
| `body/hand-right` | `hand-right` | `Hand_19` | Default |
| `body/knife` | `knife` | `knife` | Default |
| `body/head/variant/head-fear` | `head` | `HeadFear` | Animation variant |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/hand` replaces: `body/hand-left`, `body/hand-right`
- `body/leg-upper` replaces: `body/leg-left-upper`, `body/leg-right-upper`
- `body/leg-lower` replaces: `body/leg-left-lower`, `body/leg-right-lower`

## `core:enemy/jenny`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hips_1` | Default |
| `body/butt` | `butt` | `Butt_6` | Default |
| `body/leg-left-upper` | `leg-left-upper` | `LegUpper_4` | Default |
| `body/penis-rod` | `penis-rod` | `PenisRod_0` | Default |
| `body/leg-right-upper` | `leg-right-upper` | `LegUpper_4` | Default |
| `body/torso-lower` | `spine` | `Spine_9` | Default |
| `body/leg-left-lower` | `leg-left-lower` | `LegLower_3` | Default |
| `body/penis-base` | `penis-base` | `PenisBase_2` | Default |
| `body/leg-right-lower` | `leg-right-lower` | `LegLower_3` | Default |
| `body/chest` | `chest` | `Chest_11` | Default |
| `body/l-foot-base` | `l-foot-base` | `FootBase_0` | Default |
| `body/penis-end` | `penis-end` | `PenisEnd` | Default |
| `body/r-foot-base` | `r-foot-base` | `FootBase_0` | Default |
| `body/breast` | `breast` | `Breast_2` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper_17` | Default |
| `body/neck` | `neck` | `Neck_4` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper_17` | Default |
| `body/l-foot-end` | `l-foot-end` | `FootEnd` | Default |
| `body/r-foot-end` | `r-foot-end` | `FootEnd` | Default |
| `body/breast-bg` | `breast-bg` | `BreastBg_0` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_1` | Default |
| `body/head` | `head` | `Head_18` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLower_1` | Default |
| `body/hand-left` | `hand-left` | `Hand_4` | Default |
| `body/hand-right` | `hand-right` | `Hand_4` | Default |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/hand` replaces: `body/hand-left`, `body/hand-right`
- `body/leg-upper` replaces: `body/leg-left-upper`, `body/leg-right-upper`
- `body/leg-lower` replaces: `body/leg-left-lower`, `body/leg-right-lower`

## `core:enemy/litigant`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hips_6` | Default |
| `body/leg-left-upper` | `leg-left-upper` | `LegUpper_16` | Default |
| `body/penis-base` | `penis-base` | `PenisMiddle` | Default |
| `body/leg-right-upper` | `leg-right-upper` | `LegUpper_16` | Default |
| `body/torso-lower` | `spine` | `Spine_8` | Default |
| `body/leg-left-lower` | `leg-left-lower` | `LegLower_16` | Default |
| `body/penis1` | `penis1` | `PenisMiddle` | Default |
| `body/leg-right-lower` | `leg-right-lower` | `LegLower_16` | Default |
| `body/chest` | `chest` | `Chest_19` | Default |
| `body/foot-left` | `foot-left` | `Foot_9` | Default |
| `body/penis2` | `penis2` | `PenisMiddle` | Default |
| `body/foot-right` | `foot-right` | `Foot_9` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper_6` | Default |
| `body/neck` | `neck` | `Neck_12` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper_6` | Default |
| `body/l-toe` | `l-toe` | `Toe` | Default |
| `body/penis3` | `penis3` | `PenisMiddle` | Default |
| `body/r-toe` | `r-toe` | `Toe` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_17` | Default |
| `body/head` | `head` | `Head_6` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLower_17` | Default |
| `body/penis4` | `penis4` | `PenisMiddle` | Default |
| `body/l-hand-base` | `l-hand-base` | `HandBase` | Default |
| `body/r-hand-base` | `r-hand-base` | `HandBase` | Default |
| `body/penis5` | `penis5` | `PenisMiddle` | Default |
| `body/l-hand-end` | `l-hand-end` | `HandEnd` | Default |
| `body/r-hand-end` | `r-hand-end` | `HandEnd` | Default |
| `body/penis6` | `penis6` | `PenisMiddle` | Default |
| `body/penis-tip` | `penis-tip` | `PenisTip_0` | Default |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/leg-upper` replaces: `body/leg-left-upper`, `body/leg-right-upper`
- `body/leg-lower` replaces: `body/leg-left-lower`, `body/leg-right-lower`

## `core:enemy/maggot`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Mid` | Default |
| `body/back` | `back` | `Back` | Default |
| `body/front` | `front` | `Front` | Default |

## `core:enemy/musca`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hips_10` | Default |
| `body/arm-bottom-upper` | `arm-bottom-upper` | `ArmUpper_8` | Default |
| `body/balls` | `balls` | `Balls_0` | Default |
| `body/penis-base` | `penis-base` | `PenisBase_1` | Default |
| `body/torso-lower` | `spine` | `Spine_1` | Default |
| `body/arm-bottom-lower` | `arm-bottom-lower` | `ArmLower_14` | Default |
| `body/penis-mid` | `penis-mid` | `PenisMid_1` | Default |
| `body/arm-mid-upper` | `arm-mid-upper` | `ArmUpper_8` | Default |
| `body/chest` | `chest` | `Chest_3` | Default |
| `body/hand-bottom` | `hand-bottom` | `Hand_3` | Default |
| `body/penis-tip` | `penis-tip` | `PenisTip` | Default |
| `body/arm-mid-lower` | `arm-mid-lower` | `ArmLower_14` | Default |
| `body/arm-top-upper` | `arm-top-upper` | `ArmUpper_8` | Default |
| `body/neck` | `neck` | `Neck_16` | Default |
| `body/wings` | `wings` | `Wing_0` | Default |
| `body/hand-mid` | `hand-mid` | `Hand_3` | Default |
| `body/arm-top-lower` | `arm-top-lower` | `ArmLower_14` | Default |
| `body/head` | `head` | `Head_15` | Default |
| `body/hand-top` | `hand-top` | `Hand_3` | Default |

## `core:enemy/orc`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hips_3` | Default |
| `body/butt` | `butt` | `Butt_10` | Default |
| `body/leg-left-upper` | `leg-left-upper` | `LegUpper_18` | Default |
| `body/penis-base` | `penis-base` | `PenisBase` | Default |
| `body/leg-right-upper` | `leg-right-upper` | `LegUpper_18` | Default |
| `body/torso-lower` | `spine` | `Spine_7` | Default |
| `body/leg-left-lower` | `leg-left-lower` | `LegLower_9` | Default |
| `body/penis-mid` | `penis-mid` | `PenisMid_0` | Default |
| `body/leg-right-lower` | `leg-right-lower` | `LegLower_9` | Default |
| `body/chest` | `chest` | `Chest_6` | Default |
| `body/foot-left` | `foot-left` | `Foot_0` | Default |
| `body/penis-tip` | `penis-tip` | `PenisTip_1` | Default |
| `body/foot-right` | `foot-right` | `Foot_0` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper_13` | Default |
| `body/neck` | `neck` | `Neck_9` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper_13` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_13` | Default |
| `body/head` | `head` | `Head_21` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLower_13` | Default |
| `body/hand-left` | `hand-left` | `Hand_15` | Default |
| `body/hand-right` | `hand-right` | `Hand_15` | Default |
| `body/shield` | `shield` | `Shield` | Default |
| `body/machete` | `machete` | `Machete` | Default |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/hand` replaces: `body/hand-left`, `body/hand-right`
- `body/leg-upper` replaces: `body/leg-left-upper`, `body/leg-right-upper`
- `body/leg-lower` replaces: `body/leg-left-lower`, `body/leg-right-lower`

## `core:enemy/spider-mother`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hips_9` | Default |
| `body/butt` | `butt` | `Butt_13` | Default |
| `body/chest` | `chest` | `Chest_10` | Default |
| `body/l-leg-front-base` | `l-leg-front-base` | `LegFrontBase_0` | Default |
| `body/r-leg-front-base` | `r-leg-front-base` | `LegFrontBase_0` | Default |
| `body/l-leg-back-base` | `l-leg-back-base` | `LegBackBase_0` | Default |
| `body/r-leg-back-base` | `r-leg-back-base` | `LegBackBase_0` | Default |
| `body/breast` | `breast` | `Breast_0` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper` | Default |
| `body/neck` | `neck` | `Neck_6` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper` | Default |
| `body/l-leg-front-upper` | `l-leg-front-upper` | `LegFrontUpper` | Default |
| `body/r-leg-front-upper` | `r-leg-front-upper` | `LegFrontUpper` | Default |
| `body/l-leg-back-upper` | `l-leg-back-upper` | `LegBackUpper_0` | Default |
| `body/r-leg-back-upper` | `r-leg-back-upper` | `LegBackUpper_0` | Default |
| `body/chest-inner` | `chest-inner` | `ChestInner` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_12` | Default |
| `body/head` | `head` | `Head_13` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLower_12` | Default |
| `body/hand-left` | `hand-left` | `Hand_14` | Default |
| `body/hand-right` | `hand-right` | `Hand_14` | Default |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/hand` replaces: `body/hand-left`, `body/hand-right`

## `core:enemy/sqoid`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `hips_0` | Default |
| `body/background` | `background` | `bg` | Default |
| `body/brain` | `brain` | `brain` | Default |

## `core:enemy/sunny`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hips_1` | Default |
| `body/butt` | `butt` | `Butt_8` | Default |
| `body/leg-left-upper` | `leg-left-upper` | `LegUpper_12` | Default |
| `body/penis-rod` | `penis-rod` | `PenisRod_0` | Default |
| `body/leg-right-upper` | `leg-right-upper` | `LegUpperBloody` | Default |
| `body/torso-lower` | `spine` | `Spine_2` | Default |
| `body/leg-left-lower` | `leg-left-lower` | `LegLower_13` | Default |
| `body/penis-base` | `penis-base` | `PenisBase_5` | Default |
| `body/leg-right-lower` | `leg-right-lower` | `LegLowerBloody_0` | Default |
| `body/chest` | `chest` | `Chest_4` | Default |
| `body/l-foot-base` | `l-foot-base` | `ShoeBase` | Default |
| `body/penis-end` | `penis-end` | `PenisEnd_3` | Default |
| `body/r-foot-base` | `r-foot-base` | `FootBase` | Default |
| `body/skirt` | `skirt` | `Skirt` | Default |
| `body/breast` | `breast` | `Breast_3` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper_5` | Default |
| `body/neck` | `neck` | `Neck_4` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper_5` | Default |
| `body/l-foot-end` | `l-foot-end` | `ShoeEnd` | Default |
| `body/r-foot-end` | `r-foot-end` | `FootEnd_0` | Default |
| `body/breast-bg` | `breast-bg` | `BreastBg_0` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_19` | Default |
| `body/head` | `head` | `Head_11` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLower_19` | Default |
| `body/hand-left` | `hand-left` | `Hand_8` | Default |
| `body/hand-right` | `hand-right` | `Hand_8` | Default |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/hand` replaces: `body/hand-left`, `body/hand-right`
- `body/leg-upper` replaces: `body/leg-left-upper`, `body/leg-right-upper`
- `body/leg-lower` replaces: `body/leg-left-lower`, `body/leg-right-lower`

## `core:enemy/zombie-1`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hip` | Default |
| `body/butt` | `butt` | `Butt` | Default |
| `body/leg-left-upper` | `leg-left-upper` | `LegUpper_19` | Default |
| `body/penis` | `penis` | `penis_0` | Default |
| `body/leg-right-upper` | `leg-right-upper` | `LegUpper_19` | Default |
| `body/torso-lower` | `spine` | `TorsoLower_5` | Default |
| `body/leg-left-lower` | `leg-left-lower` | `LegLower_2` | Default |
| `body/leg-right-lower` | `leg-right-lower` | `LegLower_2` | Default |
| `body/chest` | `chest` | `Chest` | Default |
| `body/foot-left` | `foot-left` | `Foot_11` | Default |
| `body/foot-right` | `foot-right` | `Foot2` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper_1` | Default |
| `body/neck` | `neck` | `Neck_18` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper_1` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_21` | Default |
| `body/head` | `head` | `Head` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLower_21` | Default |
| `body/hand-left` | `hand-left` | `Hand_6` | Default |
| `body/hand-right` | `hand-right` | `Hand_6` | Default |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/hand` replaces: `body/hand-left`, `body/hand-right`
- `body/leg-upper` replaces: `body/leg-left-upper`, `body/leg-right-upper`
- `body/leg-lower` replaces: `body/leg-left-lower`, `body/leg-right-lower`

## `core:enemy/zombie-2`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hip_6` | Default |
| `body/butt` | `butt` | `Butt_2` | Default |
| `body/leg-left-upper` | `leg-left-upper` | `LegUpper_8` | Default |
| `body/penis` | `penis` | `penis_1` | Default |
| `body/leg-right-upper` | `leg-right-upper` | `LegUpper_8` | Default |
| `body/torso-lower` | `spine` | `TorsoLower_3` | Default |
| `body/leg-left-lower` | `leg-left-lower` | `LegLower_19` | Default |
| `body/leg-right-lower` | `leg-right-lower` | `LegLower_19` | Default |
| `body/chest` | `chest` | `Chest_14` | Default |
| `body/foot-left` | `foot-left` | `Foot_4` | Default |
| `body/foot-right` | `foot-right` | `Foot_4` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper_19` | Default |
| `body/neck` | `neck` | `Neck_14` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper_19` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_3` | Default |
| `body/head` | `head` | `Head_3` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLower_3` | Default |
| `body/hand-left` | `hand-left` | `Hand_5` | Default |
| `body/hand-right` | `hand-right` | `Hand_5` | Default |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/hand` replaces: `body/hand-left`, `body/hand-right`
- `body/leg-upper` replaces: `body/leg-left-upper`, `body/leg-right-upper`
- `body/leg-lower` replaces: `body/leg-left-lower`, `body/leg-right-lower`

## `core:enemy/zombie-3`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hip 1` | Default |
| `body/butt` | `butt` | `Butt` | Default |
| `body/leg-left-upper` | `leg-left-upper` | `LegUpper_19 1` | Default |
| `body/penis` | `penis` | `penis_0 1` | Default |
| `body/leg-right-upper` | `leg-right-upper` | `LegUpper_19 1` | Default |
| `body/torso-lower` | `spine` | `TorsoLower_5 1` | Default |
| `body/leg-left-lower` | `leg-left-lower` | `LegLower_2 1` | Default |
| `body/leg-right-lower` | `leg-right-lower` | `LegLower_2 1` | Default |
| `body/chest` | `chest` | `Chest 1` | Default |
| `body/foot-left` | `foot-left` | `Foot_11 1` | Default |
| `body/foot-right` | `foot-right` | `Foot_11 1` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper_1 1` | Default |
| `body/neck` | `neck` | `Neck_18 1` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper_1 1` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_21 1` | Default |
| `body/head` | `head` | `Head` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLower_21 1` | Default |
| `body/hand-left` | `hand-left` | `Hand_6 1` | Default |
| `body/hand-right` | `hand-right` | `Hand_6 1` | Default |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/hand` replaces: `body/hand-left`, `body/hand-right`
- `body/leg-upper` replaces: `body/leg-left-upper`, `body/leg-right-upper`
- `body/leg-lower` replaces: `body/leg-left-lower`, `body/leg-right-lower`

## `core:enemy/zombie-grabber`

| Relative slot | Rig bone | Core sprite | Usage |
| --- | --- | --- | --- |
| `body/hips` | `hips` | `Hip_8` | Default |
| `body/butt` | `butt` | `Butt_9` | Default |
| `body/leg-left-upper` | `leg-left-upper` | `LegUpper_10` | Default |
| `body/penis` | `penis` | `penis_5` | Default |
| `body/leg-right-upper` | `leg-right-upper` | `LegUpper_10` | Default |
| `body/torso-lower` | `spine` | `TorsoLower_4` | Default |
| `body/leg-left-lower` | `leg-left-lower` | `LegLower_12` | Default |
| `body/leg-right-lower` | `leg-right-lower` | `LegLower_12` | Default |
| `body/chest` | `chest` | `Chest_1` | Default |
| `body/foot-left` | `foot-left` | `Foot_3` | Default |
| `body/foot-right` | `foot-right` | `Foot_3` | Default |
| `body/arm-left-upper` | `arm-left-upper` | `ArmUpper_4` | Default |
| `body/neck` | `neck` | `Neck_17` | Default |
| `body/arm-right-upper` | `arm-right-upper` | `ArmUpper_4` | Default |
| `body/arm-left-lower` | `arm-left-lower` | `ArmLower_18` | Default |
| `body/head` | `head` | `Head_4` | Default |
| `body/arm-right-lower` | `arm-right-lower` | `ArmLower_18` | Default |
| `body/hand-left` | `hand-left` | `Hand_17` | Default |
| `body/hand-right` | `hand-right` | `Hand_17` | Default |

Compatibility aliases:

- `body/arm-upper` replaces: `body/arm-left-upper`, `body/arm-right-upper`
- `body/arm-lower` replaces: `body/arm-left-lower`, `body/arm-right-lower`
- `body/hand` replaces: `body/hand-left`, `body/hand-right`
- `body/leg-upper` replaces: `body/leg-left-upper`, `body/leg-right-upper`
- `body/leg-lower` replaces: `body/leg-left-lower`, `body/leg-right-lower`
