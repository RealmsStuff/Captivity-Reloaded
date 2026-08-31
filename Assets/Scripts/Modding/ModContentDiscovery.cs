using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaptivityReloaded.Modding
{
	public sealed class ModContentDiscoveryResult
	{
		public List<AssetPatchDefinition> AssetPatches { get; } = new List<AssetPatchDefinition>();
		public List<EnemyDefinition> Enemies { get; } = new List<EnemyDefinition>();
		public List<ClothingDefinition> Clothing { get; } = new List<ClothingDefinition>();
		public List<WeaponDefinition> Weapons { get; } = new List<WeaponDefinition>();
		public List<UsableDefinition> Usables { get; } = new List<UsableDefinition>();
		public List<StageDefinition> Stages { get; } = new List<StageDefinition>();
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class ModContentDiscovery
	{
		public static ModContentDiscoveryResult Discover(IEnumerable<ModPack> i_packs)
		{
			ModContentDiscoveryResult result = new ModContentDiscoveryResult();
			if (i_packs == null) return result;
			foreach (ModPack pack in i_packs)
			{
				if (pack?.Manifest == null || pack.Manifest.Id == "core") continue;
				foreach (string contentRoot in pack.Manifest.ContentRoots)
				{
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
					foreach (string file in files) DiscoverFile(file, pack, result);
				}
			}
			return result;
		}

		private static void DiscoverFile(string i_file, ModPack i_pack, ModContentDiscoveryResult io_result)
		{
			try
			{
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
				else if (string.Equals(type, "weapon", StringComparison.Ordinal))
				{
					WeaponDefinitionLoadResult loaded = WeaponDefinitionParser.Parse(json, i_pack.Manifest.Id, i_file);
					io_result.Report.Merge(loaded.Report);
					if (loaded.Report.IsValid) io_result.Weapons.Add(loaded.Definition);
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
					if (loaded.Report.IsValid) io_result.Stages.Add(loaded.Definition);
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
	}
}
