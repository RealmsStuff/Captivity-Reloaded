# Captivity Reloaded Challenge

> Generated from `challenge.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | stable |  |
| `type` | const: `challenge` | yes | stable |  |
| `id` | ref: `challengeId` | yes | stable |  |
| `displayName` | string | yes | stable | min length: `1` |
| `description` | string | yes | stable | min length: `1` |
| `extends` | ref: `challengeId` | no | stable |  |
| `objective` | ref: `objectiveName` | no | stable |  |
| `count` | ref: `count` | no | stable |  |
| `stage` | ref: `stageId` | no | stable |  |
| `enemies` | array of ref: `enemyId` | no | stable | unique items; Allowlist for killCount, weaponKillCount, rapeCount, orgasmCount, and impregnationCount. Core and external enemy IDs may be mixed. |
| `items` | array of ref: `itemId` | no | stable | unique items |
| `steps` | array of ref: `objective` | no | experimental | min items: `1`; max items: `32` |
| `rewards` | array of ref: `clothingId` | no | stable | min items: `1`; unique items |
| `rewardBundle` | object | no | experimental | min properties: `1` |

### Fields under `root.rewardBundle`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `currency` | integer | no |  | min: `0`; max: `1000000` |
| `items` | array of ref: `itemGrant` | no |  | max items: `32` |
| `weapons` | array of ref: `weaponGrant` | no |  | max items: `32` |
| `content` | array of ref: `contentId` | no |  | unique items |

## Definition: `contentId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `challengeId`

Schema form: allOf.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `stageId`

Schema form: allOf.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `enemyId`

Schema form: allOf.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `itemId`

Schema form: allOf.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `clothingId`

Schema form: allOf.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `count`

Schema form: integer.

Constraints: min: `1`; max: `1000000`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `objectiveName`

Schema form: enum: `killCount`, `reachWave`, `surviveWaves`, `pickupCount`, `interactionCount`, `shotsFired`, `damageTaken`, `birthCount`, `impregnationCount`, `rapeCount`, `orgasmCount`, `mindBreakCount`, `useItemCount`, `weaponKillCount`, `flawlessWaves`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `objective`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `objective` | ref: `objectiveName` | yes |  |  |
| `count` | ref: `count` | yes |  |  |
| `enemies` | array of ref: `enemyId` | no |  | unique items |
| `items` | array of ref: `itemId` | no |  | unique items |

## Definition: `itemGrant`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `id` | ref: `itemId` | yes |  |  |
| `amount` | integer | no |  | min: `1`; max: `99` |

## Definition: `weaponGrant`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `id` | ref: `itemId` | yes |  |  |
| `amount` | const: `1` | no |  |  |
