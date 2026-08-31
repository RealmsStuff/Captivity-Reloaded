using System;
using System.Collections;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

public static class ExternalClothingFactory
{
	public static void Schedule(LibraryClothes i_library)
	{
		if (i_library == null || ModLoaderRuntime.ClothingDefinitions.Count == 0) return;
		ExternalClothingFactoryHost host = i_library.GetComponent<ExternalClothingFactoryHost>();
		if (host == null) host = i_library.gameObject.AddComponent<ExternalClothingFactoryHost>();
		host.Begin(i_library);
	}

	internal static void Build(LibraryClothes i_library)
	{
		Dictionary<string, ModPack> packs = new Dictionary<string, ModPack>(StringComparer.Ordinal);
		foreach (ModPack pack in ModLoaderRuntime.LoadedPacks)
			if (pack?.Manifest != null) packs[pack.Manifest.Id] = pack;

		int runtimeId = -2;
		foreach (ClothingDefinition definition in ModLoaderRuntime.ClothingDefinitions)
		{
			if (ModLoaderRuntime.Registry.TryGet(definition.Id, out ContentRegistration existing) && existing.RuntimeAsset != null) continue;
			if (!ModLoaderRuntime.Registry.TryGet(definition.Extends, out ContentRegistration baseEntry) || !(baseEntry.RuntimeAsset is Clothing template))
			{
				Report("clothing.factory-template", "Core clothing template is not bound: " + definition.Extends, definition.Source);
				continue;
			}
			if (!packs.TryGetValue(definition.PackId, out ModPack pack))
			{
				Report("clothing.factory-pack", "Clothing pack is not loaded: " + definition.PackId, definition.Source);
				continue;
			}

			Clothing clone = UnityEngine.Object.Instantiate(template, i_library.transform);
			clone.gameObject.name = definition.Id.ToString();
			clone.gameObject.SetActive(false);
			clone.Initialize();
			clone.SetId(runtimeId--);
			RuntimeContentIdentity identity = clone.GetComponent<RuntimeContentIdentity>();
			if (identity == null) identity = clone.gameObject.AddComponent<RuntimeContentIdentity>();
			identity.Configure(definition.Id, ContentCategory.Clothing);
			if (!ApplyAtlas(clone, definition, pack.RootPath))
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			i_library.AddRuntimeClothing(clone);
			ModLoaderRuntime.Registry.BindRuntimeAsset(definition.Id, clone, ModLoaderRuntime.LastReport);
			Debug.Log("[ModLoader] Built external clothing " + definition.Id + " from " + definition.Extends + ".");
		}
	}

	private static bool ApplyAtlas(Clothing i_clone, ClothingDefinition i_definition, string i_packRoot)
	{
		if (!RuntimePngAssetLoader.TryLoad(i_packRoot, i_definition.Visual.Atlas, i_definition.Id + "/atlas",
			FilterMode.Point, ModLoaderRuntime.LastReport, "clothing.factory-atlas", "clothing.factory-atlas-decode",
			i_definition.Source, out Texture2D atlas)) return false;

		Dictionary<string, Sprite> baselines = BuildSlotMap(i_clone);
		foreach (KeyValuePair<string, AtlasRegionDefinition> region in i_definition.Visual.Regions)
		{
			if (!baselines.TryGetValue(region.Key, out Sprite baseline) || baseline == null)
			{
				Report("clothing.factory-region", "Core clothing template does not expose region: " + region.Key, i_definition.Source);
				UnityEngine.Object.Destroy(atlas);
				return false;
			}
			AtlasRegionDefinition area = region.Value;
			Rect rect = new Rect(area.X, area.Y, area.Width, area.Height);
			if (rect.xMax > atlas.width || rect.yMax > atlas.height)
			{
				Report("clothing.factory-region-bounds", "Atlas region is outside the PNG: " + region.Key, i_definition.Source);
				UnityEngine.Object.Destroy(atlas);
				return false;
			}
			Vector2 pivot = new Vector2(baseline.pivot.x / baseline.rect.width, baseline.pivot.y / baseline.rect.height);
			Sprite sprite = Sprite.Create(atlas, rect, pivot, i_definition.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect, baseline.border);
			sprite.name = i_definition.Id + "/" + region.Key;
			if (region.Key == "icon") i_clone.SetIcon(sprite);
			else FindPiece(i_clone, region.Key).GetComponent<SpriteRenderer>().sprite = sprite;
		}
		return true;
	}

	private static Dictionary<string, Sprite> BuildSlotMap(Clothing i_clothing)
	{
		Dictionary<string, Sprite> slots = new Dictionary<string, Sprite>(StringComparer.Ordinal);
		if (i_clothing.GetIcon() != null) slots["icon"] = i_clothing.GetIcon();
		foreach (ClothingPiece piece in i_clothing.GetClothingPieces())
		{
			SpriteRenderer renderer = piece.GetComponent<SpriteRenderer>();
			if (renderer == null || renderer.sprite == null) continue;
			string key = ClothingSlotCatalog.FromCorePieceName(piece.name);
			if (!slots.ContainsKey(key)) slots.Add(key, renderer.sprite);
		}
		return slots;
	}

	private static ClothingPiece FindPiece(Clothing i_clothing, string i_key)
	{
		foreach (ClothingPiece piece in i_clothing.GetClothingPieces())
			if (ClothingSlotCatalog.FromCorePieceName(piece.name) == i_key) return piece;
		return null;
	}

	private static void Report(string i_code, string i_message, string i_source)
	{
		ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, i_code, i_message, i_source);
		Debug.LogError("[ModLoader] " + i_code + ": " + i_message + " [" + i_source + "]");
	}
}

public sealed class ExternalClothingFactoryHost : MonoBehaviour
{
	private bool m_started;

	public void Begin(LibraryClothes i_library)
	{
		if (m_started) return;
		m_started = true;
		StartCoroutine(BuildAfterLibraryAwake(i_library));
	}

	private IEnumerator BuildAfterLibraryAwake(LibraryClothes i_library)
	{
		yield return null;
		ExternalClothingFactory.Build(i_library);
	}
}
