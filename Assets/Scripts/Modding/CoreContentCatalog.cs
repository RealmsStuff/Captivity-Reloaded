using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	internal sealed class CoreContentCatalogDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)]
		public int SchemaVersion { get; set; }

		[JsonProperty("entries", Required = Required.Always)]
		public List<CoreContentCatalogRecord> Entries { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	internal sealed class CoreContentCatalogRecord
	{
		[JsonProperty("id", Required = Required.Always)]
		public string Id { get; set; }

		[JsonProperty("category", Required = Required.Always)]
		public string Category { get; set; }

		[JsonProperty("legacyId")]
		public int? LegacyId { get; set; }

		[JsonProperty("legacyName")]
		public string LegacyName { get; set; }
	}

	public sealed class CoreContentCatalogEntry
	{
		public ContentId Id { get; }
		public ContentCategory Category { get; }
		public int? LegacyId { get; }
		public string LegacyName { get; }

		internal CoreContentCatalogEntry(ContentId i_id, ContentCategory i_category, int? i_legacyId, string i_legacyName)
		{
			Id = i_id;
			Category = i_category;
			LegacyId = i_legacyId;
			LegacyName = i_legacyName ?? string.Empty;
		}
	}

	public sealed class CoreContentCatalogLoadResult
	{
		public IReadOnlyList<CoreContentCatalogEntry> Entries { get; internal set; } = new CoreContentCatalogEntry[0];
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class CoreContentCatalogParser
	{
		public const int SupportedSchemaVersion = 1;

		public static CoreContentCatalogLoadResult Parse(string i_json, string i_source)
		{
			CoreContentCatalogLoadResult result = new CoreContentCatalogLoadResult();
			if (string.IsNullOrWhiteSpace(i_json))
			{
				result.Report.Add(ValidationSeverity.Error, "catalog.empty", "Core content catalog JSON is empty.", i_source);
				return result;
			}

			CoreContentCatalogDocument document;
			try
			{
				document = JsonConvert.DeserializeObject<CoreContentCatalogDocument>(i_json, new JsonSerializerSettings
				{
					MissingMemberHandling = MissingMemberHandling.Error
				});
			}
			catch (JsonException exception)
			{
				result.Report.Add(ValidationSeverity.Error, "catalog.json", exception.Message, i_source);
				return result;
			}

			if (document == null)
			{
				result.Report.Add(ValidationSeverity.Error, "catalog.null", "Core content catalog resolved to null.", i_source);
				return result;
			}
			if (document.SchemaVersion != SupportedSchemaVersion)
			{
				result.Report.Add(ValidationSeverity.Error, "catalog.schema-version", "Unsupported schemaVersion " + document.SchemaVersion + ".", i_source);
			}
			if (document.Entries == null)
			{
				result.Report.Add(ValidationSeverity.Error, "catalog.entries", "The Core content catalog requires an entries array.", i_source);
				return result;
			}

			List<CoreContentCatalogEntry> entries = new List<CoreContentCatalogEntry>();
			HashSet<ContentId> contentIds = new HashSet<ContentId>();
			HashSet<string> legacySelectors = new HashSet<string>(System.StringComparer.Ordinal);
			foreach (CoreContentCatalogRecord record in document.Entries)
			{
				if (record == null || !ContentId.TryParse(record.Id, out ContentId id) || id.Namespace != "core")
				{
					result.Report.Add(ValidationSeverity.Error, "catalog.id", "Every entry must have a valid ID in the 'core' namespace.", i_source);
					continue;
				}
				if (!System.Enum.TryParse(record.Category, ignoreCase: false, out ContentCategory category) ||
					(category != ContentCategory.Enemy && category != ContentCategory.Stage && category != ContentCategory.Item && category != ContentCategory.Clothing))
				{
					result.Report.Add(ValidationSeverity.Error, "catalog.category", "Core adapter category must be Enemy, Stage, Clothing, or Item: " + record.Category, i_source);
					continue;
				}
				bool hasId = record.LegacyId.HasValue;
				bool hasName = !string.IsNullOrWhiteSpace(record.LegacyName);
				if (hasId == hasName)
				{
					result.Report.Add(ValidationSeverity.Error, "catalog.legacy-selector", "Every Core entry must declare exactly one of legacyId or legacyName: " + id, i_source);
					continue;
				}
				if (hasId && record.LegacyId.Value < 0)
				{
					result.Report.Add(ValidationSeverity.Error, "catalog.legacy-id", "Legacy IDs must be non-negative: " + id, i_source);
					continue;
				}
				if (hasName && record.LegacyName.Trim() != record.LegacyName)
				{
					result.Report.Add(ValidationSeverity.Error, "catalog.legacy-name", "Legacy names cannot have leading or trailing whitespace: " + id, i_source);
					continue;
				}
				if (!contentIds.Add(id))
				{
					result.Report.Add(ValidationSeverity.Error, "catalog.duplicate-id", "Duplicate Core content ID: " + id, i_source);
					continue;
				}
				string legacyKey = category + ":" + (hasId ? "id:" + record.LegacyId.Value : "name:" + record.LegacyName);
				if (!legacySelectors.Add(legacyKey))
				{
					result.Report.Add(ValidationSeverity.Error, "catalog.duplicate-legacy-selector", "Duplicate " + category + " legacy selector: " + legacyKey, i_source);
					continue;
				}
				entries.Add(new CoreContentCatalogEntry(id, category, record.LegacyId, record.LegacyName));
			}

			result.Entries = entries;
			return result;
		}
	}
}
