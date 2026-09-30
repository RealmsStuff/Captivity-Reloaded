using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreChallengeRecord
	{
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("legacyId", Required = Required.Always)] public int LegacyId { get; set; }
		[JsonProperty("objectiveAdapter", Required = Required.Always)] public string ObjectiveAdapter { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreChallengeCatalogDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; }
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("entries", Required = Required.Always)] public List<CoreChallengeRecord> Entries { get; set; }
	}

	public sealed class CoreChallengeDefinition
	{
		public ContentId Id { get; }
		public int LegacyId { get; }
		public string ObjectiveAdapter { get; }
		public string Source { get; }
		internal CoreChallengeDefinition(ContentId i_id, CoreChallengeRecord i_record, string i_source)
		{
			Id = i_id; LegacyId = i_record.LegacyId; ObjectiveAdapter = i_record.ObjectiveAdapter; Source = i_source ?? string.Empty;
		}
	}

	public sealed class CoreChallengeCatalogLoadResult
	{
		public List<CoreChallengeDefinition> Definitions { get; } = new List<CoreChallengeDefinition>();
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class CoreChallengeCatalogParser
	{
		private static readonly HashSet<string> s_adapters = new HashSet<string>(StringComparer.Ordinal)
		{
			"birthTotal", "headHumperLetGo", "impregnation", "interaction", "killCount", "killCountTotal",
			"litresTakenTotal", "mindBrokenTotal", "pickUp", "rape", "rapeGeneral", "reachWave", "sitApproval",
			"bitch", "filicide", "insectsest", "lamarr", "nimbleLegs", "samus", "stoneLady", "swollen"
		};

		public static CoreChallengeCatalogLoadResult Parse(string i_json, string i_source)
		{
			CoreChallengeCatalogLoadResult result = new CoreChallengeCatalogLoadResult();
			CoreChallengeCatalogDocument document;
			try { document = JsonConvert.DeserializeObject<CoreChallengeCatalogDocument>(i_json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }
			catch (JsonException exception) { Error(result, "json", exception.Message, i_source); return result; }
			if (document == null) { Error(result, "null", "Core challenge catalog resolved to null.", i_source); return result; }
			if (document.SchemaVersion != 1) Error(result, "schema-version", "Unsupported schemaVersion.", i_source);
			if (document.Type != "coreChallengeCatalog") Error(result, "type", "Definition type must be 'coreChallengeCatalog'.", i_source);
			if (document.Entries == null || document.Entries.Count == 0) Error(result, "entries", "entries must contain at least one Core challenge record.", i_source);
			HashSet<ContentId> ids = new HashSet<ContentId>(); HashSet<int> legacyIds = new HashSet<int>();
			foreach (CoreChallengeRecord record in document.Entries ?? new List<CoreChallengeRecord>())
			{
				bool validId = ContentId.TryParse(record.Id, out ContentId id) && id.Namespace == "core" && id.Path.StartsWith("challenge/", StringComparison.Ordinal);
				if (!validId) Error(result, "id", "Entry IDs must use a core:challenge/ path.", i_source);
				else if (!ids.Add(id)) Error(result, "duplicate-id", "Duplicate Core challenge ID: " + id, i_source);
				if (record.LegacyId < 1) Error(result, "legacy-id", "legacyId must be positive.", i_source);
				else if (!legacyIds.Add(record.LegacyId)) Error(result, "duplicate-legacy-id", "Duplicate Core challenge legacyId: " + record.LegacyId, i_source);
				if (!s_adapters.Contains(record.ObjectiveAdapter ?? string.Empty)) Error(result, "objective-adapter", "Unknown objectiveAdapter: " + record.ObjectiveAdapter, i_source);
				if (validId && s_adapters.Contains(record.ObjectiveAdapter ?? string.Empty)) result.Definitions.Add(new CoreChallengeDefinition(id, record, i_source));
			}
			if (!result.Report.IsValid) result.Definitions.Clear();
			return result;
		}

		private static void Error(CoreChallengeCatalogLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "core-challenge." + i_code, i_message, i_source);
		}
	}
}
