# Captivity Reloaded Asset Patch

> Generated from `asset-patch.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes | stable |  |
| `type` | const: `assetPatch` | yes | stable |  |
| `id` | string | yes | stable | pattern: `^[a-z0-9][a-z0-9._-]*:patch/[a-z0-9][a-z0-9._/-]*$` |
| `target` | ref: `contentId` | yes | stable |  |
| `replacements` | object map of ref: `pngPath` | yes | stable | min properties: `1`; max properties: `256` |

## Definition: `contentId`

Schema form: string.

Constraints: pattern: `^[a-z0-9][a-z0-9._-]*:[a-z0-9][a-z0-9-]*(?:/[a-z0-9][a-z0-9-]*)*$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |

## Definition: `pngPath`

Schema form: string.

Constraints: pattern: `^(?!/)(?!.*(?:^\|/)\.\.(?:/\|$))(?!.*\\).+\.[pP][nN][gG]$`.

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| _No named properties_ | | | | This definition is composed through references or conditional schemas. |
