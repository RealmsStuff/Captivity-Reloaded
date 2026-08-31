using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class AssetPatchDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)]
		public int SchemaVersion { get; set; }

		[JsonProperty("type", Required = Required.Always)]
		public string Type { get; set; }

		[JsonProperty("id", Required = Required.Always)]
		public string Id { get; set; }

		[JsonProperty("target", Required = Required.Always)]
		public string Target { get; set; }

		[JsonProperty("replacements", Required = Required.Always)]
		public Dictionary<string, string> Replacements { get; set; }
	}

	public sealed class AssetReplacementDefinition
	{
		public ContentId SlotId { get; }
		public string AssetPath { get; }

		public AssetReplacementDefinition(ContentId i_slotId, string i_assetPath)
		{
			SlotId = i_slotId;
			AssetPath = i_assetPath;
		}
	}

	public sealed class AssetPatchDefinition
	{
		public ContentId Id { get; }
		public ContentId Target { get; }
		public string PackId { get; }
		public string Source { get; }
		public IReadOnlyList<AssetReplacementDefinition> Replacements { get; }

		public AssetPatchDefinition(ContentId i_id, ContentId i_target, string i_packId, string i_source,
			IReadOnlyList<AssetReplacementDefinition> i_replacements)
		{
			Id = i_id;
			Target = i_target;
			PackId = i_packId;
			Source = i_source;
			Replacements = i_replacements;
		}
	}

	public sealed class AssetPatchLoadResult
	{
		public AssetPatchDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class AssetPatchParser
	{
		public const int SupportedSchemaVersion = 1;

		public static AssetPatchLoadResult Parse(string i_json, string i_packId, string i_source)
		{
			AssetPatchLoadResult result = new AssetPatchLoadResult();
			AssetPatchDocument document;
			try
			{
				document = JsonConvert.DeserializeObject<AssetPatchDocument>(i_json, new JsonSerializerSettings
				{
					MissingMemberHandling = MissingMemberHandling.Error
				});
			}
			catch (JsonException exception)
			{
				result.Report.Add(ValidationSeverity.Error, "asset-patch.json", exception.Message, i_source);
				return result;
			}

			if (document == null)
			{
				result.Report.Add(ValidationSeverity.Error, "asset-patch.null", "Asset patch resolved to null.", i_source);
				return result;
			}
			if (document.SchemaVersion != SupportedSchemaVersion)
				result.Report.Add(ValidationSeverity.Error, "asset-patch.schema-version", "Unsupported schemaVersion " + document.SchemaVersion + ".", i_source);
			if (!string.Equals(document.Type, "assetPatch", StringComparison.Ordinal))
				result.Report.Add(ValidationSeverity.Error, "asset-patch.type-name", "Definition type must be 'assetPatch'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId patchId) || patchId.Namespace != i_packId || !patchId.Path.StartsWith("patch/", StringComparison.Ordinal))
				result.Report.Add(ValidationSeverity.Error, "asset-patch.id", "Patch ID must use the defining pack namespace and a patch/ path.", i_source);
			if (!ContentId.TryParse(document.Target, out ContentId target))
				result.Report.Add(ValidationSeverity.Error, "asset-patch.target-id", "Patch target is not a valid public ID.", i_source);

			List<AssetReplacementDefinition> replacements = new List<AssetReplacementDefinition>();
			if (document.Replacements == null || document.Replacements.Count == 0)
			{
				result.Report.Add(ValidationSeverity.Error, "asset-patch.replacements", "At least one replacement is required.", i_source);
			}
			else if (!string.IsNullOrEmpty(target.ToString()))
			{
				foreach (KeyValuePair<string, string> replacement in document.Replacements)
				{
					if (!IsSafeSlotSuffix(replacement.Key) || !ContentId.TryParse(target + "/" + replacement.Key, out ContentId slotId))
					{
						result.Report.Add(ValidationSeverity.Error, "asset-patch.slot", "Replacement key is not a safe slot path: " + replacement.Key, i_source);
						continue;
					}
					if (!ModPath.IsSafeRelativePath(replacement.Value) || !replacement.Value.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
					{
						result.Report.Add(ValidationSeverity.Error, "asset-patch.asset-path", "Replacement must reference a safe relative PNG path: " + replacement.Value, i_source);
						continue;
					}
					replacements.Add(new AssetReplacementDefinition(slotId, replacement.Value));
				}
			}

			if (result.Report.IsValid) result.Definition = new AssetPatchDefinition(patchId, target, i_packId, i_source, replacements);
			return result;
		}

		private static bool IsSafeSlotSuffix(string i_value)
		{
			return !string.IsNullOrWhiteSpace(i_value) && ContentId.TryParse("slot:" + i_value, out _);
		}
	}

	public sealed class AssetPatchDiscoveryResult
	{
		public List<AssetPatchDefinition> Definitions { get; } = new List<AssetPatchDefinition>();
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class AssetPatchDiscovery
	{
		public static AssetPatchDiscoveryResult Discover(IEnumerable<ModPack> i_packs)
		{
			AssetPatchDiscoveryResult result = new AssetPatchDiscoveryResult();
			if (i_packs == null) return result;
			foreach (ModPack pack in i_packs)
			{
				if (pack?.Manifest == null || pack.Manifest.Id == "core") continue;
				foreach (string contentRoot in pack.Manifest.ContentRoots)
				{
					string root = Path.GetFullPath(Path.Combine(pack.RootPath, contentRoot));
					if (!IsInside(root, pack.RootPath) || !Directory.Exists(root))
					{
						result.Report.Add(ValidationSeverity.Warning, "asset-patch.content-root", "Content root does not exist or escapes its pack.", root);
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
						result.Report.Add(ValidationSeverity.Error, "asset-patch.read-root", exception.Message, root);
						continue;
					}
					foreach (string file in files) DiscoverFile(file, pack, result);
				}
			}
			return result;
		}

		private static void DiscoverFile(string i_file, ModPack i_pack, AssetPatchDiscoveryResult io_result)
		{
			try
			{
				string json = File.ReadAllText(i_file);
				JObject root = JObject.Parse(json);
				if (!string.Equals((string)root["type"], "assetPatch", StringComparison.Ordinal)) return;
				AssetPatchLoadResult loaded = AssetPatchParser.Parse(json, i_pack.Manifest.Id, i_file);
				io_result.Report.Merge(loaded.Report);
				if (loaded.Report.IsValid) io_result.Definitions.Add(loaded.Definition);
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is JsonException)
			{
				io_result.Report.Add(ValidationSeverity.Error, "asset-patch.read-definition", exception.Message, i_file);
			}
		}

		public static bool IsInside(string i_path, string i_root)
		{
			string path = Path.GetFullPath(i_path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
			string root = Path.GetFullPath(i_root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
			return path.StartsWith(root, StringComparison.OrdinalIgnoreCase);
		}
	}
}
