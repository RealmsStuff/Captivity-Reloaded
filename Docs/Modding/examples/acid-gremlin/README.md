# Acid Gremlin draft example

This example is a contract illustration, not an installable mod yet. Runtime mod loading has not been implemented and the referenced PNG is intentionally not included.

The intended pack contains:

```text
manifest.json
content/acid-gremlin.json
assets/sprites/acid-gremlin.png
```

The PNG is a uniform 128-by-128 cell atlas. Cell indices run left-to-right and then top-to-bottom. The JSON maps cells to inherited Gremlin rig slots. Unlisted optional slots retain their Core artwork during the draft vertical slice.

The first runtime acceptance test is successful when placing this folder in `Mods` registers the enemy, resolves the inherited Core Gremlin template and allows an explicit stage spawn-pool patch to use its content ID without rebuilding the game.
