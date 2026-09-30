# Weapons and items

Weapons may inherit a supported Core template or use the experimental fully original runtime format. Inherited definitions override published statistics, economy fields, sprite slots, and reviewed behavior modules while retaining the prefab. An `originalWeaponSprites` definition omits `extends`; the game constructs its sprite rig, pickup collider, muzzle point, firing presentation, and casing timing directly from JSON without a Core weapon prefab or weapon animation events.

## Fully original weapons

Original weapons support a required `body`, an optional separate `icon`, and up to 16 independently positioned visual parts. Each entry in `visual.parts` may set its own pivot, offset, rotation, scale, and sorting order. The runtime constructs these renderers, the pickup collider, muzzle point, effects, and timed sprite animations without cloning a Core gun prefab. Original weapons must provide complete standalone statistics and currently use the event-free `Magazine` reload type. The held weapon still uses the player's reviewed `weaponType` and `holdType` pose; that is a player-animation contract, not an inherited weapon rig.

```json
{
  "schemaVersion": 1,
  "type": "weapon",
  "id": "my.pack:item/weapon/original-sidearm",
  "displayName": "Original Sidearm",
  "stats": {
    "damage": 5, "ammoMax": 60, "magazineSize": 10,
    "rangeMultiplier": 1, "fireIntervalSeconds": 0.2,
    "reloadSeconds": 1, "equipSeconds": 0.4,
    "weight": 1, "value": 25, "semiAutomatic": true,
    "marketable": true, "holdType": "OneHanded",
    "reloadType": "Magazine", "weaponType": "Pistol"
  },
  "visual": {
    "type": "originalWeaponSprites", "pixelsPerUnit": 32,
    "pivotX": 0.15, "pivotY": 0.5,
    "muzzleOffsetX": 0.8, "muzzleOffsetY": 0.1,
    "colliderWidth": 0.8, "colliderHeight": 0.3,
    "sprites": {
      "body": "assets/weapon/body.png",
      "slide": "assets/weapon/slide.png"
    },
    "parts": {
      "slide": { "offsetX": 0.05, "sortingOrder": 2 }
    }
  }
}
```

## Published gun templates

The first authoring kits are under `ModSDK/WeaponTemplates`:

| Core template | Sprite slots |
| --- | --- |
| `core:item/weapon/pistol` | `body`, `slide`, `magazine`, `base` |
| `core:item/weapon/tenelli-so3` | `body`, `base`, `magazine`, `shell` |
| `core:item/weapon/revolver-44` | `body`, `hammer`, `chamber`, `base`, `bullet`, `bullet-1` through `bullet-5` |

`body` is the inventory/drop icon. Other slots address named renderers inside the inherited prefab. When a prefab has multiple renderers with the same published name, such as the held and dropped Tenelli magazine, one mapping updates all of them.

Weapon PNGs are separate full-canvas images rather than atlas regions. Keep their original dimensions: 32 by 32 for the pistol and revolver, and 48 by 32 for the Tenelli SO3. The LibreSprite generator creates overlapping named layers that can be exported individually.

Common statistics include damage, total and magazine ammunition, bullets per shot, penetration, range, firing interval, recoil, knockback, reload/equip time, weight, value, infinite ammunition, semi-automatic behavior, marketability, hold type, and reload type. Omitted values remain owned by the inherited Core prefab.

## Reviewed weapon behavior

The optional `behavior` object supports `burstCount` with `burstIntervalSeconds`, tracer color/thickness/lifetime, camera shake, shooter knockback, custom WAV/OGG shoot and reload sounds, PNG muzzle flashes, and a PNG casing. Muzzle flashes expose offset, duration, and light intensity. Casings expose shoot/reload ejection rules, offset, force, gravity, angular velocity, and lifetime. All paths are relative to the pack and validated before construction. Casing colliders are calculated from opaque pixels, so a small horizontal shell or dart may safely use a padded 32 by 32 canvas without floating on the transparent border.

`behavior.projectile` selects `hitscan` (the inherited default) or `physical`. A physical projectile requires a PNG `sprite`; it may define `pixelsPerUnit`, `speed`, `gravity`, `lifetimeSeconds`, PNG `impactSprites`, `impactLifetimeSeconds`, `explosionRadius`, and `explosionDamageMultiplier`. Bounds are intentionally conservative. Physical projectiles sweep between frames so fast projectiles do not depend on trigger callbacks. An explosion damages each nearby enemy once.

`behavior.throwable` changes primary fire into hold-to-aim, release-to-throw input using the same aim source on mouse, controller, and mobile. It supports throw speed, gravity, angular velocity, lifetime, repeat-safe impact damage and sounds, plus either an optional timed fuse or `explodeOnImpact` with radial damage, force, sound, and color. `explosionStyle` accepts `burst` or `fire`; bounded ignite duration, damage, and tick-rate fields make impact-fired Molotov variants possible without scripts. The fuse begins when aiming begins. A throwable cannot also declare primary projectile, melee, charge, or beam behavior. Every visual, audio, damage, physics, explosion, and ignition field is pack-authored, so the recovered examples can be copied and reskinned. See [Original Dev Extras](../../../../ExampleMods/original-dev-extras/README.md) for working Rock and Frag Grenade definitions recovered from the v1.0.5bDebug assets.

A Molotov-style behavior can be authored by copying a throwable weapon, replacing its body/icon PNG and sounds, then using:

```json
"throwable": {
  "throwSpeed": 10,
  "gravity": 1,
  "angularVelocity": 240,
  "lifetimeSeconds": 12,
  "impactDamageMultiplier": 0.25,
  "explodeOnImpact": true,
  "explosionRadius": 2.5,
  "explosionDamageMultiplier": 0.5,
  "explosionForce": 2,
  "explosionStyle": "fire",
  "explosionColor": "#FF7A20FF",
  "igniteDurationSeconds": 6,
  "igniteDamagePerTick": 2,
  "igniteTicksPerSecond": 2,
  "impactSounds": ["assets/audio/glass-break.wav"],
  "explosionSounds": ["assets/audio/fire-burst.wav"]
}
```

```json
"behavior": {
  "burstCount": 3,
  "burstIntervalSeconds": 0.08,
  "shootSounds": ["assets/audio/fire.ogg"],
  "muzzleFlashSprites": ["assets/fx/flash.png"],
  "projectile": {
    "mode": "physical",
    "sprite": "assets/projectiles/dart.png",
    "speed": 18,
    "gravity": 1.5,
    "lifetimeSeconds": 4,
    "impactSprites": ["assets/fx/dart-impact.png"],
    "explosionRadius": 0
  }
}
```

### Additional firing modules

`behavior.charge` changes primary ranged fire into hold-and-release fire. Set `seconds`, `minimumDamageMultiplier`, and `maximumDamageMultiplier`. Optional charging/ready text, colors, and `tintStrength` provide an obvious full-charge cue.

`behavior.beam` performs bounded hitscan damage ticks. It supports primary or alternate `input`, duration, tick interval, damage multiplier, ammunition per tick, color, and thickness.

`behavior.melee` performs an overlap attack in the aimed direction without using ammunition. It supports primary or alternate `input`, range, radius, damage multiplier, knockback, and maximum targets. Melee strikes can activate shootable interactables.

`behavior.alternateFire` supports `hitscan`, `projectile`, `melee`, or `beam`, plus damage multiplier, ammunition cost, cooldown, a separate burst count/interval, and a recoil multiplier. Each alternate burst round presents recoil rather than visually appearing as one shot. Alternate fire uses right mouse or the controller's left trigger. Left trigger retains Expose when the equipped weapon has no alternate attack. Mobile canvases can call `PlayerController.TriggerMobileAlternateFire` from a button.

### Ammunition profiles

`behavior.ammunition` gives a weapon a safe data-defined ammunition profile. The `type` label accepts `standard`, `fmj`, `hollow-point`, `incendiary`, `toxic`, `freeze`, or `custom`; actual behavior comes from bounded numeric fields, so labels cannot invoke code. Supported modifiers are base damage, penetration bonus, weak-point damage, damage over time with duration/tick interval, and a temporary movement-speed multiplier. These apply to hitscan, physical projectile, melee, and beam hits.

```json
"ammunition": {
  "type": "incendiary",
  "damageMultiplier": 0.9,
  "penetrationBonus": 1,
  "weakpointMultiplier": 1.25,
  "damageOverTime": 2,
  "effectDurationSeconds": 3,
  "tickIntervalSeconds": 0.5
}
```

This is presently a per-weapon profile. It enables weapons and additive variants with distinct ammunition, but does not yet expose a player-facing magazine/ammunition selector.

Set `playShootSound` or `showMuzzleFlash` to `false` for silent or non-firearm attacks. `visual.hiddenSlots` hides inherited visible prefab pieces, which is useful when adapting a gun rig into a one-piece melee weapon.

### Data-driven weapon animations

`behavior.animations` can remap inherited Animator state names and scale fire/reload playback. Its optional `clips` dictionary defines data-driven sprite animation clips named `idle`, `equip`, `fire`, `alternate`, `charge`, or `reload`. Each clip contains 1–120 timed frames; each frame maps visible published weapon parts to pack-local PNGs. Non-looping clips return to `idle` when it exists, otherwise they restore the weapon's configured sprites.

```json
"animations": {
  "clips": {
    "fire": {
      "frames": [
        { "durationSeconds": 0.05, "sprites": { "slide": "assets/fire-1.png" } },
        { "durationSeconds": 0.08, "sprites": { "slide": "assets/fire-2.png" } }
      ]
    }
  }
}
```

These sprite clips are safe replacements for visual animation. They do not import arbitrary Unity `.anim` files or execute animation-event methods. The [Weapon Behavior Showcase](../../../../ExampleMods/weapon-behavior-showcase/README.md) supplies charge, beam, hitscan alternate-fire, melee, and sprite-animation test weapons.

## Reviewed usable effects

Usables may inherit a Core item with `coreUsableSprites`, or omit `extends` and use `originalUsableSprites`. A fully original usable supplies its own icon/world PNG, pivot, collider size, and sorting order and must use `effectMode: "replace"`; it does not clone a Core usable prefab or execute its effects.

To reuse a vanilla item's specialized implementation, set `extends` to its `core:item/usable/...` ID and leave `effectMode` as `inherit` (the default). The resulting item retains the Core prefab's presentation, audio, and `HandleUse` behavior. `add` keeps that behavior and appends reviewed JSON effects; `replace` suppresses it. Mods cannot name arbitrary C# classes or methods, so a mechanic with no Core template and no reviewed effect module still needs a new safe runtime module.

The safe effect list currently includes `restoreHealth`, `restoreStrength`, `restoreStamina`, `reducePleasure`, timed `ragdoll`, timed `invulnerability`, bounded `statModifier`, `refillAmmo` (adds `amount` to every carried gun), `fillAllAmmo`, `repairClothing`, and `gainMoney`. Effects execute in JSON order. This provides composable medicine, ammunition, repair, and reward items while intentionally excluding arbitrary method or script names.
