using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	public sealed class ModContentDiscoveryResult
	{
		public List<AssetPatchDefinition> AssetPatches { get; } = new List<AssetPatchDefinition>();
		public List<EnemyDefinition> Enemies { get; } = new List<EnemyDefinition>();
		public List<ClothingDefinition> Clothing { get; } = new List<ClothingDefinition>();
		public List<PlayerAttachmentDefinition> PlayerAttachments { get; } = new List<PlayerAttachmentDefinition>();
		public List<WeaponDefinition> Weapons { get; } = new List<WeaponDefinition>();
		public List<UsableDefinition> Usables { get; } = new List<UsableDefinition>();
		public List<StageDefinition> Stages { get; } = new List<StageDefinition>();
		public List<StageScriptDefinition> StageScripts { get; } = new List<StageScriptDefinition>();
		public List<ChallengeDefinition> Challenges { get; } = new List<ChallengeDefinition>();
		public List<DifficultyDefinition> Difficulties { get; } = new List<DifficultyDefinition>();
		public List<RuleProfileDefinition> RuleProfiles { get; } = new List<RuleProfileDefinition>();
		public List<NormalizedPlayerAnimationDefinition> PlayerAnimations { get; } = new List<NormalizedPlayerAnimationDefinition>();
		public List<NormalizedEnemyAnimationDefinition> EnemyAnimations { get; } = new List<NormalizedEnemyAnimationDefinition>();
		public List<CoreWeaponDefinition> CoreWeapons { get; } = new List<CoreWeaponDefinition>();
		public List<CoreEnemyDefinition> CoreEnemies { get; } = new List<CoreEnemyDefinition>();
		public List<CoreItemDefinition> CoreItems { get; } = new List<CoreItemDefinition>();
		public List<CoreStageDefinition> CoreStages { get; } = new List<CoreStageDefinition>();
		public List<CoreClothingDefinition> CoreClothing { get; } = new List<CoreClothingDefinition>();
		public List<CoreChallengeDefinition> CoreChallenges { get; } = new List<CoreChallengeDefinition>();
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class ModContentDiscovery
	{
		public static ModContentDiscoveryResult DiscoverPackagedCore(IEnumerable<TextAsset> i_definitions)
		{
			ModContentDiscoveryResult result = new ModContentDiscoveryResult();
			foreach (TextAsset asset in i_definitions ?? new TextAsset[0])
			{
				if (asset == null) continue;
				string source = "core/content/" + asset.name + ".json";
				try
				{
					JObject root = JObject.Parse(asset.text);
					string type = (string)root["type"];
					if (string.Equals(type, "difficulty", StringComparison.Ordinal))
					{
						DifficultyDefinitionLoadResult loaded = DifficultyDefinitionParser.Parse(asset.text, "core", source);
						result.Report.Merge(loaded.Report);
						if (loaded.Report.IsValid) result.Difficulties.Add(loaded.Definition);
					}
					else if (string.Equals(type, "ruleProfile", StringComparison.Ordinal))
					{
						RuleProfileLoadResult loaded = RuleProfileParser.Parse(asset.text, "core", source);
						result.Report.Merge(loaded.Report);
						if (loaded.Report.IsValid) result.RuleProfiles.Add(loaded.Definition);
					}
					else if (string.Equals(type, "coreWeapon", StringComparison.Ordinal))
					{
						CoreWeaponLoadResult loaded = CoreWeaponParser.Parse(asset.text, source);
						result.Report.Merge(loaded.Report);
						if (loaded.Report.IsValid) result.CoreWeapons.Add(loaded.Definition);
					}
					else if (string.Equals(type, "coreEnemy", StringComparison.Ordinal))
					{
						CoreEnemyLoadResult loaded = CoreEnemyParser.Parse(asset.text, source);
						result.Report.Merge(loaded.Report);
						if (loaded.Report.IsValid) result.CoreEnemies.Add(loaded.Definition);
					}
					else if (string.Equals(type, "coreItem", StringComparison.Ordinal))
					{
						CoreItemLoadResult loaded = CoreItemParser.Parse(asset.text, source);
						result.Report.Merge(loaded.Report);
						if (loaded.Report.IsValid) result.CoreItems.Add(loaded.Definition);
					}
					else if (string.Equals(type, "coreStage", StringComparison.Ordinal))
					{
						CoreStageLoadResult loaded = CoreStageParser.Parse(asset.text, source);
						result.Report.Merge(loaded.Report);
						if (loaded.Report.IsValid) result.CoreStages.Add(loaded.Definition);
					}
					else if (string.Equals(type, "coreClothingCatalog", StringComparison.Ordinal))
					{
						CoreClothingCatalogLoadResult loaded = CoreClothingCatalogParser.Parse(asset.text, source);
						result.Report.Merge(loaded.Report);
						if (loaded.Report.IsValid) result.CoreClothing.AddRange(loaded.Definitions);
					}
					else if (string.Equals(type, "coreChallengeCatalog", StringComparison.Ordinal))
					{
						CoreChallengeCatalogLoadResult loaded = CoreChallengeCatalogParser.Parse(asset.text, source);
						result.Report.Merge(loaded.Report);
						if (loaded.Report.IsValid) result.CoreChallenges.AddRange(loaded.Definitions);
					}
					else if (string.Equals(type, "enemyAnimation", StringComparison.Ordinal))
					{
						NormalizedEnemyAnimationLoadResult loaded = NormalizedEnemyAnimationParser.Parse(asset.text, "core", source);
						result.Report.Merge(loaded.Report);
						if (loaded.Report.IsValid) result.EnemyAnimations.Add(loaded.Definition);
					}
					else result.Report.Add(ValidationSeverity.Error, "core-content.type", "Unsupported packaged Core definition type: " + (type ?? "<null>"), source);
				}
				catch (JsonException exception)
				{
					result.Report.Add(ValidationSeverity.Error, "core-content.json", exception.Message, source);
				}
			}
			return result;
		}

		public static ModContentDiscoveryResult Discover(IEnumerable<ModPack> i_packs)
		{
			ModContentDiscoveryResult result = new ModContentDiscoveryResult();
			if (i_packs == null) return result;
			foreach (ModPack pack in i_packs)
			{
				if (pack?.Manifest == null || pack.Manifest.Id == "core") continue;
				int definitionCount = 0;
				bool definitionLimitReached = false;
				foreach (string contentRoot in pack.Manifest.ContentRoots)
				{
					if (definitionLimitReached) break;
					string root = Path.GetFullPath(Path.Combine(pack.RootPath, contentRoot));
					if (!AssetPatchDiscovery.IsInside(root, pack.RootPath) || !Directory.Exists(root))
					{
						result.Report.Add(ValidationSeverity.Warning, "content.root", "Content root does not exist or escapes its pack.", root);
						continue;
					}
					string[] files;
					try
					{
						files = Directory.GetFiles(root, "*.json", SearchOption.AllDirectories);
						Array.Sort(files, StringComparer.Ordinal);
					}
					catch (Exception exception)
					{
						result.Report.Add(ValidationSeverity.Error, "content.read-root", exception.Message, root);
						continue;
					}
					foreach (string file in files)
					{
						if (++definitionCount > ModFileLimits.MaximumDefinitionsPerPack)
						{
							result.Report.Add(ValidationSeverity.Error, "content.definition-limit",
								"Pack contains more than " + ModFileLimits.MaximumDefinitionsPerPack + " JSON content files.", pack.RootPath);
							definitionLimitReached = true;
							break;
						}
						DiscoverFile(file, pack, result);
					}
				}
			}
			return result;
		}

		private static void DiscoverFile(string i_file, ModPack i_pack, ModContentDiscoveryResult io_result)
		{
			try
			{
				long bytes = new FileInfo(i_file).Length;
				if (bytes <= 0 || bytes > ModFileLimits.MaximumContentDefinitionBytes)
				{
					io_result.Report.Add(ValidationSeverity.Error, "content.definition-size",
						"Content JSON must be between 1 byte and 1 MiB.", i_file);
					return;
				}
				string json = File.ReadAllText(i_file);
				JObject root = JObject.Parse(json);
				string type = (string)root["type"];
				if (string.Equals(type, "assetPatch", StringComparison.Ordinal))
				{
					AssetPatchLoadResult loaded = AssetPatchParser.Parse(json, i_pack.Manifest.Id, i_file);
					io_result.Report.Merge(loaded.Report);
					if (loaded.Report.IsValid) io_result.AssetPatches.Add(loaded.Definition);
				}
				else if (string.Equals(type, "enemy", StringComparison.Ordinal))
				{
					EnemyDefinitionLoadResult loaded = EnemyDefinitionParser.Parse(json, i_pack.Manifest.Id, i_file);
					io_result.Report.Merge(loaded.Report);
					if (loaded.Report.IsValid) io_result.Enemies.Add(loaded.Definition);
				}
				else if (string.Equals(type, "clothing", StringComparison.Ordinal))
				{
					ClothingDefinitionLoadResult loaded = ClothingDefinitionParser.Parse(json, i_pack.Manifest.Id, i_file);
					io_result.Report.Merge(loaded.Report);
					if (loaded.Report.IsValid) io_result.Clothing.Add(loaded.Definition);
				}
				else if (string.Equals(type, "playerAttachment", StringComparison.Ordinal))
				{
					PlayerAttachmentLoadResult loaded = PlayerAttachmentParser.Parse(json, i_pack.Manifest.Id, i_file);
					io_result.Report.Merge(loaded.Report);
					if (loaded.Report.IsValid) io_result.PlayerAttachments.Add(loaded.Definition);
				}
				else if (string.Equals(type, "weapon", StringComparison.Ordinal))
				{
					WeaponDefinitionLoadResult loaded = WeaponDefinitionParser.Parse(json, i_pack.Manifest.Id, i_file);
					io_result.Report.Merge(loaded.Report);
					if (loaded.Report.IsValid)
					{
						int issueCount = io_result.Report.Issues.Count;
						ValidateWeaponBehaviorFiles(loaded.Definition, i_pack.RootPath, io_result.Report);
						if (io_result.Report.Issues.Count == issueCount) io_result.Weapons.Add(loaded.Definition);
					}
				}
				else if (string.Equals(type, "usable", StringComparison.Ordinal))
				{
					UsableDefinitionLoadResult loaded = UsableDefinitionParser.Parse(json, i_pack.Manifest.Id, i_file);
					io_result.Report.Merge(loaded.Report);
					if (loaded.Report.IsValid) io_result.Usables.Add(loaded.Definition);
				}
				else if (string.Equals(type, "stage", StringComparison.Ordinal))
				{
					StageDefinitionLoadResult loaded = StageDefinitionParser.Parse(json, i_pack.Manifest.Id, i_file);
					io_result.Report.Merge(loaded.Report);
					if (loaded.Report.IsValid)
					{
						loaded.Definition.AssetRoot = i_pack.RootPath;
						ValidateStageAudioFiles(loaded.Definition, io_result.Report);
					}
					if (loaded.Report.IsValid && loaded.Definition.Layout.Type == "tiledJson")
					{
						TiledLevelLoadResult level = TiledLevelParser.Load(i_pack.RootPath,
							loaded.Definition.Layout.Path, i_file);
						io_result.Report.Merge(level.Report);
						bool matchesEncounter = level.Report.IsValid;
						if (level.Report.IsValid)
						{
							HashSet<string> points = new HashSet<string>(StringComparer.Ordinal);
							foreach (TiledLevelObject point in level.Definition.EnemySpawners) points.Add(point.Name);
							foreach (StageSpawnerDefinition spawner in loaded.Definition.Spawners)
								if (!points.Remove(spawner.Id))
								{
									io_result.Report.Add(ValidationSeverity.Error, "stage.layout.spawner-missing",
										"Tiled map has no enemy-spawner point named " + spawner.Id + ".", i_file);
									matchesEncounter = false;
								}
							foreach (string unused in points)
							{
								io_result.Report.Add(ValidationSeverity.Error, "stage.layout.spawner-unused",
									"Tiled enemy-spawner point has no matching stage spawner: " + unused + ".", i_file);
								matchesEncounter = false;
							}
						}
						if (matchesEncounter) loaded.Definition.Layout.TiledLevel = level.Definition;
					}
					if (loaded.Report.IsValid && (loaded.Definition.Layout.Type != "tiledJson"
						|| loaded.Definition.Layout.TiledLevel != null)) io_result.Stages.Add(loaded.Definition);
				}
				else if (string.Equals(type, "stageScript", StringComparison.Ordinal))
				{
					StageScriptLoadResult loaded = StageScriptParser.Parse(json, i_pack.Manifest.Id, i_file);
					io_result.Report.Merge(loaded.Report);
					if (loaded.Report.IsValid) io_result.StageScripts.Add(loaded.Definition);
				}
				else if (string.Equals(type, "challenge", StringComparison.Ordinal))
				{
					ChallengeDefinitionLoadResult loaded = ChallengeDefinitionParser.Parse(json, i_pack.Manifest.Id, i_file);
					io_result.Report.Merge(loaded.Report);
					if (loaded.Report.IsValid) io_result.Challenges.Add(loaded.Definition);
				}
				else if (string.Equals(type, "difficulty", StringComparison.Ordinal))
				{
					DifficultyDefinitionLoadResult loaded = DifficultyDefinitionParser.Parse(json, i_pack.Manifest.Id, i_file);
					io_result.Report.Merge(loaded.Report);
					if (loaded.Report.IsValid) io_result.Difficulties.Add(loaded.Definition);
				}
				else if (string.Equals(type, "ruleProfile", StringComparison.Ordinal))
				{
					RuleProfileLoadResult loaded = RuleProfileParser.Parse(json, i_pack.Manifest.Id, i_file);
					io_result.Report.Merge(loaded.Report);
					if (loaded.Report.IsValid) io_result.RuleProfiles.Add(loaded.Definition);
				}
				else if (string.Equals(type, "playerAnimation", StringComparison.Ordinal))
				{
					NormalizedPlayerAnimationLoadResult loaded = NormalizedPlayerAnimationParser.Parse(json, i_pack.Manifest.Id, i_file, i_pack.RootPath);
					io_result.Report.Merge(loaded.Report);
					if (loaded.Report.IsValid) io_result.PlayerAnimations.Add(loaded.Definition);
				}
				else if (string.Equals(type, "enemyAnimation", StringComparison.Ordinal))
				{
					NormalizedEnemyAnimationLoadResult loaded = NormalizedEnemyAnimationParser.Parse(json, i_pack.Manifest.Id, i_file, i_pack.RootPath);
					io_result.Report.Merge(loaded.Report);
					if (loaded.Report.IsValid) io_result.EnemyAnimations.Add(loaded.Definition);
				}
				else
				{
					io_result.Report.Add(ValidationSeverity.Error, "content.type", "Unsupported or missing content definition type: " + (type ?? "<null>"), i_file);
				}
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is JsonException)
			{
				io_result.Report.Add(ValidationSeverity.Error, "content.read-definition", exception.Message, i_file);
			}
		}

		private static void ValidateStageAudioFiles(StageDefinition i_stage, ValidationReport io_report)
		{
			if (i_stage == null || i_stage.Audio == null) return;
			ValidateStageAudioFile(i_stage.AssetRoot, i_stage.Audio.Ambience, "ambience", i_stage.Source, io_report);
			ValidateStageAudioFile(i_stage.AssetRoot, i_stage.Audio.EntryMusic, "entry-music", i_stage.Source, io_report);
			ValidateStageAudioFile(i_stage.AssetRoot, i_stage.Audio.WaveMusic, "wave-music", i_stage.Source, io_report);
			ValidateStageAudioFile(i_stage.AssetRoot, i_stage.Audio.WaveComplete, "wave-complete", i_stage.Source, io_report);
		}

		private static void ValidateWeaponBehaviorFiles(WeaponDefinition i_weapon, string i_root, ValidationReport io_report)
		{
			if (i_weapon == null || i_weapon.Behavior == null) return;
			foreach (string path in i_weapon.Behavior.ShootSounds ?? new List<string>())
				ValidatePackAsset(i_root, path, 64L * 1024L * 1024L, "weapon.behavior.shoot-sound-file", i_weapon.Source, io_report);
			foreach (string path in i_weapon.Behavior.ReloadSounds ?? new List<string>())
				ValidatePackAsset(i_root, path, 64L * 1024L * 1024L, "weapon.behavior.reload-sound-file", i_weapon.Source, io_report);
			foreach (string path in i_weapon.Behavior.MuzzleFlashSprites ?? new List<string>())
				ValidatePackAsset(i_root, path, 16L * 1024L * 1024L, "weapon.behavior.muzzle-flash-file", i_weapon.Source, io_report);
			if (!string.IsNullOrWhiteSpace(i_weapon.Behavior.CasingSprite))
				ValidatePackAsset(i_root, i_weapon.Behavior.CasingSprite, 16L * 1024L * 1024L, "weapon.behavior.casing-file", i_weapon.Source, io_report);
			WeaponProjectileDefinition projectile = i_weapon.Behavior.Projectile;
			if (projectile != null && !string.IsNullOrWhiteSpace(projectile.Sprite))
				ValidatePackAsset(i_root, projectile.Sprite, 16L * 1024L * 1024L, "weapon.projectile.sprite-file", i_weapon.Source, io_report);
			foreach (string path in projectile?.ImpactSprites ?? new List<string>())
				ValidatePackAsset(i_root, path, 16L * 1024L * 1024L, "weapon.projectile.impact-file", i_weapon.Source, io_report);
			WeaponAnimationDefinition animation = i_weapon.Behavior.Animations;
			if (animation == null) return;
			foreach (WeaponSpriteAnimationClipDefinition clip in (animation.Clips ?? new Dictionary<string, WeaponSpriteAnimationClipDefinition>()).Values)
				foreach (WeaponSpriteAnimationFrameDefinition frame in clip?.Frames ?? new List<WeaponSpriteAnimationFrameDefinition>())
					foreach (string path in (frame?.Sprites ?? new Dictionary<string, string>()).Values)
						ValidatePackAsset(i_root, path, 16L * 1024L * 1024L, "weapon.animations.sprite-file", i_weapon.Source, io_report);
		}

		private static void ValidatePackAsset(string i_root, string i_relative, long i_maxBytes, string i_code,
			string i_source, ValidationReport io_report)
		{
			try
			{
				string path = Path.GetFullPath(Path.Combine(i_root, i_relative));
				if (!AssetPatchDiscovery.IsInside(path, i_root) || !File.Exists(path))
				{
					io_report.Add(ValidationSeverity.Error, i_code, "Asset is missing or outside its pack: " + i_relative, i_source);
					return;
				}
				long bytes = new FileInfo(path).Length;
				if (bytes <= 0 || bytes > i_maxBytes)
					io_report.Add(ValidationSeverity.Error, i_code, "Asset is empty or larger than its allowed limit: " + i_relative, i_source);
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException || exception is NotSupportedException)
			{
				io_report.Add(ValidationSeverity.Error, i_code, exception.Message, i_source);
			}
		}

		private static void ValidateStageAudioFile(string i_root, string i_relative, string i_slot, string i_source, ValidationReport io_report)
		{
			if (string.IsNullOrWhiteSpace(i_relative)) return;
			try
			{
				string path = Path.GetFullPath(Path.Combine(i_root, i_relative));
				if (!AssetPatchDiscovery.IsInside(path, i_root) || !File.Exists(path) || new FileInfo(path).Length <= 0
					|| new FileInfo(path).Length > 64L * 1024L * 1024L)
					io_report.Add(ValidationSeverity.Error, "stage.audio-file", "Stage " + i_slot + " audio is missing, outside its pack, empty, or larger than 64 MiB: " + i_relative, i_source);
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException || exception is NotSupportedException)
			{
				io_report.Add(ValidationSeverity.Error, "stage.audio-file", exception.Message, i_source);
			}
		}
	}
}
