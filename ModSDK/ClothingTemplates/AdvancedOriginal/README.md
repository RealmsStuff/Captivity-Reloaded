# Advanced original clothing template

Copy `clothing.json` into a mod content root, change its namespace to the manifest ID, and provide the PNG files named in `visual.sprites`.

Each non-icon `piece/...` key is an arbitrary rig slot created at runtime. Give every piece an attachment entry and choose the nearest player bone. `piece/back` is the conventional name for backpacks, capes, wings, and similar rear art, but custom paths such as `piece/tail`, `piece/armor/pauldron`, or `piece/anatomy/accessory` are accepted.

The base sprite is the fallback for every body. Delete `bodyVariants` when no alternate artwork is needed. A body-variant key can match a loaded body-mod pack ID or the normalized player prefab name.

Sway physics are optional and intended for visual secondary motion. Start with the supplied values, lower `motionInfluence` if movement is too violent, raise `damping` to settle faster, and keep `maxAngle` conservative.
