# Advanced Clothing Showcase

This unlocked test pack exercises Mod API v1 advanced clothing support without inheriting a Core garment prefab.

- **Swaying Wing Harness** uses `piece/back`, attaches to `Spine`, and enables bounded visual sway. If the Shaded Girl pack is loaded, it also demonstrates a larger body-variant sprite.
- **Persistent Anatomy Attachment** uses `playerAttachment`, attaches directly to `Hips`, and is present whenever the pack is enabled without occupying a wardrobe category.

The wing is an original garment render rig. The anatomy example is a persistent player-rig extension. Neither creates a new animated bone; both anchor artwork to an existing semantic player bone. Physics currently means safe procedural visual sway, not Unity joints or collision.
