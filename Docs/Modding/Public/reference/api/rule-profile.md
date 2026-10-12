# Captivity Reloaded Rule Profile / Game Mode

> Generated from `rule-profile.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | stable |  |
| `type` | const: `ruleProfile` | yes | stable |  |
| `id` | string | yes | stable | pattern: `^[a-z0-9][a-z0-9._-]*:rule/[a-z0-9][a-z0-9._/-]*$` |
| `displayName` | string | yes | stable | min length: `1`; max length: `80` |
| `description` | string | no | stable |  |
| `activation` | enum: `selectable`, `pack` | no | experimental | default: `selectable` |
| `modules` | array of oneOf | yes | experimental | min items: `1`; max items: `16` |

## Definition: `weaponId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:item/weapon/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `weaponProgression`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `weaponProgression` | yes |  |  |
| `trigger` | const: `enemyKilled` | yes |  |  |
| `selection` | enum: `random`, `ordered` | yes |  |  |
| `starterWeapon` | ref: `weaponId` | yes |  |  |
| `weaponPool` | array of ref: `weaponId` | yes |  | min items: `1`; max items: `256`; unique items |
| `replaceExistingWeapons` | boolean | yes |  |  |

## Definition: `spawnModifiers`

Schema form: object.

Constraints: min properties: `2`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `spawnModifiers` | yes |  |  |
| `waveSpawnMultiplier` | ref: `multiplier` | no |  |  |
| `waveSpawnAdd` | ref: `add` | no |  |  |
| `maxStageEnemiesMultiplier` | ref: `multiplier` | no |  |  |
| `maxStageEnemiesAdd` | ref: `add` | no |  |  |
| `maxRoomEnemiesMultiplier` | ref: `multiplier` | no |  |  |
| `maxRoomEnemiesAdd` | ref: `add` | no |  |  |
| `spawnerChanceMultiplier` | ref: `multiplier` | no |  |  |
| `spawnDelayMultiplier` | ref: `multiplier` | no |  |  |
| `initialSpawnDelayMultiplier` | ref: `multiplier` | no |  |  |
| `enemySpeedVarianceMin` | number | no |  | min: `-10`; max: `10` |
| `enemySpeedVarianceMax` | number | no |  | min: `-10`; max: `10` |
| `enemyHealthVarianceMin` | number | no |  | min: `-0.99`; max: `10` |
| `enemyHealthVarianceMax` | number | no |  | min: `-0.99`; max: `10` |
| `enemyWaveHealthGrowthRandomBase` | number | no |  | min: `0.01`; max: `10` |
| `enemySpeedGrowthAfterWave` | integer | no |  | min: `0`; max: `10000` |
| `enemySpeedGrowthPerWave` | number | no |  | min: `0`; max: `10` |
| `zombieChaseChancePercent` | integer | no |  | min: `0`; max: `100` |
| `zombieFallChancePercent` | integer | no |  | min: `0`; max: `100` |
| `zombieChaseCheckSeconds` | number | no |  | min: `0.01`; max: `300` |
| `zombieChaseDurationMin` | number | no |  | min: `0.01`; max: `300` |
| `zombieChaseDurationMax` | number | no |  | min: `0.01`; max: `300` |
| `zombieChaseSpeedMaxBonus` | number | no |  | min: `0`; max: `100` |
| `deathHoundRetrySeconds` | number | no |  | min: `0.01`; max: `300` |
| `deathHoundPrepareSeconds` | number | no |  | min: `0.01`; max: `300` |
| `deathHoundRoamSpeedBonus` | number | no |  | min: `0`; max: `100` |
| `deathHoundChargeSpeedBonus` | number | no |  | min: `0`; max: `100` |
| `deathHoundChargeSeconds` | number | no |  | min: `0.01`; max: `300` |
| `deathHoundLeapXMultiplier` | number | no |  | min: `0.01`; max: `10` |
| `deathHoundLandCheckDelay` | number | no |  | min: `0`; max: `10` |
| `deathHoundPrepareInnerDivisor` | number | no |  | min: `0.01`; max: `100` |
| `deathHoundLeapDistanceDivisor` | number | no |  | min: `0.01`; max: `100` |
| `deathHoundHitDistance` | number | no |  | min: `0.01`; max: `100` |
| `flyChargeAccelBonus` | number | no |  | min: `0`; max: `100` |
| `flyChargeSpeedBonus` | number | no |  | min: `0`; max: `100` |
| `flyRechargeDelayMultiplier` | number | no |  | min: `0.1`; max: `10` |
| `mindBreakTargetMultiplier` | number | no |  | min: `0.01`; max: `10` |
| `hypnosisProgressMultiplier` | integer | no |  | min: `1`; max: `100` |
| `legacyShackAberrants` | boolean | no |  |  |
| `legacyShackTankLimit` | integer | no |  | min: `0`; max: `100` |
| `legacyShackTankDelaySeconds` | number | no |  | min: `0`; max: `300` |
| `legacyShackBoomerLimit` | integer | no |  | min: `0`; max: `100` |
| `legacyShackBoomerDelaySeconds` | number | no |  | min: `0`; max: `300` |
| `legacyShackPinkHoundLimit` | integer | no |  | min: `0`; max: `100` |
| `legacyShackPinkHoundDelaySeconds` | number | no |  | min: `0`; max: `300` |
| `legacyShackBoomerExplodes` | boolean | no |  |  |

## Definition: `waveRules`

Schema form: object.

Constraints: min properties: `2`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `waveRules` | yes |  |  |
| `spawnGrowthPerWave` | integer | no |  | min: `0`; max: `1000` |
| `intermissionSeconds` | integer | no |  | min: `0`; max: `300` |
| `maximumWaves` | integer | no |  | min: `1`; max: `10000` |

## Definition: `economyRules`

Schema form: object.

Constraints: min properties: `2`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `economyRules` | yes |  |  |
| `startingMoney` | integer | no |  | min: `0`; max: `1000000` |
| `infiniteMoney` | boolean | no | experimental |  |
| `bountyMultiplier` | number | no |  | min: `0`; max: `10` |
| `ammoDropChance` | number | no |  | min: `0`; max: `1` |
| `ammoDropChancePerWave` | number | no |  | min: `-1`; max: `1` |
| `ammoDropChanceMinimum` | number | no |  | min: `0`; max: `1` |
| `repeatConsumablePurchases` | boolean | no | experimental |  |
| `clothingRepairEnabled` | boolean | no | experimental |  |
| `clothingRepairCostMultiplier` | number | no | experimental | min: `0`; max: `10` |
| `ammoDropLifetimeSeconds` | number | no | experimental | min: `1`; max: `600` |
| `vendorsAlwaysAvailable` | boolean | no | experimental |  |
| `itemValueOffsets` | ref: `itemOffsets` | no |  |  |
| `weaponAmmoCapacityOffsets` | ref: `weaponOffsets` | no |  |  |

## Definition: `weaponTuning`

Schema form: object.

Constraints: min properties: `2`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `weaponTuning` | yes |  |  |
| `pistolEquipSeconds` | ref: `timing` | no |  |  |
| `pistolReloadSeconds` | ref: `timing` | no |  |  |
| `smgOneHandedEquipSeconds` | ref: `timing` | no |  |  |
| `smgOneHandedReloadSeconds` | ref: `timing` | no |  |  |
| `smgTwoHandedEquipSeconds` | ref: `timing` | no |  |  |
| `smgTwoHandedReloadSeconds` | ref: `timing` | no |  |  |
| `shotgunOneHandedEquipSeconds` | ref: `timing` | no |  |  |
| `shotgunOneHandedReloadSeconds` | ref: `timing` | no |  |  |
| `shotgunTwoHandedEquipSeconds` | ref: `timing` | no |  |  |
| `shotgunTwoHandedReloadSeconds` | ref: `timing` | no |  |  |
| `rifleEquipSeconds` | ref: `timing` | no |  |  |
| `rifleReloadSeconds` | ref: `timing` | no |  |  |
| `singleBarrelReloadSeconds` | ref: `timing` | no |  |  |
| `minimumShotDelaySeconds` | ref: `timing` | no |  |  |

## Definition: `playerRules`

Schema form: object.

Constraints: min properties: `2`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `playerRules` | yes |  |  |
| `playerDamageTakenMultiplier` | number | no |  | min: `0`; max: `10` |
| `grantAllWeapons` | boolean | no | experimental |  |
| `ignoreWeightLimit` | boolean | no | experimental |  |
| `debugHotkeys` | boolean | no | experimental |  |
| `startWithoutWeapon` | boolean | no | experimental |  |
| `gunDamageMultiplier` | number | no |  | min: `0.1`; max: `10` |
| `selfPleasureEnabled` | boolean | no | experimental |  |
| `selfPleasurePerSecond` | number | no | experimental | min: `0.1`; max: `1000` |
| `selfPleasureHeartCost` | integer | no | experimental | min: `0`; max: `1` |
| `enemyFinishersEnabled` | boolean | no | experimental |  |
| `clothingDamageEnabled` | boolean | no | experimental |  |
| `safeKnockouts` | boolean | no | experimental |  |
| `playerHealthMultiplier` | number | no | experimental | min: `0.1`; max: `10` |
| `maximumHearts` | integer | no | experimental | min: `1`; max: `12` |
| `pleasureMaximumMultiplier` | number | no | experimental | min: `0.1`; max: `10` |
| `libidoMaximumMultiplier` | number | no | experimental | min: `0.1`; max: `10` |
| `pleasureGainMultiplier` | number | no | experimental | min: `0`; max: `10` |
| `rapeLibidoGainMultiplier` | number | no | experimental | min: `0`; max: `10` |
| `voluntaryExposeSkipsEscape` | boolean | no | experimental |  |
| `retainBuffsOnClimax` | boolean | no | experimental |  |
| `strengthDamageWhileRestrainedMultiplier` | number | no | experimental | min: `0`; max: `10` |
| `strengthDamageFreeMultiplier` | number | no | experimental | min: `0`; max: `10` |
| `clothingDamageChance` | number | no | experimental | min: `0`; max: `1` |
| `forceAutomaticWeapons` | boolean | no | experimental |  |
| `autoReloadOnEmpty` | boolean | no | experimental |  |
| `weaponRangeMultiplier` | number | no | experimental | min: `0.1`; max: `10` |
| `cameraZoomMultiplier` | number | no | experimental | min: `0.25`; max: `4` |

## Definition: `consumableOverride`

Schema form: object.

Constraints: min properties: `3`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `consumableOverride` | yes |  |  |
| `item` | enum: `aspirin`, `morphine`, `anaphrodisiac`, `buffout`, `psycho`, `second-wind`, `siid-21` | yes |  |  |
| `healthRestore` | number | no |  | min: `0`; max: `10000` |
| `strengthRestoreMultiplier` | number | no |  | min: `0`; max: `10` |
| `libidoReduction` | number | no |  | min: `0`; max: `10000` |
| `pleasureReduction` | number | no |  | min: `0`; max: `10000` |
| `effectDurationSeconds` | number | no |  | min: `0`; max: `100000` |
| `restoreHeart` | boolean | no |  |  |
| `invulnerabilitySeconds` | number | no |  | min: `0`; max: `10000` |
| `gunDamageBonus` | number | no |  | min: `0`; max: `100` |
| `accelerationBonus` | number | no |  | min: `0`; max: `100` |
| `sprintBonus` | number | no |  | min: `0`; max: `100` |
| `dashBonus` | number | no |  | min: `0`; max: `100` |

## Definition: `eventReward`

Schema form: object.

Constraints: min properties: `3`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `eventReward` | yes |  |  |
| `trigger` | enum: `enemyKilled`, `playerClimax`, `playerBirth`, `playerInfusion`, `playerImplantation` | yes |  |  |
| `healthReward` | number | no |  | min: `0`; max: `10000` |
| `strengthReward` | number | no |  | min: `0`; max: `10000` |
| `pleasureReductionReward` | number | no |  | min: `0`; max: `10000` |
| `libidoReductionReward` | number | no |  | min: `0`; max: `10000` |
| `moneyReward` | integer | no |  | min: `0`; max: `1000000` |
| `perCompletedExperimentMultiplier` | number | no |  | min: `0`; max: `10` |

## Definition: `experimentScaling`

Schema form: object.

Constraints: min properties: `2`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `experimentScaling` | yes |  |  |
| `speedPerExperimentPercent` | number | no |  | min: `0`; max: `100` |
| `dashPerExperimentPercent` | number | no |  | min: `0`; max: `100` |
| `gunDamagePerExperimentPercent` | number | no |  | min: `0`; max: `100` |
| `healthPerExperimentPercent` | number | no |  | min: `0`; max: `100` |
| `knockbackPerExperimentPercent` | number | no |  | min: `0`; max: `100` |

## Definition: `scoringRules`

Schema form: object.

Constraints: min properties: `2`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `scoringRules` | yes |  |  |
| `killScore` | integer | no |  | min: `0`; max: `1000000` |
| `waveScore` | integer | no |  | min: `0`; max: `1000000` |
| `damageTakenPenalty` | integer | no |  | min: `0`; max: `1000000` |

## Definition: `goalRules`

Schema form: object.

Constraints: min properties: `2`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `goalRules` | yes |  |  |
| `targetScore` | integer | no |  | min: `1`; max: `100000000` |
| `targetKills` | integer | no |  | min: `1`; max: `1000000` |
| `targetWave` | integer | no |  | min: `1`; max: `10000` |
| `goalMatch` | enum: `all`, `any` | no |  |  |
| `timeLimitSeconds` | number | no |  | min: `1`; max: `86400` |
| `maxDamageEvents` | integer | no |  | min: `1`; max: `1000000` |

## Definition: `itemOffsets`

Schema form: object map of integer.

Constraints: max properties: `64`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `weaponOffsets`

Schema form: object map of integer.

Constraints: max properties: `64`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `timing`

Schema form: number.

Constraints: min: `0.02`; max: `30`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `multiplier`

Schema form: number.

Constraints: min: `0.1`; max: `10`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `add`

Schema form: integer.

Constraints: min: `-1000`; max: `1000`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |
