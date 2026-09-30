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
		ExternalClothingFactoryHost host = ExternalFactoryRunner.GetOrAdd<ExternalClothingFactoryHost>();
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
			if (!packs.TryGetValue(definition.PackId, out ModPack pack))
			{
				Report("clothing.factory-pack", "Clothing pack is not loaded: " + definition.PackId, definition.Source);
				continue;
			}

			Clothing clone;
			if (definition.IsOriginal)
			{
				if (!TryCreateOriginalClothing(i_library.transform, definition, pack.RootPath, out clone)) continue;
			}
			else
			{
				if (!ModLoaderRuntime.Registry.TryGet(definition.Extends.Value, out ContentRegistration baseEntry) || !(baseEntry.RuntimeAsset is Clothing template))
				{
					Report("clothing.factory-template", "Core clothing template is not bound: " + definition.Extends.Value, definition.Source);
					continue;
				}
				clone = UnityEngine.Object.Instantiate(template, i_library.transform);
			}
			clone.gameObject.name = definition.Id.ToString();
			clone.gameObject.SetActive(false);
			clone.Initialize();
			clone.SetId(runtimeId--);
			clone.ConfigureModClothing(definition.Category, definition.IncompatibleCategories, definition.Effects);
			RuntimeContentIdentity identity = clone.GetComponent<RuntimeContentIdentity>();
			if (identity == null) identity = clone.gameObject.AddComponent<RuntimeContentIdentity>();
			identity.Configure(definition.Id, ContentCategory.Clothing);
			if (!definition.IsOriginal && !ApplyAtlas(clone, definition, pack.RootPath))
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			if (!ApplyAttachments(clone, definition))
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			if (!ApplyBodyVariants(clone, definition, pack.RootPath))
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			i_library.AddRuntimeClothing(clone);
			ModLoaderRuntime.Registry.BindRuntimeAsset(definition.Id, clone, ModLoaderRuntime.LastReport);
			Debug.Log("[ModLoader] Built external clothing " + definition.Id
				+ (definition.IsOriginal ? " from a JSON rig." : " from " + definition.Extends.Value + "."));
		}
	}

	private static bool TryCreateOriginalClothing(Transform i_parent, ClothingDefinition i_definition, string i_packRoot, out Clothing o_clothing)
	{
		o_clothing = null;
		GameObject root = new GameObject(i_definition.Id.ToString());
		root.transform.SetParent(i_parent, false);
		int playerLayer = LayerMask.NameToLayer("Player");
		if (playerLayer >= 0) root.layer = playerLayer;
		o_clothing = root.AddComponent<Clothing>();
		foreach (KeyValuePair<string, string> entry in i_definition.Visual.Sprites)
		{
			if (!RuntimePngAssetLoader.TryLoad(i_packRoot, entry.Value, i_definition.Id + "/" + entry.Key,
				FilterMode.Point, ModLoaderRuntime.LastReport, "clothing.factory-file", "clothing.factory-decode",
				i_definition.Source, out Texture2D texture))
			{
				UnityEngine.Object.Destroy(root);
				o_clothing = null;
				return false;
			}
			ClothingAttachmentDefinition attachment = null;
			i_definition.Visual.Attachments?.TryGetValue(entry.Key, out attachment);
			Vector2 pivot = new Vector2(attachment?.PivotX ?? 0.5f, attachment?.PivotY ?? 0.5f);
			Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot,
				i_definition.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect);
			sprite.name = i_definition.Id + "/" + entry.Key;
			if (entry.Key == "icon")
			{
				o_clothing.SetIcon(sprite);
				continue;
			}
			GameObject pieceObject = new GameObject("clp_" + entry.Key.Substring("piece/".Length).Replace('/', '_'));
			pieceObject.transform.SetParent(root.transform, false);
			if (playerLayer >= 0) pieceObject.layer = playerLayer;
			SpriteRenderer renderer = pieceObject.AddComponent<SpriteRenderer>();
			renderer.sprite = sprite;
			renderer.sortingLayerName = "Player";
			ClothingPiece piece = pieceObject.AddComponent<ClothingPiece>();
			pieceObject.AddComponent<ModClothingSlotIdentity>().Configure(entry.Key);
			piece.ConfigureModAttachment(attachment);
		}
		o_clothing.Initialize();
		return true;
	}

	private static bool ApplyAtlas(Clothing i_clone, ClothingDefinition i_definition, string i_packRoot)
	{
		if (i_definition.Visual.Type == "coreClothingSprites") return ApplySprites(i_clone, i_definition, i_packRoot);
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
			Vector2 pivot = GetPivot(i_definition, region.Key, baseline, area.Width, area.Height);
			Sprite sprite = Sprite.Create(atlas, rect, pivot, i_definition.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect, baseline.border);
			sprite.name = i_definition.Id + "/" + region.Key;
			if (region.Key == "icon") i_clone.SetIcon(sprite);
			else FindPiece(i_clone, region.Key).GetComponent<SpriteRenderer>().sprite = sprite;
		}
		return true;
	}

	private static bool ApplySprites(Clothing i_clone, ClothingDefinition i_definition, string i_packRoot)
	{
		Dictionary<string, Sprite> baselines = BuildSlotMap(i_clone);
		foreach (KeyValuePair<string, string> entry in i_definition.Visual.Sprites)
		{
			if (!baselines.TryGetValue(entry.Key, out Sprite baseline) || baseline == null)
			{
				Report("clothing.factory-sprite", "Core clothing template does not expose sprite: " + entry.Key, i_definition.Source);
				return false;
			}
			if (!RuntimePngAssetLoader.TryLoad(i_packRoot, entry.Value, i_definition.Id + "/" + entry.Key,
				FilterMode.Point, ModLoaderRuntime.LastReport, "clothing.factory-file", "clothing.factory-decode",
				i_definition.Source, out Texture2D texture)) return false;
			Vector2 pivot = GetPivot(i_definition, entry.Key, baseline, texture.width, texture.height);
			Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot,
				i_definition.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect, baseline.border);
			sprite.name = i_definition.Id + "/" + entry.Key;
			if (entry.Key == "icon") i_clone.SetIcon(sprite);
			else FindPiece(i_clone, entry.Key).GetComponent<SpriteRenderer>().sprite = sprite;
		}
		return true;
	}

	private static bool ApplyAttachments(Clothing i_clothing, ClothingDefinition i_definition)
	{
		foreach (KeyValuePair<string, ClothingAttachmentDefinition> entry in
			i_definition.Visual.Attachments ?? new Dictionary<string, ClothingAttachmentDefinition>())
		{
			ClothingPiece piece = FindPiece(i_clothing, entry.Key);
			if (piece == null)
			{
				Report("clothing.factory-attachment", "Core clothing template does not expose attachment slot: " + entry.Key, i_definition.Source);
				return false;
			}
			piece.ConfigureModAttachment(entry.Value);
			if (entry.Value.Physics != null)
			{
				ModClothingSway sway = piece.GetComponent<ModClothingSway>();
				if (sway == null) sway = piece.gameObject.AddComponent<ModClothingSway>();
				sway.Configure(entry.Value.Physics);
			}
		}
		return true;
	}

	private static bool ApplyBodyVariants(Clothing i_clothing, ClothingDefinition i_definition, string i_packRoot)
	{
		foreach (KeyValuePair<string, Dictionary<string, string>> variant in
			i_definition.Visual.BodyVariants ?? new Dictionary<string, Dictionary<string, string>>())
		{
			foreach (KeyValuePair<string, string> entry in variant.Value)
			{
				ClothingPiece piece = FindPiece(i_clothing, entry.Key);
				Sprite baseline = piece == null ? null : piece.GetComponent<SpriteRenderer>()?.sprite;
				if (piece == null || baseline == null)
				{
					Report("clothing.factory-body-variant", "Body variant targets an unavailable piece: " + entry.Key, i_definition.Source);
					return false;
				}
				if (!RuntimePngAssetLoader.TryLoad(i_packRoot, entry.Value,
					i_definition.Id + "/variant/" + variant.Key + "/" + entry.Key, FilterMode.Point,
					ModLoaderRuntime.LastReport, "clothing.factory-file", "clothing.factory-decode",
					i_definition.Source, out Texture2D texture)) return false;
				Vector2 pivot = GetPivot(i_definition, entry.Key, baseline, texture.width, texture.height);
				Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot,
					i_definition.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect);
				sprite.name = i_definition.Id + "/variant/" + variant.Key + "/" + entry.Key;
				ModClothingVariantSprite variants = piece.GetComponent<ModClothingVariantSprite>();
				if (variants == null) variants = piece.gameObject.AddComponent<ModClothingVariantSprite>();
				variants.Add(variant.Key, sprite);
			}
		}
		return true;
	}

	private static Vector2 GetPivot(ClothingDefinition i_definition, string i_key, Sprite i_baseline, int i_width, int i_height)
	{
		Vector2 pivot = RuntimePngAssetLoader.GetReplacementPivot(i_baseline, i_width, i_height);
		if (i_definition.Visual.Attachments != null
			&& i_definition.Visual.Attachments.TryGetValue(i_key, out ClothingAttachmentDefinition attachment))
		{
			if (attachment.PivotX.HasValue) pivot.x = attachment.PivotX.Value;
			if (attachment.PivotY.HasValue) pivot.y = attachment.PivotY.Value;
		}
		return pivot;
	}

	private static Dictionary<string, Sprite> BuildSlotMap(Clothing i_clothing)
	{
		Dictionary<string, Sprite> slots = new Dictionary<string, Sprite>(StringComparer.Ordinal);
		if (i_clothing.GetIcon() != null) slots["icon"] = i_clothing.GetIcon();
		foreach (ClothingPiece piece in i_clothing.GetClothingPieces())
		{
			SpriteRenderer renderer = piece.GetComponent<SpriteRenderer>();
			if (renderer == null || renderer.sprite == null) continue;
			string key = GetSlot(piece);
			if (!slots.ContainsKey(key)) slots.Add(key, renderer.sprite);
		}
		return slots;
	}

	private static ClothingPiece FindPiece(Clothing i_clothing, string i_key)
	{
		foreach (ClothingPiece piece in i_clothing.GetClothingPieces())
			if (GetSlot(piece) == i_key) return piece;
		return null;
	}

	private static string GetSlot(ClothingPiece i_piece)
	{
		ModClothingSlotIdentity identity = i_piece.GetComponent<ModClothingSlotIdentity>();
		return identity == null ? ClothingSlotCatalog.FromCorePieceName(i_piece.name) : identity.Slot;
	}

	private static void Report(string i_code, string i_message, string i_source)
	{
		ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, i_code, i_message, i_source);
	}
}

public sealed class ModClothingSlotIdentity : MonoBehaviour
{
	[SerializeField] private string m_slot;
	public string Slot => m_slot;
	public void Configure(string i_slot) { m_slot = i_slot; }
}

public sealed class ModClothingVariantSprite : MonoBehaviour
{
	[SerializeField] private List<string> m_variantNames = new List<string>();
	[SerializeField] private List<Sprite> m_variantSprites = new List<Sprite>();

	public void Add(string i_name, Sprite i_sprite)
	{
		m_variantNames.Add(i_name);
		m_variantSprites.Add(i_sprite);
	}

	private void OnEnable()
	{
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		if (renderer == null) return;
		string explicitVariant = PlayerPrefs.GetString("ModBodyVariant", string.Empty).Trim().ToLowerInvariant();
		if (TryApply(explicitVariant, renderer)) return;
		Player player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
		if (player != null)
		{
			string playerName = player.gameObject.name.Replace("(Clone)", string.Empty).Trim().ToLowerInvariant();
			if (TryApply(playerName, renderer)) return;
		}
		foreach (ModPack pack in ModLoaderRuntime.LoadedPacks)
			if (pack?.Manifest != null && TryApply(pack.Manifest.Id.ToLowerInvariant(), renderer)) return;
		TryApply("default", renderer);
	}

	private bool TryApply(string i_variant, SpriteRenderer i_renderer)
	{
		if (string.IsNullOrEmpty(i_variant)) return false;
		for (int index = 0; index < m_variantNames.Count && index < m_variantSprites.Count; index++)
			if (string.Equals(m_variantNames[index], i_variant, StringComparison.OrdinalIgnoreCase))
			{
				i_renderer.sprite = m_variantSprites[index];
				return true;
			}
		return false;
	}
}

public sealed class ModClothingSway : MonoBehaviour
{
	[SerializeField] private float m_spring = 45f;
	[SerializeField] private float m_damping = 9f;
	[SerializeField] private float m_gravity = 0.25f;
	[SerializeField] private float m_motionInfluence = 1f;
	[SerializeField] private float m_maxAngle = 30f;
	private float m_baseAngle;
	private float m_angle;
	private float m_velocity;
	private float m_parentAngle;
	private Vector3 m_parentPosition;
	private bool m_needsBaseline;

	public void Configure(ClothingPhysicsDefinition i_definition)
	{
		if (i_definition == null) return;
		m_spring = i_definition.Spring ?? 45f;
		m_damping = i_definition.Damping ?? 9f;
		m_gravity = i_definition.Gravity ?? 0.25f;
		m_motionInfluence = i_definition.MotionInfluence ?? 1f;
		m_maxAngle = i_definition.MaxAngle ?? 30f;
	}

	private void OnEnable()
	{
		m_angle = 0f;
		m_velocity = 0f;
		m_needsBaseline = true;
	}

	private void LateUpdate()
	{
		if (transform.parent == null) return;
		if (m_needsBaseline)
		{
			m_baseAngle = transform.localEulerAngles.z;
			m_parentAngle = transform.parent.eulerAngles.z;
			m_parentPosition = transform.parent.position;
			m_needsBaseline = false;
			return;
		}
		float deltaTime = Mathf.Min(Time.deltaTime, 0.05f);
		if (deltaTime <= 0f) return;
		float parentAngle = transform.parent.eulerAngles.z;
		float rotationImpulse = -Mathf.DeltaAngle(m_parentAngle, parentAngle) * m_motionInfluence;
		float horizontalSpeed = (transform.parent.position.x - m_parentPosition.x) / deltaTime;
		float gravityTarget = Mathf.DeltaAngle(parentAngle, 0f) * m_gravity;
		float target = Mathf.Clamp(gravityTarget + rotationImpulse - horizontalSpeed * m_motionInfluence,
			-m_maxAngle, m_maxAngle);
		m_velocity += ((target - m_angle) * m_spring - m_velocity * m_damping) * deltaTime;
		m_angle = Mathf.Clamp(m_angle + m_velocity * deltaTime, -m_maxAngle, m_maxAngle);
		transform.localEulerAngles = new Vector3(0f, 0f, m_baseAngle + m_angle);
		m_parentAngle = parentAngle;
		m_parentPosition = transform.parent.position;
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

public static class ExternalChallengeFactory
{
	public static void Schedule(ManagerChallenge i_manager)
	{
		if (i_manager == null || ModLoaderRuntime.ChallengeDefinitions.Count == 0) return;
		ExternalFactoryRunner.GetOrAdd<ExternalChallengeFactoryHost>().Begin(i_manager);
	}

	internal static void Build(ManagerChallenge i_manager)
	{
		List<Challenge> added = new List<Challenge>();
		foreach (ChallengeDefinition definition in ModLoaderRuntime.ChallengeDefinitions)
		{
			List<NPC> enemies = new List<NPC>();
			List<Clothing> rewards = new List<Clothing>();
			Stage stage = null;
			bool valid = true;
			foreach (ContentId id in definition.Enemies)
				if (ModLoaderRuntime.Registry.TryGet(id, out ContentRegistration entry) && entry.RuntimeAsset is NPC npc) enemies.Add(npc); else { Report("challenge.factory-enemy", "Challenge enemy is not bound: " + id, definition.Source); valid = false; }
			foreach (ContentId id in definition.Rewards)
				if (ModLoaderRuntime.Registry.TryGet(id, out ContentRegistration entry) && entry.RuntimeAsset is Clothing clothing) rewards.Add(clothing); else { Report("challenge.factory-reward", "Challenge reward is not bound: " + id, definition.Source); valid = false; }
			foreach (ContentId id in definition.RewardContent)
				if (ModLoaderRuntime.Registry.TryGet(id, out ContentRegistration entry) && entry.RuntimeAsset is Clothing clothing && !rewards.Contains(clothing)) rewards.Add(clothing);
			if (definition.Stage.HasValue)
			{
				ContentId stageId = definition.Stage.Value;
				if (!ModLoaderRuntime.Registry.TryGet(stageId, out ContentRegistration entry) || !(entry.RuntimeAsset is Stage boundStage)) { Report("challenge.factory-stage", "Challenge stage is not bound: " + stageId, definition.Source); valid = false; }
				else stage = boundStage;
			}
			if (!valid) continue;
			GameObject gameObject;
			Challenge challenge;
			if (definition.Extends.HasValue)
			{
				if (!ModLoaderRuntime.Registry.TryGet(definition.Extends.Value, out ContentRegistration inherited)
					|| !(inherited.RuntimeAsset is Challenge template))
				{
					Report("challenge.factory-template", "Inherited challenge is not bound: " + definition.Extends.Value, definition.Source);
					continue;
				}
				gameObject = UnityEngine.Object.Instantiate(template.gameObject, i_manager.transform);
				challenge = gameObject.GetComponent<Challenge>();
				if (!definition.Stage.HasValue) stage = template.GetStageAssociated();
			}
			else
			{
				gameObject = new GameObject(definition.Id.ToString());
				gameObject.transform.SetParent(i_manager.transform, false);
				ModEventChallenge eventChallenge = gameObject.AddComponent<ModEventChallenge>();
				eventChallenge.Configure(definition.Objective, definition.Count, definition.Enemies, definition.Items, definition.Steps);
				challenge = eventChallenge;
			}
			gameObject.name = definition.Id.ToString();
			challenge.SetState(0);
			challenge.ConfigureModChallenge(ModLoaderRuntime.GetChallengeRuntimeId(definition.Id), definition.DisplayName, definition.Description,
				rewards, definition.RewardItems, definition.RewardWeapons, definition.RewardContent, definition.RewardCurrency, stage);
			RuntimeContentIdentity identity = gameObject.GetComponent<RuntimeContentIdentity>();
			if (identity == null) identity = gameObject.AddComponent<RuntimeContentIdentity>();
			identity.Configure(definition.Id, ContentCategory.Challenge);
			i_manager.AddRuntimeChallenge(challenge);
			ModLoaderRuntime.Registry.BindRuntimeAsset(definition.Id, challenge, ModLoaderRuntime.LastReport);
			added.Add(challenge);
		}
		if (added.Count > 0)
		{
			_ = ManagerDB.AddChallenges(added);
			i_manager.UpdateChallenges();
		}
	}
	private static void Report(string i_code, string i_message, string i_source) { ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, i_code, i_message, i_source); }
}

public sealed class ModEventChallenge : Challenge
{
	private string m_objective;
	private int m_target;
	private int m_progress;
	private readonly HashSet<ContentId> m_enemies = new HashSet<ContentId>();
	private readonly HashSet<ContentId> m_items = new HashSet<ContentId>();
	private Player m_player;
	private ManagerWave m_wave;
	private bool m_tookDamageThisWave;
	private readonly List<ChallengeObjectiveDefinition> m_steps = new List<ChallengeObjectiveDefinition>();
	private int m_stepIndex;

	public void Configure(string i_objective, int i_target, IEnumerable<ContentId> i_enemies, IEnumerable<ContentId> i_items,
		IEnumerable<ChallengeObjectiveDefinition> i_steps = null)
	{
		m_steps.Clear();
		if (i_steps != null) foreach (ChallengeObjectiveDefinition step in i_steps) m_steps.Add(step);
		if (m_steps.Count == 0)
			m_steps.Add(new ChallengeObjectiveDefinition(i_objective, i_target,
				ToStrings(i_enemies), ToStrings(i_items)));
		LoadStep(0);
	}

	private static IEnumerable<string> ToStrings(IEnumerable<ContentId> i_ids)
	{
		List<string> result = new List<string>();
		if (i_ids != null) foreach (ContentId id in i_ids) result.Add(id.ToString());
		return result;
	}

	private void LoadStep(int i_index)
	{
		m_stepIndex = i_index;
		m_progress = 0;
		m_tookDamageThisWave = false;
		m_enemies.Clear(); m_items.Clear();
		ChallengeObjectiveDefinition step = m_steps[m_stepIndex];
		m_objective = step.Objective; m_target = step.Count;
		foreach (ContentId enemy in step.Enemies) m_enemies.Add(enemy);
		foreach (ContentId item in step.Items) m_items.Add(item);
	}

	protected override void HandleActivation()
	{
		LoadStep(0);
		m_player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
		Stage stage = CommonReferences.Instance == null || CommonReferences.Instance.GetManagerStages() == null
			? null : CommonReferences.Instance.GetManagerStages().GetStageCurrent();
		m_wave = stage == null ? null : stage.GetManagerWave();
		if (m_player != null)
		{
			m_player.OnKill += HandleKill;
			m_player.OnPickUp += HandlePickup;
			Interactable.OnAnyActivated += HandleInteraction;
			m_player.OnShoot += HandleShot;
			m_player.OnTakeDamage += HandleDamage;
			m_player.OnBirthEnd += HandleBirth;
			m_player.OnFetusInsert += HandleImpregnation;
			m_player.OnBeingRaped += HandleRape;
			m_player.OnOrgasm += HandleOrgasm;
			m_player.OnDie += HandleMindBreak;
		}
		if (m_wave != null)
		{
			m_wave.OnWaveStart += HandleWaveStart;
			m_wave.OnWaveEnd += HandleWaveEnd;
		}
		Usable.OnAnyUsed += HandleUsableUsed;
	}

	protected override void HandleDeActivation()
	{
		if (m_player != null)
		{
			m_player.OnKill -= HandleKill;
			m_player.OnPickUp -= HandlePickup;
			Interactable.OnAnyActivated -= HandleInteraction;
			m_player.OnShoot -= HandleShot;
			m_player.OnTakeDamage -= HandleDamage;
			m_player.OnBirthEnd -= HandleBirth;
			m_player.OnFetusInsert -= HandleImpregnation;
			m_player.OnBeingRaped -= HandleRape;
			m_player.OnOrgasm -= HandleOrgasm;
			m_player.OnDie -= HandleMindBreak;
		}
		if (m_wave != null)
		{
			m_wave.OnWaveStart -= HandleWaveStart;
			m_wave.OnWaveEnd -= HandleWaveEnd;
		}
		Usable.OnAnyUsed -= HandleUsableUsed;
		m_player = null;
		m_wave = null;
	}

	protected override void TrackCompletion()
	{
		if (m_objective == "reachWave" && m_wave != null && m_wave.GetNumWaveCurrent() >= m_target) CompleteStep();
		else if (m_progress >= m_target) CompleteStep();
	}

	private void AddProgress(string i_objective, int i_amount = 1)
	{
		if (m_objective != i_objective) return;
		m_progress += i_amount;
		if (m_progress >= m_target) CompleteStep();
	}

	private void CompleteStep()
	{
		if (m_stepIndex + 1 >= m_steps.Count) { Complete(); return; }
		LoadStep(m_stepIndex + 1);
		TrackCompletion();
	}

	private void HandleKill(NPC i_enemy)
	{
		if (m_objective == "killCount" && (m_enemies.Count == 0 || Matches(i_enemy, m_enemies))) AddProgress("killCount");
		else if (m_objective == "weaponKillCount" && m_player != null
			&& (m_enemies.Count == 0 || Matches(i_enemy, m_enemies))
			&& (m_items.Count == 0 || Matches(m_player.GetEquippableEquipped(), m_items))) AddProgress("weaponKillCount");
	}

	private void HandlePickup(PickUpable i_item)
	{
		if (m_objective != "pickupCount") return;
		if (m_items.Count == 0 || Matches(i_item, m_items)) AddProgress("pickupCount", i_item == null ? 1 : i_item.GetAmount());
	}

	private void HandleInteraction(Interactable i_interactable, Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (i_initiator is Player && i_activationType != InteractableActivationType.Operator) AddProgress("interactionCount");
	}
	private void HandleShot(List<Bullet> i_bullets) { AddProgress("shotsFired"); }
	private void HandleDamage() { m_tookDamageThisWave = true; AddProgress("damageTaken"); }
	private void HandleBirth() { AddProgress("birthCount"); }
	private void HandleImpregnation(Fetus i_fetus)
	{
		if (m_objective == "impregnationCount" && (m_enemies.Count == 0
			|| (i_fetus != null && Matches(i_fetus.GetNpcParent(), m_enemies)))) AddProgress("impregnationCount");
	}
	private void HandleRape()
	{
		if (m_objective == "rapeCount" && MatchesCurrentRaper()) AddProgress("rapeCount");
	}
	private void HandleOrgasm()
	{
		if (m_objective == "orgasmCount" && MatchesCurrentRaper()) AddProgress("orgasmCount");
	}
	private bool MatchesCurrentRaper()
	{
		if (m_enemies.Count == 0) return true;
		Raper raper = m_player == null ? null : m_player.GetRaperCurrent();
		return raper != null && Matches(raper.GetNPC(), m_enemies);
	}
	private void HandleMindBreak() { AddProgress("mindBreakCount"); }
	private void HandleUsableUsed(Usable i_usable)
	{
		if (m_objective == "useItemCount" && (m_items.Count == 0 || Matches(i_usable, m_items))) AddProgress("useItemCount");
	}
	private void HandleWaveStart()
	{
		m_tookDamageThisWave = false;
		if (m_objective == "reachWave") TrackCompletion();
	}
	private void HandleWaveEnd()
	{
		string objectiveAtWaveEnd = m_objective;
		if (objectiveAtWaveEnd == "surviveWaves") AddProgress("surviveWaves");
		else if (objectiveAtWaveEnd == "flawlessWaves" && !m_tookDamageThisWave) AddProgress("flawlessWaves");
	}

	private static bool Matches(Component i_component, HashSet<ContentId> i_ids)
	{
		return i_component != null && RuntimeContentIdentity.TryResolve(i_component, out ContentId id, out ContentCategory _)
			&& i_ids.Contains(id);
	}
}

public sealed class ExternalChallengeFactoryHost : MonoBehaviour
{
	private bool m_started;
	public void Begin(ManagerChallenge i_manager) { if (m_started) return; m_started = true; StartCoroutine(BuildLater(i_manager)); }
	private IEnumerator BuildLater(ManagerChallenge i_manager) { yield return null; yield return null; ExternalChallengeFactory.Build(i_manager); }
}
