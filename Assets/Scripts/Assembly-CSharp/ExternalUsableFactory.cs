using System;
using System.Collections;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

public static class ExternalUsableFactory
{
	public static void Schedule(LibraryUsables i_library)
	{
		if (i_library == null || ModLoaderRuntime.UsableDefinitions.Count == 0) return;
		ExternalUsableFactoryHost host = ExternalFactoryRunner.GetOrAdd<ExternalUsableFactoryHost>();
		host.Begin(i_library);
	}

	internal static void Build(LibraryUsables i_library)
	{
		Dictionary<string, ModPack> packs = new Dictionary<string, ModPack>(StringComparer.Ordinal);
		foreach (ModPack pack in ModLoaderRuntime.LoadedPacks)
			if (pack?.Manifest != null) packs[pack.Manifest.Id] = pack;

		foreach (UsableDefinition definition in ModLoaderRuntime.UsableDefinitions)
		{
			if (ModLoaderRuntime.Registry.TryGet(definition.Id, out ContentRegistration existing) && existing.RuntimeAsset != null) continue;
			if (!packs.TryGetValue(definition.PackId, out ModPack pack))
			{
				Report("usable.factory-pack", "Usable pack is not loaded: " + definition.PackId, definition.Source);
				continue;
			}

			Usable clone;
			if (definition.HasBaseTemplate)
			{
				if (!ModLoaderRuntime.Registry.TryGet(definition.Extends, out ContentRegistration baseEntry) || !(baseEntry.RuntimeAsset is Usable template))
				{
					Report("usable.factory-template", "Core usable template is not bound: " + definition.Extends, definition.Source);
					continue;
				}
				clone = UnityEngine.Object.Instantiate(template, i_library.transform);
			}
			else
			{
				clone = CreateOriginal(definition, i_library.transform);
			}
			clone.gameObject.name = definition.Id.ToString();
			clone.gameObject.SetActive(false);
			clone.ConfigureModItem(definition.DisplayName, definition.Description);
			clone.ConfigureModUsable(definition.Stats, definition.EffectMode, definition.Effects, !definition.HasBaseTemplate);
			RuntimeContentIdentity identity = clone.GetComponent<RuntimeContentIdentity>();
			if (identity == null) identity = clone.gameObject.AddComponent<RuntimeContentIdentity>();
			identity.Configure(definition.Id, ContentCategory.Item);
			if (!ApplySprites(clone, definition, pack.RootPath))
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			i_library.AddRuntimeUsable(clone);
			ModLoaderRuntime.Registry.BindRuntimeAsset(definition.Id, clone, ModLoaderRuntime.LastReport);
			Debug.Log("[ModLoader] Built external usable " + definition.Id +
				(definition.HasBaseTemplate ? " from " + definition.Extends + "." : " without a Core prefab."));
		}
	}

	private static Usable CreateOriginal(UsableDefinition i_definition, Transform i_parent)
	{
		GameObject gameObject = new GameObject(i_definition.Id.ToString());
		gameObject.SetActive(false);
		gameObject.transform.SetParent(i_parent, false);
		SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
		renderer.sortingLayerName = "Item";
		renderer.sortingOrder = i_definition.Visual.SortingOrder;
		Rigidbody2D body = gameObject.AddComponent<Rigidbody2D>();
		body.gravityScale = 1f;
		BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
		collider.size = new Vector2(i_definition.Visual.ColliderWidth, i_definition.Visual.ColliderHeight);
		return gameObject.AddComponent<RuntimeModUsable>();
	}

	private static bool ApplySprites(Usable i_clone, UsableDefinition i_definition, string i_packRoot)
	{
		Sprite iconBaseline = i_clone.GetSpriteIcon();
		SpriteRenderer worldRenderer = i_clone.GetComponent<SpriteRenderer>();
		bool original = !i_definition.HasBaseTemplate;
		if ((!original && iconBaseline == null) || worldRenderer == null || (!original && worldRenderer.sprite == null))
		{
			Report("usable.factory-sprite", "Core usable template does not expose icon and world sprites.", i_definition.Source);
			return false;
		}

		string worldPath = string.IsNullOrEmpty(i_definition.Visual.World) ? i_definition.Visual.Icon : i_definition.Visual.World;
		Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>(StringComparer.Ordinal);
		if (!TryLoadTexture(i_packRoot, i_definition.Visual.Icon, i_definition, textures, out Texture2D iconTexture) ||
			!TryLoadTexture(i_packRoot, worldPath, i_definition, textures, out Texture2D worldTexture))
		{
			foreach (Texture2D texture in textures.Values) UnityEngine.Object.Destroy(texture);
			return false;
		}

		Sprite icon = original
			? CreateOriginalSprite(iconTexture, i_definition, i_definition.Id + "/icon")
			: CreateSprite(iconTexture, iconBaseline, i_definition.Visual.PixelsPerUnit, i_definition.Id + "/icon");
		Sprite world = original
			? CreateOriginalSprite(worldTexture, i_definition, i_definition.Id + "/world")
			: CreateSprite(worldTexture, worldRenderer.sprite, i_definition.Visual.PixelsPerUnit, i_definition.Id + "/world");
		i_clone.SetModItemIcon(icon);
		worldRenderer.sprite = world;
		return true;
	}

	private static bool TryLoadTexture(string i_packRoot, string i_path, UsableDefinition i_definition,
		Dictionary<string, Texture2D> io_textures, out Texture2D o_texture)
	{
		if (io_textures.TryGetValue(i_path, out o_texture)) return true;
		if (!RuntimePngAssetLoader.TryLoad(i_packRoot, i_path, i_definition.Id + "/" + i_path,
			FilterMode.Point, ModLoaderRuntime.LastReport, "usable.factory-file", "usable.factory-decode",
			i_definition.Source, out o_texture)) return false;
		io_textures.Add(i_path, o_texture);
		return true;
	}

	private static Sprite CreateSprite(Texture2D i_texture, Sprite i_baseline, float i_pixelsPerUnit, string i_name)
	{
		Vector2 pivot = RuntimePngAssetLoader.GetReplacementPivot(i_baseline, i_texture.width, i_texture.height);
		Sprite sprite = Sprite.Create(i_texture, new Rect(0, 0, i_texture.width, i_texture.height), pivot,
			i_pixelsPerUnit, 0, SpriteMeshType.FullRect, i_baseline.border);
		sprite.name = i_name;
		return sprite;
	}

	private static Sprite CreateOriginalSprite(Texture2D i_texture, UsableDefinition i_definition, string i_name)
	{
		Sprite sprite = Sprite.Create(i_texture, new Rect(0, 0, i_texture.width, i_texture.height),
			new Vector2(i_definition.Visual.PivotX, i_definition.Visual.PivotY),
			i_definition.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect);
		sprite.name = i_name;
		return sprite;
	}

	private static void Report(string i_code, string i_message, string i_source)
	{
		ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, i_code, i_message, i_source);
	}
}

public sealed class RuntimeModUsable : Usable
{
	protected override bool HandleUse(bool i_isAltFire)
	{
		return false;
	}
}

public sealed class ExternalUsableFactoryHost : MonoBehaviour
{
	private bool m_started;

	public void Begin(LibraryUsables i_library)
	{
		if (m_started) return;
		m_started = true;
		StartCoroutine(BuildAfterLibraryAwake(i_library));
	}

	private IEnumerator BuildAfterLibraryAwake(LibraryUsables i_library)
	{
		yield return null;
		ExternalUsableFactory.Build(i_library);
	}
}
