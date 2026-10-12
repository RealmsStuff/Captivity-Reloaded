# Captivity Reloaded Clothing

> Generated from `clothing.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | stable |  |
| `type` | const: `clothing` | yes | stable |  |
| `id` | ref: `clothingId` | yes | stable |  |
| `displayName` | string | yes | stable | min length: `1` |
| `extends` | ref: `coreClothingId` | no | stable |  |
| `description` | string | no | stable |  |
| `unlockedByDefault` | boolean | no | stable | default: `False` |
| `category` | ref: `category` | no | experimental |  |
| `incompatibleCategories` | array of ref: `category` | no | experimental | unique items |
| `visual` | ref: `visual` | yes | stable |  |
| `effects` | ref: `effects` | no | experimental |  |

## Definition: `clothingId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:clothing/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `coreClothingId`

Schema form: string.

Constraints: pattern: `^core:clothing/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `category`

Schema form: enum: `Hair`, `Upper`, `Lower`, `Shoes`, `Hat`, `Sleeves`, `Stockings`, `Other`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `pngPath`

Schema form: string.

Constraints: pattern: `^(?!/)(?!.*(?:^\|/)\.\.(?:/\|$))(?!.*\\).+\.[pP][nN][gG]$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `slot`

Schema form: string.

Constraints: pattern: `^(?:icon\|piece/[a-z0-9][a-z0-9/-]*)$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `region`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `x` | integer | yes |  | min: `0` |
| `y` | integer | yes |  | min: `0` |
| `width` | integer | yes |  | min: `1` |
| `height` | integer | yes |  | min: `1` |

## Definition: `attachment`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `bone` | enum: `Hips`, `Butt`, `Spine`, `Chest`, `Neck`, `Head`, `rArmUpper`, `rArmLower`, `rHand`, `lArmUpper`, `lArmLower`, `lHand`, `rLegUpper`, `rLegLower`, `rFoot`, `lLegUpper`, `lLegLower`, `lFoot`, `Ear`, `Face` | no |  |  |
| `offsetX` | number | no |  | min: `-10`; max: `10` |
| `offsetY` | number | no |  | min: `-10`; max: `10` |
| `rotation` | number | no |  | min: `-360`; max: `360` |
| `pivotX` | number | no |  | min: `0`; max: `1` |
| `pivotY` | number | no |  | min: `0`; max: `1` |
| `sortingOffset` | integer | no |  | min: `-100`; max: `100` |
| `attachToBone` | boolean | no |  |  |
| `hideBodyPart` | boolean | no |  |  |
| `droppable` | boolean | no |  |  |
| `destroyable` | boolean | no |  |  |
| `dropOnOralThrust` | boolean | no |  |  |
| `destroyOnOralThrust` | boolean | no |  |  |
| `physics` | ref: `physics` | no |  |  |

## Definition: `physics`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `mode` | const: `sway` | yes |  |  |
| `spring` | number | no |  | min: `0.1`; max: `200` |
| `damping` | number | no |  | min: `0`; max: `50` |
| `gravity` | number | no |  | min: `0`; max: `1` |
| `motionInfluence` | number | no |  | min: `0`; max: `10` |
| `maxAngle` | number | no |  | min: `0`; max: `90` |
| `idleAmplitude` | number | no |  | min: `0`; max: `20` |
| `idleFrequency` | number | no |  | min: `0`; max: `5` |

## Definition: `bodyVariant`

Schema form: object map of ref: `pngPath`.

Constraints: min properties: `1`; max properties: `32`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `visual`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | enum: `coreClothingAtlas`, `coreClothingSprites`, `originalClothingSprites`, `originalClothingAtlas` | yes |  |  |
| `atlas` | ref: `pngPath` | no |  |  |
| `sprites` | object map of ref: `pngPath` | no |  |  |
| `pixelsPerUnit` | number | no |  | exclusive min: `0`; max: `1024`; default: `32` |
| `regions` | object map of ref: `region` | no |  |  |
| `attachments` | object map of ref: `attachment` | no | experimental |  |
| `bodyVariants` | object map of ref: `bodyVariant` | no | experimental | max properties: `32` |

## Definition: `effects`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `damageTakenMultiplier` | number | no |  | min: `0.05`; max: `2` |
| `escapePowerMultiplier` | number | no |  | min: `0.1`; max: `3` |
| `bountyMultiplier` | number | no |  | min: `0`; max: `3` |
| `statModifiers` | object map of number | no |  | max properties: `8` |
