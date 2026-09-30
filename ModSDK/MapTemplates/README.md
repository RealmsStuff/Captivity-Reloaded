# Map templates

`TiledStage` is the first no-Unity map-authoring kit. It creates collision platforms, the player start, and enemy-spawner positions from a finite orthogonal Tiled JSON map while inheriting the remaining presentation and gameplay objects from a Core stage.

This is the collision-layout milestone. Tiled tile rendering, custom backgrounds, navigation links, ambience, and interactive props are not loaded yet. Keeping those separate prevents authors from building against guessed fields that later become incompatible.
