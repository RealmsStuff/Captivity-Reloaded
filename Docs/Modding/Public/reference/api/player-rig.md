# Captivity Reloaded Normalized Player Rig

> Generated from `player-rig.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | stable |  |
| `type` | const: `playerRig` | yes | stable |  |
| `id` | string | yes | stable | pattern: `^[a-z0-9][a-z0-9._-]*:player-rig/[a-z0-9][a-z0-9._/-]*$` |
| `coordinateSystem` | ref: `coordinateSystem` | yes | stable |  |
| `bones` | array of ref: `bone` | yes | stable | min items: `1`; max items: `128` |
| `clothingContract` | ref: `clothingContract` | yes | stable |  |
| `source` | ref: `source` | no | experimental |  |

## Definition: `boneName`

Schema form: enum: `hips`, `butt`, `spine`, `chest`, `neck`, `head`, `arm-right-upper`, `arm-right-lower`, `hand-right`, `arm-left-upper`, `arm-left-lower`, `hand-left`, `leg-right-upper`, `leg-right-lower`, `foot-right`, `leg-left-upper`, `leg-left-lower`, `foot-left`, `ear`, `face`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `coordinateSystem`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `units` | const: `world` | yes |  |  |
| `yUp` | const: `True` | yes |  |  |
| `rotation` | const: `degrees-clockwise-negative` | yes |  |  |
| `pixelsPerUnit` | number | yes |  | exclusive min: `0`; max: `1024` |

## Definition: `transform`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `x` | number | yes |  |  |
| `y` | number | yes |  |  |
| `rotation` | number | yes |  |  |
| `scaleX` | number | yes |  |  |
| `scaleY` | number | yes |  |  |

## Definition: `attachment`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `target` | string | yes |  | pattern: `^bone/[a-z0-9-]+$` |
| `pivotX` | number | yes |  | min: `0`; max: `1` |
| `pivotY` | number | yes |  | min: `0`; max: `1` |

## Definition: `spriteCanvas`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `width` | integer | yes |  | min: `1`; max: `8192` |
| `height` | integer | yes |  | min: `1`; max: `8192` |
| `pivotX` | number | yes |  |  |
| `pivotY` | number | yes |  |  |
| `sortingOrder` | integer | yes |  | min: `-32768`; max: `32767` |

## Definition: `bone`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `name` | ref: `boneName` | yes |  |  |
| `parent` | oneOf | yes |  |  |
| `defaultTransform` | ref: `transform` | yes |  |  |
| `attachment` | ref: `attachment` | yes |  |  |
| `spriteCanvas` | ref: `spriteCanvas` | no |  |  |

## Definition: `clothingContract`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `slots` | array of string | yes | stable | min items: `1`; unique items |
| `categories` | array of enum: `Hair`, `Upper`, `Lower`, `Shoes`, `Hat`, `Sleeves`, `Stockings`, `Other` | yes | stable | min items: `1`; unique items |
| `pivotCoordinates` | const: `normalized-bottom-left` | yes | stable |  |
| `canvasBounds` | const: `sprite-rectangle` | yes | stable | Artwork is not restricted to a legacy 32 by 32 canvas. |
| `sortingOrderMin` | const: `-32768` | yes | stable |  |
| `sortingOrderMax` | const: `32767` | yes | stable |  |
| `sortingOffsetMin` | const: `-100` | yes | stable |  |
| `sortingOffsetMax` | const: `100` | yes | stable |  |
| `sameCategoryCompatible` | const: `False` | yes | stable |  |
| `incompatibleCategoriesSymmetric` | const: `True` | yes | stable |  |
| `bodyVariantMatchOrder` | array of schema | yes | experimental |  |
| `bodyVariantFallback` | const: `visual.sprites` | yes | experimental |  |

## Definition: `source`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `unityPrefab` | string | no |  |  |
