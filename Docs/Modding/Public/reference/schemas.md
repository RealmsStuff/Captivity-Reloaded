# Schema index

JSON Schemas under `Docs/Modding/schemas` define document structure, required properties, field types, enumerations, and numeric limits. Runtime validation remains authoritative for rules that depend on other files, loaded Core templates, asset dimensions, or content IDs.

For a browsable table of every root property and reusable definition, use the generated [field-by-field API reference](api-fields.md).

## Pack and distribution contracts

| Schema | Used for |
| --- | --- |
| `manifest.schema.json` | Pack identity, version, dependencies, conflicts, content roots, previews, and generated bundle metadata |
| `catalog-v1.schema.json` | Community catalog listings and immutable releases; not a pack content document |
| `asset-patch.schema.json` | Explicit replacement of published asset slots |

## Content contracts

| Schema | Content `type` | Guide |
| --- | --- | --- |
| `challenge.schema.json` | `challenge` | [Rules](../content/rules.md) |
| `clothing.schema.json` | `clothing` | [Clothing](../content/clothing.md) |
| `difficulty.schema.json` | `difficulty` | [Rules](../content/rules.md) |
| `enemy.schema.json` | `enemy` | [Enemies](../content/enemies.md) |
| `enemy-animation.schema.json` | `enemyAnimation` | [Enemies](../content/enemies.md) |
| `player-animation.schema.json` | `playerAnimation` | [Enemies](../content/enemies.md) |
| `player-attachment.schema.json` | `playerAttachment` | [Clothing](../content/clothing.md) |
| `rule-profile.schema.json` | `ruleProfile` | [Rules](../content/rules.md) |
| `stage.schema.json` | `stage` | [Stages and maps](../content/stages-and-maps.md) |
| `stage-script.schema.json` | `stageScript` | [Stage scripts](../content/stage-scripts.md) |
| `usable.schema.json` | `usable` | [Weapons and items](../content/weapons-and-items.md) |
| `weapon.schema.json` | `weapon` | [Weapons and items](../content/weapons-and-items.md) |

## Tooling and map contracts

| Schema | Used for |
| --- | --- |
| `enemy-rig-reference.schema.json` | Frozen Core-enemy rig reference exchanged with authoring tools |
| `player-rig.schema.json` | Normalized player rig data |
| `tiled-map.schema.json` | Supported Tiled JSON map subset |
| `tiled-tileset.schema.json` | External Tiled JSON/TSJ tilesets |

## Which source wins?

Use this order when sources seem to disagree:

1. Runtime parser and cross-file validation for what the current build accepts.
2. JSON Schema for structural validation and limits.
3. Public wiki for workflows and explanation.
4. Example packs for practical composition.
5. Internal planning documents for design history only.

Report any disagreement between the first three as a documentation or schema defect. Do not work around it by depending on an undocumented parser behavior.

## Validation

From the repository root:

```powershell
dotnet run --project Tools/ModSchemaValidator -- .
```

To validate one loose pack:

```powershell
dotnet run --project Tools/ModSchemaValidator -- . Mods/example.my-mod
```

See [Validation and errors](validation.md) for the difference between schema and runtime validation.
