using System.Linq;
using NUnit.Framework;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class AttackMissAnimationTests
	{
		private const string Enemy = @"{
  'schemaVersion': 1, 'type': 'enemy', 'id': 'example.enemies:enemy/leaper', 'displayName': 'Leaper',
  'stats': { 'healthMax': 50, 'speedAcceleration': 4, 'speedMax': 3, 'traction': 0.2 },
  'spawn': { 'inheritTemplateSpawners': false },
  'ai': { 'type': 'groundChase', 'preferredRange': 1, 'retreatRange': 0.5, 'reactionSeconds': 0.1 },
  'attacks': [
    { 'id': 'leap', 'type': 'melee', 'animation': 'leap', 'missAnimation': 'land', 'chance': 1, 'damage': 6,
      'cooldownSeconds': 3, 'initiateRange': 2.5, 'hitRange': 1, 'durationSeconds': 1 },
    { 'id': 'swipe', 'type': 'melee', 'animation': 'swipe', 'chance': 1, 'damage': 4,
      'cooldownSeconds': 2, 'initiateRange': 1.2, 'hitRange': 1, 'durationSeconds': 0.6, 'hitTimeSeconds': 0.3 }
  ],
  'animation': { 'clips': {
    'idle': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }] },
    'move': { 'durationSeconds': 0.5, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'x': 0 } } }] },
    'leap': { 'durationSeconds': 1, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }], 'events': [{ 'time': 0.5, 'type': 'attackHit' }] },
    'land': { 'durationSeconds': 1.4, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }, { 'time': 1.4, 'bones': { 'hips': { 'y': -0.2 } } }] },
    'swipe': { 'durationSeconds': 0.6, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }] }
  } },
  'visual': { 'type': 'originalSkeletonAtlas', 'atlas': 'assets/leaper.png', 'pixelsPerUnit': 32,
    'bodyWidth': 0.8, 'bodyHeight': 2,
    'regions': { 'hips': { 'x': 0, 'y': 0, 'width': 32, 'height': 32 } },
    'bones': [{ 'id': 'hips', 'region': 'hips' }],
    'hitZones': [{ 'bone': 'hips', 'shape': 'box', 'width': 0.8, 'height': 0.8 }]
  }
}";

		private static EnemyDefinitionLoadResult Parse(string i_json)
		{
			return EnemyDefinitionParser.Parse(i_json, "example.enemies", "leaper.json");
		}

		private static string Issues(EnemyDefinitionLoadResult i_result)
		{
			return string.Join("\n", i_result.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray());
		}

		[Test]
		public void Parse_AcceptsMissAnimation()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy);
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			Assert.That(result.Definition.Attacks.Single(i_attack => i_attack.Id == "leap").MissAnimation, Is.EqualTo("land"));
			Assert.That(result.Definition.Attacks.Single(i_attack => i_attack.Id == "swipe").MissAnimation, Is.Null,
				"Only the attacks that name a miss clip get one.");
		}

		[TestCase("'missAnimation': 'land'", "'missAnimation': 'stumble'")]
		[TestCase("'missAnimation': 'land'", "'missAnimation': 'Land Hard'")]
		public void Parse_RejectsUnknownMissAnimation(string i_find, string i_replace)
		{
			Assert.That(Enemy.Contains(i_find), Is.True, "Fixture no longer contains: " + i_find);
			EnemyDefinitionLoadResult result = Parse(Enemy.Replace(i_find, i_replace));
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "enemy.attacks.miss-animation-missing"), Is.True, Issues(result));
		}
	}
}
