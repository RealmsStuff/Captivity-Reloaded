# Prey Green Zombie conversion

This is a data-only enemy variant recovered from the user-supplied legacy **Prey Mod (Bunny Girls)** by `@Draco66Electro`. It demonstrates the first `coreRigAtlas` workflow: one PNG atlas and one enemy JSON file extend `core:enemy/zombie-1` while reusing its AI, animation rig, colliders and gameplay behaviour.

The source overhaul replaced assets inside Unity's `sharedassets0.assets`. The pieces in this atlas were matched to the existing zombie rig using their original sprite pivots and then packed into one portable texture. This example does not include or execute legacy code.

Example packs are disabled by default. To enable this one in an editor checkout, copy this entire `prey-green-zombie` directory into the project-level `Mods` directory. For a built game, copy it into `Mods` beside the game executable.
