# Captivity Reloaded Persistent Player Attachment

> Generated from `player-attachment.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | stable |  |
| `type` | const: `playerAttachment` | yes | experimental |  |
| `id` | string | yes | experimental | pattern: `^[a-z0-9][a-z0-9._-]*:player-attachment/[a-z0-9][a-z0-9._/-]*$` |
| `sprite` | ref: `pngPath` | no | experimental |  |
| `skinSprites` | object map of ref: `pngPath` | no | experimental | min properties: `1`; max properties: `7` |
| `pixelsPerUnit` | number | no | experimental | min: `1`; max: `1024`; default: `32` |
| `bone` | enum: `Hips`, `Butt`, `Spine`, `Chest`, `Neck`, `Head`, `rArmUpper`, `rArmLower`, `rHand`, `lArmUpper`, `lArmLower`, `lHand`, `rLegUpper`, `rLegLower`, `rFoot`, `lLegUpper`, `lLegLower`, `lFoot`, `Ear`, `Face` | yes | experimental |  |
| `attachToBone` | boolean | no | experimental | default: `True` |
| `offsetX` | number | no | experimental | min: `-10`; max: `10`; default: `0` |
| `offsetY` | number | no | experimental | min: `-10`; max: `10`; default: `0` |
| `rotation` | number | no | experimental | min: `-360`; max: `360`; default: `0` |
| `pivotX` | number | no | experimental | min: `0`; max: `1`; default: `0.5` |
| `pivotY` | number | no | experimental | min: `0`; max: `1`; default: `0.5` |
| `sortingOffset` | integer | no | experimental | min: `-100`; max: `100`; default: `1` |
| `tintWithSkin` | boolean | no | experimental | default: `False` |
| `pregnancyGrowth` | object | no | experimental |  |
| `physics` | object | no | experimental |  |

### Fields under `root.pregnancyGrowth`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `transitionSeconds` | number | no |  | min: `0.05`; max: `10`; default: `1.5` |
| `breakSpineClothing` | boolean | no |  | default: `False` |
| `clothingBreakDelaySeconds` | number | no |  | min: `0`; max: `10`; default: `0.5` |
| `stages` | array of object | yes |  | min items: `2`; max items: `16` |

### Fields under `root.pregnancyGrowth.stages[]`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `minimumFetuses` | integer | yes |  | min: `0`; max: `64` |
| `offsetX` | number | yes |  | min: `-10`; max: `10` |
| `offsetY` | number | yes |  | min: `-10`; max: `10` |
| `scaleX` | number | yes |  | min: `0`; max: `10` |
| `scaleY` | number | yes |  | min: `0`; max: `10` |

### Fields under `root.physics`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `mode` | const: `sway` | yes |  |  |
| `spring` | number | no |  | min: `0.1`; max: `200` |
| `damping` | number | no |  | min: `0`; max: `50` |
| `gravity` | number | no |  | min: `0`; max: `1` |
| `motionInfluence` | number | no |  | min: `0`; max: `10` |
| `maxAngle` | number | no |  | min: `0`; max: `90` |

## Definition: `pngPath`

Schema form: string.

Constraints: pattern: `^(?!/)(?!.*(?:^\|/)\.\.(?:/\|$))(?!.*\\).+\.[pP][nN][gG]$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |
