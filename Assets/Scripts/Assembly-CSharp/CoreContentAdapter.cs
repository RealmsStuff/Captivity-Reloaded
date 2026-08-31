using CaptivityReloaded.Modding;
using UnityEngine;

public static class CoreContentAdapter
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void BindCoreContent()
	{
		Library[] libraries = Resources.FindObjectsOfTypeAll<Library>();
		ManagerStages[] stageManagers = Resources.FindObjectsOfTypeAll<ManagerStages>();
		Library library = FindSceneObject(libraries);
		ManagerStages stageManager = FindSceneObject(stageManagers);
		if (library == null || stageManager == null) return;

		foreach (CoreContentCatalogEntry entry in ModLoaderRuntime.CoreContentCatalog)
		{
			Object runtimeAsset = Resolve(entry, library, stageManager);
			if (runtimeAsset == null)
			{
				ModLoaderRuntime.LastReport.Add(ValidationSeverity.Warning, "adapter.core-missing", "Core " + entry.Category + " legacy ID " + entry.LegacyId + " could not be resolved.", "core/catalog.json");
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

	private static Object Resolve(CoreContentCatalogEntry i_entry, Library i_library, ManagerStages i_stageManager)
	{
		if (i_entry.Category == ContentCategory.Enemy)
		{
			return i_library.Actors == null ? null : i_library.Actors.GetNpc(i_entry.LegacyId);
		}
		if (i_entry.Category == ContentCategory.Stage)
		{
			foreach (Stage stage in i_stageManager.GetAllStages())
			{
				if (stage.GetId() == i_entry.LegacyId) return stage;
			}
		}
		return null;
	}
}
