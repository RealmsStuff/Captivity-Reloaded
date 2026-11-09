using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class ModularFinisherReactionsTests
	{
		private const string Enemy = @"{
  'schemaVersion': 1, 'type': 'enemy', 'id': 'example.enemies:enemy/breeder', 'displayName': 'Breeder',
  'stats': { 'healthMax': 50, 'speedAcceleration': 4, 'speedMax': 3, 'traction': 0.2 },
  'spawn': { 'inheritTemplateSpawners': false },
  'behavior': { 'modules': [{ 'type': 'downedFinisher', 'triggerRange': 1.5, 'meterMax': 50, 'inputPower': 1.5,
    'phases': [
      { 'id': 'pin', 'durationSeconds': 4, 'animation': 'idle',
        'reactions': { 'oral': true, 'mutePlayer': false, 'camera': 'enemyHips',
          'thrust': { 'pleasure': 0.1, 'libido': 0.2, 'struggleDamage': 4, 'sounds': ['assets/thrust-1.ogg', 'assets/thrust-2.wav'], 'volume': 0.8 },
          'cumThrust': { 'pleasure': 0.3, 'libido': 0.1, 'sounds': ['assets/cum.ogg'] },
          'playerVoice': { 'chance': 0.33, 'sounds': ['assets/moan.ogg'] },
          'enemyVoice': { 'chance': 0.2, 'sounds': ['assets/grunt.ogg'] },
          'layers': [{ 'chance': 0.5, 'sounds': ['assets/slap.ogg'] }],
          'uniqueSounds': ['assets/gulp.ogg'] } },
      { 'id': 'climax', 'durationSeconds': 2, 'animation': 'move' }
    ] }] },
  'ai': { 'type': 'groundChase', 'preferredRange': 1, 'retreatRange': 0.5, 'reactionSeconds': 0.1 },
  'attacks': [{ 'id': 'swipe', 'type': 'melee', 'animation': 'idle', 'hitTimeSeconds': 0.2,
    'chance': 1, 'damage': 8, 'cooldownSeconds': 1, 'initiateRange': 1.5, 'hitRange': 1.2, 'durationSeconds': 0.5 }],
  'animation': { 'clips': {
    'idle': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }] },
    'move': { 'durationSeconds': 0.5, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'x': 0 } } }] }
  } },
  'visual': { 'type': 'originalSkeletonAtlas', 'atlas': 'assets/breeder.png', 'pixelsPerUnit': 32,
    'bodyWidth': 0.8, 'bodyHeight': 2,
    'regions': { 'hips': { 'x': 0, 'y': 0, 'width': 32, 'height': 32 } },
    'bones': [{ 'id': 'hips', 'region': 'hips' }],
    'hitZones': [{ 'bone': 'hips', 'shape': 'box', 'width': 0.8, 'height': 0.8 }]
  }
}";

		private static EnemyDefinitionLoadResult Parse(string i_json)
		{
			return EnemyDefinitionParser.Parse(i_json, "example.enemies", "breeder.json");
		}

		private static string Issues(EnemyDefinitionLoadResult i_result)
		{
			return string.Join("\n", i_result.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray());
		}

		[Test]
		public void Parse_AcceptsPhaseReactions()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy);
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			EnemyBehaviorModuleDefinition module = result.Definition.Behavior.Modules.Single();
			EnemyFinisherReactionsDefinition reactions = module.Phases[0].Reactions;
			Assert.That(reactions.Oral, Is.True);
			Assert.That(reactions.Camera, Is.EqualTo("enemyHips"));
			Assert.That(reactions.Thrust.Libido, Is.EqualTo(0.2f));
			Assert.That(reactions.Thrust.StruggleDamage, Is.EqualTo(4f));
			Assert.That(reactions.Thrust.Sounds, Is.EqualTo(new[] { "assets/thrust-1.ogg", "assets/thrust-2.wav" }));
			Assert.That(reactions.PlayerVoice.Chance, Is.EqualTo(0.33f));
			Assert.That(reactions.Layers.Single().Sounds, Is.EqualTo(new[] { "assets/slap.ogg" }));
			Assert.That(module.Phases[1].Reactions, Is.Null);
		}

		[TestCase("'camera': 'enemyHips'", "'camera': 'enemyHead'", "enemy.behavior.finisher-reactions-camera")]
		[TestCase("'chance': 0.33", "'chance': 1.5", "enemy.stats.finisher-reactions-player-voice-chance")]
		[TestCase("'pleasure': 0.1", "'pleasure': 11", "enemy.stats.finisher-reactions-thrust-pleasure")]
		[TestCase("'libido': 0.1, 'sounds'", "'libido': -1, 'sounds'", "enemy.stats.finisher-reactions-cum-thrust-libido")]
		[TestCase("'volume': 0.8", "'volume': 2", "enemy.stats.finisher-reactions-thrust-volume")]
		[TestCase("'assets/thrust-1.ogg'", "'../thrust-1.ogg'", "enemy.behavior.finisher-reactions-thrust-sound")]
		[TestCase("'assets/moan.ogg'", "'assets/moan.mp3'", "enemy.behavior.finisher-reactions-player-voice-sound")]
		[TestCase("'assets/gulp.ogg'", "'C:/gulp.ogg'", "enemy.behavior.finisher-reactions-unique-sound")]
		public void Parse_RejectsInvalidReactions(string i_find, string i_replace, string i_code)
		{
			Assert.That(Enemy.Contains(i_find), Is.True, "Fixture no longer contains: " + i_find);
			EnemyDefinitionLoadResult result = Parse(Enemy.Replace(i_find, i_replace));
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == i_code), Is.True, Issues(result));
		}

		[Test]
		public void Parse_LimitsLayersAndSoundLists()
		{
			string layer = "{ 'chance': 0.5, 'sounds': ['assets/slap.ogg'] }";
			string nineLayers = string.Join(", ", Enumerable.Repeat(layer, 9).ToArray());
			EnemyDefinitionLoadResult layers = Parse(Enemy.Replace("'layers': [" + layer + "]", "'layers': [" + nineLayers + "]"));
			Assert.That(layers.Report.Issues.Any(i_issue => i_issue.Code == "enemy.behavior.finisher-reactions-layers"), Is.True, Issues(layers));
			string sounds = string.Join(", ", Enumerable.Range(0, 33).Select(i_index => "'assets/gulp-" + i_index + ".ogg'").ToArray());
			EnemyDefinitionLoadResult unique = Parse(Enemy.Replace("'uniqueSounds': ['assets/gulp.ogg']", "'uniqueSounds': [" + sounds + "]"));
			Assert.That(unique.Report.Issues.Any(i_issue => i_issue.Code == "enemy.behavior.finisher-reactions-unique-sound-count"), Is.True, Issues(unique));
		}

		[Test]
		public void Shake_MatchesRaperThrustAndCumThrust()
		{
			Assert.That(Enumerable.Range(0, 4).Select(i_power => FinisherReactions.Shake(i_power, false)), Is.EqualTo(new[] { 0.15f, 0.25f, 0.6f, 0.75f }));
			Assert.That(Enumerable.Range(0, 4).Select(i_power => FinisherReactions.Shake(i_power, true)), Is.EqualTo(new[] { 0.3f, 0.5f, 0.75f, 1f }));
			Assert.That(FinisherReactions.ShakeSeconds, Is.EqualTo(0.4f));
		}

		[Test]
		public void Power_RoundsAndClampsTheCueAmount()
		{
			Assert.That(FinisherReactions.Power(null), Is.EqualTo(0));
			Assert.That(FinisherReactions.Power(2f), Is.EqualTo(2));
			Assert.That(FinisherReactions.Power(2.6f), Is.EqualTo(3));
			Assert.That(FinisherReactions.Power(9f), Is.EqualTo(3));
			Assert.That(FinisherReactions.Power(-1f), Is.EqualTo(0));
			Assert.That(FinisherReactions.Power(float.NaN), Is.EqualTo(0));
		}

		[Test]
		public void Gains_MatchRaperPlayerGainPleasure()
		{
			EnemyFinisherThrustDefinition thrust = new EnemyFinisherThrustDefinition { Pleasure = 0.1f, Libido = 0.2f };
			EnemyFinisherThrustDefinition cum = new EnemyFinisherThrustDefinition { Pleasure = 0.3f, Libido = 0.05f };
			// PlayerGainPleasure: libido x 10, pleasure x 2, strength (libido + pleasure) x 5.
			FinisherReactions.Gains plain = FinisherReactions.GetGains(thrust, null);
			Assert.That(plain.Libido, Is.EqualTo(2f).Within(0.0001f));
			Assert.That(plain.Pleasure, Is.EqualTo(0.2f).Within(0.0001f));
			Assert.That(plain.Strength, Is.EqualTo(1.5f).Within(0.0001f));
			// PlayerGainCumPleasure adds the cum thrust's additional powers to each.
			FinisherReactions.Gains cumming = FinisherReactions.GetGains(thrust, cum);
			Assert.That(cumming.Libido, Is.EqualTo(2.5f).Within(0.0001f));
			Assert.That(cumming.Pleasure, Is.EqualTo(0.8f).Within(0.0001f));
			Assert.That(cumming.Strength, Is.EqualTo(3.25f).Within(0.0001f));
			FinisherReactions.Gains none = FinisherReactions.GetGains(null, null);
			Assert.That(new[] { none.Libido, none.Pleasure, none.Strength }, Is.EqualTo(new[] { 0f, 0f, 0f }));
			Assert.That(FinisherReactions.StruggleDamage(new EnemyFinisherThrustDefinition { StruggleDamage = 4f }, new EnemyFinisherThrustDefinition { StruggleDamage = 1f }), Is.EqualTo(5f));
		}

		[Test]
		public void TryPick_RollsTheChanceThenPicksASound()
		{
			List<string> sounds = new List<string> { "a.ogg", "b.ogg", "c.ogg" };
			Assert.That(FinisherReactions.TryPick(sounds, 0.25f, 0.3f, 0f, out _), Is.False);
			Assert.That(FinisherReactions.TryPick(sounds, 0.25f, 0.25f, 0f, out string first), Is.True);
			Assert.That(first, Is.EqualTo("a.ogg"));
			Assert.That(FinisherReactions.TryPick(sounds, null, 1f, 0.5f, out string middle), Is.True);
			Assert.That(middle, Is.EqualTo("b.ogg"));
			Assert.That(FinisherReactions.TryPick(sounds, 1f, 0f, 1f, out string last), Is.True);
			Assert.That(last, Is.EqualTo("c.ogg"));
			Assert.That(FinisherReactions.TryPick(sounds, 0f, 0.01f, 0f, out _), Is.False);
			Assert.That(FinisherReactions.TryPick(new List<string>(), 1f, 0f, 0f, out _), Is.False);
			Assert.That(FinisherReactions.TryPick(null, 1f, 0f, 0f, out _), Is.False);
		}

		[Test]
		public void SoundPaths_ListEveryReactionSound()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy);
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			Assert.That(FinisherReactions.GetSoundPaths(result.Definition.Behavior.Modules.Single()).ToArray(), Is.EqualTo(new[]
			{
				"assets/thrust-1.ogg", "assets/thrust-2.wav", "assets/cum.ogg", "assets/moan.ogg", "assets/grunt.ogg", "assets/slap.ogg", "assets/gulp.ogg"
			}));
		}
	}
}
