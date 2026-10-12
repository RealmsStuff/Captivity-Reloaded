# Troubleshooting

Start in the title-screen **Mods** panel. Select the pack and read the complete validation message before changing files. Invalid external packs are isolated so unrelated valid packs can still load.

## The mod does not appear

Check that:

- the pack contains `manifest.json` at its root;
- there is no extra directory created by ZIP extraction;
- Windows loose packs are under `Mods` beside the game executable;
- Android and WebGL installs were made through the in-game catalog or placed in the platform's persistent `Mods` directory;
- a `.capmod` archive was explicitly installed from **Mods > Local Files** rather than merely copied into `Mods`.

Correct:

```text
Mods/example.my-mod/manifest.json
```

Incorrect:

```text
Mods/example.my-mod/example.my-mod/manifest.json
```

The same rule applies inside an installable archive: opening the `.capmod` should show `manifest.json` immediately, not another folder. A manually created `.zip` is source material, not an installable package, and changing its extension does not flatten its contents.

## The pack is yellow, disabled, or conflicting

Common causes are:

- missing or incompatible required dependencies;
- duplicate pack IDs;
- an explicit conflict with another enabled pack;
- two packs replacing the same slot without an authorized override;
- content IDs using a namespace different from the manifest ID;
- a dependency cycle or unsupported version range.

Fix the reported pack first. A dependent pack cannot load while its required dependency is invalid or disabled.

## A JSON file is rejected

Run the repository validator:

```powershell
dotnet run --project Tools/ModSchemaValidator -- . Mods/example.my-mod
```

Typical structural failures include unknown fields, a wrong `type`, a missing required property, a value outside its allowed range, a Windows-style `\` path, or a path that leaves the pack directory. The JSON path in the error identifies the failing value.

Schema success does not prove that referenced IDs or assets exist. Launch the game and inspect the Mods panel for cross-file and runtime validation.

### `Could not find member ...`

The named property is not part of that document type in the running build. Check the matching field-reference page and schema. Do not merely rename it to another plausible term. A similarly named field on a Tiled object, rule profile, stage action, or another content type does not make it valid here.

If the property came from generated files, confirm that the referenced generator actually exists in the same source checkout and rerun it. A README that names a missing generator or planning document is not evidence that the output matches the public API.

### `player-animation.event-name`

Player-animation gameplay events use a strict allowlist: `pleasure`, `scaledPleasure`, `libido`, `strengthDamage`, `struggleDamage`, and `healthDamage`. Visual reactions belong in animation tracks or supported presentation effects. Names such as `hitReaction` are not inert visual labels and are rejected.

### The pack validates on another machine or branch

Compare the exact game version or source commit, not only `modApiVersion`. During the pre-v1 freeze period, experimental fields can differ between development revisions. Revalidate and rebuild the final `.capmod` with the project revision used for the target player.

## Artwork is missing or misaligned

- Confirm filename case matches the JSON path exactly.
- Use `/` in pack-relative JSON paths on every platform.
- Confirm the file is inside the pack and below the documented size limit.
- Check pixels per unit, pivot, atlas coordinates, and region dimensions.
- For inherited rigs, use published semantic region or asset-slot names rather than Unity object names.
- For Tiled maps, confirm tileset image paths resolve from the map or TSJ file and remain inside the pack.

## A change did not take effect

Close the Mods panel after making an install, update, removal, or enable-state change. The game reloads the title scene and rebuilds the content registry. Editing files while a gameplay stage is active does not hot-reload content.

If a catalog update reports local changes, review the locally modified copy before confirming replacement. The installer preserves recoverable backups, but it should not be treated as source control.

## Saves or unlocks appear missing

Persistent mod state is keyed by the full content ID. Reinstalling the same pack with the same IDs reconnects retained state. Renaming an ID creates different content; changing only `displayName` does not.

Do not reuse a released ID for unrelated content. See [Defaults and compatibility](defaults-and-compatibility.md).

## The command-line validator will not start

The first run may need NuGet network access. Run it from the repository root with a supported .NET SDK. Dependencies restore beneath `Temp/NuGetPackages`. If package restore is blocked, resolve the network/proxy issue and rerun; do not treat a failed tool launch as a successful content validation.

## Unity tests do not run

Use Unity `2021.3.45f1`. Batch tests still require a valid Unity license for the account running them. A zero-test result is a failure. See [Building and testing](../development/building-and-testing.md) and the [release verification runbook](release-verification-runbook.md).

## Before reporting a problem

Include:

- game build or source commit;
- platform;
- pack ID and version;
- whether the pack was loose, `.capmod`, or catalog-installed;
- the exact Mods-panel validation code and message;
- the smallest pack that reproduces the issue;
- whether the schema validator passes.

Do not include saves, screenshots, or logs containing private information without reviewing them first.
