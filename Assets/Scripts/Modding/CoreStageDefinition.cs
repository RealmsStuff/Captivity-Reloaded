using System;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreStageDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; }
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("legacyId", Required = Required.Always)] public int LegacyId { get; set; }
		[JsonProperty("displayName", Required = Required.Always)] public string DisplayName { get; set; }
		[JsonProperty("description")] public string Description { get; set; }
		[JsonProperty("firstWaveEnemyCount")] public int? FirstWaveEnemyCount { get; set; }
	}

	public sealed class CoreStageDefinition
	{
		public ContentId Id { get; }
		public int LegacyId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public int? FirstWaveEnemyCount { get; }

		internal CoreStageDefinition(ContentId i_id, string i_source, CoreStageDocument i_document)
		{
			Id = i_id; LegacyId = i_document.LegacyId; Source = i_source ?? string.Empty;
			DisplayName = i_document.DisplayName; Description = i_document.Description ?? string.Empty;
			FirstWaveEnemyCount = i_document.FirstWaveEnemyCount;
		}
	}

	public sealed class CoreStageLoadResult
	{
		public CoreStageDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class CoreStageParser
	{
		public static CoreStageLoadResult Parse(string i_json, string i_source)
		{
			CoreStageLoadResult result = new CoreStageLoadResult();
			CoreStageDocument document;
			try { document = JsonConvert.DeserializeObject<CoreStageDocument>(i_json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }
			catch (JsonException exception) { Error(result, "json", exception.Message, i_source); return result; }
			if (document == null) { Error(result, "null", "Core stage definition resolved to null.", i_source); return result; }
			if (document.SchemaVersion != 1) Error(result, "schema-version", "Unsupported schemaVersion.", i_source);
			if (document.Type != "coreStage") Error(result, "type", "Definition type must be 'coreStage'.", i_source);
			bool validId = ContentId.TryParse(document.Id, out ContentId id) && id.Namespace == "core" && id.Path.StartsWith("stage/", StringComparison.Ordinal);
			if (!validId) Error(result, "id", "Core stage ID must use a core:stage/ path.", i_source);
			if (document.LegacyId < 0) Error(result, "legacy-id", "legacyId cannot be negative.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName)) Error(result, "display-name", "displayName is required.", i_source);
			if (document.FirstWaveEnemyCount.HasValue && document.FirstWaveEnemyCount.Value < 1)
				Error(result, "first-wave-enemy-count", "firstWaveEnemyCount must be at least 1 when supplied.", i_source);
			if (validId && id.Path == "stage/hub" && document.FirstWaveEnemyCount.HasValue)
				Error(result, "hub-wave", "The Hub cannot define a wave count.", i_source);
			if (validId && id.Path != "stage/hub" && !document.FirstWaveEnemyCount.HasValue)
				Error(result, "wave-required", "Non-Hub Core stages require firstWaveEnemyCount.", i_source);
			if (result.Report.IsValid) result.Definition = new CoreStageDefinition(id, i_source, document);
			return result;
		}

		private static void Error(CoreStageLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "core-stage." + i_code, i_message, i_source);
		}
	}
}
