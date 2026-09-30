# Third-party notices

This inventory must be completed before a public source, SDK, or player release. "Present in the project" does not by itself establish redistribution permission.

## Kenney input artwork

The controller prompt artwork under `Assets/Resources/InputGlyphs/Controller` comes from Kenney "Input Prompts." The mobile control artwork under `Assets/Resources/InputGlyphs/Mobile` comes from Kenney "Mobile Controls." The included license files identify both sets as Creative Commons Zero (CC0 1.0):

- `Assets/Resources/InputGlyphs/Controller-License.txt`
- `Assets/Resources/InputGlyphs/Mobile-License.txt`

## Unity packages and TextMesh Pro

Unity Package Manager dependencies are pinned in `Packages/manifest.json` and `Packages/packages-lock.json`. Their applicable Unity/package license files and notices must accompany any distribution when required.

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

Original/reconstructed game content and converted community examples are credited to their respective creators in the project and mod metadata. The source material did not include separate license files. Under the project's release policy, these attributed assets are not treated as unresolved third-party provenance; maintainers must preserve their existing credits in redistributed copies.
