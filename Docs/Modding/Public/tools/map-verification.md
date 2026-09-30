# Map verification

Use this pass before treating the experimental Tiled contract as release-ready. The automated check catches authoring mistakes; the play pass catches Unity physics, navigation, and lifecycle problems.

## Before opening Unity

From `ModSDK/MapTemplates`, run:

```powershell
powershell -ExecutionPolicy Bypass -File .\verify-tiled-pack.ps1
```

To check a copied or third-party pack instead, pass its directory:

```powershell
powershell -ExecutionPolicy Bypass -File .\verify-tiled-pack.ps1 ..\..\Mods\tiled-starter-kit
```

The verifier rejects malformed JSON, absolute or escaping paths, missing templates/tilesets/images/audio, non-finite or non-orthogonal maps, invalid dimensions, missing or duplicate player spawns, and mismatched stage/Tiled spawner IDs.

## Starter arena play pass

Start **Tiled Starter Arena** with only the starter kit and Core enabled, then check:

- The player begins on the floor, not in the air or inside geometry.
- The repeated space-bay background, dirt, grass platforms, lamp animation, poster, barrel, and chair render.
- All solid platforms can be stood on and all marked platform ends can be grabbed.
- Ground enemies traverse the long floor and can pursue across the elevated/platform-link area without navigation errors.
- The standard, Jacky, and roller doors open using their respective switch, keypad, curse, or proximity behavior.
- The note closes using mouse, keyboard, and controller; the keypad code `1234` opens the standard door.
- Both vendors, the weapon case, loose revolver, altar, light, particles, and moving platform behave normally.
- The paid machine enforces its revolver, money, wave, and enemy-count conditions, then performs its linked actions once.
- Ambient audio is global; the machine hum becomes quieter with distance.

## Lifecycle pass

After the object pass:

1. Exit to the location screen and enter the arena again three times.
2. Restart a run from the pause menu.
3. Return to the main menu and reload the save.
4. Confirm that no duplicate map objects, audio sources, navigation nodes, or persistent effects remain.
5. Confirm that the stage high score still saves against its content ID.
6. Review the top-left status alerts for `stage.*` validation errors. Genuine Unity exceptions still appear in the Console.

Repeat the same pass once with all compatible example mods enabled. This catches asset overrides and load-order behavior that an isolated test cannot expose.

## Large-map pass

Duplicate decorations and non-colliding tiles in a temporary copy until it represents a realistically busy map. Test several waves while watching frame time and memory. Do not publish hard object or texture limits until this measurement has been repeated on a low-end target machine.
