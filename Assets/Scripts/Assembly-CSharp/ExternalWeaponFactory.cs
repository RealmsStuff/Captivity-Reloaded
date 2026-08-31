using System;
using System.Collections;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

public static class ExternalWeaponFactory
{
	private static readonly Dictionary<string, string> PistolParts = new Dictionary<string, string>(StringComparer.Ordinal)
	{
		{ "body", "bullet" },
		{ "slide", "slide" },
		{ "base", "base" }
	};

	public static void Schedule(LibraryGuns i_library)
	{
		if (i_library == null || ModLoaderRuntime.WeaponDefinitions.Count == 0) return;
		ExternalWeaponFactoryHost host = i_library.GetComponent<ExternalWeaponFactoryHost>();
		if (host == null) host = i_library.gameObject.AddComponent<ExternalWeaponFactoryHost>();
		host.Begin(i_library);
	}

	internal static void Build(LibraryGuns i_library)
	{
		Dictionary<string, ModPack> packs = new Dictionary<string, ModPack>(StringComparer.Ordinal);
		foreach (ModPack pack in ModLoaderRuntime.LoadedPacks)
			if (pack?.Manifest != null) packs[pack.Manifest.Id] = pack;

		foreach (WeaponDefinition definition in ModLoaderRuntime.WeaponDefinitions)
		{
			if (ModLoaderRuntime.Registry.TryGet(definition.Id, out ContentRegistration existing) && existing.RuntimeAsset != null) continue;
			if (!ModLoaderRuntime.Registry.TryGet(definition.Extends, out ContentRegistration baseEntry) || !(baseEntry.RuntimeAsset is Gun template))
			{
				Report("weapon.factory-template", "Core weapon template is not bound: " + definition.Extends, definition.Source);
				continue;
			}
			if (!packs.TryGetValue(definition.PackId, out ModPack pack))
			{
				Report("weapon.factory-pack", "Weapon pack is not loaded: " + definition.PackId, definition.Source);
				continue;
			}

			Gun clone = UnityEngine.Object.Instantiate(template, i_library.transform);
			clone.gameObject.name = definition.Id.ToString();
			clone.gameObject.SetActive(false);
			clone.ConfigureModItem(definition.DisplayName, definition.Description);
			RuntimeContentIdentity identity = clone.GetComponent<RuntimeContentIdentity>();
			if (identity == null) identity = clone.gameObject.AddComponent<RuntimeContentIdentity>();
			identity.Configure(definition.Id, ContentCategory.Item);
			if (!ApplySprites(clone, definition, pack.RootPath))
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			i_library.AddRuntimeGun(clone);
			ModLoaderRuntime.Registry.BindRuntimeAsset(definition.Id, clone, ModLoaderRuntime.LastReport);
			Debug.Log("[ModLoader] Built external weapon " + definition.Id + " from " + definition.Extends + ".");
		}
	}

	private static bool ApplySprites(Gun i_clone, WeaponDefinition i_definition, string i_packRoot)
	{
		List<Texture2D> textures = new List<Texture2D>();
		List<Sprite> sprites = new List<Sprite>();
		List<KeyValuePair<SpriteRenderer, Sprite>> replacements = new List<KeyValuePair<SpriteRenderer, Sprite>>();
		foreach (KeyValuePair<string, string> requested in i_definition.Visual.Sprites)
		{
			Transform part = FindRecursive(i_clone.transform, PistolParts[requested.Key]);
			SpriteRenderer renderer = part == null ? null : part.GetComponent<SpriteRenderer>();
			if (renderer == null || renderer.sprite == null)
			{
				Report("weapon.factory-slot", "Core pistol template does not expose sprite slot: " + requested.Key, i_definition.Source);
				DestroyCreated(textures, sprites);
				return false;
			}
			Sprite baseline = renderer.sprite;
			if (!RuntimePngAssetLoader.TryLoad(i_packRoot, requested.Value, i_definition.Id + "/" + requested.Key,
				baseline.texture.filterMode, ModLoaderRuntime.LastReport, "weapon.factory-file", "weapon.factory-decode",
				i_definition.Source, out Texture2D texture))
			{
				DestroyCreated(textures, sprites);
				return false;
			}
			textures.Add(texture);
			Vector2 pivot = new Vector2(baseline.pivot.x / baseline.rect.width, baseline.pivot.y / baseline.rect.height);
			Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot,
				i_definition.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect, baseline.border);
			sprite.name = i_definition.Id + "/" + requested.Key;
			sprites.Add(sprite);
			replacements.Add(new KeyValuePair<SpriteRenderer, Sprite>(renderer, sprite));
		}

		foreach (KeyValuePair<SpriteRenderer, Sprite> replacement in replacements)
			replacement.Key.sprite = replacement.Value;
		return true;
	}

	private static void DestroyCreated(IEnumerable<Texture2D> i_textures, IEnumerable<Sprite> i_sprites)
	{
		foreach (Sprite sprite in i_sprites) UnityEngine.Object.Destroy(sprite);
		foreach (Texture2D texture in i_textures) UnityEngine.Object.Destroy(texture);
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
		ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, i_code, i_message, i_source);
		Debug.LogError("[ModLoader] " + i_code + ": " + i_message + " [" + i_source + "]");
	}
}

public sealed class ExternalWeaponFactoryHost : MonoBehaviour
{
	private bool m_started;

	public void Begin(LibraryGuns i_library)
	{
		if (m_started) return;
		m_started = true;
		StartCoroutine(BuildAfterLibraryAwake(i_library));
	}

	private IEnumerator BuildAfterLibraryAwake(LibraryGuns i_library)
	{
		yield return null;
		ExternalWeaponFactory.Build(i_library);
	}
}
