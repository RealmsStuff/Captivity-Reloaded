# Captivity Reloaded Tiled JSON Tileset

> Generated from `tiled-tileset.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

Experimental schema for an external Tiled JSON/TSJ tileset referenced by a Mod API v1 authored map. Tiled-owned editor metadata remains open.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | const: `tileset` | yes | experimental |  |
| `name` | string | yes | experimental | min length: `1` |
| `tilewidth` | integer | yes | experimental | min: `1`; max: `1000000` |
| `tileheight` | integer | yes | experimental | min: `1`; max: `1000000` |
| `tilecount` | integer | yes | experimental | min: `1`; max: `1000000` |
| `columns` | integer | yes | experimental | min: `1`; max: `1000000` |
| `spacing` | integer | no | experimental | min: `0`; default: `0` |
| `margin` | integer | no | experimental | min: `0`; default: `0` |
| `image` | ref: `pngPath` | yes | experimental |  |
| `imagewidth` | integer | yes | experimental | min: `1` |
| `imageheight` | integer | yes | experimental | min: `1` |
| `objectalignment` | enum: `unspecified`, `topleft`, `top`, `topright`, `left`, `center`, `right`, `bottomleft`, `bottom`, `bottomright` | no | experimental |  |
| `tiles` | array of ref: `tile` | no | experimental |  |
| `properties` | array of ref: `property` | no | experimental |  |
| `version` | string number | no | experimental |  |
| `tiledversion` | string | no | experimental |  |

## Definition: `pngPath`

Schema form: string.

Constraints: pattern: `^(?!/)(?![A-Za-z]:)(?!.*\\).+\.[pP][nN][gG]$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `tile`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `id` | integer | yes |  | min: `0` |
| `animation` | array of ref: `frame` | no |  | min items: `2`; max items: `256` |

## Definition: `frame`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `tileid` | integer | yes |  | min: `0` |
| `duration` | integer | yes |  | min: `16`; max: `60000` |

## Definition: `property`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `name` | string | yes |  | min length: `1` |
| `type` | enum: `string`, `file`, `bool`, `int`, `float`, `color` | yes |  |  |
| `value` | schema | yes |  |  |
