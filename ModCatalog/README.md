# Captivity Reloaded mod catalog

This directory mirrors the catalog bundled into the game as its offline fallback. Entries must conform to
[`catalog-v1.schema.json`](../Docs/Modding/schemas/catalog-v1.schema.json). The game fetches the raw
`catalog-v1.json` file, validates the complete document, and falls back to its last valid cache when offline.

The online endpoint is the permanent `RealmsStuff/CR-Mods` community repository. This bundled copy is the
offline fallback and should be refreshed from a reviewed catalog release before each game release. Installed
packs are updated transactionally: previous versions remain available for rollback, local changes require an
explicit confirmation, and uninstall keeps a recoverable copy.
