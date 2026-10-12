# Captivity Reloaded Tiled Map Subset

> Generated from `tiled-map.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

Experimental schema for finite orthogonal Tiled JSON consumed by Mod API v1. Tiled may emit additional editor metadata, so document objects remain open while gameplay classes and runtime fields are constrained by the loader.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `orientation` | const: `orthogonal` | yes | experimental |  |
| `infinite` | const: `False` | no | experimental | default: `False` |
| `width` | integer | yes | experimental | min: `1`; max: `4096` |
| `height` | integer | yes | experimental | min: `1`; max: `4096` |
| `tilewidth` | integer | yes | experimental | min: `1`; max: `4096` |
| `tileheight` | integer | yes | experimental | min: `1`; max: `4096` |
| `layers` | array of ref: `layer` | yes | experimental | min items: `1` |
| `tilesets` | array of ref: `tileset` | yes | experimental |  |

## Definition: `safePath`

Schema form: string.

Constraints: pattern: `^(?!/)(?![A-Za-z]:)(?!.*\\).+$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `layer`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | enum: `tilelayer`, `objectgroup`, `imagelayer`, `group` | yes |  |  |
| `name` | string | no |  |  |
| `offsetx` | number | no |  |  |
| `offsety` | number | no |  |  |
| `x` | integer | no |  |  |
| `y` | integer | no |  |  |
| `width` | integer | no |  | min: `0` |
| `height` | integer | no |  | min: `0` |
| `opacity` | number | no |  | min: `0`; max: `1` |
| `visible` | boolean | no |  |  |
| `data` | array of integer | no |  |  |
| `layers` | array of ref: `layer` | no |  |  |
| `objects` | array of ref: `object` | no |  |  |
| `image` | ref: `safePath` | no |  |  |
| `repeatx` | boolean | no |  |  |
| `repeaty` | boolean | no |  |  |

## Definition: `tileset`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `firstgid` | integer | no |  | min: `1` |
| `source` | ref: `safePath` | no |  |  |
| `name` | string | no |  |  |
| `tilewidth` | integer | no |  | min: `1` |
| `tileheight` | integer | no |  | min: `1` |
| `tilecount` | integer | no |  | min: `1` |
| `columns` | integer | no |  | min: `1` |
| `spacing` | integer | no |  | min: `0` |
| `margin` | integer | no |  | min: `0` |
| `image` | ref: `safePath` | no |  |  |
| `imagewidth` | integer | no |  | min: `1` |
| `imageheight` | integer | no |  | min: `1` |

## Definition: `object`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `template` | ref: `safePath` | no |  |  |
| `id` | integer | no |  | min: `0` |
| `name` | string | no |  |  |
| `type` | ref: `gameplayClass` | no |  |  |
| `class` | ref: `gameplayClass` | no |  |  |
| `x` | number | no |  | min: `-100000`; max: `100000` |
| `y` | number | no |  | min: `-100000`; max: `100000` |
| `width` | number | no |  | min: `0`; max: `100000` |
| `height` | number | no |  | min: `0`; max: `100000` |
| `point` | boolean | no |  |  |
| `gid` | integer | no |  | min: `0`; max: `4294967295` |
| `rotation` | number | no |  |  |
| `visible` | boolean | no |  |  |
| `properties` | array of ref: `property` | no |  |  |

## Definition: `gameplayClass`

Schema form: enum: ``, `platform`, `player-spawn`, `enemy-spawner`, `scripted-actor`, `decoration`, `weapon-vendor`, `usable-vendor`, `weapon-case`, `door`, `door-switch`, `core-art`, `core-prop`, `core-stage-object`, `stage-marker`, `stage-item`, `note`, `keypad`, `altar`, `interaction`, `machine`, `logic-switch`, `easter-egg-step`, `light-bulb`, `proximity-light`, `freeform-light`, `global-light`, `point-light`, `pickup`, `moving-platform`, `particle-emitter`, `nav-node`, `ambient-audio`, `audio-source`, `room`, `room-entry`, `room-transition`, `script-trigger`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `property`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `name` | string | yes |  | min length: `1` |
| `type` | enum: `string`, `file`, `bool`, `int`, `float`, `color` | yes |  |  |
| `value` | schema | yes |  |  |
