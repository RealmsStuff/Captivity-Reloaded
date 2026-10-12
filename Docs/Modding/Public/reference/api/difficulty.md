# Captivity Reloaded Difficulty

> Generated from `difficulty.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | stable |  |
| `type` | const: `difficulty` | yes | stable |  |
| `id` | string | yes | stable | pattern: `^[a-z0-9][a-z0-9._-]*:difficulty/[a-z0-9][a-z0-9._/-]*$` |
| `displayName` | string | yes | stable | min length: `1`; max length: `40` |
| `sortOrder` | integer | no | stable | min: `-100000`; max: `100000`; default: `0` |
| `enemyHealthMultiplier` | ref: `multiplier` | yes | stable |  |
| `playerDamageTakenMultiplier` | ref: `multiplier` | yes | stable |  |
| `escapeStrengthMultiplier` | ref: `multiplier` | yes | stable |  |

## Definition: `multiplier`

Schema form: number.

Constraints: min: `0.1`; max: `10`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |
