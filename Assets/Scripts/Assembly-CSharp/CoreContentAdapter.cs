using CaptivityReloaded.Modding;
using UnityEngine;

public static class CoreContentAdapter
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void BindCoreContent()
	{
		Library[] libraries = Resources.FindObjectsOfTypeAll<Library>();
		ManagerStages[] stageManagers = Resources.FindObjectsOfTypeAll<ManagerStages>();
		ManagerChallenge[] challengeManagers = Resources.FindObjectsOfTypeAll<ManagerChallenge>();
		Library library = FindSceneObject(libraries);
		ManagerStages stageManager = FindSceneObject(stageManagers);
		ManagerChallenge challengeManager = FindSceneObject(challengeManagers);
		if (library == null || stageManager == null) return;

		foreach (CoreContentCatalogEntry entry in ModLoaderRuntime.CoreContentCatalog)
		{
			Object runtimeAsset = Resolve(entry, library, stageManager, challengeManager);
			if (runtimeAsset == null)
			{
				string selector = entry.LegacyId.HasValue ? "legacy ID " + entry.LegacyId.Value : "legacy name '" + entry.LegacyName + "'";
				ModLoaderRuntime.LastReport.Add(ValidationSeverity.Warning, "adapter.core-missing", "Core " + entry.Category + " " + selector + " could not be resolved.", "core/catalog.json");
				continue;
			}

			ModLoaderRuntime.Registry.Register(new ContentRegistration(entry.Id, entry.Category, "core", "core/catalog.json", runtimeAsset), ModLoaderRuntime.LastReport);
		}

		Debug.Log("[ModLoader] Bound " + ModLoaderRuntime.Registry.Count + " packaged Core gameplay entries.");
	}

	private static T FindSceneObject<T>(T[] i_objects) where T : Component
	{
		foreach (T item in i_objects)
		{
			if (item != null && item.gameObject.scene.IsValid()) return item;
		}
		return null;
	}

	private static Object Resolve(CoreContentCatalogEntry i_entry, Library i_library, ManagerStages i_stageManager, ManagerChallenge i_challengeManager)
	{
		if (i_entry.Category == ContentCategory.Enemy)
		{
			return i_library.Actors == null || !i_entry.LegacyId.HasValue ? null : i_library.Actors.GetNpc(i_entry.LegacyId.Value);
		}
		if (i_entry.Category == ContentCategory.Stage)
		{
			foreach (Stage stage in i_stageManager.GetAllStages())
			{
				if (i_entry.LegacyId.HasValue && stage.GetId() == i_entry.LegacyId.Value) return stage;
			}
		}
		if (i_entry.Category == ContentCategory.Clothing)
		{
			return i_library.Clothes == null || !i_entry.LegacyId.HasValue ? null : i_library.Clothes.GetClothing(i_entry.LegacyId.Value);
		}
		if (i_entry.Category == ContentCategory.Challenge && i_entry.LegacyId.HasValue && i_challengeManager != null)
		{
			foreach (Challenge challenge in i_challengeManager.GetAllChallenges())
			{
				if (challenge.GetId() == i_entry.LegacyId.Value) return challenge;
			}
		}
		if (i_entry.Category == ContentCategory.Item)
		{
			Gun gun = i_library.Guns == null ? null : i_library.Guns.GetGun(i_entry.LegacyName);
			if (gun != null) return gun;
			if (i_library.Usables != null)
			{
				foreach (GameObject usableObject in i_library.Usables.GetAllUsables())
				{
					Usable usable = usableObject.GetComponent<Usable>();
					if (usable != null && usable.GetName() == i_entry.LegacyName) return usable;
				}
			}
			if (i_library.Items != null)
			{
				AmmoBox ammoBox = i_library.Items.GetAmmoBoxTemplate();
				if (ammoBox != null && ammoBox.GetName() == i_entry.LegacyName) return ammoBox;
			}
		}
		return null;
	}
}
