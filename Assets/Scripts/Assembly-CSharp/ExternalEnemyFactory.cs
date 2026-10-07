using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CaptivityReloaded.Modding;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

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
		ExternalEnemyFactoryHost host = ExternalFactoryRunner.GetOrAdd<ExternalEnemyFactoryHost>();
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
			if (!packs.TryGetValue(definition.PackId, out ModPack pack))
			{
				Report("enemy.factory-pack", "Enemy pack is not loaded: " + definition.PackId, definition.Source);
				continue;
			}
			if (!definition.Extends.HasValue)
			{
				BuildOriginal(definition, pack, i_library);
				continue;
			}
			if (!ModLoaderRuntime.Registry.TryGet(definition.Extends.Value, out ContentRegistration baseEntry) || !(baseEntry.RuntimeAsset is NPC template))
			{
				Report("enemy.factory-template", "Core enemy template is not bound: " + definition.Extends.Value, definition.Source);
				continue;
			}

			NPC clone = UnityEngine.Object.Instantiate(template, i_library.transform);
			clone.gameObject.name = definition.Id.ToString();
			clone.gameObject.SetActive(false);
			RuntimeContentIdentity identity = clone.GetComponent<RuntimeContentIdentity>();
			if (identity == null) identity = clone.gameObject.AddComponent<RuntimeContentIdentity>();
			identity.Configure(definition.Id, ContentCategory.Enemy);
			bool attacksValid = true;
			foreach (EnemyAttackDefinition attack in definition.Attacks)
				if (attack.Index >= clone.GetAttackCount())
				{
					Report("enemy.factory-attack", "Template has no attack at index " + attack.Index + ".", definition.Source);
					attacksValid = false;
				}
			if (!attacksValid)
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			clone.ConfigureModEnemy(definition.DisplayName, definition.Description, definition.Stats);
			clone.ConfigureModEnemyGameplay(definition.Behavior, definition.Attacks, definition.Animation);
			if (definition.Behavior.Modules.Count > 0)
			{
				ModEnemyBehaviorController behaviorController = clone.GetComponent<ModEnemyBehaviorController>();
				if (behaviorController == null) behaviorController = clone.gameObject.AddComponent<ModEnemyBehaviorController>();
				behaviorController.Configure(definition.Id.ToString());
			}
			List<PickUpable> drops = new List<PickUpable>();
			foreach (string dropIdText in definition.Drops.Items)
			{
				ContentId dropId = ContentId.Parse(dropIdText);
				if (ModLoaderRuntime.Registry.TryGet(dropId, out ContentRegistration drop) && drop.RuntimeAsset is PickUpable item)
					drops.Add(item);
				else Report("enemy.factory-drop", "Drop item is not bound: " + dropId, definition.Source);
			}
			if (definition.Drops.Chance.HasValue || definition.Drops.Items.Count > 0)
				clone.ConfigureModDrops(definition.Drops.Chance, drops);
			if (definition.Visual != null && !ApplyAtlas(clone, definition, pack.RootPath))
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			ModLoaderRuntime.Registry.BindRuntimeAsset(definition.Id, clone, ModLoaderRuntime.LastReport);
			int spawners = definition.Spawn.InheritTemplateSpawners
				? AddToTemplateSpawners(i_stageManager, template, clone, definition.Spawn.SelectionWeight ?? 1f) : 0;
			Debug.Log("[ModLoader] Built external enemy " + definition.Id + " from " + definition.Extends + ".");
			if (definition.Spawn.InheritTemplateSpawners)
				Debug.Log("[ModLoader] Added external enemy " + definition.Id + " to " + spawners + " inherited stage spawners.");
		}
	}

	private static void BuildOriginal(EnemyDefinition i_definition, ModPack i_pack, LibraryActors i_library)
	{
		if (!RuntimePngAssetLoader.TryLoad(i_pack.RootPath, i_definition.Visual.Atlas, i_definition.Id + "/atlas",
			FilterMode.Point, ModLoaderRuntime.LastReport, "enemy.factory-atlas", "enemy.factory-atlas-decode",
			i_definition.Source, out Texture2D atlas)) return;
		Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
		Dictionary<string, Sprite> animationSprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
		foreach (KeyValuePair<string, AtlasRegionDefinition> pair in i_definition.Visual.Regions)
		{
			AtlasRegionDefinition area = pair.Value;
			Rect rect = new Rect(area.X, area.Y, area.Width, area.Height);
			if (rect.xMax > atlas.width || rect.yMax > atlas.height)
			{
				Report("enemy.factory-region-bounds", "Atlas region is outside the PNG: " + pair.Key, i_definition.Source);
				UnityEngine.Object.Destroy(atlas);
				return;
			}
		}

		GameObject root = new GameObject(i_definition.Id.ToString());
		root.SetActive(false);
		root.transform.SetParent(i_library.transform, false);
		root.layer = LayerMask.NameToLayer("Actor");
		Rigidbody2D body = root.AddComponent<Rigidbody2D>();
		body.gravityScale = i_definition.Ai != null && i_definition.Ai.Type == "flyingChase" ? 0f : 1f;
		body.freezeRotation = true;
		body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		CapsuleCollider2D bodyCollider = root.AddComponent<CapsuleCollider2D>();
		bodyCollider.size = new Vector2(i_definition.Visual.BodyWidth.Value, i_definition.Visual.BodyHeight.Value);
		bodyCollider.offset = new Vector2(i_definition.Visual.BodyOffsetX ?? 0f, i_definition.Visual.BodyOffsetY ?? 0f);
		root.AddComponent<SpriteRenderer>().enabled = false;

		GameObject skeletonObject = new GameObject("Skeleton");
		skeletonObject.transform.SetParent(root.transform, false);
		skeletonObject.layer = root.layer;
		Animator animator = skeletonObject.AddComponent<Animator>();
		animator.enabled = false;
		Dictionary<string, Transform> bones = new Dictionary<string, Transform>(StringComparer.Ordinal);
		List<EnemyBoneDefinition> pending = new List<EnemyBoneDefinition>(i_definition.Visual.Bones);
		while (pending.Count > 0)
		{
			bool progressed = false;
			for (int index = pending.Count - 1; index >= 0; index--)
			{
				EnemyBoneDefinition definition = pending[index];
				if (!string.IsNullOrEmpty(definition.Parent) && !bones.ContainsKey(definition.Parent)) continue;
				Transform parent = string.IsNullOrEmpty(definition.Parent) ? skeletonObject.transform : bones[definition.Parent];
				GameObject boneObject = new GameObject(definition.Id);
				boneObject.layer = root.layer;
				boneObject.transform.SetParent(parent, false);
				boneObject.transform.localPosition = new Vector2(definition.X, definition.Y);
				boneObject.transform.localRotation = Quaternion.Euler(0f, 0f, definition.Rotation);
				Rigidbody2D boneBody = boneObject.AddComponent<Rigidbody2D>();
				boneBody.isKinematic = true;
				boneObject.AddComponent<Bone>();
				if (!string.IsNullOrEmpty(definition.Parent))
				{
					HingeJoint2D joint = boneObject.AddComponent<HingeJoint2D>();
					joint.connectedBody = parent.GetComponent<Rigidbody2D>();
					joint.autoConfigureConnectedAnchor = true;
					joint.useLimits = true;
					joint.limits = new JointAngleLimits2D { min = -75f, max = 75f };
				}
				GameObject part = new GameObject("bp_" + definition.Id);
				part.layer = LayerMask.NameToLayer("BodyPart");
				part.transform.SetParent(boneObject.transform, false);
				SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
				AtlasRegionDefinition area = i_definition.Visual.Regions[definition.Region];
				Sprite sprite = Sprite.Create(atlas, new Rect(area.X, area.Y, area.Width, area.Height),
					new Vector2(definition.PivotX, definition.PivotY), i_definition.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect);
				sprite.name = i_definition.Id + "/" + definition.Region;
				renderer.sprite = sprite;
				renderer.sortingLayerName = "Actor";
				renderer.sortingOrder = definition.SortingOrder;
				sprites[definition.Id] = sprite;
				animationSprites[definition.Id + "|" + definition.Region] = sprite;
				BodyPartActor bodyPart = part.AddComponent<BodyPartActor>();
				EnemyHitZoneDefinition zone = i_definition.Visual.HitZones.Find(item => item.Bone == definition.Id);
				if (zone != null)
				{
					Collider2D hitCollider;
					if (zone.Shape == "circle") { CircleCollider2D circle = part.AddComponent<CircleCollider2D>(); circle.radius = zone.Radius.Value; hitCollider = circle; }
					else { BoxCollider2D box = part.AddComponent<BoxCollider2D>(); box.size = new Vector2(zone.Width.Value, zone.Height.Value); hitCollider = box; }
					hitCollider.offset = new Vector2(zone.OffsetX, zone.OffsetY);
					hitCollider.isTrigger = true;
					bodyPart.ConfigureModHitZone(zone.DamageMultiplier, true);
				}
				else bodyPart.ConfigureModHitZone("normal", true);
				bones[definition.Id] = boneObject.transform;
				pending.RemoveAt(index);
				progressed = true;
			}
			if (!progressed) { Report("enemy.factory-skeleton", "Could not resolve the original skeleton hierarchy.", i_definition.Source); UnityEngine.Object.Destroy(root); return; }
		}
		foreach (KeyValuePair<string, EnemyAnimationClipDefinition> clip in i_definition.Animation.Clips)
			foreach (EnemyAnimationFrameDefinition frame in clip.Value.Frames)
				foreach (KeyValuePair<string, EnemyBonePoseDefinition> pose in frame.Bones)
				{
					if (pose.Value == null || string.IsNullOrEmpty(pose.Value.Region)) continue;
					string key = pose.Key + "|" + pose.Value.Region;
					if (animationSprites.ContainsKey(key)) continue;
					EnemyBoneDefinition bone = i_definition.Visual.Bones.Find(item => item.Id == pose.Key);
					AtlasRegionDefinition area = i_definition.Visual.Regions[pose.Value.Region];
					Sprite sprite = Sprite.Create(atlas, new Rect(area.X, area.Y, area.Width, area.Height),
						new Vector2(bone.PivotX, bone.PivotY), i_definition.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect);
					sprite.name = i_definition.Id + "/" + pose.Key + "/" + pose.Value.Region;
					animationSprites[key] = sprite;
				}
		foreach (KeyValuePair<string, EnemyAnimationClipDefinition> clip in i_definition.Animation.Clips)
			foreach (EnemyAnimationEventDefinition animationEvent in clip.Value.Events ?? new List<EnemyAnimationEventDefinition>())
			{
				if (animationEvent == null || animationEvent.Type != "spriteEffect") continue;
				string key = "@event|" + animationEvent.Region;
				if (animationSprites.ContainsKey(key)) continue;
				AtlasRegionDefinition area = i_definition.Visual.Regions[animationEvent.Region];
				Sprite sprite = Sprite.Create(atlas, new Rect(area.X, area.Y, area.Width, area.Height),
					new Vector2(0.5f, 0.5f), i_definition.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect);
				sprite.name = i_definition.Id + "/event/" + animationEvent.Region;
				animationSprites[key] = sprite;
			}
		HashSet<string> projectileRegions = new HashSet<string>(StringComparer.Ordinal);
		foreach (EnemyAttackDefinition attack in i_definition.Attacks)
		{
			if (!string.IsNullOrEmpty(attack.ProjectileRegion)) projectileRegions.Add(attack.ProjectileRegion);
			foreach (EnemyAttackStageDefinition attackStage in attack.Stages ?? new List<EnemyAttackStageDefinition>())
				if (attackStage != null && !string.IsNullOrEmpty(attackStage.ProjectileRegion)) projectileRegions.Add(attackStage.ProjectileRegion);
		}
		foreach (string region in projectileRegions)
		{
			string key = "@event|" + region;
			if (animationSprites.ContainsKey(key)) continue;
			AtlasRegionDefinition area = i_definition.Visual.Regions[region];
			Sprite sprite = Sprite.Create(atlas, new Rect(area.X, area.Y, area.Width, area.Height),
				new Vector2(0.5f, 0.5f), i_definition.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect);
			sprite.name = i_definition.Id + "/projectile/" + region;
			animationSprites[key] = sprite;
		}
		skeletonObject.AddComponent<SkeletonActor>();
		ModularEnemy npc = root.AddComponent<ModularEnemy>();
		List<AttackNPC> attacks = new List<AttackNPC>();
		foreach (EnemyAttackDefinition definition in i_definition.Attacks)
		{
			GameObject attackObject = new GameObject("attack_" + definition.Id);
			attackObject.transform.SetParent(root.transform, false);
			AttackNPC attack = attackObject.AddComponent<AttackNPC>();
			attack.ConfigureModAttack(definition);
			attacks.Add(attack);
		}
		npc.ConfigureOriginal(i_definition.Id.ToString(), attacks);
		npc.ConfigureModEnemy(i_definition.DisplayName, i_definition.Description, i_definition.Stats);
		npc.ConfigureModEnemyGameplay(i_definition.Behavior, null, null);
		if (sprites.TryGetValue("head", out Sprite icon) || sprites.TryGetValue("hips", out icon)) npc.SetModIcon(icon);
		ConfigureDrops(npc, i_definition);
		root.AddComponent<ModularEnemySpriteSet>().Configure(animationSprites);
		root.AddComponent<ModularEnemyAnimationController>().Configure(i_definition.Id.ToString());
		ModularEnemyEventAssets eventAssets = root.AddComponent<ModularEnemyEventAssets>();
		eventAssets.Configure(i_definition.Id.ToString());
		ExternalFactoryRunner.GetOrAdd<ModularEnemyAudioLoader>().Load(eventAssets, i_definition, i_pack.RootPath);
		if (i_definition.Behavior.Modules.Count > 0) root.AddComponent<ModEnemyBehaviorController>().Configure(i_definition.Id.ToString());
		EnemyBehaviorModuleDefinition finisher = i_definition.Behavior.Modules.Find(item => item != null && item.Type == "downedFinisher");
		if (finisher != null) root.AddComponent<ModularEnemyFinisher>().Configure(i_definition.Id.ToString());
		RuntimeContentIdentity identity = root.AddComponent<RuntimeContentIdentity>();
		identity.Configure(i_definition.Id, ContentCategory.Enemy);
		ModLoaderRuntime.Registry.BindRuntimeAsset(i_definition.Id, npc, ModLoaderRuntime.LastReport);
		Debug.Log("[ModLoader] Built fully original enemy " + i_definition.Id + ".");
	}

	private static void ConfigureDrops(NPC i_npc, EnemyDefinition i_definition)
	{
		List<PickUpable> drops = new List<PickUpable>();
		foreach (string dropIdText in i_definition.Drops.Items)
		{
			ContentId dropId = ContentId.Parse(dropIdText);
			if (ModLoaderRuntime.Registry.TryGet(dropId, out ContentRegistration drop) && drop.RuntimeAsset is PickUpable item) drops.Add(item);
			else Report("enemy.factory-drop", "Drop item is not bound: " + dropId, i_definition.Source);
		}
		if (i_definition.Drops.Chance.HasValue || i_definition.Drops.Items.Count > 0) i_npc.ConfigureModDrops(i_definition.Drops.Chance, drops);
	}

	private static int AddToTemplateSpawners(ManagerStages i_stageManager, NPC i_template, NPC i_variant, float i_selectionWeight)
	{
		if (i_stageManager == null) return 0;
		int count = 0;
		foreach (Stage stage in i_stageManager.GetAllStages())
		{
			if (stage == null) continue;
			foreach (Spawner spawner in stage.GetComponentsInChildren<Spawner>(true))
				if (spawner.AddNpcVariant(i_template, i_variant, i_selectionWeight)) count++;
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
				Vector2 pivot = RuntimePngAssetLoader.GetReplacementPivot(baseline, area.Width, area.Height);
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
	}
}

public sealed class ModEnemyBehaviorController : MonoBehaviour
{
	[SerializeField] private string m_definitionId;
	private NPC m_npc;
	private IReadOnlyList<EnemyBehaviorModuleDefinition> m_modules;
	private float m_nextRegeneration;
	private bool m_berserkActive;
	private float m_nextSpeedPulse;
	private float m_speedPulseEnd;
	private readonly List<StatModifier> m_speedPulseModifiers = new List<StatModifier>();
	private bool m_deathModulesHandled;

	public void Configure(string i_definitionId)
	{
		m_definitionId = i_definitionId;
		Resolve();
	}

	private void OnEnable()
	{
		Resolve();
		m_berserkActive = false;
		m_nextRegeneration = Time.time;
		m_nextSpeedPulse = Time.time;
		m_speedPulseEnd = 0f;
		m_deathModulesHandled = false;
		if (m_npc != null)
		{
			m_npc.OnHit += OnNpcHit;
			m_npc.OnGetHit += OnNpcGetHit;
		}
	}

	private void OnDisable()
	{
		if (m_npc != null)
		{
			m_npc.OnHit -= OnNpcHit;
			m_npc.OnGetHit -= OnNpcGetHit;
			if (m_speedPulseModifiers.Count > 0) m_npc.RemoveStatModifier(m_speedPulseModifiers);
		}
		m_speedPulseModifiers.Clear();
	}

	private void Update()
	{
		if (m_npc == null || m_modules == null || m_npc.IsDead()) return;
		foreach (EnemyBehaviorModuleDefinition module in m_modules)
		{
			if (module.Type == "regeneration" && Time.time >= m_nextRegeneration)
			{
				m_npc.RestoreHealth(module.Amount.Value);
				m_nextRegeneration = Time.time + module.IntervalSeconds.Value;
			}
			else if (module.Type == "berserk" && !m_berserkActive &&
				m_npc.GetHealthCurrent() / Mathf.Max(0.01f, m_npc.GetStat("HealthMax").GetValueTotal()) <= module.HealthThreshold.Value)
			{
				m_berserkActive = true;
				if (module.SpeedBonus.HasValue)
				{
					m_npc.AddStatModifier("SpeedAccel", module.SpeedBonus.Value);
					m_npc.AddStatModifier("SpeedMax", module.SpeedBonus.Value);
				}
				if (module.DamageMultiplierBonus.HasValue) m_npc.AddStatModifier("DamageMultiplier", module.DamageMultiplierBonus.Value);
			}
			else if (module.Type == "speedPulse")
			{
				if (m_speedPulseModifiers.Count > 0 && Time.time >= m_speedPulseEnd)
				{
					m_npc.RemoveStatModifier(m_speedPulseModifiers);
					m_speedPulseModifiers.Clear();
				}
				if (m_speedPulseModifiers.Count == 0 && Time.time >= m_nextSpeedPulse)
				{
					m_speedPulseModifiers.Add(m_npc.AddStatModifier("SpeedAccel", module.SpeedBonus.Value));
					m_speedPulseModifiers.Add(m_npc.AddStatModifier("SpeedMax", module.SpeedBonus.Value));
					m_speedPulseEnd = Time.time + module.DurationSeconds.Value;
					m_nextSpeedPulse = Time.time + module.IntervalSeconds.Value;
				}
			}
		}
	}

	private void OnNpcHit(Actor i_attacker, Actor i_receiver)
	{
		if (m_modules == null || m_npc == null) return;
		foreach (EnemyBehaviorModuleDefinition module in m_modules)
			if (module.Type == "lifesteal") m_npc.RestoreHealth(module.Amount.Value);
			else if (module.Type == "onHitRagdoll" && i_receiver != null) i_receiver.Ragdoll(module.DurationSeconds.Value);
	}

	private void OnNpcGetHit(Actor i_attacker, Actor i_receiver)
	{
		if (m_modules == null || i_attacker == null) return;
		foreach (EnemyBehaviorModuleDefinition module in m_modules)
			if (module.Type == "thorns") i_attacker.TakeDamage(module.Amount.Value);
	}

	public void HandleDeathModules()
	{
		if (m_deathModulesHandled || m_modules == null || m_npc == null) return;
		m_deathModulesHandled = true;
		foreach (EnemyBehaviorModuleDefinition module in m_modules)
		{
			if (module.Type == "spawnOnDeath" && ContentId.TryParse(module.Enemy, out ContentId enemyId)
				&& ModLoaderRuntime.Registry.TryGet(enemyId, out ContentRegistration entry) && entry.RuntimeAsset is NPC template)
			{
				ManagerStages stages = CommonReferences.Instance.GetManagerStages();
				Stage stage = stages == null ? null : stages.GetStageCurrent();
				if (stage == null) continue;
				float radius = module.Radius ?? 0f;
				for (int count = 0; count < module.Count.Value; count++)
				{
					NPC spawned = UnityEngine.Object.Instantiate(template, stage.GetActorsParent());
					Vector2 position = m_npc.GetPos() + UnityEngine.Random.insideUnitCircle * radius;
					spawned.transform.position = position;
					spawned.gameObject.SetActive(true);
					spawned.Spawn(i_isFadeIn: true);
					if (spawned is Walker) spawned.PlaceFeetOnPos(position);
				}
			}
		}
	}

	private void Resolve()
	{
		m_npc = GetComponent<NPC>();
		if (string.IsNullOrEmpty(m_definitionId)) return;
		foreach (EnemyDefinition definition in ModLoaderRuntime.EnemyDefinitions)
			if (definition.Id.ToString() == m_definitionId)
			{
				m_modules = definition.Behavior.Modules;
				return;
			}
	}
}

public sealed class ModularEnemy : Walker
{
	[SerializeField] private string m_definitionId;
	private EnemyDefinition m_definition;
	private ModularEnemyAnimationController m_modAnimation;
	private float m_nextThink;
	private Actor m_animationAttackTarget;
	private EnemyAttackDefinition m_animationAttackDefinition;
	private int m_animationAttackHitsRemaining;
	private int m_animationAttackStageIndex;
	private bool m_animationAttackCooldownStarted;
	private Path m_navigationPath;
	private NavNode m_navigationStart;
	private Platform m_navigationEnemyPlatform;
	private Platform m_navigationPlayerPlatform;
	private float m_nextNavigationRefresh;
	private Coroutine m_navigationTraversal;
	private float m_navigationGravity;
	private float m_nextDirectClimb;

	public void ConfigureOriginal(string i_definitionId, List<AttackNPC> i_attacks)
	{
		m_definitionId = i_definitionId;
		m_attacks = i_attacks ?? new List<AttackNPC>();
		m_interactions = new List<Interaction>();
		Resolve();
	}

	public override void Awake()
	{
		base.Awake();
		Resolve();
		m_modAnimation = GetComponent<ModularEnemyAnimationController>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_nextThink = Time.time;
		m_navigationPath = null;
		m_navigationStart = null;
		m_navigationTraversal = null;
		m_isCanClimb = true;
	}

	public override void FixedUpdate()
	{
		m_isGrounded = GetIsGroundedRayCast();
		base.FixedUpdate();
	}

	protected override void AddXAIComponent()
	{
		Resolve();
		if (m_definition == null || m_definition.Ai == null || m_definition.Ai.Type != "groundChase") return;
		// Use the same path-node, zero-gravity jump, ledge lookup, and climb
		// sequence as Core walkers. Modular attacks remain virtual on this actor.
		m_xAI = gameObject.AddComponent<XAIWalker>();
		m_xAI.Initialize(this);
	}
	protected override void RetrieveAnimationInfos(RuntimeAnimatorController i_runtimeAnimatorController) { }

	protected override void HandleThinking()
	{
		if (m_definition == null || Time.time < m_nextThink || CommonReferences.Instance == null || CommonReferences.Instance.GetPlayer() == null) return;
		m_nextThink = Time.time + (m_definition.Ai?.ReactionSeconds ?? 0.1f);
		Player player = CommonReferences.Instance.GetPlayer();
		ModularEnemyFinisher activeFinisher = GetComponent<ModularEnemyFinisher>();
		if (activeFinisher != null && activeFinisher.GetIsActive())
		{
			StopMovingHorizontally();
			return;
		}
		if (ModularEnemyFinisher.ShouldApproachActiveSession(this))
		{
			MoveToPlayer();
			return;
		}
		ModularEnemyFinisher voluntaryFinisher = GetComponent<ModularEnemyFinisher>();
		if ((player.GetStateActorCurrent() == StateActor.Ragdoll || player.IsExposing()) && voluntaryFinisher != null)
		{
			if (voluntaryFinisher.ShouldApproachDownedPlayer(player)) MoveToPlayer();
			else StopMovingHorizontally();
			return;
		}
		if (player.GetStateActorCurrent() == StateActor.Ragdoll)
		{
			StopMovingHorizontally();
			return;
		}
		if (player.IsDead() || player.GetIsBeingRaped() || player.GetStatePlayerCurrent() == StatePlayer.Labor)
		{
			StopMovingHorizontally();
			return;
		}
		if (m_definition.Ai.Type == "groundChase" && TryClimbTowardHigherPlayer(player)) return;
		if (m_definition.Ai.Type == "groundChase" && m_xAI != null)
		{
			m_xAI.HandleIntelligence();
			return;
		}
		float distance = GetDistanceBetweenPlayerHips();
		if (distance > GetRangeVision())
		{
			if (m_definition.Ai.Type == "flyingChase") StopMoving(); else StopMovingHorizontally();
			return;
		}
		// Match XAIWalker: ground enemies enter combat only after reaching the
		// player's platform. Otherwise ranged attacks can prevent path traversal.
		bool canAttackFromCurrentPlatform = m_definition.Ai.Type == "flyingChase"
			|| (GetPlatformCurrent() != null && GetPlatformCurrent() == player.GetPlatformCurrent());
		if (!m_isAttacking && m_isCanAttack && m_attackCurrent != null && canAttackFromCurrentPlatform
			&& GetStateActorCurrent() != StateActor.Jumping && distance <= GetRangeInitiateAttackAttackCurrent())
		{
			FacePlayer();
			StartAttack();
		}
		else if (!m_isAttacking)
		{
			float preferred = m_definition.Ai?.PreferredRange ?? 0.8f;
			if (m_definition.Ai.Type == "holdPosition") StopMovingHorizontally();
			else if (m_definition.Ai.RetreatRange.HasValue && distance < m_definition.Ai.RetreatRange.Value) MoveAwayFromPlayer();
			else if (distance > preferred) MoveToPlayer();
			else if (m_definition.Ai.Type == "flyingChase") StopMoving();
			else StopMovingHorizontally();
		}
	}

	public override void MoveToPlayer()
	{
		Player player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
		if (player == null) return;
		if (m_definition != null && m_definition.Ai.Type == "flyingChase")
		{
			Vector2 direction = player.GetPosHips() - GetPos();
			SetIsFacingLeft(direction.x < 0f);
			GetRigidbody2D().velocity = direction.normalized * GetStat("SpeedMax").GetValueTotal();
		}
		else if (player.GetPlatformCurrent() != GetPlatformCurrent())
		{
			if (player.GetPosFeet().y < GetPosFeet().y - 0.5f)
				WalkOffPlatformTowards(player.GetPosFeet());
			else if (!NavigateToPlayerPlatform(player))
				MoveHorizontal(player.GetPosHips().x < GetPos().x);
		}
		else
		{
			ClearNavigationPath();
			MoveHorizontal(player.GetPosHips().x < GetPos().x);
		}
	}

	private bool TryClimbTowardHigherPlayer(Player i_player)
	{
		if (i_player == null || i_player.GetPosFeet().y <= GetPosFeet().y + 0.5f) return false;
		if (i_player.GetPlatformCurrent() != null && GetPlatformCurrent() != null
			&& i_player.GetPlatformCurrent() == GetPlatformCurrent()) return false;
		if (GetStateActorCurrent() == StateActor.Climbing || IsTryingToClimb()) return true;
		Stage stage = CommonReferences.Instance.GetManagerStages().GetStageCurrent();
		if (stage == null) return false;

		Ledge best = null;
		float bestRise = float.PositiveInfinity;
		float bestHorizontal = float.PositiveInfinity;
		foreach (Ledge ledge in stage.GetAllLedges())
		{
			if (ledge == null || !ledge.gameObject.activeInHierarchy) continue;
			BoxCollider2D collider = ledge.GetCollider();
			Platform platform = collider == null ? null : collider.GetComponent<Platform>();
			if (platform == null || platform == GetPlatformCurrent()) continue;
			// These flags control automatic player grabbing. Core XAIWalker does not
			// require them when an AI navigation route calls TryToClimb.
			float rise = ledge.GetPos().y - GetPosFeet().y;
			if (rise < 0.5f || ledge.GetPos().y > i_player.GetPosFeet().y + 0.75f) continue;
			float horizontal = Mathf.Abs(ledge.GetPos().x - GetPos().x);
			if (rise < bestRise - 0.1f || (Mathf.Abs(rise - bestRise) <= 0.1f && horizontal < bestHorizontal))
			{
				best = ledge;
				bestRise = rise;
				bestHorizontal = horizontal;
			}
		}
		if (best == null) return false;

		float deltaX = best.GetPos().x - GetPos().x;
		Collider2D actorCollider = GetComponent<Collider2D>();
		float climbReach = (actorCollider == null ? 0.5f : actorCollider.bounds.extents.x) + 0.35f;
		if (Mathf.Abs(deltaX) > climbReach)
		{
			MoveHorizontal(deltaX < 0f);
			return true;
		}
		if (Time.time < m_nextDirectClimb) return true;
		m_nextDirectClimb = Time.time + 0.75f;
		StopMoving();
		Vector2 jumpTarget = best.GetPos();
		jumpTarget.y -= GetHeight() * 0.5f;
		JumpZeroGravity(jumpTarget, 0.5f);
		TryToClimb(best);
		return true;
	}

	private bool NavigateToPlayerPlatform(Player i_player)
	{
		if (m_navigationTraversal != null) return true;
		ManagerStages stages = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetManagerStages();
		Stage stage = stages == null ? null : stages.GetStageCurrent();
		NavMap navMap = stage == null ? null : stage.GetNavMap();
		Platform enemyPlatform = GetPlatformCurrent();
		Platform playerPlatform = i_player.GetPlatformCurrent();
		if (navMap == null || enemyPlatform == null || playerPlatform == null) return false;

		if (m_navigationPath == null || Time.time >= m_nextNavigationRefresh
			|| enemyPlatform != m_navigationEnemyPlatform || playerPlatform != m_navigationPlayerPlatform)
		{
			ClearNavigationPath();
			m_navigationStart = navMap.CreateNpcStartNode(this);
			m_navigationPath = new PathFinder().CreatePathToPlayer(this, m_navigationStart);
			m_navigationEnemyPlatform = enemyPlatform;
			m_navigationPlayerPlatform = playerPlatform;
			m_nextNavigationRefresh = Time.time + 1f;
		}
		if (m_navigationPath == null) return false;

		PathNode current = m_navigationPath.GetPathNodeCurrent();
		while (current != null && Vector2.Distance(GetPosFeet(), current.GetNavNode().GetPos()) < 0.65f)
		{
			current.Complete();
			current = m_navigationPath.GetPathNodeCurrent();
			if (current == null) return false;
			NodeConnection connection = current.GetNodeConnectionToParent();
			if (connection != null && connection.GetNodeConnectionType() == NodeConnectionType.Climb)
			{
				Vector2 destination = current.GetNavNode().GetPos();
				// Descending enemies should leave a platform and fall. Authored climb
				// links are only used to reach a genuinely higher platform.
				if (destination.y <= GetPosFeet().y + 0.25f) return false;
				m_navigationTraversal = StartCoroutine(CoroutineTraverseNavigationLink(destination));
				return true;
			}
		}
		if (current == null) return false;
		MoveHorizontal(current.GetNavNode().GetPos().x < GetPos().x);
		return true;
	}

	private IEnumerator CoroutineTraverseNavigationLink(Vector2 i_destination)
	{
		m_isThinking = false;
		m_isCanMove = false;
		StopMoving();
		Rigidbody2D body = GetRigidbody2D();
		m_navigationGravity = body.gravityScale;
		body.gravityScale = 0f;
		Vector2 origin = GetPosFeet();
		float duration = Mathf.Clamp(Vector2.Distance(origin, i_destination) / 7f, 0.25f, 0.65f);
		for (float elapsed = 0f; elapsed < duration; elapsed += Time.fixedDeltaTime)
		{
			float amount = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			PlaceFeetOnPos(Vector2.Lerp(origin, i_destination, amount));
			yield return new WaitForFixedUpdate();
		}
		PlaceFeetOnPos(i_destination);
		body.velocity = Vector2.zero;
		body.gravityScale = m_navigationGravity;
		m_isCanMove = true;
		m_isThinking = true;
		m_navigationTraversal = null;
		m_navigationPath = null;
		m_nextThink = Time.time;
	}

	private void ClearNavigationPath()
	{
		if (m_navigationStart != null && CommonReferences.Instance != null)
		{
			ManagerStages stages = CommonReferences.Instance.GetManagerStages();
			Stage stage = stages == null ? null : stages.GetStageCurrent();
			NavMap navMap = stage == null ? null : stage.GetNavMap();
			if (navMap != null) navMap.DestroyNode(m_navigationStart);
		}
		m_navigationStart = null;
		m_navigationPath = null;
	}

	public override void MoveAwayFromPlayer()
	{
		Player player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
		if (player == null) return;
		if (m_definition != null && m_definition.Ai.Type == "flyingChase")
		{
			Vector2 direction = GetPos() - player.GetPosHips();
			SetIsFacingLeft(direction.x < 0f);
			GetRigidbody2D().velocity = direction.normalized * GetStat("SpeedMax").GetValueTotal();
		}
		else MoveHorizontal(player.GetPosHips().x >= GetPos().x);
	}

	public override void UpdateAnim()
	{
		if (m_modAnimation == null) m_modAnimation = GetComponent<ModularEnemyAnimationController>();
		ModularEnemyFinisher finisher = GetComponent<ModularEnemyFinisher>();
		if (m_modAnimation == null || m_isAttacking || m_isDead || (finisher != null && finisher.GetIsActive())) return;
		m_modAnimation.Play(Mathf.Abs(GetVelocity().x) > 0.1f ? "move" : "idle");
	}

	protected override IEnumerator CoroutineAttack(Actor i_targetActor)
	{
		m_isAttacking = true;
		int index = m_attacks.IndexOf(m_attackCurrent);
		EnemyAttackDefinition definition = m_definition != null && index >= 0 && index < m_definition.Attacks.Count ? m_definition.Attacks[index] : null;
		if (definition == null) { m_isAttacking = false; yield break; }
		if (!m_attackCurrent.IsMovesDuringAttack()) { m_isCanMove = false; StopMovingHorizontally(); }
		m_animationAttackTarget = i_targetActor;
		m_animationAttackDefinition = definition;
		m_animationAttackStageIndex = 0;
		m_animationAttackCooldownStarted = false;
		m_modAnimation?.Play(definition.Animation, true);
		bool eventDrivenHit = m_modAnimation != null && m_modAnimation.HasEvent(definition.Animation, "attackHit");
		m_animationAttackHitsRemaining = eventDrivenHit ? m_modAnimation.CountEvents(definition.Animation, "attackHit") : 1;
		float elapsed = 0f;
		if (!eventDrivenHit)
		{
			float hitTime = definition.HitTimeSeconds ?? 0f;
			if (hitTime > 0f) yield return new WaitForSeconds(hitTime);
			elapsed = hitTime;
			PerformConfiguredAttack(i_targetActor, definition, true);
		}
		float remaining = Mathf.Max(0f, (definition.DurationSeconds ?? elapsed) - elapsed);
		if (remaining > 0f) yield return new WaitForSeconds(remaining);
		m_attackCurrent.HandleAttackEnd();
		m_animationAttackTarget = null;
		m_animationAttackDefinition = null;
		m_animationAttackHitsRemaining = 0;
		m_animationAttackStageIndex = 0;
		m_isAttacking = false;
		m_isCanMove = true;
	}

	public void HandleAnimationEvent(EnemyAnimationEventDefinition i_event)
	{
		if (!m_isAttacking || m_isDead || i_event == null) return;
		if (i_event.Type == "attackHit")
		{
			m_animationAttackHitsRemaining = Mathf.Max(0, m_animationAttackHitsRemaining - 1);
			EnemyAttackStageDefinition stage = null;
			if (m_animationAttackDefinition != null && m_animationAttackDefinition.Type == "multiStage"
				&& m_animationAttackDefinition.Stages != null && m_animationAttackStageIndex < m_animationAttackDefinition.Stages.Count)
				stage = m_animationAttackDefinition.Stages[m_animationAttackStageIndex++];
			PerformConfiguredAttack(m_animationAttackTarget, m_animationAttackDefinition, stage, m_animationAttackHitsRemaining == 0);
		}
		else if (i_event.Type == "impulse")
		{
			Rigidbody2D body = GetRigidbody2D();
			if (body == null || body.bodyType != RigidbodyType2D.Dynamic) return;
			float x = i_event.X ?? 0f;
			if ((i_event.RelativeToFacing ?? true) && GetIsFacingLeft()) x = -x;
			body.AddForce(new Vector2(x, i_event.Y ?? 0f), ForceMode2D.Impulse);
		}
	}

	private void PerformConfiguredAttack(Actor i_targetActor, EnemyAttackDefinition i_definition, bool i_startCooldown)
	{
		PerformConfiguredAttack(i_targetActor, i_definition, null, i_startCooldown);
	}

	private void PerformConfiguredAttack(Actor i_targetActor, EnemyAttackDefinition i_definition,
		EnemyAttackStageDefinition i_stage, bool i_startCooldown)
	{
		Player player = i_targetActor as Player;
		if (i_definition == null || i_targetActor == null || player == null || m_isDead || player.IsDead()
			|| player.GetIsBeingRaped()
			|| player.GetStatePlayerCurrent() == StatePlayer.Labor) return;
		if (i_startCooldown && !m_animationAttackCooldownStarted)
		{
			m_animationAttackCooldownStarted = true;
			StartAttackCooldown();
		}
		string type = i_stage == null ? i_definition.Type : i_stage.Type;
		if (type == "hitscan") PerformHitscanAttack(player, i_definition, i_stage);
		else if (type == "projectile") SpawnProjectileAttack(player, i_definition, i_stage);
		else if (type == "area") PerformAreaAttack(player, i_definition, i_stage);
		else if (type == "grab") PerformGrabAttack(player, i_definition, i_stage);
		else PerformMeleeAttack(player, i_definition, i_stage);
	}

	private void PerformMeleeAttack(Player i_player, EnemyAttackDefinition i_definition, EnemyAttackStageDefinition i_stage)
	{
		if (i_player.GetStateActorCurrent() == StateActor.Ragdoll) return;
		float range = Resolve(i_stage?.HitRange, i_definition.HitRange, 1f);
		if (Vector2.Distance(GetPosHips(), i_player.GetPosHips()) <= range) ApplyConfiguredHit(i_player, i_definition, i_stage);
	}

	private void PerformHitscanAttack(Player i_player, EnemyAttackDefinition i_definition, EnemyAttackStageDefinition i_stage)
	{
		if (i_player == null || i_player.GetStateActorCurrent() == StateActor.Ragdoll || !IsHasLineOfSightToPlayerRaycast()) return;
		Vector2 origin = GetSkeletonActor() == null ? GetPos() : (Vector2)GetSkeletonActor().GetBoneHead().transform.position;
		Vector2 target = i_player.GetPosHips();
		if (ResourceContainer.Resources != null && ResourceContainer.Resources.m_lineShoot != null)
		{
			SpriteRenderer line = UnityEngine.Object.Instantiate(ResourceContainer.Resources.m_lineShoot,
				CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform);
			Vector2 direction = target - origin;
			line.transform.position = origin;
			line.transform.right = direction;
			line.transform.localScale = new Vector3(direction.magnitude * 32f, 1f, 1f);
			line.color = new Color(1f, 0.35f, 0.2f, 1f);
			line.gameObject.SetActive(true);
			line.enabled = true;
			UnityEngine.Object.Destroy(line.gameObject, 0.15f);
		}
		if (Vector2.Distance(origin, target) <= Resolve(i_stage?.HitRange, i_definition.HitRange, 1f))
			ApplyConfiguredHit(i_player, i_definition, i_stage);
	}

	private void PerformAreaAttack(Player i_player, EnemyAttackDefinition i_definition, EnemyAttackStageDefinition i_stage)
	{
		if (i_player.GetStateActorCurrent() == StateActor.Ragdoll) return;
		float x = Resolve(i_stage?.OffsetX, i_definition.OffsetX, 0f);
		if (GetIsFacingLeft()) x = -x;
		Vector2 center = GetPos() + new Vector2(x, Resolve(i_stage?.OffsetY, i_definition.OffsetY, 0f));
		float radius = Resolve(i_stage?.AreaRadius, i_definition.AreaRadius, 1f);
		if (Vector2.Distance(center, i_player.GetPosHips()) <= radius) ApplyConfiguredHit(i_player, i_definition, i_stage);
	}

	private void PerformGrabAttack(Player i_player, EnemyAttackDefinition i_definition, EnemyAttackStageDefinition i_stage)
	{
		float range = Resolve(i_stage?.HitRange, i_definition.HitRange, 1f);
		if (Vector2.Distance(GetPosHips(), i_player.GetPosHips()) > range) return;
		ModularEnemyFinisher finisher = GetComponent<ModularEnemyFinisher>();
		if (finisher != null) finisher.TryBeginFromAttack(i_player);
	}

	private void SpawnProjectileAttack(Player i_player, EnemyAttackDefinition i_definition, EnemyAttackStageDefinition i_stage)
	{
		Vector2 origin = GetSkeletonActor() == null ? GetPos() : (Vector2)GetSkeletonActor().GetBoneHead().transform.position;
		Vector2 displacement = i_player.GetPosHips() - origin;
		Vector2 direction = displacement.normalized;
		if (direction.sqrMagnitude <= 0.001f) direction = GetIsFacingLeft() ? Vector2.left : Vector2.right;
		string region = i_stage?.ProjectileRegion ?? i_definition.ProjectileRegion;
		ModularEnemySpriteSet sprites = GetComponent<ModularEnemySpriteSet>();
		if (sprites == null || !sprites.TryGetEvent(region, out Sprite sprite)) return;
		GameObject projectileObject = new GameObject("mod-enemy-projectile_" + i_definition.Id);
		projectileObject.layer = LayerMask.NameToLayer("Projectile");
		Stage stage = CommonReferences.Instance.GetManagerStages().GetStageCurrent();
		projectileObject.transform.SetParent(stage == null ? null : stage.transform, true);
		projectileObject.transform.position = origin;
		projectileObject.transform.right = direction;
		SpriteRenderer renderer = projectileObject.AddComponent<SpriteRenderer>();
		renderer.sprite = sprite;
		renderer.sortingLayerName = "Actor";
		renderer.sortingOrder = 100;
		Rigidbody2D body = projectileObject.AddComponent<Rigidbody2D>();
		float gravityScale = Resolve(i_stage?.ProjectileGravityScale, i_definition.ProjectileGravityScale, 0f);
		float projectileSpeed = Resolve(i_stage?.ProjectileSpeed, i_definition.ProjectileSpeed, 8f);
		body.gravityScale = gravityScale;
		body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		if (Mathf.Abs(gravityScale) > 0.001f)
		{
			// Compensate the launch velocity so gravity creates a visible arc that
			// still passes through a stationary target's firing-time position.
			float travelSeconds = Mathf.Clamp(displacement.magnitude / projectileSpeed, 0.1f,
				Resolve(i_stage?.ProjectileLifetimeSeconds, i_definition.ProjectileLifetimeSeconds, 4f));
			Vector2 gravity = Physics2D.gravity * gravityScale;
			body.velocity = displacement / travelSeconds - gravity * (0.5f * travelSeconds);
			projectileObject.transform.right = body.velocity.normalized;
		}
		else body.velocity = direction * projectileSpeed;
		CircleCollider2D collider = projectileObject.AddComponent<CircleCollider2D>();
		collider.isTrigger = true;
		collider.radius = Resolve(i_stage?.ProjectileRadius, i_definition.ProjectileRadius, 0.15f);
		float damage = Resolve(i_stage?.Damage, i_definition.Damage, 0f) * GetStat("DamageMultiplier").GetValueTotal();
		float knockbackX = Resolve(i_stage?.KnockbackX, i_definition.KnockbackX, 0f) * GetStat("KnockbackXMultiplier").GetValueTotal();
		float knockbackY = Resolve(i_stage?.KnockbackY, i_definition.KnockbackY, 0f) * GetStat("KnockbackYMultiplier").GetValueTotal();
		projectileObject.AddComponent<ModularEnemyProjectile>().Configure(this, damage, knockbackX, knockbackY,
			Resolve(i_stage?.ProjectileLifetimeSeconds, i_definition.ProjectileLifetimeSeconds, 4f));
	}

	private void ApplyConfiguredHit(Player i_player, EnemyAttackDefinition i_definition, EnemyAttackStageDefinition i_stage)
	{
		if (i_player == null || !i_player.GetIsCanBeAttacked()) return;
		float damage = Resolve(i_stage?.Damage, i_definition.Damage, 0f) * GetStat("DamageMultiplier").GetValueTotal();
		float knockbackX = Resolve(i_stage?.KnockbackX, i_definition.KnockbackX, 0f) * GetStat("KnockbackXMultiplier").GetValueTotal();
		float knockbackY = Resolve(i_stage?.KnockbackY, i_definition.KnockbackY, 0f) * GetStat("KnockbackYMultiplier").GetValueTotal();
		ApplyProjectileHit(i_player, damage, knockbackX, knockbackY);
	}

	public void ApplyProjectileHit(Player i_player, float i_damage, float i_knockbackX, float i_knockbackY)
	{
		if (i_player == null || m_isDead || i_player.IsDead() || !i_player.GetIsCanBeAttacked() || i_player.GetIsBeingRaped()) return;
		i_player.TakeConfiguredHit(this, i_damage, i_knockbackX, i_knockbackY);
		NotifyDirectAttackHit(i_player);
	}

	private static float Resolve(float? i_stageValue, float? i_attackValue, float i_fallback)
	{
		return i_stageValue ?? i_attackValue ?? i_fallback;
	}

	public override void Die()
	{
		if (m_navigationTraversal != null)
		{
			StopCoroutine(m_navigationTraversal);
			m_navigationTraversal = null;
			GetRigidbody2D().gravityScale = m_navigationGravity;
		}
		ClearNavigationPath();
		if (m_modAnimation != null) m_modAnimation.enabled = false;
		base.Die();
	}

	private void Resolve()
	{
		foreach (EnemyDefinition definition in ModLoaderRuntime.EnemyDefinitions)
			if (definition.Id.ToString() == m_definitionId) { m_definition = definition; return; }
	}
}

public sealed class ModularEnemyProjectile : MonoBehaviour
{
	private ModularEnemy m_owner;
	private Rigidbody2D m_body;
	private float m_damage;
	private float m_knockbackX;
	private float m_knockbackY;
	private bool m_spent;

	public void Configure(ModularEnemy i_owner, float i_damage, float i_knockbackX, float i_knockbackY, float i_lifetime)
	{
		m_owner = i_owner;
		m_body = GetComponent<Rigidbody2D>();
		m_damage = i_damage;
		m_knockbackX = i_knockbackX;
		m_knockbackY = i_knockbackY;
		UnityEngine.Object.Destroy(gameObject, i_lifetime);
	}

	private void FixedUpdate()
	{
		if (m_body != null && m_body.velocity.sqrMagnitude > 0.001f)
			transform.right = m_body.velocity.normalized;
	}

	private void OnTriggerEnter2D(Collider2D i_other)
	{
		if (m_spent || i_other == null) return;
		Player player = i_other.GetComponentInParent<Player>();
		if (player != null)
		{
			m_spent = true;
			if (m_owner != null) m_owner.ApplyProjectileHit(player, m_damage, m_knockbackX, m_knockbackY);
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		if (i_other.gameObject.layer == LayerMask.NameToLayer("Platform"))
		{
			m_spent = true;
			UnityEngine.Object.Destroy(gameObject);
		}
	}
}

public sealed class ModularEnemyFinisher : MonoBehaviour, ISmasherHudSource
{
	private static readonly HashSet<NPC> ReservedParticipants = new HashSet<NPC>();
	private static ModularEnemyFinisher s_activeSession;
	[SerializeField] private string m_enemyDefinitionId;
	private NPC m_npc;
	private EnemyBehaviorModuleDefinition m_definition;
	private Player m_player;
	private bool m_isActive;
	private float m_meter;
	private float m_startedAt;
	private float m_downedAt = -1f;
	private float m_nextAllowed;
	private KeyCode m_keyExpected = KeyCode.A;
	private bool m_waitForPlayerRecovery;
	private readonly Dictionary<string, Transform> m_playerBones = new Dictionary<string, Transform>(StringComparer.Ordinal);
	private readonly Dictionary<string, Vector3> m_playerBasePositions = new Dictionary<string, Vector3>(StringComparer.Ordinal);
	private readonly Dictionary<string, Vector3> m_playerBaseScales = new Dictionary<string, Vector3>(StringComparer.Ordinal);
	private readonly Dictionary<string, float> m_playerBaseRotations = new Dictionary<string, float>(StringComparer.Ordinal);
	private readonly Dictionary<string, SpriteRenderer> m_playerRenderers = new Dictionary<string, SpriteRenderer>(StringComparer.Ordinal);
	private readonly Dictionary<string, Color> m_playerBaseColors = new Dictionary<string, Color>(StringComparer.Ordinal);
	private readonly Dictionary<string, int> m_playerBaseSorting = new Dictionary<string, int>(StringComparer.Ordinal);
	private readonly Dictionary<string, BodyPartPlayer> m_playerBodyParts = new Dictionary<string, BodyPartPlayer>(StringComparer.Ordinal);
	private readonly Dictionary<string, Sprite> m_playerBaseSprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
	private readonly Dictionary<string, Sprite> m_runtimeSprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
	private readonly List<FinisherRendererSnapshot> m_rendererSnapshots = new List<FinisherRendererSnapshot>();
	private readonly List<FinisherSortingGroupSnapshot> m_sortingGroupSnapshots = new List<FinisherSortingGroupSnapshot>();
	private readonly List<ActiveFinisherParticipant> m_participants = new List<ActiveFinisherParticipant>();
	private readonly List<FinisherColliderPair> m_participantCollisionPairs = new List<FinisherColliderPair>();
	private Animator m_playerSkeletonAnimator;
	private bool m_playerAnimatorWasEnabled;
	private Rigidbody2D m_playerBody;
	private Rigidbody2D m_enemyBody;
	private RigidbodyType2D m_playerBodyType;
	private RigidbodyType2D m_enemyBodyType;
	private bool m_actorPhysicsLocked;
	private bool m_isIgnoringActorCollision;
	private Vector2 m_playerLockedPosition;
	private Vector2 m_enemyLockedPosition;
	private int m_nextPlayerAnimationEvent;
	private int m_playerAnimationEventCycle;
	private int m_nextPresentationTrigger;
	private int m_presentationTriggerCycle;
	private int m_phaseIndex;
	private float m_phaseStartedAt;
	private string m_lastOutcome = "not started";
	private StatusPlayerHud m_statusHud;
	private StatusPlayerHudItem m_activeStatus;
	private readonly StickCircleGesture m_circleGesture = new StickCircleGesture();
	private bool m_directionInputReady = true;

	public void Configure(string i_enemyDefinitionId)
	{
		m_enemyDefinitionId = i_enemyDefinitionId;
		Resolve();
	}

	private void Awake()
	{
		m_npc = GetComponent<NPC>();
		Resolve();
	}

	private void OnEnable()
	{
		if (m_npc == null) m_npc = GetComponent<NPC>();
		Resolve();
		m_isActive = false;
		m_downedAt = -1f;
		m_waitForPlayerRecovery = false;
	}

	private void Update()
	{
		if (m_definition == null || m_npc == null || CommonReferences.Instance == null) return;
		if (m_player == null) m_player = CommonReferences.Instance.GetPlayer();
		if (m_player == null) return;
		if (m_isActive) { UpdateFinisher(); return; }
		if (m_waitForPlayerRecovery)
		{
			if (m_player.GetStateActorCurrent() == StateActor.Ragdoll) return;
			m_waitForPlayerRecovery = false;
		}
		if (m_npc.IsDead() || m_npc.GetIsAttacking() || Time.time < m_nextAllowed || m_player.IsDead() || m_player.GetIsBeingRaped()
			|| (m_player.GetStateActorCurrent() != StateActor.Ragdoll && !m_player.IsExposing()))
		{
			m_downedAt = -1f;
			return;
		}
		if (m_downedAt < 0f) m_downedAt = Time.time;
		if (Time.time - m_downedAt < (m_definition.StartDelaySeconds ?? 0.2f)) return;
		if (Vector2.Distance(m_npc.GetPosFeet(), m_player.GetPosHips()) <= m_definition.TriggerRange.Value) BeginFinisher();
	}

	private void FixedUpdate()
	{
		if (m_isActive) MaintainActorPhysicsLock();
	}

	private void LateUpdate()
	{
		if (m_isActive) MaintainActorPhysicsLock();
	}

	public bool ShouldApproachDownedPlayer(Player i_player)
	{
		return !m_isActive && m_definition != null && i_player != null && !m_npc.IsDead()
			&& (i_player.GetStateActorCurrent() == StateActor.Ragdoll || i_player.IsExposing())
			&& Vector2.Distance(m_npc.GetPosFeet(), i_player.GetPosHips()) > m_definition.TriggerRange.Value * 0.85f;
	}

	public bool GetIsActive() { return m_isActive; }
	public static bool ShouldApproachActiveSession(NPC i_candidate)
	{
		return s_activeSession != null && s_activeSession.m_isActive
			&& s_activeSession.HasOpenLateJoinSlotFor(i_candidate, true);
	}

	public bool TryBeginFromAttack(Player i_player)
	{
		if (m_isActive || m_definition == null || m_npc == null || i_player == null || m_npc.IsDead()
			|| i_player.IsDead() || i_player.GetIsBeingRaped() || Time.time < m_nextAllowed) return false;
		m_player = i_player;
		BeginFinisher();
		return m_isActive;
	}

	private void BeginFinisher()
	{
		ManagerHud managerHud = CommonReferences.Instance.GetManagerHud();
		ManagerHudRapeGames hud = managerHud == null ? null : managerHud.GetManagerHudRapeGames();
		if (hud == null || hud.GetHudSmasher() == null)
		{
			m_nextAllowed = Time.time + 1f;
			return;
		}
		if (!TryReserveInitialParticipants())
		{
			m_nextAllowed = Time.time + 1f;
			return;
		}
		m_isActive = true;
		s_activeSession = this;
		m_lastOutcome = "running";
		m_statusHud = managerHud.GetStatusPlayerHud();
		if (m_statusHud != null)
		{
			m_statusHud.BeginModularFinisherStatusContext(m_definition.Statuses, m_npc.GetName());
			m_activeStatus = m_statusHud.CreateModularFinisherStatus(m_player.IsDead() ? "mating" : "active");
		}
		m_meter = 0f;
		m_startedAt = Time.time;
		m_phaseIndex = 0;
		m_phaseStartedAt = Time.time;
		m_keyExpected = KeyCode.A;
		m_circleGesture.Reset();
		m_directionInputReady = true;
		m_npc.StopMoving();
		m_npc.SetIsCanAttack(false);
		m_npc.SetIsThinking(false);
		m_npc.SetIsInvulnerable(true, false);
		m_player.StopMoving();
		m_player.HideEquippedWeapon();
		if (m_player.GetStateActorCurrent() == StateActor.Ragdoll) m_player.DisableRagdoll();
		AlignPlayerToFinisherFloor();
		AlignEnemyToPlayerForFinisher();
		m_player.SetIsExposing(false);
		m_player.SetIsCrouching(false);
		m_player.SetStateActor(StateActor.Idle);
		// Match the state used by Core finishers. Besides identifying the QTE to
		// other enemies, this prevents ordinary combat/recovery logic from trying
		// to move the player while the paired pose owns both actors.
		// A modular finisher is not backed by a Core Raper component, so set the
		// state directly. Raising Player.OnBeingRaped here would make legacy
		// challenge listeners dereference a nonexistent Raper.
		m_player.SetStatePlayer(StatePlayer.BeingRaped);
		m_player.SetIsInvulnerable(true, false);
		SetActorCollisionIgnored(true);
		LockActorPhysics();
		BeginPairedSorting();
		BeginReservedParticipantControl();
		BeginPlayerAnimation();
		CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(true);
		PlayCurrentPhase();
		try { hud.ShowHudSmasher(this); }
		catch (Exception exception)
		{
			Debug.LogException(exception);
			EndFinisher(false, false);
			return;
		}
		CameraXGame camera = CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent();
		if (camera != null) camera.ZoomToFOV(camera.GetFOVOriginal() / 2f, 0.25f);
	}

	private void AlignPlayerToFinisherFloor()
	{
		if (m_player == null) return;
		// Do not use the modular enemy's reported feet as the floor. Original rigs
		// can have custom body/root offsets, so that value may be well above ground.
		// Resolve the actual platform directly below the player's visual bounds.
		Vector2 origin = m_player.GetPosTopHead() + Vector2.up * 0.25f;
		float distance = Mathf.Max(5f, m_player.GetHeight() + 3f);
		RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, distance,
			LayerMask.GetMask("Platform", "Ledge"));
		foreach (RaycastHit2D hit in hits)
		{
			if (hit.collider == null || hit.collider.isTrigger) continue;
			m_player.PlaceFeetOnPos(new Vector2(m_player.GetPos().x, hit.point.y));
			return;
		}
	}

	private void AlignEnemyToPlayerForFinisher()
	{
		if (m_player == null || m_npc == null) return;
		// Match the pair preview: both actor roots share an X coordinate and both
		// reported feet sit on the same floor line before their local pose tracks run.
		// Composite rigs can then use one authored coordinate frame consistently.
		m_npc.PlaceFeetOnPos(m_player.GetPosFeet());
	}

	private void UpdateFinisher()
	{
		if (m_npc.IsDead() || m_player == null || m_player.IsDead()) { EndFinisher(false, false); return; }
		if (!UpdateParticipantAvailability()) return;
		ScreenGame screen = CommonReferences.Instance.GetManagerScreens().GetScreenGame();
		if (screen != null && screen.IsPaused()) return;
		MaintainActorPhysicsLock();
		if (!AdvanceFinisherPhase()) return;
		UpdatePlayerAnimation();
		m_meter = Mathf.Max(0f, m_meter - GetCurrentDecayPerSecond() * Time.deltaTime);
		ManagerInput input = CommonReferences.Instance.GetManagerInput();
		PlayerController controller = CommonReferences.Instance.GetPlayerController();
		bool pressed = UpdateQteInput(GetCurrentInputPattern(), input, controller);
		if (pressed)
		{
			m_meter += GetCurrentInputPower();
			HudSmasher hud = CommonReferences.Instance.GetManagerHud().GetManagerHudRapeGames().GetHudSmasher();
			if (hud != null) hud.Thrust();
		}
		if (m_meter >= m_definition.MeterMax.Value) EndFinisher(true, true);
		else if (!HasPhases() && Time.time - m_startedAt >= m_definition.DurationSeconds.Value) EndFinisher(false, true);
	}

	private bool AdvanceFinisherPhase()
	{
		if (!HasPhases()) return true;
		while (m_phaseIndex < m_definition.Phases.Count
			&& Time.time - m_phaseStartedAt >= m_definition.Phases[m_phaseIndex].DurationSeconds)
		{
			m_phaseStartedAt += m_definition.Phases[m_phaseIndex].DurationSeconds;
			m_phaseIndex++;
			if (m_phaseIndex >= m_definition.Phases.Count)
			{
				EndFinisher(false, true);
				return false;
			}
			TryJoinLateParticipants();
			PlayCurrentPhase();
		}
		return m_isActive;
	}

	private void PlayCurrentPhase()
	{
		m_nextPlayerAnimationEvent = 0;
		m_playerAnimationEventCycle = 0;
		m_nextPresentationTrigger = 0;
		m_presentationTriggerCycle = 0;
		m_keyExpected = KeyCode.A;
		m_directionInputReady = true;
		m_circleGesture.Reset();
		ResetPlayerPose();
		string animation = GetCurrentPhase()?.Animation ?? m_definition.Animation;
		m_npc.GetComponent<ModularEnemyAnimationController>()?.Play(
			string.IsNullOrEmpty(animation) ? "finisher" : animation, true);
		foreach (ActiveFinisherParticipant participant in m_participants)
			if (participant.Started && participant.Controller != null)
				participant.Controller.Play(GetParticipantAnimation(participant.Definition), true);
	}

	private void EndFinisher(bool i_success, bool i_applyOutcome)
	{
		if (!m_isActive) return;
		m_isActive = false;
		m_lastOutcome = !i_applyOutcome ? "cancelled" : i_success ? "success" : "failure";
		m_nextAllowed = Time.time + (m_definition.CooldownSeconds ?? 2f);
		m_downedAt = -1f;
		ManagerHudRapeGames hud = CommonReferences.Instance.GetManagerHud().GetManagerHudRapeGames();
		if (hud != null) hud.HideHudSmasher();
		EndPlayerAnimation();
		EndParticipantControl();
		EndPairedSorting();
		SeparateActorsForRelease();
		UnlockActorPhysics();
		CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(false);
		m_player.SetIsInvulnerable(false, true);
		m_player.SetIsBeingRaped(false);
		SetActorCollisionIgnored(false);
		if (!m_player.IsDead()) m_player.ShowEquippedWeapon();
		m_npc.SetIsInvulnerable(false, false);
		m_npc.SetIsCanAttack(true);
		m_npc.SetIsThinking(true);
		CameraXGame camera = CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent();
		if (camera != null && camera.isActiveAndEnabled) camera.ZoomToFOV(camera.GetFOVOriginal(), 0.25f);
		m_npc.GetComponent<ModularEnemyAnimationController>()?.Play("idle", true);
		if (m_activeStatus != null && m_statusHud != null)
		{
			m_statusHud.DestroyStatusItem(m_activeStatus);
			m_activeStatus = null;
		}
		if (i_applyOutcome)
		{
			if (!i_success) AddFailureStatuses();
			EnemyFinisherOutcomeDefinition outcome = i_success ? m_definition.SuccessOutcome : m_definition.FailureOutcome;
			if (outcome != null) ApplyOutcome(outcome);
			else if (i_success)
			{
				if ((m_definition.SuccessRecoveryHealth ?? 0f) > 0f) m_player.RestoreHealth(m_definition.SuccessRecoveryHealth.Value);
				if ((m_definition.SuccessStunSeconds ?? 0f) > 0f) m_npc.Ragdoll(m_definition.SuccessStunSeconds.Value);
			}
			else
			{
				m_waitForPlayerRecovery = true;
				if ((m_definition.FailureDamage ?? 0f) > 0f) m_player.TakeDamage(m_definition.FailureDamage.Value);
				if (!m_player.IsDead()) m_player.Ragdoll(1f);
			}
		}
		if (m_statusHud != null) m_statusHud.EndModularFinisherStatusContext();
		m_statusHud = null;
		ReleaseSessionReservations();
	}

	private void AddFailureStatuses()
	{
		if (m_statusHud == null) return;
		m_statusHud.CreateModularFinisherStatus("infusion", 5f);
		m_statusHud.CreateModularFinisherStatus("succumbed", 5f);
	}

	private void ApplyOutcome(EnemyFinisherOutcomeDefinition i_outcome)
	{
		if ((i_outcome.HealthRecovery ?? 0f) > 0f) m_player.RestoreHealth(i_outcome.HealthRecovery.Value);
		if ((i_outcome.HealthDamage ?? 0f) > 0f) m_player.TakeDamage(i_outcome.HealthDamage.Value);
		if ((i_outcome.StrengthDamage ?? 0f) > 0f) m_player.DamageStrength(i_outcome.StrengthDamage.Value);
		if ((i_outcome.Pleasure ?? 0f) > 0f) m_player.GainPleasureFlat(i_outcome.Pleasure.Value);
		if ((i_outcome.Libido ?? 0f) > 0f) m_player.GainLibido(i_outcome.Libido.Value);
		if ((i_outcome.EnemyStunSeconds ?? 0f) > 0f && !m_npc.IsDead()) m_npc.Ragdoll(i_outcome.EnemyStunSeconds.Value);
		if ((i_outcome.PlayerRagdollSeconds ?? 0f) > 0f && !m_player.IsDead())
		{
			m_waitForPlayerRecovery = true;
			m_player.Ragdoll(i_outcome.PlayerRagdollSeconds.Value);
		}
	}

	private void OnDisable()
	{
		if (m_isActive && CommonReferences.Instance != null && m_player != null) EndFinisher(false, false);
		if (!m_actorPhysicsLocked) return;
		EndPlayerAnimation();
		EndParticipantControl();
		EndPairedSorting();
		UnlockActorPhysics();
		if (CommonReferences.Instance != null && CommonReferences.Instance.GetPlayerController() != null)
			CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(false);
		if (m_player != null)
		{
			m_player.SetIsInvulnerable(false, true);
			m_player.SetIsBeingRaped(false);
		}
		SetActorCollisionIgnored(false);
		if (m_npc != null)
		{
			m_npc.SetIsInvulnerable(false, false);
			m_npc.SetIsCanAttack(true);
			m_npc.SetIsThinking(true);
		}
		ReleaseSessionReservations();
	}

	private bool UpdateQteInput(string i_pattern, ManagerInput i_input, PlayerController i_controller)
	{
		bool mobile = i_controller != null && i_controller.GetIsMobileControlsEnabled();
		bool gamepad = !mobile && i_input != null && i_input.IsControllerLastUsed()
			&& UnityEngine.InputSystem.Gamepad.current != null;
		switch (i_pattern)
		{
		case "tap":
			return mobile ? i_controller.GetIsMobileJumpPressed() : i_input.IsButtonDown(InputButton.Jump);
		case "alternate":
			return UpdateAlternateInput(i_input, i_controller, mobile, gamepad);
		case "rotate":
			if (mobile) return m_circleGesture.Update(i_controller.GetMobileAimInput());
			if (gamepad) return m_circleGesture.Update(UnityEngine.InputSystem.Gamepad.current.rightStick.ReadValue());
			return UpdateAlternateInput(i_input, i_controller, false, false);
		default: // adaptive preserves the v1 input behavior.
			if (mobile)
				return i_controller.GetIsMobileJumpPressed() || m_circleGesture.Update(i_controller.GetMobileAimInput());
			if (gamepad)
				return m_circleGesture.Update(UnityEngine.InputSystem.Gamepad.current.rightStick.ReadValue());
			if (i_input.IsButton(InputButton.Jump))
			{
				m_meter += GetCurrentInputPower() * 0.2f * Time.deltaTime * 10f;
				return false;
			}
			return UpdateAlternateInput(i_input, i_controller, false, false);
		}
	}

	private bool UpdateAlternateInput(ManagerInput i_input, PlayerController i_controller, bool i_mobile, bool i_gamepad)
	{
		if (!i_mobile && !i_gamepad)
		{
			if (m_keyExpected == KeyCode.A && i_input.IsButtonDown(InputButton.MoveLeft))
			{
				m_keyExpected = KeyCode.D;
				return true;
			}
			if (m_keyExpected == KeyCode.D && i_input.IsButtonDown(InputButton.MoveRight))
			{
				m_keyExpected = KeyCode.A;
				return true;
			}
			return false;
		}

		float horizontal;
		if (i_mobile) horizontal = i_controller.GetMobileMovementInput().x;
		else
		{
			UnityEngine.InputSystem.Gamepad pad = UnityEngine.InputSystem.Gamepad.current;
			float dpad = pad.dpad.ReadValue().x;
			horizontal = Mathf.Abs(dpad) > 0.25f ? dpad : pad.leftStick.ReadValue().x;
		}
		if (Mathf.Abs(horizontal) < 0.25f)
		{
			m_directionInputReady = true;
			return false;
		}
		if (!m_directionInputReady) return false;
		if (m_keyExpected == KeyCode.A && horizontal <= -0.6f)
		{
			m_keyExpected = KeyCode.D;
			m_directionInputReady = false;
			return true;
		}
		if (m_keyExpected == KeyCode.D && horizontal >= 0.6f)
		{
			m_keyExpected = KeyCode.A;
			m_directionInputReady = false;
			return true;
		}
		return false;
	}

	public float GetMeterCurrent() { return m_meter; }
	public float GetMeterMax() { return m_definition?.MeterMax ?? 100f; }
	public int GetPhaseIndex() { return HasPhases() ? Mathf.Clamp(m_phaseIndex, 0, m_definition.Phases.Count - 1) : 0; }
	public string GetPhaseId() { return GetCurrentPhase()?.Id ?? (m_definition == null ? "none" : "single"); }
	public string GetEnemyAnimationName() { return GetCurrentPhase()?.Animation ?? m_definition?.Animation ?? "none"; }
	public string GetPlayerAnimationReference() { return GetCurrentPhase()?.PlayerAnimationReference ?? m_definition?.PlayerAnimationReference ?? "inline/none"; }
	public string GetLastOutcome() { return m_lastOutcome; }
	public int GetParticipantCount() { return m_participants.Count(i_participant => i_participant.Started && i_participant.Npc != null); }
	public int GetOpenParticipantSlotCount()
	{
		return (m_definition?.Participants ?? new List<EnemyFinisherParticipantDefinition>())
			.Count(i_definition => !HasParticipant(i_definition.Id));
	}
	public void CancelForTest() { if (m_isActive) EndFinisher(false, false); }
	public float GetTimeLeft() { return Mathf.Max(0f, GetTimeMax() - (Time.time - m_startedAt)); }
	public float GetTimeMax()
	{
		if (!HasPhases()) return m_definition?.DurationSeconds ?? 1f;
		float total = 0f;
		foreach (EnemyFinisherPhaseDefinition phase in m_definition.Phases) total += phase.DurationSeconds;
		return total;
	}
	public KeyCode GetKeyCodeToPress() { return m_keyExpected; }
	public bool UsesCircularInput()
	{
		string pattern = GetCurrentInputPattern();
		if (pattern == "tap") return true;
		if (pattern == "alternate") return false;
		return CommonReferences.Instance.GetPlayerController().GetIsMobileControlsEnabled()
			|| CommonReferences.Instance.GetManagerInput().IsControllerLastUsed();
	}
	public string GetInputPrompt()
	{
		string pattern = GetCurrentInputPattern();
		if (pattern == "tap") return "Tap Jump";
		if (pattern == "rotate") return CommonReferences.Instance.GetPlayerController().GetIsMobileControlsEnabled()
			? "Rotate aim stick" : "Rotate right stick";
		return CommonReferences.Instance.GetPlayerController().GetIsMobileControlsEnabled()
			? "Rotate aim stick or tap Jump" : "Rotate right stick";
	}
	public Sprite GetInputGlyph()
	{
		return GetCurrentInputPattern() == "tap"
			? InputGlyphLibrary.GetPromptSprite(InputButton.Jump)
			: InputGlyphLibrary.GetStruggleSprite();
	}
	public bool AllowsHoldInput() { return GetCurrentInputPattern() == "adaptive"; }

	private bool TryReserveInitialParticipants()
	{
		CleanupDestroyedReservations();
		m_participants.Clear();
		if ((s_activeSession != null && s_activeSession != this) || m_npc == null || ReservedParticipants.Contains(m_npc)) return false;
		ReservedParticipants.Add(m_npc);
		foreach (EnemyFinisherParticipantDefinition definition in m_definition?.Participants ?? new List<EnemyFinisherParticipantDefinition>())
		{
			NPC candidate = FindParticipantCandidate(definition, GetJoinRange(definition));
			if (candidate == null)
			{
				if (!definition.Required) continue;
				ReleaseSessionReservations();
				return false;
			}
			ReservedParticipants.Add(candidate);
			m_participants.Add(new ActiveFinisherParticipant(definition, candidate));
		}
		return true;
	}

	private void TryJoinLateParticipants()
	{
		foreach (EnemyFinisherParticipantDefinition definition in m_definition?.Participants ?? new List<EnemyFinisherParticipantDefinition>())
		{
			if (GetJoinPolicy(definition) != "phaseBoundary" || HasParticipant(definition.Id)) continue;
			NPC candidate = FindParticipantCandidate(definition, GetJoinRange(definition));
			if (candidate == null) continue;
			ReservedParticipants.Add(candidate);
			ActiveFinisherParticipant participant = new ActiveFinisherParticipant(definition, candidate);
			m_participants.Add(participant);
			BeginParticipantControl(participant);
		}
	}

	private bool UpdateParticipantAvailability()
	{
		for (int index = m_participants.Count - 1; index >= 0; index--)
		{
			ActiveFinisherParticipant participant = m_participants[index];
			if (participant.Npc != null && !participant.Npc.IsDead()) continue;
			if (participant.Npc != null) ReservedParticipants.Remove(participant.Npc);
			m_participants.RemoveAt(index);
			if (!participant.Definition.Required) continue;
			EndFinisher(false, false);
			return false;
		}
		return true;
	}

	private bool HasOpenLateJoinSlotFor(NPC i_candidate, bool i_useApproachRange)
	{
		if (i_candidate == null || ReservedParticipants.Contains(i_candidate)) return false;
		foreach (EnemyFinisherParticipantDefinition definition in m_definition?.Participants ?? new List<EnemyFinisherParticipantDefinition>())
		{
			if (GetJoinPolicy(definition) != "phaseBoundary" || HasParticipant(definition.Id) || !MatchesParticipant(i_candidate, definition)) continue;
			float range = i_useApproachRange ? GetApproachRange(definition) : GetJoinRange(definition);
			if (Vector2.Distance(i_candidate.GetPosFeet(), m_player.GetPosFeet()) <= range) return true;
		}
		return false;
	}

	private NPC FindParticipantCandidate(EnemyFinisherParticipantDefinition i_definition, float i_range)
	{
		if (CommonReferences.Instance == null || m_player == null) return null;
		Stage stage = CommonReferences.Instance.GetManagerStages()?.GetStageCurrent();
		if (stage == null) return null;
		NPC best = null;
		float bestDistance = float.PositiveInfinity;
		foreach (NPC candidate in stage.GetAllNPCs())
		{
			if (!MatchesParticipant(candidate, i_definition)) continue;
			float distance = Vector2.Distance(candidate.GetPosFeet(), m_player.GetPosFeet());
			if (distance > i_range || distance >= bestDistance) continue;
			best = candidate;
			bestDistance = distance;
		}
		return best;
	}

	private bool MatchesParticipant(NPC i_candidate, EnemyFinisherParticipantDefinition i_definition)
	{
		if (i_candidate == null || i_candidate == m_npc || !i_candidate.gameObject.activeInHierarchy
			|| i_candidate.IsDead() || i_candidate.GetIsAttacking() || ReservedParticipants.Contains(i_candidate)
			|| i_candidate.GetComponent<ModularEnemyAnimationController>() == null) return false;
		return RuntimeContentIdentity.TryResolve(i_candidate, out ContentId id, out ContentCategory category)
			&& category == ContentCategory.Enemy && id.ToString() == i_definition.Enemy;
	}

	private bool HasParticipant(string i_id)
	{
		return m_participants.Exists(i_participant => i_participant.Definition != null && i_participant.Definition.Id == i_id);
	}

	private static string GetJoinPolicy(EnemyFinisherParticipantDefinition i_definition)
	{
		return string.IsNullOrEmpty(i_definition?.JoinPolicy) ? "phaseBoundary" : i_definition.JoinPolicy;
	}

	private float GetJoinRange(EnemyFinisherParticipantDefinition i_definition)
	{
		return i_definition?.JoinRange ?? m_definition?.TriggerRange ?? 1.5f;
	}

	private float GetApproachRange(EnemyFinisherParticipantDefinition i_definition)
	{
		return i_definition?.ApproachRange ?? Mathf.Min(50f, Mathf.Max(8f, GetJoinRange(i_definition) * 3f));
	}

	private void BeginReservedParticipantControl()
	{
		foreach (ActiveFinisherParticipant participant in m_participants) BeginParticipantControl(participant);
	}

	private void BeginParticipantControl(ActiveFinisherParticipant i_participant)
	{
		if (i_participant == null || i_participant.Started || i_participant.Npc == null) return;
		NPC npc = i_participant.Npc;
		npc.StopMoving();
		npc.SetIsCanAttack(false);
		npc.SetIsThinking(false);
		npc.SetIsInvulnerable(true, false);
		Vector2 anchor = m_player.GetPosFeet();
		npc.PlaceFeetOnPos(anchor + new Vector2(i_participant.Definition.OffsetX ?? 0f, i_participant.Definition.OffsetY ?? 0f));
		if (i_participant.Definition.Facing == "left") npc.SetIsFacingLeft(true);
		else if (i_participant.Definition.Facing == "right") npc.SetIsFacingLeft(false);
		if (i_participant.Body != null)
		{
			i_participant.Body.velocity = Vector2.zero;
			i_participant.Body.angularVelocity = 0f;
			i_participant.Body.bodyType = RigidbodyType2D.Kinematic;
			i_participant.LockedPosition = i_participant.Body.position;
		}
		IgnoreParticipantCollisions(npc);
		PromotePairedSorting(npc.gameObject);
		i_participant.Started = true;
	}

	private void IgnoreParticipantCollisions(NPC i_npc)
	{
		IgnoreParticipantCollisionPair(i_npc, m_player);
		IgnoreParticipantCollisionPair(i_npc, m_npc);
		foreach (ActiveFinisherParticipant other in m_participants)
			if (other.Started && other.Npc != null && other.Npc != i_npc) IgnoreParticipantCollisionPair(i_npc, other.Npc);
	}

	private void IgnoreParticipantCollisionPair(Actor i_left, Actor i_right)
	{
		if (i_left == null || i_right == null) return;
		foreach (Collider2D left in i_left.GetAllColliders())
			foreach (Collider2D right in i_right.GetAllColliders())
			{
				if (left == null || right == null) continue;
				Physics2D.IgnoreCollision(left, right, true);
				m_participantCollisionPairs.Add(new FinisherColliderPair(left, right));
			}
	}

	private string GetParticipantAnimation(EnemyFinisherParticipantDefinition i_definition)
	{
		EnemyFinisherPhaseDefinition phase = GetCurrentPhase();
		if (phase?.ParticipantAnimations != null && phase.ParticipantAnimations.TryGetValue(i_definition.Id, out string animation)) return animation;
		return string.IsNullOrEmpty(i_definition.Animation) ? "idle" : i_definition.Animation;
	}

	private void EndParticipantControl()
	{
		foreach (FinisherColliderPair pair in m_participantCollisionPairs) pair.Restore();
		m_participantCollisionPairs.Clear();
		foreach (ActiveFinisherParticipant participant in m_participants) participant.Restore();
	}

	private void ReleaseSessionReservations()
	{
		if (m_npc != null) ReservedParticipants.Remove(m_npc);
		foreach (ActiveFinisherParticipant participant in m_participants)
			if (participant.Npc != null) ReservedParticipants.Remove(participant.Npc);
		m_participants.Clear();
		if (s_activeSession == this) s_activeSession = null;
	}

	private static void CleanupDestroyedReservations()
	{
		ReservedParticipants.RemoveWhere(i_npc => i_npc == null);
	}

	private void LockActorPhysics()
	{
		m_playerBody = m_player == null ? null : m_player.GetRigidbody2D();
		m_enemyBody = m_npc == null ? null : m_npc.GetRigidbody2D();
		if (m_playerBody != null)
		{
			m_playerBodyType = m_playerBody.bodyType;
			m_playerBody.velocity = Vector2.zero;
			m_playerBody.angularVelocity = 0f;
			m_playerBody.bodyType = RigidbodyType2D.Kinematic;
			m_playerLockedPosition = m_playerBody.position;
		}
		if (m_enemyBody != null)
		{
			m_enemyBodyType = m_enemyBody.bodyType;
			m_enemyBody.velocity = Vector2.zero;
			m_enemyBody.angularVelocity = 0f;
			m_enemyBody.bodyType = RigidbodyType2D.Kinematic;
			m_enemyLockedPosition = m_enemyBody.position;
		}
		m_actorPhysicsLocked = true;
	}

	private void MaintainActorPhysicsLock()
	{
		if (!m_actorPhysicsLocked) return;
		if (m_playerBody != null)
		{
			m_playerBody.position = m_playerLockedPosition;
			m_playerBody.transform.position = new Vector3(m_playerLockedPosition.x, m_playerLockedPosition.y, m_playerBody.transform.position.z);
			m_playerBody.velocity = Vector2.zero;
			m_playerBody.angularVelocity = 0f;
		}
		if (m_enemyBody != null)
		{
			m_enemyBody.position = m_enemyLockedPosition;
			m_enemyBody.transform.position = new Vector3(m_enemyLockedPosition.x, m_enemyLockedPosition.y, m_enemyBody.transform.position.z);
			m_enemyBody.velocity = Vector2.zero;
			m_enemyBody.angularVelocity = 0f;
		}
		foreach (ActiveFinisherParticipant participant in m_participants)
		{
			if (!participant.Started || participant.Body == null) continue;
			participant.Body.position = participant.LockedPosition;
			participant.Body.transform.position = new Vector3(participant.LockedPosition.x, participant.LockedPosition.y,
				participant.Body.transform.position.z);
			participant.Body.velocity = Vector2.zero;
			participant.Body.angularVelocity = 0f;
		}
	}

	private void UnlockActorPhysics()
	{
		if (!m_actorPhysicsLocked) return;
		if (m_playerBody != null)
		{
			m_playerBody.bodyType = m_playerBodyType;
			m_playerBody.velocity = Vector2.zero;
			m_playerBody.angularVelocity = 0f;
		}
		if (m_enemyBody != null)
		{
			m_enemyBody.bodyType = m_enemyBodyType;
			m_enemyBody.velocity = Vector2.zero;
			m_enemyBody.angularVelocity = 0f;
		}
		m_actorPhysicsLocked = false;
	}

	private void SetActorCollisionIgnored(bool i_ignore)
	{
		if (m_isIgnoringActorCollision == i_ignore || m_player == null || m_npc == null) return;
		foreach (Collider2D enemyCollider in m_npc.GetAllColliders())
		{
			if (enemyCollider == null) continue;
			foreach (Collider2D playerCollider in m_player.GetAllColliders())
			{
				if (playerCollider != null) Physics2D.IgnoreCollision(enemyCollider, playerCollider, i_ignore);
			}
		}
		m_isIgnoringActorCollision = i_ignore;
	}

	private void BeginPairedSorting()
	{
		EndPairedSorting();
		PromotePairedSorting(m_player == null ? null : m_player.gameObject);
		PromotePairedSorting(m_npc == null ? null : m_npc.gameObject);
	}

	private void PromotePairedSorting(GameObject i_root)
	{
		int rendererStart = m_rendererSnapshots.Count;
		int groupStart = m_sortingGroupSnapshots.Count;
		CapturePairedSorting(i_root);
		for (int index = rendererStart; index < m_rendererSnapshots.Count; index++) m_rendererSnapshots[index].UseLayer("Player");
		for (int index = groupStart; index < m_sortingGroupSnapshots.Count; index++) m_sortingGroupSnapshots[index].UseLayer("Player");
	}

	private void CapturePairedSorting(GameObject i_root)
	{
		if (i_root == null) return;
		foreach (SpriteRenderer renderer in i_root.GetComponentsInChildren<SpriteRenderer>(true))
			if (renderer != null) m_rendererSnapshots.Add(new FinisherRendererSnapshot(renderer));
		foreach (SortingGroup group in i_root.GetComponentsInChildren<SortingGroup>(true))
			if (group != null) m_sortingGroupSnapshots.Add(new FinisherSortingGroupSnapshot(group));
	}

	private void EndPairedSorting()
	{
		foreach (FinisherRendererSnapshot snapshot in m_rendererSnapshots) snapshot.Restore();
		foreach (FinisherSortingGroupSnapshot snapshot in m_sortingGroupSnapshots) snapshot.Restore();
		m_rendererSnapshots.Clear();
		m_sortingGroupSnapshots.Clear();
	}

	private void SeparateActorsForRelease()
	{
		if (!m_actorPhysicsLocked || m_playerBody == null || m_enemyBody == null) return;
		Collider2D playerCollider = m_player.GetComponent<Collider2D>();
		Collider2D enemyCollider = m_npc.GetComponent<Collider2D>();
		float separation = 0.8f;
		if (playerCollider != null && enemyCollider != null)
			separation = Mathf.Max(separation, playerCollider.bounds.extents.x + enemyCollider.bounds.extents.x + 0.1f);
		float direction = Mathf.Sign(m_enemyLockedPosition.x - m_playerLockedPosition.x);
		if (Mathf.Approximately(direction, 0f)) direction = m_npc.GetIsFacingLeft() ? 1f : -1f;
		if (Mathf.Abs(m_enemyLockedPosition.x - m_playerLockedPosition.x) < separation)
		{
			m_enemyLockedPosition.x = m_playerLockedPosition.x + direction * separation;
			MaintainActorPhysicsLock();
		}
	}

	private void BeginPlayerAnimation()
	{
		m_nextPlayerAnimationEvent = 0;
		m_playerAnimationEventCycle = 0;
		m_playerBones.Clear();
		m_playerBasePositions.Clear();
		m_playerBaseScales.Clear();
		m_playerBaseRotations.Clear();
		m_playerRenderers.Clear();
		m_playerBaseColors.Clear();
		m_playerBaseSorting.Clear();
		m_playerBodyParts.Clear();
		m_playerBaseSprites.Clear();
		if (!HasAnyPlayerAnimation() || m_player == null) return;
		SkeletonPlayer skeleton = m_player.GetSkeletonPlayer();
		foreach (string name in GetPlayerBoneNames())
		{
			if (!TryGetPlayerBoneType(name, out BoneTypePlayer type)) continue;
			BonePlayer bone = skeleton.GetBone(type);
			if (bone == null) continue;
			Transform target = bone.transform;
			m_playerBones[name] = target;
			m_playerBasePositions[name] = target.localPosition;
			m_playerBaseScales[name] = target.localScale;
			m_playerBaseRotations[name] = target.localEulerAngles.z;
			SpriteRenderer renderer = target.GetComponentInChildren<SpriteRenderer>(true);
			if (renderer != null)
			{
				m_playerRenderers[name] = renderer;
				m_playerBaseColors[name] = renderer.color;
				BodyPartPlayer bodyPart = renderer.GetComponent<BodyPartPlayer>();
				if (bodyPart != null) m_playerBodyParts[name] = bodyPart;
				m_playerBaseSorting[name] = bodyPart == null ? renderer.sortingOrder : bodyPart.GetSortingOrder();
				m_playerBaseSprites[name] = renderer.sprite;
			}
		}
		m_playerSkeletonAnimator = skeleton.GetComponent<Animator>();
		if (m_playerSkeletonAnimator != null)
		{
			m_playerAnimatorWasEnabled = m_playerSkeletonAnimator.enabled;
			m_playerSkeletonAnimator.enabled = false;
		}
	}

	private void UpdatePlayerAnimation()
	{
		EnemyAnimationClipDefinition clip = GetCurrentPlayerAnimation();
		if (clip == null) return;
		float totalElapsed = Time.time - (HasPhases() ? m_phaseStartedAt : m_startedAt);
		ApplyPlayerAnimationEvents(clip, totalElapsed);
		ApplyPresentationTriggers(clip, totalElapsed);
		if (clip.Frames == null || clip.Frames.Count == 0 || m_playerBones.Count == 0) return;
		float elapsed = totalElapsed;
		if (clip.Loop) elapsed %= clip.DurationSeconds; else elapsed = Mathf.Min(elapsed, clip.DurationSeconds);
		EnemyAnimationFrameDefinition from = clip.Frames[0], to = clip.Frames[clip.Frames.Count - 1];
		for (int index = 0; index < clip.Frames.Count; index++)
		{
			if (clip.Frames[index].Time <= elapsed) from = clip.Frames[index];
			if (clip.Frames[index].Time >= elapsed) { to = clip.Frames[index]; break; }
		}
		float blend = Mathf.Approximately(from.Time, to.Time) ? 0f : Mathf.InverseLerp(from.Time, to.Time, elapsed);
		HashSet<string> animated = new HashSet<string>(from.Bones.Keys, StringComparer.Ordinal);
		foreach (string bone in to.Bones.Keys) animated.Add(bone);
		foreach (string name in animated)
		{
			if (!m_playerBones.TryGetValue(name, out Transform target)) continue;
			from.Bones.TryGetValue(name, out EnemyBonePoseDefinition a);
			to.Bones.TryGetValue(name, out EnemyBonePoseDefinition b);
			if (a == null) a = b; if (b == null) b = a; if (a == null) continue;
			Vector3 basePosition = m_playerBasePositions[name], baseScale = m_playerBaseScales[name];
			Vector3 positionA = new Vector3(a.X ?? basePosition.x, a.Y ?? basePosition.y, basePosition.z);
			Vector3 positionB = new Vector3(b.X ?? basePosition.x, b.Y ?? basePosition.y, basePosition.z);
			Vector3 scaleA = new Vector3(a.ScaleX ?? baseScale.x, a.ScaleY ?? baseScale.y, baseScale.z);
			Vector3 scaleB = new Vector3(b.ScaleX ?? baseScale.x, b.ScaleY ?? baseScale.y, baseScale.z);
			target.localPosition = Vector3.Lerp(positionA, positionB, blend);
			target.localScale = Vector3.Lerp(scaleA, scaleB, blend);
			target.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(a.Rotation ?? m_playerBaseRotations[name], b.Rotation ?? m_playerBaseRotations[name], blend));
			if (m_playerRenderers.TryGetValue(name, out SpriteRenderer renderer))
			{
				Color baseColor = m_playerBaseColors[name];
				Color colorA = new Color(a.ColorR ?? baseColor.r, a.ColorG ?? baseColor.g, a.ColorB ?? baseColor.b, a.ColorA ?? baseColor.a);
				Color colorB = new Color(b.ColorR ?? baseColor.r, b.ColorG ?? baseColor.g, b.ColorB ?? baseColor.b, b.ColorA ?? baseColor.a);
				renderer.color = Color.Lerp(colorA, colorB, blend);
				int sortingOrder = Mathf.RoundToInt(Mathf.Lerp(a.RuntimeSortingOrder ?? m_playerBaseSorting[name], b.RuntimeSortingOrder ?? m_playerBaseSorting[name], blend));
				if (m_playerBodyParts.TryGetValue(name, out BodyPartPlayer bodyPart)) bodyPart.SetSortingOrder(sortingOrder);
				else renderer.sortingOrder = sortingOrder;
				if (a.RuntimeSprite != null) renderer.sprite = a.RuntimeSprite;
				else
				{
					string spriteName = string.IsNullOrEmpty(a.RuntimeSpriteName) ? b.RuntimeSpriteName : a.RuntimeSpriteName;
					if (!string.IsNullOrEmpty(spriteName) && TryResolveRuntimeSprite(spriteName, out Sprite sprite)) renderer.sprite = sprite;
				}
			}
		}
	}

	private void ApplyPresentationTriggers(EnemyAnimationClipDefinition i_clip, float i_totalElapsed)
	{
		NormalizedAnimationPresentation presentation = i_clip.NormalizedPresentation;
		if (presentation == null || presentation.EffectTriggers == null || presentation.EffectTriggers.Count == 0 || i_clip.DurationSeconds <= 0f) return;
		int guard = 0;
		while (guard++ < 256)
		{
			if (!i_clip.Loop && m_presentationTriggerCycle > 0) return;
			NormalizedEffectTrigger trigger = presentation.EffectTriggers[m_nextPresentationTrigger];
			float occurrence = m_presentationTriggerCycle * i_clip.DurationSeconds + trigger.Time;
			if (occurrence > i_totalElapsed + 0.0001f) return;
			foreach (string effectId in trigger.Effects)
			{
				NormalizedEffectDefinition effect = null;
				foreach (NormalizedEffectDefinition candidate in presentation.Effects)
					if (candidate != null && candidate.Id == effectId) { effect = candidate; break; }
				if (effect != null) PlayNormalizedEffect(effect);
			}
			m_nextPresentationTrigger++;
			if (m_nextPresentationTrigger < presentation.EffectTriggers.Count) continue;
			m_nextPresentationTrigger = 0;
			m_presentationTriggerCycle++;
		}
	}

	private void PlayNormalizedEffect(NormalizedEffectDefinition i_effect)
	{
		Transform parent = null;
		if (i_effect.Target.StartsWith("bone/", StringComparison.Ordinal))
			m_playerBones.TryGetValue(i_effect.Target.Substring(5), out parent);
		else if (i_effect.Target.StartsWith("enemy-bone/", StringComparison.Ordinal))
			parent = FindTransformRecursive(m_npc.transform, i_effect.Target.Substring("enemy-bone/".Length));
		NormalizedParticleEffectRuntime.Play(i_effect, parent);
	}

	private bool TryResolveRuntimeSprite(string i_name, out Sprite o_sprite)
	{
		if (m_runtimeSprites.TryGetValue(i_name, out o_sprite)) return o_sprite != null;
		foreach (Sprite candidate in Resources.FindObjectsOfTypeAll<Sprite>())
			if (candidate != null && candidate.name == i_name) { m_runtimeSprites[i_name] = candidate; o_sprite = candidate; return true; }
		m_runtimeSprites[i_name] = null;
		o_sprite = null;
		return false;
	}

	private static Transform FindTransformRecursive(Transform i_root, string i_name)
	{
		if (i_root == null) return null;
		if (i_root.name == i_name) return i_root;
		for (int index = 0; index < i_root.childCount; index++)
		{
			Transform result = FindTransformRecursive(i_root.GetChild(index), i_name);
			if (result != null) return result;
		}
		return null;
	}

	private void ApplyPlayerAnimationEvents(EnemyAnimationClipDefinition i_clip, float i_totalElapsed)
	{
		List<EnemyAnimationEventDefinition> events = i_clip.Events;
		if (events == null || events.Count == 0 || i_clip.DurationSeconds <= 0f) return;
		int guard = 0;
		while (guard++ < 256)
		{
			if (!i_clip.Loop && m_playerAnimationEventCycle > 0) return;
			EnemyAnimationEventDefinition animationEvent = events[m_nextPlayerAnimationEvent];
			float occurrence = m_playerAnimationEventCycle * i_clip.DurationSeconds + animationEvent.Time;
			if (occurrence > i_totalElapsed + 0.0001f) return;
			ApplyPlayerAnimationEvent(animationEvent);
			m_nextPlayerAnimationEvent++;
			if (m_nextPlayerAnimationEvent < events.Count) continue;
			m_nextPlayerAnimationEvent = 0;
			m_playerAnimationEventCycle++;
		}
	}

	private void ApplyPlayerAnimationEvent(EnemyAnimationEventDefinition i_event)
	{
		if (i_event == null || !i_event.Amount.HasValue || m_player == null) return;
		float amount = i_event.Amount.Value;
		switch (i_event.Type)
		{
		case "pleasure":
			m_player.GainPleasureFlat(amount);
			break;
		case "scaledPleasure":
			m_player.GainPleasure(amount);
			break;
		case "libido":
			m_player.GainLibido(amount);
			break;
		case "strengthDamage":
			m_player.DamageStrength(amount);
			break;
		case "struggleDamage":
			m_meter = Mathf.Max(0f, m_meter - amount);
			HudSmasher hud = CommonReferences.Instance.GetManagerHud().GetManagerHudRapeGames().GetHudSmasher();
			if (hud != null) hud.Thrust();
			break;
		case "healthDamage":
			m_player.TakeDamage(amount);
			break;
		}
	}

	private void EndPlayerAnimation()
	{
		foreach (KeyValuePair<string, Transform> pair in m_playerBones)
		{
			if (pair.Value == null) continue;
			pair.Value.localPosition = m_playerBasePositions[pair.Key];
			pair.Value.localScale = m_playerBaseScales[pair.Key];
			pair.Value.localRotation = Quaternion.Euler(0f, 0f, m_playerBaseRotations[pair.Key]);
			if (m_playerRenderers.TryGetValue(pair.Key, out SpriteRenderer renderer))
			{
				renderer.color = m_playerBaseColors[pair.Key];
				if (m_playerBodyParts.TryGetValue(pair.Key, out BodyPartPlayer bodyPart)) bodyPart.SetSortingOrder(m_playerBaseSorting[pair.Key]);
				else renderer.sortingOrder = m_playerBaseSorting[pair.Key];
				renderer.sprite = m_playerBaseSprites[pair.Key];
			}
		}
		if (m_playerSkeletonAnimator != null) m_playerSkeletonAnimator.enabled = m_playerAnimatorWasEnabled;
		m_playerBones.Clear();
		m_playerRenderers.Clear();
		m_playerBodyParts.Clear();
	}

	private void ResetPlayerPose()
	{
		foreach (KeyValuePair<string, Transform> pair in m_playerBones)
		{
			if (pair.Value == null) continue;
			pair.Value.localPosition = m_playerBasePositions[pair.Key];
			pair.Value.localScale = m_playerBaseScales[pair.Key];
			pair.Value.localRotation = Quaternion.Euler(0f, 0f, m_playerBaseRotations[pair.Key]);
			if (m_playerRenderers.TryGetValue(pair.Key, out SpriteRenderer renderer))
			{
				renderer.color = m_playerBaseColors[pair.Key];
				if (m_playerBodyParts.TryGetValue(pair.Key, out BodyPartPlayer bodyPart)) bodyPart.SetSortingOrder(m_playerBaseSorting[pair.Key]);
				else renderer.sortingOrder = m_playerBaseSorting[pair.Key];
				renderer.sprite = m_playerBaseSprites[pair.Key];
			}
		}
	}

	private sealed class ActiveFinisherParticipant
	{
		public readonly EnemyFinisherParticipantDefinition Definition;
		public readonly NPC Npc;
		public readonly ModularEnemyAnimationController Controller;
		public readonly Rigidbody2D Body;
		public Vector2 LockedPosition;
		public bool Started;
		private readonly bool m_wasThinking;
		private readonly bool m_couldAttack;
		private readonly bool m_wasInvulnerable;
		private readonly bool m_wasFacingLeft;
		private readonly RigidbodyType2D m_bodyType;
		private readonly Vector2 m_position;
		private readonly Vector2 m_velocity;
		private readonly float m_angularVelocity;
		private readonly string m_animation;

		public ActiveFinisherParticipant(EnemyFinisherParticipantDefinition i_definition, NPC i_npc)
		{
			Definition = i_definition;
			Npc = i_npc;
			Controller = i_npc == null ? null : i_npc.GetComponent<ModularEnemyAnimationController>();
			Body = i_npc == null ? null : i_npc.GetRigidbody2D();
			m_wasThinking = i_npc != null && i_npc.GetIsThinking();
			m_couldAttack = i_npc != null && i_npc.GetIsCanAttack();
			m_wasInvulnerable = i_npc != null && i_npc.GetIsInvulnerable();
			m_wasFacingLeft = i_npc != null && i_npc.GetIsFacingLeft();
			m_position = Body == null ? Vector2.zero : Body.position;
			m_velocity = Body == null ? Vector2.zero : Body.velocity;
			m_angularVelocity = Body == null ? 0f : Body.angularVelocity;
			m_bodyType = Body == null ? RigidbodyType2D.Dynamic : Body.bodyType;
			m_animation = Controller == null ? null : Controller.GetCurrentClipName();
		}

		public void Restore()
		{
			if (!Started || Npc == null) return;
			if (Body != null)
			{
				Body.position = m_position;
				Body.transform.position = new Vector3(m_position.x, m_position.y, Body.transform.position.z);
				Body.bodyType = m_bodyType;
				Body.velocity = m_velocity;
				Body.angularVelocity = m_angularVelocity;
			}
			Npc.SetIsFacingLeft(m_wasFacingLeft);
			Npc.SetIsInvulnerable(m_wasInvulnerable, false);
			if (!Npc.IsDead())
			{
				Npc.SetIsCanAttack(m_couldAttack);
				Npc.SetIsThinking(m_wasThinking);
			}
			if (Controller != null) Controller.Play(string.IsNullOrEmpty(m_animation) ? "idle" : m_animation, true);
			Started = false;
		}
	}

	private sealed class FinisherColliderPair
	{
		private readonly Collider2D m_left;
		private readonly Collider2D m_right;
		public FinisherColliderPair(Collider2D i_left, Collider2D i_right) { m_left = i_left; m_right = i_right; }
		public void Restore() { if (m_left != null && m_right != null) Physics2D.IgnoreCollision(m_left, m_right, false); }
	}

	private sealed class FinisherRendererSnapshot
	{
		private readonly SpriteRenderer m_renderer;
		private readonly string m_layer;
		private readonly int m_order;

		public FinisherRendererSnapshot(SpriteRenderer i_renderer)
		{
			m_renderer = i_renderer;
			m_layer = i_renderer.sortingLayerName;
			m_order = i_renderer.sortingOrder;
		}

		public void UseLayer(string i_layer) { if (m_renderer != null) m_renderer.sortingLayerName = i_layer; }
		public void Restore()
		{
			if (m_renderer == null) return;
			m_renderer.sortingLayerName = m_layer;
			m_renderer.sortingOrder = m_order;
		}
	}

	private sealed class FinisherSortingGroupSnapshot
	{
		private readonly SortingGroup m_group;
		private readonly string m_layer;
		private readonly int m_order;

		public FinisherSortingGroupSnapshot(SortingGroup i_group)
		{
			m_group = i_group;
			m_layer = i_group.sortingLayerName;
			m_order = i_group.sortingOrder;
		}

		public void UseLayer(string i_layer) { if (m_group != null) m_group.sortingLayerName = i_layer; }
		public void Restore()
		{
			if (m_group == null) return;
			m_group.sortingLayerName = m_layer;
			m_group.sortingOrder = m_order;
		}
	}

	private bool HasPhases()
	{
		return m_definition?.Phases != null && m_definition.Phases.Count > 0;
	}

	private EnemyFinisherPhaseDefinition GetCurrentPhase()
	{
		return HasPhases() && m_phaseIndex >= 0 && m_phaseIndex < m_definition.Phases.Count
			? m_definition.Phases[m_phaseIndex] : null;
	}

	private EnemyAnimationClipDefinition GetCurrentPlayerAnimation()
	{
		return GetCurrentPhase()?.PlayerAnimation ?? m_definition?.PlayerAnimation;
	}

	private bool HasAnyPlayerAnimation()
	{
		if (m_definition?.PlayerAnimation != null) return true;
		foreach (EnemyFinisherPhaseDefinition phase in m_definition?.Phases ?? new List<EnemyFinisherPhaseDefinition>())
			if (phase?.PlayerAnimation != null) return true;
		return false;
	}

	private float GetCurrentInputPower()
	{
		return GetCurrentPhase()?.InputPower ?? m_definition?.InputPower ?? 1f;
	}

	private string GetCurrentInputPattern()
	{
		string pattern = GetCurrentPhase()?.InputPattern ?? m_definition?.InputPattern;
		return string.IsNullOrEmpty(pattern) ? "adaptive" : pattern;
	}

	private float GetCurrentDecayPerSecond()
	{
		return GetCurrentPhase()?.DecayPerSecond ?? m_definition?.DecayPerSecond ?? 0f;
	}

	private static string[] GetPlayerBoneNames()
	{
		return new[] { "hips", "butt", "spine", "chest", "neck", "head", "arm-right-upper", "arm-right-lower", "hand-right", "arm-left-upper", "arm-left-lower", "hand-left", "leg-right-upper", "leg-right-lower", "foot-right", "leg-left-upper", "leg-left-lower", "foot-left", "ear", "face" };
	}

	private static bool TryGetPlayerBoneType(string i_name, out BoneTypePlayer o_type)
	{
		switch (i_name)
		{
		case "hips": o_type = BoneTypePlayer.Hips; return true;
		case "butt": o_type = BoneTypePlayer.Butt; return true;
		case "spine": o_type = BoneTypePlayer.Spine; return true;
		case "chest": o_type = BoneTypePlayer.Chest; return true;
		case "neck": o_type = BoneTypePlayer.Neck; return true;
		case "head": o_type = BoneTypePlayer.Head; return true;
		case "arm-right-upper": o_type = BoneTypePlayer.rArmUpper; return true;
		case "arm-right-lower": o_type = BoneTypePlayer.rArmLower; return true;
		case "hand-right": o_type = BoneTypePlayer.rHand; return true;
		case "arm-left-upper": o_type = BoneTypePlayer.lArmUpper; return true;
		case "arm-left-lower": o_type = BoneTypePlayer.lArmLower; return true;
		case "hand-left": o_type = BoneTypePlayer.lHand; return true;
		case "leg-right-upper": o_type = BoneTypePlayer.rLegUpper; return true;
		case "leg-right-lower": o_type = BoneTypePlayer.rLegLower; return true;
		case "foot-right": o_type = BoneTypePlayer.rFoot; return true;
		case "leg-left-upper": o_type = BoneTypePlayer.lLegUpper; return true;
		case "leg-left-lower": o_type = BoneTypePlayer.lLegLower; return true;
		case "foot-left": o_type = BoneTypePlayer.lFoot; return true;
		case "ear": o_type = BoneTypePlayer.Ear; return true;
		case "face": o_type = BoneTypePlayer.Face; return true;
		default: o_type = BoneTypePlayer.Hips; return false;
		}
	}

	private void Resolve()
	{
		m_definition = null;
		if (string.IsNullOrEmpty(m_enemyDefinitionId)) return;
		foreach (EnemyDefinition enemy in ModLoaderRuntime.EnemyDefinitions)
			if (enemy.Id.ToString() == m_enemyDefinitionId)
			{
				m_definition = enemy.Behavior?.Modules?.Find(item => item != null && item.Type == "downedFinisher");
				return;
			}
	}
}

public sealed class ModularEnemySpriteSet : MonoBehaviour
{
	[SerializeField] private List<string> m_keys = new List<string>();
	[SerializeField] private List<Sprite> m_sprites = new List<Sprite>();
	private Dictionary<string, Sprite> m_lookup;
	public void Configure(Dictionary<string, Sprite> i_sprites)
	{
		m_keys.Clear(); m_sprites.Clear();
		foreach (KeyValuePair<string, Sprite> pair in i_sprites) { m_keys.Add(pair.Key); m_sprites.Add(pair.Value); }
		m_lookup = null;
	}
	public bool TryGet(string i_bone, string i_region, out Sprite o_sprite)
	{
		if (m_lookup == null)
		{
			m_lookup = new Dictionary<string, Sprite>(StringComparer.Ordinal);
			for (int index = 0; index < Mathf.Min(m_keys.Count, m_sprites.Count); index++) m_lookup[m_keys[index]] = m_sprites[index];
		}
		return m_lookup.TryGetValue(i_bone + "|" + i_region, out o_sprite);
	}
	public bool TryGetEvent(string i_region, out Sprite o_sprite)
	{
		if (m_lookup == null)
		{
			m_lookup = new Dictionary<string, Sprite>(StringComparer.Ordinal);
			for (int index = 0; index < Mathf.Min(m_keys.Count, m_sprites.Count); index++) m_lookup[m_keys[index]] = m_sprites[index];
		}
		return m_lookup.TryGetValue("@event|" + i_region, out o_sprite);
	}
}

public sealed class ModularEnemyAnimationController : MonoBehaviour
{
	[SerializeField] private string m_definitionId;
	private EnemyAnimationDefinition m_animation;
	private readonly Dictionary<string, Transform> m_bones = new Dictionary<string, Transform>(StringComparer.Ordinal);
	private readonly Dictionary<string, Vector3> m_basePosition = new Dictionary<string, Vector3>(StringComparer.Ordinal);
	private readonly Dictionary<string, Vector3> m_baseScale = new Dictionary<string, Vector3>(StringComparer.Ordinal);
	private readonly Dictionary<string, float> m_baseRotation = new Dictionary<string, float>(StringComparer.Ordinal);
	private readonly Dictionary<string, SpriteRenderer> m_renderers = new Dictionary<string, SpriteRenderer>(StringComparer.Ordinal);
	private readonly Dictionary<string, Sprite> m_baseSprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
	private readonly Dictionary<string, Color> m_baseColors = new Dictionary<string, Color>(StringComparer.Ordinal);
	private readonly Dictionary<string, int> m_baseSortingOrders = new Dictionary<string, int>(StringComparer.Ordinal);
	private ModularEnemySpriteSet m_spriteSet;
	private string m_clipName;
	private float m_started;
	private int m_nextEvent;
	private int m_eventCycle;
	private int m_nextEffectTrigger;
	private int m_effectTriggerCycle;

	public void Configure(string i_definitionId) { m_definitionId = i_definitionId; Resolve(); }
	public string GetCurrentClipName() { return m_clipName; }

	private void OnEnable() { Resolve(); CacheBones(); Play("idle", true); }

	public void Play(string i_clip, bool i_restart = false)
	{
		if (!i_restart && m_clipName == i_clip) return;
		if (m_animation?.Clips == null || !m_animation.Clips.ContainsKey(i_clip)) return;
		m_clipName = i_clip;
		m_started = Time.time;
		m_nextEvent = 0;
		m_eventCycle = 0;
		m_nextEffectTrigger = 0;
		m_effectTriggerCycle = 0;
		ResetPose();
	}

	public bool HasEvent(string i_clip, string i_type)
	{
		if (m_animation?.Clips == null || !m_animation.Clips.TryGetValue(i_clip, out EnemyAnimationClipDefinition clip)
			|| clip.Events == null) return false;
		return clip.Events.Exists(item => item != null && item.Type == i_type);
	}

	public int CountEvents(string i_clip, string i_type)
	{
		if (m_animation?.Clips == null || !m_animation.Clips.TryGetValue(i_clip, out EnemyAnimationClipDefinition clip)
			|| clip.Events == null) return 0;
		return clip.Events.FindAll(item => item != null && item.Type == i_type).Count;
	}

	private void Update()
	{
		NPC owner = GetComponent<NPC>();
		if (owner != null && owner.GetStateActorCurrent() == StateActor.Ragdoll) return;
		if (m_animation?.Clips == null || string.IsNullOrEmpty(m_clipName) || !m_animation.Clips.TryGetValue(m_clipName, out EnemyAnimationClipDefinition clip)) return;
		float elapsed = (Time.time - m_started) * (m_animation.SpeedMultiplier ?? 1f);
		ApplyEvents(clip, elapsed, owner);
		ApplyEffectTriggers(clip, elapsed);
		if (clip.Loop) elapsed %= clip.DurationSeconds; else elapsed = Mathf.Min(elapsed, clip.DurationSeconds);
		EnemyAnimationFrameDefinition from = clip.Frames[0], to = clip.Frames[clip.Frames.Count - 1];
		for (int index = 0; index < clip.Frames.Count; index++)
		{
			if (clip.Frames[index].Time <= elapsed) from = clip.Frames[index];
			if (clip.Frames[index].Time >= elapsed) { to = clip.Frames[index]; break; }
		}
		float blend = Mathf.Approximately(from.Time, to.Time) ? 0f : Mathf.InverseLerp(from.Time, to.Time, elapsed);
		HashSet<string> animated = new HashSet<string>(from.Bones.Keys, StringComparer.Ordinal);
		foreach (string bone in to.Bones.Keys) animated.Add(bone);
		foreach (string bone in animated)
		{
			if (!m_bones.TryGetValue(bone, out Transform target)) continue;
			from.Bones.TryGetValue(bone, out EnemyBonePoseDefinition a);
			to.Bones.TryGetValue(bone, out EnemyBonePoseDefinition b);
			if (a == null) a = b; if (b == null) b = a; if (a == null) continue;
			Vector3 basePos = m_basePosition[bone], baseScale = m_baseScale[bone];
			if (m_renderers.TryGetValue(bone, out SpriteRenderer frameRenderer))
			{
				if (a.RuntimeSprite != null) frameRenderer.sprite = a.RuntimeSprite;
				else if (!string.IsNullOrEmpty(a.Region) && m_spriteSet != null && m_spriteSet.TryGet(bone, a.Region, out Sprite frameSprite)) frameRenderer.sprite = frameSprite;
			}
			Vector3 posA = new Vector3(a.X ?? basePos.x, a.Y ?? basePos.y, basePos.z);
			Vector3 posB = new Vector3(b.X ?? basePos.x, b.Y ?? basePos.y, basePos.z);
			Vector3 scaleA = new Vector3(a.ScaleX ?? baseScale.x, a.ScaleY ?? baseScale.y, baseScale.z);
			Vector3 scaleB = new Vector3(b.ScaleX ?? baseScale.x, b.ScaleY ?? baseScale.y, baseScale.z);
			target.localPosition = Vector3.Lerp(posA, posB, blend);
			target.localScale = Vector3.Lerp(scaleA, scaleB, blend);
			target.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(a.Rotation ?? m_baseRotation[bone], b.Rotation ?? m_baseRotation[bone], blend));
			if (m_renderers.TryGetValue(bone, out SpriteRenderer visual))
			{
				Color baseColor = m_baseColors[bone];
				Color colorA = new Color(a.ColorR ?? baseColor.r, a.ColorG ?? baseColor.g, a.ColorB ?? baseColor.b, a.ColorA ?? baseColor.a);
				Color colorB = new Color(b.ColorR ?? baseColor.r, b.ColorG ?? baseColor.g, b.ColorB ?? baseColor.b, b.ColorA ?? baseColor.a);
				visual.color = Color.Lerp(colorA, colorB, blend);
				visual.sortingOrder = Mathf.RoundToInt(Mathf.Lerp(a.RuntimeSortingOrder ?? m_baseSortingOrders[bone], b.RuntimeSortingOrder ?? m_baseSortingOrders[bone], blend));
			}
		}
	}

	private void ApplyEffectTriggers(EnemyAnimationClipDefinition i_clip, float i_totalElapsed)
	{
		NormalizedAnimationPresentation presentation = i_clip.NormalizedPresentation;
		if (presentation == null || presentation.EffectTriggers.Count == 0 || i_clip.DurationSeconds <= 0f) return;
		int guard = 0;
		while (guard++ < 256)
		{
			if (!i_clip.Loop && m_effectTriggerCycle > 0) return;
			NormalizedEffectTrigger trigger = presentation.EffectTriggers[m_nextEffectTrigger];
			float occurrence = m_effectTriggerCycle * i_clip.DurationSeconds + trigger.Time;
			if (occurrence > i_totalElapsed + 0.0001f) return;
			foreach (string effectId in trigger.Effects)
			{
				NormalizedEffectDefinition effect = null;
				foreach (NormalizedEffectDefinition candidate in presentation.Effects)
					if (candidate != null && candidate.Id == effectId) { effect = candidate; break; }
				if (effect == null) continue;
				string targetName = effect.Target.StartsWith("enemy-bone/", StringComparison.Ordinal)
					? effect.Target.Substring("enemy-bone/".Length)
					: effect.Target.StartsWith("bone/", StringComparison.Ordinal) ? effect.Target.Substring(5) : null;
				if (targetName != null && m_bones.TryGetValue(targetName, out Transform parent)) NormalizedParticleEffectRuntime.Play(effect, parent);
			}
			m_nextEffectTrigger++;
			if (m_nextEffectTrigger < presentation.EffectTriggers.Count) continue;
			m_nextEffectTrigger = 0;
			m_effectTriggerCycle++;
		}
	}

	private void ApplyEvents(EnemyAnimationClipDefinition i_clip, float i_totalElapsed, NPC i_owner)
	{
		List<EnemyAnimationEventDefinition> events = i_clip.Events;
		if (events == null || events.Count == 0 || i_clip.DurationSeconds <= 0f || i_owner == null) return;
		ModularEnemy modularEnemy = i_owner as ModularEnemy;
		if (modularEnemy == null) return;
		int guard = 0;
		while (guard++ < 256)
		{
			if (!i_clip.Loop && m_eventCycle > 0) return;
			EnemyAnimationEventDefinition animationEvent = events[m_nextEvent];
			float occurrence = m_eventCycle * i_clip.DurationSeconds + animationEvent.Time;
			if (occurrence > i_totalElapsed + 0.0001f) return;
			HandleEvent(animationEvent, modularEnemy);
			m_nextEvent++;
			if (m_nextEvent < events.Count) continue;
			m_nextEvent = 0;
			m_eventCycle++;
		}
	}

	private void HandleEvent(EnemyAnimationEventDefinition i_event, ModularEnemy i_owner)
	{
		if (i_event.Type == "attackHit" || i_event.Type == "impulse") i_owner.HandleAnimationEvent(i_event);
		else if (i_event.Type == "sound") GetComponent<ModularEnemyEventAssets>()?.PlaySound(i_event.File, i_event.Volume ?? 1f);
		else if (i_event.Type == "cameraShake")
		{
			if (CommonReferences.Instance == null) return;
			CameraXGame camera = CommonReferences.Instance.GetManagerCamerasXGame()?.GetCameraXGameCurrent();
			if (camera != null) camera.Shake(i_event.Amount ?? 0f, 0.1f);
		}
		else if (i_event.Type == "spriteEffect") PlaySpriteEffect(i_event);
	}

	private void PlaySpriteEffect(EnemyAnimationEventDefinition i_event)
	{
		if (m_spriteSet == null || !m_spriteSet.TryGetEvent(i_event.Region, out Sprite sprite)) return;
		Transform parent = transform;
		if (!string.IsNullOrEmpty(i_event.Bone) && m_bones.TryGetValue(i_event.Bone, out Transform bone)) parent = bone;
		GameObject effect = new GameObject("event_" + i_event.Region);
		effect.transform.SetParent(parent, false);
		effect.transform.localPosition = new Vector3(i_event.X ?? 0f, i_event.Y ?? 0f, 0f);
		float scale = i_event.Scale ?? 1f;
		effect.transform.localScale = new Vector3(scale, scale, 1f);
		SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
		renderer.sprite = sprite;
		renderer.sortingLayerName = "Actor";
		renderer.sortingOrder = i_event.SortingOrder ?? 100;
		UnityEngine.Object.Destroy(effect, i_event.DurationSeconds ?? 0.15f);
	}

	private void Resolve()
	{
		foreach (EnemyDefinition definition in ModLoaderRuntime.EnemyDefinitions)
			if (definition.Id.ToString() == m_definitionId) { m_animation = definition.Animation; return; }
	}

	private void CacheBones()
	{
		m_bones.Clear(); m_basePosition.Clear(); m_baseScale.Clear(); m_baseRotation.Clear(); m_renderers.Clear(); m_baseSprites.Clear(); m_baseColors.Clear(); m_baseSortingOrders.Clear();
		m_spriteSet = GetComponent<ModularEnemySpriteSet>();
		if (m_animation == null) return;
		EnemyDefinition definition = null;
		foreach (EnemyDefinition candidate in ModLoaderRuntime.EnemyDefinitions) if (candidate.Id.ToString() == m_definitionId) { definition = candidate; break; }
		if (definition == null) return;
		foreach (EnemyBoneDefinition bone in definition.Visual.Bones)
		{
			Transform target = FindRecursive(transform, bone.Id);
			if (target == null) continue;
			m_bones[bone.Id] = target; m_basePosition[bone.Id] = target.localPosition;
			m_baseScale[bone.Id] = target.localScale; m_baseRotation[bone.Id] = target.localEulerAngles.z;
			SpriteRenderer renderer = target.GetComponentInChildren<SpriteRenderer>(true);
			if (renderer != null) { m_renderers[bone.Id] = renderer; m_baseSprites[bone.Id] = renderer.sprite; m_baseColors[bone.Id] = renderer.color; m_baseSortingOrders[bone.Id] = renderer.sortingOrder; }
		}
	}

	private void ResetPose()
	{
		foreach (KeyValuePair<string, Transform> pair in m_bones)
		{
			pair.Value.localPosition = m_basePosition[pair.Key]; pair.Value.localScale = m_baseScale[pair.Key];
			pair.Value.localRotation = Quaternion.Euler(0f, 0f, m_baseRotation[pair.Key]);
			if (m_renderers.TryGetValue(pair.Key, out SpriteRenderer renderer) && m_baseSprites.TryGetValue(pair.Key, out Sprite sprite))
			{
				renderer.sprite = sprite; renderer.color = m_baseColors[pair.Key]; renderer.sortingOrder = m_baseSortingOrders[pair.Key];
			}
		}
	}

	private static Transform FindRecursive(Transform i_root, string i_name)
	{
		if (i_root.name == i_name) return i_root;
		for (int index = 0; index < i_root.childCount; index++) { Transform result = FindRecursive(i_root.GetChild(index), i_name); if (result != null) return result; }
		return null;
	}
}

public sealed class ModularEnemyEventAssets : MonoBehaviour
{
	private static readonly Dictionary<string, AudioClip> s_sharedAudio = new Dictionary<string, AudioClip>(StringComparer.Ordinal);
	[SerializeField] private string m_definitionId;
	[SerializeField] private List<string> m_audioPaths = new List<string>();
	[SerializeField] private List<AudioClip> m_audioClips = new List<AudioClip>();
	private Dictionary<string, AudioClip> m_audioLookup;
	private AudioSource m_audioSource;

	public void Configure(string i_definitionId) { m_definitionId = i_definitionId; }

	public void AddAudio(string i_path, AudioClip i_clip)
	{
		if (string.IsNullOrEmpty(i_path) || i_clip == null || m_audioPaths.Contains(i_path)) return;
		m_audioPaths.Add(i_path);
		m_audioClips.Add(i_clip);
		m_audioLookup = null;
		s_sharedAudio[m_definitionId + "|" + i_path] = i_clip;
	}

	public void PlaySound(string i_path, float i_volume)
	{
		if (m_audioLookup == null)
		{
			m_audioLookup = new Dictionary<string, AudioClip>(StringComparer.Ordinal);
			for (int index = 0; index < Mathf.Min(m_audioPaths.Count, m_audioClips.Count); index++)
				if (!string.IsNullOrEmpty(m_audioPaths[index]) && m_audioClips[index] != null) m_audioLookup[m_audioPaths[index]] = m_audioClips[index];
		}
		if (!m_audioLookup.TryGetValue(i_path ?? string.Empty, out AudioClip clip)
			&& !s_sharedAudio.TryGetValue(m_definitionId + "|" + (i_path ?? string.Empty), out clip)) return;
		if (m_audioSource == null)
		{
			m_audioSource = gameObject.AddComponent<AudioSource>();
			m_audioSource.playOnAwake = false;
			m_audioSource.spatialBlend = 1f;
			m_audioSource.rolloffMode = AudioRolloffMode.Linear;
			m_audioSource.minDistance = 2f;
			m_audioSource.maxDistance = 30f;
			m_audioSource.dopplerLevel = 0f;
			if (CommonReferences.Instance != null && CommonReferences.Instance.GetManagerAudio() != null)
			{
				var groups = CommonReferences.Instance.GetManagerAudio().GetAudioMixer().FindMatchingGroups("SFX");
				if (groups.Length > 0) m_audioSource.outputAudioMixerGroup = groups[0];
			}
		}
		m_audioSource.PlayOneShot(clip, Mathf.Clamp01(i_volume));
	}
}

public static class NormalizedParticleEffectRuntime
{
	private static Material s_particleMaterial;

	public static void Play(NormalizedEffectDefinition i_effect, Transform i_parent)
	{
		if (i_effect == null || i_parent == null) return;
		string kind = string.IsNullOrWhiteSpace(i_effect.Kind) ? "particle" : i_effect.Kind;
		if (kind == "light") { PlayLight(i_effect, i_parent); return; }
		if (kind == "trail") { PlayTrail(i_effect, i_parent); return; }
		if (kind == "decal") { PlayDecal(i_effect, i_parent); return; }
		GameObject effectObject = new GameObject("normalized_" + i_effect.Id.Replace('/', '_'));
		effectObject.SetActive(false);
		effectObject.transform.SetParent(i_parent, false);
		effectObject.transform.localPosition = new Vector3(i_effect.PositionX, i_effect.PositionY, i_effect.PositionZ);
		effectObject.transform.localRotation = Quaternion.Euler(i_effect.RotationX, i_effect.RotationY, i_effect.RotationZ);
		effectObject.transform.localScale = new Vector3(i_effect.ScaleX, i_effect.ScaleY, i_effect.ScaleZ);
		ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
		ParticleSystem.MainModule main = particles.main;
		main.duration = Mathf.Max(0.01f, i_effect.DurationSeconds);
		main.loop = i_effect.Loop;
		main.maxParticles = Mathf.Clamp(i_effect.MaxParticles, 1, 10000);
		main.startLifetime = new ParticleSystem.MinMaxCurve(i_effect.StartLifetimeMin, i_effect.StartLifetimeMax);
		main.startSpeed = new ParticleSystem.MinMaxCurve(i_effect.StartSpeedMin, i_effect.StartSpeedMax);
		main.startSize = new ParticleSystem.MinMaxCurve(i_effect.StartSizeMin, i_effect.StartSizeMax);
		main.gravityModifier = new ParticleSystem.MinMaxCurve(i_effect.GravityMin, i_effect.GravityMax);
		main.simulationSpace = i_effect.SimulationSpace == "World" ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
		if (i_effect.StartColorMin != null && i_effect.StartColorMax != null)
			main.startColor = new ParticleSystem.MinMaxGradient(ToColor(i_effect.StartColorMin), ToColor(i_effect.StartColorMax));
		ParticleSystem.EmissionModule emission = particles.emission;
		emission.rateOverTime = new ParticleSystem.MinMaxCurve(i_effect.EmissionRateMin, i_effect.EmissionRateMax);
		ParticleSystem.ShapeModule shape = particles.shape;
		shape.enabled = i_effect.ShapeEnabled;
		if (Enum.TryParse(i_effect.Shape, true, out ParticleSystemShapeType shapeType)) shape.shapeType = shapeType;
		shape.radius = Mathf.Max(0f, i_effect.ShapeRadius);
		shape.angle = Mathf.Clamp(i_effect.ShapeAngle, 0f, 90f);
		ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
		if (s_particleMaterial == null)
		{
			Shader shader = Shader.Find("Sprites/Default");
			if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
			if (shader != null) s_particleMaterial = new Material(shader) { name = "Normalized Particle Material" };
		}
		Material texturedMaterial = null;
		if (i_effect.RuntimeTexture != null)
		{
			texturedMaterial = CreateMaterial(i_effect.RuntimeTexture);
			if (texturedMaterial != null) renderer.sharedMaterial = texturedMaterial;
			int tilesX = Mathf.Clamp(i_effect.TextureSheetTilesX, 1, 64), tilesY = Mathf.Clamp(i_effect.TextureSheetTilesY, 1, 64);
			if (tilesX > 1 || tilesY > 1)
			{
				ParticleSystem.TextureSheetAnimationModule sheet = particles.textureSheetAnimation;
				sheet.enabled = true; sheet.mode = ParticleSystemAnimationMode.Grid; sheet.numTilesX = tilesX; sheet.numTilesY = tilesY;
				sheet.animation = ParticleSystemAnimationType.WholeSheet;
				sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
				sheet.startFrame = new ParticleSystem.MinMaxCurve(Mathf.Clamp(i_effect.TextureSheetFrame, 0, tilesX * tilesY - 1) / (float)(tilesX * tilesY));
			}
		}
		else if (s_particleMaterial != null) renderer.sharedMaterial = s_particleMaterial;
		ApplyAdvancedModules(particles, i_effect);
		if (!string.IsNullOrEmpty(i_effect.SortingLayer)) renderer.sortingLayerName = i_effect.SortingLayer;
		renderer.sortingOrder = i_effect.SortingOrder;
		if (Enum.TryParse(i_effect.RenderMode, true, out ParticleSystemRenderMode renderMode) && renderMode != ParticleSystemRenderMode.Mesh) renderer.renderMode = renderMode;
		effectObject.SetActive(true);
		particles.Play();
		if (i_effect.EmissionRateMin <= 0f && i_effect.EmissionRateMax <= 0f && (i_effect.Bursts == null || i_effect.Bursts.Count == 0))
			particles.Emit(Mathf.Clamp(i_effect.MaxParticles, 1, 12));
		float lifetime = Mathf.Max(0.1f, i_effect.DurationSeconds + i_effect.StartLifetimeMax + 1f);
		UnityEngine.Object.Destroy(effectObject, lifetime);
		if (texturedMaterial != null) UnityEngine.Object.Destroy(texturedMaterial, lifetime);
	}

	public static void ApplyAdvancedModules(ParticleSystem i_particles, NormalizedEffectDefinition i_effect)
	{
		if (i_particles == null || i_effect == null) return;
		ParticleSystem.MainModule main = i_particles.main;
		if (i_effect.StartLifetimeCurve != null) main.startLifetime = ToMinMaxCurve(i_effect.StartLifetimeCurve);
		if (i_effect.StartSpeedCurve != null) main.startSpeed = ToMinMaxCurve(i_effect.StartSpeedCurve);
		if (i_effect.StartSizeCurve != null) main.startSize = ToMinMaxCurve(i_effect.StartSizeCurve);
		if (i_effect.GravityCurve != null) main.gravityModifier = ToMinMaxCurve(i_effect.GravityCurve);
		if (i_effect.StartColorGradient != null) main.startColor = ToMinMaxGradient(i_effect.StartColorGradient);
		ParticleSystem.EmissionModule emission = i_particles.emission;
		if (i_effect.EmissionRateCurve != null) emission.rateOverTime = ToMinMaxCurve(i_effect.EmissionRateCurve);
		if (i_effect.EmissionRateOverDistanceCurve != null) emission.rateOverDistance = ToMinMaxCurve(i_effect.EmissionRateOverDistanceCurve);
		if (i_effect.Bursts != null && i_effect.Bursts.Count > 0)
		{
			ParticleSystem.Burst[] bursts = i_effect.Bursts.Select(i_burst =>
			{
				ParticleSystem.Burst burst = new ParticleSystem.Burst(Mathf.Max(0f, i_burst.Time),
					(short)Mathf.Clamp(Mathf.RoundToInt(i_burst.CountMin), 0, short.MaxValue), (short)Mathf.Clamp(Mathf.RoundToInt(i_burst.CountMax), 0, short.MaxValue),
					Mathf.Max(1, i_burst.CycleCount), Mathf.Max(0f, i_burst.RepeatInterval));
				burst.probability = Mathf.Clamp01(i_burst.Probability); return burst;
			}).ToArray();
			emission.SetBursts(bursts);
		}
		if (i_effect.ColorOverLifetime != null)
		{
			ParticleSystem.ColorOverLifetimeModule color = i_particles.colorOverLifetime; color.enabled = true; color.color = ToMinMaxGradient(i_effect.ColorOverLifetime);
		}
		ApplySize(i_particles, i_effect.SizeOverLifetime);
		ApplyVelocity(i_particles, i_effect.VelocityOverLifetime);
		ApplyRotation(i_particles, i_effect.RotationOverLifetime);
		ApplyNoise(i_particles, i_effect.Noise);
		int tilesX = Mathf.Clamp(i_effect.TextureSheetTilesX, 1, 64), tilesY = Mathf.Clamp(i_effect.TextureSheetTilesY, 1, 64);
		if (tilesX > 1 || tilesY > 1 || i_effect.TextureSheetFrameOverTime != null)
		{
			ParticleSystem.TextureSheetAnimationModule sheet = i_particles.textureSheetAnimation; sheet.enabled = true;
			sheet.mode = ParticleSystemAnimationMode.Grid; sheet.numTilesX = tilesX; sheet.numTilesY = tilesY;
			if (Enum.TryParse(i_effect.TextureSheetAnimation, true, out ParticleSystemAnimationType animation)) sheet.animation = animation;
			sheet.cycleCount = Mathf.Max(1, i_effect.TextureSheetCycleCount); sheet.rowIndex = Mathf.Clamp(i_effect.TextureSheetRowIndex, 0, tilesY - 1); sheet.useRandomRow = i_effect.TextureSheetUseRandomRow;
			sheet.frameOverTime = i_effect.TextureSheetFrameOverTime == null ? new ParticleSystem.MinMaxCurve(0f) : ToMinMaxCurve(i_effect.TextureSheetFrameOverTime);
			sheet.startFrame = i_effect.TextureSheetStartFrame == null
				? new ParticleSystem.MinMaxCurve(Mathf.Clamp(i_effect.TextureSheetFrame, 0, tilesX * tilesY - 1) / (float)(tilesX * tilesY)) : ToMinMaxCurve(i_effect.TextureSheetStartFrame);
		}
	}

	private static void ApplySize(ParticleSystem i_particles, NormalizedParticleAxisCurves i_value)
	{
		if (i_value == null || !i_value.Enabled) return; ParticleSystem.SizeOverLifetimeModule module = i_particles.sizeOverLifetime; module.enabled = true; module.separateAxes = i_value.SeparateAxes;
		if (i_value.SeparateAxes) { module.x = ToMinMaxCurve(i_value.X); module.y = ToMinMaxCurve(i_value.Y); module.z = ToMinMaxCurve(i_value.Z); } else module.size = ToMinMaxCurve(i_value.X);
	}

	private static void ApplyVelocity(ParticleSystem i_particles, NormalizedParticleAxisCurves i_value)
	{
		if (i_value == null || !i_value.Enabled) return; ParticleSystem.VelocityOverLifetimeModule module = i_particles.velocityOverLifetime; module.enabled = true;
		module.x = ToMinMaxCurve(i_value.X); module.y = ToMinMaxCurve(i_value.Y); module.z = ToMinMaxCurve(i_value.Z);
		if (Enum.TryParse(i_value.Space, true, out ParticleSystemSimulationSpace space)) module.space = space;
	}

	private static void ApplyRotation(ParticleSystem i_particles, NormalizedParticleAxisCurves i_value)
	{
		if (i_value == null || !i_value.Enabled) return; ParticleSystem.RotationOverLifetimeModule module = i_particles.rotationOverLifetime; module.enabled = true; module.separateAxes = i_value.SeparateAxes;
		if (i_value.SeparateAxes) { module.x = ToMinMaxCurve(i_value.X); module.y = ToMinMaxCurve(i_value.Y); module.z = ToMinMaxCurve(i_value.Z); } else module.z = ToMinMaxCurve(i_value.Z ?? i_value.X);
	}

	private static void ApplyNoise(ParticleSystem i_particles, NormalizedParticleNoise i_value)
	{
		if (i_value == null || !i_value.Enabled) return; ParticleSystem.NoiseModule module = i_particles.noise; module.enabled = true; module.separateAxes = i_value.SeparateAxes;
		module.strengthX = ToMinMaxCurve(i_value.StrengthX); module.strengthY = ToMinMaxCurve(i_value.StrengthY); module.strengthZ = ToMinMaxCurve(i_value.StrengthZ);
		module.frequency = Mathf.Max(0.0001f, i_value.Frequency); module.scrollSpeed = ToMinMaxCurve(i_value.ScrollSpeed); module.damping = i_value.Damping;
		module.octaveCount = Mathf.Clamp(i_value.OctaveCount, 1, 4); module.octaveMultiplier = Mathf.Clamp01(i_value.OctaveMultiplier); module.octaveScale = Mathf.Max(0.0001f, i_value.OctaveScale);
		if (Enum.TryParse(i_value.Quality, true, out ParticleSystemNoiseQuality quality)) module.quality = quality;
	}

	private static ParticleSystem.MinMaxCurve ToMinMaxCurve(NormalizedParticleCurve i_value)
	{
		if (i_value == null) return new ParticleSystem.MinMaxCurve(0f);
		AnimationCurve min = ToAnimationCurve(i_value.CurveMin), max = ToAnimationCurve(i_value.CurveMax);
		if (i_value.Mode == "Curve") return new ParticleSystem.MinMaxCurve(i_value.Multiplier, max);
		if (i_value.Mode == "TwoCurves") return new ParticleSystem.MinMaxCurve(i_value.Multiplier, min, max);
		if (i_value.Mode == "TwoConstants") return new ParticleSystem.MinMaxCurve(i_value.ConstantMin, i_value.ConstantMax);
		return new ParticleSystem.MinMaxCurve(i_value.ConstantMax);
	}

	private static AnimationCurve ToAnimationCurve(List<NormalizedParticleCurveKey> i_keys)
	{
		if (i_keys == null || i_keys.Count == 0) return AnimationCurve.Linear(0f, 0f, 1f, 0f);
		return new AnimationCurve(i_keys.Select(i_key => new Keyframe(i_key.Time, i_key.Value, i_key.InTangent, i_key.OutTangent)).ToArray());
	}

	private static ParticleSystem.MinMaxGradient ToMinMaxGradient(NormalizedParticleGradient i_value)
	{
		Color minColor = ToColor(i_value.ColorMin ?? new NormalizedColor { R = 1f, G = 1f, B = 1f, A = 1f }), maxColor = ToColor(i_value.ColorMax ?? new NormalizedColor { R = 1f, G = 1f, B = 1f, A = 1f });
		if (i_value.Mode == "Gradient") return new ParticleSystem.MinMaxGradient(ToGradient(i_value.GradientMax));
		if (i_value.Mode == "RandomColor") { ParticleSystem.MinMaxGradient random = new ParticleSystem.MinMaxGradient(ToGradient(i_value.GradientMax)); random.mode = ParticleSystemGradientMode.RandomColor; return random; }
		if (i_value.Mode == "TwoGradients") return new ParticleSystem.MinMaxGradient(ToGradient(i_value.GradientMin), ToGradient(i_value.GradientMax));
		if (i_value.Mode == "TwoColors") return new ParticleSystem.MinMaxGradient(minColor, maxColor);
		return new ParticleSystem.MinMaxGradient(maxColor);
	}

	private static Gradient ToGradient(List<NormalizedParticleColorKey> i_keys)
	{
		List<NormalizedParticleColorKey> keys = i_keys == null || i_keys.Count == 0 ? new List<NormalizedParticleColorKey> { new NormalizedParticleColorKey { Time = 0f, Color = new NormalizedColor { R = 1, G = 1, B = 1, A = 1 } }, new NormalizedParticleColorKey { Time = 1f, Color = new NormalizedColor { R = 1, G = 1, B = 1, A = 1 } } } : i_keys;
		Gradient gradient = new Gradient(); gradient.SetKeys(keys.Select(i_key => new GradientColorKey(ToColor(i_key.Color), Mathf.Clamp01(i_key.Time))).ToArray(), keys.Select(i_key => new GradientAlphaKey(ToColor(i_key.Color).a, Mathf.Clamp01(i_key.Time))).ToArray()); return gradient;
	}

	private static void PlayLight(NormalizedEffectDefinition i_effect, Transform i_parent)
	{
		GameObject effectObject = CreateEffectObject(i_effect, i_parent, "light");
		UnityEngine.Rendering.Universal.Light2D light = effectObject.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
		light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
		light.color = ToColor(i_effect.StartColorMin ?? new NormalizedColor { R = 1f, G = 1f, B = 1f, A = 1f });
		light.intensity = Mathf.Clamp(i_effect.LightIntensity, 0f, 20f);
		light.pointLightOuterRadius = Mathf.Clamp(i_effect.LightRadius, 0.01f, 100f);
		light.pointLightInnerRadius = light.pointLightOuterRadius * 0.35f;
		UnityEngine.Object.Destroy(effectObject, Mathf.Max(0.02f, i_effect.DurationSeconds));
	}

	private static void PlayTrail(NormalizedEffectDefinition i_effect, Transform i_parent)
	{
		GameObject effectObject = CreateEffectObject(i_effect, i_parent, "trail");
		TrailRenderer trail = effectObject.AddComponent<TrailRenderer>();
		trail.time = Mathf.Clamp(i_effect.TrailTime, 0.01f, 30f); trail.widthMultiplier = Mathf.Clamp(i_effect.TrailWidth, 0.001f, 20f);
		trail.minVertexDistance = 0.02f; trail.numCapVertices = 2; trail.numCornerVertices = 2;
		Color first = ToColor(i_effect.StartColorMin ?? new NormalizedColor { R = 1f, G = 1f, B = 1f, A = 1f });
		Color last = ToColor(i_effect.StartColorMax ?? new NormalizedColor { R = 1f, G = 1f, B = 1f, A = 0f });
		trail.startColor = first; trail.endColor = last;
		Material material = CreateMaterial(i_effect.RuntimeTexture); if (material != null) trail.sharedMaterial = material;
		float lifetime = Mathf.Max(0.02f, i_effect.DurationSeconds) + trail.time;
		UnityEngine.Object.Destroy(effectObject, lifetime); if (material != null) UnityEngine.Object.Destroy(material, lifetime);
	}

	private static void PlayDecal(NormalizedEffectDefinition i_effect, Transform i_parent)
	{
		if (i_effect.RuntimeTexture == null) return;
		GameObject effectObject = CreateEffectObject(i_effect, i_parent, "decal");
		int tilesX = Mathf.Clamp(i_effect.TextureSheetTilesX, 1, 64), tilesY = Mathf.Clamp(i_effect.TextureSheetTilesY, 1, 64);
		int frame = Mathf.Clamp(i_effect.TextureSheetFrame, 0, tilesX * tilesY - 1), column = frame % tilesX, row = tilesY - 1 - frame / tilesX;
		float width = i_effect.RuntimeTexture.width / (float)tilesX, height = i_effect.RuntimeTexture.height / (float)tilesY;
		Sprite sprite = Sprite.Create(i_effect.RuntimeTexture, new Rect(column * width, row * height, width, height), new Vector2(.5f, .5f), Mathf.Clamp(i_effect.DecalPixelsPerUnit, 1f, 1024f), 0, SpriteMeshType.FullRect);
		SpriteRenderer renderer = effectObject.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = ToColor(i_effect.StartColorMin ?? new NormalizedColor { R = 1f, G = 1f, B = 1f, A = 1f });
		if (!string.IsNullOrEmpty(i_effect.SortingLayer)) renderer.sortingLayerName = i_effect.SortingLayer; renderer.sortingOrder = i_effect.SortingOrder;
		float lifetime = Mathf.Max(0.02f, i_effect.DurationSeconds); UnityEngine.Object.Destroy(effectObject, lifetime); UnityEngine.Object.Destroy(sprite, lifetime);
	}

	private static GameObject CreateEffectObject(NormalizedEffectDefinition i_effect, Transform i_parent, string i_kind)
	{
		GameObject result = new GameObject("normalized_" + i_kind + "_" + i_effect.Id.Replace('/', '_'));
		result.transform.SetParent(i_parent, false); result.transform.localPosition = new Vector3(i_effect.PositionX, i_effect.PositionY, i_effect.PositionZ);
		result.transform.localRotation = Quaternion.Euler(i_effect.RotationX, i_effect.RotationY, i_effect.RotationZ);
		result.transform.localScale = new Vector3(i_effect.ScaleX, i_effect.ScaleY, i_effect.ScaleZ); return result;
	}

	private static Material CreateMaterial(Texture2D i_texture)
	{
		Shader shader = Shader.Find("Sprites/Default"); if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
		if (shader == null) return null; Material material = new Material(shader) { name = "Normalized Effect Material" }; if (i_texture != null) material.mainTexture = i_texture; return material;
	}

	private static Color ToColor(NormalizedColor i_color) { return new Color(i_color.R, i_color.G, i_color.B, i_color.A); }
}

public sealed class ModularEnemyAudioLoader : MonoBehaviour
{
	public void Load(ModularEnemyEventAssets i_assets, EnemyDefinition i_definition, string i_packRoot)
	{
		HashSet<string> paths = new HashSet<string>(StringComparer.Ordinal);
		foreach (EnemyAnimationClipDefinition clip in i_definition.Animation.Clips.Values)
			foreach (EnemyAnimationEventDefinition animationEvent in clip.Events ?? new List<EnemyAnimationEventDefinition>())
				if (animationEvent != null && animationEvent.Type == "sound" && paths.Add(animationEvent.File))
					StartCoroutine(LoadClip(i_assets, i_definition, i_packRoot, animationEvent.File));
	}

	private IEnumerator LoadClip(ModularEnemyEventAssets i_assets, EnemyDefinition i_definition, string i_root, string i_relative)
	{
		string full;
		try
		{
			full = System.IO.Path.GetFullPath(System.IO.Path.Combine(i_root, i_relative));
			if (!ModPath.IsSafeRelativePath(i_relative) || !AssetPatchDiscovery.IsInside(full, i_root) || !System.IO.File.Exists(full))
				throw new System.IO.FileNotFoundException("Enemy audio is missing or outside its pack.", i_relative);
			long length = new System.IO.FileInfo(full).Length;
			if (length <= 0 || length > 32L * 1024L * 1024L) throw new System.IO.InvalidDataException("Enemy audio must be between 1 byte and 32 MiB.");
		}
		catch (Exception exception)
		{
			Report("enemy.factory-audio-file", exception.Message + ": " + i_relative, i_definition.Source);
			yield break;
		}
		AudioType type = System.IO.Path.GetExtension(full).Equals(".wav", StringComparison.OrdinalIgnoreCase) ? AudioType.WAV : AudioType.OGGVORBIS;
		using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(new Uri(full).AbsoluteUri, type))
		{
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success)
			{
				Report("enemy.factory-audio-decode", request.error + ": " + i_relative, i_definition.Source);
				yield break;
			}
			AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
			if (i_assets != null && clip != null) { clip.name = i_definition.Id + "/audio/" + i_relative; i_assets.AddAudio(i_relative, clip); }
		}
	}

	private static void Report(string i_code, string i_message, string i_source)
	{
		ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, i_code, i_message, i_source);
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
