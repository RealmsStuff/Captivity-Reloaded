# Captivity Reloaded modding wiki

Welcome to the Captivity Reloaded modding wiki. This documentation explains how to create, test, package, and publish mods for the data-driven Mod API.

> **Content notice:** The game and some examples contain mature sexual themes. Individual catalog entries may contain additional warnings.

Captivity Reloaded uses Unity **2021.3.45f1**. Most mods are ordinary folders of JSON and assets; Unity is used when you need the project’s authoring, preview, validation, or packaging tools.

## How to start modding

If this is your first mod, follow these pages in order:

1. [Set up the modding environment](tools/installing-authoring-tools.md).
2. Build [your first pack](getting-started/first-pack.md).
3. Learn how [pack IDs and files](concepts/packs-and-ids.md) work.
4. Copy the closest working pack from the [example pack index](reference/example-packs.md) or `ModSDK`.
5. [Validate](reference/validation.md) and [package](getting-started/packaging.md) the mod.
6. Complete the [mod-author test matrix](reference/author-test-matrix.md).
7. [Publish it to the community catalog](getting-started/publishing-to-catalog.md).

## What can be made with mods

### Good first projects

- [Asset patches](content/asset-patches.md) for published Core artwork slots.
- [Inherited weapons or usable items](content/weapons-and-items.md).
- [Inherited clothing](content/clothing.md), hairstyles, and equipped stat changes.
- [Challenges, difficulties, and rule profiles](content/rules.md).
- Small changes built from a maintained [example pack](reference/example-packs.md).

These projects reuse existing Core behavior and normally require fewer files and less runtime testing.

### More advanced projects

- [Original enemies](content/enemies.md) with custom atlases, movement, attacks, and effects.
- [Player and enemy animations](content/rig-animations-roadmap.md), including optional [DragonBones round trips](tools/dragonbones-round-trip.md).
- [Tiled stages and maps](content/stages-and-maps.md).
- [Stage scripts](content/stage-scripts.md), interactions, machines, encounters, and room transitions.
- Unity-authored assets packaged into platform-specific AssetBundles.

Advanced content has more cross-file references and should be tested on every platform and movement type it claims to support.

## How mods work

A mod is a versioned pack with a `manifest.json`, namespaced content IDs, and declared dependencies. The loader discovers enabled packs, resolves their order, validates their content, and registers supported definitions without allowing arbitrary mod C# code.

Mods can inherit reviewed Core templates, provide fully data-defined content, or replace explicitly published asset slots. Stable content IDs reconnect saves and dependencies across updates. Read [Core concepts](concepts/README.md) for the model and [Implementation status](reference/status.md) for the current feature boundary.

{% hint style="warning" %}
Mod API v1 is in freeze review. Fields marked **Experimental** may change before promotion. The JSON Schemas and runtime validator are authoritative over tutorial snippets.
{% endhint %}

## Installing and testing mods

Mod authors still need to test the same artifact their users receive. [Installing and managing mods](getting-started/installing-mods.md) covers loose folders, `.capmod` files, the in-game catalog, updates, rollback, disabling, and uninstalling. Read [Windows, Android, and WebGL](getting-started/platforms.md) before making cross-platform claims.

## Reference and troubleshooting

| Need | Go to |
| --- | --- |
| Exact JSON properties and limits | [Field-by-field API reference](reference/api-fields.md) |
| Authoritative JSON contracts | [Schema index](reference/schemas.md) |
| Patchable Core artwork | [Public asset-slot catalog](reference/asset-slots.md) |
| Runtime diagnostic codes | [Validation and error-code catalog](reference/error-codes.md) |
| Defaults and compatibility rules | [Defaults and compatibility](reference/defaults-and-compatibility.md) |
| Common failures | [Troubleshooting](reference/troubleshooting.md) |
| Mod API changes | [Changelog](reference/api-changelog.md) and [migration guides](reference/migration-guides.md) |

## Working on Captivity Reloaded

Project contributors should use:

- [Development setup](development/setup.md)
- [Building and testing](development/building-and-testing.md)
- [Repository layout](development/repository-layout.md)
- [Contributing documentation](development/contributing-documentation.md)
- [Release verification runbook](reference/release-verification-runbook.md)

The detailed cross-file contract lives in the [Mod API v1 specification](../spec-v1.md). Documents outside `Docs/Modding/Public` contain implementation history and planning notes rather than user instructions.
