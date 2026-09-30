using System;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

/// <summary>Creates data-only, always-present player rig extensions without wardrobe/save participation.</summary>
public static class ExternalPlayerAttachmentFactory
{
	public static void Schedule()
	{
		if (ModLoaderRuntime.PlayerAttachmentDefinitions.Count == 0) return;
		ExternalFactoryRunner.GetOrAdd<ExternalPlayerAttachmentFactoryHost>().Configure();
	}
}

public sealed class ExternalPlayerAttachmentFactoryHost : MonoBehaviour
{
	private sealed class PreparedAttachment
	{
		public PlayerAttachmentDefinition Definition;
		public Sprite DefaultSprite;
		public readonly Dictionary<string, Sprite> SkinSprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
	}

	private readonly List<PreparedAttachment> m_attachments = new List<PreparedAttachment>();
	private float m_nextScan;

	public void Configure()
	{
		m_attachments.Clear();
		Dictionary<string, ModPack> packs = new Dictionary<string, ModPack>(StringComparer.Ordinal);
		foreach (ModPack pack in ModLoaderRuntime.LoadedPacks)
			if (pack?.Manifest != null) packs[pack.Manifest.Id] = pack;
		foreach (PlayerAttachmentDefinition definition in ModLoaderRuntime.PlayerAttachmentDefinitions)
		{
			if (!packs.TryGetValue(definition.PackId, out ModPack pack)) continue;
			PreparedAttachment prepared = new PreparedAttachment { Definition = definition };
			if (!string.IsNullOrWhiteSpace(definition.Document.Sprite))
				prepared.DefaultSprite = LoadSprite(pack.RootPath, definition, definition.Document.Sprite, "default");
			if (definition.Document.TintWithSkin && prepared.DefaultSprite != null)
				foreach (SkinColor skin in Enum.GetValues(typeof(SkinColor)))
					prepared.SkinSprites[skin.ToString().ToLowerInvariant()] = CreateSkinMatchedSprite(
						prepared.DefaultSprite, SkinTonePalette.GetColor(skin), definition.Id + "/" + skin.ToString().ToLowerInvariant());
			foreach (KeyValuePair<string, string> entry in definition.Document.SkinSprites ?? new Dictionary<string, string>())
			{
				Sprite sprite = LoadSprite(pack.RootPath, definition, entry.Value, entry.Key);
				if (sprite != null) prepared.SkinSprites[entry.Key] = sprite;
			}
			if (prepared.DefaultSprite != null || prepared.SkinSprites.Count > 0) m_attachments.Add(prepared);
		}
		AttachToScenePlayers();
	}

	private void Update()
	{
		if (Time.unscaledTime < m_nextScan) return;
		m_nextScan = Time.unscaledTime + 0.5f;
		AttachToScenePlayers();
	}

	private void AttachToScenePlayers()
	{
		foreach (SkeletonPlayer skeleton in Resources.FindObjectsOfTypeAll<SkeletonPlayer>())
		{
			if (skeleton == null || !skeleton.gameObject.scene.IsValid()) continue;
			foreach (PreparedAttachment attachment in m_attachments)
				Attach(skeleton, attachment);
		}
	}

	private static void Attach(SkeletonPlayer i_skeleton, PreparedAttachment i_attachment)
	{
		if (!Enum.TryParse(i_attachment.Definition.Document.Bone, true, out BoneTypePlayer boneType)) return;
		BonePlayer bone = i_skeleton.GetBone(boneType);
		if (bone == null || bone.GetBodyPartPlayer() == null) return;
		string attachmentName = "mod-player-attachment_" + i_attachment.Definition.Id.Path.Replace('/', '_');
		foreach (ModPersistentPlayerAttachment existing in i_skeleton.GetComponentsInChildren<ModPersistentPlayerAttachment>(true))
		{
			// Runtime-only fields (notably the generated skin-sprite dictionary) are
			// not reliable across Unity Instantiate. Wardrobe previews clone the whole
			// skeleton, so recognize their attachment by its stable object name too.
			if (existing.Id != i_attachment.Definition.Id.ToString() && existing.name != attachmentName) continue;
			existing.Configure(i_attachment.Definition.Id.ToString(), i_skeleton, bone,
				existing.GetComponent<SpriteRenderer>(), i_attachment.DefaultSprite,
				i_attachment.SkinSprites, i_attachment.Definition.Document.SortingOffset,
				i_attachment.Definition.Document.PregnancyGrowth);
			return;
		}
		Transform parent = i_attachment.Definition.Document.AttachToBone ? bone.transform : bone.GetBodyPartPlayer().transform;
		GameObject gameObject = new GameObject(attachmentName);
		gameObject.transform.SetParent(parent, false);
		gameObject.transform.localPosition = new Vector3(i_attachment.Definition.Document.OffsetX, i_attachment.Definition.Document.OffsetY, 0f);
		gameObject.transform.localEulerAngles = new Vector3(0f, 0f, i_attachment.Definition.Document.Rotation);
		int playerLayer = LayerMask.NameToLayer("Player");
		if (playerLayer >= 0) gameObject.layer = playerLayer;
		SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
		renderer.sortingLayerName = "Player";
		ModPersistentPlayerAttachment runtime = gameObject.AddComponent<ModPersistentPlayerAttachment>();
		runtime.Configure(i_attachment.Definition.Id.ToString(), i_skeleton, bone, renderer,
			i_attachment.DefaultSprite, i_attachment.SkinSprites, i_attachment.Definition.Document.SortingOffset,
			i_attachment.Definition.Document.PregnancyGrowth);
		if (i_attachment.Definition.Document.Physics != null)
		{
			ModClothingSway sway = gameObject.AddComponent<ModClothingSway>();
			sway.Configure(i_attachment.Definition.Document.Physics);
		}
	}

	private static Sprite LoadSprite(string i_packRoot, PlayerAttachmentDefinition i_definition, string i_path, string i_variant)
	{
		if (!RuntimePngAssetLoader.TryLoad(i_packRoot, i_path, i_definition.Id + "/" + i_variant,
			FilterMode.Point, ModLoaderRuntime.LastReport, "player-attachment.file", "player-attachment.decode",
			i_definition.Source, out Texture2D texture)) return null;
		PlayerAttachmentDocument document = i_definition.Document;
		Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
			new Vector2(document.PivotX, document.PivotY), document.PixelsPerUnit, 0, SpriteMeshType.FullRect);
		sprite.name = i_definition.Id + "/" + i_variant;
		return sprite;
	}

	private static Sprite CreateSkinMatchedSprite(Sprite i_source, Color i_tone, string i_name)
	{
		Texture2D source = i_source.texture;
		Color32[] pixels = source.GetPixels32();
		Color32 tone = i_tone;
		for (int index = 0; index < pixels.Length; index++)
		{
			Color32 pixel = pixels[index];
			if (pixel.a == 0) continue;
			byte brightest = Math.Max(pixel.r, Math.Max(pixel.g, pixel.b));
			if (brightest <= 40) continue; // Preserve the authored outline.
			float shade = Mathf.Lerp(0.62f, 1f, Mathf.Clamp01((brightest - 70f) / 100f));
			pixels[index] = new Color32((byte)(tone.r * shade), (byte)(tone.g * shade),
				(byte)(tone.b * shade), pixel.a);
		}
		Texture2D texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
		texture.name = i_name;
		texture.filterMode = FilterMode.Point;
		texture.SetPixels32(pixels);
		texture.Apply(false, false);
		Vector2 pivot = new Vector2(i_source.pivot.x / i_source.rect.width, i_source.pivot.y / i_source.rect.height);
		Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), pivot,
			i_source.pixelsPerUnit, 0, SpriteMeshType.FullRect, i_source.border);
		sprite.name = i_name;
		return sprite;
	}
}

public sealed class ModPersistentPlayerAttachment : MonoBehaviour
{
	private string m_id;
	private SkeletonPlayer m_skeleton;
	private BonePlayer m_bone;
	private SpriteRenderer m_renderer;
	private Sprite m_defaultSprite;
	private Dictionary<string, Sprite> m_skinSprites;
	private int m_sortingOffset;
	private SkinColor m_lastSkin = (SkinColor)(-1);
	private PlayerAttachmentPregnancyGrowthDefinition m_pregnancyGrowth;
	private int m_pregnancyStage = -1;
	private Coroutine m_growthCoroutine;

	public string Id => m_id;

	public void Configure(string i_id, SkeletonPlayer i_skeleton, BonePlayer i_bone, SpriteRenderer i_renderer,
		Sprite i_defaultSprite, Dictionary<string, Sprite> i_skinSprites, int i_sortingOffset,
		PlayerAttachmentPregnancyGrowthDefinition i_pregnancyGrowth)
	{
		m_id = i_id; m_skeleton = i_skeleton; m_bone = i_bone; m_renderer = i_renderer;
		m_defaultSprite = i_defaultSprite; m_skinSprites = i_skinSprites; m_sortingOffset = i_sortingOffset;
		m_pregnancyGrowth = i_pregnancyGrowth;
		RefreshForSkinTone();
		RefreshPregnancyGrowth(true);
	}

	private void LateUpdate()
	{
		if (m_skeleton == null || m_bone == null || m_renderer == null) return;
		if (m_lastSkin != m_skeleton.GetSkinColor()) RefreshForSkinTone();
		m_renderer.sortingOrder = m_bone.GetBodyPartPlayer().GetSortingOrder() + m_sortingOffset;
		RefreshPregnancyGrowth(false);
	}

	private void RefreshPregnancyGrowth(bool i_immediate)
	{
		if (m_pregnancyGrowth?.Stages == null || m_pregnancyGrowth.Stages.Count == 0) return;
		Player player = GetComponentInParent<Player>();
		int fetusCount = player == null ? 0 : player.GetNumOfFetuses();
		PlayerAttachmentPregnancyStageDefinition selected = null;
		int selectedIndex = -1;
		for (int index = 0; index < m_pregnancyGrowth.Stages.Count; index++)
		{
			PlayerAttachmentPregnancyStageDefinition candidate = m_pregnancyGrowth.Stages[index];
			if (candidate.MinimumFetuses <= fetusCount && (selected == null || candidate.MinimumFetuses > selected.MinimumFetuses))
			{
				selected = candidate;
				selectedIndex = index;
			}
		}
		if (selected == null || selectedIndex == m_pregnancyStage) return;
		bool changedAfterInitialization = m_pregnancyStage >= 0;
		m_pregnancyStage = selectedIndex;
		if (m_growthCoroutine != null) StopCoroutine(m_growthCoroutine);
		if (i_immediate || !isActiveAndEnabled)
		{
			ApplyPregnancyTransform(selected);
			m_growthCoroutine = null;
		}
		else m_growthCoroutine = StartCoroutine(AnimatePregnancyTransform(selected));
		if (changedAfterInitialization && player != null && m_pregnancyGrowth.BreakSpineClothing)
			StartCoroutine(BreakSpineClothingAfterDelay(m_pregnancyGrowth.ClothingBreakDelaySeconds));
	}

	private System.Collections.IEnumerator AnimatePregnancyTransform(PlayerAttachmentPregnancyStageDefinition i_stage)
	{
		Vector3 positionFrom = transform.localPosition;
		Vector3 scaleFrom = transform.localScale;
		Vector3 positionTo = new Vector3(i_stage.OffsetX, i_stage.OffsetY, positionFrom.z);
		Vector3 scaleTo = new Vector3(i_stage.ScaleX, i_stage.ScaleY, 1f);
		float duration = Mathf.Max(0.05f, m_pregnancyGrowth.TransitionSeconds);
		float elapsed = 0f;
		if (m_renderer != null) m_renderer.enabled = true;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float amount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
			transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, amount);
			transform.localScale = Vector3.LerpUnclamped(scaleFrom, scaleTo, amount);
			yield return null;
		}
		ApplyPregnancyTransform(i_stage);
		m_growthCoroutine = null;
	}

	private void ApplyPregnancyTransform(PlayerAttachmentPregnancyStageDefinition i_stage)
	{
		transform.localPosition = new Vector3(i_stage.OffsetX, i_stage.OffsetY, transform.localPosition.z);
		transform.localScale = new Vector3(i_stage.ScaleX, i_stage.ScaleY, 1f);
		if (m_renderer != null) m_renderer.enabled = i_stage.ScaleX > 0f && i_stage.ScaleY > 0f;
	}

	private System.Collections.IEnumerator BreakSpineClothingAfterDelay(float i_delay)
	{
		if (i_delay > 0f) yield return new WaitForSeconds(i_delay);
		if (m_skeleton == null) yield break;
		List<ClothingPiece> pieces = new List<ClothingPiece>(m_skeleton.GetClothingPiecesAttached());
		foreach (ClothingPiece piece in pieces)
			if (piece != null && piece.GetBoneToAttachTo() == BoneTypePlayer.Spine) piece.DropOrDestroy(false);
	}

	public void RefreshForSkinTone()
	{
		SkeletonPlayer owner = GetComponentInParent<SkeletonPlayer>();
		if (owner != null) m_skeleton = owner;
		if (m_renderer == null) m_renderer = GetComponent<SpriteRenderer>();
		if (m_skeleton == null || m_renderer == null) return;
		m_lastSkin = m_skeleton.GetSkinColor();
		string skin = m_lastSkin.ToString().ToLowerInvariant();
		if (m_skinSprites != null && m_skinSprites.TryGetValue(skin, out Sprite sprite)) m_renderer.sprite = sprite;
		else if (m_defaultSprite != null) m_renderer.sprite = m_defaultSprite;
		else if (m_skinSprites != null)
			foreach (Sprite fallback in m_skinSprites.Values) { m_renderer.sprite = fallback; break; }
	}
}
