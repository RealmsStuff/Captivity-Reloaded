# Mod-author test matrix

Use this matrix before publishing a mod release. It is intentionally smaller than the project's full [release verification runbook](release-verification-runbook.md): this page covers evidence a mod author should produce for one pack.

## Required for every pack

| Test | Pass condition |
| --- | --- |
| Schema validation | The repository validator reports no errors for the unpacked source. |
| Packager validation | **Validate Mod** reports no errors. |
| Clean install | The final `.capmod` installs through **Browse** or the catalog without manual extraction. |
| Restart | After the requested restart, the Mods panel reports the pack as loaded. |
| Advertised content | Every feature named in the release description can be reached and exercised. |
| Save and reload | The game saves, closes, restarts, and reconnects the pack's stable content IDs. |
| Disable and re-enable | Disabling removes runtime content safely; re-enabling reconnects retained save data. |
| Uninstall | Removing the pack does not prevent unrelated saves or Core content from loading. |
| Package identity | Tested bytes, SHA-256, manifest version, and published catalog record agree. |

Test the final packaged artifact, not only the loose development folder.

## Dependency and conflict tests

Run the rows that apply:

| Scenario | Expected result |
| --- | --- |
| Required dependency absent | Installation or loading is blocked with a specific dependency diagnostic. |
| Required dependency present | Both packs load in deterministic order. |
| Optional dependency absent | The pack loads using its documented fallback. |
| Declared conflict present | The conflicting combination is reported and does not silently override content. |
| Update from previous release | Existing IDs retain their saves and unlock state. |

If the release changes or removes an ID, document the consequence. Renaming an ID is removal plus addition unless an API migration mechanism explicitly says otherwise.

## Content-specific checks

### Asset patches

- Confirm every replacement in gameplay, not only in a preview.
- Change skins, equipment, scenes, or spawned instances when those paths can recreate the renderer.
- Disable the patch and confirm the Core baseline returns.
- Test alongside another patch of the same target to confirm the conflict is visible.

### Enemies and animations

- Exercise spawning, navigation, every attack, damage, death, drops, and wave scaling.
- Exercise every authored animation event and effect at least once.
- Test both facings and any paired player animation.
- Leave and re-enter the stage to catch stale runtime bindings.

### Clothing

- Check the wardrobe icon, equip/unequip, all supported skins or body variants, tearing, armor/stat effects, and save reload.
- Inspect movement poses rather than judging alignment only from the wardrobe preview.

### Weapons and usable items

- Exercise primary and alternate actions, ammunition/recharge rules, projectiles or beams, impacts, audio, and animation.
- Verify purchase, pickup, inventory, save reload, and unavailable-resource behavior where applicable.

### Stages and Tiled maps

- Test spawn, exit, restart, moving geometry, navigation, encounters, interactions, and referenced templates.
- Walk every intended route with both the player and relevant enemy movement types.
- Test at the largest intended enemy/object count and watch memory as well as frame rate.

### Rules, difficulties, and challenges

- Confirm the selection UI displays the correct name and description.
- Verify every override against both a new save/session and an existing compatible save.
- Confirm omitted fields inherit the documented baseline rather than an accidental editor value.

## Platform claims

Only list a platform as supported after installing and playing the final package on that platform's release build. A Windows-only test does not validate Android or WebGL AssetBundles. Record the game version, operating system or browser/device version, package digest, and result for each claimed platform.

Keep failures with their [validation codes](error-codes.md), logs, and reproduction steps. That record makes compatibility fixes and future API migrations much faster.
