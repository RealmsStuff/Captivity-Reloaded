# Captivity Reloaded Stage

> Generated from `stage.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | stable |  |
| `type` | const: `stage` | yes | stable |  |
| `id` | ref: `stageId` | yes | stable |  |
| `displayName` | string | yes | stable | min length: `1` |
| `extends` | ref: `coreStageId` | yes | stable |  |
| `description` | string | no | stable |  |
| `layout` | ref: `layout` | yes | experimental |  |
| `waves` | ref: `waves` | yes | stable |  |
| `audio` | ref: `audio` | no | experimental |  |
| `camera` | ref: `camera` | no | experimental |  |
| `testTools` | ref: `testTools` | no | experimental |  |
| `spawners` | array of ref: `spawner` | yes | stable | min items: `1`; max items: `256` |

## Definition: `stageId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:stage/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `coreStageId`

Schema form: string.

Constraints: pattern: `^core:stage/(?!hub$)[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `enemyId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:enemy/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `path`

Schema form: string.

Constraints: pattern: `^(?!/)(?!.*(?:^\|/)\.\.(?:/\|$))(?!.*\\).+$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `point`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `x` | number | yes |  | min: `-100000`; max: `100000` |
| `y` | number | yes |  | min: `-100000`; max: `100000` |

## Definition: `layout`

Schema form: oneOf.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

### Fields for `layout (oneOf 1)`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `coreStageLayout` | yes |  |  |
| `playerSpawn` | ref: `point` | yes |  |  |

### Fields for `layout (oneOf 2)`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `tiledJson` | yes |  |  |
| `path` | allOf | yes |  |  |
| `pixelsPerUnit` | integer | yes |  | min: `1`; max: `512` |
| `hideInheritedVisuals` | boolean | no |  | default: `False` |
| `preserveInheritedStageObjects` | boolean | no |  | default: `False` |
| `reuseInheritedSpawners` | boolean | no | experimental | default: `False` |

## Definition: `waves`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `firstWaveEnemyCount` | integer | yes |  | min: `1`; max: `1000` |

## Definition: `audio`

Schema form: object.

Constraints: min properties: `1`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `ambience` | ref: `audioPath` | no |  |  |
| `entryMusic` | ref: `audioPath` | no |  |  |
| `waveMusic` | ref: `audioPath` | no |  |  |
| `waveComplete` | ref: `audioPath` | no |  |  |

## Definition: `camera`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `bounds` | ref: `cameraBounds` | no |  |  |
| `unbounded` | const: `True` | no |  |  |

## Definition: `cameraBounds`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `minX` | number | yes |  | min: `-100000`; max: `100000` |
| `minY` | number | yes |  | min: `-100000`; max: `100000` |
| `maxX` | number | yes |  | min: `-100000`; max: `100000` |
| `maxY` | number | yes |  | min: `-100000`; max: `100000` |

## Definition: `testTools`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `enabled` | const: `True` | yes |  |  |
| `enemy` | ref: `enemyId` | yes |  |  |
| `spawnPosition` | ref: `point` | no |  |  |

## Definition: `audioPath`

Schema form: allOf.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `spawner`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `id` | string | yes |  | pattern: `^[a-z0-9][a-z0-9-]*$` |
| `position` | ref: `point` | yes |  |  |
| `enemies` | array of ref: `enemyId` | yes |  | min items: `1`; max items: `64`; unique items |
| `selectionWeight` | number | yes |  | min: `0.001`; max: `1000` |
| `minimumWave` | integer | no |  | min: `0`; max: `10000`; default: `0` |
| `delaySeconds` | number | yes |  | min: `0.02`; max: `300` |
| `delayJitterSeconds` | number | no |  | min: `0`; max: `300`; default: `0` |
| `initialDelaySeconds` | number | no |  | min: `0`; max: `300`; default: `0` |
| `initialDelayJitterSeconds` | number | no |  | min: `0`; max: `300`; default: `0` |
| `spawnOutOfSight` | boolean | no |  | default: `False` |
| `enabled` | boolean | no | experimental | default: `True` |
| `requiredOpenDoors` | array of ref: `localId` | no | experimental | max items: `32`; unique items |
| `requiredClosedDoors` | array of ref: `localId` | no | experimental | max items: `32`; unique items |

## Definition: `localId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |
