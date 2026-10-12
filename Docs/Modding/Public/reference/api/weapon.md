# Captivity Reloaded Weapon

> Generated from `weapon.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | stable |  |
| `type` | const: `weapon` | yes | stable |  |
| `id` | ref: `weaponId` | yes | stable |  |
| `displayName` | string | yes | stable | min length: `1` |
| `extends` | ref: `coreWeaponId` | no | stable |  |
| `description` | string | no | stable |  |
| `stats` | ref: `stats` | no | stable |  |
| `visual` | ref: `visual` | yes | stable |  |
| `behavior` | ref: `behavior` | no | experimental |  |

## Definition: `weaponId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:item/weapon/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `prefabId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:prefab/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `coreWeaponId`

Schema form: string.

Constraints: pattern: `^core:item/weapon/[a-z0-9][a-z0-9._/-]*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `path`

Schema form: string.

Constraints: pattern: `^(?!/)(?!.*(?:^\|/)\.\.(?:/\|$))(?!.*\\).+$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `pngPath`

Schema form: allOf.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `audioPath`

Schema form: allOf.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `color`

Schema form: string.

Constraints: pattern: `^#[0-9A-Fa-f]{6}(?:[0-9A-Fa-f]{2})?$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `stats`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `damage` | integer | no |  | min: `0`; max: `10000` |
| `ammoMax` | integer | no |  | min: `1`; max: `100000` |
| `magazineSize` | integer | no |  | min: `1`; max: `100000` |
| `bulletsPerShot` | integer | no |  | min: `1`; max: `64` |
| `penetration` | integer | no |  | min: `0`; max: `64` |
| `rangeMultiplier` | number | no |  | min: `0.05`; max: `1` |
| `fireIntervalSeconds` | number | no |  | min: `0.02`; max: `10` |
| `recoil` | number | no |  | min: `0`; max: `1` |
| `movementRecoil` | number | no |  | min: `0`; max: `1` |
| `knockbackX` | number | no |  | min: `0`; max: `1000` |
| `knockbackY` | number | no |  | min: `0`; max: `1000` |
| `reloadSeconds` | number | no |  | min: `0.01`; max: `60` |
| `equipSeconds` | number | no |  | min: `0`; max: `60` |
| `weight` | integer | no |  | min: `0`; max: `100000` |
| `value` | integer | no |  | min: `0`; max: `100000000` |
| `infiniteAmmo` | boolean | no |  |  |
| `semiAutomatic` | boolean | no |  |  |
| `marketable` | boolean | no |  |  |
| `holdType` | enum: `OneHanded`, `TwoHanded1`, `TwoHanded2`, `TwoHanded3` | no |  |  |
| `reloadType` | enum: `Magazine`, `PumpAction`, `SingleBarrel` | no |  |  |
| `weaponType` | enum: `Pistol`, `Smg`, `Shotgun`, `Rifle`, `Special` | no |  |  |

## Definition: `visual`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | enum: `coreWeaponSprites`, `originalWeaponSprites` | yes |  |  |
| `pixelsPerUnit` | number | no |  | exclusive min: `0`; max: `1024`; default: `32` |
| `bundlePrefab` | ref: `prefabId` | no | experimental |  |
| `sprites` | object map of ref: `pngPath` | yes |  | min properties: `1` |
| `hiddenSlots` | array of string | no | experimental | unique items |
| `pivotX` | number | no | experimental | min: `0`; max: `1` |
| `pivotY` | number | no | experimental | min: `0`; max: `1` |
| `muzzleOffsetX` | number | no | experimental | min: `-10`; max: `10` |
| `muzzleOffsetY` | number | no | experimental | min: `-10`; max: `10` |
| `colliderWidth` | number | no | experimental | min: `0.01`; max: `10` |
| `colliderHeight` | number | no | experimental | min: `0.01`; max: `10` |
| `colliderOffsetX` | number | no | experimental | min: `-10`; max: `10` |
| `colliderOffsetY` | number | no | experimental | min: `-10`; max: `10` |
| `sortingOrder` | integer | no | experimental | min: `-100`; max: `100` |
| `parts` | object map of ref: `part` | no | experimental | max properties: `16` |

## Definition: `part`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `pivotX` | number | no |  | min: `0`; max: `1` |
| `pivotY` | number | no |  | min: `0`; max: `1` |
| `offsetX` | number | no |  | min: `-10`; max: `10` |
| `offsetY` | number | no |  | min: `-10`; max: `10` |
| `rotationDegrees` | number | no |  | min: `-360`; max: `360` |
| `scaleX` | number | no |  | min: `-10`; max: `10` |
| `scaleY` | number | no |  | min: `-10`; max: `10` |
| `sortingOrder` | integer | no |  | min: `-100`; max: `100` |

## Definition: `behavior`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `burstCount` | integer | no |  | min: `1`; max: `12` |
| `burstIntervalSeconds` | number | no |  | min: `0.02`; max: `2` |
| `tracerColor` | ref: `color` | no |  |  |
| `tracerThickness` | integer | no |  | min: `1`; max: `32` |
| `tracerFrames` | integer | no |  | min: `1`; max: `60` |
| `cameraShake` | boolean | no |  |  |
| `shooterKnockback` | boolean | no |  |  |
| `soundVolume` | number | no |  | min: `0`; max: `1` |
| `playShootSound` | boolean | no |  |  |
| `showMuzzleFlash` | boolean | no |  |  |
| `shootSounds` | array of ref: `audioPath` | no |  | min items: `1`; max items: `16` |
| `reloadSounds` | array of ref: `audioPath` | no |  | min items: `1`; max items: `16` |
| `muzzleFlashSprites` | array of ref: `pngPath` | no |  | min items: `1`; max items: `16` |
| `effectPixelsPerUnit` | number | no |  | min: `1`; max: `1024` |
| `casingSprite` | ref: `pngPath` | no |  |  |
| `casingOn` | enum: `shoot`, `reload`, `both` | no |  |  |
| `casingForceX` | number | no |  | min: `-100`; max: `100` |
| `casingForceY` | number | no |  | min: `-100`; max: `100` |
| `casingLifetimeSeconds` | number | no |  | min: `0.1`; max: `60` |
| `casingOffsetX` | number | no |  | min: `-10`; max: `10` |
| `casingOffsetY` | number | no |  | min: `-10`; max: `10` |
| `casingAngularVelocity` | number | no |  | min: `-10000`; max: `10000` |
| `casingGravityScale` | number | no |  | min: `0`; max: `20` |
| `muzzleFlashOffsetX` | number | no |  | min: `-10`; max: `10` |
| `muzzleFlashOffsetY` | number | no |  | min: `-10`; max: `10` |
| `muzzleFlashDurationSeconds` | number | no |  | min: `0.01`; max: `2` |
| `muzzleLightIntensityMultiplier` | number | no |  | min: `0`; max: `10` |
| `projectile` | ref: `projectile` | no |  |  |
| `melee` | ref: `melee` | no |  |  |
| `charge` | ref: `charge` | no |  |  |
| `beam` | ref: `beam` | no |  |  |
| `throwable` | ref: `throwable` | no |  |  |
| `alternateFire` | ref: `alternateFire` | no |  |  |
| `animations` | ref: `animations` | no |  |  |
| `ammunition` | ref: `ammunition` | no |  |  |

## Definition: `projectile`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `mode` | enum: `hitscan`, `physical` | no |  | default: `hitscan` |
| `sprite` | ref: `pngPath` | no |  |  |
| `pixelsPerUnit` | number | no |  | min: `1`; max: `1024` |
| `speed` | number | no |  | min: `0.1`; max: `250` |
| `gravity` | number | no |  | min: `-100`; max: `100` |
| `lifetimeSeconds` | number | no |  | min: `0.05`; max: `30` |
| `impactSprites` | array of ref: `pngPath` | no |  | min items: `1`; max items: `16` |
| `impactLifetimeSeconds` | number | no |  | min: `0.02`; max: `10` |
| `explosionRadius` | number | no |  | min: `0`; max: `25` |
| `explosionDamageMultiplier` | number | no |  | min: `0`; max: `10` |

## Definition: `melee`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `input` | enum: `primary`, `alternate` | no |  | default: `primary` |
| `range` | number | no |  | min: `0.1`; max: `10` |
| `radius` | number | no |  | min: `0.05`; max: `5` |
| `damageMultiplier` | number | no |  | min: `0.01`; max: `10` |
| `knockbackX` | number | no |  | min: `0`; max: `1000` |
| `knockbackY` | number | no |  | min: `0`; max: `1000` |
| `maxTargets` | integer | no |  | min: `1`; max: `32` |

## Definition: `charge`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `seconds` | number | no |  | min: `0.1`; max: `10` |
| `minimumDamageMultiplier` | number | no |  | min: `0.01`; max: `10` |
| `maximumDamageMultiplier` | number | no |  | min: `0.01`; max: `25` |
| `chargingText` | string | no |  | min length: `1`; max length: `80` |
| `readyText` | string | no |  | min length: `1`; max length: `80` |
| `chargingColor` | ref: `color` | no |  |  |
| `readyColor` | ref: `color` | no |  |  |
| `tintStrength` | number | no |  | min: `0`; max: `1` |

## Definition: `beam`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `input` | enum: `primary`, `alternate` | no |  | default: `primary` |
| `durationSeconds` | number | no |  | min: `0.05`; max: `10` |
| `tickIntervalSeconds` | number | no |  | min: `0.02`; max: `2` |
| `damageMultiplier` | number | no |  | min: `0`; max: `10` |
| `ammoPerTick` | integer | no |  | min: `0`; max: `1000` |
| `color` | ref: `color` | no |  |  |
| `thickness` | integer | no |  | min: `1`; max: `32` |

## Definition: `throwable`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `throwSpeed` | number | no |  | min: `0.1`; max: `100` |
| `gravity` | number | no |  | min: `0`; max: `20` |
| `angularVelocity` | number | no |  | min: `-10000`; max: `10000` |
| `lifetimeSeconds` | number | no |  | min: `0.1`; max: `120` |
| `impactDamageMultiplier` | number | no |  | min: `0`; max: `25` |
| `destroyOnImpact` | boolean | no |  |  |
| `explodeOnImpact` | boolean | no |  |  |
| `fuseSeconds` | number | no |  | min: `0.05`; max: `60` |
| `explosionRadius` | number | no |  | min: `0`; max: `25` |
| `explosionDamageMultiplier` | number | no |  | min: `0`; max: `25` |
| `explosionForce` | number | no |  | min: `0`; max: `1000` |
| `tickIntervalSeconds` | number | no |  | min: `0.05`; max: `10` |
| `impactSounds` | array of ref: `audioPath` | no |  | min items: `1`; max items: `16` |
| `tickSounds` | array of ref: `audioPath` | no |  | min items: `1`; max items: `16` |
| `explosionSounds` | array of ref: `audioPath` | no |  | min items: `1`; max items: `16` |
| `explosionColor` | ref: `color` | no |  |  |
| `explosionStyle` | enum: `burst`, `fire` | no |  |  |
| `igniteDurationSeconds` | number | no |  | min: `0.1`; max: `60` |
| `igniteDamagePerTick` | number | no |  | min: `0`; max: `1000` |
| `igniteTicksPerSecond` | number | no |  | min: `0.1`; max: `20` |

## Definition: `alternateFire`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `mode` | enum: `hitscan`, `projectile`, `melee`, `beam` | yes |  |  |
| `damageMultiplier` | number | no |  | min: `0`; max: `25` |
| `ammoCost` | integer | no |  | min: `0`; max: `10000` |
| `cooldownSeconds` | number | no |  | min: `0.02`; max: `30` |
| `burstCount` | integer | no |  | min: `1`; max: `12` |
| `burstIntervalSeconds` | number | no |  | min: `0.02`; max: `2` |
| `recoilMultiplier` | number | no |  | min: `0`; max: `10` |

## Definition: `ammunition`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | enum: `standard`, `fmj`, `hollow-point`, `incendiary`, `toxic`, `freeze`, `custom` | no |  |  |
| `damageMultiplier` | number | no |  | min: `0`; max: `10` |
| `penetrationBonus` | integer | no |  | min: `0`; max: `64` |
| `weakpointMultiplier` | number | no |  | min: `0`; max: `10` |
| `damageOverTime` | number | no |  | min: `0`; max: `10000` |
| `effectDurationSeconds` | number | no |  | min: `0.05`; max: `60` |
| `tickIntervalSeconds` | number | no |  | min: `0.05`; max: `10` |
| `slowMultiplier` | number | no |  | min: `0`; max: `1` |
| `slowDurationSeconds` | number | no |  | min: `0.05`; max: `60` |

## Definition: `animations`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `idleState` | ref: `state` | no |  |  |
| `fireState` | ref: `state` | no |  |  |
| `reloadState` | ref: `state` | no |  |  |
| `fireSpeedMultiplier` | number | no |  | min: `0.05`; max: `10` |
| `reloadSpeedMultiplier` | number | no |  | min: `0.05`; max: `10` |
| `clips` | object map of ref: `clip` | no |  | max properties: `6` |

## Definition: `state`

Schema form: string.

Constraints: min length: `1`; max length: `80`; pattern: `^[^/\\]+$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `clip`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `loop` | boolean | no |  |  |
| `frames` | array of ref: `frame` | yes |  | min items: `1`; max items: `120` |

## Definition: `frame`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `durationSeconds` | number | yes |  | min: `0.01`; max: `10` |
| `sprites` | object map of ref: `pngPath` | yes |  | min properties: `1` |
