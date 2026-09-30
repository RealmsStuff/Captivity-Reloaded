# Captivity Multi-Tool parity audit

Status date: 2026-09-29

The archived Multi-Tool `Assembly-CSharp.dll` was decompiled and compared directly with the archived C4C DLL. Only four classes differ: `Gun`, `ManagerWave`, `Player`, and `ScreenGame`. This confirms that Multi-Tool inherits C4C and adds one immediate-mode cheat panel rather than a separate general-purpose plugin framework.

## Recovered controls

- Add $10,000.
- Complete all challenges.
- Unlock all clothing.
- Add one heart, capped at five.
- Add or remove libido.
- Add or remove pleasure.
- Damage or heal the player.
- Damage strength.
- Toggle infinite strength and stamina.
- Trigger one orgasm or five orgasms (mind break).
- Toggle infinite ammunition and refill the active magazine continuously for no-reload mode.
- Choose and grant a weapon from the shipped gun library.
- Change the wave intermission in 30-second increments.
- Spawn any shipped actor normally, pregnant, permanently ragdolled, or with thinking disabled.
- Apply or remove the original hypnosis/expose state.
- Switch directly to Hub, Shack, Cave, Jungle, Space Station, or FER.

The original panel is desktop-only IMGUI, assumes a gun is equipped for no-reload, uses hard-coded Core stage indices, and performs save-mutating unlock actions without confirmation. Those unsafe assumptions should not be copied literally.

## Current conversion coverage

`ExampleMods/developer-toolkit` now preserves the C4C-derived baseline and exposes every recovered control family through `DeveloperToolkitPanel`. The earlier placeholder's unrelated always-on infinite money, damage immunity, weight bypass, automatic arsenal grant, and debug-hotkey behavior was removed. The package now uses the release ID `legacy.captivity-multi-tool`.

## Implementation boundary

The compatibility version should use a dedicated developer panel, registered content IDs, and the selected rule profile as its activation gate. Runtime-only actions can be direct. Save-mutating actions (complete challenges and unlock clothing) should require an explicit confirmation. Actor and stage lists should include enabled mod content and reject incompatible entries instead of casting every library actor to `NPC` as the original did.

Implemented on 2026-09-29. The panel is responsive and scrollable, has keyboard and on-screen entry points, resets continuous options when its profile is deselected, and compiled cleanly with the Unity project assembly. It still requires an isolated in-game pass before publication.
