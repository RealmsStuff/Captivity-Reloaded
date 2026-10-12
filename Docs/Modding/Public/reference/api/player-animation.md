# Captivity Reloaded Normalized Player Animation

> Generated from `player-animation.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | stable |  |
| `type` | const: `playerAnimation` | yes | stable |  |
| `id` | string | yes | stable | pattern: `^[a-z0-9][a-z0-9._-]*:player-animation/[a-z0-9][a-z0-9._/-]*$` |
| `rig` | string | yes | stable | pattern: `^[a-z0-9][a-z0-9._-]*:player-rig/[a-z0-9][a-z0-9._/-]*$` |
| `displayName` | string | yes | stable | min length: `1`; max length: `100` |
| `durationSeconds` | number | yes | stable | exclusive min: `0`; max: `600` |
| `frameRate` | number | yes | stable | exclusive min: `0`; max: `240` |
| `loop` | boolean | yes | stable |  |
| `tracks` | array of ref: `numericTrack` | yes | stable | max items: `1024` |
| `objectTracks` | array of ref: `objectTrack` | no | experimental | max items: `256` |
| `events` | array of ref: `event` | no | experimental | max items: `256` |
| `effects` | array of ref: `effect` | no | experimental | max items: `128` |
| `effectTriggers` | array of ref: `effectTrigger` | no | experimental | max items: `512` |
| `warnings` | array of string | no | experimental | unique items |
| `source` | ref: `source` | no | experimental |  |

## Definition: `target`

Schema form: string.

Constraints: pattern: `^(?:bone\|sprite)/(?:hips\|butt\|spine\|chest\|neck\|head\|arm-right-upper\|arm-right-lower\|hand-right\|arm-left-upper\|arm-left-lower\|hand-left\|leg-right-upper\|leg-right-lower\|foot-right\|leg-left-upper\|leg-left-lower\|foot-left\|ear\|face)$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `numericProperty`

Schema form: enum: `position.x`, `position.y`, `position.z`, `rotation.x`, `rotation.y`, `rotation.z`, `scale.x`, `scale.y`, `scale.z`, `color.r`, `color.g`, `color.b`, `color.a`, `sortingOrder`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `key`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0` |
| `value` | number | yes |  |  |
| `inTangent` | number null | yes |  |  |
| `outTangent` | number null | yes |  |  |

## Definition: `numericTrack`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `target` | ref: `target` | yes |  |  |
| `property` | ref: `numericProperty` | yes |  |  |
| `keys` | array of ref: `key` | yes |  | min items: `1` |

## Definition: `objectTrack`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `target` | ref: `target` | yes |  |  |
| `property` | const: `sprite` | yes |  |  |
| `keys` | array of object | yes |  | min items: `1` |

### Fields under `objectTrack.keys[]`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0` |
| `asset` | string null | no |  |  |
| `name` | string null | no |  |  |

## Definition: `event`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0` |
| `name` | string | yes |  | min length: `1` |
| `stringValue` | string | no |  |  |
| `floatValue` | number | no |  |  |
| `intValue` | integer | no |  |  |

## Definition: `effect`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `id` | string | yes |  | pattern: `^[a-z0-9][a-z0-9._/-]*$` |
| `kind` | enum: `particle`, `light`, `trail`, `decal` | no |  |  |
| `trigger` | enum: `thrust`, `cumThrust`, `unique` | yes |  |  |
| `index` | integer | yes |  | min: `0`; max: `127` |
| `target` | string | yes |  | pattern: `^(?:bone/(?:hips\|butt\|spine\|chest\|neck\|head\|arm-right-upper\|arm-right-lower\|hand-right\|arm-left-upper\|arm-left-lower\|hand-left\|leg-right-upper\|leg-right-lower\|foot-right\|leg-left-upper\|leg-left-lower\|foot-left\|ear\|face)\|enemy-bone/[A-Za-z0-9_. -]+)$` |
| `durationSeconds` | number | yes |  | min: `0`; max: `600` |
| `loop` | boolean | yes |  |  |
| `maxParticles` | integer | yes |  | min: `0`; max: `100000` |
| `startLifetimeMin` | number | yes |  |  |
| `startLifetimeMax` | number | yes |  |  |
| `startSpeedMin` | number | yes |  |  |
| `startSpeedMax` | number | yes |  |  |
| `startSizeMin` | number | yes |  |  |
| `startSizeMax` | number | yes |  |  |
| `gravityMin` | number | yes |  |  |
| `gravityMax` | number | yes |  |  |
| `scaleX` | number | yes |  |  |
| `scaleY` | number | yes |  |  |
| `scaleZ` | number | yes |  |  |
| `positionX` | number | no |  |  |
| `positionY` | number | no |  |  |
| `positionZ` | number | no |  |  |
| `rotationX` | number | no |  |  |
| `rotationY` | number | no |  |  |
| `rotationZ` | number | no |  |  |
| `simulationSpace` | enum: `Local`, `World`, `Custom` | yes |  |  |
| `startColorMin` | ref: `color` | yes |  |  |
| `startColorMax` | ref: `color` | yes |  |  |
| `emissionRateMin` | number | yes |  | min: `0` |
| `emissionRateMax` | number | yes |  | min: `0` |
| `shapeEnabled` | boolean | yes |  |  |
| `shape` | string | yes |  |  |
| `shapeRadius` | number | yes |  | min: `0` |
| `shapeAngle` | number | yes |  | min: `0`; max: `360` |
| `textureSheetTilesX` | integer | yes |  | min: `1`; max: `64` |
| `textureSheetTilesY` | integer | yes |  | min: `1`; max: `64` |
| `textureSheetFrame` | integer | no |  | min: `0`; max: `4095` |
| `textureSheetSprites` | array of string | yes |  | max items: `256` |
| `bursts` | array of ref: `particleBurst` | no |  | max items: `64` |
| `startLifetimeCurve` | ref: `particleCurve` | no |  |  |
| `startSpeedCurve` | ref: `particleCurve` | no |  |  |
| `startSizeCurve` | ref: `particleCurve` | no |  |  |
| `gravityCurve` | ref: `particleCurve` | no |  |  |
| `emissionRateCurve` | ref: `particleCurve` | no |  |  |
| `emissionRateOverDistanceCurve` | ref: `particleCurve` | no |  |  |
| `startColorGradient` | ref: `particleGradient` | no |  |  |
| `colorOverLifetime` | ref: `particleGradient` | no |  |  |
| `sizeOverLifetime` | ref: `particleAxes` | no |  |  |
| `velocityOverLifetime` | ref: `particleAxes` | no |  |  |
| `rotationOverLifetime` | ref: `particleAxes` | no |  |  |
| `noise` | ref: `particleNoise` | no |  |  |
| `textureSheetFrameOverTime` | ref: `particleCurve` | no |  |  |
| `textureSheetStartFrame` | ref: `particleCurve` | no |  |  |
| `textureSheetAnimation` | enum: `WholeSheet`, `SingleRow` | no |  |  |
| `textureSheetCycleCount` | integer | no |  | min: `1`; max: `1000` |
| `textureSheetRowIndex` | integer | no |  | min: `0`; max: `63` |
| `textureSheetUseRandomRow` | boolean | no |  |  |
| `sortingLayer` | string | no |  |  |
| `sortingOrder` | integer | yes |  | min: `-32768`; max: `32767` |
| `renderMode` | string | no |  |  |
| `mesh` | string | no |  |  |
| `material` | string | no |  |  |
| `texture` | string | no |  | pattern: `^(?![A-Za-z]:\|/\|.*(?:^\|/)\.\.(?:/\|$)).+\.png$` |
| `lightIntensity` | number | no |  | min: `0`; max: `20` |
| `lightRadius` | number | no |  | exclusive min: `0`; max: `100` |
| `trailWidth` | number | no |  | exclusive min: `0`; max: `20` |
| `trailTime` | number | no |  | exclusive min: `0`; max: `30` |
| `decalPixelsPerUnit` | number | no |  | min: `1`; max: `1024` |
| `source` | object | yes |  |  |

### Fields under `effect.source`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `unityPrefab` | string | yes |  |  |
| `hierarchyPath` | string | yes |  |  |

## Definition: `effectTrigger`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0` |
| `source` | enum: `enemy:Thrust`, `enemy:CumThrust`, `enemy:PlayParticleUnique` | yes |  |  |
| `effects` | array of string | yes |  | min items: `1`; unique items |

## Definition: `color`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `r` | number | yes |  |  |
| `g` | number | yes |  |  |
| `b` | number | yes |  |  |
| `a` | number | yes |  |  |

## Definition: `particleCurveKey`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  |  |
| `value` | number | yes |  |  |
| `inTangent` | number | yes |  |  |
| `outTangent` | number | yes |  |  |

## Definition: `particleCurve`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `mode` | enum: `Constant`, `Curve`, `TwoCurves`, `TwoConstants` | yes |  |  |
| `multiplier` | number | yes |  |  |
| `constantMin` | number | yes |  |  |
| `constantMax` | number | yes |  |  |
| `curveMin` | array of ref: `particleCurveKey` | yes |  | max items: `128` |
| `curveMax` | array of ref: `particleCurveKey` | yes |  | max items: `128` |

## Definition: `particleColorKey`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0`; max: `1` |
| `color` | ref: `color` | yes |  |  |

## Definition: `particleGradient`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `mode` | enum: `Color`, `Gradient`, `TwoColors`, `TwoGradients`, `RandomColor` | yes |  |  |
| `colorMin` | ref: `color` | no |  |  |
| `colorMax` | ref: `color` | no |  |  |
| `gradientMin` | array of ref: `particleColorKey` | yes |  | max items: `32` |
| `gradientMax` | array of ref: `particleColorKey` | yes |  | max items: `32` |

## Definition: `particleAxes`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `enabled` | boolean | yes |  |  |
| `separateAxes` | boolean | yes |  |  |
| `space` | enum: `Local`, `World`, `Custom` | no |  |  |
| `x` | ref: `particleCurve` | no |  |  |
| `y` | ref: `particleCurve` | no |  |  |
| `z` | ref: `particleCurve` | no |  |  |

## Definition: `particleBurst`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `time` | number | yes |  | min: `0` |
| `countMin` | number | yes |  | min: `0` |
| `countMax` | number | yes |  | min: `0` |
| `cycleCount` | integer | yes |  | min: `1`; max: `1000` |
| `repeatInterval` | number | yes |  | min: `0` |
| `probability` | number | yes |  | min: `0`; max: `1` |

## Definition: `particleNoise`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `enabled` | boolean | yes |  |  |
| `separateAxes` | boolean | yes |  |  |
| `strengthX` | ref: `particleCurve` | no |  |  |
| `strengthY` | ref: `particleCurve` | no |  |  |
| `strengthZ` | ref: `particleCurve` | no |  |  |
| `frequency` | number | yes |  | exclusive min: `0` |
| `scrollSpeed` | ref: `particleCurve` | no |  |  |
| `damping` | boolean | yes |  |  |
| `octaveCount` | integer | yes |  | min: `1`; max: `4` |
| `octaveMultiplier` | number | yes |  | min: `0`; max: `1` |
| `octaveScale` | number | yes |  | exclusive min: `0` |
| `quality` | enum: `Low`, `Medium`, `High` | no |  |  |

## Definition: `source`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `unityClip` | string | no |  |  |
