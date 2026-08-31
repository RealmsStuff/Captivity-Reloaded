using System;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class UsableVisualDefinition
	{
		[JsonProperty("type", Required = Required.Always)]
		public string Type { get; set; }

		[JsonProperty("icon", Required = Required.Always)]
		public string Icon { get; set; }

		[JsonProperty("world")]
		public string World { get; set; }

		[JsonProperty("pixelsPerUnit")]
		public float PixelsPerUnit { get; set; } = 32f;
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class UsableDefinitionDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)]
		public int SchemaVersion { get; set; }

		[JsonProperty("type", Required = Required.Always)]
		public string Type { get; set; }

		[JsonProperty("id", Required = Required.Always)]
		public string Id { get; set; }

		[JsonProperty("displayName", Required = Required.Always)]
		public string DisplayName { get; set; }

		[JsonProperty("extends", Required = Required.Always)]
		public string Extends { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("visual", Required = Required.Always)]
		public UsableVisualDefinition Visual { get; set; }
	}

	public sealed class UsableDefinition
	{
		public ContentId Id { get; }
		public ContentId Extends { get; }
		public string PackId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public UsableVisualDefinition Visual { get; }

		public UsableDefinition(ContentId i_id, ContentId i_extends, string i_packId, string i_source, UsableDefinitionDocument i_document)
		{
			Id = i_id;
			Extends = i_extends;
			PackId = i_packId;
			Source = i_source;
			DisplayName = i_document.DisplayName;
			Description = i_document.Description ?? string.Empty;
			Visual = i_document.Visual;
		}
	}

	public sealed class UsableDefinitionLoadResult
	{
		public UsableDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class UsableDefinitionParser
	{
		public const int SupportedSchemaVersion = 1;

		public static UsableDefinitionLoadResult Parse(string i_json, string i_packId, string i_source)
		{
			UsableDefinitionLoadResult result = new UsableDefinitionLoadResult();
			UsableDefinitionDocument document;
			try
			{
				document = JsonConvert.DeserializeObject<UsableDefinitionDocument>(i_json, new JsonSerializerSettings
				{
					MissingMemberHandling = MissingMemberHandling.Error
				});
			}
			catch (JsonException exception)
			{
				result.Report.Add(ValidationSeverity.Error, "usable.json", exception.Message, i_source);
				return result;
			}
			if (document == null)
			{
				result.Report.Add(ValidationSeverity.Error, "usable.null", "Usable definition resolved to null.", i_source);
				return result;
			}

			if (document.SchemaVersion != SupportedSchemaVersion) Error(result, "schema-version", "Unsupported schemaVersion " + document.SchemaVersion + ".", i_source);
			if (!string.Equals(document.Type, "usable", StringComparison.Ordinal)) Error(result, "type", "Definition type must be 'usable'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != i_packId || !id.Path.StartsWith("item/usable/", StringComparison.Ordinal))
				Error(result, "id", "Usable ID must use the defining pack namespace and an item/usable/ path.", i_source);
			if (!ContentId.TryParse(document.Extends, out ContentId extends) || extends.Namespace != "core" || !extends.Path.StartsWith("item/usable/", StringComparison.Ordinal))
				Error(result, "extends", "V1 extends must reference a Core item/usable content ID.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName)) Error(result, "display-name", "displayName is required.", i_source);
			ValidateVisual(document.Visual, result.Report, i_source);
			if (result.Report.IsValid) result.Definition = new UsableDefinition(id, extends, i_packId, i_source, document);
			return result;
		}

		private static void ValidateVisual(UsableVisualDefinition i_visual, ValidationReport io_report, string i_source)
		{
			if (i_visual == null)
			{
				io_report.Add(ValidationSeverity.Error, "usable.visual", "visual is required.", i_source);
				return;
			}
			if (!string.Equals(i_visual.Type, "coreUsableSprites", StringComparison.Ordinal))
				io_report.Add(ValidationSeverity.Error, "usable.visual.type", "V1 visual type must be 'coreUsableSprites'.", i_source);
			ValidatePng(i_visual.Icon, "icon", io_report, i_source);
			if (!string.IsNullOrEmpty(i_visual.World)) ValidatePng(i_visual.World, "world", io_report, i_source);
			if (float.IsNaN(i_visual.PixelsPerUnit) || float.IsInfinity(i_visual.PixelsPerUnit) || i_visual.PixelsPerUnit <= 0f || i_visual.PixelsPerUnit > 1024f)
				io_report.Add(ValidationSeverity.Error, "usable.visual.pixels-per-unit", "pixelsPerUnit must be greater than 0 and at most 1024.", i_source);
		}

		private static void ValidatePng(string i_path, string i_field, ValidationReport io_report, string i_source)
		{
			if (!ModPath.IsSafeRelativePath(i_path) || !i_path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				io_report.Add(ValidationSeverity.Error, "usable.visual." + i_field, i_field + " must be a safe pack-relative PNG path.", i_source);
		}

		private static void Error(UsableDefinitionLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "usable." + i_code, i_message, i_source);
		}
	}
}
