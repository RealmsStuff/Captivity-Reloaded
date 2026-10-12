# Captivity Reloaded Core Enemy Rig Reference

> Generated from `enemy-rig-reference.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | stable |  |
| `type` | const: `enemyRigReference` | yes | stable |  |
| `id` | string | yes | stable | pattern: `^core:enemy-rig/[a-z0-9][a-z0-9._/-]*$` |
| `enemy` | string | yes | stable | pattern: `^core:enemy/[a-z0-9][a-z0-9._/-]*$` |
| `prefab` | string | yes | stable | min length: `1` |
| `controller` | string null | no | stable |  |
| `sampleRoot` | string | yes | stable |  |
| `bones` | array of ref: `bone` | yes | stable | min items: `1`; max items: `512` |
| `warnings` | array of string | yes | stable |  |

## Definition: `transform`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `x` | number | yes |  |  |
| `y` | number | yes |  |  |
| `z` | number | yes |  |  |
| `rotationX` | number | yes |  |  |
| `rotationY` | number | yes |  |  |
| `rotationZ` | number | yes |  |  |
| `scaleX` | number | yes |  |  |
| `scaleY` | number | yes |  |  |
| `scaleZ` | number | yes |  |  |

## Definition: `sprite`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `unityPath` | string | yes |  |  |
| `name` | string null | no |  |  |
| `asset` | string null | no |  |  |
| `pivotX` | number | yes |  |  |
| `pivotY` | number | yes |  |  |
| `width` | number | yes |  | min: `0` |
| `height` | number | yes |  | min: `0` |
| `sortingLayer` | string | yes |  |  |
| `sortingOrder` | integer | yes |  | min: `-32768`; max: `32767` |

## Definition: `bone`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `name` | string | yes |  | pattern: `^[a-z0-9][a-z0-9-]*$` |
| `parent` | string null | no |  | pattern: `^[a-z0-9][a-z0-9-]*$` |
| `unityPath` | string | yes |  |  |
| `unityPaths` | array of string | yes |  | min items: `1`; unique items |
| `defaultTransform` | ref: `transform` | yes |  |  |
| `sprites` | array of ref: `sprite` | yes |  |  |
