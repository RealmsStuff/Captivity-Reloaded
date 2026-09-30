# Core migration status

This is the working boundary between loose Core data and compatibility adapters. A content ID or JSON metadata record does not, by itself, count as removal of a prefab dependency.

| Content | Core entries | Loose data now | Compatibility dependency still present |
| --- | ---: | --- | --- |
| Difficulties | 3 | Complete definitions and runtime registry | None |
| Weapons | 22 | Identity, metadata, economy, firing statistics | Rig, sprites, audio, projectiles, colliders, reload objects, and specialized mechanics |
| Enemies | 21 | Identity, base statistics, vision/wave flags, normalized rig references, and up to 244 public semantic animation aliases | Vanilla Animator playback, attacks, drops, visual construction, colliders, interactions, and specialized AI |
| Clothing | 98 | Identity, category, packaged artwork, icons, complete piece attachments, hat behavior, tearing connections, and compatibility lists | Existing garment objects remain as per-entry compatibility fallbacks while reconstructed wardrobe parity is verified |
| Challenges | 79 | Identity and objective-adapter classification; inheritable original tracking components; general objectives with mixed Core/custom enemy allowlists | Specialized components still retain their original serialized references rather than exposing arbitrary retargeting |
| Stages | 7 | Identity, metadata, first-wave count, plus the FER Tiled port's layout, encounters, interactions, doors, lights, scripted actors and stable Core prop IDs | The seven Core entries remain prefab-backed; FER retains its root stage script and 28 exact animated/particle presentation hierarchies through the Core prop catalog |
| Items | 11 | Identity, economy and presentation text; reviewed effects for Aspirin, Morphine, and Hyper | Artwork, audio, animations and the remaining specialized usable behavior |

## Current migration order

1. Export and package every Core enemy semantic animation, then validate ownership from each `coreEnemy.animationReferences` map.
2. Run **Export Core Clothing Presentation** to crop and package every registered garment sprite and record its full attachment, tearing, sorting and compatibility metadata. Compare those 98 definitions before replacing any wardrobe object.
3. Export weapon rigs, effects and audio, and compare original-weapon runtime behavior against each vanilla gun.
4. Continue replacing genuinely general challenge adapters with `ModEventChallenge`; retain specialized objectives whose serialized behavior is not safely expressible as data.
5. Run FER side-by-side Play Mode parity checks, then migrate ordinary usables and additional stage layouts/machines.

The adapter is removed for a category only after old saves, UI selection, gameplay behavior and representative scenes pass side-by-side parity tests.

## Required Unity migration command

Run **Captivity Reloaded > Modding > Export All Core Enemy Animations** after scripts reload. This creates the packaged animation directory and updates all 21 Core enemy documents with their exported semantic references. The command is intentionally editor-only because it reads Unity animation clips and prefab hierarchies.

Then run **Captivity Reloaded > Modding > Export Core Clothing Presentation**. It matches the 93 reusable clothing prefabs and the five scene-only garments by legacy ID, packages cropped sprites under `Resources/Modding/Core/Clothing`, and augments `core-clothing.json`. Runtime validation checks every generated Resource path. This is the lossless capture phase; the wardrobe continues using its compatibility objects until visual and tearing parity is verified.
