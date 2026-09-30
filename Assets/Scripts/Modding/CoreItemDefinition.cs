using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreItemDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; }
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("legacyName", Required = Required.Always)] public string LegacyName { get; set; }
		[JsonProperty("displayName", Required = Required.Always)] public string DisplayName { get; set; }
		[JsonProperty("description")] public string Description { get; set; }
		[JsonProperty("stats", Required = Required.Always)] public UsableStatsDefinition Stats { get; set; }
		[JsonProperty("effectMode")] public string EffectMode { get; set; } = "inherit";
		[JsonProperty("effects")] public List<UsableEffectDefinition> Effects { get; set; } = new List<UsableEffectDefinition>();
	}

	public sealed class CoreItemDefinition
	{
		public ContentId Id { get; }
		public string LegacyName { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public UsableStatsDefinition Stats { get; }
		public string EffectMode { get; }
		public IReadOnlyList<UsableEffectDefinition> Effects { get; }

		internal CoreItemDefinition(ContentId i_id, string i_source, CoreItemDocument i_document)
		{
			Id = i_id; LegacyName = i_document.LegacyName; Source = i_source ?? string.Empty;
			DisplayName = i_document.DisplayName; Description = i_document.Description ?? string.Empty;
			Stats = i_document.Stats;
			EffectMode = i_document.EffectMode ?? "inherit";
			Effects = i_document.Effects ?? new List<UsableEffectDefinition>();
		}
	}

	public sealed class CoreItemLoadResult
	{
		public CoreItemDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class CoreItemParser
	{
		public static CoreItemLoadResult Parse(string i_json, string i_source)
		{
			CoreItemLoadResult result = new CoreItemLoadResult();
			CoreItemDocument document;
			try { document = JsonConvert.DeserializeObject<CoreItemDocument>(i_json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }
			catch (JsonException exception) { Error(result, "json", exception.Message, i_source); return result; }
			if (document == null) { Error(result, "null", "Core item definition resolved to null.", i_source); return result; }
			if (document.SchemaVersion != 1) Error(result, "schema-version", "Unsupported schemaVersion.", i_source);
			if (document.Type != "coreItem") Error(result, "type", "Definition type must be 'coreItem'.", i_source);
			bool validId = ContentId.TryParse(document.Id, out ContentId id) && id.Namespace == "core" &&
				(id.Path.StartsWith("item/usable/", StringComparison.Ordinal) || id.Path.StartsWith("item/consumable/", StringComparison.Ordinal));
			if (!validId) Error(result, "id", "Core item ID must use core:item/usable/ or core:item/consumable/.", i_source);
			if (string.IsNullOrWhiteSpace(document.LegacyName)) Error(result, "legacy-name", "legacyName is required for the prefab adapter.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName)) Error(result, "display-name", "displayName is required.", i_source);
			if (document.Stats == null) Error(result, "stats", "stats is required.", i_source);
			else UsableDefinitionParser.ValidateStats(document.Stats, result.Report, i_source);
			UsableDefinitionParser.ValidateEffects(document.EffectMode, document.Effects, result.Report, i_source);
			if (validId && id.Path.StartsWith("item/consumable/", StringComparison.Ordinal) && document.Stats != null &&
				(document.Stats.EquipSeconds.HasValue || document.Stats.Marketable.HasValue || document.Stats.GoodEffectDescriptions != null || document.Stats.BadEffectDescriptions != null ||
				document.EffectMode != "inherit" || (document.Effects != null && document.Effects.Count > 0)))
				Error(result, "consumable-stats", "Core consumables currently support only weight and value.", i_source);
			if (result.Report.IsValid) result.Definition = new CoreItemDefinition(id, i_source, document);
			return result;
		}

		private static void Error(CoreItemLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "core-item." + i_code, i_message, i_source);
		}
	}
}
