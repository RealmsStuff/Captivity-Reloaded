# Repository layout

## Runtime and Unity content

| Path | Contents |
| --- | --- |
| `Assets/Scripts/Assembly-CSharp` | Game runtime, adapters, factories, menus, and integration code |
| `Assets/Scripts/Modding` | Data contracts, parsers, registries, dependency logic, storage, and validation |
| `Assets/Editor/Modding` | Unity authoring, export, preview, migration, and packaging tools |
| `Assets/Resources/Modding/Core` | Packaged Core catalog and data definitions |
| `Assets/Tests/Editor` | EditMode parser, schema, editor, and regression tests |
| `Assets/Tests/PlayMode` | Runtime loader and state regression tests |

## Documentation and authoring material

| Path | Contents |
| --- | --- |
| `Docs/Modding/Public` | Published GitBook wiki and user-facing workflows |
| `Docs/Modding/spec-v1.md` | Detailed draft contract and cross-file behavior |
| `Docs/Modding/schemas` | Machine-readable Draft 2020-12 schemas |
| `Docs/Modding` | Architecture, migration, decisions, and internal implementation notes |
| `Docs/Planning` | Audits and future-work documents; not authoritative user instructions |
| `ExampleMods` | Runnable packs, regression fixtures, and maintained conversions |
| `ModSDK` | Copyable templates and normalized authoring references |

## Distribution and tooling

| Path | Contents |
| --- | --- |
| `ModCatalog` | Reviewed catalog fallback bundled into the game |
| `Tools/ModSchemaValidator` | Offline structural validation for packs and examples |
| `Tools/ModCatalogValidator` | Networked catalog/release integrity validation |
| `Tools/Release` | Read-only audit and Unity test orchestration |
| `.github/workflows` | Schema and catalog continuous-integration checks |

## Local and generated state

The following are development output, installed content, caches, or backups and should not be treated as source:

- `Library`, `Temp`, `obj`, `Logs`, `UserSettings`, and `.vs`;
- `Build`, `Builds`, and `TestResults`;
- generated `.csproj` and `.sln` files;
- `Mods` and installer state directories beginning with `.mod-`;
- `SaveBackups`, `ModAuthoringBackups`, and DragonBones working exports.

Consult `.gitignore` before promoting a generated result into the repository. Promotion should be intentional, minimal, and accompanied by provenance where assets are involved.
