using System.Collections.Generic;
using System.Linq;
using CaptivityReloaded.Modding;
using NUnit.Framework;

namespace CaptivityReloaded.Tests.Modding
{
	public sealed class EnemyAttackAuthoringTests
	{
		[Test]
		public void RelativeWeightsDoNotNeedToSumToOne()
		{
			ValidationReport report = new ValidationReport();
			EnemyAttackAuthoring.ValidateStrategy(new List<EnemyAttackDefinition>
			{
				Attack("melee", 3f, 1.5f), Attack("projectile", 1f, 8f)
			}, new EnemyAiDefinition { Type = "groundChase", PreferredRange = 1f }, report, "test");

			Assert.That(report.IsValid, Is.True);
		}

		[Test]
		public void ZeroTotalSelectionWeightIsRejected()
		{
			ValidationReport report = new ValidationReport();
			EnemyAttackAuthoring.ValidateStrategy(new List<EnemyAttackDefinition> { Attack("melee", 0f, 2f) },
				new EnemyAiDefinition { Type = "groundChase", PreferredRange = 1f }, report, "test");

			Assert.That(report.Issues.Any(i_issue => i_issue.Code == "enemy.attacks.weight-total"), Is.True);
		}

		[Test]
		public void PursuitCannotStopBeyondEveryAttackRange()
		{
			ValidationReport report = new ValidationReport();
			EnemyAttackAuthoring.ValidateStrategy(new List<EnemyAttackDefinition> { Attack("melee", 1f, 1.5f) },
				new EnemyAiDefinition { Type = "groundChase", PreferredRange = 3f }, report, "test");

			Assert.That(report.Issues.Any(i_issue => i_issue.Code == "enemy.ai.attack-range-gap"), Is.True);
		}

		[Test]
		public void HoldPositionMayIntentionallyWaitOutsideAttackRange()
		{
			ValidationReport report = new ValidationReport();
			EnemyAttackAuthoring.ValidateStrategy(new List<EnemyAttackDefinition> { Attack("area", 1f, 2f) },
				new EnemyAiDefinition { Type = "holdPosition", PreferredRange = 20f }, report, "test");

			Assert.That(report.IsValid, Is.True);
		}

		private static EnemyAttackDefinition Attack(string i_type, float i_weight, float i_range)
		{
			return new EnemyAttackDefinition { Id = i_type, Type = i_type, Chance = i_weight, InitiateRange = i_range };
		}
	}
}
