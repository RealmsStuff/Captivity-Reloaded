using System;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CoreAssetSlotBinder
{
	private static readonly ContentId PistolOwnerId = ContentId.Parse("core:item/weapon/pistol");

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
