# Simple Nerf Gun

Faithful `.capmod` source conversion of @SomeScrub's legacy **Captivity: Nerf Gun** archive.

The original mod replaces the starter pistol's body, slide, and base sprites and makes no gameplay changes. This package therefore patches `core:item/weapon/pistol` in place: it keeps the original damage, ammunition, reload, firing mode, inventory identity, vendor behavior, and save compatibility.

This is intentionally separate from `somescrub.additive-nerf-pistol`. That example adapts the artwork into an additional weapon and demonstrates new weapon behavior; this package preserves the legacy archive's original reskin-only intent.
