using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CaptivityReloaded.Modding;
using UnityEngine;
using UnityEngine.Networking;

public static class ExternalWeaponFactory
{
	public static void Schedule(LibraryGuns i_library)
	{
		if (i_library == null || ModLoaderRuntime.WeaponDefinitions.Count == 0) return;
		ExternalWeaponFactoryHost host = ExternalFactoryRunner.GetOrAdd<ExternalWeaponFactoryHost>();
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
			if (!packs.TryGetValue(definition.PackId, out ModPack pack))
			{
				Report("weapon.factory-pack", "Weapon pack is not loaded: " + definition.PackId, definition.Source);
				continue;
			}

			Gun clone;
			if (definition.IsOriginal)
			{
				if (!TryCreateOriginalWeapon(i_library.transform, definition, pack.RootPath, out clone)) continue;
			}
			else
			{
				if (!ModLoaderRuntime.Registry.TryGet(definition.Extends.Value, out ContentRegistration baseEntry) || !(baseEntry.RuntimeAsset is Gun template))
				{
					Report("weapon.factory-template", "Core weapon template is not bound: " + definition.Extends.Value, definition.Source);
					continue;
				}
				clone = UnityEngine.Object.Instantiate(template, i_library.transform);
			}
			clone.gameObject.name = definition.Id.ToString();
			clone.gameObject.SetActive(false);
			clone.ConfigureModItem(definition.DisplayName, definition.Description);
			clone.ConfigureModWeaponStats(definition.Stats);
			RuntimeContentIdentity identity = clone.GetComponent<RuntimeContentIdentity>();
			if (identity == null) identity = clone.gameObject.AddComponent<RuntimeContentIdentity>();
			identity.Configure(definition.Id, ContentCategory.Item);
			if (!definition.IsOriginal && !ApplySprites(clone, definition, pack.RootPath))
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			if (!ApplyBehaviorAssets(clone, definition, pack.RootPath))
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			if (!ApplyAnimationAssets(clone, definition, pack.RootPath))
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			ExternalFactoryRunner.GetOrAdd<ModWeaponAudioLoader>().Load(clone, definition, pack.RootPath);
			i_library.AddRuntimeGun(clone);
			ModLoaderRuntime.Registry.BindRuntimeAsset(definition.Id, clone, ModLoaderRuntime.LastReport);
			Debug.Log("[ModLoader] Built external weapon " + definition.Id + (definition.IsOriginal ? " from JSON rig." : " from " + definition.Extends.Value + "."));
		}
	}

	private static bool TryCreateOriginalWeapon(Transform i_parent, WeaponDefinition i_definition, string i_packRoot, out Gun o_gun)
	{
		o_gun = null;
		WeaponVisualDefinition visual = i_definition.Visual;
		GameObject root = new GameObject(i_definition.Id.ToString());
		root.transform.SetParent(i_parent, false);
		int itemLayer = LayerMask.NameToLayer("Item");
		if (itemLayer >= 0) root.layer = itemLayer;
		Dictionary<string, Sprite> partSprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
		foreach (KeyValuePair<string, string> entry in visual.Sprites)
		{
			if (entry.Key == "icon") continue;
			if (!RuntimePngAssetLoader.TryLoad(i_packRoot, entry.Value, i_definition.Id + "/" + entry.Key,
				FilterMode.Point, ModLoaderRuntime.LastReport, "weapon.factory-file", "weapon.factory-decode",
				i_definition.Source, out Texture2D texture))
			{
				UnityEngine.Object.Destroy(root);
				return false;
			}
			WeaponVisualPartDefinition layout = null;
			visual.Parts?.TryGetValue(entry.Key, out layout);
			Vector2 pivot = new Vector2(layout?.PivotX ?? (entry.Key == "body" ? visual.PivotX ?? 0.15f : 0.5f),
				layout?.PivotY ?? (entry.Key == "body" ? visual.PivotY ?? 0.5f : 0.5f));
			Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot,
				visual.PixelsPerUnit, 0, SpriteMeshType.FullRect);
			sprite.name = i_definition.Id + "/" + entry.Key;
			partSprites[entry.Key] = sprite;

			GameObject part = new GameObject(entry.Key);
			part.transform.SetParent(root.transform, false);
			part.transform.localPosition = new Vector3(layout?.OffsetX ?? 0f, layout?.OffsetY ?? 0f, 0f);
			part.transform.localRotation = Quaternion.Euler(0f, 0f, layout?.RotationDegrees ?? 0f);
			part.transform.localScale = new Vector3(layout?.ScaleX ?? 1f, layout?.ScaleY ?? 1f, 1f);
			SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
			renderer.sprite = sprite;
			renderer.sortingLayerName = "Item";
			renderer.sortingOrder = layout?.SortingOrder ?? visual.SortingOrder ?? 0;
		}

		Sprite bodySprite = partSprites["body"];
		Sprite iconSprite = bodySprite;
		if (visual.Sprites.TryGetValue("icon", out string iconPath) && iconPath != visual.Sprites["body"])
		{
			if (!RuntimePngAssetLoader.TryLoad(i_packRoot, iconPath, i_definition.Id + "/icon", FilterMode.Point,
				ModLoaderRuntime.LastReport, "weapon.factory-file", "weapon.factory-decode", i_definition.Source, out Texture2D iconTexture))
			{
				UnityEngine.Object.Destroy(root);
				return false;
			}
			iconSprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f),
				visual.PixelsPerUnit, 0, SpriteMeshType.FullRect);
			iconSprite.name = i_definition.Id + "/icon";
		}
		Rigidbody2D rigidbody = root.AddComponent<Rigidbody2D>();
		rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
		collider.size = new Vector2(visual.ColliderWidth ?? Mathf.Max(0.02f, bodySprite.bounds.size.x * 0.8f),
			visual.ColliderHeight ?? Mathf.Max(0.02f, bodySprite.bounds.size.y * 0.8f));
		collider.offset = new Vector2(visual.ColliderOffsetX ?? 0f, visual.ColliderOffsetY ?? 0f);
		GameObject muzzle = new GameObject("muzzle");
		muzzle.transform.SetParent(root.transform, false);
		muzzle.transform.localPosition = new Vector3(visual.MuzzleOffsetX ?? bodySprite.bounds.max.x,
			visual.MuzzleOffsetY ?? 0f, 0f);
		o_gun = root.AddComponent<Gun>();
		o_gun.ConfigureOriginalWeaponRuntime(muzzle, iconSprite, i_definition.Stats);
		return true;
	}

	private static bool ApplyBehaviorAssets(Gun i_clone, WeaponDefinition i_definition, string i_packRoot)
	{
		WeaponBehaviorDefinition behavior = i_definition.Behavior;
		float pixelsPerUnit = behavior.EffectPixelsPerUnit ?? i_definition.Visual.PixelsPerUnit;
		List<Sprite> muzzle = behavior.MuzzleFlashSprites == null ? null : new List<Sprite>();
		if (behavior.MuzzleFlashSprites != null)
			foreach (string path in behavior.MuzzleFlashSprites)
			{
				if (!TryCreateEffectSprite(i_packRoot, path, pixelsPerUnit, i_definition, out Sprite sprite)) return false;
				muzzle.Add(sprite);
			}
		Sprite casing = null;
		if (!string.IsNullOrWhiteSpace(behavior.CasingSprite)
			&& !TryCreateEffectSprite(i_packRoot, behavior.CasingSprite, pixelsPerUnit, i_definition, out casing)) return false;
		Sprite projectileSprite = null;
		List<Sprite> impactSprites = null;
		if (behavior.Projectile != null)
		{
			float projectilePpu = behavior.Projectile.PixelsPerUnit ?? pixelsPerUnit;
			if (!string.IsNullOrWhiteSpace(behavior.Projectile.Sprite)
				&& !TryCreateEffectSprite(i_packRoot, behavior.Projectile.Sprite, projectilePpu, i_definition, out projectileSprite)) return false;
			if (behavior.Projectile.ImpactSprites != null)
			{
				impactSprites = new List<Sprite>();
				foreach (string path in behavior.Projectile.ImpactSprites)
				{
					if (!TryCreateEffectSprite(i_packRoot, path, projectilePpu, i_definition, out Sprite sprite)) return false;
					impactSprites.Add(sprite);
				}
			}
		}
		i_clone.ConfigureModWeaponBehavior(behavior, muzzle, casing, projectileSprite, impactSprites);
		return true;
	}

	private static bool ApplyAnimationAssets(Gun i_clone, WeaponDefinition i_definition, string i_packRoot)
	{
		WeaponAnimationDefinition animation = i_definition.Behavior.Animations;
		if (animation?.Clips == null || animation.Clips.Count == 0) return true;
		Dictionary<string, ModWeaponSpriteClip> clips = new Dictionary<string, ModWeaponSpriteClip>(StringComparer.Ordinal);
		foreach (KeyValuePair<string, WeaponSpriteAnimationClipDefinition> clipEntry in animation.Clips)
		{
			ModWeaponSpriteClip runtimeClip = new ModWeaponSpriteClip { Loop = clipEntry.Value.Loop == true };
			foreach (WeaponSpriteAnimationFrameDefinition frame in clipEntry.Value.Frames)
			{
				ModWeaponSpriteFrame runtimeFrame = new ModWeaponSpriteFrame { Duration = frame.DurationSeconds };
				foreach (KeyValuePair<string, string> spriteEntry in frame.Sprites)
				{
					if (!TryGetPartName(i_definition, spriteEntry.Key, out string partName) || partName == null)
					{
						Report("weapon.animations.slot", "Animation slot is unavailable: " + spriteEntry.Key, i_definition.Source);
						return false;
					}
					List<SpriteRenderer> renderers = new List<SpriteRenderer>();
					FindAllRecursive(i_clone.transform, partName, renderers);
					if (renderers.Count == 0 || renderers[0].sprite == null)
					{
						Report("weapon.animations.slot", "Animation slot has no renderer: " + spriteEntry.Key, i_definition.Source);
						return false;
					}
					Sprite baseline = renderers[0].sprite;
					if (!RuntimePngAssetLoader.TryLoad(i_packRoot, spriteEntry.Value, i_definition.Id + "/animation/" + clipEntry.Key,
						FilterMode.Point, ModLoaderRuntime.LastReport, "weapon.animations.file", "weapon.animations.decode",
						i_definition.Source, out Texture2D texture)) return false;
					Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
						RuntimePngAssetLoader.GetReplacementPivot(baseline, texture.width, texture.height),
						i_definition.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect, baseline.border);
					sprite.name = i_definition.Id + "/animation/" + clipEntry.Key + "/" + spriteEntry.Key;
					foreach (SpriteRenderer renderer in renderers)
						runtimeFrame.Bindings.Add(new ModWeaponSpriteBinding { Renderer = renderer, Sprite = sprite });
				}
				runtimeClip.Frames.Add(runtimeFrame);
			}
			clips.Add(clipEntry.Key, runtimeClip);
		}
		i_clone.gameObject.AddComponent<ModWeaponSpriteAnimator>().Configure(clips);
		return true;
	}

	private static bool TryGetPartName(WeaponDefinition i_definition, string i_slot, out string o_partName)
	{
		if (i_definition.IsOriginal)
		{
			o_partName = i_slot != "icon" && i_definition.Visual.Sprites.ContainsKey(i_slot) ? i_slot : null;
			return o_partName != null;
		}
		return WeaponTemplateCatalog.TryGetCorePartName(i_definition.Extends.Value, i_slot, out o_partName);
	}

	private static bool TryCreateEffectSprite(string i_root, string i_path, float i_pixelsPerUnit,
		WeaponDefinition i_definition, out Sprite o_sprite)
	{
		o_sprite = null;
		if (TryGetBundledSprite(i_definition, System.IO.Path.GetFileNameWithoutExtension(i_path), out o_sprite)) return true;
		if (!RuntimePngAssetLoader.TryLoad(i_root, i_path, i_definition.Id + "/effect/" + i_path,
			FilterMode.Point, ModLoaderRuntime.LastReport, "weapon.factory-effect-file", "weapon.factory-effect-decode",
			i_definition.Source, out Texture2D texture)) return false;
		o_sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f),
			i_pixelsPerUnit, 0, SpriteMeshType.FullRect);
		o_sprite.name = i_definition.Id + "/effect/" + i_path;
		return true;
	}

	private static bool ApplySprites(Gun i_clone, WeaponDefinition i_definition, string i_packRoot)
	{
		List<Texture2D> textures = new List<Texture2D>();
		List<Sprite> sprites = new List<Sprite>();
		List<Action> replacements = new List<Action>();
		foreach (KeyValuePair<string, string> requested in i_definition.Visual.Sprites)
		{
			if (!WeaponTemplateCatalog.TryGetCorePartName(i_definition.Extends.Value, requested.Key, out string partName))
			{
				Report("weapon.factory-slot", "Core weapon template does not publish sprite slot: " + requested.Key, i_definition.Source);
				DestroyCreated(textures, sprites);
				return false;
			}

			List<SpriteRenderer> renderers = new List<SpriteRenderer>();
			Sprite baseline;
			if (partName == null)
			{
				baseline = i_clone.GetSpriteIcon();
			}
			else
			{
				FindAllRecursive(i_clone.transform, partName, renderers);
				baseline = renderers.Count == 0 ? null : renderers[0].sprite;
			}
			if (baseline == null || (partName != null && renderers.Exists(renderer => renderer == null || renderer.sprite == null)))
			{
				Report("weapon.factory-slot", "Core weapon template does not expose sprite slot: " + requested.Key, i_definition.Source);
				DestroyCreated(textures, sprites);
				return false;
			}
			if (TryGetBundledSprite(i_definition, requested.Key, out Sprite bundledSprite))
			{
				if (partName == null) replacements.Add(() => i_clone.SetModItemIcon(bundledSprite));
				else replacements.Add(() => { foreach (SpriteRenderer renderer in renderers) renderer.sprite = bundledSprite; });
				continue;
			}
			if (!RuntimePngAssetLoader.TryLoad(i_packRoot, requested.Value, i_definition.Id + "/" + requested.Key,
				baseline.texture.filterMode, ModLoaderRuntime.LastReport, "weapon.factory-file", "weapon.factory-decode",
				i_definition.Source, out Texture2D texture))
			{
				DestroyCreated(textures, sprites);
				return false;
			}
			textures.Add(texture);
			Vector2 pivot = RuntimePngAssetLoader.GetReplacementPivot(baseline, texture.width, texture.height);
			Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot,
				i_definition.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect, baseline.border);
			sprite.name = i_definition.Id + "/" + requested.Key;
			sprites.Add(sprite);
			if (partName == null)
				replacements.Add(() => i_clone.SetModItemIcon(sprite));
			else
				replacements.Add(() => { foreach (SpriteRenderer renderer in renderers) renderer.sprite = sprite; });
		}

		foreach (Action replacement in replacements) replacement();
		List<SpriteRenderer> hiddenRenderers = new List<SpriteRenderer>();
		foreach (string hiddenSlot in i_definition.Visual.HiddenSlots ?? new List<string>())
		{
			if (!WeaponTemplateCatalog.TryGetCorePartName(i_definition.Extends.Value, hiddenSlot, out string partName) || partName == null) continue;
			List<SpriteRenderer> renderers = new List<SpriteRenderer>();
			FindAllRecursive(i_clone.transform, partName, renderers);
			foreach (SpriteRenderer renderer in renderers) if (!hiddenRenderers.Contains(renderer)) hiddenRenderers.Add(renderer);
		}
		if (hiddenRenderers.Count > 0) i_clone.gameObject.AddComponent<ModHiddenWeaponSlots>().Configure(hiddenRenderers);
		return true;
	}

	private static bool TryGetBundledSprite(WeaponDefinition i_definition, string i_partName, out Sprite o_sprite)
	{
		o_sprite = null;
		if (string.IsNullOrEmpty(i_definition.Visual.BundlePrefab)
			|| !ModAssetBundleRegistry.TryGetPrefab(i_definition.Visual.BundlePrefab, out GameObject prefab)) return false;
		Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
		foreach (Transform candidate in transforms)
		{
			if (!string.Equals(candidate.name, i_partName, StringComparison.Ordinal)) continue;
			SpriteRenderer renderer = candidate.GetComponent<SpriteRenderer>();
			if (renderer != null && renderer.sprite != null) { o_sprite = renderer.sprite; return true; }
		}
		return false;
	}

	private static void DestroyCreated(IEnumerable<Texture2D> i_textures, IEnumerable<Sprite> i_sprites)
	{
		foreach (Sprite sprite in i_sprites) UnityEngine.Object.Destroy(sprite);
		foreach (Texture2D texture in i_textures) UnityEngine.Object.Destroy(texture);
	}

	private static void FindAllRecursive(Transform i_root, string i_name, List<SpriteRenderer> io_renderers)
	{
		if (string.Equals(i_root.name, i_name, StringComparison.Ordinal))
		{
			SpriteRenderer renderer = i_root.GetComponent<SpriteRenderer>();
			if (renderer != null) io_renderers.Add(renderer);
		}
		for (int index = 0; index < i_root.childCount; index++)
			FindAllRecursive(i_root.GetChild(index), i_name, io_renderers);
	}

	private static void Report(string i_code, string i_message, string i_source)
	{
		ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, i_code, i_message, i_source);
	}
}

[Serializable]
public sealed class ModWeaponSpriteBinding
{
	public SpriteRenderer Renderer;
	public Sprite Sprite;
}

[Serializable]
public sealed class ModHiddenWeaponSlots : MonoBehaviour
{
	[SerializeField] private List<SpriteRenderer> m_renderers = new List<SpriteRenderer>();
	public void Configure(IEnumerable<SpriteRenderer> i_renderers)
	{
		m_renderers = new List<SpriteRenderer>(i_renderers);
		Apply();
	}
	public void Apply() { foreach (SpriteRenderer renderer in m_renderers) if (renderer != null) renderer.enabled = false; }
}

[Serializable]
public sealed class ModWeaponSpriteFrame
{
	public float Duration;
	public List<ModWeaponSpriteBinding> Bindings = new List<ModWeaponSpriteBinding>();
}

[Serializable]
public sealed class ModWeaponSpriteClip
{
	public bool Loop;
	public List<ModWeaponSpriteFrame> Frames = new List<ModWeaponSpriteFrame>();
}

[Serializable]
public sealed class ModWeaponNamedSpriteClip
{
	public string Name;
	public ModWeaponSpriteClip Clip;
}

public sealed class ModWeaponSpriteAnimator : MonoBehaviour
{
	[SerializeField] private List<ModWeaponNamedSpriteClip> m_serializedClips = new List<ModWeaponNamedSpriteClip>();
	private Dictionary<string, ModWeaponSpriteClip> m_clips = new Dictionary<string, ModWeaponSpriteClip>(StringComparer.Ordinal);
	private readonly Dictionary<SpriteRenderer, Sprite> m_defaults = new Dictionary<SpriteRenderer, Sprite>();

	private void Awake() { BuildRuntimeLookup(); }

	public void Configure(Dictionary<string, ModWeaponSpriteClip> i_clips)
	{
		m_serializedClips.Clear();
		foreach (KeyValuePair<string, ModWeaponSpriteClip> entry in i_clips)
			m_serializedClips.Add(new ModWeaponNamedSpriteClip { Name = entry.Key, Clip = entry.Value });
		BuildRuntimeLookup();
	}

	private void BuildRuntimeLookup()
	{
		m_clips = new Dictionary<string, ModWeaponSpriteClip>(StringComparer.Ordinal);
		m_defaults.Clear();
		foreach (ModWeaponNamedSpriteClip entry in m_serializedClips)
			if (entry != null && !string.IsNullOrEmpty(entry.Name) && entry.Clip != null) m_clips[entry.Name] = entry.Clip;
		foreach (ModWeaponSpriteClip clip in m_clips.Values)
			foreach (ModWeaponSpriteFrame frame in clip.Frames)
				foreach (ModWeaponSpriteBinding binding in frame.Bindings)
					if (binding.Renderer != null && !m_defaults.ContainsKey(binding.Renderer)) m_defaults.Add(binding.Renderer, binding.Renderer.sprite);
	}

	public void Play(string i_name, string i_fallback = null)
	{
		if (m_clips == null) return;
		if (!m_clips.TryGetValue(i_name, out ModWeaponSpriteClip clip))
		{
			if (!string.IsNullOrEmpty(i_fallback) && m_clips.TryGetValue(i_fallback, out clip)) { }
			else if (i_name == "equip" && m_clips.TryGetValue("idle", out clip)) { }
			else return;
		}
		StopAllCoroutines();
		StartCoroutine(PlayClip(clip));
	}

	private IEnumerator PlayClip(ModWeaponSpriteClip i_clip)
	{
		do
		{
			foreach (ModWeaponSpriteFrame frame in i_clip.Frames)
			{
				foreach (ModWeaponSpriteBinding binding in frame.Bindings)
					if (binding.Renderer != null) binding.Renderer.sprite = binding.Sprite;
				yield return new WaitForSeconds(frame.Duration);
			}
		} while (i_clip.Loop);
		if (m_clips.TryGetValue("idle", out ModWeaponSpriteClip idle) && idle != i_clip)
		{
			StartCoroutine(PlayClip(idle));
			yield break;
		}
		foreach (KeyValuePair<SpriteRenderer, Sprite> entry in m_defaults)
			if (entry.Key != null) entry.Key.sprite = entry.Value;
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

public sealed class ModWeaponAudioLoader : MonoBehaviour
{
	public void Load(Gun i_gun, WeaponDefinition i_definition, string i_packRoot)
	{
		WeaponBehaviorDefinition behavior = i_definition.Behavior;
		bool replaceShoot = behavior.ShootSounds != null;
		bool replaceReload = behavior.ReloadSounds != null;
		WeaponThrowableDefinition throwable = behavior.Throwable;
		bool hasThrowableAudio = throwable != null && (throwable.ImpactSounds != null || throwable.TickSounds != null || throwable.ExplosionSounds != null);
		if (!replaceShoot && !replaceReload && !hasThrowableAudio) return;
		i_gun.BeginModAudioOverride(replaceShoot, replaceReload, behavior.SoundVolume ?? 1f);
		if (replaceShoot) foreach (string path in behavior.ShootSounds) StartCoroutine(LoadClip(i_gun, i_definition, i_packRoot, path, "shoot"));
		if (replaceReload) foreach (string path in behavior.ReloadSounds) StartCoroutine(LoadClip(i_gun, i_definition, i_packRoot, path, "reload"));
		if (throwable?.ImpactSounds != null) foreach (string path in throwable.ImpactSounds) StartCoroutine(LoadClip(i_gun, i_definition, i_packRoot, path, "impact"));
		if (throwable?.TickSounds != null) foreach (string path in throwable.TickSounds) StartCoroutine(LoadClip(i_gun, i_definition, i_packRoot, path, "tick"));
		if (throwable?.ExplosionSounds != null) foreach (string path in throwable.ExplosionSounds) StartCoroutine(LoadClip(i_gun, i_definition, i_packRoot, path, "explosion"));
	}

	private IEnumerator LoadClip(Gun i_gun, WeaponDefinition i_definition, string i_root, string i_relative, string i_role)
	{
		string full = System.IO.Path.GetFullPath(System.IO.Path.Combine(i_root, i_relative));
		AudioType type = System.IO.Path.GetExtension(full).Equals(".ogg", StringComparison.OrdinalIgnoreCase)
			? AudioType.OGGVORBIS : AudioType.WAV;
		using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(new Uri(full).AbsoluteUri, type))
		{
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success)
			{
				Report("weapon.factory-audio", "Could not load weapon audio " + i_relative + ": " + request.error, i_definition.Source);
				yield break;
			}
			AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
			if (i_gun != null && clip != null)
			{
				clip.name = i_definition.Id + "/audio/" + i_relative;
				if (i_role == "shoot" || i_role == "reload") i_gun.AddModAudioClip(i_role == "shoot", clip);
				else i_gun.AddModThrowableAudioClip(i_role, clip);
			}
		}
	}

	private static void Report(string i_code, string i_message, string i_source)
	{
		ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, i_code, i_message, i_source);
	}
}
