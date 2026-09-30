using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreEnemyDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; }
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("legacyId", Required = Required.Always)] public int LegacyId { get; set; }
		[JsonProperty("displayName", Required = Required.Always)] public string DisplayName { get; set; }
		[JsonProperty("description")] public string Description { get; set; }
		[JsonProperty("stats", Required = Required.Always)] public EnemyStatsDefinition Stats { get; set; }
		[JsonProperty("behavior")] public EnemyBehaviorDefinition Behavior { get; set; }
		[JsonProperty("animationReferences")] public Dictionary<string, string> AnimationReferences { get; set; } = new Dictionary<string, string>();
	}

	public sealed class CoreEnemyDefinition
	{
		public ContentId Id { get; }
		public int LegacyId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public EnemyStatsDefinition Stats { get; }
		public EnemyBehaviorDefinition Behavior { get; }
		public IReadOnlyDictionary<string, string> AnimationReferences { get; }

		internal CoreEnemyDefinition(ContentId i_id, string i_source, CoreEnemyDocument i_document)
		{
			Id = i_id; LegacyId = i_document.LegacyId; Source = i_source ?? string.Empty;
			DisplayName = i_document.DisplayName; Description = i_document.Description;
			Stats = i_document.Stats; Behavior = i_document.Behavior ?? new EnemyBehaviorDefinition();
			AnimationReferences = i_document.AnimationReferences ?? new Dictionary<string, string>();
		}
	}

	public sealed class CoreEnemyLoadResult
	{
		public CoreEnemyDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class CoreEnemyParser
	{
		public static CoreEnemyLoadResult Parse(string i_json, string i_source)
		{
			CoreEnemyLoadResult result = new CoreEnemyLoadResult();
			CoreEnemyDocument document;
			try { document = JsonConvert.DeserializeObject<CoreEnemyDocument>(i_json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }
			catch (JsonException exception) { Error(result, "json", exception.Message, i_source); return result; }
			if (document == null) { Error(result, "null", "Core enemy definition resolved to null.", i_source); return result; }
			if (document.SchemaVersion != 1) Error(result, "schema-version", "Unsupported schemaVersion.", i_source);
			if (document.Type != "coreEnemy") Error(result, "type", "Definition type must be 'coreEnemy'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != "core" || !id.Path.StartsWith("enemy/", StringComparison.Ordinal))
				Error(result, "id", "Core enemy ID must use core:enemy/.", i_source);
			if (document.LegacyId <= 0) Error(result, "legacy-id", "legacyId must be a positive vanilla NPC ID.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName)) Error(result, "display-name", "displayName is required.", i_source);
			if (document.Stats == null) Error(result, "stats", "stats is required.", i_source);
			else EnemyDefinitionParser.ValidateStats(document.Stats, result.Report, i_source);
			if (document.Behavior?.VisionRange != null)
			{
				float vision = document.Behavior.VisionRange.Value;
				if (float.IsNaN(vision) || float.IsInfinity(vision) || vision < 0.1f || vision > 10000f)
					Error(result, "vision-range", "visionRange must be between 0.1 and 10000.", i_source);
			}
			if (document.Behavior?.Modules != null && document.Behavior.Modules.Count != 0)
				Error(result, "behavior-modules", "Core adapter definitions retain prefab behavior modules.", i_source);
			foreach (KeyValuePair<string, string> animation in document.AnimationReferences ?? new Dictionary<string, string>())
			{
				if (string.IsNullOrWhiteSpace(animation.Key) || animation.Key.Length > 100)
					Error(result, "animation-name", "Animation reference names must contain 1 through 100 characters.", i_source);
				if (!ContentId.TryParse(animation.Value, out ContentId animationId) || animationId.Namespace != "core"
					|| !animationId.Path.StartsWith("enemy-animation/", StringComparison.Ordinal))
					Error(result, "animation-reference", "Core enemy animations must reference core:enemy-animation/ IDs.", i_source);
			}
			if (result.Report.IsValid) result.Definition = new CoreEnemyDefinition(id, i_source, document);
			return result;
		}

		private static void Error(CoreEnemyLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "core-enemy." + i_code, i_message, i_source);
		}
	}
}
