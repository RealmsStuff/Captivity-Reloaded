# Captivity Reloaded Asset Patch

> Generated from `asset-patch.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | stable |  |
| `type` | const: `assetPatch` | yes | stable |  |
| `id` | string | yes | stable | pattern: `^[a-z0-9][a-z0-9._-]*:patch/[a-z0-9][a-z0-9._/-]*$` |
| `target` | ref: `contentId` | yes | stable |  |
| `replacements` | object map of ref: `pngPath` | no | stable | min properties: `1`; max properties: `256` |
| `animations` | object map of string | no | experimental | min properties: `1`; max properties: `64`; Core enemy targets only: semantic clip name -> enemyAnimation that plays in place of that Animator state. |
| `playerAnimations` | object map of string | no | experimental | min properties: `1`; max properties: `32`; Core enemy targets only: finisher-phase-N -> playerAnimation for the player's side of that grab phase. |

## Definition: `contentId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:[a-z0-9][a-z0-9-]*(?:/[a-z0-9][a-z0-9-]*)*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `semanticName`

Schema form: string.

Constraints: max length: `64`; pattern: `^[a-z0-9]+(?:-[a-z0-9]+)*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `finisherPhase`

Schema form: string.

Constraints: pattern: `^finisher-phase-[1-9][0-9]?$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `pngPath`

Schema form: string.

Constraints: pattern: `^(?!/)(?!.*(?:^\|/)\.\.(?:/\|$))(?!.*\\).+\.[pP][nN][gG]$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |
