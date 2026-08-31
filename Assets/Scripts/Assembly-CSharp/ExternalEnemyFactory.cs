using System;
using System.Collections;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

public static class ExternalEnemyFactory
{
	private static readonly Dictionary<string, string[]> RigParts = new Dictionary<string, string[]>(StringComparer.Ordinal)
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
		{ "body/foot-right", new[] { "bp_rFoot" } }
	};

	public static void Schedule(LibraryActors i_library, ManagerStages i_stageManager)
	{
		if (i_library == null || ModLoaderRuntime.EnemyDefinitions.Count == 0) return;
		ExternalEnemyFactoryHost host = i_library.GetComponent<ExternalEnemyFactoryHost>();
		if (host == null) host = i_library.gameObject.AddComponent<ExternalEnemyFactoryHost>();
		host.Begin(i_library, i_stageManager);
	}

	internal static void Build(LibraryActors i_library, ManagerStages i_stageManager)
	{
		Dictionary<string, ModPack> packs = new Dictionary<string, ModPack>(StringComparer.Ordinal);
		foreach (ModPack pack in ModLoaderRuntime.LoadedPacks)
			if (pack?.Manifest != null) packs[pack.Manifest.Id] = pack;

		foreach (EnemyDefinition definition in ModLoaderRuntime.EnemyDefinitions)
		{
			if (ModLoaderRuntime.Registry.TryGet(definition.Id, out ContentRegistration existing) && existing.RuntimeAsset != null) continue;
			if (!ModLoaderRuntime.Registry.TryGet(definition.Extends, out ContentRegistration baseEntry) || !(baseEntry.RuntimeAsset is NPC template))
			{
				Report("enemy.factory-template", "Core enemy template is not bound: " + definition.Extends, definition.Source);
				continue;
			}
			if (!packs.TryGetValue(definition.PackId, out ModPack pack))
			{
				Report("enemy.factory-pack", "Enemy pack is not loaded: " + definition.PackId, definition.Source);
				continue;
			}

			NPC clone = UnityEngine.Object.Instantiate(template, i_library.transform);
			clone.gameObject.name = definition.Id.ToString();
			clone.gameObject.SetActive(false);
			RuntimeContentIdentity identity = clone.GetComponent<RuntimeContentIdentity>();
			if (identity == null) identity = clone.gameObject.AddComponent<RuntimeContentIdentity>();
			identity.Configure(definition.Id, ContentCategory.Enemy);
			clone.ConfigureModEnemy(definition.DisplayName, definition.Description, definition.Stats);
			if (!ApplyAtlas(clone, definition, pack.RootPath))
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			ModLoaderRuntime.Registry.BindRuntimeAsset(definition.Id, clone, ModLoaderRuntime.LastReport);
			int spawners = definition.Spawn.InheritTemplateSpawners ? AddToTemplateSpawners(i_stageManager, template, clone) : 0;
			Debug.Log("[ModLoader] Built external enemy " + definition.Id + " from " + definition.Extends + ".");
			if (definition.Spawn.InheritTemplateSpawners)
				Debug.Log("[ModLoader] Added external enemy " + definition.Id + " to " + spawners + " inherited stage spawners.");
		}
	}

	private static int AddToTemplateSpawners(ManagerStages i_stageManager, NPC i_template, NPC i_variant)
	{
		if (i_stageManager == null) return 0;
		int count = 0;
		foreach (Stage stage in i_stageManager.GetAllStages())
		{
			if (stage == null) continue;
			foreach (Spawner spawner in stage.GetComponentsInChildren<Spawner>(true))
				if (spawner.AddNpcVariant(i_template, i_variant)) count++;
		}
		return count;
	}

	private static bool ApplyAtlas(NPC i_clone, EnemyDefinition i_definition, string i_packRoot)
	{
		if (!RuntimePngAssetLoader.TryLoad(i_packRoot, i_definition.Visual.Atlas, i_definition.Id + "/atlas",
			FilterMode.Point, ModLoaderRuntime.LastReport, "enemy.factory-atlas", "enemy.factory-atlas-decode",
			i_definition.Source, out Texture2D atlas)) return false;

		foreach (KeyValuePair<string, AtlasRegionDefinition> region in i_definition.Visual.Regions)
		{
			if (!RigParts.TryGetValue(region.Key, out string[] objectNames))
			{
				Report("enemy.factory-region", "Core rig does not expose region: " + region.Key, i_definition.Source);
				UnityEngine.Object.Destroy(atlas);
				return false;
			}
			AtlasRegionDefinition area = region.Value;
			Rect rect = new Rect(area.X, area.Y, area.Width, area.Height);
			if (rect.xMax > atlas.width || rect.yMax > atlas.height)
			{
				Report("enemy.factory-region-bounds", "Atlas region is outside the PNG: " + region.Key, i_definition.Source);
				UnityEngine.Object.Destroy(atlas);
				return false;
			}
			foreach (string objectName in objectNames)
			{
				Transform part = FindRecursive(i_clone.transform, objectName);
				SpriteRenderer renderer = part == null ? null : part.GetComponent<SpriteRenderer>();
				if (renderer == null || renderer.sprite == null)
				{
					Report("enemy.factory-rig-part", "Template is missing required renderer: " + objectName, i_definition.Source);
					UnityEngine.Object.Destroy(atlas);
					return false;
				}
				Sprite baseline = renderer.sprite;
				Vector2 pivot = new Vector2(baseline.pivot.x / baseline.rect.width, baseline.pivot.y / baseline.rect.height);
				Sprite sprite = Sprite.Create(atlas, rect, pivot, i_definition.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect, baseline.border);
				sprite.name = i_definition.Id + "/" + region.Key;
				renderer.sprite = sprite;
			}
		}
		return true;
	}

	private static Transform FindRecursive(Transform i_root, string i_name)
	{
		if (string.Equals(i_root.name, i_name, StringComparison.Ordinal)) return i_root;
		for (int index = 0; index < i_root.childCount; index++)
		{
			Transform found = FindRecursive(i_root.GetChild(index), i_name);
			if (found != null) return found;
		}
		return null;
	}

	private static void Report(string i_code, string i_message, string i_source)
	{
		ValidationIssue issue = new ValidationIssue(ValidationSeverity.Error, i_code, i_message, i_source);
		ModLoaderRuntime.LastReport.Add(issue.Severity, issue.Code, issue.Message, issue.Source);
		Debug.LogError("[ModLoader] " + issue);
	}
}

public sealed class ExternalEnemyFactoryHost : MonoBehaviour
{
	private bool m_started;

	public void Begin(LibraryActors i_library, ManagerStages i_stageManager)
	{
		if (m_started) return;
		m_started = true;
		StartCoroutine(BuildAfterLibraryStart(i_library, i_stageManager));
	}

	private IEnumerator BuildAfterLibraryStart(LibraryActors i_library, ManagerStages i_stageManager)
	{
		yield return null;
		ExternalEnemyFactory.Build(i_library, i_stageManager);
	}
}
