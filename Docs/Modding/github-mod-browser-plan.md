# GitHub mod browser and downloader plan

Status: catalog browsing, searching/filtering, remote preview PNGs, archive preflight, bounded download, first install, staged updates, rollback, recoverable uninstall/restore, required-dependency batch installs, and local-change protection are implemented in source. Each release can declare required dependencies and conflicts; these must match the downloaded manifest. The browser resolves compatible dependency versions, shows the install set, rejects cycles/incompatible ranges/conflicts, and stages every ZIP before committing the batch. A failed commit attempts to restore all prior folders. Catalog installs record a bounded file-tree fingerprint in sibling `.mod-install-state` storage. An update with edited or untracked files requires a second confirmation and preserves a separate copy under `.mod-modified` before any active folder changes. The ledger retains multiple known release fingerprints so legitimate rollbacks remain recognized. Updates retain previous versions in sibling `.mod-backups` storage; startup restores a valid backup if an update was interrupted with no active pack. Uninstall moves a pack to sibling `.mod-removed` storage; the Mods screen can restore it. The permanent catalog is maintained at `RealmsStuff/CR-Mods`. First install and automatic dependency installation worked in the user's Unity test; update/rollback/uninstall/local-change protection and Windows/Android/WebGL player behavior still need in-game testing. The repository catalog validator verifies the catalog, remote previews, release hashes/sizes, ZIP safety, manifest agreement, and extracted content schemas.

## Mod-menu prerequisite

Before building the downloader, improve the installed-mods screen: ordered optional preview PNGs (blank box when absent, cycling every three seconds), visible manifest descriptions, green loaded rows, yellow invalid/conflicting rows, red disabled rows, no textual status prefixes, and nine visible rows. Reserve a Browse button at the upper right, but keep it unavailable until the catalog viewer is implemented. These menu changes do not install or download mods.

Implementation is in source; Unity Play Mode visual verification is still pending. The nine rows retain the original button thickness by using the unused space below the list and lowering the bottom controls, while the panel/background remains its original size. Browse is enabled and opens the catalog list.

## Goal

Add a **Browse** section to the existing Mods menu that can discover, install, and update approved data-only packs hosted on GitHub. Local unpacked folders remain the canonical installed format, and manually installed mods continue to work.

The BDCCMods model uses a repository-hosted JSON catalog whose entries contain display metadata, compatibility strings, and direct ZIP URLs. Captivity Reloaded can use the same simple catalog idea while adding immutable versions, checksums, dependency metadata, and strict installation validation.

## Repository model

Use a separate community catalog repository so accepting or removing a listing does not require a game release. A root `catalog-v1.json` contains one record per published pack and version:

```json
{
  "schemaVersion": 1,
  "packs": [
    {
      "id": "example.author.example-mod",
      "displayName": "Example Mod",
      "authors": ["Example Author"],
      "summary": "Adds an example enemy.",
      "sourceRepository": "https://github.com/example/example-mod",
      "tags": ["enemy"],
      "contentWarnings": [],
      "versions": [
        {
          "version": "1.0.0",
          "modApiVersion": 1,
          "gameVersion": ">=1.0.0",
          "download": "https://github.com/example/example-mod/releases/download/v1.0.0/example-mod.zip",
          "sha256": "...",
          "sizeBytes": 123456,
          "dependencies": [{"id": "example.author.shared-library", "version": ">=1.0.0"}],
          "conflicts": ["example.author.incompatible-mod"]
        }
      ]
    }
  ]
}
```

Use GitHub Release assets rather than ZIPs from a moving branch. A catalog entry must point to an immutable versioned archive and include its byte size and SHA-256 digest. Preview images may be linked separately and loaded only when the details page is opened.

## In-game flow

1. The Mods menu gains **Installed**, **Browse**, and **Updates** views.
2. Browse downloads the small catalog with a timeout and caches the last valid copy for offline use.
3. The player can search or filter by content type, author, compatibility, and installed state.
4. A details view shows description, version, dependencies, content warnings, source link, archive size, and validation status.
5. Install downloads a temporary ZIP, displays progress, checks the digest, safely extracts into a private sibling staging directory, and runs the existing manifest/content validator.
6. A valid staged pack is moved into its final `Mods/<pack-id>` directory. It becomes active after restart, matching the existing enable/disable behavior.
7. Updates use the same staged validation and retain the previous version until the replacement succeeds.

## Security and integrity rules

- Accept HTTPS GitHub release URLs only in the initial version.
- Impose download-size, extracted-size, file-count, texture, audio, and JSON limits.
- Reject absolute archive paths, `..` traversal, links/reparse points, and files outside the staged pack.
- Require exactly one root `manifest.json`; its ID and version must match the catalog record.
- Reject executable libraries and extensions outside the data-only allowlist.
- Verify SHA-256 before extraction and validate the complete pack before installation.
- Never extract directly over an installed pack.
- Keep one recoverable backup during an update and replace directories atomically where possible.
- Treat catalog descriptions, authors, and URLs as untrusted text; never interpret markup or commands.
- Isolate catalog/network failures from normal startup so offline play and local mods always work.

## Dependencies and updates

- Compare versions with the existing semantic-version implementation.
- Show required dependencies before installation and offer to install compatible catalog versions together.
- Show optional dependencies without forcing installation.
- Refuse dependency cycles or incompatible Mod API/game versions before downloading archives.
- Preserve the existing enabled state and namespaced save data during updates.
- Catalog `dependencies` and `conflicts` must exactly match the release ZIP's manifest lists; optional dependencies are not fetched automatically.
- Detect a locally modified or untracked installed folder, require confirmation, and preserve a separate copy before replacing it. Implemented in source; runtime testing remains.

## Catalog contribution workflow

1. A creator publishes a versioned ZIP as a GitHub Release asset.
2. They submit a pull request adding or updating their catalog record.
3. GitHub Actions downloads the archive, checks its digest and size, validates its manifest/content schemas, and rejects duplicate IDs or versions.
4. Maintainers review metadata, compatibility, previews, and content-warning tags before merging.
5. A generated compact catalog can be published for the game; the human-reviewed source records remain auditable in Git.

Licensing and redistribution requirements should be added only after the project decides its final policy.

## Implementation phases

1. Freeze the catalog schema and publish a small test repository with two example packs. The schema/parser contract and two test packs are live in the temporary repository.
2. Add a read-only catalog client with caching, timeouts, and an Installed/Browse comparison model. Implemented; live catalog publication remains.
3. Build the Browse and details UI using the existing Mods-menu style and controller/mouse navigation. Implemented, including search, type/status filters, update markers, and bounded remote preview slideshows.
4. Add bounded download progress, SHA-256 verification, and safe staged extraction. Implemented in source with exact test-archive hashes and sizes; runtime testing remains.
5. Add validated install, update, rollback, and optional recoverable uninstall behavior. Implemented in source; update/rollback/uninstall runtime testing remains. Removed packs are stored beside `Mods` rather than sent to the OS recycle bin.
6. Add dependency transactions and clearer compatibility/conflict messaging. Implemented in source; in-game testing with dependent catalog fixtures remains.
7. Add automated catalog checks, offline/error tests, corrupt-archive tests, and end-to-end Windows build tests. Catalog/release checks and corrupt transactional archive tests are implemented. Cache fallback and platform player smoke tests remain.

Before publishing a live catalog, freeze the Mod API schemas and archive limits and complete archive-install and platform tests; otherwise the downloader could distribute packs that the final v1 contract rejects.

The repeatable player checklist is maintained in [catalog-platform-smoke-tests.md](catalog-platform-smoke-tests.md). Player builds are not created automatically while iterating on the mod system.
