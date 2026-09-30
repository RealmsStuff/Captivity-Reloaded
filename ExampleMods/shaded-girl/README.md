# Shaded Girl conversion

This is a data-only conversion of the legacy **Shaded girl mod** by `@dudleytheschemer`. It replaces 13 semantic body-part slots for each of the player's four skin tones. Shared left/right limb artwork is declared once and applied to both sides by Core.

The PNGs were recovered from the supplied legacy `sharedassets0.assets`. Unrelated embedded textures with generic names such as `Chest`, `Hand`, and `LegUpper` were excluded by matching the player artwork, skin palette, and alpha silhouettes.

To enable it in an editor checkout, copy this entire `shaded-girl` directory into the project-level `Mods` directory. For a built game, copy it into `Mods` beside the game executable.
