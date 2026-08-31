using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

	public static void Schedule(LibraryActors i_library)
	{
		if (i_library == null || ModLoaderRuntime.EnemyDefinitions.Count == 0) return;
		ExternalEnemyFactoryHost host = i_library.GetComponent<ExternalEnemyFactoryHost>();
		if (host == null) host = i_library.gameObject.AddComponent<ExternalEnemyFactoryHost>();
		host.Begin(i_library);
	}

	internal static void Build(LibraryActors i_library)
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
			clone.ConfigureModEnemy(definition.DisplayName, definition.Description, definition.Stats);
			if (!ApplyAtlas(clone, definition, pack.RootPath))
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			ModLoaderRuntime.Registry.BindRuntimeAsset(definition.Id, clone, ModLoaderRuntime.LastReport);
			Debug.Log("[ModLoader] Built external enemy " + definition.Id + " from " + definition.Extends + ".");
		}
	}

	private static bool ApplyAtlas(NPC i_clone, EnemyDefinition i_definition, string i_packRoot)
	{
		string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(i_packRoot, i_definition.Visual.Atlas));
		if (!AssetPatchDiscovery.IsInside(path, i_packRoot) || !File.Exists(path))
		{
			Report("enemy.factory-atlas", "Enemy atlas is missing or outside its pack.", path);
			return false;
		}
		try
		{
			FileInfo file = new FileInfo(path);
			if (file.Length <= 0 || file.Length > 32 * 1024 * 1024) throw new InvalidDataException("Atlas size must be between 1 byte and 32 MiB.");
			Texture2D atlas = new Texture2D(2, 2, TextureFormat.RGBA32, false);
			if (!atlas.LoadImage(File.ReadAllBytes(path), false)) throw new InvalidDataException("Unity could not decode the enemy atlas.");
			atlas.name = i_definition.Id + "/atlas";
			atlas.filterMode = FilterMode.Point;

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
		catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is InvalidDataException)
		{
			Report("enemy.factory-atlas-decode", exception.Message, path);
			return false;
		}
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

	public void Begin(LibraryActors i_library)
	{
		if (m_started) return;
		m_started = true;
		StartCoroutine(BuildAfterLibraryStart(i_library));
	}

	private IEnumerator BuildAfterLibraryStart(LibraryActors i_library)
	{
		yield return null;
		ExternalEnemyFactory.Build(i_library);
	}
}
