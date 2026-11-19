using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CaptivityReloaded.Modding.Tests
{
	// The enemy animation event hitStop: the game holds still for a few frames on a big hit.
	public sealed class AnimationHitStopTests
	{
		// A one-shot clip carrying only the event under test.
		private static NormalizedEnemyAnimationLoadResult Parse(JObject i_event, bool i_loop = false)
		{
			JObject document = new JObject
			{
				["schemaVersion"] = 1, ["type"] = "enemyAnimation", ["id"] = "example.hitstop:enemy-animation/brute/slam",
				["enemy"] = "example.hitstop:enemy/brute", ["displayName"] = "Slam", ["durationSeconds"] = 1,
				["frameRate"] = 60, ["loop"] = i_loop,
				["tracks"] = new JArray(new JObject { ["target"] = "bone/hips", ["property"] = "rotation.z",
					["keys"] = new JArray(new JObject { ["time"] = 0, ["value"] = 0 }, new JObject { ["time"] = 1, ["value"] = 10 }) }),
				["events"] = new JArray(i_event)
			};
			return NormalizedEnemyAnimationParser.Parse(document.ToString(), "example.hitstop", "slam.enemy-animation.json");
		}

		private static string Issues(NormalizedEnemyAnimationLoadResult i_result)
		{
			return string.Join("\n", i_result.Report.Issues.Select(i_issue => i_issue.ToString()).ToArray());
		}

		[Test]
		public void Parse_AcceptsAHitStop()
		{
			NormalizedEnemyAnimationLoadResult result = Parse(new JObject { ["time"] = 0.1f, ["type"] = "hitStop", ["durationSeconds"] = 0.05f });
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
		}

		[Test]
		public void Parse_AcceptsTheLongestHold()
		{
			NormalizedEnemyAnimationLoadResult result = Parse(new JObject { ["time"] = 0.1f, ["type"] = "hitStop", ["durationSeconds"] = 0.25f });
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
		}

		[TestCase(0f)]
		[TestCase(-0.1f)]
		[TestCase(0.3f)]
		public void Parse_RejectsAHoldThatIsNotAFewFrames(float i_seconds)
		{
			NormalizedEnemyAnimationLoadResult result = Parse(new JObject { ["time"] = 0.1f, ["type"] = "hitStop", ["durationSeconds"] = i_seconds });
			Assert.That(result.Report.Issues.Select(i_issue => i_issue.Code), Does.Contain("enemy-animation.event-hit-stop"), Issues(result));
		}

		[Test]
		public void Parse_RejectsAHitStopWithoutALength()
		{
			NormalizedEnemyAnimationLoadResult result = Parse(new JObject { ["time"] = 0.1f, ["type"] = "hitStop" });
			Assert.That(result.Report.Issues.Select(i_issue => i_issue.Code), Does.Contain("enemy-animation.event-hit-stop"), Issues(result));
		}

		[Test]
		public void Parse_RejectsAnotherEventsFieldsOnAHitStop()
		{
			NormalizedEnemyAnimationLoadResult result = Parse(new JObject { ["time"] = 0.1f, ["type"] = "hitStop", ["durationSeconds"] = 0.05f, ["amount"] = 0.5f });
			Assert.That(result.Report.Issues.Select(i_issue => i_issue.Code), Does.Contain("enemy-animation.event-fields"), Issues(result));
		}

		// Like the other gameplay events, a hold would fire on every loop.
		[Test]
		public void Parse_RejectsAHitStopOnALoop()
		{
			NormalizedEnemyAnimationLoadResult result = Parse(new JObject { ["time"] = 0.1f, ["type"] = "hitStop", ["durationSeconds"] = 0.05f }, true);
			Assert.That(result.Report.Issues.Select(i_issue => i_issue.Code), Does.Contain("enemy-animation.event-loop"), Issues(result));
		}

		// The validator's bound and the runtime's cap are the same number.
		[Test]
		public void TheRuntimeCap_MatchesTheValidator()
		{
			Type hitStop = Type.GetType("ModularHitStop, Assembly-CSharp");
			Assert.That(hitStop, Is.Not.Null, "The hit stop component is missing from the game assembly.");
			FieldInfo maximum = hitStop.GetField("MaximumSeconds", BindingFlags.Public | BindingFlags.Static);
			Assert.That(maximum, Is.Not.Null);
			Assert.That((float)maximum.GetValue(null), Is.EqualTo(0.25f));
		}
	}
}
