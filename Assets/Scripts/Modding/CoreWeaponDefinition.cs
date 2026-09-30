using System;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreWeaponDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; }
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("legacyName", Required = Required.Always)] public string LegacyName { get; set; }
		[JsonProperty("displayName", Required = Required.Always)] public string DisplayName { get; set; }
		[JsonProperty("description")] public string Description { get; set; }
		[JsonProperty("stats", Required = Required.Always)] public WeaponStatsDefinition Stats { get; set; }
	}

	public sealed class CoreWeaponDefinition
	{
		public ContentId Id { get; }
		public string Source { get; }
		public string LegacyName { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public WeaponStatsDefinition Stats { get; }

		internal CoreWeaponDefinition(ContentId i_id, string i_source, CoreWeaponDocument i_document)
		{
			Id = i_id; Source = i_source ?? string.Empty; LegacyName = i_document.LegacyName;
			DisplayName = i_document.DisplayName; Description = i_document.Description ?? string.Empty; Stats = i_document.Stats;
		}
	}

	public sealed class CoreWeaponLoadResult
	{
		public CoreWeaponDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class CoreWeaponParser
	{
		public static CoreWeaponLoadResult Parse(string i_json, string i_source)
		{
			CoreWeaponLoadResult result = new CoreWeaponLoadResult();
			CoreWeaponDocument document;
			try { document = JsonConvert.DeserializeObject<CoreWeaponDocument>(i_json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }
			catch (JsonException exception) { Error(result, "json", exception.Message, i_source); return result; }
			if (document == null) { Error(result, "null", "Core weapon definition resolved to null.", i_source); return result; }
			if (document.SchemaVersion != 1) Error(result, "schema-version", "Unsupported schemaVersion.", i_source);
			if (document.Type != "coreWeapon") Error(result, "type", "Definition type must be 'coreWeapon'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != "core" || !id.Path.StartsWith("item/weapon/", StringComparison.Ordinal))
				Error(result, "id", "Core weapon ID must use core:item/weapon/.", i_source);
			if (string.IsNullOrWhiteSpace(document.LegacyName)) Error(result, "legacy-name", "legacyName is required for the prefab adapter.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName)) Error(result, "display-name", "displayName is required.", i_source);
			if (document.Stats == null) Error(result, "stats", "stats is required.", i_source);
			else WeaponDefinitionParser.ValidateStats(document.Stats, result.Report, i_source);
			if (result.Report.IsValid) result.Definition = new CoreWeaponDefinition(id, i_source, document);
			return result;
		}

		private static void Error(CoreWeaponLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "core-weapon." + i_code, i_message, i_source);
		}
	}
}
