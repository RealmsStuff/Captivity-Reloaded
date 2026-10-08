using System.Linq;
using NUnit.Framework;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class AttackKnockdownTests
	{
		private const string Enemy = @"{
  'schemaVersion': 1, 'type': 'enemy', 'id': 'example.enemies:enemy/boxer', 'displayName': 'Boxer',
  'stats': { 'healthMax': 50, 'speedAcceleration': 4, 'speedMax': 3, 'traction': 0.2 },
  'spawn': { 'inheritTemplateSpawners': false },
  'ai': { 'type': 'groundChase', 'preferredRange': 1, 'retreatRange': 0.5, 'reactionSeconds': 0.1 },
  'attacks': [
    { 'id': 'haymaker', 'type': 'melee', 'animation': 'swing', 'chance': 1, 'damage': 14, 'ragdollSeconds': 2.5,
      'cooldownSeconds': 6, 'initiateRange': 1.5, 'hitRange': 1.3, 'durationSeconds': 1 },
    { 'id': 'combo', 'type': 'multiStage', 'animation': 'combo', 'chance': 1, 'damage': 4,
      'cooldownSeconds': 3, 'initiateRange': 1.4, 'hitRange': 1.25, 'durationSeconds': 1,
      'stages': [{ 'type': 'melee' }, { 'type': 'melee', 'damage': 9, 'ragdollSeconds': 1 }] }
  ],
  'animation': { 'clips': {
    'idle': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }] },
    'move': { 'durationSeconds': 0.5, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'x': 0 } } }] },
    'swing': { 'durationSeconds': 1, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }], 'events': [{ 'time': 0.6, 'type': 'attackHit' }] },
    'combo': { 'durationSeconds': 1, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }],
      'events': [{ 'time': 0.3, 'type': 'attackHit' }, { 'time': 0.7, 'type': 'attackHit' }] }
  } },
  'visual': { 'type': 'originalSkeletonAtlas', 'atlas': 'assets/boxer.png', 'pixelsPerUnit': 32,
    'bodyWidth': 0.8, 'bodyHeight': 2,
    'regions': { 'hips': { 'x': 0, 'y': 0, 'width': 32, 'height': 32 } },
    'bones': [{ 'id': 'hips', 'region': 'hips' }],
    'hitZones': [{ 'bone': 'hips', 'shape': 'box', 'width': 0.8, 'height': 0.8 }]
  }
}";

		private static EnemyDefinitionLoadResult Parse(string i_json)
		{
			return EnemyDefinitionParser.Parse(i_json, "example.enemies", "boxer.json");
		}

		private static string Issues(EnemyDefinitionLoadResult i_result)
		{
			return string.Join("\n", i_result.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray());
		}

		[Test]
		public void Parse_AcceptsPerAttackAndPerStageKnockdown()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy);
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			Assert.That(result.Definition.Attacks.Single(i_attack => i_attack.Id == "haymaker").RagdollSeconds, Is.EqualTo(2.5f));
			EnemyAttackDefinition combo = result.Definition.Attacks.Single(i_attack => i_attack.Id == "combo");
			Assert.That(combo.RagdollSeconds, Is.Null, "Only the attacks that ask for a knockdown get one.");
			Assert.That(combo.Stages.Select(i_stage => i_stage.RagdollSeconds), Is.EqualTo(new float?[] { null, 1f }));
		}

		[TestCase("'ragdollSeconds': 2.5", "'ragdollSeconds': 31", "enemy.stats.attack-ragdoll")]
		[TestCase("'ragdollSeconds': 2.5", "'ragdollSeconds': -1", "enemy.stats.attack-ragdoll")]
		[TestCase("'ragdollSeconds': 1 }", "'ragdollSeconds': 40 }", "enemy.stats.attack-stage-ragdoll")]
		public void Parse_RejectsOutOfRangeKnockdown(string i_find, string i_replace, string i_code)
		{
			Assert.That(Enemy.Contains(i_find), Is.True, "Fixture no longer contains: " + i_find);
			EnemyDefinitionLoadResult result = Parse(Enemy.Replace(i_find, i_replace));
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == i_code), Is.True, Issues(result));
		}
	}
}
