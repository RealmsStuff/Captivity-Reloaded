using System.Linq;
using CaptivityReloaded.Modding;
using NUnit.Framework;

namespace CaptivityReloaded.Tests.Modding
{
	public sealed class EnemyAiAuthoringTests
	{
		[Test]
		public void EveryMovementPresetProducesValidRuntimeAi()
		{
			Assert.That(EnemyAiAuthoring.Presets.Count, Is.EqualTo(4));
			foreach (EnemyAiAuthoringPreset preset in EnemyAiAuthoring.Presets)
			{
				ValidationReport report = new ValidationReport();
				EnemyAiAuthoring.Validate(new EnemyAiDefinition
				{
					Type = preset.Type,
					PreferredRange = preset.PreferredRange,
					RetreatRange = preset.RetreatRange,
					ReactionSeconds = preset.ReactionSeconds
				}, report, preset.Id);

				Assert.That(report.IsValid, Is.True, string.Join("\n", report.Issues.Select(i_issue => i_issue.ToString()).ToArray()));
				Assert.That(preset.VisionRange, Is.GreaterThan(0f));
				Assert.That(preset.SpeedAcceleration, Is.GreaterThan(0f));
				Assert.That(preset.SpeedMax, Is.GreaterThan(0f));
			}
		}

		[Test]
		public void UnknownMovementBrainIsRejected()
		{
			ValidationReport report = new ValidationReport();
			EnemyAiAuthoring.Validate(new EnemyAiDefinition { Type = "teleportChase" }, report, "test");

			Assert.That(report.IsValid, Is.False);
			Assert.That(report.Issues.Any(i_issue => i_issue.Code == "enemy.ai.type"), Is.True);
		}

		[Test]
		public void RetreatRangeCannotExceedPreferredRange()
		{
			ValidationReport report = new ValidationReport();
			EnemyAiAuthoring.Validate(new EnemyAiDefinition
			{
				Type = "groundChase",
				PreferredRange = .5f,
				RetreatRange = 1f,
				ReactionSeconds = .1f
			}, report, "test");

			Assert.That(report.IsValid, Is.False);
			Assert.That(report.Issues.Any(i_issue => i_issue.Code == "enemy.ai.retreat-range"), Is.True);
		}

		[Test]
		public void ReactionTimeBelowRuntimeFloorIsRejected()
		{
			ValidationReport report = new ValidationReport();
			EnemyAiAuthoring.Validate(new EnemyAiDefinition
			{
				Type = "flyingChase",
				PreferredRange = 1f,
				RetreatRange = 0f,
				ReactionSeconds = .001f
			}, report, "test");

			Assert.That(report.IsValid, Is.False);
			Assert.That(report.Issues.Any(i_issue => i_issue.Code == "enemy.ai-reaction-seconds"), Is.True);
		}
	}
}
