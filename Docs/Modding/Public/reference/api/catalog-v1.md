# Captivity Reloaded community mod catalog v1

> Generated from `catalog-v1.schema.json`. Runtime validation remains authoritative for cross-file and engine-dependent rules.

## Root fields

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `schemaVersion` | const: `1` | yes |  |  |
| `packs` | array of object | yes |  | max items: `500` |

### Fields under `root.packs[]`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `id` | string | yes |  | pattern: `^[a-z0-9][a-z0-9._-]*$` |
| `displayName` | string | yes |  | min length: `1`; max length: `120` |
| `authors` | ref: `authors` | yes |  |  |
| `summary` | string | yes |  | min length: `1`; max length: `4000` |
| `sourceRepository` | string | yes |  | pattern: `^https://github\.com/[^/?#]+/[^/?#]+$` |
| `tags` | ref: `tags` | no |  |  |
| `contentWarnings` | ref: `warnings` | no |  |  |
| `previewImages` | array of string | no |  | max items: `8`; unique items |
| `versions` | array of object | yes |  | min items: `1`; max items: `16` |

### Fields under `root.packs[].versions[]`

| Field | Type | Required | Stability | Constraints and meaning |
| --- | --- | --- | --- | --- |
| `version` | string | yes |  | min length: `5` |
| `modApiVersion` | integer | yes |  | min: `1`; max: `1000` |
| `gameVersion` | string | yes |  | min length: `1` |
| `download` | string | yes |  | pattern: `^https://github\.com/[^/?#]+/[^/?#]+/releases/download/[^/?#]+/[^/?#]+\.[zZ][iI][pP]$` |
| `sha256` | string | yes |  | pattern: `^[a-fA-F0-9]{64}$` |
| `sizeBytes` | integer | yes |  | min: `1`; max: `268435456` |
| `dependencies` | array of ref: `dependency` | no |  | max items: `32` |
| `conflicts` | array of string | no |  | max items: `32`; unique items |
