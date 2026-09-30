# Release verification runbook

This is the required order for a Mod API release. A pass from the developer's long-lived Unity folder is useful diagnostics, but it does not count as release verification.

## 1. Freeze a release candidate

1. Run `Tools/Release/Invoke-ReleaseAudit.ps1`.
2. Review every tracked modification and every untracked source file.
3. Keep source, examples, SDK files, tests, documentation, Unity `.meta` files, schemas, and release tooling.
4. Exclude Unity caches, IDE projects, test/build output, installed mods, saves, installer recovery folders, and generated DragonBones working exports.
5. Commit the reviewed candidate to a dedicated release branch.
6. Clone that exact commit into a new directory. Do not copy its `Library`, `Mods`, saves, or settings from the development checkout.

## 2. Automated suites

Run the complete EditMode suite, then the complete PlayMode suite, with Unity 2021.3.45f1. Archive the XML and editor log with the commit hash. Zero discovered tests is a failure, even if Unity exits successfully.

The release record must contain: commit hash, Unity revision, platform, total/passed/failed/skipped counts, result paths, and tester/date.

## 3. Content matrix

Install exactly one example at a time, restart, load its content, exercise its advertised behavior, save, reload, and then remove it. Record every example manifest directory as its own row. After isolated passes, run all compatible examples together once to catch ID, ordering, override, and dependency conflicts.

The Tiled stress arena is a separate performance gate. It must be part of the release-candidate source—not a forgotten local `Mods` folder—and retain its 256-by-96 map, 15 spawners, authored navigation links, moving geometry, and 60-enemy opening wave.

## 4. Save compatibility matrix

Use copied test saves, never the only copy of a player's save.

| Scenario | Required result |
| --- | --- |
| Mod missing | Save opens; unresolved content is reported; unrelated progress remains intact. |
| Content removed in a newer mod | Save opens; removed IDs fail safely; remaining content reconnects. |
| Mod upgraded without ID changes | Existing unlocks/progress reconnect to the same stable IDs. |
| Content renamed without an explicit migration | It is treated as removed plus added; no silent reassignment occurs. |
| Disabled then re-enabled | Saved mod data is retained and reconnects after re-enable. |
| Interrupted update | The last valid installed version is restored from recovery state. |

Keep before/after copies and logs for each row.

## 5. Player builds and browser matrix

Build only after the source and automated-test gates pass. Produce Windows, Android, and WebGL from the same commit. Use a clean persistent-data location per platform.

On every platform verify: catalog load, preview load/fallback, first install, dependency batch install, update available, update, local-change warning, rollback, uninstall, restore, corrupt download rejection, hash mismatch rejection, restart behavior, and launch of installed content. Android must also be tested after a full app stop/relaunch. WebGL must be tested after browser storage is cleared and again with persisted storage.

## 6. Release evidence

Do not mark the release checklist complete from memory. Store a small release record containing test XML/log paths, build hashes, platform/browser/device versions, example results, save fixtures used, catalog commit/hash, known issues, and final approval.
