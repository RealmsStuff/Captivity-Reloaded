# Validation and error-code catalog

This searchable catalog is generated from literal validation calls in the runtime, editor tools, and repository validators. Use your browser search for an exact code shown by the Mods panel or authoring tool.

Regenerate it with `Tools/Documentation/Generate-ValidationCatalog.ps1`. Codes assembled dynamically at runtime may not appear here; the displayed message and source remain authoritative.

## Severity

- **Error:** the document, operation, or pack cannot proceed safely.
- **Warning:** content may load with a fallback or requires author attention.
- **Info:** state or platform information that does not itself invalidate content.

## `adapter.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `adapter.core-missing` | Warning | Core  | `Assets/Scripts/Assembly-CSharp/CoreContentAdapter.cs` |

## `asset-patch.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `asset-patch.asset-path` | Error | Replacement must reference a safe relative PNG path:  | `Assets/Scripts/Modding/AssetPatchDefinition.cs` |
| `asset-patch.content-root` | Warning | Content root does not exist or escapes its pack. | `Assets/Scripts/Modding/AssetPatchDefinition.cs` |
| `asset-patch.definition-size` | Error | Content JSON must be between 1 byte and 1 MiB. | `Assets/Scripts/Modding/AssetPatchDefinition.cs` |
| `asset-patch.id` | Error | Patch ID must use the defining pack namespace and a patch/ path. | `Assets/Scripts/Modding/AssetPatchDefinition.cs` |
| `asset-patch.namespace` | Error | Patch ID namespace must match its defining pack:  | `Assets/Scripts/Modding/AssetSlotRegistry.cs` |
| `asset-patch.null` | Error | Asset patch resolved to null. | `Assets/Scripts/Modding/AssetPatchDefinition.cs` |
| `asset-patch.override` | Warning | Pack ' | `Assets/Scripts/Modding/AssetSlotRegistry.cs` |
| `asset-patch.pack` | Error | Defining pack is not loaded:  | `Assets/Scripts/Modding/RuntimeSpritePatchLoader.cs` |
| `asset-patch.replacements` | Error | At least one replacement is required. | `Assets/Scripts/Modding/AssetPatchDefinition.cs` |
| `asset-patch.runtime-type` | Error | PNG replacement requires a Sprite slot:  | `Assets/Scripts/Modding/RuntimeSpritePatchLoader.cs` |
| `asset-patch.schema-version` | Error | Unsupported schemaVersion  | `Assets/Scripts/Modding/AssetPatchDefinition.cs` |
| `asset-patch.slot` | Error | Replacement key is not a safe slot path:  | `Assets/Scripts/Modding/AssetPatchDefinition.cs` |
| `asset-patch.target` | Error | Patch targets an unknown public asset slot:  | `Assets/Scripts/Modding/AssetSlotRegistry.cs, Assets/Scripts/Modding/RuntimeSpritePatchLoader.cs` |
| `asset-patch.target-id` | Error | Patch target is not a valid public ID. | `Assets/Scripts/Modding/AssetPatchDefinition.cs` |
| `asset-patch.type` | Error | Replacement does not match slot asset type  | `Assets/Scripts/Modding/AssetSlotRegistry.cs` |
| `asset-patch.type-name` | Error | Definition type must be 'assetPatch'. | `Assets/Scripts/Modding/AssetPatchDefinition.cs` |

## `asset-slot.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `asset-slot.baseline` | Error | Asset slot requires a baseline asset:  | `Assets/Scripts/Modding/AssetSlotRegistry.cs` |
| `asset-slot.duplicate` | Error | Asset slot is already registered:  | `Assets/Scripts/Modding/AssetSlotRegistry.cs` |
| `asset-slot.enemy-animation-sprite` | Error | Core enemy animation sprite was not loaded for public slot  | `Assets/Scripts/Assembly-CSharp/CoreAssetSlotBinder.cs` |
| `asset-slot.enemy-renderer` | Error | Core enemy renderer was not found for public slot  | `Assets/Scripts/Assembly-CSharp/CoreAssetSlotBinder.cs` |
| `asset-slot.namespace` | Error | Asset slot and owner namespaces must match their defining pack:  | `Assets/Scripts/Modding/AssetSlotRegistry.cs` |
| `asset-slot.null` | Error | Cannot register a null asset slot. | `Assets/Scripts/Modding/AssetSlotRegistry.cs` |

## `bootstrap.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `bootstrap.catalog-missing` | Error | Packaged Core content catalog could not be loaded. | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `bootstrap.core-missing` | Error | Packaged Core manifest could not be loaded. | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `bootstrap.external-disabled` | Info | External mod discovery is unavailable on this platform. | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |

## `bundle.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `bundle.asset` | Error | Every entry must be a unique prefab asset below Assets/. | `Assets/Editor/Modding/CaptivityUnityBundlePackager.cs` |
| `bundle.asset-null` | Error | The prefab list contains an empty entry. | `Assets/Editor/Modding/CaptivityUnityBundlePackager.cs` |
| `bundle.assets` | Error | Assign at least one prefab to the authoring profile. | `Assets/Editor/Modding/CaptivityUnityBundlePackager.cs` |
| `bundle.build` | Error | Unity failed to build the  | `Assets/Editor/Modding/CaptivityUnityBundlePackager.cs` |
| `bundle.descriptor` | Error | Prefab root requires CapmodPrefabDescriptor:  | `Assets/Editor/Modding/CaptivityUnityBundlePackager.cs` |
| `bundle.generated-metadata` | Error | Remove assetBundles from the source manifest; the packager generates it. | `Assets/Editor/Modding/CaptivityUnityBundlePackager.cs` |
| `bundle.platform-module` | Error | Unity support for  | `Assets/Editor/Modding/CaptivityUnityBundlePackager.cs` |
| `bundle.platforms` | Error | Select at least one bundle platform. | `Assets/Editor/Modding/CaptivityUnityBundlePackager.cs` |
| `bundle.prefab-id` | Error | Prefab IDs must be unique and use  | `Assets/Editor/Modding/CaptivityUnityBundlePackager.cs` |
| `bundle.script` | Error | The first prefab slice only permits CapmodPrefabDescriptor; unsupported script:  | `Assets/Editor/Modding/CaptivityUnityBundlePackager.cs` |

## `catalog.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `catalog.cache-write` | Error | Catalog cache path is missing. | `Assets/Scripts/Modding/ModCatalog.cs` |
| `catalog.category` | Error | Core adapter category must be Enemy, Stage, Clothing, Item, or Challenge:  | `Assets/Scripts/Modding/CoreContentCatalog.cs` |
| `catalog.duplicate-id` | Error | Duplicate Core content ID:  | `Assets/Scripts/Modding/CoreContentCatalog.cs` |
| `catalog.duplicate-legacy-selector` | Error | Duplicate  | `Assets/Scripts/Modding/CoreContentCatalog.cs` |
| `catalog.empty` | Error | Core content catalog JSON is empty. | `Assets/Scripts/Modding/CoreContentCatalog.cs` |
| `catalog.entries` | Error | The Core content catalog requires an entries array. | `Assets/Scripts/Modding/CoreContentCatalog.cs` |
| `catalog.id` | Error | Every entry must have a valid ID in the 'core' namespace. | `Assets/Scripts/Modding/CoreContentCatalog.cs` |
| `catalog.legacy-id` | Error | Legacy IDs must be non-negative:  | `Assets/Scripts/Modding/CoreContentCatalog.cs` |
| `catalog.legacy-name` | Error | Legacy names cannot have leading or trailing whitespace:  | `Assets/Scripts/Modding/CoreContentCatalog.cs` |
| `catalog.legacy-selector` | Error | Every Core entry must declare exactly one of legacyId or legacyName:  | `Assets/Scripts/Modding/CoreContentCatalog.cs` |
| `catalog.null` | Error | Core content catalog resolved to null. | `Assets/Scripts/Modding/CoreContentCatalog.cs` |
| `catalog.schema-version` | Error | Unsupported schemaVersion  | `Assets/Scripts/Modding/CoreContentCatalog.cs` |

## `challenge.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `challenge.enemy-missing` | Error | Unknown challenge enemy:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `challenge.extends-missing` | Error | Unknown inherited challenge:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `challenge.id-collision` | Error | External challenge runtime ID collision for  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `challenge.item-missing` | Error | Unknown challenge item:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `challenge.null` | Error | Challenge definition resolved to null. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `challenge.reward-content-missing` | Error | Unknown content reward:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `challenge.reward-item-missing` | Error | Unknown challenge item reward:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `challenge.reward-missing` | Error | Unknown challenge reward:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `challenge.reward-not-bound` | Error | Challenge reward is not a bound  | `Assets/Scripts/Assembly-CSharp/ManagerChallenge.cs` |
| `challenge.reward-weapon-missing` | Error | Unknown challenge weapon reward:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `challenge.stage-missing` | Error | Unknown challenge stage:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `challenge.step-enemy-missing` | Error | Unknown challenge step enemy:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `challenge.step-item-missing` | Error | Unknown challenge step item:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |

## `clothing.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `clothing.effects.bounty` | Error | bountyMultiplier must be between 0 and 3. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.effects.damage-taken` | Error | damageTakenMultiplier must be between 0.05 and 2. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.effects.escape-power` | Error | escapePowerMultiplier must be between 0.1 and 3. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.effects.stat` | Error | statModifiers contains an unsupported stat or value:  | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.effects.stat-count` | Error | statModifiers supports at most 8 entries. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.extends-missing` | Error | Clothing extends unknown Core clothing:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `clothing.null` | Error | Clothing definition resolved to null. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.physics.mode` | Error | physics.mode must be sway. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.physics.range` | Error | Clothing sway values are outside supported bounds. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual` | Error | visual is required. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.atlas` | Error | atlas must be a safe pack-relative PNG path. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.attachment` | Error | attachments must target a published clothing piece slot. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.attachment-bone` | Error | Unknown player bone:  | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.attachment-range` | Error | Attachment offsets, pivot, rotation, or sortingOffset are outside supported bounds. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.body-variant` | Error | Body variants need a safe name and 1 through 32 sprite overrides. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.body-variant-sprite` | Error | Body variants may replace existing non-icon piece slots with safe PNG paths:  | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.original-attachment` | Error | Every original clothing piece requires an attachment definition:  | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.original-attachment-bone` | Error | Every original clothing piece must name its player bone:  | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.original-attachment-slot` | Error | Original attachment definitions must match a supplied piece sprite:  | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.original-icon` | Error | Fully original clothing requires an icon sprite. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.original-piece-count` | Error | Fully original clothing supports an icon and at most 32 pieces. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.pixels-per-unit` | Error | pixelsPerUnit must be greater than 0 and at most 1024. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.region-name` | Error | Region is not a published V1 clothing slot:  | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.region-rect` | Error | Region rectangle must have a non-negative origin and positive size:  | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.regions` | Error | At least one atlas region is required. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.sprite-name` | Error | Sprite is not a published V1 clothing slot:  | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.sprite-path` | Error | Sprite paths must be safe pack-relative PNG paths. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.sprites` | Error | At least one sprite mapping is required. | `Assets/Scripts/Modding/ClothingDefinition.cs` |
| `clothing.visual.type` | Error | visual.type must be coreClothingAtlas, coreClothingSprites, originalClothingSprites, or originalClothingAtlas. | `Assets/Scripts/Modding/ClothingDefinition.cs` |

## `content.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `content.definition-limit` | Error | Pack contains more than  | `Assets/Scripts/Modding/ModContentDiscovery.cs` |
| `content.definition-size` | Error | Content JSON must be between 1 byte and 1 MiB. | `Assets/Scripts/Modding/ModContentDiscovery.cs` |
| `content.root` | Warning | Content root does not exist or escapes its pack. | `Assets/Scripts/Modding/ModContentDiscovery.cs` |
| `content.type` | Error | Unsupported or missing content definition type:  | `Assets/Scripts/Modding/ModContentDiscovery.cs` |

## `core-challenge.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `core-challenge.catalog-missing` | Error | Core challenge is absent from the legacy adapter catalog:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-challenge.definition-missing` | Error | Core challenge has no packaged definition:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-challenge.legacy-id` | Error | Core challenge legacyId does not match the adapter catalog:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |

## `core-clothing.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `core-clothing.catalog-missing` | Error | Core clothing is absent from the legacy adapter catalog:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-clothing.definition-missing` | Error | Core clothing has no packaged definition:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-clothing.icon-missing` | Error | Migrated Core clothing icon is not loadable:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-clothing.legacy-id` | Error | Core clothing legacyId does not match the adapter catalog:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-clothing.sprite-missing` | Error | Migrated Core clothing piece is not loadable:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |

## `core-content.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `core-content.type` | Error | Unsupported packaged Core definition type:  | `Assets/Scripts/Modding/ModContentDiscovery.cs` |

## `core-enemy.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `core-enemy.animation-ref-missing` | Error | Unknown normalized Core enemy animation:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-enemy.animation-ref-owner` | Error | Animation  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-enemy.catalog-missing` | Error | Core enemy is absent from the legacy adapter catalog:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-enemy.duplicate` | Error | Duplicate packaged Core enemy definition:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-enemy.legacy-duplicate` | Error | Duplicate Core enemy legacyId:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-enemy.legacy-id` | Error | Core enemy legacyId does not match the adapter catalog:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |

## `core-item.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `core-item.catalog-missing` | Error | Core item is absent from the legacy adapter catalog:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-item.duplicate` | Error | Duplicate packaged Core item definition:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-item.legacy-name` | Error | Core item legacyName does not match the adapter catalog:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |

## `core-stage.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `core-stage.catalog-missing` | Error | Core stage is absent from the legacy adapter catalog:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-stage.duplicate` | Error | Duplicate packaged Core stage definition:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-stage.legacy-duplicate` | Error | Duplicate Core stage legacyId:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-stage.legacy-id` | Error | Core stage legacyId does not match the adapter catalog:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |

## `core-weapon.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `core-weapon.catalog-missing` | Error | Core weapon is absent from the legacy adapter catalog:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-weapon.duplicate` | Error | Duplicate packaged Core weapon definition:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `core-weapon.legacy-name` | Error | Core weapon legacyName does not match the adapter catalog:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |

## `dependency.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `dependency.conflict` | Warning | Disabled pack ' | `Assets/Scripts/Modding/ModDependencyResolver.cs` |
| `dependency.cycle` | Error | Pack ' | `Assets/Scripts/Modding/ModDependencyResolver.cs` |
| `dependency.optional-disabled` | Info | Optional dependency ' | `Assets/Scripts/Modding/ModDependencyResolver.cs` |
| `dependency.optional-missing` | Info | Optional dependency ' | `Assets/Scripts/Modding/ModDependencyResolver.cs` |
| `dependency.optional-version` | Warning | Optional dependency ' | `Assets/Scripts/Modding/ModDependencyResolver.cs` |

## `difficulty.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `difficulty.core-missing` | Error | Packaged Core difficulty is missing:  | `Assets/Scripts/Modding/DifficultyDefinition.cs` |
| `difficulty.duplicate-id` | Error | Duplicate difficulty ID:  | `Assets/Scripts/Modding/DifficultyDefinition.cs` |
| `difficulty.duplicate-name` | Error | Difficulty displayName must be unique:  | `Assets/Scripts/Modding/DifficultyDefinition.cs` |

## `discovery.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `discovery.no-directory` | Info | No external Mods directory was found. | `Assets/Scripts/Modding/ModDiscovery.cs` |
| `discovery.no-manifest` | Warning | Mod directory has no manifest.json. | `Assets/Scripts/Modding/ModDiscovery.cs` |
| `discovery.pack-limit` | Error | Mods directory contains more than  | `Assets/Scripts/Modding/ModDiscovery.cs` |

## `dragonbones.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `dragonbones.armature` | Error | The DragonBones file contains no armature. | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.bone-duplicate` | Error | Duplicate bone name:  | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.bone-missing` | Error | Required bone was removed or renamed:  | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.clip-duration` | Error | Animation has no positive frame duration:  | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.clip-missing` | Error | Required animation was removed or renamed:  | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.clip-unmapped` | Warning | Animation is not recorded in the sidecar and will not be imported:  | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.frame-rate` | Error | Armature frameRate must be from 1 through 240. | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.import.document` | Error | Converted output is not a named enemyAnimation or playerAnimation. | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.import.duplicate` | Error | More than one converted animation targets  | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.import.empty` | Error | No matching animations were converted from the edited armature. | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.import.target` | Error | No existing mod animation has ID  | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.interpolation-approximation` | Warning | DragonBones curve and tweenEasing values are not imported by runtime v1. Imported keys use the normalized animation interpolation rules and may not match the edited motion exactly. | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.layer-metadata` | Warning | One or more display slots lost their Captivity sorting metadata. Animation motion can import, but keep layer changes in the enemy JSON or Unity VFX editor. | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.rotation-direction` | Warning | DragonBones clockwise rotation directives are not preserved by runtime v1. Review rotations that cross 180 degrees after import. | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.sidecar` | Error | The sidecar type is not supported. | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.sidecar-data` | Error | The round-trip sidecar is missing its rig or source documents. | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.sidecar-set` | Error | The round-trip sidecar is missing a rig set. | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.slot-missing` | Warning | Bone has no display slot; motion imports but its sprite may be absent in DragonBones:  | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.target-duplicate` | Error | The loose mod contains duplicate animation ID  | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.timeline-bone` | Error | Animation  | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |
| `dragonbones.z-order-ignored` | Warning | DragonBones zOrder timelines are not imported by runtime v1. Keep draw-order changes in normalized sortingOrder tracks and review the paired preview. | `Assets/Editor/Modding/DragonBonesAnimationBridge.cs` |

## `enemy.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `enemy.ai.attack-range-gap` | Error | preferredRange cannot exceed every attack's initiateRange or the enemy can stop outside attack range. | `Assets/Scripts/Modding/EnemyAttackAuthoring.cs` |
| `enemy.ai.retreat-range` | Error | retreatRange cannot be greater than preferredRange. | `Assets/Scripts/Modding/EnemyAiAuthoring.cs` |
| `enemy.ai.type` | Error | Original enemy AI type must be groundChase, flyingChase, or holdPosition. | `Assets/Scripts/Modding/EnemyAiAuthoring.cs` |
| `enemy.animation-ref-duplicate` | Error | Animation clip is defined inline and by reference:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `enemy.animation-ref-missing` | Error | Unknown normalized enemy animation:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `enemy.animation-ref-owner` | Error | Normalized animation  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `enemy.animation-refs.id` | Error | animationRefs values must be enemy-animation content IDs. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation-refs.limit` | Error | animationRefs supports at most 64 clips. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation-refs.name` | Error | animationRefs keys must be semantic clip names. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.attack-hit-fields` | Error | attackHit does not accept amount or impulse fields. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.base-clips` | Error | Original animation sets require idle and move clips. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.camera-shake` | Error | cameraShake amount must be between 0 and 1. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.camera-shake-fields` | Error | cameraShake contains fields belonging to another event type. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.clip` | Error | Clips need a semantic name, bounded duration, and 1..256 frames. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.clips` | Error | At most 32 inline and referenced clips are supported. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.cue-amount` | Error | cue amount must be finite and between -100000 and 100000. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.cue-fields` | Error | cue accepts only cue, index, and amount metadata. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.cue-index` | Error | cue index must be between 0 and 255. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.cue-name` | Error | cue requires a semantic cue name of at most 80 characters. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.effect-bone` | Error | spriteEffect bone must reference an enemy bone. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.effect-fields` | Error | spriteEffect contains fields belonging to another event type. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.effect-region` | Error | spriteEffect region must reference an atlas region. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.effect-sorting` | Error | spriteEffect sortingOrder must be between -10000 and 10000. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.event-after-attack` | Error | Animation event at  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.event-clip` | Error | Enemy animation events are allowed only on clips referenced by an attack or finisher. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.event-loop` | Error | Enemy gameplay events are not allowed on looping clips. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.event-time` | Error | Enemy animation event times must be ordered and fall within the clip duration. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.event-type` | Error | Unsupported enemy animation event type:  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.events` | Error | Animation clips support at most 64 timed events. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.frame` | Error | Frame times must be ordered within the clip and include bone poses. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.impulse` | Error | impulse requires a finite x or y value between -1000 and 1000. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.impulse-amount` | Error | impulse uses x and y rather than amount. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.impulse-fields` | Error | impulse contains fields belonging to another event type. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.pose-bone` | Error | Animation references an unknown bone:  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.pose-region` | Error | Animation references an unknown atlas region:  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.required` | Error | Original enemies require inline animation clips or animationRefs. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.sound-fields` | Error | sound contains fields belonging to another event type. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.sound-file` | Error | sound requires a safe pack-relative WAV or OGG file. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.animation.sound-volume` | Error | sound volume must be between 0 and 1. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.animation` | Error | Original attacks require an animation clip name. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.animation-missing` | Error | Attack references an unknown animation clip:  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.entry` | Error | Attack entries cannot be null. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.grab-finisher` | Error | Grab attacks require the playable starter grab/finisher option. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs, Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.hit-time` | Error | hitTimeSeconds cannot exceed durationSeconds. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.hit-timing` | Error | Original attacks require hitTimeSeconds or an attackHit event in their animation clip. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.id` | Error | Attack IDs must be unique lowercase semantic names. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs, Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.index` | Error | Inherited attack indexes must be unique values from 0 through 31. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.projectile-region` | Error | Projectile attacks require projectileRegion to reference an atlas region. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.ragdoll-inherited` | Error | ragdollSeconds is supported only on original-enemy attacks. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.required` | Error | Original enemies require at least one attack. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.stage-events` | Error | multiStage attacks require exactly one ordered attackHit animation event per stage. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.stage-type` | Error | Each attack stage must use melee, hitscan, projectile, area, or grab. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.stages` | Error | Multi-stage attacks require 2 through 16 hit stages. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs, Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.type` | Error | The starter wizard supports melee, hitscan, projectile, area, grab, and multi-stage attacks. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs, Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.unexpected-stages` | Error | Only multiStage attacks may define stages. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.attacks.weight-total` | Error | At least one attack must have a selection weight greater than zero. | `Assets/Scripts/Modding/EnemyAttackAuthoring.cs` |
| `enemy.behavior.finisher-animation` | Error | downedFinisher animation must be a semantic clip name. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-draft` | Error | The starter finisher requires the DragonBones project and paired-player draft options. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs` |
| `enemy.behavior.finisher-original` | Error | downedFinisher is currently supported only by fully original enemies. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-participant-animation` | Error | Participant animation must be a semantic clip name. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-participant-animation-id` | Error | participantAnimations references an unknown participant:  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-participant-animation-missing` | Error | Participant  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `enemy.behavior.finisher-participant-animation-name` | Error | Participant phase animations must be semantic clip names. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-participant-approach` | Error | Participant approachRange cannot be smaller than joinRange. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-participant-enemy` | Error | Each participant enemy must be an enemy content ID. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-participant-facing` | Error | Participant facing must be preserve, left, or right. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-participant-id` | Error | Participant IDs must be unique semantic names and cannot be 'owner'. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-participant-missing` | Error | Finisher participant references an unavailable original enemy:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `enemy.behavior.finisher-participant-original` | Error | Finisher participants currently require an originalSkeletonAtlas enemy:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `enemy.behavior.finisher-participant-phases` | Error | phaseBoundary participants require a phased finisher. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-participant-policy` | Error | Participant joinPolicy must be startOnly or phaseBoundary. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-participant-required` | Error | Required participants must use joinPolicy startOnly. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-participants` | Error | downedFinisher supports at most 3 secondary NPC participants. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-phase-animation` | Error | Each finisher phase requires an enemy animation clip name. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-phase-animation-missing` | Error | Finisher phase references an unknown enemy animation clip:  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-phase-duration` | Error | Each finisher phase durationSeconds must be 0.1..60. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-phase-id` | Error | Finisher phase IDs must be unique semantic names. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-phase-total` | Error | Combined finisher phase duration cannot exceed 300 seconds. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-phases` | Error | downedFinisher phases must contain 2..8 entries. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-player-animation` | Error | playerAnimation requires a bounded duration and 1..256 frames. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-player-animation-ref` | Error | playerAnimationRef must be a player-animation content ID. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-player-animation-source` | Error | Use playerAnimation or playerAnimationRef, not both. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-player-bone` | Error | playerAnimation references an unsupported player bone:  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-player-event-amount` | Error | playerAnimation event amount must be greater than 0 and within the limit for its event type. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-player-event-fields` | Error | playerAnimation finisher events cannot define fields belonging to enemy-side events. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-player-event-time` | Error | playerAnimation event times must be ordered and fall within the clip duration. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-player-event-type` | Error | Supported playerAnimation event types are pleasure, scaledPleasure, libido, strengthDamage, struggleDamage, and healthDamage. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-player-events` | Error | playerAnimation supports at most 64 timed events. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-player-frame` | Error | playerAnimation frame times must be ordered within the clip and include bone poses. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-player-region` | Error | playerAnimation cannot replace player sprite regions. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-status-description` | Error | Finisher status descriptions support at most 256 characters. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-status-empty` | Error | Finisher status  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-status-key` | Error | Unsupported finisher status:  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.finisher-status-title` | Error | Finisher status titles must contain 1..64 characters. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.module` | Error | Behavior modules must have unique types. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs, Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.module-type` | Error | Unsupported behavior module:  | `Assets/Editor/Modding/NewEnemyWizardWindow.cs, Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.modules` | Error | At most 8 behavior modules are supported. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.on-hit-clothing-id` | Error | onHitEquipClothing clothing must be a clothing content ID. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.spawn-on-death-count` | Error | Spawn-on-death count must be 1 through 32. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs, Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.spawn-on-death-enemy` | Error | Spawn-on-death requires a full enemy content ID. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs, Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.speed-pulse-overlap` | Error | Pulse duration must be shorter than its interval. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs, Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.behavior.vision-range` | Error | Vision range must be between 0.1 and 10000. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs` |
| `enemy.drop-missing` | Error | Enemy references an unknown drop item:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `enemy.drops.duplicate` | Error | Drop item IDs must be unique. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs` |
| `enemy.drops.item` | Error | Invalid drop item ID:  | `Assets/Editor/Modding/NewEnemyWizardWindow.cs, Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.extends-missing` | Error | Enemy extends an unknown Core enemy:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `enemy.finisher-clothing-missing` | Error | Enemy finisher references unknown clothing:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `enemy.null` | Error | Enemy definition resolved to null. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.on-hit-clothing-missing` | Error | Enemy references unknown on-hit clothing:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `enemy.player-animation-ref-missing` | Error | Unknown normalized player animation:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `enemy.spawn-on-death-missing` | Error | Enemy references an unknown spawnOnDeath enemy:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `enemy.spawn.original-inherit` | Error | Original enemies cannot inherit Core template spawners; reference their ID from a stage spawner instead. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.stats.bounty` | Error | Bounty cannot be negative. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs, Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.stats.speed-acceleration` | Error | Acceleration must be between 0.01 and 1000. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs` |
| `enemy.stats.speed-max` | Error | Maximum speed must be between 0.01 and 1000. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs` |
| `enemy.stats.traction` | Error | Traction must be between 0 and 1. | `Assets/Editor/Modding/NewEnemyWizardWindow.cs` |
| `enemy.visual` | Error | Original enemies require a visual definition. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.atlas` | Error | atlas must be a safe pack-relative PNG path. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.bone-cycle` | Error | Skeleton parent hierarchy contains a cycle at  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.bone-id` | Error | Bone IDs must be unique semantic names. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.bone-parent` | Error | Bone references an invalid parent:  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.bone-region` | Error | Bone references an unknown atlas region:  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.bone-transform` | Error | Bone transforms must be finite and pivots must be 0..1. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.bones` | Error | Original skeletons require 1..64 bones. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.hips` | Error | Original skeletons require a hips bone. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.hit-zone-bone` | Error | Hit zones require one unique existing bone. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.hit-zone-damage` | Error | damageMultiplier must be low, normal, critical, or block. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.hit-zone-shape` | Error | Hit-zone shape must be box or circle. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.hit-zones` | Error | Original skeletons require 1..64 hit zones. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.pixels-per-unit` | Error | pixelsPerUnit must be greater than 0 and at most 1024. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.region-name` | Error | Region name is not a safe semantic path:  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.region-rect` | Error | Region rectangle must have a non-negative origin and positive size:  | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.regions` | Error | At least one atlas region is required. | `Assets/Scripts/Modding/EnemyDefinition.cs` |
| `enemy.visual.type` | Error | visual.type must be ' | `Assets/Scripts/Modding/EnemyDefinition.cs` |

## `enemy-animation.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `enemy-animation.bone-missing` | Error | Animation  | `Assets/Scripts/Modding/NormalizedEnemyAnimationDefinition.cs` |
| `enemy-animation.duplicate-id` | Error | Duplicate enemy animation ID:  | `Assets/Scripts/Modding/NormalizedEnemyAnimationDefinition.cs` |
| `enemy-animation.effect-bone-missing` | Error | Animation  | `Assets/Scripts/Modding/NormalizedEnemyAnimationDefinition.cs` |
| `enemy-animation.reference-bone` | Error | Sprite effect references a missing rig bone:  | `Assets/Scripts/Modding/EnemyAnimationReferenceValidator.cs` |
| `enemy-animation.reference-document` | Error | No enemy animation document was supplied. | `Assets/Scripts/Modding/EnemyAnimationReferenceValidator.cs` |
| `enemy-animation.reference-effect` | Error | Effect trigger references an unknown effect:  | `Assets/Scripts/Modding/EnemyAnimationReferenceValidator.cs` |
| `enemy-animation.reference-region` | Error | Sprite key references a missing atlas region:  | `Assets/Scripts/Modding/EnemyAnimationReferenceValidator.cs` |
| `enemy-animation.region-missing` | Error | Animation  | `Assets/Scripts/Modding/NormalizedEnemyAnimationDefinition.cs` |
| `enemy-animation.sample-limit` | Warning | Animation  | `Assets/Scripts/Modding/NormalizedEnemyAnimationDefinition.cs` |
| `enemy-animation.sprite-file` | Error | Animation sprite assets must be PNG files. | `Assets/Scripts/Modding/NormalizedEnemyAnimationDefinition.cs` |

## `integrity.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `integrity.preserved` | Info | Preserved locally modified copy at  | `Assets/Scripts/Modding/ModInstallIntegrity.cs` |

## `local.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `local.archive-retained` | Warning | Mod installed, but the source archive remains in Mods:  | `Assets/Scripts/Modding/LocalCapmodInstaller.cs` |

## `manifest.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `manifest.api-version` | Error | Unsupported modApiVersion  | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.bundle` | Error | assetBundles cannot contain null entries. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.bundle-count` | Error | assetBundles may contain at most one payload per supported platform. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.bundle-path` | Error | Bundle paths must be unique .bundle files below  | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.bundle-platform` | Error | Bundle platforms must be unique: windows, android, or webgl. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.bundle-sha256` | Error | Bundle SHA-256 must contain exactly 64 hexadecimal characters. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.bundle-size` | Error | Bundle size is outside the supported range. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.conflicts-core` | Error | External packs cannot conflict with required Core. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.content-root-path` | Error | Content root must be a safe relative path:  | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.content-roots` | Error | At least one content root is required. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.core-id` | Error | The packaged Core manifest must use ID 'core'. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.display-name` | Error | displayName is required. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.duplicate-content-root` | Error | Content root is declared more than once:  | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.duplicate-dependency` | Error | Dependency ' | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.empty` | Error | Manifest JSON is empty. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.id` | Error | Pack ID is invalid. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.null` | Error | Manifest resolved to null. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.overrides` | Error | overrides entries must be unique public content or asset-slot IDs:  | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.preview-count` | Error | previewImages may contain at most 8 PNG files. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.preview-path` | Error | previewImages entries must be unique safe relative PNG paths:  | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.priority` | Error | priority must be between -100000 and 100000. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.reserved-id` | Error | External packs cannot use the reserved ID 'core'. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.schema-version` | Error | Unsupported schemaVersion  | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.self-dependency` | Error | A pack cannot depend on itself. | `Assets/Scripts/Modding/ModManifestParser.cs` |
| `manifest.version` | Error | Version is not valid semantic versioning. | `Assets/Scripts/Modding/ModManifestParser.cs` |

## `plan.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `plan.size` | Error | Dependency batch exceeds 16 packs or the platform download limit. | `Assets/Scripts/Assembly-CSharp/CoreContentAdapter.cs` |

## `player-animation.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `player-animation.duplicate-id` | Error | Duplicate player animation ID:  | `Assets/Scripts/Modding/NormalizedPlayerAnimationDefinition.cs` |
| `player-animation.presentation-experimental` | Warning | Normalized sprite, color, sorting, and particle presentation is experimental and is now applied by the runtime finisher importer. | `Assets/Scripts/Modding/NormalizedPlayerAnimationDefinition.cs` |
| `player-animation.sample-limit` | Warning | Animation  | `Assets/Scripts/Modding/NormalizedPlayerAnimationDefinition.cs` |

## `recovery.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `recovery.restored` | Warning | Recovered the previous version after an interrupted update:  | `Assets/Scripts/Modding/ModCatalog.cs` |

## `registry.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `registry.bind-missing` | Error | Cannot bind an unregistered content ID:  | `Assets/Scripts/Modding/ContentRegistry.cs` |
| `registry.bind-null` | Error | Cannot bind a null runtime asset:  | `Assets/Scripts/Modding/ContentRegistry.cs` |
| `registry.duplicate` | Error | Content ID is already registered:  | `Assets/Scripts/Modding/ContentRegistry.cs` |
| `registry.namespace` | Error | Content ID namespace must match its defining pack:  | `Assets/Scripts/Modding/ContentRegistry.cs` |
| `registry.null` | Error | Cannot register a null content entry. | `Assets/Scripts/Modding/ContentRegistry.cs` |

## `retexture.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `retexture.duplicate-slot` | Error | The slot was added more than once:  | `Assets/Scripts/Modding/SimpleRetexturePackBuilder.cs` |
| `retexture.empty` | Error | Add at least one replacement PNG. | `Assets/Scripts/Modding/SimpleRetexturePackBuilder.cs` |
| `retexture.exists` | Error | Retexture files already exist. Confirm replacement in the wizard to update them. | `Assets/Scripts/Modding/SimpleRetexturePackBuilder.cs` |
| `retexture.pack` | Error | Choose a mod folder containing manifest.json. | `Assets/Scripts/Modding/SimpleRetexturePackBuilder.cs` |
| `retexture.patch-id` | Error | Patch name must use lowercase ID characters, for example jenny-blue. | `Assets/Scripts/Modding/SimpleRetexturePackBuilder.cs` |
| `retexture.png` | Error | Replacement must be an existing PNG file. | `Assets/Scripts/Modding/SimpleRetexturePackBuilder.cs` |
| `retexture.scale` | Error | Source scale must be between 1 and 16. | `Assets/Scripts/Modding/SimpleRetexturePackBuilder.cs` |
| `retexture.slot` | Error | Invalid relative slot:  | `Assets/Scripts/Modding/SimpleRetexturePackBuilder.cs` |
| `retexture.target` | Error | Target must be a valid public content ID. | `Assets/Scripts/Modding/SimpleRetexturePackBuilder.cs` |

## `rule.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `rule.starter-missing` | Error | Unknown starter weapon:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `rule.weapon-missing` | Error | Unknown progression weapon:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |

## `stage.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `stage.audio-decode` | Error | Could not decode  | `Assets/Scripts/Assembly-CSharp/ExternalStageFactory.cs` |
| `stage.audio-file` | Error | Stage  | `Assets/Scripts/Modding/ModContentDiscovery.cs` |
| `stage.audio-load` | Error | Could not load  | `Assets/Scripts/Assembly-CSharp/ExternalStageFactory.cs` |
| `stage.audio.empty` | Error | audio must replace at least one stage audio slot. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.camera.bounds` | Error | Camera bounds require finite min/max coordinates with max greater than min. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.enemy-missing` | Error | Stage references an unknown enemy:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `stage.extends-missing` | Error | Stage extends an unknown Core stage:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `stage.interaction-challenge-missing` | Error | Stage interaction references an unknown required challenge:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `stage.interaction-give-item-missing` | Error | Stage interaction references an unknown reward item:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `stage.interaction-required-item-missing` | Error | Stage interaction references an unknown required item:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `stage.layout` | Error | layout is required. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.layout.hide-inherited-visuals` | Error | hideInheritedVisuals is only available for tiledJson layouts. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.layout.path` | Error | Tiled layout path must be a safe pack-relative .json file. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.layout.pixels-per-unit` | Error | pixelsPerUnit must be between 1 and 512. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.layout.player-spawn` | Error | tiledJson layouts define player-spawn in the Tiled map. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.layout.preserve-inherited-stage-objects` | Error | preserveInheritedStageObjects is only available for tiledJson layouts. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.layout.reuse-inherited-spawners` | Error | reuseInheritedSpawners is only available for tiledJson compatibility ports. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.layout.spawner-missing` | Error | Tiled map has no enemy-spawner point named  | `Assets/Scripts/Modding/ModContentDiscovery.cs` |
| `stage.layout.spawner-unused` | Error | Tiled enemy-spawner point has no matching stage spawner:  | `Assets/Scripts/Modding/ModContentDiscovery.cs` |
| `stage.layout.type` | Error | layout.type must be 'coreStageLayout' or 'tiledJson'. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.null` | Error | Stage definition resolved to null. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.pickup-missing` | Error | Stage pickup references an unknown item:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `stage.spawner.closed-door-missing` | Error | Spawner references an unknown required-closed Tiled door:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `stage.spawner.delay-jitter` | Error | delayJitterSeconds cannot exceed delaySeconds. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.spawner.door-conflict` | Error | Spawner cannot require the same door to be both open and closed:  | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.spawner.duplicate-enemy` | Error | Spawner contains a duplicate enemy ID:  | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.spawner.enemies` | Error | Each spawner requires between 1 and 64 enemy IDs. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.spawner.enemy-id` | Error | Spawner enemy references must use enemy/ content IDs:  | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.spawner.id` | Error | Spawner IDs must be unique lowercase path segments:  | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.spawner.initial-delay-jitter` | Error | initialDelayJitterSeconds cannot exceed initialDelaySeconds. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.spawner.minimum-wave` | Error | minimumWave must be between 0 and 10000. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.spawner.null` | Error | Spawner entries cannot be null. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.spawner.open-door-missing` | Error | Spawner references an unknown required-open Tiled door:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `stage.spawners` | Error | A stage requires between 1 and 256 spawners. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.test-tools.enabled` | Error | testTools must be omitted unless enabled is true. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.test-tools.enemy` | Error | testTools.enemy must be an enemy content ID. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.waves.first-wave-enemy-count` | Error | firstWaveEnemyCount must be between 1 and 1000. | `Assets/Scripts/Modding/StageDefinition.cs` |
| `stage.weapon-case-missing` | Error | Stage weapon case references an unknown weapon:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |

## `stage-script.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `stage-script.duplicate-id` | Error | Duplicate stage script ID:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `stage-script.stage-missing` | Error | Stage script targets an unknown external stage:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `stage-script.volume-missing` | Error | Stage script references an unknown script-trigger rectangle:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |

## `tiled.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `tiled.altar-shape` | Error | altar objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.audio-file` | Error | Audio must be a pack-local WAV or OGG between 1 byte and 64 MiB:  | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.audio-mixer-group` | Error | mixerGroup must be Ambience or SFX. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.audio-values` | Error | volume must be 0..1 and distance must satisfy 0.1 <= minDistance <= maxDistance <= 10000. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.core-art-id` | Error | Unknown public Core map-art ID:  | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.core-art-shape` | Error | core-art objects must be non-empty rectangles with finite rotation. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.core-art-sorting-layer` | Error | core-art sortingLayer must be Background, Decoration, or Platform. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.core-art-sorting-order` | Error | core-art sortingOrder must be between -10000 and 10000. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.core-prop-asset` | Error | core-prop requires a core:stage-prop/... asset ID. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.core-prop-shape` | Error | core-prop objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.core-stage-object-kind` | Error | coreObjectKind must be object, door, interaction, actor, item, pose, or light. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.core-stage-object-shape` | Error | core-stage-object adapters must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.core-stage-object-source` | Error | core-stage-object requires a bounded relative sourcePath inside its inherited Core stage. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.decoration-adaptive-threshold` | Error | Decoration adaptiveModeThreshold must be between 0 and 1. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.decoration-gid` | Error | Decoration ' | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.decoration-opacity` | Error | Decoration opacity must be between 0 and 1. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.decoration-shape` | Error | Decoration objects must be visible, non-empty tile objects with finite rotation. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.decoration-sorting-layer` | Error | Decoration sortingLayer must be Sky, Background, Decoration, or Platform. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.decoration-sorting-order` | Error | Decoration sortingOrder must be between -10000 and 10000. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.door-price` | Error | door price must be between 0 and 1,000,000. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.door-properties` | Error | door item and sprite identifiers may not exceed 128 characters. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.door-proximity` | Error | proximityRadius must be greater than 0 and at most 100. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.door-shape` | Error | door objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.door-switch-shape` | Error | door-switch objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.door-switch-targets` | Error | door-switch requires a comma-separated targetDoors property. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.door-switch-visual` | Error | onVisualFile and offVisualFile must both be pack-local PNGs with visualPixelsPerUnit from 1 to 1024. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.door-type` | Error | doorType must be standard, jacky, or roller. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.door-visual` | Error | Standard and Jacky doors require an open/closed PNG pair; roller doors use one visualFile PNG. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.file` | Error | Tiled JSON is missing or outside its pack:  | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.file-size` | Error | Tiled JSON must be between 1 byte and 16 MiB. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.freeform-light-shape` | Error | freeform-light must be a point. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.freeform-light-values` | Error | freeform-light has invalid color, shape, intensity, or sorting layers. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.global-light-shape` | Error | global-light must be a point. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.global-light-values` | Error | global-light has invalid color, intensity, falloff, or sorting layers. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.image-layer-file` | Error | Image layers require a relative pack-local PNG. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.image-layer-values` | Error | Image-layer offsets must be finite; opacity must be 0..1 and parallax factors must be 0..2. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-chain-cycle` | Error | Interaction chains must not contain cycles (at  | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-challenges` | Error | requiredChallenges must contain at most 100 challenge content IDs. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-door-action` | Error | doorAction must be open, close, or toggle. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-effect` | Error | interaction requires at least one supported action. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-enemy-count` | Error | minEnemies/maxEnemies must be 0..10000 and maxEnemies must be zero or at least minEnemies. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-give-item` | Error | giveItem must be an item content ID with giveItemAmount 1..10000, or both must be empty/zero. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-health` | Error | restoreHealth must be between 0 and 10000. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-light-action` | Error | lightAction must be on, off, toggle, or activate. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-message` | Error | interaction message may contain at most 512 characters. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-money` | Error | giveMoney must be between -1000000 and 1000000. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-physics` | Error | knockback components must be -1000..1000 and ragdollSeconds must be 0..60. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-portable-visual` | Error | visualFile and activatedVisualFile must both be pack-local PNGs with valid visual settings. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-price` | Error | price must be 0 through 1000000 and is only supported for use interactions. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-required-interactions` | Error | requiredInteractions may contain at most 100 interaction IDs. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-required-item` | Error | requiredItem must be an item content ID on a use interaction; consumeRequiredItem requires it. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-required-money` | Error | requiredMoney must be between 0 and 1000000. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-shape` | Error | use interactions must be points; touch and shoot interactions must be non-empty rectangles. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-spawn` | Error | spawnCount must be 1..1000 when targetSpawners is set, or 0 when it is empty. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-timing` | Error | activationsRequired must be 1..10000 and delaySeconds must be 0..3600. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-trigger` | Error | interaction trigger must be use, touch, or shoot. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-visual` | Error | visualArt must be a public Core map-art ID and visualScale must be 0.1..20. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.interaction-wave` | Error | minWave/maxWave must be non-negative and maxWave must be zero or at least minWave. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.keypad-code` | Error | keypad code must contain 1 through 16 digits. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.keypad-shape` | Error | keypad objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.keypad-targets` | Error | keypad requires a comma-separated targetDoors property. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.light-flicker` | Error | light-bulb flicker must be between 0 and 1. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.light-portable-visual` | Error | Portable light visuals require paired pack-local PNGs and valid sprite, color, and radius settings. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.light-shape` | Error | light-bulb objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.moving-platform-art` | Error | Unknown public Core map-art ID:  | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.moving-platform-offset` | Error | moveX/moveY must define a finite non-zero destination within 100000 pixels. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.moving-platform-shape` | Error | moving-platform objects must be non-empty rectangles. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.moving-platform-timing` | Error | travelSeconds must be 0.1..3600 and pauseSeconds must be 0..3600. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.nav-node-connection` | Error | nav-node connectionType must be move or climb. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.nav-node-shape` | Error | nav-node objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.note-font-size` | Error | note fontSize must be between 8 and 100. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.note-shape` | Error | note objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.note-text` | Error | note text must contain 1 through 4096 characters. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.null` | Error | Tiled map resolved to null. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.object-coordinate` | Error | Gameplay object coordinates must be finite and bounded. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.particle-color` | Error | particle color must use #RRGGBB or #RRGGBBAA. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.particle-shape` | Error | particle-emitter objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.particle-values` | Error | particle values are outside their supported bounds. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.pickup-amount` | Error | pickup amount must be between 1 and 10000. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.pickup-item` | Error | pickup requires an item property containing an item content ID. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.pickup-shape` | Error | pickup objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.platform-shape` | Error | Platform objects must be non-empty rectangles. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.point-light-shape` | Error | point-light must be a point. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.point-light-values` | Error | point-light has invalid cone, pulse, or rendering settings. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.proximity-light-file` | Error | proximity-light requires a pack-local PNG no larger than 16 MiB. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.proximity-light-shape` | Error | proximity-light must be a point. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.proximity-light-values` | Error | proximity-light has invalid visual, detection, or shape settings. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.room-entry-room` | Error | room-entry requires a room property. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.room-entry-shape` | Error | room-entry objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.room-shape` | Error | room objects must be non-empty rectangles. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.room-transition-destination` | Error | room-transition requires a destination room-entry ID. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.room-transition-shape` | Error | room-transition objects must be non-empty rectangles. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.script-trigger-shape` | Error | script-trigger objects must be non-empty rectangles. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.scripted-actor-enemy` | Error | scripted-actor requires an enemy content ID. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.scripted-actor-shape` | Error | scripted-actor objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.stage-item-file` | Error | stage-item requires a pack-local PNG no larger than 16 MiB. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.stage-item-shape` | Error | stage-item objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.stage-item-text` | Error | stage-item requires a displayName and bounded onPickupSignal. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.stage-item-values` | Error | stage-item visual, collider, weight, or value properties are outside their supported bounds. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.stage-marker-shape` | Error | stage-marker objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.template-cycle` | Error | Tiled object-template cycle detected:  | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.template-file` | Error | Tiled object template is missing or outside its pack:  | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.template-limit` | Error | Tiled map exceeds the object-template expansion limit. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.template-object` | Error | Tiled template has no object definition:  | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.template-path` | Error | Tiled object templates must be relative pack-local .tx files. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.template-size` | Error | Tiled object templates must be between 1 byte and 1 MiB. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.tile-diagonal-flip` | Warning | Diagonal tile flips are experimental and currently render without the diagonal transform. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.tile-gid` | Error | Tile layer ' | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.tile-layer-data` | Error | Visible tile layers require at most 1,000,000 cells in an uncompressed numeric data array matching width times height. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.tile-layer-values` | Error | Tile layer offsets and opacity must be finite and opacity must be between 0 and 1. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.tileset-animation` | Error | Animated tiles require 2..256 valid in-tileset frames with durations of 16..60000 ms. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.tileset-border` | Error | Sprite borders must be non-negative and fit inside the tileset tile dimensions. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.tileset-dimensions` | Error | Tilesets require positive tile/image dimensions, tilecount, and columns. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.tileset-gid` | Error | Tileset firstgid values must be positive and strictly increasing. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.tileset-image` | Error | Tileset images must be relative PNG paths inside the pack. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.tileset-image-size` | Error | Tileset tile grid does not fit inside its declared image dimensions. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.tileset-object-alignment` | Error | Tileset objectalignment is not a supported Tiled alignment. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.tileset-source` | Error | External tilesets require a relative pack-local .json or .tsj source. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.vendor-visual` | Error | visualFile must be a pack-local PNG no larger than 16 MiB, with visualPixelsPerUnit from 1 to 1024. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.weapon-case-shape` | Error | weapon-case objects must be points. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.weapon-case-size` | Error | weapon-case caseSize must be 1, 2, or 3. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.weapon-case-visual` | Error | visualFile and brokenVisualFile must both be pack-local PNGs with visualPixelsPerUnit from 1 to 1024. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |
| `tiled.weapon-case-weapon` | Error | weapon-case requires a weapon property containing an item/weapon/ content ID. | `Assets/Scripts/Modding/TiledLevelDefinition.cs` |

## `ui.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `ui.mods-canvas-missing` | Error | Cannot open the Mods screen because the title buttons have no Canvas. | `Assets/Scripts/Assembly-CSharp/CoreContentAdapter.cs` |

## `usable.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `usable.extends-missing` | Error | Usable extends an unknown Core item:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `usable.null` | Error | Usable definition resolved to null. | `Assets/Scripts/Modding/UsableDefinition.cs` |
| `usable.visual` | Error | visual is required. | `Assets/Scripts/Modding/UsableDefinition.cs` |
| `usable.visual.collider` | Error | colliderWidth and colliderHeight must be greater than 0 and at most 16. | `Assets/Scripts/Modding/UsableDefinition.cs` |
| `usable.visual.pivot` | Error | pivotX and pivotY must be between 0 and 1. | `Assets/Scripts/Modding/UsableDefinition.cs` |
| `usable.visual.pixels-per-unit` | Error | pixelsPerUnit must be greater than 0 and at most 1024. | `Assets/Scripts/Modding/UsableDefinition.cs` |
| `usable.visual.sorting-order` | Error | sortingOrder must be between -1000 and 1000. | `Assets/Scripts/Modding/UsableDefinition.cs` |
| `usable.visual.type` | Error | visual type must be 'coreUsableSprites' or 'originalUsableSprites'. | `Assets/Scripts/Modding/UsableDefinition.cs` |

## `weapon.*`

| Code | Severity | Message prefix | Source |
| --- | --- | --- | --- |
| `weapon.alternate.beam` | Error | A beam alternate fire requires behavior.beam. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.alternate.burst-interval` | Error | alternateFire.burstIntervalSeconds is required when burstCount is greater than one. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.alternate.melee` | Error | A melee alternate fire requires behavior.melee. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.alternate.mode` | Error | alternateFire.mode must be hitscan, projectile, melee, or beam. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.alternate.projectile` | Error | A projectile alternate fire requires behavior.projectile. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.ammunition.duration` | Error | effectDurationSeconds is required when damageOverTime is used. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.ammunition.slow-duration` | Error | slowDurationSeconds is required when slowMultiplier is below one. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.ammunition.type` | Error | ammunition.type must be standard, fmj, hollow-point, incendiary, toxic, freeze, or custom. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.animations.asset-path` | Error | Animation sprites must use safe pack-relative PNG paths. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.animations.clip-count` | Error | animations.clips supports at most idle, equip, fire, alternate, charge, and reload. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.animations.clip-name` | Error | Animation clip names must be idle, equip, fire, alternate, charge, or reload. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.animations.frame` | Error | Animation frames require a duration from 0.01 through 10 seconds and at least one sprite. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.animations.frames` | Error | Each animation clip requires 1 through 120 frames. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.animations.original-state` | Error | Fully original weapons use timed sprite clips and cannot reference inherited Animator states or speed multipliers. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.animations.slot` | Error | Animation sprites must target a visible published prefab slot:  | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.animations.state` | Error | Animation state names must contain 1 to 80 characters and no path separators. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.beam.color` | Error | beam.color must use #RRGGBB or #RRGGBBAA. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.beam.input` | Error | beam.input must be primary or alternate. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.behavior.burst-interval` | Error | burstIntervalSeconds is required when burstCount is greater than one. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.behavior.casing-on` | Error | casingOn must be shoot, reload, or both. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.behavior.tracer-color` | Error | tracerColor must use #RRGGBB or #RRGGBBAA. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.charge.charging-color` | Error | charge.chargingColor must use #RRGGBB or #RRGGBBAA. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.charge.damage-range` | Error | minimumDamageMultiplier cannot exceed maximumDamageMultiplier. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.charge.ready-color` | Error | charge.readyColor must use #RRGGBB or #RRGGBBAA. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.extends-missing` | Error | Weapon extends an unknown Core item:  | `Assets/Scripts/Modding/ModLoaderBootstrap.cs` |
| `weapon.melee.input` | Error | melee.input must be primary or alternate. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.null` | Error | Weapon definition resolved to null. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.projectile.mode` | Error | projectile.mode must be hitscan or physical. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.projectile.sprite` | Error | A physical projectile requires a PNG sprite. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.stats.ammo-pair` | Error | ammoMax and magazineSize must be overridden together. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.stats.magazine-size` | Error | magazineSize cannot exceed ammoMax. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.stats.original-reload-type` | Error | Fully original weapons currently support the event-free Magazine reload type. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.stats.original-required` | Error | A fully original weapon requires a complete stats object. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.throwable.explode-on-impact` | Error | throwable.explodeOnImpact requires a positive explosionRadius. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.throwable.explosion-color` | Error | throwable.explosionColor must use #RRGGBB or #RRGGBBAA. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.throwable.explosion-damage` | Error | A throwable explosionRadius requires a positive explosionDamageMultiplier. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.throwable.explosion-style` | Error | throwable.explosionStyle must be burst or fire. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.throwable.ignite` | Error | throwable igniteDurationSeconds requires igniteDamagePerTick and igniteTicksPerSecond. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.throwable.primary-conflict` | Error | throwable cannot be combined with projectile, melee, charge, or beam primary behavior. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.visual` | Error | visual is required. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.visual.asset-path` | Error | Weapon sprites must use safe pack-relative PNG paths:  | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.visual.body` | Error | A fully original weapon requires the body sprite. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.visual.bundle-prefab` | Error | bundlePrefab must be a prefab/... ID owned by the defining pack. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.visual.hidden-slot` | Error | hiddenSlots must name visible published prefab slots:  | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.visual.original-fields` | Error | Rig, collider, pivot, and muzzle fields are only valid for originalWeaponSprites. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.visual.part` | Error | Each parts key must name a visible sprite from visual.sprites. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.visual.part-sorting-order` | Error | part sortingOrder must be between -100 and 100. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.visual.parts` | Error | parts is only valid for originalWeaponSprites. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.visual.pixels-per-unit` | Error | pixelsPerUnit must be greater than 0 and at most 1024. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.visual.slot` | Error | Sprite slot is not published by the selected Core weapon template:  | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.visual.sorting-order` | Error | sortingOrder must be between -100 and 100. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.visual.sprites` | Error | At least one weapon sprite is required. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
| `weapon.visual.type` | Error | visual.type must be coreWeaponSprites or originalWeaponSprites. | `Assets/Scripts/Modding/WeaponDefinition.cs` |
