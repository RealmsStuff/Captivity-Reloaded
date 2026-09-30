using System;
using System.IO;
using CaptivityReloaded.Modding;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AdditiveNerfBundleExampleGenerator
{
	private const string Root = "Assets/ModAuthoring/Examples/AdditiveNerfPistol";
	private const string PrefabPath = Root + "/AdditiveNerfPistol.prefab";
	private const string ProfilePath = Root + "/AdditiveNerfPistolProfile.asset";
	private const string SessionKey = "CaptivityReloaded.GeneratedAdditiveNerfBundleExample";

	static AdditiveNerfBundleExampleGenerator()
	{
		if (!SessionState.GetBool(SessionKey, false)) EditorApplication.delayCall += GenerateOnce;
	}

	[MenuItem("Captivity Reloaded/Modding/Examples/Create or Refresh Additive Nerf Bundle Example")]
	public static void Generate()
	{
		Directory.CreateDirectory(Root);
		string[] textureNames = { "base", "body", "slide", "muzzle-flash", "dart-casing" };
		foreach (string name in textureNames) ConfigureTexture(Root + "/Textures/" + name + ".png");

		GameObject root = new GameObject("Additive Nerf Pistol Bundle Visual");
		try
		{
			CapmodPrefabDescriptor descriptor = root.AddComponent<CapmodPrefabDescriptor>();
			SerializedObject serialized = new SerializedObject(descriptor);
			serialized.FindProperty("m_id").stringValue = "somescrub.additive-nerf-pistol:prefab/weapon/nerf-pistol";
			serialized.FindProperty("m_kind").enumValueIndex = (int)CapmodPrefabKind.Prop;
			serialized.FindProperty("m_description").stringValue =
				"Unity-authored presentation for the additive Nerf pistol example.";
			serialized.ApplyModifiedPropertiesWithoutUndo();

			CreateSpritePart(root.transform, "base", 0, true);
			CreateSpritePart(root.transform, "body", 1, true);
			CreateSpritePart(root.transform, "slide", 2, true);
			GameObject effects = new GameObject("effects");
			effects.transform.SetParent(root.transform, false);
			CreateSpritePart(effects.transform, "muzzle-flash", 3, false);
			CreateSpritePart(effects.transform, "dart-casing", 3, false);
			effects.SetActive(false);

			PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
		}
		finally { UnityEngine.Object.DestroyImmediate(root); }

		GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
		CaptivityModAuthoringProfile profile = AssetDatabase.LoadAssetAtPath<CaptivityModAuthoringProfile>(ProfilePath);
		if (profile == null)
		{
			profile = ScriptableObject.CreateInstance<CaptivityModAuthoringProfile>();
			AssetDatabase.CreateAsset(profile, ProfilePath);
		}
		SerializedObject profileData = new SerializedObject(profile);
		profileData.FindProperty("sourceDirectory").stringValue = "ExampleMods/additive-nerf-pistol";
		SerializedProperty prefabs = profileData.FindProperty("prefabs");
		prefabs.arraySize = 1;
		prefabs.GetArrayElementAtIndex(0).objectReferenceValue = prefab;
		profileData.FindProperty("buildWindows").boolValue = true;
		profileData.FindProperty("buildAndroid").boolValue = true;
		profileData.FindProperty("buildWebGl").boolValue = true;
		profileData.ApplyModifiedPropertiesWithoutUndo();
		EditorUtility.SetDirty(profile);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Debug.Log("[ModPackager] Additive Nerf Unity bundle example is ready at " + ProfilePath);
	}

	private static void GenerateOnce()
	{
		SessionState.SetBool(SessionKey, true);
		if (!File.Exists(PrefabPath) || !File.Exists(ProfilePath)) Generate();
	}

	private static void ConfigureTexture(string i_path)
	{
		AssetDatabase.ImportAsset(i_path, ImportAssetOptions.ForceSynchronousImport);
		TextureImporter importer = AssetImporter.GetAtPath(i_path) as TextureImporter;
		if (importer == null) throw new InvalidOperationException("Could not import Nerf example texture: " + i_path);
		importer.textureType = TextureImporterType.Sprite;
		importer.spriteImportMode = SpriteImportMode.Single;
		importer.spritePixelsPerUnit = 32f;
		importer.mipmapEnabled = false;
		importer.alphaIsTransparency = true;
		importer.filterMode = FilterMode.Point;
		importer.textureCompression = TextureImporterCompression.Uncompressed;
		importer.SaveAndReimport();
	}

	private static void CreateSpritePart(Transform i_parent, string i_name, int i_sortingOrder, bool i_active)
	{
		Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Textures/" + i_name + ".png");
		if (sprite == null) throw new InvalidOperationException("Nerf example sprite did not import: " + i_name);
		GameObject part = new GameObject(i_name);
		part.transform.SetParent(i_parent, false);
		SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
		renderer.sprite = sprite;
		renderer.sortingOrder = i_sortingOrder;
		part.SetActive(i_active);
	}
}
