using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponStatsDefinition
	{
		[JsonProperty("damage")]
		public int? Damage { get; set; }

		[JsonProperty("ammoMax")]
		public int? AmmoMax { get; set; }

		[JsonProperty("magazineSize")]
		public int? MagazineSize { get; set; }

		[JsonProperty("bulletsPerShot")]
		public int? BulletsPerShot { get; set; }

		[JsonProperty("penetration")]
		public int? Penetration { get; set; }

		[JsonProperty("rangeMultiplier")]
		public float? RangeMultiplier { get; set; }

		[JsonProperty("fireIntervalSeconds")]
		public float? FireIntervalSeconds { get; set; }

		[JsonProperty("recoil")]
		public float? Recoil { get; set; }

		[JsonProperty("movementRecoil")]
		public float? MovementRecoil { get; set; }

		[JsonProperty("knockbackX")]
		public float? KnockbackX { get; set; }

		[JsonProperty("knockbackY")]
		public float? KnockbackY { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponVisualDefinition
	{
		[JsonProperty("type", Required = Required.Always)]
		public string Type { get; set; }

		[JsonProperty("pixelsPerUnit")]
		public float PixelsPerUnit { get; set; } = 32f;

		[JsonProperty("sprites", Required = Required.Always)]
		public Dictionary<string, string> Sprites { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponDefinitionDocument
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
		public WeaponStatsDefinition Stats { get; set; }

		[JsonProperty("visual", Required = Required.Always)]
		public WeaponVisualDefinition Visual { get; set; }
	}

	public sealed class WeaponDefinition
	{
		public ContentId Id { get; }
		public ContentId Extends { get; }
		public string PackId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public WeaponStatsDefinition Stats { get; }
		public WeaponVisualDefinition Visual { get; }

		public WeaponDefinition(ContentId i_id, ContentId i_extends, string i_packId, string i_source, WeaponDefinitionDocument i_document)
		{
			Id = i_id;
			Extends = i_extends;
			PackId = i_packId;
			Source = i_source;
			DisplayName = i_document.DisplayName;
			Description = i_document.Description ?? string.Empty;
			Stats = i_document.Stats ?? new WeaponStatsDefinition();
			Visual = i_document.Visual;
		}
	}

	public sealed class WeaponDefinitionLoadResult
	{
		public WeaponDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class WeaponDefinitionParser
	{
		public const int SupportedSchemaVersion = 1;
		private static readonly HashSet<string> PistolSpriteSlots = new HashSet<string>(StringComparer.Ordinal)
		{
			"body", "slide", "base"
		};

		public static WeaponDefinitionLoadResult Parse(string i_json, string i_packId, string i_source)
		{
			WeaponDefinitionLoadResult result = new WeaponDefinitionLoadResult();
			WeaponDefinitionDocument document;
			try
			{
				document = JsonConvert.DeserializeObject<WeaponDefinitionDocument>(i_json, new JsonSerializerSettings
				{
					MissingMemberHandling = MissingMemberHandling.Error
				});
			}
			catch (JsonException exception)
			{
				result.Report.Add(ValidationSeverity.Error, "weapon.json", exception.Message, i_source);
				return result;
			}
			if (document == null)
			{
				result.Report.Add(ValidationSeverity.Error, "weapon.null", "Weapon definition resolved to null.", i_source);
				return result;
			}

			if (document.SchemaVersion != SupportedSchemaVersion) Error(result, "schema-version", "Unsupported schemaVersion " + document.SchemaVersion + ".", i_source);
			if (!string.Equals(document.Type, "weapon", StringComparison.Ordinal)) Error(result, "type", "Definition type must be 'weapon'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != i_packId || !id.Path.StartsWith("item/weapon/", StringComparison.Ordinal))
				Error(result, "id", "Weapon ID must use the defining pack namespace and an item/weapon/ path.", i_source);
			if (!ContentId.TryParse(document.Extends, out ContentId extends) || extends != ContentId.Parse("core:item/weapon/pistol"))
				Error(result, "extends", "The first V1 weapon shape must extend core:item/weapon/pistol.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName)) Error(result, "display-name", "displayName is required.", i_source);
			ValidateStats(document.Stats, result.Report, i_source);
			ValidateVisual(document.Visual, result.Report, i_source);
			if (result.Report.IsValid) result.Definition = new WeaponDefinition(id, extends, i_packId, i_source, document);
			return result;
		}

		private static void ValidateStats(WeaponStatsDefinition i_stats, ValidationReport io_report, string i_source)
		{
			if (i_stats == null) return;
			ValidateRange(i_stats.Damage, 0, 10000, "damage", io_report, i_source);
			ValidateRange(i_stats.AmmoMax, 1, 100000, "ammo-max", io_report, i_source);
			ValidateRange(i_stats.MagazineSize, 1, 100000, "magazine-size", io_report, i_source);
			ValidateRange(i_stats.BulletsPerShot, 1, 64, "bullets-per-shot", io_report, i_source);
			ValidateRange(i_stats.Penetration, 0, 64, "penetration", io_report, i_source);
			ValidateRange(i_stats.RangeMultiplier, 0.05f, 1f, "range-multiplier", io_report, i_source);
			ValidateRange(i_stats.FireIntervalSeconds, 0.02f, 10f, "fire-interval-seconds", io_report, i_source);
			ValidateRange(i_stats.Recoil, 0f, 1f, "recoil", io_report, i_source);
			ValidateRange(i_stats.MovementRecoil, 0f, 1f, "movement-recoil", io_report, i_source);
			ValidateRange(i_stats.KnockbackX, 0f, 1000f, "knockback-x", io_report, i_source);
			ValidateRange(i_stats.KnockbackY, 0f, 1000f, "knockback-y", io_report, i_source);

			if (i_stats.AmmoMax.HasValue != i_stats.MagazineSize.HasValue)
				io_report.Add(ValidationSeverity.Error, "weapon.stats.ammo-pair", "ammoMax and magazineSize must be overridden together.", i_source);
			else if (i_stats.AmmoMax.HasValue && i_stats.MagazineSize.Value > i_stats.AmmoMax.Value)
				io_report.Add(ValidationSeverity.Error, "weapon.stats.magazine-size", "magazineSize cannot exceed ammoMax.", i_source);
		}

		private static void ValidateRange(int? i_value, int i_min, int i_max, string i_name, ValidationReport io_report, string i_source)
		{
			if (i_value.HasValue && (i_value.Value < i_min || i_value.Value > i_max))
				io_report.Add(ValidationSeverity.Error, "weapon.stats." + i_name, i_name + " must be between " + i_min + " and " + i_max + ".", i_source);
		}

		private static void ValidateRange(float? i_value, float i_min, float i_max, string i_name, ValidationReport io_report, string i_source)
		{
			if (!i_value.HasValue) return;
			float value = i_value.Value;
			if (float.IsNaN(value) || float.IsInfinity(value) || value < i_min || value > i_max)
				io_report.Add(ValidationSeverity.Error, "weapon.stats." + i_name, i_name + " must be between " + i_min + " and " + i_max + ".", i_source);
		}

		private static void ValidateVisual(WeaponVisualDefinition i_visual, ValidationReport io_report, string i_source)
		{
			if (i_visual == null)
			{
				io_report.Add(ValidationSeverity.Error, "weapon.visual", "visual is required.", i_source);
				return;
			}
			if (!string.Equals(i_visual.Type, "coreWeaponSprites", StringComparison.Ordinal))
				io_report.Add(ValidationSeverity.Error, "weapon.visual.type", "V1 visual type must be 'coreWeaponSprites'.", i_source);
			if (float.IsNaN(i_visual.PixelsPerUnit) || float.IsInfinity(i_visual.PixelsPerUnit) || i_visual.PixelsPerUnit <= 0f || i_visual.PixelsPerUnit > 1024f)
				io_report.Add(ValidationSeverity.Error, "weapon.visual.pixels-per-unit", "pixelsPerUnit must be greater than 0 and at most 1024.", i_source);
			if (i_visual.Sprites == null || i_visual.Sprites.Count == 0)
			{
				io_report.Add(ValidationSeverity.Error, "weapon.visual.sprites", "At least one weapon sprite is required.", i_source);
				return;
			}
			foreach (KeyValuePair<string, string> sprite in i_visual.Sprites)
			{
				if (!PistolSpriteSlots.Contains(sprite.Key))
					io_report.Add(ValidationSeverity.Error, "weapon.visual.slot", "Unknown V1 pistol sprite slot: " + sprite.Key, i_source);
				if (!ModPath.IsSafeRelativePath(sprite.Value) || !sprite.Value.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
					io_report.Add(ValidationSeverity.Error, "weapon.visual.asset-path", "Weapon sprites must use safe pack-relative PNG paths: " + sprite.Value, i_source);
			}
		}

		private static void Error(WeaponDefinitionLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "weapon." + i_code, i_message, i_source);
		}
	}
}
