using System.Collections.Generic;
using System.Linq;

namespace CaptivityReloaded.Modding
{
	public static class EnemyAttackAuthoring
	{
		public static readonly string[] DeliveryTypes = { "melee", "hitscan", "projectile", "area", "grab", "multiStage" };

		public static void ValidateStrategy(IReadOnlyList<EnemyAttackDefinition> i_attacks, EnemyAiDefinition i_ai,
			ValidationReport io_report, string i_source)
		{
			if (io_report == null || i_attacks == null || i_attacks.Count == 0) return;
			List<EnemyAttackDefinition> attacks = i_attacks.Where(i_attack => i_attack != null).ToList();
			if (attacks.Count == 0) return;

			float totalWeight = attacks.Where(i_attack => i_attack.Chance.HasValue).Sum(i_attack => i_attack.Chance.Value);
			if (totalWeight <= 0f)
				io_report.Add(ValidationSeverity.Error, "enemy.attacks.weight-total", "At least one attack must have a selection weight greater than zero.", i_source);

			if (i_ai == null || i_ai.Type == "holdPosition" || !i_ai.PreferredRange.HasValue) return;
			float maximumInitiateRange = attacks.Where(i_attack => i_attack.InitiateRange.HasValue)
				.Select(i_attack => i_attack.InitiateRange.Value).DefaultIfEmpty(0f).Max();
			if (i_ai.PreferredRange.Value > maximumInitiateRange)
				io_report.Add(ValidationSeverity.Error, "enemy.ai.attack-range-gap",
					"preferredRange cannot exceed every attack's initiateRange or the enemy can stop outside attack range.", i_source);
		}
	}
}
