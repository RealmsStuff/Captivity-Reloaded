# Windows, Android, and WebGL

The runtime contains platform-specific mod storage, package, and AssetBundle paths for Windows, Android, and WebGL. Windows is the primary development target. Android and WebGL support is implemented but is not yet release-certified; the remaining hands-on checks are tracked in the [v1 release checklist](../reference/v1-release-checklist.md).

## Platform matrix

| Behavior | Windows | Android | WebGL |
| --- | --- | --- | --- |
| Mod storage | `Mods` beside the game executable | `Mods` inside `Application.persistentDataPath` | `Mods` inside `Application.persistentDataPath` |
| Intended install route | Manual folder, local `.capmod`, or catalog | In-game catalog | In-game catalog |
| AssetBundle platform tag | `windows` | `android` | `webgl` |
| Maximum one archive | 256 MiB | 256 MiB | 32 MiB |
| Maximum dependency install batch | 512 MiB | 512 MiB | 64 MiB |
| Catalog download staging | Temporary file | Temporary file | Memory |
| Release-build smoke test | Required | Pending | Pending |

The size values above are enforced by the current runtime. See [Resource limits](../reference/resource-limits.md) for the rest of the safety ceilings.

## Android author instructions

Build an Android AssetBundle whenever the pack uses Unity-bundled assets and declare it with `"platform": "android"` in `assetBundles`. Portable JSON and image-only packs do not need a bundle unless their content type requires one.

Install through the in-game catalog. The runtime resolves storage as `Application.persistentDataPath/Mods`; its physical location is device- and Unity-version-dependent, so the public workflow does not require authors or players to browse it directly. Restart the game after installation so discovery and dependency ordering run from a clean startup.

Before advertising Android support, exercise install, update, rollback, enable/disable, dependency failure, and an actual gameplay load on the target release build. This hands-on matrix is not yet complete.

## WebGL author instructions

Build a WebGL AssetBundle and declare it with `"platform": "webgl"` when the pack uses bundled Unity assets. Keep archives below 32 MiB and the selected pack plus newly required dependencies below 64 MiB. WebGL downloads are staged in memory, so a package that fits desktop or Android limits can still be rejected.

Use the in-game catalog rather than assuming filesystem access. The runtime targets `Application.persistentDataPath/Mods`; browser persistence and quota behavior depend on the hosting and browser environment. Restart or reload the game after installation, then verify that the installed pack survives a fresh session.

Before advertising WebGL support, test catalog refresh, install, persistence after reload, update, rollback, dependency errors, quota failure, and gameplay in the exact hosted build. This hands-on matrix is not yet complete.

## Portable-pack fallback

If a manifest lists AssetBundles, a local `.capmod` must contain the bundle for the current platform. At runtime, a missing or unloadable matching bundle can produce a warning and allow portable data fallbacks where the content type supports them. Do not rely on that warning path as cross-platform support: either ship the matching bundle or verify that the pack is genuinely portable.

This page deliberately distinguishes code-confirmed behavior from release certification. A checked implementation path is not evidence that a browser, device, store build, or hosting configuration has passed the release matrix.
