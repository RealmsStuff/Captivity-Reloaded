using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	public static class ModLoaderRuntime
	{
		public static ContentRegistry Registry { get; private set; } = new ContentRegistry();
		public static AssetSlotRegistry AssetSlots { get; private set; } = new AssetSlotRegistry();
		public static IReadOnlyList<ModPack> LoadedPacks { get; private set; } = new ModPack[0];
		public static IReadOnlyList<ModPackStatus> PackStatuses { get; private set; } = new ModPackStatus[0];
		public static IReadOnlyList<CoreContentCatalogEntry> CoreContentCatalog { get; private set; } = new CoreContentCatalogEntry[0];
		public static LegacyContentMap LegacyContentMap { get; private set; } = new LegacyContentMap(null);
		public static IReadOnlyList<AssetPatchDefinition> AssetPatches { get; private set; } = new AssetPatchDefinition[0];
		public static IReadOnlyList<EnemyDefinition> EnemyDefinitions { get; private set; } = new EnemyDefinition[0];
		public static IReadOnlyList<ClothingDefinition> ClothingDefinitions { get; private set; } = new ClothingDefinition[0];
		public static IReadOnlyList<PlayerAttachmentDefinition> PlayerAttachmentDefinitions { get; private set; } = new PlayerAttachmentDefinition[0];
		public static IReadOnlyList<WeaponDefinition> WeaponDefinitions { get; private set; } = new WeaponDefinition[0];
		public static IReadOnlyList<UsableDefinition> UsableDefinitions { get; private set; } = new UsableDefinition[0];
		public static IReadOnlyList<StageDefinition> StageDefinitions { get; private set; } = new StageDefinition[0];
		public static IReadOnlyList<StageScriptDefinition> StageScriptDefinitions { get; private set; } = new StageScriptDefinition[0];
		public static IReadOnlyList<ChallengeDefinition> ChallengeDefinitions { get; private set; } = new ChallengeDefinition[0];
		public static IReadOnlyList<DifficultyDefinition> DifficultyDefinitions { get; private set; } = new DifficultyDefinition[0];
		public static IReadOnlyList<RuleProfileDefinition> RuleProfileDefinitions { get; private set; } = new RuleProfileDefinition[0];
		public static IReadOnlyList<NormalizedPlayerAnimationDefinition> PlayerAnimationDefinitions { get; private set; } = new NormalizedPlayerAnimationDefinition[0];
		public static IReadOnlyList<NormalizedEnemyAnimationDefinition> EnemyAnimationDefinitions { get; private set; } = new NormalizedEnemyAnimationDefinition[0];
		public static IReadOnlyList<CoreWeaponDefinition> CoreWeaponDefinitions { get; private set; } = new CoreWeaponDefinition[0];
		public static IReadOnlyList<CoreEnemyDefinition> CoreEnemyDefinitions { get; private set; } = new CoreEnemyDefinition[0];
		public static IReadOnlyList<CoreItemDefinition> CoreItemDefinitions { get; private set; } = new CoreItemDefinition[0];
		public static IReadOnlyList<CoreStageDefinition> CoreStageDefinitions { get; private set; } = new CoreStageDefinition[0];
		public static IReadOnlyList<CoreClothingDefinition> CoreClothingDefinitions { get; private set; } = new CoreClothingDefinition[0];
		public static IReadOnlyList<CoreChallengeDefinition> CoreChallengeDefinitions { get; private set; } = new CoreChallengeDefinition[0];
		public static ValidationReport LastReport { get; private set; } = new ValidationReport();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			Registry = new ContentRegistry();
			AssetSlots = new AssetSlotRegistry();
			ValidationReport report = new ValidationReport();
			List<ModPack> packs = new List<ModPack>();
			List<ModPackStatus> discoveryStatuses = new List<ModPackStatus>();
			CoreContentCatalog = new CoreContentCatalogEntry[0];
			LegacyContentMap = new LegacyContentMap(null);
			AssetPatches = new AssetPatchDefinition[0];
			EnemyDefinitions = new EnemyDefinition[0];
			ClothingDefinitions = new ClothingDefinition[0];
			PlayerAttachmentDefinitions = new PlayerAttachmentDefinition[0];
			WeaponDefinitions = new WeaponDefinition[0];
			UsableDefinitions = new UsableDefinition[0];
			StageDefinitions = new StageDefinition[0];
			StageScriptDefinitions = new StageScriptDefinition[0];
			ChallengeDefinitions = new ChallengeDefinition[0];
			DifficultyDefinitions = new DifficultyDefinition[0];
			RuleProfileDefinitions = new RuleProfileDefinition[0];
			PlayerAnimationDefinitions = new NormalizedPlayerAnimationDefinition[0];
			EnemyAnimationDefinitions = new NormalizedEnemyAnimationDefinition[0];
			CoreWeaponDefinitions = new CoreWeaponDefinition[0];
			CoreEnemyDefinitions = new CoreEnemyDefinition[0];
			CoreItemDefinitions = new CoreItemDefinition[0];
			CoreStageDefinitions = new CoreStageDefinition[0];
			CoreClothingDefinitions = new CoreClothingDefinition[0];
			CoreChallengeDefinitions = new CoreChallengeDefinition[0];
			PackStatuses = new ModPackStatus[0];

			TextAsset coreManifestAsset = Resources.Load<TextAsset>("Modding/Core/manifest");
			if (coreManifestAsset == null)
			{
				report.Add(ValidationSeverity.Error, "bootstrap.core-missing", "Packaged Core manifest could not be loaded.");
			}
			else
			{
				ManifestLoadResult core = ModManifestParser.Parse(coreManifestAsset.text, "core/manifest.json", i_isCore: true);
				report.Merge(core.Report);
				if (core.Report.IsValid) packs.Add(new ModPack(core.Manifest, core.Version, "core"));
			}

			TextAsset coreCatalogAsset = Resources.Load<TextAsset>("Modding/Core/catalog");
			if (coreCatalogAsset == null)
			{
				report.Add(ValidationSeverity.Error, "bootstrap.catalog-missing", "Packaged Core content catalog could not be loaded.");
			}
			else
			{
				CoreContentCatalogLoadResult catalog = CoreContentCatalogParser.Parse(coreCatalogAsset.text, "core/catalog.json");
				report.Merge(catalog.Report);
				if (catalog.Report.IsValid)
				{
					CoreContentCatalog = catalog.Entries;
					LegacyContentMap = new LegacyContentMap(catalog.Entries);
				}
			}

			string modsDirectory = ModStoragePaths.GetModsDirectory();
			if (!string.IsNullOrEmpty(modsDirectory))
			{
				report.Merge(ModInstallRecovery.RecoverInterruptedUpdates(modsDirectory));
				ModDiscoveryResult discovery = ModDiscovery.Discover(modsDirectory);
				report.Merge(discovery.Report);
				packs.AddRange(discovery.Packs);
				discoveryStatuses.AddRange(discovery.InvalidStatuses);
			}
			else report.Add(ValidationSeverity.Info, "bootstrap.external-disabled", "External mod discovery is unavailable on this platform.");

			DependencyResolutionResult dependencies = ModDependencyResolver.Resolve(packs, ModEnableState.IsEnabled);
			report.Merge(dependencies.Report);
			LoadedPacks = dependencies.OrderedPacks;
			ModAssetBundleRegistry.Initialize(LoadedPacks, report);
			discoveryStatuses.AddRange(dependencies.Statuses);
			discoveryStatuses.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
			PackStatuses = discoveryStatuses;
			ModContentDiscoveryResult coreContent = ModContentDiscovery.DiscoverPackagedCore(Resources.LoadAll<TextAsset>("Modding/Core/Content"));
			report.Merge(coreContent.Report);
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(LoadedPacks);
			report.Merge(content.Report);
			RegisterCoreCatalog(report);
			List<DifficultyDefinition> difficulties = new List<DifficultyDefinition>(coreContent.Difficulties);
			difficulties.AddRange(content.Difficulties);
			List<RuleProfileDefinition> ruleProfiles = new List<RuleProfileDefinition>(coreContent.RuleProfiles);
			ruleProfiles.AddRange(content.RuleProfiles);
			AssetPatches = content.AssetPatches;
			EnemyDefinitions = content.Enemies;
			ClothingDefinitions = content.Clothing;
			PlayerAttachmentDefinitions = content.PlayerAttachments;
			WeaponDefinitions = content.Weapons;
			UsableDefinitions = content.Usables;
			StageDefinitions = content.Stages;
			ChallengeDefinitions = content.Challenges;
			DifficultyDefinitions = difficulties;
			RuleProfileDefinitions = ruleProfiles;
			PlayerAnimationDefinitions = content.PlayerAnimations;
			List<NormalizedEnemyAnimationDefinition> enemyAnimations = new List<NormalizedEnemyAnimationDefinition>(coreContent.EnemyAnimations);
			enemyAnimations.AddRange(content.EnemyAnimations);
			EnemyAnimationDefinitions = enemyAnimations;
			CoreWeaponDefinitions = coreContent.CoreWeapons;
			CoreEnemyDefinitions = coreContent.CoreEnemies;
			CoreItemDefinitions = coreContent.CoreItems;
			CoreStageDefinitions = coreContent.CoreStages;
			CoreClothingDefinitions = coreContent.CoreClothing;
			CoreChallengeDefinitions = coreContent.CoreChallenges;
			ValidateCoreWeapons(coreContent.CoreWeapons, report);
			ValidateCoreEnemies(coreContent.CoreEnemies, report);
			ValidateCoreItems(coreContent.CoreItems, report);
			ValidateCoreStages(coreContent.CoreStages, report);
			ValidateCoreClothing(coreContent.CoreClothing, report);
			ValidateCoreChallenges(coreContent.CoreChallenges, report);
			DifficultyRegistry.Initialize(difficulties, report);
			NormalizedPlayerAnimationRegistry.Initialize(content.PlayerAnimations, report);
			NormalizedEnemyAnimationRegistry.Initialize(enemyAnimations, report);
			ValidateCoreEnemyAnimationReferences(coreContent.CoreEnemies, report);
			ResolvePlayerAnimationReferences(content.Enemies, report);
			ResolveEnemyAnimationReferences(content.Enemies, report);
			RegisterExternalClothing(content.Clothing, report);
			RegisterExternalWeapons(content.Weapons, report);
			RegisterExternalUsables(content.Usables, report);
			RegisterExternalEnemies(content.Enemies, report);
			RegisterExternalStages(content.Stages, report);
			StageScriptDefinitions = ValidateStageScripts(content.StageScripts, content.Stages, report);
			RegisterExternalChallenges(content.Challenges, report);
			RegisterExternalDifficulties(difficulties, report);
			RegisterExternalRuleProfiles(ruleProfiles, report);
			List<RuleProfileDefinition> validRuleProfiles = new List<RuleProfileDefinition>();
			foreach (RuleProfileDefinition profile in ruleProfiles)
				if (Registry.TryGet(profile.Id, out ContentRegistration registration) && registration.Category == ContentCategory.Rule)
					validRuleProfiles.Add(profile);
			RuleProfileRegistry.Initialize(validRuleProfiles);
			PackStatuses = MarkContentFailures(discoveryStatuses, report);
			LastReport = report;

			foreach (ValidationIssue issue in report.Issues)
				if (issue.Severity == ValidationSeverity.Info) Debug.Log("[ModLoader] " + issue);

			Debug.Log("[ModLoader] Validation complete. Packs=" + LoadedPacks.Count + ", registry entries=" + Registry.Count + ", valid=" + report.IsValid + ".");
		}

		private static void RegisterCoreCatalog(ValidationReport io_report)
		{
			foreach (CoreContentCatalogEntry entry in CoreContentCatalog)
				Registry.Register(new ContentRegistration(entry.Id, entry.Category, "core", "core/catalog.json"), io_report);
		}

		private static IReadOnlyList<ModPackStatus> MarkContentFailures(List<ModPackStatus> i_statuses, ValidationReport i_report)
		{
			List<ModPackStatus> result = new List<ModPackStatus>();
			foreach (ModPackStatus status in i_statuses)
			{
				bool contentFailed = false;
				if (status.State == ModPackState.Loaded && status.Pack != null && status.Id != "core")
					foreach (ValidationIssue issue in i_report.Issues)
						if (issue.Severity == ValidationSeverity.Error && !string.IsNullOrEmpty(issue.Source) &&
							issue.Source.StartsWith(status.RootPath, System.StringComparison.OrdinalIgnoreCase)) { contentFailed = true; break; }
				result.Add(contentFailed
					? new ModPackStatus(status.Pack, ModPackState.Invalid, "One or more content files failed validation. Errors are shown as top-left HUD alerts in game.")
					: status);
			}
			result.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
			return result;
		}

		private static void ResolvePlayerAnimationReferences(IEnumerable<EnemyDefinition> i_enemies, ValidationReport io_report)
		{
			foreach (EnemyDefinition enemy in i_enemies ?? new EnemyDefinition[0])
			{
				foreach (EnemyBehaviorModuleDefinition module in enemy.Behavior?.Modules ?? new List<EnemyBehaviorModuleDefinition>())
				{
					ResolvePlayerAnimationReference(module.PlayerAnimationReference, i_clip => module.PlayerAnimation = i_clip,
						enemy.Source, io_report);
					foreach (EnemyFinisherPhaseDefinition phase in module.Phases ?? new List<EnemyFinisherPhaseDefinition>())
						ResolvePlayerAnimationReference(phase.PlayerAnimationReference, i_clip => phase.PlayerAnimation = i_clip,
							enemy.Source, io_report);
				}
			}
		}

		private static void ResolvePlayerAnimationReference(string i_reference, System.Action<EnemyAnimationClipDefinition> i_assign,
			string i_source, ValidationReport io_report)
		{
			if (string.IsNullOrWhiteSpace(i_reference)) return;
			if (!ContentId.TryParse(i_reference, out ContentId id)
				|| !NormalizedPlayerAnimationRegistry.TryGet(id, out NormalizedPlayerAnimationDefinition animation))
			{
				io_report.Add(ValidationSeverity.Error, "enemy.player-animation-ref-missing", "Unknown normalized player animation: " + i_reference, i_source);
				return;
			}
			i_assign(animation.CreateFinisherClip(io_report));
		}

		private static void ResolveEnemyAnimationReferences(IEnumerable<EnemyDefinition> i_enemies, ValidationReport io_report)
		{
			foreach (EnemyDefinition enemy in i_enemies ?? new EnemyDefinition[0])
			{
				HashSet<string> bones = new HashSet<string>(System.StringComparer.Ordinal);
				foreach (EnemyBoneDefinition bone in enemy.Visual?.Bones ?? new List<EnemyBoneDefinition>()) bones.Add(bone.Id);
				HashSet<string> regions = new HashSet<string>(System.StringComparer.Ordinal);
				if (enemy.Visual?.Regions != null) foreach (string region in enemy.Visual.Regions.Keys) regions.Add(region);
				foreach (KeyValuePair<string, string> pair in enemy.AnimationReferences)
				{
					if (!ContentId.TryParse(pair.Value, out ContentId id) || !NormalizedEnemyAnimationRegistry.TryGet(id, out NormalizedEnemyAnimationDefinition animation))
					{
						io_report.Add(ValidationSeverity.Error, "enemy.animation-ref-missing", "Unknown normalized enemy animation: " + pair.Value, enemy.Source);
						continue;
					}
					if (animation.Enemy != enemy.Id)
					{
						io_report.Add(ValidationSeverity.Error, "enemy.animation-ref-owner", "Normalized animation " + animation.Id + " targets " + animation.Enemy + " instead of " + enemy.Id + ".", enemy.Source);
						continue;
					}
					if (enemy.Animation.Clips.ContainsKey(pair.Key))
					{
						io_report.Add(ValidationSeverity.Error, "enemy.animation-ref-duplicate", "Animation clip is defined inline and by reference: " + pair.Key, enemy.Source);
						continue;
					}
					int issueCount = io_report.Issues.Count;
					EnemyAnimationClipDefinition clip = animation.CreateClip(bones, regions, io_report);
					if (io_report.Issues.Count == issueCount) enemy.Animation.Clips.Add(pair.Key, clip);
				}
			}
		}

		private static void ValidateCoreEnemyAnimationReferences(IEnumerable<CoreEnemyDefinition> i_enemies, ValidationReport io_report)
		{
			foreach (CoreEnemyDefinition enemy in i_enemies ?? new CoreEnemyDefinition[0])
				foreach (KeyValuePair<string, string> pair in enemy.AnimationReferences)
				{
					if (!ContentId.TryParse(pair.Value, out ContentId id)
						|| !NormalizedEnemyAnimationRegistry.TryGet(id, out NormalizedEnemyAnimationDefinition animation))
					{
						io_report.Add(ValidationSeverity.Error, "core-enemy.animation-ref-missing",
							"Unknown normalized Core enemy animation: " + pair.Value, enemy.Source);
						continue;
					}
					if (animation.Enemy != enemy.Id)
						io_report.Add(ValidationSeverity.Error, "core-enemy.animation-ref-owner",
							"Animation " + animation.Id + " targets " + animation.Enemy + " instead of " + enemy.Id + ".", enemy.Source);
				}
		}

		private static void RegisterExternalChallenges(IEnumerable<ChallengeDefinition> i_challenges, ValidationReport io_report)
		{
			HashSet<ContentId> enemies = new HashSet<ContentId>();
			foreach (ContentRegistration entry in Registry.GetByCategory(ContentCategory.Enemy)) enemies.Add(entry.Id);
			HashSet<ContentId> clothing = new HashSet<ContentId>();
			foreach (ContentRegistration entry in Registry.GetByCategory(ContentCategory.Clothing)) clothing.Add(entry.Id);
			HashSet<ContentId> stages = new HashSet<ContentId>();
			foreach (ContentRegistration entry in Registry.GetByCategory(ContentCategory.Stage)) stages.Add(entry.Id);
			HashSet<ContentId> items = new HashSet<ContentId>();
			foreach (ContentRegistration entry in Registry.GetByCategory(ContentCategory.Item)) items.Add(entry.Id);
			foreach (CoreContentCatalogEntry entry in CoreContentCatalog)
			{
				if (entry.Category == ContentCategory.Enemy) enemies.Add(entry.Id);
				else if (entry.Category == ContentCategory.Clothing) clothing.Add(entry.Id);
				else if (entry.Category == ContentCategory.Stage) stages.Add(entry.Id);
				else if (entry.Category == ContentCategory.Item) items.Add(entry.Id);
			}
			HashSet<int> runtimeIds = new HashSet<int>();
			foreach (ChallengeDefinition challenge in i_challenges)
			{
				bool valid = true;
				foreach (ContentId enemy in challenge.Enemies) if (!enemies.Contains(enemy)) { io_report.Add(ValidationSeverity.Error, "challenge.enemy-missing", "Unknown challenge enemy: " + enemy, challenge.Source); valid = false; }
				foreach (ContentId item in challenge.Items) if (!items.Contains(item)) { io_report.Add(ValidationSeverity.Error, "challenge.item-missing", "Unknown challenge item: " + item, challenge.Source); valid = false; }
				foreach (ChallengeObjectiveDefinition step in challenge.Steps)
				{
					foreach (ContentId enemy in step.Enemies) if (!enemies.Contains(enemy)) { io_report.Add(ValidationSeverity.Error, "challenge.step-enemy-missing", "Unknown challenge step enemy: " + enemy, challenge.Source); valid = false; }
					foreach (ContentId item in step.Items) if (!items.Contains(item)) { io_report.Add(ValidationSeverity.Error, "challenge.step-item-missing", "Unknown challenge step item: " + item, challenge.Source); valid = false; }
				}
				foreach (ContentId reward in challenge.Rewards) if (!clothing.Contains(reward)) { io_report.Add(ValidationSeverity.Error, "challenge.reward-missing", "Unknown challenge reward: " + reward, challenge.Source); valid = false; }
				foreach (ChallengeGrantDefinition reward in challenge.RewardItems) if (!items.Contains(reward.Id)) { io_report.Add(ValidationSeverity.Error, "challenge.reward-item-missing", "Unknown challenge item reward: " + reward.Id, challenge.Source); valid = false; }
				foreach (ChallengeGrantDefinition reward in challenge.RewardWeapons) if (!items.Contains(reward.Id)) { io_report.Add(ValidationSeverity.Error, "challenge.reward-weapon-missing", "Unknown challenge weapon reward: " + reward.Id, challenge.Source); valid = false; }
				foreach (ContentId reward in challenge.RewardContent) if (!Registry.TryGet(reward, out ContentRegistration _) && !enemies.Contains(reward) && !clothing.Contains(reward) && !stages.Contains(reward) && !items.Contains(reward)) { io_report.Add(ValidationSeverity.Error, "challenge.reward-content-missing", "Unknown content reward: " + reward, challenge.Source); valid = false; }
				if (challenge.Stage.HasValue && !stages.Contains(challenge.Stage.Value)) { io_report.Add(ValidationSeverity.Error, "challenge.stage-missing", "Unknown challenge stage: " + challenge.Stage.Value, challenge.Source); valid = false; }
				if (challenge.Extends.HasValue && (!Registry.TryGet(challenge.Extends.Value, out ContentRegistration inherited)
					|| inherited.Category != ContentCategory.Challenge)) { io_report.Add(ValidationSeverity.Error, "challenge.extends-missing", "Unknown inherited challenge: " + challenge.Extends.Value, challenge.Source); valid = false; }
				int runtimeId = GetChallengeRuntimeId(challenge.Id);
				if (!runtimeIds.Add(runtimeId)) { io_report.Add(ValidationSeverity.Error, "challenge.id-collision", "External challenge runtime ID collision for " + challenge.Id, challenge.Source); valid = false; }
				if (valid) Registry.Register(new ContentRegistration(challenge.Id, ContentCategory.Challenge, challenge.PackId, challenge.Source), io_report);
			}
		}

		private static void RegisterExternalDifficulties(IEnumerable<DifficultyDefinition> i_difficulties, ValidationReport io_report)
		{
			foreach (DifficultyDefinition difficulty in i_difficulties)
				Registry.Register(new ContentRegistration(difficulty.Id, ContentCategory.Rule, difficulty.PackId, difficulty.Source), io_report);
		}

		private static void RegisterExternalRuleProfiles(IEnumerable<RuleProfileDefinition> i_profiles, ValidationReport io_report)
		{
			HashSet<ContentId> knownItems = new HashSet<ContentId>();
			foreach (CoreContentCatalogEntry entry in CoreContentCatalog)
				if (entry.Category == ContentCategory.Item) knownItems.Add(entry.Id);
			foreach (ContentRegistration entry in Registry.GetByCategory(ContentCategory.Item)) knownItems.Add(entry.Id);
			foreach (RuleProfileDefinition profile in i_profiles)
			{
				bool valid = true;
				foreach (WeaponProgressionRule progression in profile.WeaponProgressions)
				{
					if (!knownItems.Contains(progression.StarterWeapon)) { io_report.Add(ValidationSeverity.Error, "rule.starter-missing", "Unknown starter weapon: " + progression.StarterWeapon, profile.Source); valid = false; }
					foreach (ContentId weapon in progression.WeaponPool)
						if (!knownItems.Contains(weapon)) { io_report.Add(ValidationSeverity.Error, "rule.weapon-missing", "Unknown progression weapon: " + weapon, profile.Source); valid = false; }
				}
				if (valid)
				{
					Registry.Register(new ContentRegistration(profile.Id, ContentCategory.Rule, profile.PackId, profile.Source), io_report);
				}
			}
		}

		private static void ValidateCoreWeapons(IEnumerable<CoreWeaponDefinition> i_weapons, ValidationReport io_report)
		{
			Dictionary<ContentId, CoreContentCatalogEntry> catalog = new Dictionary<ContentId, CoreContentCatalogEntry>();
			foreach (CoreContentCatalogEntry entry in CoreContentCatalog)
				if (entry.Category == ContentCategory.Item) catalog[entry.Id] = entry;
			HashSet<ContentId> seen = new HashSet<ContentId>();
			foreach (CoreWeaponDefinition weapon in i_weapons)
			{
				if (!seen.Add(weapon.Id)) io_report.Add(ValidationSeverity.Error, "core-weapon.duplicate", "Duplicate packaged Core weapon definition: " + weapon.Id, weapon.Source);
				if (!catalog.TryGetValue(weapon.Id, out CoreContentCatalogEntry entry)) io_report.Add(ValidationSeverity.Error, "core-weapon.catalog-missing", "Core weapon is absent from the legacy adapter catalog: " + weapon.Id, weapon.Source);
				else if (!string.Equals(entry.LegacyName, weapon.LegacyName, System.StringComparison.Ordinal)) io_report.Add(ValidationSeverity.Error, "core-weapon.legacy-name", "Core weapon legacyName does not match the adapter catalog: " + weapon.Id, weapon.Source);
			}
		}

		private static void ValidateCoreEnemies(IEnumerable<CoreEnemyDefinition> i_enemies, ValidationReport io_report)
		{
			Dictionary<ContentId, CoreContentCatalogEntry> catalog = new Dictionary<ContentId, CoreContentCatalogEntry>();
			foreach (CoreContentCatalogEntry entry in CoreContentCatalog)
				if (entry.Category == ContentCategory.Enemy) catalog[entry.Id] = entry;
			HashSet<ContentId> seen = new HashSet<ContentId>();
			HashSet<int> legacyIds = new HashSet<int>();
			foreach (CoreEnemyDefinition enemy in i_enemies)
			{
				if (!seen.Add(enemy.Id)) io_report.Add(ValidationSeverity.Error, "core-enemy.duplicate", "Duplicate packaged Core enemy definition: " + enemy.Id, enemy.Source);
				if (!legacyIds.Add(enemy.LegacyId)) io_report.Add(ValidationSeverity.Error, "core-enemy.legacy-duplicate", "Duplicate Core enemy legacyId: " + enemy.LegacyId, enemy.Source);
				if (!catalog.TryGetValue(enemy.Id, out CoreContentCatalogEntry entry)) io_report.Add(ValidationSeverity.Error, "core-enemy.catalog-missing", "Core enemy is absent from the legacy adapter catalog: " + enemy.Id, enemy.Source);
				else if (entry.LegacyId != enemy.LegacyId) io_report.Add(ValidationSeverity.Error, "core-enemy.legacy-id", "Core enemy legacyId does not match the adapter catalog: " + enemy.Id, enemy.Source);
			}
		}

		private static void ValidateCoreItems(IEnumerable<CoreItemDefinition> i_items, ValidationReport io_report)
		{
			Dictionary<ContentId, CoreContentCatalogEntry> catalog = new Dictionary<ContentId, CoreContentCatalogEntry>();
			foreach (CoreContentCatalogEntry entry in CoreContentCatalog)
				if (entry.Category == ContentCategory.Item) catalog[entry.Id] = entry;
			HashSet<ContentId> seen = new HashSet<ContentId>();
			foreach (CoreItemDefinition item in i_items)
			{
				if (!seen.Add(item.Id)) io_report.Add(ValidationSeverity.Error, "core-item.duplicate", "Duplicate packaged Core item definition: " + item.Id, item.Source);
				if (!catalog.TryGetValue(item.Id, out CoreContentCatalogEntry entry)) io_report.Add(ValidationSeverity.Error, "core-item.catalog-missing", "Core item is absent from the legacy adapter catalog: " + item.Id, item.Source);
				else if (!string.Equals(entry.LegacyName, item.LegacyName, System.StringComparison.Ordinal)) io_report.Add(ValidationSeverity.Error, "core-item.legacy-name", "Core item legacyName does not match the adapter catalog: " + item.Id, item.Source);
			}
		}

		private static void ValidateCoreStages(IEnumerable<CoreStageDefinition> i_stages, ValidationReport io_report)
		{
			Dictionary<ContentId, CoreContentCatalogEntry> catalog = new Dictionary<ContentId, CoreContentCatalogEntry>();
			foreach (CoreContentCatalogEntry entry in CoreContentCatalog)
				if (entry.Category == ContentCategory.Stage) catalog[entry.Id] = entry;
			HashSet<ContentId> seen = new HashSet<ContentId>();
			HashSet<int> legacyIds = new HashSet<int>();
			foreach (CoreStageDefinition stage in i_stages)
			{
				if (!seen.Add(stage.Id)) io_report.Add(ValidationSeverity.Error, "core-stage.duplicate", "Duplicate packaged Core stage definition: " + stage.Id, stage.Source);
				if (!legacyIds.Add(stage.LegacyId)) io_report.Add(ValidationSeverity.Error, "core-stage.legacy-duplicate", "Duplicate Core stage legacyId: " + stage.LegacyId, stage.Source);
				if (!catalog.TryGetValue(stage.Id, out CoreContentCatalogEntry entry)) io_report.Add(ValidationSeverity.Error, "core-stage.catalog-missing", "Core stage is absent from the legacy adapter catalog: " + stage.Id, stage.Source);
				else if (entry.LegacyId != stage.LegacyId) io_report.Add(ValidationSeverity.Error, "core-stage.legacy-id", "Core stage legacyId does not match the adapter catalog: " + stage.Id, stage.Source);
			}
		}

		private static void ValidateCoreClothing(IEnumerable<CoreClothingDefinition> i_clothing, ValidationReport io_report)
		{
			Dictionary<ContentId, CoreContentCatalogEntry> catalog = new Dictionary<ContentId, CoreContentCatalogEntry>();
			foreach (CoreContentCatalogEntry entry in CoreContentCatalog)
				if (entry.Category == ContentCategory.Clothing) catalog[entry.Id] = entry;
			HashSet<ContentId> defined = new HashSet<ContentId>();
			foreach (CoreClothingDefinition clothing in i_clothing)
			{
				defined.Add(clothing.Id);
				if (!catalog.TryGetValue(clothing.Id, out CoreContentCatalogEntry entry)) io_report.Add(ValidationSeverity.Error, "core-clothing.catalog-missing", "Core clothing is absent from the legacy adapter catalog: " + clothing.Id, clothing.Source);
				else if (entry.LegacyId != clothing.LegacyId) io_report.Add(ValidationSeverity.Error, "core-clothing.legacy-id", "Core clothing legacyId does not match the adapter catalog: " + clothing.Id, clothing.Source);
				if (clothing.HasLoosePresentation)
				{
					if (Resources.Load<Texture2D>(clothing.Icon.Resource) == null)
						io_report.Add(ValidationSeverity.Error, "core-clothing.icon-missing", "Migrated Core clothing icon is not loadable: " + clothing.Icon.Resource, clothing.Source);
					foreach (CoreClothingPieceRecord piece in clothing.Pieces)
						if (piece?.Sprite == null || Resources.Load<Texture2D>(piece.Sprite.Resource) == null)
							io_report.Add(ValidationSeverity.Error, "core-clothing.sprite-missing", "Migrated Core clothing piece is not loadable: " + (piece?.Sprite?.Resource ?? "<null>"), clothing.Source);
				}
			}
			foreach (CoreContentCatalogEntry entry in catalog.Values)
				if (!defined.Contains(entry.Id)) io_report.Add(ValidationSeverity.Error, "core-clothing.definition-missing", "Core clothing has no packaged definition: " + entry.Id, "core/content/core-clothing.json");
		}

		private static void ValidateCoreChallenges(IEnumerable<CoreChallengeDefinition> i_challenges, ValidationReport io_report)
		{
			Dictionary<ContentId, CoreContentCatalogEntry> catalog = new Dictionary<ContentId, CoreContentCatalogEntry>();
			foreach (CoreContentCatalogEntry entry in CoreContentCatalog)
				if (entry.Category == ContentCategory.Challenge) catalog[entry.Id] = entry;
			HashSet<ContentId> defined = new HashSet<ContentId>();
			foreach (CoreChallengeDefinition challenge in i_challenges)
			{
				defined.Add(challenge.Id);
				if (!catalog.TryGetValue(challenge.Id, out CoreContentCatalogEntry entry)) io_report.Add(ValidationSeverity.Error, "core-challenge.catalog-missing", "Core challenge is absent from the legacy adapter catalog: " + challenge.Id, challenge.Source);
				else if (entry.LegacyId != challenge.LegacyId) io_report.Add(ValidationSeverity.Error, "core-challenge.legacy-id", "Core challenge legacyId does not match the adapter catalog: " + challenge.Id, challenge.Source);
			}
			foreach (CoreContentCatalogEntry entry in catalog.Values)
				if (!defined.Contains(entry.Id)) io_report.Add(ValidationSeverity.Error, "core-challenge.definition-missing", "Core challenge has no packaged definition: " + entry.Id, "core/content/core-challenges.json");
		}

		public static int GetChallengeRuntimeId(ContentId i_id)
		{
			unchecked { uint hash = 2166136261; foreach (char value in i_id.ToString()) { hash ^= value; hash *= 16777619; } return -100000 - (int)(hash % 2000000000u); }
		}

		private static void RegisterExternalClothing(IEnumerable<ClothingDefinition> i_clothing, ValidationReport io_report)
		{
			HashSet<ContentId> coreClothing = new HashSet<ContentId>();
			foreach (CoreContentCatalogEntry entry in CoreContentCatalog)
				if (entry.Category == ContentCategory.Clothing) coreClothing.Add(entry.Id);

			foreach (ClothingDefinition clothing in i_clothing)
			{
				if (clothing.Extends.HasValue && !coreClothing.Contains(clothing.Extends.Value))
				{
					io_report.Add(ValidationSeverity.Error, "clothing.extends-missing", "Clothing extends unknown Core clothing: " + clothing.Extends.Value, clothing.Source);
					continue;
				}
				Registry.Register(new ContentRegistration(clothing.Id, ContentCategory.Clothing, clothing.PackId, clothing.Source), io_report);
			}
		}

		private static void RegisterExternalEnemies(IEnumerable<EnemyDefinition> i_enemies, ValidationReport io_report)
		{
			HashSet<ContentId> coreEnemies = new HashSet<ContentId>();
			HashSet<ContentId> knownItems = new HashSet<ContentId>();
			HashSet<ContentId> knownEnemies = new HashSet<ContentId>();
			foreach (CoreContentCatalogEntry entry in CoreContentCatalog)
				if (entry.Category == ContentCategory.Enemy) { coreEnemies.Add(entry.Id); knownEnemies.Add(entry.Id); }
				else if (entry.Category == ContentCategory.Item) knownItems.Add(entry.Id);
			foreach (ContentRegistration item in Registry.GetByCategory(ContentCategory.Item)) knownItems.Add(item.Id);
			foreach (EnemyDefinition definition in i_enemies) knownEnemies.Add(definition.Id);

			foreach (EnemyDefinition enemy in i_enemies)
			{
				if (enemy.Extends.HasValue && !coreEnemies.Contains(enemy.Extends.Value))
				{
					io_report.Add(ValidationSeverity.Error, "enemy.extends-missing", "Enemy extends an unknown Core enemy: " + enemy.Extends.Value, enemy.Source);
					continue;
				}
				bool dropsValid = true;
				foreach (string drop in enemy.Drops.Items)
				{
					ContentId dropId = ContentId.Parse(drop);
					if (knownItems.Contains(dropId)) continue;
					io_report.Add(ValidationSeverity.Error, "enemy.drop-missing", "Enemy references an unknown drop item: " + dropId, enemy.Source);
					dropsValid = false;
				}
				foreach (EnemyBehaviorModuleDefinition module in enemy.Behavior.Modules)
					if (module.Type == "spawnOnDeath" && (!ContentId.TryParse(module.Enemy, out ContentId spawnId) || !knownEnemies.Contains(spawnId)))
					{
						io_report.Add(ValidationSeverity.Error, "enemy.spawn-on-death-missing", "Enemy references an unknown spawnOnDeath enemy: " + module.Enemy, enemy.Source);
						dropsValid = false;
					}
				if (dropsValid) Registry.Register(new ContentRegistration(enemy.Id, ContentCategory.Enemy, enemy.PackId, enemy.Source), io_report);
			}
		}

		private static void RegisterExternalWeapons(IEnumerable<WeaponDefinition> i_weapons, ValidationReport io_report)
		{
			HashSet<ContentId> coreItems = new HashSet<ContentId>();
			foreach (CoreContentCatalogEntry entry in CoreContentCatalog)
				if (entry.Category == ContentCategory.Item) coreItems.Add(entry.Id);

			foreach (WeaponDefinition weapon in i_weapons)
			{
				if (weapon.Extends.HasValue && !coreItems.Contains(weapon.Extends.Value))
				{
					io_report.Add(ValidationSeverity.Error, "weapon.extends-missing", "Weapon extends an unknown Core item: " + weapon.Extends.Value, weapon.Source);
					continue;
				}
				Registry.Register(new ContentRegistration(weapon.Id, ContentCategory.Item, weapon.PackId, weapon.Source), io_report);
			}
		}

		private static void RegisterExternalUsables(IEnumerable<UsableDefinition> i_usables, ValidationReport io_report)
		{
			HashSet<ContentId> coreItems = new HashSet<ContentId>();
			foreach (CoreContentCatalogEntry entry in CoreContentCatalog)
				if (entry.Category == ContentCategory.Item) coreItems.Add(entry.Id);

			foreach (UsableDefinition usable in i_usables)
			{
				if (!coreItems.Contains(usable.Extends))
				{
					io_report.Add(ValidationSeverity.Error, "usable.extends-missing", "Usable extends an unknown Core item: " + usable.Extends, usable.Source);
					continue;
				}
				Registry.Register(new ContentRegistration(usable.Id, ContentCategory.Item, usable.PackId, usable.Source), io_report);
			}
		}

		private static void RegisterExternalStages(IEnumerable<StageDefinition> i_stages, ValidationReport io_report)
		{
			HashSet<ContentId> coreStages = new HashSet<ContentId>();
			HashSet<ContentId> knownEnemies = new HashSet<ContentId>();
			HashSet<ContentId> knownWeapons = new HashSet<ContentId>();
			HashSet<ContentId> knownItems = new HashSet<ContentId>();
			HashSet<ContentId> knownChallenges = new HashSet<ContentId>();
			foreach (CoreContentCatalogEntry entry in CoreContentCatalog)
			{
				if (entry.Category == ContentCategory.Stage) coreStages.Add(entry.Id);
				else if (entry.Category == ContentCategory.Enemy) knownEnemies.Add(entry.Id);
				else if (entry.Category == ContentCategory.Challenge) knownChallenges.Add(entry.Id);
				else if (entry.Category == ContentCategory.Item)
				{
					knownItems.Add(entry.Id);
					if (entry.Id.Path.StartsWith("item/weapon/", System.StringComparison.Ordinal)) knownWeapons.Add(entry.Id);
				}
			}
			foreach (ContentRegistration enemy in Registry.GetByCategory(ContentCategory.Enemy)) knownEnemies.Add(enemy.Id);
			foreach (ContentRegistration item in Registry.GetByCategory(ContentCategory.Item))
			{
				knownItems.Add(item.Id);
				if (item.Id.Path.StartsWith("item/weapon/", System.StringComparison.Ordinal)) knownWeapons.Add(item.Id);
			}
			foreach (ContentRegistration challenge in Registry.GetByCategory(ContentCategory.Challenge)) knownChallenges.Add(challenge.Id);

			foreach (StageDefinition stage in i_stages)
			{
				if (!stage.UsesRuntimeTemplate && !coreStages.Contains(stage.Extends))
				{
					io_report.Add(ValidationSeverity.Error, "stage.extends-missing", "Stage extends an unknown Core stage: " + stage.Extends, stage.Source);
					continue;
				}
				bool referencesValid = true;
				HashSet<string> tiledDoors = new HashSet<string>(System.StringComparer.Ordinal);
				foreach (TiledDoorDefinition door in stage.Layout.TiledLevel?.Doors ?? new TiledDoorDefinition[0])
					tiledDoors.Add(door.Point.Name);
				foreach (TiledCoreStageObjectDefinition adapter in stage.Layout.TiledLevel?.CoreStageObjects ?? new TiledCoreStageObjectDefinition[0])
					if (adapter.Kind == "door") tiledDoors.Add(adapter.Point.Name);
				foreach (StageSpawnerDefinition spawner in stage.Spawners)
				{
					foreach (ContentId enemyId in spawner.Enemies)
					{
						if (knownEnemies.Contains(enemyId)) continue;
						io_report.Add(ValidationSeverity.Error, "stage.enemy-missing", "Stage references an unknown enemy: " + enemyId, stage.Source);
						referencesValid = false;
					}
					foreach (string door in spawner.RequiredOpenDoors)
						if (!tiledDoors.Contains(door))
						{
							io_report.Add(ValidationSeverity.Error, "stage.spawner.open-door-missing", "Spawner references an unknown required-open Tiled door: " + door, stage.Source);
							referencesValid = false;
						}
					foreach (string door in spawner.RequiredClosedDoors)
						if (!tiledDoors.Contains(door))
						{
							io_report.Add(ValidationSeverity.Error, "stage.spawner.closed-door-missing", "Spawner references an unknown required-closed Tiled door: " + door, stage.Source);
							referencesValid = false;
						}
				}
				foreach (TiledWeaponCaseDefinition weaponCase in stage.Layout.TiledLevel?.WeaponCases ?? new TiledWeaponCaseDefinition[0])
				{
					if (knownWeapons.Contains(weaponCase.Weapon)) continue;
					io_report.Add(ValidationSeverity.Error, "stage.weapon-case-missing", "Stage weapon case references an unknown weapon: " + weaponCase.Weapon, stage.Source);
					referencesValid = false;
				}
				foreach (TiledPickupDefinition pickup in stage.Layout.TiledLevel?.Pickups ?? new TiledPickupDefinition[0])
				{
					if (knownItems.Contains(pickup.Item)) continue;
					io_report.Add(ValidationSeverity.Error, "stage.pickup-missing", "Stage pickup references an unknown item: " + pickup.Item, stage.Source);
					referencesValid = false;
				}
				foreach (TiledInteractionDefinition interaction in stage.Layout.TiledLevel?.Interactions ?? new TiledInteractionDefinition[0])
				{
					if (interaction.RequiredItem.HasValue && !knownItems.Contains(interaction.RequiredItem.Value))
					{
						io_report.Add(ValidationSeverity.Error, "stage.interaction-required-item-missing", "Stage interaction references an unknown required item: " + interaction.RequiredItem.Value, stage.Source);
						referencesValid = false;
					}
					if (interaction.GiveItem.HasValue && !knownItems.Contains(interaction.GiveItem.Value))
					{
						io_report.Add(ValidationSeverity.Error, "stage.interaction-give-item-missing", "Stage interaction references an unknown reward item: " + interaction.GiveItem.Value, stage.Source);
						referencesValid = false;
					}
					foreach (ContentId challenge in interaction.RequiredChallenges)
						if (!knownChallenges.Contains(challenge))
						{
							io_report.Add(ValidationSeverity.Error, "stage.interaction-challenge-missing", "Stage interaction references an unknown required challenge: " + challenge, stage.Source);
							referencesValid = false;
						}
				}
				if (referencesValid)
					Registry.Register(new ContentRegistration(stage.Id, ContentCategory.Stage, stage.PackId, stage.Source), io_report);
			}
		}

		private static IReadOnlyList<StageScriptDefinition> ValidateStageScripts(IEnumerable<StageScriptDefinition> i_scripts,
			IEnumerable<StageDefinition> i_stages, ValidationReport io_report)
		{
			Dictionary<ContentId, StageDefinition> stages = new Dictionary<ContentId, StageDefinition>();
			foreach (StageDefinition stage in i_stages) stages[stage.Id] = stage;
			HashSet<ContentId> ids = new HashSet<ContentId>();
			List<StageScriptDefinition> valid = new List<StageScriptDefinition>();
			foreach (StageScriptDefinition script in i_scripts ?? new StageScriptDefinition[0])
			{
				if (!ids.Add(script.Id))
				{
					io_report.Add(ValidationSeverity.Error, "stage-script.duplicate-id", "Duplicate stage script ID: " + script.Id, script.Source);
					continue;
				}
				if (!stages.TryGetValue(script.Stage, out StageDefinition targetStage))
				{
					io_report.Add(ValidationSeverity.Error, "stage-script.stage-missing", "Stage script targets an unknown external stage: " + script.Stage, script.Source);
					continue;
				}
				bool targetsValid = true;
				foreach (StageScriptSequenceDocument sequence in script.Sequences)
					if (sequence.Trigger == "player-enter" || sequence.Trigger == "player-exit")
					{
						bool found = false;
						foreach (TiledLevelObject trigger in targetStage.Layout?.TiledLevel?.ScriptTriggers ?? new TiledLevelObject[0])
							if (trigger.Name == sequence.ObjectId) { found = true; break; }
						if (!found)
						{
							io_report.Add(ValidationSeverity.Error, "stage-script.volume-missing",
								"Stage script references an unknown script-trigger rectangle: " + sequence.ObjectId, script.Source);
							targetsValid = false;
						}
					}
				if (!targetsValid) continue;
				valid.Add(script);
			}
			return valid;
		}
	}

}
