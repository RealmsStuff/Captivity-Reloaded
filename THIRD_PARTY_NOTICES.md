# Third-party notices

This is the reviewed third-party and provenance inventory for the repository. "Present in the project" does not by itself establish redistribution permission. A player, SDK, example, or catalog release may include only groups whose status is **Cleared** for that artifact.

| Group | Status | Publication boundary |
| --- | --- | --- |
| Repository source code | Cleared under GPL-3.0 | Preserve `LICENSE`, notices, modification history, and corresponding-source obligations. |
| Kenney input artwork | Cleared under CC0 1.0 | Preserve the included source license files even though attribution is not required. |
| Mono.Data.Sqlite | Cleared under MIT | Include `ThirdPartyLicenses/Mono-MIT.txt`. |
| SQLite native library | Cleared as public-domain SQLite | Preserve the version, hash, and upstream notice below. |
| Unity packages and generated player components | Review per release | Apply Unity's current package/editor terms to the exact distributed build. |
| Original and reconstructed game content | Project distribution only | Preserve existing credits; do not represent individual extracted assets as independently relicensed. |
| Converted community and legacy examples | Permission varies; catalog release blocked unless cleared | Review each pack's metadata and `THIRD_PARTY.md` where present. Unknown or legacy attribution is not redistribution permission. |
| External authoring tools | Not distributed by the repository | Unity, Tiled, LibreSprite, and DragonBones installers remain external downloads or maintainer-local tools. |
| Public wiki text and screenshots | Cleared for project documentation | Screenshots illustrate the project UI and are not a standalone asset pack. |

## Kenney input artwork

The controller prompt artwork under `Assets/Resources/InputGlyphs/Controller` comes from Kenney "Input Prompts." The mobile control artwork under `Assets/Resources/InputGlyphs/Mobile` comes from Kenney "Mobile Controls." The included license files identify both sets as Creative Commons Zero (CC0 1.0):

- `Assets/Resources/InputGlyphs/Controller-License.txt`
- `Assets/Resources/InputGlyphs/Mobile-License.txt`

## Unity packages and TextMesh Pro

Unity Package Manager dependencies are pinned in `Packages/manifest.json` and `Packages/packages-lock.json`. Their applicable Unity/package license files and notices must accompany any distribution when required.

Unity Editor, Tiled, LibreSprite, and DragonBones installers are not part of the repository or a `.capmod`. Documentation may link to their official download pages. The maintainer-local DragonBones 5.6.3 installer and its Adobe AIR runtime must not be copied into the game source, SDK, wiki repository, or a mod release.

## Bundled database libraries

### Mono.Data.Sqlite

`Assets/Plugins/Mono.Data.Sqlite.dll` is the Mono ADO.NET provider for SQLite. Its assembly identity is `Mono.Data.Sqlite, Version=2.0.0.0`, and its file version is `1.0.61.0`. Mono class-library code is distributed under the MIT license; the required license text is included at `ThirdPartyLicenses/Mono-MIT.txt`.

- Upstream source: https://github.com/mono/mono/tree/main/mcs/class/Mono.Data.Sqlite
- Upstream license: https://github.com/mono/mono/blob/main/LICENSE
- SHA-256: `156124C42A8CA830E850E1D1ED22D7ACB3D8BC28677404259F53F8FB4B6C5748`

### SQLite

`Assets/Plugins/x86_64/sqlite3.dll` is SQLite `3.33.0` for 64-bit Windows. SQLite's authors dedicate SQLite source code to the public domain.

- Official release: https://www.sqlite.org/releaselog/3_33_0.html
- Copyright and public-domain statement: https://www.sqlite.org/copyright.html
- SHA-256: `75D6BDC2CE9E0E718F99897910BFADEAAC3D8D7CF2F08DDC4129F7441A525079`

## Original and reconstructed game content

Original and reconstructed game content is credited to its respective creators in project and mod metadata. Under the project's current distribution policy, it may remain part of Captivity Reloaded while those credits are preserved. This policy does not grant permission to extract or redistribute that art, audio, or data as a general-purpose asset library.

Converted community examples are a separate category. Attribution identifies provenance but does not establish permission. Packs with a `THIRD_PARTY.md` file retain their specific release gate; packs credited only to a legacy or unknown author are not eligible for public catalog archives until a maintainer records permission or replaces the affected material. They may remain as internal migration fixtures when they are excluded from public release artifacts.

## Release review procedure

Before publishing an artifact:

1. List every included top-level asset and package group.
2. Match each group to a row above and any pack-local notice.
3. Exclude groups with a blocked or unknown publication boundary.
4. Include `LICENSE`, this file, applicable third-party license text, and preserved creator credits.
5. Record the reviewed commit, artifact hash, reviewer, and date in the release evidence.

Documentation publication does not approve a player, SDK, example, or catalog release. Those artifacts require their own inventory because they contain different files.

Release provenance approval: **no**. Change this to **yes** only in the reviewed release commit after every included artifact group has been cleared and recorded in the release evidence.
