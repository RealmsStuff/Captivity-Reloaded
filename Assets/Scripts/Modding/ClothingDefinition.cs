using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ClothingVisualDefinition
	{
		[JsonProperty("type", Required = Required.Always)]
		public string Type { get; set; }

		[JsonProperty("atlas", Required = Required.Always)]
		public string Atlas { get; set; }

		[JsonProperty("pixelsPerUnit")]
		public float PixelsPerUnit { get; set; } = 32f;

		[JsonProperty("regions", Required = Required.Always)]
		public Dictionary<string, AtlasRegionDefinition> Regions { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ClothingDefinitionDocument
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

		[JsonProperty("unlockedByDefault")]
		public bool UnlockedByDefault { get; set; }

		[JsonProperty("visual", Required = Required.Always)]
		public ClothingVisualDefinition Visual { get; set; }
	}

	public sealed class ClothingDefinition
	{
		public ContentId Id { get; }
		public ContentId Extends { get; }
		public string PackId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public ClothingVisualDefinition Visual { get; }
		public bool UnlockedByDefault { get; }

		public ClothingDefinition(ContentId i_id, ContentId i_extends, string i_packId, string i_source, ClothingDefinitionDocument i_document)
		{
			Id = i_id;
			Extends = i_extends;
			PackId = i_packId;
			Source = i_source;
			DisplayName = i_document.DisplayName;
			Description = i_document.Description ?? string.Empty;
			UnlockedByDefault = i_document.UnlockedByDefault;
			Visual = i_document.Visual;
		}
	}

	public sealed class ClothingDefinitionLoadResult
	{
		public ClothingDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class ClothingDefinitionParser
	{
		public const int SupportedSchemaVersion = 1;

		public static ClothingDefinitionLoadResult Parse(string i_json, string i_packId, string i_source)
		{
			ClothingDefinitionLoadResult result = new ClothingDefinitionLoadResult();
			ClothingDefinitionDocument document;
			try
			{
				document = JsonConvert.DeserializeObject<ClothingDefinitionDocument>(i_json, new JsonSerializerSettings
				{
					MissingMemberHandling = MissingMemberHandling.Error
				});
			}
			catch (JsonException exception)
			{
				result.Report.Add(ValidationSeverity.Error, "clothing.json", exception.Message, i_source);
				return result;
			}
			if (document == null)
			{
				result.Report.Add(ValidationSeverity.Error, "clothing.null", "Clothing definition resolved to null.", i_source);
				return result;
			}

			if (document.SchemaVersion != SupportedSchemaVersion) Error(result, "schema-version", "Unsupported schemaVersion " + document.SchemaVersion + ".", i_source);
			if (!string.Equals(document.Type, "clothing", StringComparison.Ordinal)) Error(result, "type", "Definition type must be 'clothing'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != i_packId || !id.Path.StartsWith("clothing/", StringComparison.Ordinal))
				Error(result, "id", "Clothing ID must use the defining pack namespace and a clothing/ path.", i_source);
			if (!ContentId.TryParse(document.Extends, out ContentId extends) || extends.Namespace != "core" || !extends.Path.StartsWith("clothing/", StringComparison.Ordinal) || extends == id)
				Error(result, "extends", "V1 extends must reference a Core clothing content ID.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName)) Error(result, "display-name", "displayName is required.", i_source);
			ValidateVisual(document.Visual, result.Report, i_source);
			if (result.Report.IsValid) result.Definition = new ClothingDefinition(id, extends, i_packId, i_source, document);
			return result;
		}

		private static void ValidateVisual(ClothingVisualDefinition i_visual, ValidationReport io_report, string i_source)
		{
			if (i_visual == null)
			{
				io_report.Add(ValidationSeverity.Error, "clothing.visual", "visual is required.", i_source);
				return;
			}
			if (!string.Equals(i_visual.Type, "coreClothingAtlas", StringComparison.Ordinal))
				io_report.Add(ValidationSeverity.Error, "clothing.visual.type", "V1 visual type must be 'coreClothingAtlas'.", i_source);
			if (!ModPath.IsSafeRelativePath(i_visual.Atlas) || !i_visual.Atlas.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				io_report.Add(ValidationSeverity.Error, "clothing.visual.atlas", "atlas must be a safe pack-relative PNG path.", i_source);
			if (float.IsNaN(i_visual.PixelsPerUnit) || float.IsInfinity(i_visual.PixelsPerUnit) || i_visual.PixelsPerUnit <= 0f || i_visual.PixelsPerUnit > 1024f)
				io_report.Add(ValidationSeverity.Error, "clothing.visual.pixels-per-unit", "pixelsPerUnit must be greater than 0 and at most 1024.", i_source);
			if (i_visual.Regions == null || i_visual.Regions.Count == 0)
			{
				io_report.Add(ValidationSeverity.Error, "clothing.visual.regions", "At least one atlas region is required.", i_source);
				return;
			}
			foreach (KeyValuePair<string, AtlasRegionDefinition> region in i_visual.Regions)
			{
				if (!ContentId.TryParse("slot:" + region.Key, out _)) io_report.Add(ValidationSeverity.Error, "clothing.visual.region-name", "Region name is not a safe semantic path: " + region.Key, i_source);
				if (region.Value == null || region.Value.X < 0 || region.Value.Y < 0 || region.Value.Width <= 0 || region.Value.Height <= 0)
					io_report.Add(ValidationSeverity.Error, "clothing.visual.region-rect", "Region rectangle must have a non-negative origin and positive size: " + region.Key, i_source);
			}
		}

		private static void Error(ClothingDefinitionLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "clothing." + i_code, i_message, i_source);
		}
	}
}
