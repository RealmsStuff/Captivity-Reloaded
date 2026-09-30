using System;
using System.Collections.Generic;
using System.Linq;

namespace CaptivityReloaded.Modding
{
	public sealed class EnemyAiAuthoringPreset
	{
		public string Id { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public string Type { get; }
		public float PreferredRange { get; }
		public float RetreatRange { get; }
		public float ReactionSeconds { get; }
		public float VisionRange { get; }
		public float SpeedAcceleration { get; }
		public float SpeedMax { get; }
		public float Traction { get; }

		public EnemyAiAuthoringPreset(string i_id, string i_displayName, string i_description, string i_type,
			float i_preferredRange, float i_retreatRange, float i_reactionSeconds, float i_visionRange,
			float i_speedAcceleration, float i_speedMax, float i_traction)
		{
			Id = i_id; DisplayName = i_displayName; Description = i_description; Type = i_type;
			PreferredRange = i_preferredRange; RetreatRange = i_retreatRange; ReactionSeconds = i_reactionSeconds;
			VisionRange = i_visionRange; SpeedAcceleration = i_speedAcceleration; SpeedMax = i_speedMax; Traction = i_traction;
		}
	}

	public static class EnemyAiAuthoring
	{
		private static readonly EnemyAiAuthoringPreset[] Values =
		{
			new EnemyAiAuthoringPreset("ground-assault", "Ground assault", "Pursues across platforms and holds close melee range.", "groundChase", .8f, 0f, .08f, 22f, 6f, 3.2f, .15f),
			new EnemyAiAuthoringPreset("ground-skirmisher", "Ground skirmisher", "Pursues and backs away when the player crowds it.", "groundChase", 1.15f, .65f, .12f, 26f, 5f, 2.8f, .15f),
			new EnemyAiAuthoringPreset("flying-assault", "Flying assault", "Moves directly through the air toward the player.", "flyingChase", 1f, .45f, .08f, 28f, 8f, 4f, .05f),
			new EnemyAiAuthoringPreset("stationary-guard", "Stationary guard", "Never pursues; attacks only when the player enters an attack's initiate range.", "holdPosition", 1f, 0f, .15f, 24f, 5f, 2.8f, .15f)
		};

		public static IReadOnlyList<EnemyAiAuthoringPreset> Presets => Values;
		public static EnemyAiAuthoringPreset GetPreset(string i_id)
		{
			return Values.FirstOrDefault(i_value => string.Equals(i_value.Id, i_id, StringComparison.Ordinal)) ?? Values[0];
		}

		public static void Validate(EnemyAiDefinition i_ai, ValidationReport io_report, string i_source)
		{
			if (io_report == null) return;
			if (i_ai == null || (i_ai.Type != "groundChase" && i_ai.Type != "flyingChase" && i_ai.Type != "holdPosition"))
			{
				io_report.Add(ValidationSeverity.Error, "enemy.ai.type", "Original enemy AI type must be groundChase, flyingChase, or holdPosition.", i_source);
				return;
			}
			Range(i_ai.PreferredRange, 0f, 100f, "enemy.ai-preferred-range", io_report, i_source);
			Range(i_ai.RetreatRange, 0f, 100f, "enemy.ai-retreat-range", io_report, i_source);
			if (i_ai.RetreatRange.HasValue && i_ai.PreferredRange.HasValue && i_ai.RetreatRange > i_ai.PreferredRange)
				io_report.Add(ValidationSeverity.Error, "enemy.ai.retreat-range", "retreatRange cannot be greater than preferredRange.", i_source);
			Range(i_ai.ReactionSeconds, .02f, 10f, "enemy.ai-reaction-seconds", io_report, i_source);
		}

		private static void Range(float? i_value, float i_min, float i_max, string i_code, ValidationReport io_report, string i_source)
		{
			if (i_value.HasValue && (float.IsNaN(i_value.Value) || float.IsInfinity(i_value.Value) || i_value.Value < i_min || i_value.Value > i_max))
				io_report.Add(ValidationSeverity.Error, i_code, "Value must be between " + i_min + " and " + i_max + ".", i_source);
		}
	}
}
