# Additive Nerf Pistol example

This disabled example converts @SomeScrub's legacy starter-pistol artwork into a separate data-driven weapon. It extends `core:item/weapon/pistol`, keeps the inherited rig and reload animation, and demonstrates a three-round burst, cyan tracer, custom muzzle flash, and custom ejected casing. The original Core pistol remains available.

Copy this entire directory into the project-level `Mods` directory, or into `Mods` beside a built game, to enable it.

This is the additive version, not the starter-pistol replacement. It is marked marketable and can therefore appear randomly in weapon-vendor stock when the player does not already own it. It is not automatically granted or placed in a stage.

## Unity-authored package example

This mod is also the first `.capmod` AssetBundle authoring example. Its Unity prefab and ready-made authoring profile live under `Assets/ModAuthoring/Examples/AdditiveNerfPistol`. Open **Captivity Reloaded > Modding > Captivity Mod Packager**, assign `AdditiveNerfPistolProfile`, and choose **Build Unity .capmod**. The JSON weapon definition remains the gameplay/fallback layer while the bundle contains the Unity-imported presentation assets.
