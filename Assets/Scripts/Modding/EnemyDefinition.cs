using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyStatsDefinition
	{
		[JsonProperty("healthMax")]
		public float? HealthMax { get; set; }

		[JsonProperty("speedAcceleration")]
		public float? SpeedAcceleration { get; set; }

		[JsonProperty("speedMax")]
		public float? SpeedMax { get; set; }

		[JsonProperty("traction")]
		public float? Traction { get; set; }

		[JsonProperty("bounty")]
		public int? Bounty { get; set; }

		[JsonProperty("healthIncreasePerWave")]
		public float? HealthIncreasePerWave { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class AtlasRegionDefinition
	{
		[JsonProperty("x", Required = Required.Always)]
		public int X { get; set; }

		[JsonProperty("y", Required = Required.Always)]
		public int Y { get; set; }

		[JsonProperty("width", Required = Required.Always)]
		public int Width { get; set; }

		[JsonProperty("height", Required = Required.Always)]
		public int Height { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyVisualDefinition
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
	public sealed class EnemyDefinitionDocument
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

		[JsonProperty("stats")]
		public EnemyStatsDefinition Stats { get; set; }

		[JsonProperty("visual", Required = Required.Always)]
		public EnemyVisualDefinition Visual { get; set; }
	}

	public sealed class EnemyDefinition
	{
		public ContentId Id { get; }
		public ContentId Extends { get; }
		public string PackId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public EnemyStatsDefinition Stats { get; }
		public EnemyVisualDefinition Visual { get; }

		public EnemyDefinition(ContentId i_id, ContentId i_extends, string i_packId, string i_source, EnemyDefinitionDocument i_document)
		{
			Id = i_id;
			Extends = i_extends;
			PackId = i_packId;
			Source = i_source;
			DisplayName = i_document.DisplayName;
			Description = i_document.Description ?? string.Empty;
			Stats = i_document.Stats ?? new EnemyStatsDefinition();
			Visual = i_document.Visual;
		}
	}

	public sealed class EnemyDefinitionLoadResult
	{
		public EnemyDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class EnemyDefinitionParser
	{
		public const int SupportedSchemaVersion = 1;

		public static EnemyDefinitionLoadResult Parse(string i_json, string i_packId, string i_source)
		{
			EnemyDefinitionLoadResult result = new EnemyDefinitionLoadResult();
			EnemyDefinitionDocument document;
			try
			{
				document = JsonConvert.DeserializeObject<EnemyDefinitionDocument>(i_json, new JsonSerializerSettings
				{
					MissingMemberHandling = MissingMemberHandling.Error
				});
			}
			catch (JsonException exception)
			{
				result.Report.Add(ValidationSeverity.Error, "enemy.json", exception.Message, i_source);
				return result;
			}
			if (document == null)
			{
				result.Report.Add(ValidationSeverity.Error, "enemy.null", "Enemy definition resolved to null.", i_source);
				return result;
			}

			if (document.SchemaVersion != SupportedSchemaVersion) Error(result, "schema-version", "Unsupported schemaVersion " + document.SchemaVersion + ".", i_source);
			if (!string.Equals(document.Type, "enemy", StringComparison.Ordinal)) Error(result, "type", "Definition type must be 'enemy'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != i_packId || !id.Path.StartsWith("enemy/", StringComparison.Ordinal))
				Error(result, "id", "Enemy ID must use the defining pack namespace and an enemy/ path.", i_source);
			if (!ContentId.TryParse(document.Extends, out ContentId extends) || !extends.Path.StartsWith("enemy/", StringComparison.Ordinal) || extends == id)
				Error(result, "extends", "extends must reference a different enemy content ID.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName)) Error(result, "display-name", "displayName is required.", i_source);

			ValidateStats(document.Stats, result.Report, i_source);
			ValidateVisual(document.Visual, result.Report, i_source);
			if (result.Report.IsValid) result.Definition = new EnemyDefinition(id, extends, i_packId, i_source, document);
			return result;
		}

		private static void ValidateStats(EnemyStatsDefinition i_stats, ValidationReport io_report, string i_source)
		{
			if (i_stats == null) return;
			ValidateRange(i_stats.HealthMax, 0.01f, 1000000f, "healthMax", io_report, i_source);
			ValidateRange(i_stats.SpeedAcceleration, 0f, 1000f, "speedAcceleration", io_report, i_source);
			ValidateRange(i_stats.SpeedMax, 0f, 1000f, "speedMax", io_report, i_source);
			ValidateRange(i_stats.Traction, 0f, 1f, "traction", io_report, i_source);
			ValidateRange(i_stats.HealthIncreasePerWave, 0f, 1000000f, "healthIncreasePerWave", io_report, i_source);
			if (i_stats.Bounty.HasValue && i_stats.Bounty.Value < 0) io_report.Add(ValidationSeverity.Error, "enemy.stats.bounty", "bounty cannot be negative.", i_source);
		}

		private static void ValidateVisual(EnemyVisualDefinition i_visual, ValidationReport io_report, string i_source)
		{
			if (i_visual == null)
			{
				io_report.Add(ValidationSeverity.Error, "enemy.visual", "visual is required.", i_source);
				return;
			}
			if (!string.Equals(i_visual.Type, "coreRigAtlas", StringComparison.Ordinal))
				io_report.Add(ValidationSeverity.Error, "enemy.visual.type", "V1 visual type must be 'coreRigAtlas'.", i_source);
			if (!ModPath.IsSafeRelativePath(i_visual.Atlas) || !i_visual.Atlas.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				io_report.Add(ValidationSeverity.Error, "enemy.visual.atlas", "atlas must be a safe pack-relative PNG path.", i_source);
			if (float.IsNaN(i_visual.PixelsPerUnit) || float.IsInfinity(i_visual.PixelsPerUnit) || i_visual.PixelsPerUnit <= 0f || i_visual.PixelsPerUnit > 1024f)
				io_report.Add(ValidationSeverity.Error, "enemy.visual.pixels-per-unit", "pixelsPerUnit must be greater than 0 and at most 1024.", i_source);
			if (i_visual.Regions == null || i_visual.Regions.Count == 0)
			{
				io_report.Add(ValidationSeverity.Error, "enemy.visual.regions", "At least one atlas region is required.", i_source);
				return;
			}
			foreach (KeyValuePair<string, AtlasRegionDefinition> region in i_visual.Regions)
			{
				if (!ContentId.TryParse("slot:" + region.Key, out _)) io_report.Add(ValidationSeverity.Error, "enemy.visual.region-name", "Region name is not a safe semantic path: " + region.Key, i_source);
				if (region.Value == null || region.Value.X < 0 || region.Value.Y < 0 || region.Value.Width <= 0 || region.Value.Height <= 0)
					io_report.Add(ValidationSeverity.Error, "enemy.visual.region-rect", "Region rectangle must have a non-negative origin and positive size: " + region.Key, i_source);
			}
		}

		private static void ValidateRange(float? i_value, float i_min, float i_max, string i_name, ValidationReport io_report, string i_source)
		{
			if (!i_value.HasValue) return;
			float value = i_value.Value;
			if (float.IsNaN(value) || float.IsInfinity(value) || value < i_min || value > i_max)
				io_report.Add(ValidationSeverity.Error, "enemy.stats." + ToKebab(i_name), i_name + " must be between " + i_min + " and " + i_max + ".", i_source);
		}

		private static string ToKebab(string i_name)
		{
			return i_name.Replace("Acceleration", "-acceleration").Replace("IncreasePerWave", "-increase-per-wave").Replace("Max", "-max").ToLowerInvariant();
		}

		private static void Error(EnemyDefinitionLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "enemy." + i_code, i_message, i_source);
		}
	}
}
