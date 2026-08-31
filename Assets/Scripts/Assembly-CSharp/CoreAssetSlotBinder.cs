using System;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CoreAssetSlotBinder
{
	private static readonly ContentId PistolOwnerId = ContentId.Parse("core:item/weapon/pistol");
	private static readonly ContentId PlayerOwnerId = ContentId.Parse("core:player");
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
	private static bool m_externalPatchesApplied;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Initialize()
	{
		BindAvailableSlots();
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private static void OnSceneLoaded(Scene i_scene, LoadSceneMode i_mode)
	{
		BindAvailableSlots();
	}

	private static void BindAvailableSlots()
	{
		BindPlayerBodySlots();
		Library[] libraries = Resources.FindObjectsOfTypeAll<Library>();
		if (libraries.Length != 0 && libraries[0].Guns != null)
		{
			Gun pistol = libraries[0].Guns.GetGun("Pistol");
			if (pistol != null)
			{
				RegisterSpriteSlot("core:weapon/pistol/body", FindDirectChild(pistol.transform, "bullet"), "Pistol main body renderer");
				RegisterSpriteSlot("core:weapon/pistol/slide", FindDirectChild(pistol.transform, "slide"), "Pistol slide renderer");
				RegisterSpriteSlot("core:weapon/pistol/base", FindDirectChild(pistol.transform, "base"), "Pistol base renderer");
			}
		}

		if (!m_externalPatchesApplied && HasPistolSlots() && HasPlayerBodySlots())
		{
			int issueStart = ModLoaderRuntime.LastReport.Issues.Count;
			List<AssetPatchRequest> patches = RuntimeSpritePatchLoader.Load(
				ModLoaderRuntime.AssetPatches, ModLoaderRuntime.LoadedPacks, ModLoaderRuntime.AssetSlots, ModLoaderRuntime.LastReport);
			ModLoaderRuntime.AssetSlots.Resolve(patches, ModLoaderRuntime.LastReport);
			for (int index = issueStart; index < ModLoaderRuntime.LastReport.Issues.Count; index++)
			{
				ValidationIssue issue = ModLoaderRuntime.LastReport.Issues[index];
				if (issue.Severity == ValidationSeverity.Error) Debug.LogError("[ModLoader] " + issue);
				else if (issue.Severity == ValidationSeverity.Warning) Debug.LogWarning("[ModLoader] " + issue);
				else Debug.Log("[ModLoader] " + issue);
			}
			Debug.Log("[ModLoader] Public asset slots=" + ModLoaderRuntime.AssetSlots.Count + ", runtime replacements=" + patches.Count + ".");
			m_externalPatchesApplied = true;
		}
	}

	private static void BindPlayerBodySlots()
	{
		Dictionary<string, List<BodyPartPlayer>> parts = new Dictionary<string, List<BodyPartPlayer>>(StringComparer.Ordinal);
		foreach (BodyPartPlayer bodyPart in Resources.FindObjectsOfTypeAll<BodyPartPlayer>())
		{
			if (!PlayerPartNames.TryGetValue(bodyPart.name, out string semanticName)) continue;
			if (!parts.TryGetValue(semanticName, out List<BodyPartPlayer> matching))
			{
				matching = new List<BodyPartPlayer>();
				parts.Add(semanticName, matching);
			}
			matching.Add(bodyPart);
		}

		foreach (KeyValuePair<string, List<BodyPartPlayer>> group in parts)
		{
			foreach (SkinColor skinColor in SkinColors)
			{
				Sprite baseline = group.Value[0].GetSkinSprite(skinColor);
				if (baseline == null) continue;
				string colorName = skinColor.ToString().ToLowerInvariant();
				ContentId slotId = ContentId.Parse("core:player/body/" + group.Key + "/" + colorName);
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

	private static bool HasPistolSlots()
	{
		return ModLoaderRuntime.AssetSlots.TryGet(ContentId.Parse("core:weapon/pistol/body"), out _)
			&& ModLoaderRuntime.AssetSlots.TryGet(ContentId.Parse("core:weapon/pistol/slide"), out _)
			&& ModLoaderRuntime.AssetSlots.TryGet(ContentId.Parse("core:weapon/pistol/base"), out _);
	}

	private static bool HasPlayerBodySlots()
	{
		foreach (string part in new HashSet<string>(PlayerPartNames.Values, StringComparer.Ordinal))
		{
			foreach (SkinColor skinColor in SkinColors)
			{
				ContentId id = ContentId.Parse("core:player/body/" + part + "/" + skinColor.ToString().ToLowerInvariant());
				if (!ModLoaderRuntime.AssetSlots.TryGet(id, out _)) return false;
			}
		}
		return true;
	}

	private static Transform FindDirectChild(Transform i_parent, string i_name)
	{
		for (int index = 0; index < i_parent.childCount; index++)
		{
			Transform child = i_parent.GetChild(index);
			if (string.Equals(child.name, i_name, StringComparison.Ordinal)) return child;
		}
		return null;
	}

	private static void RegisterSpriteSlot(string i_id, Transform i_part, string i_source)
	{
		if (i_part == null) return;
		SpriteRenderer renderer = i_part.GetComponent<SpriteRenderer>();
		if (renderer == null || renderer.sprite == null) return;
		ContentId slotId = ContentId.Parse(i_id);
		Action<UnityEngine.Object> apply = asset => renderer.sprite = (Sprite)asset;
		if (ModLoaderRuntime.AssetSlots.TryGet(slotId, out AssetSlotRegistration existing))
		{
			existing.AddBinding(apply);
			return;
		}

		AssetSlotRegistration registration = new AssetSlotRegistration(
			slotId,
			PistolOwnerId,
			"core",
			i_source,
			renderer.sprite,
			typeof(Sprite),
			apply);
		ModLoaderRuntime.AssetSlots.Register(registration, ModLoaderRuntime.LastReport);
	}
}
