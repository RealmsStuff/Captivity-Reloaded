# Captivity Reloaded

Captivity Reloaded is a Unity 2021.3 LTS project with a data-driven Mod API, in-game mod manager, example packs, authoring SDK, and JSON Schema validation tools.

> **Content notice:** The game and some repository examples contain mature sexual themes. Review the repository and mod descriptions before distributing or demonstrating them.

## Choose a starting point

- **Players and mod users:** read the [wiki home](Docs/Modding/Public/README.md), then [installing and managing mods](Docs/Modding/Public/getting-started/installing-mods.md).
- **First-time mod authors:** follow [Your first pack](Docs/Modding/Public/getting-started/first-pack.md) and browse the [example pack index](Docs/Modding/Public/reference/example-packs.md).
- **Artists and map authors:** use the [authoring tools](Docs/Modding/Public/tools/README.md) and the templates under `ModSDK`.
- **Project contributors:** start with [development setup](Docs/Modding/Public/development/setup.md) and [building and testing](Docs/Modding/Public/development/building-and-testing.md).
- **API reference:** use the [schema index](Docs/Modding/Public/reference/schemas.md), [defaults and compatibility](Docs/Modding/Public/reference/defaults-and-compatibility.md), and the [draft v1 contract](Docs/Modding/spec-v1.md).

## Repository map

| Path | Purpose |
| --- | --- |
| `Assets` | Unity scenes, scripts, resources, editor tools, and tests |
| `Docs/Modding/Public` | Published GitBook-compatible wiki |
| `Docs/Modding/schemas` | Draft 2020-12 JSON Schemas |
| `ExampleMods` | Runnable examples and maintained conversions |
| `ModSDK` | Copyable art, animation, enemy, weapon, clothing, and map templates |
| `ModCatalog` | Catalog fallback bundled with the game |
| `Tools` | Schema, catalog, test, and release utilities |

## Current status

The Mod API is in v1 freeze review. Stable candidates and experimental fields are identified in the schemas. Runtime support is broad, but the [release checklist](Docs/Modding/Public/reference/v1-release-checklist.md) still contains platform, gameplay, and licensing gates.

See [Implementation status](Docs/Modding/Public/reference/status.md) for the supported feature boundary. Do not infer release readiness from a parser or schema existing.

## License and redistribution

Source licensing and asset redistribution are separate concerns in this reconstructed project. Read [Licensing and redistribution](Docs/Modding/Public/reference/licensing-and-redistribution.md) and `THIRD_PARTY_NOTICES.md` before publishing binaries, SDK assets, converted mods, or catalog releases.
