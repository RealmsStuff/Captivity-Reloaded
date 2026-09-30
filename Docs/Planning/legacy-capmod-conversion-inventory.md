# Legacy `.capmod` conversion inventory

Status date: 2026-09-29

This inventory maps the locally archived releases under `Mods to possible features` to their maintained source packs under `ExampleMods`. It distinguishes content conversion from packaging: a source directory with a manifest and validated content is `.capmod`-ready, but it is not a release artifact until the packager exports and validates the archive.

## Summary

- Archived top-level folders: **31**
- Combination collections that do not need their own conversion: **2**
- Exact duplicate aliases: **4**
- Distinct standalone legacy releases: **25**
- Maintained source conversions with claimed complete intent: **25**
- Maintained conversions requiring a focused parity audit: **0**
- Known partial conversions: **0**
- Legacy release artifacts currently present under `Builds/Mods`: **25**

The additive Nerf Pistol also has a `.capmod` test artifact, but it is a new additive example rather than a faithful standalone legacy release.

### Status meanings

| Status | Meaning |
|---|---|
| Converted | Maintained source represents the original release's documented intent. It still needs release packaging and isolated testing. |
| Audit | A source conversion exists, but a specific parity question remains. |
| Partial | Only part of the original release is represented. Do not publish it as a complete conversion. |
| Duplicate | The archived payload is byte-identical to another listed source. |
| Composition | An archive combines other mods and should be replaced by dependency-based installation. |

## Standalone releases

| Archived source | Maintained source | Status | Packaged now | Evidence and remaining work |
|---|---|---:|---:|---|
| `Apothem gun game v2` | `ExampleMods/apothem-gun-game` | Converted | Yes | Recreates the documented kill-triggered weapon progression and starter pistol. Its corrected 21-Core-weapon progression passed in-game testing on 2026-09-29. |
| `C4C Mod` | `ExampleMods/legacy-c4c` | Converted | Yes | Direct DLL comparison recovered its exact ammo rules, five-heart/arousal changes, drug balance, infusion/implantation rewards, expose behavior, and embedded Shack configuration. C4C's Boomers use the original non-exploding variant and its Pink Hound delay differs from the standalone overhaul. Its isolated gameplay pass completed on 2026-09-29, including voluntary surrender, all Shack variants, five-heart/libido limits, infusions and repeat purchases. |
| `Captivity Multi-Tool` | `ExampleMods/developer-toolkit` | Converted | Yes | Direct DLL comparison established that it is C4C plus a four-class tool-menu patch. The C4C baseline and every recovered menu family are represented; registered content replaces hard-coded arrays, continuous options clear with the profile, and permanent save actions require confirmation. Runtime testing remains. |
| `captivity sfw version` | `ExampleMods/captivity-sfw` | Converted | Yes | Contains the 32 anatomy replacements plus the reviewed no-finisher, no-clothing-damage, safe-knockout and health/stamina rules. Its isolated visual and gameplay pass completed on 2026-09-29. |
| `CAR (Cry When Raped)` | `ExampleMods/cry-when-raped` | Converted | Yes | Faithful player-face artwork replacement. Its in-game visual pass completed on 2026-09-29. |
| `Clothing Overhaul+Shack Downgrade` | `ExampleMods/legacy-clothing-overhaul` | Converted | Yes | Clothing-loss pressure and the three timed Shack aberrants are represented. Its in-game gameplay pass completed on 2026-09-29. |
| `CoD Perk Machine` | `ExampleMods/cod-perk-machine` | Converted | Yes | Replaces the usable/medicine vendor artwork and preserves its normal behavior. Its in-game replacement and vendor-behavior pass completed before the isolated conversion sweep. |
| `CoD Wonderweapon` | `ExampleMods/cod-wonderweapon` | Converted | Yes | Replaces the Core Schockgewehr artwork. Its isolated in-game visual pass completed on 2026-09-29. |
| `dakozan extended difficulties` | `ExampleMods/dakozan-extended-difficulties` | Converted and play-tested | Yes | Very Hard and Nightmare both load; user verification passed on 2026-09-29. |
| `Femboy edition` | `ExampleMods/femboy-refitted-shirt` | Converted | Yes | Both supplied archives were checked; the body, placed-character pieces and all affected Core clothing are represented. Its isolated in-game visual pass completed on 2026-09-29. |
| `Futanari + Small Tweaks` | `ExampleMods/futanari-small-tweaks` | Converted | Yes | Depends on `legacy.small-tweaks` and adds the five unique anatomy replacements. Its isolated dependency-combination visual pass completed on 2026-09-29. |
| `FutaZombies` | `ExampleMods/futazombies` | Converted | Yes | Patches all three Core zombie rigs without creating replacement enemy identities. Its isolated in-game visual pass completed on 2026-09-29. |
| `Goblin Slayer Armor` | `ExampleMods/goblin-slayer-armor` | Converted | Yes | Patches the four existing Jungle Knight garments and preserves their unlock path and gameplay data. Its corrected wardrobe previews, armor artwork and helmet hair hiding passed in-game testing on 2026-09-29. |
| `Luin's Balance Mod` | `ExampleMods/legacy-luins-balance` | Converted | Yes | Rules source includes economy, weapon timing, spawn/enemy tuning, drugs, clothing pressure and shortened challenge counters. Its in-game gameplay pass completed on 2026-09-29. |
| `Nerf Gun` | `ExampleMods/simple-nerf-gun` | Converted | Yes | Faithful three-sprite starter-pistol replacement. Its in-game visual pass completed on 2026-09-29. `additive-nerf-pistol` is intentionally a separate demonstration, not its replacement. |
| `Overpower` | `ExampleMods/legacy-overpower` | Converted | Yes | Includes its C4C-derived baseline, automatic fire, reload, range/camera, experiment scaling and event rewards. Its in-game gameplay pass completed on 2026-09-29. |
| `Prey Mod (Bunny Girls)` | `ExampleMods/prey-bunny-girls` | Converted | Yes | All 154 authored texture replacements are represented across player palettes, customization, clothing, zombies, placed actors and stage artwork. Four compression-only records are excluded. Its isolated in-game visual pass completed on 2026-09-29. `prey-green-zombie` remains a separate authoring demonstration. |
| `Raygun Revolver` | `ExampleMods/raygun-revolver` | Converted | Yes | Faithful Core .44 Revolver artwork replacement. Its isolated in-game visual pass completed on 2026-09-29. |
| `shaded girl mod` | `ExampleMods/shaded-girl` | Converted | Yes | Replaces the documented player body slots for all four original skin palettes. Its isolated in-game visual pass completed on 2026-09-29. |
| `Small Tweaks` | `ExampleMods/small-tweaks` | Converted | Yes | Preserves nine intentional player, hair, placed-character and muzzle-flash changes. Its isolated in-game visual pass completed on 2026-09-29. |
| `Smaller Breast and Butt Mod` | `ExampleMods/smaller-breast-and-butt` | Converted | Yes | Depends on Smaller Breast and contributes the unique butt/clothing/paired-character edits. Its isolated dependency-combination visual pass completed on 2026-09-29. |
| `Smaller Breast Mod` | `ExampleMods/smaller-breast` | Converted | Yes | Body, clothing, icon and paired-character edits are represented. Six omitted spine PNGs discovered by the regression suite were recovered from the original archive on 2026-09-29. Its isolated in-game visual pass completed on 2026-09-29. |
| `Smiling Blush + Small Tweaks` | `ExampleMods/smiling-blush-small-tweaks` | Converted | Yes | Depends on Small Tweaks and contributes the twelve unique mouth replacements. Its isolated dependency-combination visual pass completed on 2026-09-29. |
| `start with no gun` | `ExampleMods/start-with-no-gun` | Converted | Yes | Removes the free starter weapon and grants the documented $500. Its in-game gameplay pass completed on 2026-09-29. |
| `tweaked FER` | `ExampleMods/tweaked-fer` | Converted | Yes | Preserves all 30 materially changed FER character/blood records. Its in-game visual pass completed on 2026-09-29. |

## Exact duplicate aliases

These do not need additional conversions. Full SHA-256 equality was checked on 2026-09-29.

| Alias | Canonical archived source | Payload evidence |
|---|---|---|
| `ClothingOverhaulMod` | `Clothing Overhaul+Shack Downgrade` | `Assembly-CSharp.dll` is byte-identical: `0CAE8AD39CB34A3B650242A7FFB6B025AAAB03FF42F0C3990EEAF148A75C1244`. |
| `Femboy` | `Femboy edition` | `sharedassets0.assets` is byte-identical: `84BCF05217C96B66864098BC70E821A18ED977BC5766EA9DC7A96CA730848BE0`. |
| `goblin slayer` | `Goblin Slayer Armor` | `sharedassets0.assets` is byte-identical: `FB94000B81B85395F0B6E370D84A4A988925A71C7E5CCC76F7BCD72212CB9304`. |
| `simple nerf gun` | `Nerf Gun` | `sharedassets0.assets` is byte-identical: `F3CEB1AA18ADFE92D7DA2246252B23D571344255B9C8FB45E077BB3DF7E1D0BD`. |

## Composition archives

### `Combinations`

This folder contains 32 RAR/ZIP/link combination entries (29 RAR files, two ZIP files and one link text file) assembled from already listed releases. They are not independent authored mods. Do not create monolithic `.capmod` ports for them. The browser's dependency resolver, conflicts and load ordering should recreate supported combinations from the maintained standalone packages.

Before deleting or moving the historical archives, retain their filenames as compatibility recipes. A filename does not prove that every constituent version is safe together.

### `Debug Edition Mods`

The six archives apply visual combinations to the original Debug build. They do not need separate catalog entries. Known Debug-only content is maintained separately in `ExampleMods/original-dev-extras` (throwables, pregnancy belly and Brothel prototype).

The serialized-object audit completed on 2026-09-29. Relative to `Debug Small Tweaks`, CAR changes exactly one texture, Nerf Gun changes exactly three, and Smiling Blush + Goblin Slayer is the union of 17 mouth textures and 13 Knight textures. The large Futa Zombies + CoD Perk Machine + CAR archive changes 43 textures: the vendor, the CAR blush, 40 images matching the maintained FutaZombies PNGs, and one additional Hand texture. The Debug Smiling Blush artwork is a distinct 17-mouth revision rather than the ordinary 12-image release. Those 18 Debug-specific visual records still need semantic ownership/extraction before they can be added to `original-dev-extras`; none justify publishing the combination archives themselves.

## Required next work

1. **Captivity Multi-Tool runtime verification**
   - Select **Captivity Multi-Tool**, open the panel with `F1`, and exercise each tab in Shack.
   - Confirm continuous cheats clear after selecting another profile.
   - Test the confirmation/cancel paths without using Confirm on a release save unless the permanent unlock is intended.

2. **Debug-only visual supplement**
   - Resolve the extra Futa hand's actor/rig ownership and extract it losslessly.
   - Extract and map the distinct 17-mouth Debug Smiling Blush revision without changing the ordinary release conversion.
   - Add those optional records to `original-dev-extras`, not to six monolithic combination packs.

3. **Install-test the packaged sources**
   - Stage each archive into an empty Mods directory, validate it, launch its advertised content, restart, and uninstall it.
   - All 25 archives were regenerated or checked with portable forward-slash ZIP paths, passed the strict static archive checks, and reproduced byte-for-byte in a second build on 2026-09-29.

4. **Record publication eligibility**
   - A technically complete conversion is not automatically redistributable.
   - Resolve each pack's provenance and permission before adding it to the permanent public catalog.
