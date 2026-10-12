# Packaging a `.capmod`

A `.capmod` is Captivity Reloaded's installable mod package. Internally it is a deterministic ZIP archive with `manifest.json` at its root. Use the project packager instead of assembling the archive by hand: it validates the source, rejects unsafe files, produces repeatable output, and reports the exact byte size and SHA-256 digest needed by the community catalog.

## Open the packager

In Unity 2021.3.45f1, select **Captivity Reloaded > Modding > Captivity Mod Packager**.

The window supports two workflows:

| Workflow | Use it for | Action |
| --- | --- | --- |
| Loose data mod | JSON, Tiled, PNG, audio, and other portable files | Choose the source folder, select **Validate Mod**, then **Build .capmod**. |
| Unity-authored mod | A loose data mod plus approved prefab assets | Choose the source folder and its Mod Authoring Profile, then select **Build Unity .capmod**. |

The source directory must contain `manifest.json` directly at its root. Save the resulting package outside the source directory.

## Archive root layout

Do not create the release by selecting the mod folder in Explorer and choosing **Compress to ZIP**. That normally adds an extra wrapper directory, and renaming the resulting `.zip` to `.capmod` does not repair it.

Correct `.capmod` contents:

```text
manifest.json
content/
assets/
levels/
```

Rejected wrapper layout:

```text
my-mod/
  manifest.json
  content/
  assets/
```

The official packager writes the correct root layout and `.capmod` extension. A source ZIP may be useful for collaborators, but it is not the installable release artifact.

## Data-only packages

Choose the directory containing the pack's manifest. **Validate Mod** checks the manifest, declared content roots, preview files, content definitions, file types, paths, counts, and size limits. Resolve every error before building. Warnings deserve review but do not necessarily prevent packaging.

Run the packager from the same game release or source commit that the mod claims to support. During the pre-v1 freeze period, `modApiVersion: 1` identifies the draft API line; it does not prove that experimental fields from two different development builds are interchangeable.

**Build .capmod** proposes a filename in this form:

```text
<pack-id>-<version>.capmod
```

The resulting archive preserves forward-slash relative paths and assigns a fixed internal timestamp so identical source files produce reproducible package contents.

## Unity-authored packages

Unity assets are optional. Use them only when a portable content type cannot express the required prefab presentation.

The Mod Authoring Profile supplies:

- the loose source directory;
- the prefab assets to package;
- the Windows, Android, and WebGL targets to build.

Every packaged prefab must be a unique prefab below `Assets/`, have a `CapmodPrefabDescriptor` on its root, and use an ID in the pack namespace under `prefab/...`. The first public bundle slice rejects other custom `MonoBehaviour` scripts. This is a data-oriented asset path, not arbitrary code loading.

Do not write `assetBundles` into the source manifest. **Build Unity .capmod** builds deterministic `content.bundle` files, places them below `bundles/<platform>/`, computes their sizes and SHA-256 digests, and writes the generated declarations into the staged manifest.

Install the Unity platform modules for every selected target. Selecting an unavailable build target produces `bundle.platform-module`.

## Accepted package contents

Installable archives accept these file extensions:

```text
.json  .tsj  .tx  .png  .ogg  .wav  .md  .txt  .bundle
```

Executables, scripts, symbolic links, reparse points, absolute paths, traversal paths, duplicate paths, and undeclared bundles are rejected. An archive may contain at most 4,096 entries and at most 512 MiB after extraction. Platform-specific download limits can be lower; see [Resource limits](../reference/resource-limits.md) and [Windows, Android, and WebGL](platforms.md).

## Verify the finished package

Before publishing:

1. Select **Validate Mod** in the exact Unity project revision used to build the target game. JSON syntax checking by itself is not sufficient.
2. Resolve every error, including unknown fields and unsupported event names. Do not assume a plausible field exists because a similar field is available on another content type.
3. Build the `.capmod` through the packager. Do not manually ZIP or rename a ZIP.
4. Copy the `.capmod` into the current build's `Mods` directory or select it with the Mods panel **Browse** action.
5. Install it through the game, restart when requested, and confirm its status is **Loaded**.
6. Exercise the advertised content using the [author test matrix](../reference/author-test-matrix.md).
7. Record the package's exact byte count and lowercase SHA-256 shown by the packager.
8. Install that exact final file once more. Do not test one archive and upload a rebuilt archive with a different digest.

If an AI assistant, custom generator, or third-party template produced any JSON, verify every generated property against the current [field reference](../reference/api-fields.md) or authoritative schemas before publishing. Generated prose, comments, and README claims do not establish API support.

Use the [validation/error-code catalog](../reference/error-codes.md) to investigate a packager or installer diagnostic.
