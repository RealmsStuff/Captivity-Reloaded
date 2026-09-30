# Catalog platform smoke tests

The pure storage and download policies are covered by EditMode tests for Windows, Android, and WebGL. The following hands-on checks require player builds and are intentionally deferred until a build is requested.

Use a clean persistent-data directory for each platform, then use the permanent `RealmsStuff/CR-Mods` catalog.

## Common workflow

1. Open Mods, enter Browse, and confirm catalog loading, search, type/status filters, update icons, and empty-result messaging.
2. Select a listing with preview PNGs. Confirm a blank box before completion, bounded loading, preserved aspect ratio, and three-second cycling.
3. Install `temp.catalog-dependent-mode` with its missing support dependency. Confirm both ZIPs are validated before either appears under Mods.
4. Restart and confirm both packs load. Confirm Installed/Not Installed filters and row colors update correctly.
5. Install an older fixture, restart, then confirm the update marker and Updates filter. Update it and restart.
6. Roll back, restart, and verify the previous version. Uninstall, restart, restore the removed copy, and restart again.
7. Modify one installed file and retry an update. Confirm the first action warns, the second preserves `.mod-modified`, and the replacement succeeds.
8. Disable networking and reopen Browse. Confirm the last valid cached catalog appears and local mods/startup remain unaffected.
9. Interrupt an update after the active folder moves away, relaunch, and confirm startup restores the valid backup.

## Platform-specific checks

| Platform | Required checks |
| --- | --- |
| Windows | Downloads stream to a temporary file; Mods remains beside the executable; no staging/recovery folders remain after success. |
| Android | Downloads stream into `Application.persistentDataPath`; install/update survives app suspension and a full relaunch; touch controls do not obstruct the Mods panel. |
| WebGL | Downloads remain in memory; a pack over 32 MiB and a batch over 64 MiB are rejected before download; installed data survives a page reload through browser persistence. |

Record the game version, commit, platform/runtime version, device/browser, and persistent-data cleanup method with every result. A passed Editor simulation does not count as a player-build pass.
