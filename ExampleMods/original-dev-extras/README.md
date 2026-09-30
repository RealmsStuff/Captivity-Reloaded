# Original Developer Extras

This optional pack restores prototype content recovered from the original developer's `v1.0.5bDebug` build without replacing `Assembly-CSharp.dll`.

The first restoration slice adds the Rock and Frag Grenade as marketable Special weapons. Hold Fire to aim and release Fire to throw. This uses Reloaded's shared aim and button-release paths, so mouse, controller, and mobile controls behave consistently. Grenade fuses begin when aiming starts, matching the recovered implementation.

The recovered 16-by-16 weapon sprites and WAV files are included unchanged. Gameplay code was cleanly reimplemented through Mod API v1 because the prototype directly modified the original player, inventory, HUD, and weapon classes. Reloading represents taking the next throwable from the carried reserve; recoverable ground pickups remain planned because the original stack-splitting drop implementation was incomplete and invasive.

The pregnancy-belly attachment restores the Debug build's four original 32-by-32 skin sprites, five fetus-count transforms, 1.5-second growth animation, and delayed tearing of Spine clothing. Reloaded's additional Olive, Brown, and Deep skin tones are generated from the authored sprite at runtime. The zero-fetus stage is hidden; stages one through four grow with each fetus, and counts above four use the final stage.

The Brothel prototype is restored as an editable Tiled stage. Its eight source-art sprites, room silhouette, platform placement, background layering, and original Rock pickup are preserved. The recovered object was disabled with ID `-1`, zero wave spawns, no audio, and incomplete collision widths; the port repairs collision to match the visible architecture and adds two conservative spawners so the stage can actually run. No invented story events or room decorations were added.
