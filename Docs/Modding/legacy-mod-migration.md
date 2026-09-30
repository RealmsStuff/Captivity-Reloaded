# Legacy mod migration notes

The example mods in `Mods to possible features` modify complete Unity asset archives or `Assembly-CSharp.dll`. Those delivery methods are tied to one game build and cannot be loaded directly by Mod API v1. Their intended behavior should instead map to stable content IDs, asset slots, and supported rule modules.

## Simple Nerf Gun

The legacy mod replaces three textures on the starter pistol: `pistol`, `slide`, and `base`.

- Content target: `core:item/weapon/pistol`
- Planned asset slots: `body`, `slide`, and `base`
- Migration form: an explicit asset patch containing replacement PNG files

The patch must not replace `sharedassets0.assets` or affect the internal developer pistol that shares the display name `Pistol`.

## Shaded Girl

The legacy mod replaces player body-part textures inside `sharedassets0.assets`. The supported migration should expose named player body slots such as head, chest, hips, arms, hands, legs, and feet. A pack will replace only the slots it declares.

Generic Unity texture names such as `Head` or `Hand` are not public identifiers because several actors and skin variants reuse them.

## Gun Game

The legacy DLL hooks a player kill, removes held weapons, and grants a random weapon from a fixed list. The supported migration needs a rule profile with:

- an `enemyKilled` trigger;
- an inventory action that removes or replaces weapons;
- a content-ID weapon pool;
- selection modes such as random or ordered progression;
- a declared starter weapon.

This behavior should reference registered weapon IDs rather than display-name strings.

## Extended Difficulties

The legacy DLL adds Very Hard and Nightmare alongside the existing profiles. It changes enemy maximum health, damage received by the player, and escape strength.

The supported migration needs data-defined difficulty profiles with explicit multipliers for those three systems. The options menu should enumerate registered profiles rather than compile an enum into `Assembly-CSharp.dll`.
