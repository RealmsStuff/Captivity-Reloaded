# Captivity Reloaded Usable Item

> Generated from `usable.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | stable |  |
| `type` | const: `usable` | yes | stable |  |
| `id` | ref: `usableId` | yes | stable |  |
| `displayName` | string | yes | stable | min length: `1` |
| `extends` | ref: `coreUsableId` | no | stable |  |
| `description` | string | no | stable |  |
| `visual` | ref: `visual` | yes | experimental |  |
| `stats` | ref: `stats` | no | experimental |  |
| `effectMode` | enum: `inherit`, `add`, `replace` | no | experimental | default: `inherit` |
| `effects` | array of ref: `effect` | no | experimental | max items: `16` |

## Definition: `usableId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:item/usable/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `coreUsableId`

Schema form: string.

Constraints: pattern: `^core:item/usable/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `pngPath`

Schema form: string.

Constraints: pattern: `^(?!/)(?!.*(?:^\|/)\.\.(?:/\|$))(?!.*\\).+\.[pP][nN][gG]$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `visual`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | enum: `coreUsableSprites`, `originalUsableSprites` | yes |  |  |
| `icon` | ref: `pngPath` | yes |  |  |
| `world` | ref: `pngPath` | no |  |  |
| `pixelsPerUnit` | number | no |  | exclusive min: `0`; max: `1024`; default: `32` |
| `pivotX` | number | no |  | min: `0`; max: `1`; default: `0.5` |
| `pivotY` | number | no |  | min: `0`; max: `1`; default: `0.5` |
| `colliderWidth` | number | no |  | exclusive min: `0`; max: `16`; default: `0.75` |
| `colliderHeight` | number | no |  | exclusive min: `0`; max: `16`; default: `0.75` |
| `sortingOrder` | integer | no |  | min: `-1000`; max: `1000`; default: `0` |

## Definition: `descriptionList`

Schema form: array of string.

Constraints: max items: `32`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `stats`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `weight` | integer | no |  | min: `0`; max: `100000` |
| `value` | integer | no |  | min: `0`; max: `100000000` |
| `equipSeconds` | number | no |  | min: `0`; max: `60` |
| `marketable` | boolean | no |  |  |
| `goodEffectDescriptions` | ref: `descriptionList` | no |  |  |
| `badEffectDescriptions` | ref: `descriptionList` | no |  |  |

## Definition: `effect`

Schema form: oneOf.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `amountEffect`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | enum: `restoreHealth`, `restoreStrength`, `restoreStamina`, `reducePleasure`, `refillAmmo`, `gainMoney` | yes |  |  |
| `amount` | number | yes |  | min: `0.01`; max: `100000` |

## Definition: `noArgumentEffect`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | enum: `fillAllAmmo`, `repairClothing` | yes |  |  |

## Definition: `durationEffect`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | enum: `ragdoll`, `invulnerability` | yes |  |  |
| `durationSeconds` | number | yes |  | min: `0.01`; max: `600` |

## Definition: `statModifier`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `statModifier` | yes |  |  |
| `stat` | enum: `HealthMax`, `SpeedAccel`, `SpeedMax`, `Traction`, `DamageMultiplierGun`, `SpeedSprint`, `PowerJump`, `PowerDash` | yes |  |  |
| `value` | number | yes |  | min: `-100000`; max: `100000` |
| `durationSeconds` | number | yes |  | min: `0.01`; max: `3600` |
| `stackable` | boolean | no |  |  |
