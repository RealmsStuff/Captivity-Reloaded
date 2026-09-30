# Licensing and redistribution policy

The repository's `LICENSE` file is GNU GPL version 3. Source code distributed as part of Captivity Reloaded must retain that license, copyright notices, modification notices, and the corresponding-source obligations that apply when binaries are conveyed.

This repository also contains reconstructed game data, artwork, audio, third-party packages, converted community mods, and contributor submissions. A GPL file alone is not proof that every imported asset was contributed by its copyright holder. Until provenance and permission are recorded for a file, do not publish that file in the SDK, an example pack, a catalog release, or a binary distribution.

## Repository releases

- Publish source corresponding to each distributed binary and identify the exact source commit.
- Include `LICENSE` and a completed third-party notices/provenance file.
- Clearly mark Captivity Reloaded as modified and state the release date.
- Do not treat Unity, package-manager, store, font, audio, or other third-party content as relicensed merely because it is present in the project.
- Do not publish extracted Core art as a general-purpose asset library. Only include assets whose redistribution permission has been verified.

## SDK and examples

- Prefer original placeholder art and audio that contributors explicitly permit redistribution of.
- Reference Core content through public IDs and slots instead of copying Core files into a mod.
- Keep attribution and license text beside any third-party sample asset.
- Exclude local DragonBones exports and installed `Mods` content unless an intentionally reviewed copy is promoted into the SDK or `ExampleMods` with provenance.

## Catalog mods

- A catalog entry may link only to a release the listed authors are permitted to distribute.
- Mod authors retain ownership of their original work and must include a license or explicit redistribution permission in the release archive.
- Catalog inclusion is permission to mirror/list that particular release; it does not transfer ownership to the game project.
- Mods may depend on public Core IDs, but must not redistribute ripped Core or third-party assets without permission.
- Removed catalog entries do not remotely delete an already installed copy. Updates and removals must remain recoverable by the user.

## Release gate

Public release is blocked until the maintainers complete asset provenance/third-party notices and confirm that the intended game binaries, Mod SDK, examples, catalog archives, and preview images are all covered. This document is a project policy, not legal advice; uncertain rights require permission from the rightsholder or removal of the material.
