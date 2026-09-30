using System;
using System.Collections.Generic;
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

		[JsonProperty("pivotX")] public float PivotX { get; set; } = 0.5f;
		[JsonProperty("pivotY")] public float PivotY { get; set; } = 0.5f;
		[JsonProperty("colliderWidth")] public float ColliderWidth { get; set; } = 0.75f;
		[JsonProperty("colliderHeight")] public float ColliderHeight { get; set; } = 0.75f;
		[JsonProperty("sortingOrder")] public int SortingOrder { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class UsableStatsDefinition
	{
		[JsonProperty("weight")] public int? Weight { get; set; }
		[JsonProperty("value")] public int? Value { get; set; }
		[JsonProperty("equipSeconds")] public float? EquipSeconds { get; set; }
		[JsonProperty("marketable")] public bool? Marketable { get; set; }
		[JsonProperty("goodEffectDescriptions")] public List<string> GoodEffectDescriptions { get; set; }
		[JsonProperty("badEffectDescriptions")] public List<string> BadEffectDescriptions { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class UsableEffectDefinition
	{
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("amount")] public float? Amount { get; set; }
		[JsonProperty("durationSeconds")] public float? DurationSeconds { get; set; }
		[JsonProperty("stat")] public string Stat { get; set; }
		[JsonProperty("value")] public float? Value { get; set; }
		[JsonProperty("stackable")] public bool? Stackable { get; set; }
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

		[JsonProperty("extends")]
		public string Extends { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("visual", Required = Required.Always)]
		public UsableVisualDefinition Visual { get; set; }

		[JsonProperty("stats")]
		public UsableStatsDefinition Stats { get; set; }

		[JsonProperty("effectMode")]
		public string EffectMode { get; set; } = "inherit";

		[JsonProperty("effects")]
		public List<UsableEffectDefinition> Effects { get; set; } = new List<UsableEffectDefinition>();
	}

	public sealed class UsableDefinition
	{
		public ContentId Id { get; }
		public ContentId Extends { get; }
		public bool HasBaseTemplate { get; }
		public string PackId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public UsableVisualDefinition Visual { get; }
		public UsableStatsDefinition Stats { get; }
		public string EffectMode { get; }
		public IReadOnlyList<UsableEffectDefinition> Effects { get; }

		public UsableDefinition(ContentId i_id, ContentId i_extends, bool i_hasBaseTemplate, string i_packId, string i_source, UsableDefinitionDocument i_document)
		{
			Id = i_id;
			Extends = i_extends;
			HasBaseTemplate = i_hasBaseTemplate;
			PackId = i_packId;
			Source = i_source;
			DisplayName = i_document.DisplayName;
			Description = i_document.Description ?? string.Empty;
			Visual = i_document.Visual;
			Stats = i_document.Stats ?? new UsableStatsDefinition();
			EffectMode = i_document.EffectMode ?? "inherit";
			Effects = i_document.Effects ?? new List<UsableEffectDefinition>();
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
			bool hasBaseTemplate = !string.IsNullOrWhiteSpace(document.Extends);
			ContentId extends = default;
			if (hasBaseTemplate && (!ContentId.TryParse(document.Extends, out extends) || extends.Namespace != "core" || !extends.Path.StartsWith("item/usable/", StringComparison.Ordinal)))
				Error(result, "extends", "extends must reference a Core item/usable content ID when provided.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName)) Error(result, "display-name", "displayName is required.", i_source);
			ValidateVisual(document.Visual, result.Report, i_source);
			if (document.Visual != null && hasBaseTemplate && string.Equals(document.Visual.Type, "originalUsableSprites", StringComparison.Ordinal))
				Error(result, "extends", "originalUsableSprites definitions must omit extends.", i_source);
			if (document.Visual != null && !hasBaseTemplate && !string.Equals(document.Visual.Type, "originalUsableSprites", StringComparison.Ordinal))
				Error(result, "extends", "A usable without extends must use originalUsableSprites.", i_source);
			ValidateStats(document.Stats, result.Report, i_source);
			ValidateEffects(document.EffectMode, document.Effects, result.Report, i_source);
			if (!hasBaseTemplate && !string.Equals(document.EffectMode ?? "inherit", "replace", StringComparison.Ordinal))
				Error(result, "effect-mode", "A fully original usable must use effectMode 'replace'.", i_source);
			if (result.Report.IsValid) result.Definition = new UsableDefinition(id, extends, hasBaseTemplate, i_packId, i_source, document);
			return result;
		}

		internal static void ValidateEffects(string i_mode, List<UsableEffectDefinition> i_effects, ValidationReport io_report, string i_source)
		{
			string mode = i_mode ?? "inherit";
			if (mode != "inherit" && mode != "add" && mode != "replace")
				Error(io_report, "effect-mode", "effectMode must be inherit, add, or replace.", i_source);
			if (i_effects == null) return;
			if (i_effects.Count > 16) Error(io_report, "effects", "effects supports at most 16 entries.", i_source);
			HashSet<string> stats = new HashSet<string>(StringComparer.Ordinal)
			{
				"HealthMax", "SpeedAccel", "SpeedMax", "Traction", "DamageMultiplierGun", "SpeedSprint", "PowerJump", "PowerDash"
			};
			foreach (UsableEffectDefinition effect in i_effects)
			{
				if (effect == null) { Error(io_report, "effect", "effect entries cannot be null.", i_source); continue; }
				switch (effect.Type)
				{
				case "restoreHealth":
				case "restoreStrength":
				case "restoreStamina":
				case "reducePleasure":
				case "refillAmmo":
				case "gainMoney":
					ValidateRange(effect.Amount, 0.01f, 100000f, "effect-amount", io_report, i_source, true);
					break;
				case "fillAllAmmo":
				case "repairClothing":
					break;
				case "ragdoll":
				case "invulnerability":
					ValidateRange(effect.DurationSeconds, 0.01f, 600f, "effect-duration", io_report, i_source, true);
					break;
				case "statModifier":
					if (!stats.Contains(effect.Stat ?? string.Empty)) Error(io_report, "effect-stat", "statModifier uses an unsupported stat.", i_source);
					ValidateRange(effect.Value, -100000f, 100000f, "effect-value", io_report, i_source, true);
					ValidateRange(effect.DurationSeconds, 0.01f, 3600f, "effect-duration", io_report, i_source, true);
					break;
				default:
					Error(io_report, "effect-type", "Unsupported effect type: " + (effect.Type ?? "<null>"), i_source);
					break;
				}
			}
		}

		private static void ValidateRange(float? i_value, float i_min, float i_max, string i_code, ValidationReport io_report, string i_source, bool i_required)
		{
			if (!i_value.HasValue) { if (i_required) Error(io_report, i_code, i_code + " is required.", i_source); return; }
			float value = i_value.Value;
			if (float.IsNaN(value) || float.IsInfinity(value) || value < i_min || value > i_max)
				Error(io_report, i_code, i_code + " must be between " + i_min + " and " + i_max + ".", i_source);
		}

		internal static void ValidateStats(UsableStatsDefinition i_stats, ValidationReport io_report, string i_source)
		{
			if (i_stats == null) return;
			if (i_stats.Weight.HasValue && (i_stats.Weight < 0 || i_stats.Weight > 100000)) Error(io_report, "weight", "weight must be between 0 and 100000.", i_source);
			if (i_stats.Value.HasValue && (i_stats.Value < 0 || i_stats.Value > 100000000)) Error(io_report, "value", "value must be between 0 and 100000000.", i_source);
			if (i_stats.EquipSeconds.HasValue && (float.IsNaN(i_stats.EquipSeconds.Value) || float.IsInfinity(i_stats.EquipSeconds.Value) || i_stats.EquipSeconds < 0f || i_stats.EquipSeconds > 60f))
				Error(io_report, "equip-seconds", "equipSeconds must be between 0 and 60.", i_source);
			ValidateDescriptions(i_stats.GoodEffectDescriptions, "good-effect-descriptions", io_report, i_source);
			ValidateDescriptions(i_stats.BadEffectDescriptions, "bad-effect-descriptions", io_report, i_source);
		}

		private static void ValidateDescriptions(List<string> i_values, string i_field, ValidationReport io_report, string i_source)
		{
			if (i_values == null) return;
			if (i_values.Count > 32) Error(io_report, i_field, i_field + " supports at most 32 entries.", i_source);
			foreach (string value in i_values) if (string.IsNullOrWhiteSpace(value) || value.Length > 256)
				Error(io_report, i_field, i_field + " entries must contain 1 to 256 characters.", i_source);
		}

		private static void Error(ValidationReport io_report, string i_code, string i_message, string i_source)
		{
			io_report.Add(ValidationSeverity.Error, "usable.stats." + i_code, i_message, i_source);
		}

		private static void ValidateVisual(UsableVisualDefinition i_visual, ValidationReport io_report, string i_source)
		{
			if (i_visual == null)
			{
				io_report.Add(ValidationSeverity.Error, "usable.visual", "visual is required.", i_source);
				return;
			}
			if (!string.Equals(i_visual.Type, "coreUsableSprites", StringComparison.Ordinal) &&
				!string.Equals(i_visual.Type, "originalUsableSprites", StringComparison.Ordinal))
				io_report.Add(ValidationSeverity.Error, "usable.visual.type", "visual type must be 'coreUsableSprites' or 'originalUsableSprites'.", i_source);
			ValidatePng(i_visual.Icon, "icon", io_report, i_source);
			if (!string.IsNullOrEmpty(i_visual.World)) ValidatePng(i_visual.World, "world", io_report, i_source);
			if (float.IsNaN(i_visual.PixelsPerUnit) || float.IsInfinity(i_visual.PixelsPerUnit) || i_visual.PixelsPerUnit <= 0f || i_visual.PixelsPerUnit > 1024f)
				io_report.Add(ValidationSeverity.Error, "usable.visual.pixels-per-unit", "pixelsPerUnit must be greater than 0 and at most 1024.", i_source);
			if (float.IsNaN(i_visual.PivotX) || i_visual.PivotX < 0f || i_visual.PivotX > 1f || float.IsNaN(i_visual.PivotY) || i_visual.PivotY < 0f || i_visual.PivotY > 1f)
				io_report.Add(ValidationSeverity.Error, "usable.visual.pivot", "pivotX and pivotY must be between 0 and 1.", i_source);
			if (float.IsNaN(i_visual.ColliderWidth) || i_visual.ColliderWidth <= 0f || i_visual.ColliderWidth > 16f || float.IsNaN(i_visual.ColliderHeight) || i_visual.ColliderHeight <= 0f || i_visual.ColliderHeight > 16f)
				io_report.Add(ValidationSeverity.Error, "usable.visual.collider", "colliderWidth and colliderHeight must be greater than 0 and at most 16.", i_source);
			if (i_visual.SortingOrder < -1000 || i_visual.SortingOrder > 1000)
				io_report.Add(ValidationSeverity.Error, "usable.visual.sorting-order", "sortingOrder must be between -1000 and 1000.", i_source);
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
