# Mod API v1 release checklist

This page tracks release readiness rather than the long-term feature wishlist. Features listed as experimental can ship with v1 without receiving the stable-v1 compatibility promise.

## Runtime verification

- [x] Verify `spawnOnDeath` behavior for spawned variants; save/reload compatibility remains covered by the general save pass below.
- [x] Verify `speedPulse` with a visually obvious test enemy.
- [ ] Exercise explicit walking, climbing, and flying navigation links. Fully original ground enemies now consume directed walking/climb paths; verify upward traversal and natural downward falls on each authored map.
- [x] Confirm beam damage ticks, charge firing, alternate-fire mechanics, and primary melee.
- [x] Regression-test the charge percentage/ready prompt.
- [ ] Regression-test alternate-fire arm recoil.
- [ ] Play-test original-enemy `attackHit`/multi-hit timing, facing-relative impulses, positional event audio, camera shake, and atlas sprite effects.
- [ ] Play-test physical projectiles, impacts, casings, custom audio, and sprite animation.
- [ ] Test every example pack separately.
- [x] Run all currently compatible installed examples together.
- [x] Test stage exit/re-entry, restart-required mod state changes, and save loading.
- [ ] Play-test the prepared 256-by-96-tile stress stage with 15 spawners, 65 authored objects, explicit platform-climb and aerial routes, and a 60-enemy opening wave.

Moving platforms, Tiled visuals, Core-art objects, and authored doors have been exercised successfully, but remain part of the wider regression pass.

## Schema freeze

- [x] Manifest JSON Schema exists.
- [x] Challenge JSON Schema exists.
- [x] Rule-profile JSON Schema exists.
- [x] Add enemy, clothing, weapon, usable, stage, difficulty, asset-patch, and Tiled-map schemas.
- [x] Mark stable and experimental fields with `x-stability`.
- [x] Document defaults, bounds, fallbacks, unknown-field behavior, and migration rules.
- [x] Validate every public example against its applicable schema with `Tools/ModSchemaValidator`.

## Compatibility and release engineering

- [x] Explicitly exclude automatic old-content-ID aliases from v1 and document the author migration policy.
- [x] Confirm disabled mods retain their saved data and reconnect when enabled.
- [ ] Test missing, removed, upgraded, and renamed mod content against existing saves.
- [ ] Run the complete EditMode and PlayMode suites from a clean checkout.
- [ ] Produce and smoke-test a clean standalone Windows build.
- [x] Automate catalog schema, immutable release hash/size, ZIP traversal/extension, manifest agreement, corrupt dependency-batch, and extracted content-schema checks.
- [ ] Complete hands-on Windows, Android, and WebGL catalog install/update/rollback smoke tests.
- [ ] Test excessive runtime object counts in a player build.
- [x] Define and enforce practical file, texture, map, and content-count limits; performance budgets remain part of stress testing.
- [x] Reconcile the draft specification, public guide, examples, and published schemas.
- [x] Verify GitBook navigation and internal links.
- [x] Document installation, packaging, troubleshooting, and Windows desktop support.
- [x] Publish generated field-by-field schema, validation-code, and Core asset-slot references.
- [x] Establish the public API changelog and future migration-guide format.
- [x] Document code-confirmed Android and WebGL behavior without marking release-build verification complete.
- [x] Capture verified installation, Mods panel, Unity authoring, Tiled, and DragonBones screenshots.
- [x] Finalize licensing, redistribution, and documentation-publication policy text.
- [ ] Complete per-artifact provenance and permission decisions for the player build, SDK, examples, catalog archives, and previews selected for release.
- [ ] Remove generated/local files from the pull request and review the final diff.

## Explicitly not required to freeze v1

- Fully loose replacement of every Core prefab and asset.
- Arbitrary C# or Unity components in data-only mods.
- Entirely original enemy skeletons and animation controllers.
- Arbitrary new player clothing rig slots or clothing physics graphs.
- A standalone graphical mod-maker.
- External mod-folder discovery on Android or WebGL.
