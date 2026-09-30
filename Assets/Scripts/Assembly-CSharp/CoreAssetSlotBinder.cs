using System;
using System.Collections;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class CoreAssetSlotBinder
{
	private static readonly ContentId PistolOwnerId = ContentId.Parse("core:item/weapon/pistol");
	private static readonly ContentId PlayerOwnerId = ContentId.Parse("core:player");
	private static readonly ContentId UsableVendorArtOwnerId = ContentId.Parse("core:map-art/usable-vendor");
	private static readonly ContentId MuzzleFlashOwnerId = ContentId.Parse("core:weapon-effects/muzzle-flashes");
	private static readonly ContentId ChestSevenOwnerId = ContentId.Parse("core:sprite/chest-7");
	private static readonly ContentId ChestFiveOwnerId = ContentId.Parse("core:sprite/chest-5");
	private static readonly ContentId ButtSixOwnerId = ContentId.Parse("core:sprite/butt-6");
	private static readonly ContentId ZombieButtOwnerId = ContentId.Parse("core:sprite/butt");
	private static readonly ContentId ZombieTwoButtOwnerId = ContentId.Parse("core:sprite/butt-2");
	private static readonly ContentId EnemyAnatomyOwnerId = ContentId.Parse("core:enemy-anatomy");
	private static readonly ContentId FerArtOwnerId = ContentId.Parse("core:stage-art/fer");
	private static readonly ContentId PlayerBlushOwnerId = ContentId.Parse("core:player/face/blush");
	private static readonly ContentId PlacedButtOwnerId = ContentId.Parse("core:sprite/butt-5");
	private static readonly ContentId PlacedTorsoOwnerId = ContentId.Parse("core:sprite/torso-lower-0");
	private static readonly SkinColor[] SkinColors = { SkinColor.Pale, SkinColor.White, SkinColor.Tan, SkinColor.Black };
	private static readonly Dictionary<string, string> PlayerPartNames = new Dictionary<string, string>(StringComparer.Ordinal)
	{
		{ "bp_spine", "torso-lower" },
		{ "bp_butt", "butt" },
		{ "bp_hip", "hips" },
		{ "bp_chest", "chest" },
		{ "bp_neck", "neck" },
		{ "bp_head", "head" },
		{ "bp_ear", "ear" },
		{ "face_eyelidLowerHideEye", "face/eyelid-lower" },
		{ "bp_lArmUpper", "arm-upper" },
		{ "bp_rArmUpper", "arm-upper" },
		{ "bp_lArmLower", "arm-lower" },
		{ "bp_rArmLower", "arm-lower" },
		{ "bp_lHand", "hand" },
		{ "bp_rHand", "hand" },
		{ "bp_lLegUpper", "leg-upper" },
		{ "bp_rLegUpper", "leg-upper" },
		{ "bp_lLegLower", "leg-lower" },
		{ "bp_rLegLower", "leg-lower" },
		{ "bp_lFoot", "foot" },
		{ "bp_rFoot", "foot" }
	};
	private static readonly Dictionary<string, string[]> CoreEnemyPartNames =
		new Dictionary<string, string[]>(StringComparer.Ordinal)
	{
		{ "body/torso-lower", new[] { "bp_spine" } },
		{ "body/butt", new[] { "bp_butt" } },
		{ "body/hips", new[] { "bp_hips" } },
		{ "body/chest", new[] { "bp_chest" } },
		{ "body/neck", new[] { "bp_neck" } },
		{ "body/head", new[] { "bp_head" } },
		{ "body/arm-upper", new[] { "bp_lArmUpper", "bp_rArmUpper" } },
		{ "body/arm-lower", new[] { "bp_lArmLower", "bp_rArmLower" } },
		{ "body/hand", new[] { "bp_lHand", "bp_rHand" } },
		{ "body/leg-upper", new[] { "bp_lLegUpper", "bp_rLegUpper" } },
		{ "body/leg-lower", new[] { "bp_lLegLower", "bp_rLegLower" } },
		{ "body/foot-left", new[] { "bp_lFoot" } },
		{ "body/foot-right", new[] { "bp_rFoot" } },
		{ "body/penis", new[] { "bp_penis" } }
	};
	private static readonly HashSet<ContentId> LoadedPatchSlots = new HashSet<ContentId>();
	private static readonly List<AssetPatchRequest> RuntimePatches = new List<AssetPatchRequest>();
	private static readonly HashSet<int> BoundUsableVendorRenderers = new HashSet<int>();
	private static readonly HashSet<ContentId> BoundNamedSlotCallbacks = new HashSet<ContentId>();
	private static readonly HashSet<SpriteRenderer> NamedSpriteRenderers = new HashSet<SpriteRenderer>();
	private static readonly Dictionary<string, Sprite> NamedSpriteReplacements =
		new Dictionary<string, Sprite>(StringComparer.Ordinal);
	private static readonly string[] LegacyVisualSpriteNames =
	{
		"Foot", "ArmLower_2", "Blush", "LegUpper_1", "Head2", "Head3", "Head1",
		"LegUpper_3", "Eyelid", "Hand_2", "Neck_5",
		"Hip_3", "Neck_8", "Head_8", "Head4", "Hip_5",
		"LegLower_11", "rontgen", "LegLower_15", "painting", "MedicBody", "Hand_16",
		"TorsoLower_8", "ArmUpper_18", "face", "ArmUpper_20", "LadyStatue", "Butt_15"
	};

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetRuntimeState()
	{
		// These collections otherwise survive when Enter Play Mode has domain
		// reload disabled, while ModLoaderRuntime creates a fresh slot registry.
		LoadedPatchSlots.Clear();
		RuntimePatches.Clear();
		BoundUsableVendorRenderers.Clear();
		BoundNamedSlotCallbacks.Clear();
		NamedSpriteRenderers.Clear();
		NamedSpriteReplacements.Clear();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Initialize()
	{
		BindAvailableSlots();
		ScheduleDelayedBind();
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private static void OnSceneLoaded(Scene i_scene, LoadSceneMode i_mode)
	{
		BindAvailableSlots();
		ScheduleDelayedBind();
	}

	private static void ScheduleDelayedBind()
	{
		ExternalFactoryRunner.GetOrAdd<CoreAssetSlotBinderHost>().Restart();
	}

	internal static void BindAfterSceneObjectsStarted()
	{
		BindAvailableSlots();
	}

	internal static void RestoreBaselineAssets()
	{
		ModLoaderRuntime.AssetSlots.RestoreBaselines();
	}

	/// <summary>Publishes a reconstructed Core garment's icon and semantic piece sprites as patchable slots.</summary>
	internal static void RegisterCoreClothingSlots(Clothing i_clothing, ContentId i_ownerId)
	{
		if (i_clothing == null || i_ownerId.Namespace != "core" || !i_ownerId.Path.StartsWith("clothing/", StringComparison.Ordinal)) return;
		Sprite icon = i_clothing.GetIcon();
		if (icon != null)
			RegisterCoreClothingSpriteSlot(ContentId.Parse(i_ownerId + "/icon"), i_ownerId, icon,
				asset => { if (i_clothing != null) i_clothing.SetIcon((Sprite)asset); }, "Core clothing icon");
		foreach (ClothingPiece piece in i_clothing.GetClothingPieces())
		{
			if (piece == null) continue;
			SpriteRenderer renderer = piece.GetComponent<SpriteRenderer>();
			ModClothingSlotIdentity identity = piece.GetComponent<ModClothingSlotIdentity>();
			if (renderer == null || renderer.sprite == null || identity == null || !ClothingSlotCatalog.IsValidPieceSlot(identity.Slot)) continue;
			SpriteRenderer boundRenderer = renderer;
			RegisterCoreClothingSpriteSlot(ContentId.Parse(i_ownerId + "/" + identity.Slot), i_ownerId,
				renderer.sprite, asset => { if (boundRenderer != null) boundRenderer.sprite = (Sprite)asset; },
				"Core clothing piece " + identity.Slot);
		}
	}

	/// <summary>
	/// Publishes slots on the shipped Core garment prefab using the exported catalog's
	/// stable piece ordering. This keeps the original rig/pivots while allowing data-only
	/// packs to replace its artwork.
	/// </summary>
	internal static void RegisterCoreClothingSlots(Clothing i_clothing, CoreClothingDefinition i_definition)
	{
		if (i_clothing == null || i_definition == null) return;
		// RuntimeInitializeOnLoadMethod can run before LibraryClothes.Awake has filled
		// the prefab's cached piece list. Initialize is idempotent and only discovers
		// the existing children; it does not rebuild or reposition the garment.
		i_clothing.Initialize();
		Sprite icon = i_clothing.GetIcon();
		if (icon != null)
			RegisterCoreClothingSpriteSlot(ContentId.Parse(i_definition.Id + "/icon"), i_definition.Id, icon,
				asset => { if (i_clothing != null) i_clothing.SetIcon((Sprite)asset); }, "Core clothing icon");

		List<ClothingPiece> pieces = i_clothing.GetClothingPieces();
		int count = Mathf.Min(pieces.Count, i_definition.Pieces.Count);
		for (int index = 0; index < count; index++)
		{
			ClothingPiece piece = pieces[index];
			CoreClothingPieceRecord record = i_definition.Pieces[index];
			SpriteRenderer renderer = piece == null ? null : piece.GetComponent<SpriteRenderer>();
			if (renderer == null || renderer.sprite == null || record == null || !ClothingSlotCatalog.IsValidPieceSlot(record.Slot)) continue;
			SpriteRenderer boundRenderer = renderer;
			RegisterCoreClothingSpriteSlot(ContentId.Parse(i_definition.Id + "/" + record.Slot), i_definition.Id,
				renderer.sprite, asset => { if (boundRenderer != null) boundRenderer.sprite = (Sprite)asset; },
				"Core prefab clothing piece " + record.Slot);
		}
	}

	/// <summary>Publishes the shipped enemy rig's semantic body renderers as independently patchable slots.</summary>
	internal static void RegisterCoreEnemySlots(NPC i_enemy, ContentId i_ownerId)
	{
		if (i_enemy == null || i_ownerId.Namespace != "core" ||
			!i_ownerId.Path.StartsWith("enemy/", StringComparison.Ordinal)) return;
		foreach (KeyValuePair<string, string[]> part in CoreEnemyPartNames)
		{
			List<SpriteRenderer> renderers = new List<SpriteRenderer>();
			foreach (string objectName in part.Value)
			{
				Transform target = FindRecursive(i_enemy.transform, objectName);
				SpriteRenderer renderer = target == null ? null : target.GetComponent<SpriteRenderer>();
				if (renderer != null && renderer.sprite != null) renderers.Add(renderer);
			}
			if (renderers.Count == 0) continue;
			ContentId slotId = ContentId.Parse(i_ownerId + "/" + part.Key);
			Sprite baseline = renderers[0].sprite;
			Action<UnityEngine.Object> apply = asset =>
			{
				Sprite replacement = (Sprite)asset;
				foreach (SpriteRenderer renderer in renderers)
					if (renderer != null) renderer.sprite = replacement;
			};
			if (ModLoaderRuntime.AssetSlots.TryGet(slotId, out AssetSlotRegistration existing))
				existing.AddBinding(apply);
			else
				ModLoaderRuntime.AssetSlots.Register(new AssetSlotRegistration(slotId, i_ownerId, "core",
					"Core enemy rig part " + part.Key, baseline, typeof(Sprite), apply), ModLoaderRuntime.LastReport);
		}
	}

	private static void RegisterCoreClothingSpriteSlot(ContentId i_slotId, ContentId i_ownerId, Sprite i_baseline,
		Action<UnityEngine.Object> i_apply, string i_source)
	{
		if (ModLoaderRuntime.AssetSlots.TryGet(i_slotId, out AssetSlotRegistration existing))
		{
			existing.AddBinding(i_apply);
			return;
		}
		ModLoaderRuntime.AssetSlots.Register(new AssetSlotRegistration(i_slotId, i_ownerId, "core", i_source,
			i_baseline, typeof(Sprite), i_apply), ModLoaderRuntime.LastReport);
	}

	private static void BindAvailableSlots()
	{
		Library[] libraries = Resources.FindObjectsOfTypeAll<Library>();
		Library library = FindSceneObject(libraries);
		BindPlayerBodySlots(library == null ? null : library.Actors);
		BindUsableVendorArtSlots();
		BindNamedCoreSpriteSlot("Chest_5", ChestFiveOwnerId, "image");
		BindNamedCoreSpriteSlot("Chest_7", ChestSevenOwnerId, "image");
		BindNamedCoreSpriteSlot("Butt_6", ButtSixOwnerId, "image");
		BindNamedCoreSpriteSlot("Butt", ZombieButtOwnerId, "image");
		BindNamedCoreSpriteSlot("Butt_2", ZombieTwoButtOwnerId, "image");
		BindEnemyAnatomySlots();
		BindFerRetouchArtSlots();
		BindNamedCoreSpriteSlot("Blush_0", PlayerBlushOwnerId, "image");
		BindNamedCoreSpriteSlot("Butt_5", PlacedButtOwnerId, "image");
		BindNamedCoreSpriteSlot("TorsoLower_0", PlacedTorsoOwnerId, "image");
		BindLegacyVisualArtSlots();
		BindPlayerMouthSlots();
		if (library != null && library.Guns != null)
		{
			RegisterCoreMuzzleFlashSlots(library.Guns);
			RegisterCoreWeaponIconSlots(library.Guns);
			Gun pistol = library.Guns.GetGun("Pistol");
			if (pistol != null)
			{
				RegisterPistolSlots(pistol);
				foreach (Gun candidate in Resources.FindObjectsOfTypeAll<Gun>())
				{
					if (candidate == null || candidate == pistol || !candidate.gameObject.scene.IsValid()) continue;
					RuntimeContentIdentity identity = candidate.GetComponent<RuntimeContentIdentity>();
					if (identity != null && identity.TryGetContentId(out ContentId identityId) && identityId == PistolOwnerId)
						RegisterPistolSlots(candidate);
				}
			}
		}

		ResolveNewlyAvailablePatches();
	}

	private static void BindPlayerMouthSlots()
	{
		ContentId ownerId = ContentId.Parse("core:player/face/mouth");
		foreach (ManagerPlayerMouth manager in Resources.FindObjectsOfTypeAll<ManagerPlayerMouth>())
		{
			if (manager == null || !manager.gameObject.scene.IsValid()) continue;
			foreach (KeyValuePair<string, Sprite> pair in manager.GetCoreMouthSprites())
			{
				if (!TryPlayerMouthSlot(pair.Key, out string suffix)) continue;
				ManagerPlayerMouth boundManager = manager;
				string originalName = pair.Key;
				ContentId slotId = ContentId.Parse(ownerId + "/" + suffix);
				Action<UnityEngine.Object> apply = asset =>
				{
					if (boundManager != null) boundManager.SetCoreMouthSprite(originalName, (Sprite)asset);
				};
				if (ModLoaderRuntime.AssetSlots.TryGet(slotId, out AssetSlotRegistration existing))
					existing.AddBinding(apply);
				else
					ModLoaderRuntime.AssetSlots.Register(new AssetSlotRegistration(slotId, ownerId, "core",
						"Core player mouth " + originalName, pair.Value, typeof(Sprite), apply), ModLoaderRuntime.LastReport);
			}
		}
	}

	private static bool TryPlayerMouthSlot(string i_name, out string o_suffix)
	{
		o_suffix = null;
		switch (i_name)
		{
		case "Mouth2": o_suffix = "mouth-2"; break;
		case "Mouth3": o_suffix = "mouth-3"; break;
		case "Mouth4": o_suffix = "mouth-4"; break;
		case "Mouth5": o_suffix = "mouth-5"; break;
		case "Mouth6": o_suffix = "mouth-6"; break;
		case "Mouth7": o_suffix = "mouth-7"; break;
		case "Mouth8": o_suffix = "mouth-8"; break;
		case "Mouth9": o_suffix = "mouth-9"; break;
		case "Mouth10": o_suffix = "mouth-10"; break;
		case "Mouth12": o_suffix = "mouth-12"; break;
		case "MouthTired2": o_suffix = "mouth-tired-2"; break;
		case "MouthOral3": o_suffix = "mouth-oral-3"; break;
		}
		return o_suffix != null;
	}

	private static void RegisterCoreMuzzleFlashSlots(LibraryGuns i_library)
	{
		HashSet<Gun> guns = new HashSet<Gun>();
		foreach (Gun gun in i_library.GetAllGuns()) if (gun != null) guns.Add(gun);
		foreach (Gun gun in Resources.FindObjectsOfTypeAll<Gun>())
			if (gun != null && gun.gameObject.scene.IsValid()) guns.Add(gun);
		foreach (Gun gun in guns)
		{
			IReadOnlyList<Sprite> sprites = gun.GetCoreMuzzleFlashSprites();
			if (sprites == null) continue;
			for (int index = 0; index < sprites.Count; index++)
			{
				Sprite sprite = sprites[index];
				if (sprite == null || !TryMuzzleFlashSlot(sprite.name, out string suffix)) continue;
				Gun boundGun = gun;
				int boundIndex = index;
				ContentId slotId = ContentId.Parse(MuzzleFlashOwnerId + "/" + suffix);
				Action<UnityEngine.Object> apply = asset =>
				{
					if (boundGun != null) boundGun.SetCoreMuzzleFlashSprite(boundIndex, (Sprite)asset);
				};
				if (ModLoaderRuntime.AssetSlots.TryGet(slotId, out AssetSlotRegistration existing))
					existing.AddBinding(apply);
				else
					ModLoaderRuntime.AssetSlots.Register(new AssetSlotRegistration(slotId, MuzzleFlashOwnerId,
						"core", "Core " + sprite.name, sprite, typeof(Sprite), apply), ModLoaderRuntime.LastReport);
			}
		}
	}

	private static bool TryMuzzleFlashSlot(string i_name, out string o_suffix)
	{
		o_suffix = null;
		if (i_name == "MuzzleFlash1") o_suffix = "1";
		else if (i_name == "MuzzleFlash2") o_suffix = "2";
		else if (i_name == "MuzzleFlash3") o_suffix = "3";
		return o_suffix != null;
	}

	private static void BindNamedCoreSpriteSlot(string i_spriteName, ContentId i_ownerId, string i_suffix)
	{
		ContentId slotId = ContentId.Parse(i_ownerId + "/" + i_suffix);
		Sprite baseline = null;
		foreach (SpriteRenderer renderer in Resources.FindObjectsOfTypeAll<SpriteRenderer>())
		{
			if (renderer == null || !renderer.gameObject.scene.IsValid() || renderer.sprite == null
				|| renderer.sprite.name != i_spriteName) continue;
			baseline = renderer.sprite;
			NamedSpriteRenderers.Add(renderer);
		}
		if (baseline == null)
			foreach (Sprite sprite in Resources.FindObjectsOfTypeAll<Sprite>())
				if (sprite != null && sprite.name == i_spriteName) { baseline = sprite; break; }
		if (baseline == null) return;

		Sprite boundBaseline = baseline;
		Action<UnityEngine.Object> apply = asset => SetNamedSpriteReplacement(i_spriteName, boundBaseline, (Sprite)asset);
		if (ModLoaderRuntime.AssetSlots.TryGet(slotId, out AssetSlotRegistration existing))
		{
			if (BoundNamedSlotCallbacks.Add(slotId)) existing.AddBinding(apply);
		}
		else if (ModLoaderRuntime.AssetSlots.Register(new AssetSlotRegistration(slotId, i_ownerId,
			"core", "Core sprite " + i_spriteName, baseline, typeof(Sprite), apply), ModLoaderRuntime.LastReport))
			BoundNamedSlotCallbacks.Add(slotId);
	}

	private static void BindEnemyAnatomySlots()
	{
		BindNamedCoreSpriteSlot("PenisTip", EnemyAnatomyOwnerId, "sprite/penis-tip");
		BindNamedCoreSpriteSlot("PenisBase", EnemyAnatomyOwnerId, "sprite/penis-base");
		BindNamedCoreSpriteSlot("PenisBase_0", EnemyAnatomyOwnerId, "sprite/penis-base-0");
		BindNamedCoreSpriteSlot("Penis", EnemyAnatomyOwnerId, "sprite/penis");
		BindNamedCoreSpriteSlot("PenisBase_1", EnemyAnatomyOwnerId, "sprite/penis-base-1");
		BindNamedCoreSpriteSlot("PenisEnd", EnemyAnatomyOwnerId, "sprite/penis-end");
		BindNamedCoreSpriteSlot("PenisRod", EnemyAnatomyOwnerId, "sprite/penis-rod");
		BindNamedCoreSpriteSlot("PenisBase_2", EnemyAnatomyOwnerId, "sprite/penis-base-2");
		BindNamedCoreSpriteSlot("penis_0", EnemyAnatomyOwnerId, "sprite/penis-0");
		BindNamedCoreSpriteSlot("penis_1", EnemyAnatomyOwnerId, "sprite/penis-1");
		BindNamedCoreSpriteSlot("PenisBase_3", EnemyAnatomyOwnerId, "sprite/penis-base-3");
		BindNamedCoreSpriteSlot("Penis_2", EnemyAnatomyOwnerId, "sprite/penis-2");
		BindNamedCoreSpriteSlot("PenisTip_0", EnemyAnatomyOwnerId, "sprite/penis-tip-0");
		BindNamedCoreSpriteSlot("PenisMid", EnemyAnatomyOwnerId, "sprite/penis-mid");
		BindNamedCoreSpriteSlot("PenisEnd_0", EnemyAnatomyOwnerId, "sprite/penis-end-0");
		BindNamedCoreSpriteSlot("PenisTip_1", EnemyAnatomyOwnerId, "sprite/penis-tip-1");
		BindNamedCoreSpriteSlot("PenisEnd_1", EnemyAnatomyOwnerId, "sprite/penis-end-1");
		BindNamedCoreSpriteSlot("PenisRod_0", EnemyAnatomyOwnerId, "sprite/penis-rod-0");
		BindNamedCoreSpriteSlot("PenisBase_4", EnemyAnatomyOwnerId, "sprite/penis-base-4");
		BindNamedCoreSpriteSlot("Penis_3", EnemyAnatomyOwnerId, "sprite/penis-3");
		BindNamedCoreSpriteSlot("PenisBase_5", EnemyAnatomyOwnerId, "sprite/penis-base-5");
		BindNamedCoreSpriteSlot("PenisBase_6", EnemyAnatomyOwnerId, "sprite/penis-base-6");
		BindNamedCoreSpriteSlot("Penis_4", EnemyAnatomyOwnerId, "sprite/penis-4");
		BindNamedCoreSpriteSlot("PenisEnd_2", EnemyAnatomyOwnerId, "sprite/penis-end-2");
		BindNamedCoreSpriteSlot("PenisEnd_3", EnemyAnatomyOwnerId, "sprite/penis-end-3");
		BindNamedCoreSpriteSlot("PenisMiddle", EnemyAnatomyOwnerId, "sprite/penis-middle");
		BindNamedCoreSpriteSlot("PenisTip_2", EnemyAnatomyOwnerId, "sprite/penis-tip-2");
		BindNamedCoreSpriteSlot("penis_5", EnemyAnatomyOwnerId, "sprite/penis-5");
		BindNamedCoreSpriteSlot("Penis_6", EnemyAnatomyOwnerId, "sprite/penis-6");
		BindNamedCoreSpriteSlot("Penis_7", EnemyAnatomyOwnerId, "sprite/penis-7");
		BindNamedCoreSpriteSlot("PenisMid_0", EnemyAnatomyOwnerId, "sprite/penis-mid-0");
		BindNamedCoreSpriteSlot("PenisMid_1", EnemyAnatomyOwnerId, "sprite/penis-mid-1");
	}

	private static void BindLegacyVisualArtSlots()
	{
		foreach (string spriteName in LegacyVisualSpriteNames)
		{
			ContentId ownerId = ContentId.Parse("core:sprite/" + StableSpriteSlug(spriteName));
			BindNamedCoreSpriteSlot(spriteName, ownerId, "image");
		}
	}

	private static string StableSpriteSlug(string i_name)
	{
		System.Text.StringBuilder result = new System.Text.StringBuilder();
		for (int index = 0; index < i_name.Length; index++)
		{
			char character = i_name[index];
			if (character == '_') character = '-';
			if (char.IsUpper(character) && index > 0 && result.Length > 0 && result[result.Length - 1] != '-')
				result.Append('-');
			result.Append(char.ToLowerInvariant(character));
		}
		return result.ToString();
	}

	private static void BindFerRetouchArtSlots()
	{
		BindNamedCoreSpriteSlot("HeadTurnedOff", FerArtOwnerId, "actor/head-turned-off");
		BindNamedCoreSpriteSlot("HeadTurnedOff_0", FerArtOwnerId, "actor/head-turned-off-alt");
		BindNamedCoreSpriteSlot("Spine_3", FerArtOwnerId, "actor/spine-3");
		BindNamedCoreSpriteSlot("Spine_5", FerArtOwnerId, "actor/spine-5");
		BindNamedCoreSpriteSlot("Spine_9", FerArtOwnerId, "actor/spine-9");
		BindNamedCoreSpriteSlot("LegUpper_4", FerArtOwnerId, "actor/leg-upper-4");
		BindNamedCoreSpriteSlot("LegUpper_6", FerArtOwnerId, "actor/leg-upper-6");
		BindNamedCoreSpriteSlot("LegUpper_17", FerArtOwnerId, "actor/leg-upper-17");
		BindNamedCoreSpriteSlot("Butt_12", FerArtOwnerId, "actor/butt-12");
		BindNamedCoreSpriteSlot("HandBroken", FerArtOwnerId, "actor/hand-broken");
		BindNamedCoreSpriteSlot("Head_11", FerArtOwnerId, "actor/head-11");
		BindNamedCoreSpriteSlot("Head_17", FerArtOwnerId, "actor/head-17");
		BindNamedCoreSpriteSlot("Head_18", FerArtOwnerId, "actor/head-18");
		BindNamedCoreSpriteSlot("Chest_12", FerArtOwnerId, "actor/chest-12");
		BindNamedCoreSpriteSlot("Chest_15", FerArtOwnerId, "actor/chest-15");
		BindNamedCoreSpriteSlot("LegUpperBloody", FerArtOwnerId, "actor/leg-upper-bloody");
		BindNamedCoreSpriteSlot("LegLowerBloody_0", FerArtOwnerId, "actor/leg-lower-bloody");
		BindNamedCoreSpriteSlot("BloodBig1", FerArtOwnerId, "effects/blood-big-1");
		BindNamedCoreSpriteSlot("BloodBig2", FerArtOwnerId, "effects/blood-big-2");
		BindNamedCoreSpriteSlot("BloodSplashesWall1", FerArtOwnerId, "effects/blood-wall-1");
		BindNamedCoreSpriteSlot("BloodSplashesWall2", FerArtOwnerId, "effects/blood-wall-2");
		BindNamedCoreSpriteSlot("BloodSplashesWall3", FerArtOwnerId, "effects/blood-wall-3");
		BindNamedCoreSpriteSlot("bloodSmall1", FerArtOwnerId, "effects/blood-small-1");
		BindNamedCoreSpriteSlot("bloodSmall2", FerArtOwnerId, "effects/blood-small-2");
		BindNamedCoreSpriteSlot("bloodSmall3", FerArtOwnerId, "effects/blood-small-3");
		BindNamedCoreSpriteSlot("bloodSmall4", FerArtOwnerId, "effects/blood-small-4");
		BindNamedCoreSpriteSlot("BloodPlatform", FerArtOwnerId, "effects/blood-platform");
	}

	private static void SetNamedSpriteReplacement(string i_name, Sprite i_baseline, Sprite i_replacement)
	{
		NamedSpriteReplacements.TryGetValue(i_name, out Sprite previous);
		if (i_replacement == null || i_replacement == i_baseline) NamedSpriteReplacements.Remove(i_name);
		else NamedSpriteReplacements[i_name] = i_replacement;
		foreach (SpriteRenderer renderer in NamedSpriteRenderers)
		{
			if (renderer == null || renderer.sprite == null) continue;
			if (renderer.sprite == previous || renderer.sprite.name == i_name)
				renderer.sprite = i_replacement == null ? i_baseline : i_replacement;
		}
	}

	internal static void ApplyAnimatedNamedSpriteReplacements()
	{
		if (NamedSpriteReplacements.Count == 0) return;
		NamedSpriteRenderers.RemoveWhere(renderer => renderer == null);
		foreach (SpriteRenderer renderer in NamedSpriteRenderers)
			if (renderer.sprite != null && NamedSpriteReplacements.TryGetValue(renderer.sprite.name, out Sprite replacement))
				renderer.sprite = replacement;
	}

	private static void ResolveNewlyAvailablePatches()
	{
		if (ModLoaderRuntime.AssetPatches.Count > 0)
		{
			List<AssetPatchRequest> newlyAvailable = RuntimeSpritePatchLoader.Load(
				ModLoaderRuntime.AssetPatches, ModLoaderRuntime.LoadedPacks, ModLoaderRuntime.AssetSlots,
				ModLoaderRuntime.LastReport, LoadedPatchSlots, true);
			foreach (AssetPatchRequest patch in newlyAvailable)
			{
				RuntimePatches.Add(patch);
				LoadedPatchSlots.Add(patch.TargetSlotId);
			}
			if (newlyAvailable.Count == 0) return;
			ModLoaderRuntime.AssetSlots.Resolve(RuntimePatches, ModLoaderRuntime.LastReport);
			Debug.Log("[ModLoader] Public asset slots=" + ModLoaderRuntime.AssetSlots.Count + ", runtime replacements=" + RuntimePatches.Count + ".");
		}
	}

	private static void BindUsableVendorArtSlots()
	{
		const string slotText = "core:map-art/usable-vendor/sprite";
		ContentId slotId = ContentId.Parse(slotText);
		AssetSlotRegistration slot;
		if (!ModLoaderRuntime.AssetSlots.TryGet(slotId, out slot))
		{
			ModStageTemplateLibrary templates = ModStageTemplateLibrary.Load();
			SpriteRenderer templateRenderer = templates == null || templates.UsableVendor == null
				? null : templates.UsableVendor.GetComponent<SpriteRenderer>();
			Sprite baseline = templateRenderer == null ? null : templateRenderer.sprite;
			if (baseline == null) return;
			slot = new AssetSlotRegistration(slotId, UsableVendorArtOwnerId,
				"core", "Core usable-vendor world artwork", baseline, typeof(Sprite));
			if (!ModLoaderRuntime.AssetSlots.Register(slot, ModLoaderRuntime.LastReport)) return;
		}

		// The reconstructed Core stage prefabs have a null sprite on the usable
		// vendor renderer. Identify them by gameplay type instead of sprite name.
		foreach (Vendor vendor in Resources.FindObjectsOfTypeAll<Vendor>())
		{
			if (vendor == null || vendor.GetVendorType() != VendorType.Usables
				|| vendor.gameObject.name.StartsWith("mod-", StringComparison.Ordinal)) continue;
			SpriteRenderer renderer = vendor.GetComponent<SpriteRenderer>();
			if (renderer == null || !BoundUsableVendorRenderers.Add(renderer.GetInstanceID())) continue;
			SpriteRenderer boundRenderer = renderer;
			Action<UnityEngine.Object> apply = asset =>
			{
				if (boundRenderer != null) boundRenderer.sprite = (Sprite)asset;
			};
			slot.AddBinding(apply);
		}
	}

	/// <summary>
	/// Binds a Tiled usable vendor to the public Core art slot. A map-local visual is
	/// its fallback, but an enabled asset patch deliberately takes precedence.
	/// </summary>
	internal static void BindUsableVendorRenderer(SpriteRenderer i_renderer, Sprite i_coreBaseline,
		Sprite i_localFallback)
	{
		if (i_renderer == null) return;
		ContentId slotId = ContentId.Parse("core:map-art/usable-vendor/sprite");
		if (!ModLoaderRuntime.AssetSlots.TryGet(slotId, out AssetSlotRegistration slot))
		{
			if (i_coreBaseline == null) return;
			slot = new AssetSlotRegistration(slotId, UsableVendorArtOwnerId,
				"core", "Core usable-vendor world artwork", i_coreBaseline, typeof(Sprite));
			if (!ModLoaderRuntime.AssetSlots.Register(slot, ModLoaderRuntime.LastReport)) return;
		}
		// The slot can first appear when a neutral Tiled stage creates its vendors,
		// after the initial patch pass. Resolve deferred patches immediately.
		ResolveNewlyAvailablePatches();
		Sprite fallback = i_localFallback != null ? i_localFallback : i_renderer.sprite;
		i_renderer.sprite = fallback;
		slot.AddBinding(asset =>
		{
			if (i_renderer == null) return;
			i_renderer.sprite = asset == slot.BaselineAsset && fallback != null
				? fallback : (Sprite)asset;
		});
		Debug.Log("[ModLoader] Usable vendor art bound: " + i_renderer.gameObject.name
			+ " resolved=" + (slot.ResolvedAsset == null ? "null" : slot.ResolvedAsset.name)
			+ " patched=" + (slot.ResolvedAsset != slot.BaselineAsset) + ".");
	}

	private static void BindPlayerBodySlots(LibraryActors i_library)
	{
		Dictionary<string, List<BodyPartPlayer>> parts = new Dictionary<string, List<BodyPartPlayer>>(StringComparer.Ordinal);
		HashSet<BodyPartPlayer> seen = new HashSet<BodyPartPlayer>();
		if (i_library != null)
		{
			foreach (Player player in i_library.GetAllPlayers())
				foreach (BodyPartPlayer bodyPart in player.GetComponentsInChildren<BodyPartPlayer>(true))
					AddPlayerBodyPart(bodyPart, parts, seen);
		}
		foreach (BodyPartPlayer bodyPart in Resources.FindObjectsOfTypeAll<BodyPartPlayer>())
			AddPlayerBodyPart(bodyPart, parts, seen);

		foreach (KeyValuePair<string, List<BodyPartPlayer>> group in parts)
		{
			foreach (SkinColor skinColor in SkinColors)
			{
				Sprite baseline = null;
				foreach (BodyPartPlayer part in group.Value)
				{
					baseline = part.GetSkinSprite(skinColor);
					if (baseline != null) break;
				}
#if UNITY_EDITOR
				if (baseline == null) baseline = GetEditorPlayerBaseline(group.Key, skinColor);
#endif
				if (baseline == null) continue;
				string colorName = skinColor.ToString().ToLowerInvariant();
				string playerSlot = group.Key.StartsWith("face/", StringComparison.Ordinal)
					? group.Key : "body/" + group.Key;
				ContentId slotId = ContentId.Parse("core:player/" + playerSlot + "/" + colorName);
				BodyPartPlayer[] boundParts = group.Value.ToArray();
				Action<UnityEngine.Object> apply = asset =>
				{
					foreach (BodyPartPlayer part in boundParts)
						if (part != null) part.SetSkinSprite(skinColor, (Sprite)asset);
				};
				if (ModLoaderRuntime.AssetSlots.TryGet(slotId, out AssetSlotRegistration existing))
					existing.AddBinding(apply);
				else
					ModLoaderRuntime.AssetSlots.Register(new AssetSlotRegistration(
						slotId, PlayerOwnerId, "core", "Alex body: " + group.Key + "/" + colorName,
						baseline, typeof(Sprite), apply), ModLoaderRuntime.LastReport);
			}
		}
	}

#if UNITY_EDITOR
	private static Sprite GetEditorPlayerBaseline(string i_semanticName, SkinColor i_skinColor)
	{
		GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Actors/Players/Alex.prefab");
		if (prefab == null) return null;
		foreach (BodyPartPlayer part in prefab.GetComponentsInChildren<BodyPartPlayer>(true))
		{
			if (PlayerPartNames.TryGetValue(part.name, out string semanticName) && semanticName == i_semanticName)
			{
				Sprite baseline = part.GetSkinSprite(i_skinColor);
				if (baseline != null) return baseline;
			}
		}
		return null;
	}
#endif

	private static void AddPlayerBodyPart(BodyPartPlayer i_bodyPart,
		Dictionary<string, List<BodyPartPlayer>> io_parts, HashSet<BodyPartPlayer> io_seen)
	{
		if (i_bodyPart == null || !i_bodyPart.gameObject.scene.IsValid() || !io_seen.Add(i_bodyPart)
			|| !PlayerPartNames.TryGetValue(i_bodyPart.name, out string semanticName)) return;
		if (!io_parts.TryGetValue(semanticName, out List<BodyPartPlayer> matching))
		{
			matching = new List<BodyPartPlayer>();
			io_parts.Add(semanticName, matching);
		}
		matching.Add(i_bodyPart);
	}

	private static Library FindSceneObject(IEnumerable<Library> i_libraries)
	{
		foreach (Library library in i_libraries)
			if (library != null && library.gameObject.scene.IsValid()) return library;
		return null;
	}

	private static void RegisterPistolSlots(Gun i_pistol)
	{
		RegisterSpriteSlot("core:weapon/pistol/slide", PistolOwnerId,
			FindRecursive(i_pistol.transform, "slide"), "Pistol slide renderer");
	}

	private static void RegisterCoreWeaponIconSlots(LibraryGuns i_library)
	{
		foreach (CoreWeaponDefinition definition in ModLoaderRuntime.CoreWeaponDefinitions)
		{
			if (!definition.Id.Path.StartsWith("item/weapon/", StringComparison.Ordinal)) continue;
			Gun coreGun = i_library.GetGun(definition.LegacyName);
			if (coreGun == null) continue;
			RegisterCoreWeaponSlots(coreGun, definition.Id);
			foreach (Gun candidate in Resources.FindObjectsOfTypeAll<Gun>())
			{
				if (candidate == null || candidate == coreGun || !candidate.gameObject.scene.IsValid()) continue;
				RuntimeContentIdentity identity = candidate.GetComponent<RuntimeContentIdentity>();
				if (identity != null && identity.TryGetContentId(out ContentId identityId) && identityId == definition.Id)
					RegisterCoreWeaponSlots(candidate, definition.Id);
			}
		}
	}

	private static void RegisterCoreWeaponSlots(Gun i_gun, ContentId i_ownerId)
	{
		RegisterCoreWeaponIconSlot(i_gun, i_ownerId);
		string weaponName = i_ownerId.Path.Substring("item/weapon/".Length);
		RegisterSpriteSlot("core:weapon/" + weaponName + "/base", i_ownerId,
			FindRecursive(i_gun.transform, "base"), i_gun.GetName() + " base renderer");
	}

	private static void RegisterCoreWeaponIconSlot(Gun i_gun, ContentId i_ownerId)
	{
		Sprite baseline = i_gun.GetSpriteIcon();
		if (baseline == null) return;
		string weaponName = i_ownerId.Path.Substring("item/weapon/".Length);
		ContentId slotId = ContentId.Parse("core:weapon/" + weaponName + "/body");
		Action<UnityEngine.Object> apply = asset =>
		{
			if (i_gun != null) i_gun.SetModItemIcon((Sprite)asset);
		};
		if (ModLoaderRuntime.AssetSlots.TryGet(slotId, out AssetSlotRegistration existing))
		{
			existing.AddBinding(apply);
			return;
		}
		ModLoaderRuntime.AssetSlots.Register(new AssetSlotRegistration(
			slotId, i_ownerId, "core", i_gun.GetName() + " inventory/body sprite", baseline, typeof(Sprite), apply),
			ModLoaderRuntime.LastReport);
	}

	private static Transform FindRecursive(Transform i_parent, string i_name)
	{
		if (string.Equals(i_parent.name, i_name, StringComparison.Ordinal)) return i_parent;
		for (int index = 0; index < i_parent.childCount; index++)
		{
			Transform child = i_parent.GetChild(index);
			if (string.Equals(child.name, i_name, StringComparison.Ordinal)) return child;
			Transform found = FindRecursive(child, i_name);
			if (found != null) return found;
		}
		return null;
	}

	private static void RegisterSpriteSlot(string i_id, ContentId i_ownerId, Transform i_part, string i_source)
	{
		if (i_part == null) return;
		SpriteRenderer renderer = i_part.GetComponent<SpriteRenderer>();
		if (renderer == null || renderer.sprite == null) return;
		ContentId slotId = ContentId.Parse(i_id);
		Action<UnityEngine.Object> apply = asset =>
		{
			if (renderer != null) renderer.sprite = (Sprite)asset;
		};
		if (ModLoaderRuntime.AssetSlots.TryGet(slotId, out AssetSlotRegistration existing))
		{
			existing.AddBinding(apply);
			return;
		}

		AssetSlotRegistration registration = new AssetSlotRegistration(
			slotId,
			i_ownerId,
			"core",
			i_source,
			renderer.sprite,
			typeof(Sprite),
			apply);
		ModLoaderRuntime.AssetSlots.Register(registration, ModLoaderRuntime.LastReport);
	}
}

public sealed class CoreAssetSlotBinderHost : MonoBehaviour
{
	private Coroutine m_pending;

	public void Restart()
	{
		if (m_pending != null) StopCoroutine(m_pending);
		m_pending = StartCoroutine(BindNextFrame());
	}

	private IEnumerator BindNextFrame()
	{
		yield return null;
		CoreAssetSlotBinder.BindAfterSceneObjectsStarted();
		m_pending = null;
	}

	private void LateUpdate()
	{
		CoreAssetSlotBinder.ApplyAnimatedNamedSpriteReplacements();
	}

	private void OnDisable()
	{
		CoreAssetSlotBinder.RestoreBaselineAssets();
	}
}
