# Captivity Reloaded Modding

This is the public authoring guide for Captivity Reloaded's data-driven mod system.

The current Mod API is an early v1 draft. It supports data-only packs, stable content IDs, dependencies and load ordering, asset patches, inherited enemies and clothing, weapons and usables, difficulties, rule profiles, challenges, and early stage formats. Some content still inherits Unity prefabs while Core migration continues.

## Start here

1. Read [Installing mods](getting-started/installing-mods.md).
2. Build [your first pack](getting-started/first-pack.md).
3. Choose a supported [content type](content/README.md).
4. Validate the pack in the in-game Mods panel before sharing it.

The repository's `ExampleMods` directory contains runnable examples. The `ModSDK` directory contains LibreSprite weapon, clothing, and inherited Zombie I templates plus the experimental Tiled map kit.

{% hint style="warning" %}
The public schema is not frozen yet. Pages and fields marked **Experimental** may change before Mod API v1 is declared stable.
{% endhint %}

Development status and release readiness are tracked separately in [Implementation status](reference/status.md) and the [Mod API v1 release checklist](reference/v1-release-checklist.md).
