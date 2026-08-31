# Femboy Refitted Shirt conversion

This disabled example converts the default-shirt artwork from the user-supplied legacy **Captivity: Femboy edition** by Mousai. It extends `core:clothing/shirt-default`, so the original attachment bones, sorting, colliders, tearing behavior, category and compatibility remain unchanged while one PNG atlas replaces the spine, chest and wardrobe icon.

The source mod replaces most player and clothing artwork inside `sharedassets0.assets`; this small conversion is intentionally limited to one outfit while the clothing API is being validated.

To enable it in an editor checkout, copy this entire `femboy-refitted-shirt` directory into the project-level `Mods` directory. For a built game, copy it into `Mods` beside the game executable.
