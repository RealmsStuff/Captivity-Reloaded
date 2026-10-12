# Captivity Reloaded Stage Script

> Generated from `stage-script.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | experimental |  |
| `type` | const: `stageScript` | yes | experimental |  |
| `id` | string | yes | experimental | pattern: `^[a-z0-9][a-z0-9._-]*:stage-script/[a-z0-9][a-z0-9._/-]*$` |
| `stage` | string | yes | experimental | pattern: `^[a-z0-9][a-z0-9._-]*:stage/[a-z0-9][a-z0-9._/-]*$` |
| `variables` | object map of number | no | experimental | max properties: `64`; default: `` |
| `machines` | array of ref: `machine` | no | experimental | max items: `32`; default: `` |
| `sequences` | array of ref: `sequence` | yes | experimental | min items: `1`; max items: `64` |

## Definition: `name`

Schema form: string.

Constraints: pattern: `^[A-Za-z_][A-Za-z0-9_-]{0,63}$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `condition`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `source` | enum: `variable`, `wave`, `enemy-count`, `door-open`, `object-active` | no |  | default: `variable` |
| `variable` | ref: `name` | no |  |  |
| `objectId` | ref: `name` | no |  |  |
| `enemy` | string | no |  | pattern: `^[a-z0-9][a-z0-9._-]*:enemy/[a-z0-9][a-z0-9._/-]*$` |
| `operator` | enum: `==`, `!=`, `<`, `<=`, `>`, `>=` | yes |  |  |
| `value` | number | yes |  |  |

## Definition: `action`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `type` | enum: `wait`, `wait-for-door`, `wait-for-enemy-count`, `wait-for-signal`, `wait-for-sequence`, `wait-until`, `wait-until-player-distant`, `repeat`, `notify`, `set-variable`, `add-variable`, `send-signal`, `run-sequence`, `start-sequence`, `cancel-sequence`, `set-machine-state`, `set-object-active`, `spawn-actor`, `move-object`, `move-object-to`, `rotate-object`, `set-door`, `set-light`, `set-spawner-enabled`, `set-interaction-enabled`, `activate-interaction`, `queue-spawns`, `teleport-player`, `set-player-input`, `set-hud-visible`, `set-player-facing`, `remove-player-clothing`, `play-player-animation`, `kill-player`, `apply-player-status`, `camera-shake`, `camera-zoom`, `set-global-light`, `play-audio`, `stop-audio`, `play-particles`, `play-animation` | yes |  |  |
| `seconds` | number | no |  | min: `0`; max: `300` |
| `message` | string | no |  | min length: `1`; max length: `200` |
| `variable` | ref: `name` | no |  |  |
| `value` | number | no |  |  |
| `signal` | ref: `name` | no |  |  |
| `objectId` | ref: `name` | no |  |  |
| `destinationId` | ref: `name` | no |  |  |
| `active` | boolean | no |  |  |
| `state` | enum: `open`, `closed`, `toggle`, `on`, `off`, `at-most`, `at-least`, `left`, `right` | no |  |  |
| `amount` | integer | no |  | min: `1`; max: `100` |
| `animation` | ref: `name` | no |  |  |
| `controller` | enum: `current`, `finisher` | no |  |  |
| `sequence` | ref: `name` | no |  |  |
| `status` | enum: `jacky-curse` | no |  |  |
| `durationSeconds` | number | no |  | min: `0.1`; max: `100000` |
| `ticksPerSecond` | number | no |  | min: `0.01`; max: `60` |
| `chance` | number | no |  | min: `0`; max: `1` |
| `chanceIncrease` | number | no |  | min: `0`; max: `1` |
| `maxActive` | integer | no |  | min: `1`; max: `32` |
| `x` | number | no |  | min: `-100000`; max: `100000` |
| `y` | number | no |  | min: `-100000`; max: `100000` |
| `conditions` | array of ref: `condition` | no |  | min items: `1`; max items: `16` |
| `actions` | array of ref: `action` | no |  | min items: `1`; max items: `32` |

## Definition: `machineState`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `id` | ref: `name` | yes |  |  |
| `actions` | array of ref: `action` | yes |  | min items: `1`; max items: `32` |

## Definition: `machine`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `id` | ref: `name` | yes |  |  |
| `objectId` | ref: `name` | yes |  |  |
| `initialState` | ref: `name` | yes |  |  |
| `states` | array of ref: `machineState` | yes |  | min items: `1`; max items: `16` |

## Definition: `sequence`

Schema form: object.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `id` | ref: `name` | yes |  |  |
| `trigger` | enum: `stage-open`, `wave-start`, `wave-end`, `signal`, `interaction`, `player-enter`, `player-exit`, `manual`, `timer`, `door-state`, `object-state`, `enemy-count`, `machine-state` | yes |  |  |
| `signal` | ref: `name` | no |  |  |
| `once` | boolean | no |  | default: `False` |
| `objectId` | ref: `name` | no |  |  |
| `state` | enum: `open`, `closed`, `active`, `inactive`, `at-most`, `at-least` | no |  |  |
| `enemy` | string | no |  | pattern: `^[a-z0-9][a-z0-9._-]*:enemy/[a-z0-9][a-z0-9._/-]*$` |
| `value` | number | no |  |  |
| `seconds` | number | no |  | min: `0.1`; max: `3600` |
| `conditions` | array of ref: `condition` | no |  | max items: `16`; default: `` |
| `actions` | array of ref: `action` | yes |  | min items: `1`; max items: `128` |
