using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CaptivityReloaded.Editor.Modding
{
	/// <summary>Extracts stable source objects from the shipped stages into two hidden Resources prefabs.</summary>
	public static class ModStageTemplateGenerator
	{
		private const string Folder = "Assets/Resources/Modding/StageTemplates";
		private const string ShellPath = Folder + "/stage_mod_shell.prefab";
		private const string ObjectsPath = Folder + "/stage_mod_objects.prefab";

		[InitializeOnLoadMethod]
		private static void GenerateInitialAssetsWhenMissing()
		{
			if (!NeedsGeneration()) return;
			EditorApplication.delayCall += TryGenerateInitialAssets;
			EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
			EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
		}

		private static void HandlePlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.EnteredEditMode) TryGenerateInitialAssets();
		}

		private static void TryGenerateInitialAssets()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode) return;
			EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
			Generate(overwrite: true);
		}

		private static bool NeedsGeneration()
		{
			GameObject shell = AssetDatabase.LoadAssetAtPath<GameObject>(ShellPath);
			GameObject objects = AssetDatabase.LoadAssetAtPath<GameObject>(ObjectsPath);
			if (shell == null || objects == null) return true;
			ModStageTemplateLibrary library = objects.GetComponent<ModStageTemplateLibrary>();
			return library == null || library.StageShell == null || library.WeaponCase == null
				|| library.WeaponVendor == null || library.UsableVendor == null || library.StandardDoor == null
				|| library.RollerDoor == null || library.JackyDoor == null || library.Switch == null
				|| library.FuseBox == null || library.LightBulb == null || library.Note == null
				|| library.Keypad == null || library.Altar == null;
		}

		[MenuItem("Tools/Captivity Modding/Regenerate shared stage templates")]
		public static void Regenerate()
		{
			Generate(overwrite: true);
		}

		internal static void EnsureGenerated()
		{
			Generate(overwrite: false);
		}

		private static void Generate(bool overwrite)
		{
			EnsureFolder("Assets/Resources/Modding", "StageTemplates");
			if (overwrite || AssetDatabase.LoadAssetAtPath<GameObject>(ShellPath) == null) BuildShell();
			if (overwrite || AssetDatabase.LoadAssetAtPath<GameObject>(ObjectsPath) == null) BuildObjectLibrary();
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			ModStageTemplateLibrary.ClearCache();
			Debug.Log("[Modding] Shared stage shell and object-template prefabs are ready.");
		}

		private static void BuildShell()
		{
			GameObject root = new GameObject("stage_mod_shell");
			root.SetActive(false);
			try
			{
				Stage stage = root.AddComponent<Stage>();
				stage.ConfigureRuntimeTemplate(-1);
				root.AddComponent<ManagerWave>();

				Transform actors = Child(root.transform, "Actors");
				Transform items = Child(root.transform, "Items");
				GameObject waypointObject = new GameObject("WaypointStart");
				waypointObject.layer = LayerMask.NameToLayer("Waypoint");
				waypointObject.transform.SetParent(root.transform, false);
				Waypoint waypoint = waypointObject.AddComponent<Waypoint>();

				GameObject navObject = new GameObject("NavMap");
				navObject.transform.SetParent(root.transform, false);
				navObject.AddComponent<NavMap>();
				Child(navObject.transform, "Nodes");

				Light2D light = CloneGlobalLight(root.transform);

				Child(root.transform, "RuntimeLayout");
				SerializedObject serialized = new SerializedObject(stage);
				serialized.FindProperty("m_nameStage").stringValue = "Runtime stage shell";
				serialized.FindProperty("m_description").stringValue = "Hidden runtime template";
				serialized.FindProperty("m_waypointStart").objectReferenceValue = waypoint;
				serialized.FindProperty("m_lightGlobal").objectReferenceValue = light;
				serialized.FindProperty("m_actorsParent").objectReferenceValue = actors;
				serialized.FindProperty("m_itemsParent").objectReferenceValue = items;
				serialized.ApplyModifiedPropertiesWithoutUndo();
				PrefabUtility.SaveAsPrefabAsset(root, ShellPath);
			}
			finally { UnityEngine.Object.DestroyImmediate(root); }
		}

		private static void BuildObjectLibrary()
		{
			GameObject root = new GameObject("stage_mod_objects");
			root.SetActive(false);
			try
			{
				Stage stage = root.AddComponent<Stage>();
				stage.ConfigureRuntimeTemplate(-2);
				ModStageTemplateLibrary library = root.AddComponent<ModStageTemplateLibrary>();

				WeaponCase weaponCase = CloneFirst<WeaponCase>("Assets/Stages/stage_cave.prefab", root.transform, "weapon-case",
					item => item.IsUsableModTemplate());
				Vendor weaponVendor = CloneFirst<Vendor>("Assets/Stages/stage_cave.prefab", root.transform, "weapon-vendor",
					item => item.GetVendorType() == VendorType.Weapons && item.IsUsableModTemplate());
				Vendor usableVendor = CloneFirst<Vendor>("Assets/Stages/stage_cave.prefab", root.transform, "usable-vendor",
					item => item.GetVendorType() == VendorType.Usables && item.IsUsableModTemplate());
				Door standardDoor = CloneFirst<Door>("Assets/Stages/stage_shack.prefab", root.transform, "standard-door",
					item => item.GetType() == typeof(Door) && item.IsUsableModTemplate());
				DoorRoller rollerDoor = CloneFirst<DoorRoller>("Assets/Stages/stage_spaceStation.prefab", root.transform, "roller-door",
					item => item.IsUsableModTemplate());
				JackyDoor jackyDoor = CloneFirst<JackyDoor>("Assets/Stages/stage_fer.prefab", root.transform, "jacky-door",
					item => item.IsUsableModTemplate());
				Switch doorSwitch = CloneFirst<Switch>("Assets/Stages/stage_hub.prefab", root.transform, "switch",
					item => item.GetType() == typeof(Switch) && item.IsUsableModTemplate());
				FuseBox fuseBox = CloneFuseBox("Assets/Stages/stage_fer.prefab", root.transform);
				LightBulb lightBulb = CloneFirst<LightBulb>("Assets/Stages/stage_cave.prefab", root.transform, "light-bulb",
					item => item.GetType() == typeof(LightBulb) && item.IsUsableModTemplate());
				Note note = CloneFirst<Note>("Assets/Stages/stage_hub.prefab", root.transform, "note", item => item.IsUsableModTemplate());
				Keypad keypad = CloneFirst<Keypad>("Assets/Stages/stage_hub.prefab", root.transform, "keypad", item => item.IsUsableModTemplate());
				Altar altar = CloneFirst<Altar>("Assets/Stages/stage_jungle.prefab", root.transform, "altar",
					item => item.HasUsableModTemplateStructure());
				if (altar != null)
				{
					altar.ConfigureModFetishes(LoadPickUpable("Fetish1"), LoadPickUpable("Fetish2"),
						LoadPickUpable("Fetish3"), LoadPickUpable("Fetish4"));
					if (!altar.IsUsableModTemplate()) Debug.LogWarning("[Modding] The shared altar is missing one or more fetish item assets.");
				}
				ButtonActivator activator = CloneFirst<Generator>("Assets/Stages/stage_cave.prefab", root.transform, "activator");
				PipeFire fireHazard = CloneFirst<PipeFire>("Assets/Stages/stage_spaceStation.prefab", root.transform, "fire-hazard");

				GameObject shellObject = AssetDatabase.LoadAssetAtPath<GameObject>(ShellPath);
				SerializedObject serialized = new SerializedObject(library);
				Set(serialized, "m_stageShell", shellObject == null ? null : shellObject.GetComponent<Stage>());
				Set(serialized, "m_weaponCase", weaponCase);
				Set(serialized, "m_weaponVendor", weaponVendor);
				Set(serialized, "m_usableVendor", usableVendor);
				Set(serialized, "m_standardDoor", standardDoor);
				Set(serialized, "m_rollerDoor", rollerDoor);
				Set(serialized, "m_jackyDoor", jackyDoor);
				Set(serialized, "m_switch", doorSwitch);
				Set(serialized, "m_fuseBox", fuseBox);
				Set(serialized, "m_lightBulb", lightBulb);
				Set(serialized, "m_note", note);
				Set(serialized, "m_keypad", keypad);
				Set(serialized, "m_altar", altar);
				Set(serialized, "m_activator", activator);
				Set(serialized, "m_fireHazard", fireHazard);
				serialized.ApplyModifiedPropertiesWithoutUndo();
				foreach (Transform child in root.transform) child.gameObject.SetActive(false);
				PrefabUtility.SaveAsPrefabAsset(root, ObjectsPath);
			}
			finally { UnityEngine.Object.DestroyImmediate(root); }
		}

		private static T CloneFirst<T>(string path, Transform parent, string name, Func<T, bool> predicate = null) where T : Component
		{
			GameObject sourceRoot = PrefabUtility.LoadPrefabContents(path);
			try
			{
				T source = sourceRoot.GetComponentsInChildren<T>(true).FirstOrDefault(item => predicate == null || predicate(item));
				if (source == null) { Debug.LogWarning("[Modding] No usable " + typeof(T).Name + " template in " + path + "."); return null; }
				GameObject clone = UnityEngine.Object.Instantiate(source.gameObject, parent);
				clone.name = name;
				clone.transform.localPosition = Vector3.zero;
				clone.transform.localRotation = Quaternion.identity;
				return clone.GetComponent<T>();
			}
			finally { PrefabUtility.UnloadPrefabContents(sourceRoot); }
		}

		private static FuseBox CloneFuseBox(string path, Transform parent)
		{
			GameObject sourceRoot = PrefabUtility.LoadPrefabContents(path);
			try
			{
				FuseBox source = sourceRoot.GetComponentsInChildren<FuseBox>(true).FirstOrDefault(item => item.IsUsableModTemplate());
				if (source == null) { Debug.LogWarning("[Modding] No usable FuseBox template in " + path + "."); return null; }
				SerializedObject sourceData = new SerializedObject(source);
				SpriteRenderer sourceIndicator = sourceData.FindProperty("m_sprRendererLightBulbToTurnOn").objectReferenceValue as SpriteRenderer;
				Light2D sourceLight = sourceData.FindProperty("m_lightLightBulb").objectReferenceValue as Light2D;

				GameObject holder = new GameObject("fuse-box");
				holder.transform.SetParent(parent, false);
				GameObject fuseObject = UnityEngine.Object.Instantiate(source.gameObject, holder.transform);
				fuseObject.name = "body";
				fuseObject.transform.localPosition = Vector3.zero;
				fuseObject.transform.localRotation = Quaternion.identity;
				FuseBox clone = fuseObject.GetComponent<FuseBox>();

				SpriteRenderer indicator = null;
				Light2D indicatorLight = null;
				if (sourceIndicator != null)
				{
					GameObject indicatorObject = UnityEngine.Object.Instantiate(sourceIndicator.gameObject, holder.transform);
					indicatorObject.name = "indicator";
					indicatorObject.transform.localPosition = source.transform.InverseTransformPoint(sourceIndicator.transform.position);
					indicator = indicatorObject.GetComponent<SpriteRenderer>();
					if (sourceLight != null && sourceLight.gameObject == sourceIndicator.gameObject)
						indicatorLight = indicatorObject.GetComponent<Light2D>();
				}
				if (sourceLight != null && indicatorLight == null)
				{
					GameObject lightObject = UnityEngine.Object.Instantiate(sourceLight.gameObject, holder.transform);
					lightObject.name = "indicator-light";
					lightObject.transform.localPosition = source.transform.InverseTransformPoint(sourceLight.transform.position);
					indicatorLight = lightObject.GetComponent<Light2D>();
				}
				clone.ConfigureModFuseBox(indicator, indicatorLight);
				return clone;
			}
			finally { PrefabUtility.UnloadPrefabContents(sourceRoot); }
		}

		private static Light2D CloneGlobalLight(Transform parent)
		{
			GameObject sourceRoot = PrefabUtility.LoadPrefabContents("Assets/Stages/stage_field.prefab");
			try
			{
				Stage sourceStage = sourceRoot.GetComponent<Stage>();
				Light2D source = sourceStage == null ? null : sourceStage.GetLightGlobal();
				GameObject lightObject;
				Light2D light;
				bool copiedSource = source != null;
				if (source != null)
				{
					lightObject = UnityEngine.Object.Instantiate(source.gameObject, parent);
					light = lightObject.GetComponent<Light2D>();
				}
				else
				{
					lightObject = new GameObject();
					lightObject.transform.SetParent(parent, false);
					light = lightObject.AddComponent<Light2D>();
				}
				lightObject.name = "GlobalLight";
				lightObject.transform.localPosition = Vector3.zero;
				lightObject.transform.localRotation = Quaternion.identity;
				if (!copiedSource) light.lightType = Light2D.LightType.Global;
				light.intensity = 1f;
				return light;
			}
			finally { PrefabUtility.UnloadPrefabContents(sourceRoot); }
		}

		private static Transform Child(Transform parent, string name)
		{
			GameObject child = new GameObject(name);
			child.transform.SetParent(parent, false);
			return child.transform;
		}

		private static PickUpable LoadPickUpable(string name)
		{
			GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Items/Etc/JungleStage/" + name + ".prefab");
			return asset == null ? null : asset.GetComponent<PickUpable>();
		}

		private static void Set(SerializedObject serialized, string name, UnityEngine.Object value)
		{
			SerializedProperty property = serialized.FindProperty(name);
			if (property != null) property.objectReferenceValue = value;
		}

		private static void EnsureFolder(string parent, string child)
		{
			string path = parent + "/" + child;
			if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
		}
	}

	internal sealed class ModStageTemplateBuildProcessor : IPreprocessBuildWithReport
	{
		public int callbackOrder => -1000;

		public void OnPreprocessBuild(BuildReport report)
		{
			ModStageTemplateGenerator.EnsureGenerated();
		}
	}
}
