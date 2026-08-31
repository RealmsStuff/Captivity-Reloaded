using System;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CoreAssetSlotBinder
{
	private static readonly ContentId PistolOwnerId = ContentId.Parse("core:item/weapon/pistol");
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
		Library[] libraries = Resources.FindObjectsOfTypeAll<Library>();
		if (libraries.Length == 0 || libraries[0].Guns == null) return;
		Gun pistol = libraries[0].Guns.GetGun("Pistol");
		if (pistol == null) return;

		RegisterSpriteSlot("core:weapon/pistol/body", FindDirectChild(pistol.transform, "bullet"), "Pistol main body renderer");
		RegisterSpriteSlot("core:weapon/pistol/slide", FindDirectChild(pistol.transform, "slide"), "Pistol slide renderer");
		RegisterSpriteSlot("core:weapon/pistol/base", FindDirectChild(pistol.transform, "base"), "Pistol base renderer");

		if (!m_externalPatchesApplied && HasPistolSlots())
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

	private static bool HasPistolSlots()
	{
		return ModLoaderRuntime.AssetSlots.TryGet(ContentId.Parse("core:weapon/pistol/body"), out _)
			&& ModLoaderRuntime.AssetSlots.TryGet(ContentId.Parse("core:weapon/pistol/slide"), out _)
			&& ModLoaderRuntime.AssetSlots.TryGet(ContentId.Parse("core:weapon/pistol/base"), out _);
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
		if (ModLoaderRuntime.AssetSlots.TryGet(slotId, out _)) return;

		AssetSlotRegistration registration = new AssetSlotRegistration(
			slotId,
			PistolOwnerId,
			"core",
			i_source,
			renderer.sprite,
			typeof(Sprite),
			asset => renderer.sprite = (Sprite)asset);
		ModLoaderRuntime.AssetSlots.Register(registration, ModLoaderRuntime.LastReport);
	}
}
