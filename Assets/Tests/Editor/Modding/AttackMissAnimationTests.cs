using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

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

		// The review case: the player is in range but can't be attacked right now (invulnerable).
		// No damage lands, so the hit check must report a miss and the miss clip plays. The full melee path also needs a
		// player skeleton for its range check, so this calls the hit step it returns directly.
		[Test]
		public void BlockedHit_ReportsAMiss()
		{
			System.Type enemyType = System.Type.GetType("ModularEnemy, Assembly-CSharp");
			System.Type playerType = System.Type.GetType("Player, Assembly-CSharp");
			Assert.That(enemyType, Is.Not.Null);
			Assert.That(playerType, Is.Not.Null);
			MethodInfo configuredHit = enemyType.GetMethod("ApplyConfiguredHit", BindingFlags.Instance | BindingFlags.NonPublic);
			MethodInfo projectileHit = enemyType.GetMethod("ApplyProjectileHit", BindingFlags.Instance | BindingFlags.Public);
			Assert.That(configuredHit, Is.Not.Null);
			Assert.That(projectileHit, Is.Not.Null);
			Assert.That(configuredHit.ReturnType, Is.EqualTo(typeof(bool)), "The melee, hitscan and area checks return whether the hit landed.");
			Assert.That(projectileHit.ReturnType, Is.EqualTo(typeof(bool)));

			EnemyDefinitionLoadResult result = Parse(Enemy);
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			EnemyAttackDefinition leap = result.Definition.Attacks.Single(i_attack => i_attack.Id == "leap");

			GameObject enemyObject = new GameObject("miss-test enemy");
			GameObject playerObject = new GameObject("miss-test player");
			try
			{
				Component enemy = enemyObject.AddComponent(enemyType);
				Component player = playerObject.AddComponent(playerType);
				Assert.That((bool)playerType.GetMethod("GetIsCanBeAttacked").Invoke(player, null), Is.False,
					"Fixture: an un-started player can't be attacked.");
				Assert.That((bool)configuredHit.Invoke(enemy, new object[] { player, leap, null }), Is.False,
					"A hit on a player who can't be attacked is a miss.");
				Assert.That((bool)projectileHit.Invoke(enemy, new object[] { player, 6f, 0f, 0f, 0f }), Is.False);
			}
			finally
			{
				Object.DestroyImmediate(enemyObject);
				Object.DestroyImmediate(playerObject);
			}
		}
	}
}
