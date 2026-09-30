# Original animation workflow findings

The recovered project does not contain a Blender, Spine, DragonBones, Spriter, Flash, Photoshop, Krita, or Aseprite source document for the character animations. It therefore cannot prove the exact drawing application used for the body-part sprites. It does preserve strong evidence for how the animations themselves were assembled.

## Evidence

- The project contains 478 native Unity `.anim` clips and 57 Unity Animator Controller assets.
- 472 clips use a 60 FPS sample rate, five use 90 FPS, and one uses 24 FPS.
- The clips key Unity hierarchy paths such as `SkeletonPlayer/hips/spine/chest/lArmUpper` and animate local position, Euler rotation, and scale curves.
- A small number of clips also use Unity object-reference curves to swap `SpriteRenderer.m_Sprite`, for example facial artwork in `Dead_2.anim`.
- 166 clips contain Unity Animation Events. Recovered event names include `Thrust`, `CumThrust`, `AnimEventPerformAttack`, audio calls, particle calls, weapon-drop calls, and enemy-specific actions.
- Character prefabs are articulated GameObject hierarchies made from `Bone`, `SpriteRenderer`, `Rigidbody2D`, and `HingeJoint2D` components. The joints are used for ragdolls; the Animator normally drives the same transforms.
- `RaperAnimation` pairs one enemy `AnimationClip` with one player `AnimationClip`, then stores loop count/duration plus synchronized audio, particles, attachment, and gameplay metadata.
- The repository contains an AssetRipper import patch and recovered/decompiled scripts. Some editor-only provenance was therefore necessarily lost when the release build was reconstructed.

## Most likely process

Perveloper most likely cut characters into separate sprite body parts, assembled those parts into a Unity GameObject bone hierarchy, and authored the motion directly in Unity's Animation window. The resulting clips were organized into Animator Controlle rs, while Unity Animation Events and `RaperAnimation` components synchronized attacks, paired scenes, sound, particles, and gameplay callbacks.

This is an inference from the serialized assets, not a surviving statement from the developer. External art software was probably used to draw and cut the sprites, but no surviving source files identify which one. The mod Design Studio intentionally mirrors the recoverable part of that workflow: hierarchical sprite posing, transform keyframes, sprite swaps, an event timeline, and paired runtime metadata, while saving portable JSON instead of Unity-only `.anim` files.
