# Weapon Behavior Showcase

This test pack adds four marketable weapons. Charged Pistol fires when primary fire is released, Beam Pistol demonstrates bounded damage ticks plus a secondary hitscan pulse, Combat Knife demonstrates a primary melee attack without a thrown alternate attack, and Original Sidearm demonstrates a weapon created without a Core weapon prefab or Animator events.

Original Sidearm is sold by the weapon vendor. Test aiming in both directions, firing, reloading, dropping, and picking it back up. Its runtime rig, collider, muzzle point, statistics, and timed sprite animation are built from JSON and PNG files.

Original Sidearm tests layered artwork, FMJ penetration, and alternate-fire burst recoil; Charged Pistol tests visible charge feedback and incendiary damage over time; Beam Pistol tests beam/alternate fire and temporary freezing. These development weapons remain isolated from the faithful Apothem Gun Game conversion.

Alternate fire is right mouse on keyboard/mouse and left trigger on controller. The mobile runtime exposes `TriggerMobileAlternateFire` for an alternate-fire HUD button. The fire animation deliberately substitutes a visibly different slide frame so authors can confirm the data-driven sprite clip is running.
