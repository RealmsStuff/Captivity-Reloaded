using System.Linq;
using NUnit.Framework;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class AnimationBlendTimeTests
	{
		private const string Enemy = @"{
  'schemaVersion': 1, 'type': 'enemy', 'id': 'example.enemies:enemy/blender', 'displayName': 'Blender',
  'stats': { 'healthMax': 10, 'speedAcceleration': 4, 'speedMax': 3, 'traction': 0.2 },
  'spawn': { 'inheritTemplateSpawners': false },
  'behavior': { 'modules': [
    { 'type': 'downedFinisher', 'triggerRange': 2, 'meterMax': 100, 'inputPower': 10, 'phaseBlendSeconds': 0.2,
      'phases': [
        { 'id': 'hold', 'durationSeconds': 2, 'animation': 'hold' },
        { 'id': 'turn', 'durationSeconds': 3, 'animation': 'turn', 'blendSeconds': 0.5 }
      ] }
  ] },
  'ai': { 'type': 'groundChase', 'preferredRange': 1, 'reactionSeconds': 0.1 },
  'attacks': [{ 'id': 'hit', 'type': 'melee', 'animation': 'idle', 'chance': 1, 'damage': 1,
    'cooldownSeconds': 1, 'initiateRange': 1, 'hitRange': 1, 'durationSeconds': 0.5, 'hitTimeSeconds': 0.2 }],
  'animation': { 'blendSeconds': 0.15, 'clips': {
    'idle': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }] },
    'move': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'x': 0 } } }] },
    'hold': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'rotation': 10 } } }] },
    'turn': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'rotation': -80 } } }] }
  } },
  'visual': { 'type': 'originalSkeletonAtlas', 'atlas': 'assets/blender.png', 'pixelsPerUnit': 32,
    'bodyWidth': 1, 'bodyHeight': 2, 'regions': { 'hips': { 'x': 0, 'y': 0, 'width': 16, 'height': 16 } },
    'bones': [{ 'id': 'hips', 'region': 'hips' }], 'hitZones': [{ 'bone': 'hips', 'shape': 'box', 'width': 1, 'height': 1 }]
  }
}";

		private static EnemyDefinitionLoadResult Parse(string i_json)
		{
			return EnemyDefinitionParser.Parse(i_json, "example.enemies", "blender.json");
		}

		private static string Issues(EnemyDefinitionLoadResult i_result)
		{
			return string.Join("\n", i_result.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray());
		}

		[Test]
		public void Parse_AcceptsClipAndPhaseBlendTimes()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy);
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			Assert.That(result.Definition.Animation.BlendSeconds, Is.EqualTo(0.15f));
			EnemyBehaviorModuleDefinition finisher = result.Definition.Behavior.Modules.Single(i_module => i_module.Type == "downedFinisher");
			Assert.That(finisher.PhaseBlendSeconds, Is.EqualTo(0.2f));
			Assert.That(finisher.Phases.Select(i_phase => i_phase.BlendSeconds), Is.EqualTo(new float?[] { null, 0.5f }),
				"A phase without blendSeconds keeps the finisher's and the clips' own blending.");
		}

		[Test]
		public void Parse_LeavesBlendTimesUnsetWhenOmitted()
		{
			string json = Enemy.Replace("'blendSeconds': 0.15, ", string.Empty).Replace(", 'phaseBlendSeconds': 0.2", string.Empty)
				.Replace(", 'blendSeconds': 0.5", string.Empty);
			EnemyDefinitionLoadResult result = Parse(json);
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			Assert.That(result.Definition.Animation.BlendSeconds, Is.Null);
			EnemyBehaviorModuleDefinition finisher = result.Definition.Behavior.Modules.Single(i_module => i_module.Type == "downedFinisher");
			Assert.That(finisher.PhaseBlendSeconds, Is.Null);
			Assert.That(finisher.Phases.All(i_phase => i_phase.BlendSeconds == null), Is.True);
		}

		[TestCase("'blendSeconds': 0.15", "'blendSeconds': 1.5", "enemy.stats.animation-blend")]
		[TestCase("'phaseBlendSeconds': 0.2", "'phaseBlendSeconds': -1", "enemy.stats.finisher-phase-blend")]
		[TestCase("'blendSeconds': 0.5", "'blendSeconds': 3", "enemy.stats.finisher-phase-entry-blend")]
		public void Parse_RejectsOutOfRangeBlendTimes(string i_find, string i_replace, string i_code)
		{
			Assert.That(Enemy.Contains(i_find), Is.True, "Fixture no longer contains: " + i_find);
			EnemyDefinitionLoadResult result = Parse(Enemy.Replace(i_find, i_replace));
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == i_code), Is.True, Issues(result));
		}
	}
}
