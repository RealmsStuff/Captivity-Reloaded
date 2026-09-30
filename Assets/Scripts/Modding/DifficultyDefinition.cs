using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class DifficultyDefinitionDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; }
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("displayName", Required = Required.Always)] public string DisplayName { get; set; }
		[JsonProperty("sortOrder")] public int SortOrder { get; set; }
		[JsonProperty("enemyHealthMultiplier", Required = Required.Always)] public float EnemyHealthMultiplier { get; set; }
		[JsonProperty("playerDamageTakenMultiplier", Required = Required.Always)] public float PlayerDamageTakenMultiplier { get; set; }
		[JsonProperty("escapeStrengthMultiplier", Required = Required.Always)] public float EscapeStrengthMultiplier { get; set; }
	}

	public sealed class DifficultyDefinition
	{
		public ContentId Id { get; }
		public string PackId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public int SortOrder { get; }
		public float EnemyHealthMultiplier { get; }
		public float PlayerDamageTakenMultiplier { get; }
		public float EscapeStrengthMultiplier { get; }

		public DifficultyDefinition(ContentId i_id, string i_packId, string i_source, string i_displayName,
			int i_sortOrder, float i_enemyHealth, float i_playerDamage, float i_escapeStrength)
		{
			Id = i_id; PackId = i_packId; Source = i_source ?? string.Empty; DisplayName = i_displayName;
			SortOrder = i_sortOrder; EnemyHealthMultiplier = i_enemyHealth;
			PlayerDamageTakenMultiplier = i_playerDamage; EscapeStrengthMultiplier = i_escapeStrength;
		}
	}

	public sealed class DifficultyDefinitionLoadResult
	{
		public DifficultyDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class DifficultyDefinitionParser
	{
		public static DifficultyDefinitionLoadResult Parse(string i_json, string i_packId, string i_source)
		{
			DifficultyDefinitionLoadResult result = new DifficultyDefinitionLoadResult();
			DifficultyDefinitionDocument document;
			try { document = JsonConvert.DeserializeObject<DifficultyDefinitionDocument>(i_json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }
			catch (JsonException exception) { result.Report.Add(ValidationSeverity.Error, "difficulty.json", exception.Message, i_source); return result; }
			if (document == null) { Error(result, "null", "Difficulty definition resolved to null.", i_source); return result; }
			if (document.SchemaVersion != 1) Error(result, "schema-version", "Unsupported schemaVersion.", i_source);
			if (document.Type != "difficulty") Error(result, "type", "Definition type must be 'difficulty'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != i_packId || !id.Path.StartsWith("difficulty/", StringComparison.Ordinal)) Error(result, "id", "Difficulty ID must use the defining pack namespace and a difficulty/ path.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName) || document.DisplayName.Length > 40) Error(result, "display-name", "displayName must contain 1 to 40 characters.", i_source);
			if (document.SortOrder < -100000 || document.SortOrder > 100000) Error(result, "sort-order", "sortOrder must be between -100000 and 100000.", i_source);
			ValidateMultiplier(document.EnemyHealthMultiplier, "enemy-health", result, i_source);
			ValidateMultiplier(document.PlayerDamageTakenMultiplier, "player-damage", result, i_source);
			ValidateMultiplier(document.EscapeStrengthMultiplier, "escape-strength", result, i_source);
			if (result.Report.IsValid) result.Definition = new DifficultyDefinition(id, i_packId, i_source, document.DisplayName,
				document.SortOrder, document.EnemyHealthMultiplier, document.PlayerDamageTakenMultiplier, document.EscapeStrengthMultiplier);
			return result;
		}

		private static void ValidateMultiplier(float i_value, string i_name, DifficultyDefinitionLoadResult io_result, string i_source)
		{
			if (float.IsNaN(i_value) || float.IsInfinity(i_value) || i_value < 0.1f || i_value > 10f)
				Error(io_result, i_name, i_name + " multiplier must be finite and between 0.1 and 10.", i_source);
		}

		private static void Error(DifficultyDefinitionLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "difficulty." + i_code, i_message, i_source);
		}
	}

	public static class DifficultyRegistry
	{
		private static readonly List<DifficultyDefinition> m_definitions = new List<DifficultyDefinition>();
		private static readonly Dictionary<string, DifficultyDefinition> m_byId = new Dictionary<string, DifficultyDefinition>(StringComparer.Ordinal);
		private static readonly Dictionary<string, DifficultyDefinition> m_byLegacyName = new Dictionary<string, DifficultyDefinition>(StringComparer.Ordinal);

		public static IReadOnlyList<DifficultyDefinition> Definitions => m_definitions;

		public static void Initialize(IEnumerable<DifficultyDefinition> i_external, ValidationReport io_report)
		{
			m_definitions.Clear(); m_byId.Clear(); m_byLegacyName.Clear();
			foreach (DifficultyDefinition definition in i_external ?? new DifficultyDefinition[0]) Add(definition, io_report);
			m_definitions.Sort((left, right) => { int order = left.SortOrder.CompareTo(right.SortOrder); return order != 0 ? order : left.Id.CompareTo(right.Id); });
			foreach (string required in new[] { "core:difficulty/casual", "core:difficulty/normal", "core:difficulty/hard" })
				if (!m_byId.ContainsKey(required)) io_report?.Add(ValidationSeverity.Error, "difficulty.core-missing", "Packaged Core difficulty is missing: " + required, "core/content");
		}

		private static void Add(DifficultyDefinition i_definition, ValidationReport io_report)
		{
			string id = i_definition.Id.ToString();
			if (m_byId.ContainsKey(id)) { io_report?.Add(ValidationSeverity.Error, "difficulty.duplicate-id", "Duplicate difficulty ID: " + id, i_definition.Source); return; }
			if (m_byLegacyName.ContainsKey(i_definition.DisplayName)) { io_report?.Add(ValidationSeverity.Error, "difficulty.duplicate-name", "Difficulty displayName must be unique: " + i_definition.DisplayName, i_definition.Source); return; }
			m_byId.Add(id, i_definition); m_byLegacyName.Add(i_definition.DisplayName, i_definition); m_definitions.Add(i_definition);
		}

		public static DifficultyDefinition Resolve(string i_savedValue)
		{
			if (!string.IsNullOrEmpty(i_savedValue) && (m_byId.TryGetValue(i_savedValue, out DifficultyDefinition value) || m_byLegacyName.TryGetValue(i_savedValue, out value))) return value;
			return m_byId.TryGetValue("core:difficulty/normal", out DifficultyDefinition normal) ? normal : null;
		}

		public static DifficultyDefinition Current => Resolve(UnityEngine.PlayerPrefs.GetString("Difficulty"));
		public static void SetCurrent(string i_id) { UnityEngine.PlayerPrefs.SetString("Difficulty", Resolve(i_id)?.Id.ToString() ?? "core:difficulty/normal"); }
	}
}
