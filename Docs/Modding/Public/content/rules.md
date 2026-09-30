# Challenges, difficulties, and rules

## Challenges

Challenge definitions have two forms. An inherited challenge uses `extends` to clone one of the 79 Core challenges, including its exact thresholds, references, and specialized behavior. The new definition supplies its own stable ID, text, and clothing rewards. An optional `stage` moves the inherited challenge to another location.

```json
{
  "schemaVersion": 1,
  "type": "challenge",
  "id": "my.pack:challenge/lamarr-remix",
  "displayName": "Lamarr Remix",
  "description": "Complete the original Lamarr objective.",
  "extends": "core:challenge/lamarr",
  "rewards": ["core:clothing/hair-2"]
}
```

This is the compatibility route for every existing objective, including specialized challenges such as Bitch, Filicide, Insectsest, Lamarr, Nimble Legs, Samus, Stone Lady, and Swollen. Core IDs are listed in `Assets/Resources/Modding/Core/Content/core-challenges.json`.

Fully data-driven challenges instead use `objective` and `count`. Published objectives are:

| Objective | Progress event |
| --- | --- |
| `killCount` | Enemies killed; optional `enemies` filter |
| `weaponKillCount` | Enemies killed with an equipped weapon; optional `enemies` and `items` filters |
| `reachWave` | Highest wave reached |
| `surviveWaves` | Waves completed |
| `flawlessWaves` | Waves completed without taking damage |
| `pickupCount` | Items picked up; optional `items` filter; stack quantity counts |
| `useItemCount` | Usable items consumed; optional `items` filter |
| `interactionCount` | Successful player interactions |
| `shotsFired` | Shots fired |
| `damageTaken` | Damage events received |
| `birthCount` | Births completed |
| `impregnationCount` | Impregnations received; optional parent `enemies` filter |
| `rapeCount` | Rape events started; optional `enemies` filter |
| `orgasmCount` | Orgasms reached; optional current-raper `enemies` filter |
| `mindBreakCount` | Player mind-break/death events |

An optional `stage` makes a challenge location-specific. Without it, the challenge is General. The stable `rewards` list unlocks clothing. The experimental `rewardBundle` can grant current-run currency, stackable/non-stackable items, a weapon, and persistent content IDs; clothing content IDs also unlock in the wardrobe. Filters are allowlists, and omitting a supported filter accepts every matching enemy or item. One enemy list can mix Core and custom IDs. Inheriting a Core challenge clones its original specialized tracking component and serialized behavior. See `ExampleMods/challenge-objectives-showcase` and `Docs/Modding/schemas/challenge.schema.json`.

## Ordered steps

Use experimental `steps` instead of the top-level `objective`, `count`, `enemies`, and `items` fields to create a sequence. Only the current step receives progress. All steps reset when the challenge is activated and must be completed in the same run.

```json
"steps": [
  { "objective": "killCount", "count": 3, "enemies": ["core:enemy/zombie-1"] },
  { "objective": "shotsFired", "count": 10 },
  { "objective": "surviveWaves", "count": 1 }
],
"rewardBundle": {
  "currency": 500,
  "items": [{ "id": "core:item/consumable/ammo-box", "amount": 2 }],
  "weapons": [{ "id": "core:item/weapon/pistol", "amount": 1 }],
  "content": ["core:clothing/hair-2"]
}
```

Challenge progress is tracked during the active run. Completion is persisted by the normal challenge database using a deterministic ID derived from the namespaced content ID.

## Difficulties and game modes

Difficulty profiles expose bounded global multipliers. Rule profiles are selectable game modes. Installed and valid profiles appear under **Options > Game mode**, alongside **Standard**, and the selection is remembered. Only the selected mode runs; disabling or removing its pack safely falls back to Standard.

One rule profile can contain up to 16 module entries across seven reviewed module types:

| Module | Supported rules |
| --- | --- |
| `weaponProgression` | Ordered or random weapons after each kill, starter weapon, inventory replacement |
| `spawnModifiers` | Wave size, concurrent enemy caps, spawner weighting, initial and recurring delays |
| `waveRules` | Enemy growth per wave, intermission duration, optional final wave |
| `economyRules` | Starting/infinite money, bounty multiplier, ammo-drop probability, repeat consumable purchases, outfit repair availability and pricing |
| `playerRules` | Incoming player-damage, health-capacity and outgoing gun-damage multipliers; dynamic all-gun grants; weight/debug options; self-pleasure; and optional nonsexual finisher, clothing-damage and knockout rules |
| `scoringRules` | Points per kill and completed wave, plus a penalty per damage event |
| `goalRules` | Score, kill, and wave victory targets; time and damage-event loss limits; `all` or `any` target matching |

`maximumWaves` stops wave advancement after the final wave and displays a mode-complete notification. A value is never inferred: omitting it retains endless vanilla waves. The [Survival Sprint example](../../../../ExampleMods/survival-sprint-mode/README.md) combines the general modules, while [Apothem Gun Game](../../../../ExampleMods/apothem-gun-game/README.md) demonstrates weapon progression.

The [Captivity Multi-Tool conversion](../../../../ExampleMods/developer-toolkit/README.md) demonstrates a game-mode-gated runtime companion panel. The panel queries the finished registry for weapons and enemies and the stage manager for Core and mod stages, so it does not copy another pack's IDs or depend on fragile stage indices.

`goalRules` provides an explicit match result. Victory targets are evaluated after kills and waves; `goalMatch` defaults to `all`. A time limit or maximum number of damage events ends the run as a loss. Reaching a victory goal stops remaining spawns and displays the final score. During play, the active profile has a persistent Captivity status card showing its score and any configured score, kill, wave, damage, and time targets.

## Experimental player and store mechanics

These mechanics are disabled in Standard mode. A mod can opt into repeat consumable purchases, outfit repair at either vendor, and the self-pleasure action through a selected rule profile:

```json
{
  "type": "economyRules",
  "repeatConsumablePurchases": true,
  "clothingRepairEnabled": true,
  "clothingRepairCostMultiplier": 1.0
},
{
  "type": "playerRules",
  "selfPleasureEnabled": true,
  "selfPleasurePerSecond": 12.0,
  "selfPleasureHeartCost": 0
}
```

When enabled by the selected profile, self-pleasure is available during an active combat wave while grounded. It uses `M`, **View / Share**, or the mobile **Pleasure** button, stops on the same input or when interrupted, and uses the late Squoid player loop. The mobile button is only created while the feature is enabled. A heart cost of `0` allows climax without losing a heart. These fields are experimental for Mod API v1 because their balance and presentation may still change. See `ExampleMods/convenience-mechanics` for a complete pack.

`enemyFinishersEnabled`, `clothingDamageEnabled`, and `safeKnockouts` provide a reviewed nonsexual-mode path without replacing game assemblies. With safe knockouts enabled, reaching zero health consumes a heart and restores health; exhausting the final heart returns the player to the Hub bed. `playerHealthMultiplier` scales both health and the stamina capacity inherited from the health stat. `ExampleMods/captivity-sfw` recreates the legacy CNR behavior with these fields.
