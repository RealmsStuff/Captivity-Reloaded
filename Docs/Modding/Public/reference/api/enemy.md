# Captivity Reloaded Enemy

> Generated from `enemy.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | stable |  |
| `type` | const: `enemy` | yes | stable |  |
| `id` | ref: `enemyId` | yes | stable |  |
| `displayName` | string | yes | stable | min length: `1` |
| `extends` | ref: `coreEnemyId` | no | stable |  |
| `description` | string | no | stable |  |
| `stats` | ref: `stats` | no | stable |  |
| `visual` | ref: `visual` | no | stable |  |
| `spawn` | ref: `spawn` | no | experimental |  |
| `attacks` | array of ref: `attack` | no | experimental | max items: `32` |
| `behavior` | ref: `behavior` | no | experimental |  |
| `animation` | ref: `animation` | no | experimental |  |
| `animationRefs` | object map of ref: `enemyAnimationId` | no | experimental | max properties: `64` |
| `ai` | ref: `ai` | no | experimental |  |
| `drops` | ref: `drops` | no | experimental |  |

### Fields for `root (oneOf 1)`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `visual` | schema | no |  |  |
| `attacks` | schema | no |  |  |

### Fields under `root (oneOf 1).visual`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `coreRigAtlas` | no |  |  |

### Fields for `root (oneOf 2)`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `visual` | schema | yes |  |  |
| `attacks` | schema | yes |  | min items: `1` |
| `animation` | schema | no |  |  |

### Fields under `root (oneOf 2).visual`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `originalSkeletonAtlas` | no |  |  |

## Definition: `enemyId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:enemy/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `enemyAnimationId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:enemy-animation/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `coreEnemyId`

Schema form: string.

Constraints: pattern: `^core:enemy/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `itemId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:item/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `pngPath`

Schema form: string.

Constraints: pattern: `^(?!/)(?!.*(?:^\|/)\.\.(?:/\|$))(?!.*\\).+\.[pP][nN][gG]$`.

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

## Definition: `stats`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `healthMax` | number | no |  | exclusive min: `0`; max: `1000000` |
| `speedAcceleration` | number | no |  | min: `0`; max: `1000` |
| `speedMax` | number | no |  | min: `0`; max: `1000` |
| `traction` | number | no |  | min: `0`; max: `1` |
| `bounty` | integer | no |  | min: `0` |
| `healthIncreasePerWave` | number | no |  | min: `0`; max: `1000000` |

## Definition: `visual`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | enum: `coreRigAtlas`, `originalSkeletonAtlas` | yes |  |  |
| `atlas` | ref: `pngPath` | yes |  |  |
| `pixelsPerUnit` | number | no |  | exclusive min: `0`; max: `1024`; default: `32` |
| `regions` | object map of ref: `region` | yes |  | min properties: `1` |
| `bones` | array of ref: `bone` | no |  | min items: `1`; max items: `64` |
| `hitZones` | array of ref: `hitZone` | no |  | min items: `1`; max items: `64` |
| `bodyWidth` | number | no |  | min: `0.05`; max: `100` |
| `bodyHeight` | number | no |  | min: `0.05`; max: `100` |
| `bodyOffsetX` | number | no |  |  |
| `bodyOffsetY` | number | no |  |  |

## Definition: `bone`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `id` | ref: `name` | yes |  |  |
| `parent` | ref: `name` | no |  |  |
| `region` | ref: `name` | yes |  |  |
| `x` | number | no |  |  |
| `y` | number | no |  |  |
| `rotation` | number | no |  |  |
| `pivotX` | number | no |  | min: `0`; max: `1` |
| `pivotY` | number | no |  | min: `0`; max: `1` |
| `sortingOrder` | integer | no |  | min: `-10000`; max: `10000` |

## Definition: `hitZone`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `bone` | ref: `name` | yes |  |  |
| `shape` | enum: `box`, `circle` | yes |  |  |
| `offsetX` | number | no |  |  |
| `offsetY` | number | no |  |  |
| `width` | number | no |  | exclusive min: `0`; max: `100` |
| `height` | number | no |  | exclusive min: `0`; max: `100` |
| `radius` | number | no |  | exclusive min: `0`; max: `100` |
| `damageMultiplier` | enum: `low`, `normal`, `critical`, `block` | no |  |  |

## Definition: `spawn`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `inheritTemplateSpawners` | boolean | yes | stable |  |
| `selectionWeight` | number | no | experimental | min: `0.001`; max: `1000`; default: `1` |

## Definition: `attack`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `index` | integer | no |  | min: `0`; max: `31` |
| `id` | ref: `name` | no |  |  |
| `type` | enum: `melee`, `hitscan`, `projectile`, `area`, `grab`, `multiStage` | no |  |  |
| `animation` | ref: `name` | no |  |  |
| `hitTimeSeconds` | number | no |  | min: `0`; max: `600` |
| `chance` | number | no |  | min: `0`; max: `1` |
| `damage` | number | no |  | min: `0`; max: `100000` |
| `knockbackX` | number | no |  | min: `0`; max: `1000` |
| `knockbackY` | number | no |  | min: `0`; max: `1000` |
| `cooldownSeconds` | number | no |  | min: `0`; max: `600` |
| `initiateRange` | number | no |  | min: `0`; max: `1000` |
| `hitRange` | number | no |  | min: `0`; max: `1000` |
| `movesDuringAttack` | boolean | no |  |  |
| `durationSeconds` | number | no |  | min: `0.01`; max: `600` |
| `areaRadius` | number | no |  | min: `0.05`; max: `100` |
| `offsetX` | number | no |  | min: `-1000`; max: `1000` |
| `offsetY` | number | no |  | min: `-1000`; max: `1000` |
| `projectileRegion` | ref: `name` | no |  |  |
| `projectileSpeed` | number | no |  | min: `0.05`; max: `1000` |
| `projectileLifetimeSeconds` | number | no |  | min: `0.05`; max: `60` |
| `projectileRadius` | number | no |  | min: `0.01`; max: `10` |
| `projectileGravityScale` | number | no |  | min: `-10`; max: `10` |
| `stages` | array of ref: `attackStage` | no |  | min items: `2`; max items: `16` |

## Definition: `attackStage`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | enum: `melee`, `hitscan`, `projectile`, `area`, `grab` | yes |  |  |
| `damage` | number | no |  | min: `0`; max: `100000` |
| `knockbackX` | number | no |  | min: `0`; max: `1000` |
| `knockbackY` | number | no |  | min: `0`; max: `1000` |
| `hitRange` | number | no |  | min: `0`; max: `1000` |
| `areaRadius` | number | no |  | min: `0.05`; max: `100` |
| `offsetX` | number | no |  | min: `-1000`; max: `1000` |
| `offsetY` | number | no |  | min: `-1000`; max: `1000` |
| `projectileRegion` | ref: `name` | no |  |  |
| `projectileSpeed` | number | no |  | min: `0.05`; max: `1000` |
| `projectileLifetimeSeconds` | number | no |  | min: `0.05`; max: `60` |
| `projectileRadius` | number | no |  | min: `0.01`; max: `10` |
| `projectileGravityScale` | number | no |  | min: `-10`; max: `10` |

## Definition: `behavior`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `visionRange` | number | no |  | min: `0.1`; max: `10000` |
| `ignoreWave` | boolean | no |  |  |
| `modules` | array of ref: `module` | no |  | max items: `8` |

## Definition: `module`

Schema form: oneOf.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `regeneration`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `regeneration` | yes |  |  |
| `amount` | number | yes |  | min: `0.01`; max: `100000` |
| `intervalSeconds` | number | yes |  | min: `0.05`; max: `600` |

## Definition: `berserk`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `berserk` | yes |  |  |
| `healthThreshold` | number | yes |  | min: `0.01`; max: `1` |
| `speedBonus` | number | no |  | min: `-1000`; max: `1000` |
| `damageMultiplierBonus` | number | no |  | min: `-100`; max: `100` |

## Definition: `amountModule`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | enum: `lifesteal`, `thorns` | yes |  |  |
| `amount` | number | yes |  | min: `0.01`; max: `100000` |

## Definition: `onHitRagdoll`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `onHitRagdoll` | yes |  |  |
| `durationSeconds` | number | yes |  | min: `0.05`; max: `30` |

## Definition: `onHitEquipClothing`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `onHitEquipClothing` | yes |  |  |
| `clothing` | ref: `clothingId` | yes |  |  |
| `chance` | number | no |  | min: `0`; max: `1` |
| `cooldownSeconds` | number | no |  | min: `0`; max: `600` |

## Definition: `spawnOnDeath`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `spawnOnDeath` | yes |  |  |
| `enemy` | ref: `enemyId` | yes |  |  |
| `count` | integer | yes |  | min: `1`; max: `32` |
| `radius` | number | no |  | min: `0`; max: `20` |

## Definition: `speedPulse`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `speedPulse` | yes |  |  |
| `speedBonus` | number | yes |  | min: `-1000`; max: `1000` |
| `intervalSeconds` | number | yes |  | min: `0.1`; max: `600` |
| `durationSeconds` | number | yes |  | min: `0.05`; max: `600` |

## Definition: `downedFinisher`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `downedFinisher` | yes |  |  |
| `triggerRange` | number | yes |  | min: `0.1`; max: `20` |
| `startDelaySeconds` | number | no |  | min: `0`; max: `10` |
| `durationSeconds` | number | no |  | min: `1`; max: `60` |
| `meterMax` | number | yes |  | min: `1`; max: `10000` |
| `inputPower` | number | yes |  | exclusive min: `0`; max: `10000` |
| `inputPattern` | enum: `adaptive`, `alternate`, `rotate`, `tap` | no |  |  |
| `decayPerSecond` | number | no |  | min: `0`; max: `10000` |
| `failureDamage` | number | no |  | min: `0`; max: `100000` |
| `successRecoveryHealth` | number | no |  | min: `0`; max: `100000` |
| `successStunSeconds` | number | no |  | min: `0`; max: `30` |
| `cooldownSeconds` | number | no |  | min: `0`; max: `600` |
| `animation` | ref: `name` | no |  |  |
| `playerAnimation` | ref: `playerClip` | no |  |  |
| `playerAnimationRef` | ref: `playerAnimationId` | no |  |  |
| `phases` | array of ref: `finisherPhase` | no |  | min items: `2`; max items: `8` |
| `participants` | array of ref: `finisherParticipant` | no |  | max items: `3` |
| `successOutcome` | ref: `finisherOutcome` | no |  |  |
| `failureOutcome` | ref: `finisherOutcome` | no |  |  |
| `statuses` | ref: `finisherStatuses` | no |  |  |

## Definition: `finisherStatuses`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `active` | ref: `finisherStatusText` | no |  |  |
| `mating` | ref: `finisherStatusText` | no |  |  |
| `infusion` | ref: `finisherStatusText` | no |  |  |
| `succumbed` | ref: `finisherStatusText` | no |  |  |
| `orgasm` | ref: `finisherStatusText` | no |  |  |
| `fertilized` | ref: `finisherStatusText` | no |  |  |
| `pregnant` | ref: `finisherStatusText` | no |  |  |
| `implanting` | ref: `finisherStatusText` | no |  |  |
| `labor` | ref: `finisherStatusText` | no |  |  |
| `birth` | ref: `finisherStatusText` | no |  |  |
| `mindBroken` | ref: `finisherStatusText` | no |  |  |

## Definition: `finisherStatusText`

Schema form: object.

Constraints: min properties: `1`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `title` | string | no |  | min length: `1`; max length: `64` |
| `description` | string | no |  | max length: `256` |

## Definition: `finisherPhase`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `id` | ref: `name` | yes |  |  |
| `durationSeconds` | number | yes |  | min: `0.1`; max: `60` |
| `animation` | ref: `name` | yes |  |  |
| `playerAnimation` | ref: `playerClip` | no |  |  |
| `playerAnimationRef` | ref: `playerAnimationId` | no |  |  |
| `inputPower` | number | no |  | exclusive min: `0`; max: `10000` |
| `inputPattern` | enum: `adaptive`, `alternate`, `rotate`, `tap` | no |  |  |
| `decayPerSecond` | number | no |  | min: `0`; max: `10000` |
| `participantAnimations` | object map of ref: `name` | no |  | max properties: `3` |

## Definition: `finisherParticipant`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `id` | ref: `name` | yes |  |  |
| `enemy` | ref: `enemyId` | yes |  |  |
| `joinPolicy` | enum: `startOnly`, `phaseBoundary` | no |  |  |
| `required` | boolean | no |  |  |
| `joinRange` | number | no |  | min: `0.1`; max: `20` |
| `approachRange` | number | no |  | min: `0.1`; max: `50` |
| `offsetX` | number | no |  | min: `-20`; max: `20` |
| `offsetY` | number | no |  | min: `-20`; max: `20` |
| `facing` | enum: `preserve`, `left`, `right` | no |  |  |
| `animation` | ref: `name` | no |  |  |

## Definition: `finisherOutcome`

Schema form: object.

Constraints: min properties: `1`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `healthDamage` | number | no |  | min: `0`; max: `100000` |
| `strengthDamage` | number | no |  | min: `0`; max: `100000` |
| `pleasure` | number | no |  | min: `0`; max: `100` |
| `libido` | number | no |  | min: `0`; max: `10000` |
| `healthRecovery` | number | no |  | min: `0`; max: `100000` |
| `enemyStunSeconds` | number | no |  | min: `0`; max: `30` |
| `playerRagdollSeconds` | number | no |  | min: `0`; max: `30` |
| `equipClothing` | array of ref: `clothingId` | no |  | max items: `8`; unique items |

## Definition: `playerClip`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `durationSeconds` | number | yes |  | exclusive min: `0`; max: `60` |
| `loop` | boolean | no |  |  |
| `frames` | array of ref: `playerFrame` | yes |  | min items: `1`; max items: `256` |
| `events` | array of ref: `playerAnimationEvent` | no |  | max items: `64` |

## Definition: `playerAnimationId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:player-animation/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `clothingId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:clothing/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `playerAnimationEvent`

Schema form: oneOf.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

### Fields for `playerAnimationEvent (oneOf 1)`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0`; max: `60` |
| `type` | enum: `pleasure`, `scaledPleasure` | yes |  |  |
| `amount` | number | yes |  | exclusive min: `0`; max: `100` |

### Fields for `playerAnimationEvent (oneOf 2)`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0`; max: `60` |
| `type` | enum: `libido`, `struggleDamage` | yes |  |  |
| `amount` | number | yes |  | exclusive min: `0`; max: `10000` |

### Fields for `playerAnimationEvent (oneOf 3)`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0`; max: `60` |
| `type` | enum: `strengthDamage`, `healthDamage` | yes |  |  |
| `amount` | number | yes |  | exclusive min: `0`; max: `100000` |

## Definition: `playerFrame`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0`; max: `60` |
| `bones` | object map of ref: `playerPose` | yes |  |  |

## Definition: `playerPose`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `x` | number | no |  |  |
| `y` | number | no |  |  |
| `rotation` | number | no |  |  |
| `scaleX` | number | no |  |  |
| `scaleY` | number | no |  |  |

## Definition: `animation`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `speedMultiplier` | number | no |  | min: `0.01`; max: `10` |
| `clips` | object map of ref: `clip` | no |  | max properties: `32` |

## Definition: `clip`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `durationSeconds` | number | yes |  | exclusive min: `0`; max: `600` |
| `loop` | boolean | no |  |  |
| `frames` | array of ref: `frame` | yes |  | min items: `1`; max items: `256` |
| `events` | array of ref: `enemyAnimationEvent` | no | stable | max items: `64` |

## Definition: `enemyAnimationEvent`

Schema form: oneOf.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

### Fields for `enemyAnimationEvent (oneOf 1)`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0`; max: `600` |
| `type` | const: `attackHit` | yes |  |  |

### Fields for `enemyAnimationEvent (oneOf 2)`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0`; max: `600` |
| `type` | const: `impulse` | yes |  |  |
| `x` | number | no |  | min: `-1000`; max: `1000` |
| `y` | number | no |  | min: `-1000`; max: `1000` |
| `relativeToFacing` | boolean | no |  | default: `True` |

### Fields for `enemyAnimationEvent (oneOf 3)`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0`; max: `600` |
| `type` | const: `sound` | yes |  |  |
| `file` | string | yes |  | pattern: `^(?!/)(?!.*(?:^\|/)\.\.(?:/\|$))(?!.*\\).+\.(?:[wW][aA][vV]\|[oO][gG][gG])$` |
| `volume` | number | no |  | min: `0`; max: `1`; default: `1` |

### Fields for `enemyAnimationEvent (oneOf 4)`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0`; max: `600` |
| `type` | const: `cameraShake` | yes |  |  |
| `amount` | number | yes |  | min: `0`; max: `1` |

### Fields for `enemyAnimationEvent (oneOf 5)`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0`; max: `600` |
| `type` | const: `spriteEffect` | yes |  |  |
| `region` | ref: `name` | yes |  |  |
| `bone` | ref: `name` | no |  |  |
| `x` | number | no |  |  |
| `y` | number | no |  |  |
| `durationSeconds` | number | no |  | min: `0.02`; max: `30`; default: `0.15` |
| `scale` | number | no |  | min: `0.01`; max: `100`; default: `1` |
| `sortingOrder` | integer | no |  | min: `-10000`; max: `10000`; default: `100` |

### Fields for `enemyAnimationEvent (oneOf 6)`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0`; max: `600` |
| `type` | const: `cue` | yes |  |  |
| `cue` | string | yes |  | max length: `80`; pattern: `^[a-z0-9][a-z0-9._/-]*$` |
| `index` | integer | no |  | min: `0`; max: `255` |
| `amount` | number | no |  | min: `-100000`; max: `100000` |

## Definition: `frame`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0`; max: `600` |
| `bones` | object map of ref: `pose` | yes |  |  |

## Definition: `pose`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `region` | ref: `name` | no |  |  |
| `x` | number | no |  |  |
| `y` | number | no |  |  |
| `rotation` | number | no |  |  |
| `scaleX` | number | no |  |  |
| `scaleY` | number | no |  |  |

## Definition: `ai`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | enum: `groundChase`, `flyingChase`, `holdPosition` | yes |  |  |
| `preferredRange` | number | no |  | min: `0`; max: `100` |
| `retreatRange` | number | no |  | min: `0`; max: `100` |
| `reactionSeconds` | number | no |  | min: `0.02`; max: `10` |

## Definition: `name`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9-]*(?:/[a-z0-9][a-z0-9-]*)*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `drops`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `chance` | number | no |  | min: `0`; max: `1` |
| `items` | array of ref: `itemId` | no |  | unique items |
