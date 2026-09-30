# Normalized player rig and animation format

The normalized format separates author-facing names from Unity hierarchy paths. Run **Captivity Reloaded > Modding > Export Normalized Player Reference** to create `Normalized/player-rig.json` plus representative locomotion, stance, weapon, item-use, birth, and paired-finisher clips. The finisher selection covers ordinary, attached, head-mounted, full-body displacement, complex sorting, and persistent-pose cases rather than exporting hundreds of near-duplicate phases.

## Frozen names and coordinates

Schema version 1 uses world units, positive Y upward, degrees for rotation, and the same negative-clockwise convention produced by Unity's 2D animation curves. The stable player bones are:

`hips`, `butt`, `spine`, `chest`, `neck`, `head`, `arm-right-upper`, `arm-right-lower`, `hand-right`, `arm-left-upper`, `arm-left-lower`, `hand-left`, `leg-right-upper`, `leg-right-lower`, `foot-right`, `leg-left-upper`, `leg-left-lower`, `foot-left`, `ear`, and `face`.

Animation targets use `bone/<name>` for transforms and `sprite/<name>` for body-part rendering. Stable numeric tracks cover position, rotation, and scale. Stable clip metadata covers identity, rig, duration, frame rate, and looping.

The exported `clothingContract` freezes the published `icon` and `piece/...` slot inventory, clothing categories, normalized bottom-left pivots, unrestricted rectangular sprite canvases, absolute sorting range, attachment sorting-offset range, and symmetric category compatibility. Body variants resolve in this order: explicit `ModBodyVariant`, player prefab name, loaded body-pack ID, then the baseline `visual.sprites` artwork. Variant discovery remains experimental while those body-pack identity conventions are tested.

The normalized enemy rig and animation shapes are frozen at schema version 1. Bone transforms, sprite swaps, color/sorting tracks, ordered safe events, cues, and bounded particle tracks must remain backward compatible; incompatible changes require a future schema version. Raw Unity source metadata and warnings remain diagnostic. Player-side stability is classified independently through `x-stability` in `player-rig.schema.json` and `player-animation.schema.json`.

External `playerAnimation` documents are discoverable runtime content and may be reused by an enemy finisher through `playerAnimationRef`. Runtime v1 imports the stable `bone/*` position, rotation, and scale tracks for `core:player-rig/alex`. It evaluates linear/Hermite curves, preserves authored key times up to the 256-frame finisher limit, and resamples larger curves to that bound. The safe player events are `pleasure`, `scaledPleasure`, `libido`, `strengthDamage`, `struggleDamage`, and `healthDamage`; normalized events store their positive amount in `floatValue`. Experimental `sprite/*` color and sorting tracks, pack-local PNG sprite keys, and bounded particle triggers are also applied and restored after playback.

External `enemyAnimation` documents target one fully original enemy and are assigned semantic clip names through that enemy's `animationRefs`. Their bone, sprite, and effect targets are checked against the declared skeleton. Sprite object keys select declared atlas regions by `name`, or load a safe pack-local PNG when `asset` is supplied. Color, sorting, timed safe events, and bounded particle triggers use the same normalized presentation model as player clips. This lets a large animation set be split into reusable, independently validated files instead of embedding every frame under `animation.clips`.

Enemy clips use `root` for bindings authored directly on the sampled actor root. Humanoid limb spellings such as `rArmUpper` and `armUpperR` both normalize to `arm-right-upper`; genuinely different limbs on non-humanoid rigs retain their descriptive slugged names.

The preview's Core enemies mode exports selected Unity clips into `NormalizedEnemies/<enemy>/<semantic-name>.json` and writes the corresponding `rig.json`. The rig reference includes semantic-to-Unity paths, hierarchy parents, default local transforms, sprite asset regions, pivots, canvas dimensions, and render ordering. Use the source toggle in the preview to compare the original Unity clip and normalized JSON at an identical timeline position. Supported bindings are converted immediately; unsupported Unity animation events are listed in `warnings` and retain their original clip in `source` for manual review. These Core documents are authoring references because their varied legacy skeletons have not yet all been frozen as public runtime rigs.

Finisher exports also contain bounded `effects` and `effectTriggers`. Effects record their player/enemy bone attachment, local position, rotation and scale, simulation space, particle lifetime, speed, size, color, gravity, emission rate, shape, texture-sheet sprites, capacity, material, texture or mesh, and render order. Triggers translate the enemy-side `Thrust`, `CumThrust`, and indexed `PlayParticleUnique` animation events into explicit timestamps and effect IDs. The original prefab and hierarchy path remain as audit metadata.

The Unity preview replays those same effect triggers while playing, scrubbing, or stepping frames. Its **VFX** toggle isolates animation/sorting review from particle presentation, and the timeline label identifies a trigger near the current playhead. Previewing never invokes the original gameplay animation-event methods.

The exporters deliberately ignore bindings that belong to the other side of a paired finisher. This prevents an enemy hierarchy accidentally embedded in an old player-side clip from becoming part of the normalized player contract, and prevents embedded `SkeletonPlayer` tracks from polluting a Core enemy rig.

Before writing a clip, the exporter audits every sorting binding. If both a raw `SpriteRenderer` order and the effective `BodyPartPlayer` sorting-group order resolve to the same normalized target, the effective body-part track wins. Unresolved, duplicate, or out-of-range sorting tracks are included in the document's experimental `warnings` list and reported in Unity's Console.
