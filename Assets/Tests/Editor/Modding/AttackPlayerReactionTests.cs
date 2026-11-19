using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CaptivityReloaded.Modding.Tests
{
	// playerReactionRef: a short player animation layered on the player when a data-defined attack lands.
	public sealed class AttackPlayerReactionTests
	{
		private const string Enemy = @"{
  'schemaVersion': 1, 'type': 'enemy', 'id': 'example.enemies:enemy/boxer', 'displayName': 'Boxer',
  'stats': { 'healthMax': 50, 'speedAcceleration': 4, 'speedMax': 3, 'traction': 0.2 },
  'spawn': { 'inheritTemplateSpawners': false },
  'ai': { 'type': 'groundChase', 'preferredRange': 1, 'retreatRange': 0.5, 'reactionSeconds': 0.1 },
  'attacks': [
    { 'id': 'jab', 'type': 'melee', 'animation': 'swing', 'chance': 1, 'damage': 5,
      'playerReactionRef': 'example.enemies:player-animation/boxer/hit-jab',
      'cooldownSeconds': 1, 'initiateRange': 1.5, 'hitRange': 1.3, 'durationSeconds': 1 },
    { 'id': 'combo', 'type': 'multiStage', 'animation': 'combo', 'chance': 1, 'damage': 4,
      'cooldownSeconds': 3, 'initiateRange': 1.4, 'hitRange': 1.25, 'durationSeconds': 1,
      'stages': [{ 'type': 'melee' }, { 'type': 'melee', 'damage': 9, 'playerReactionRef': 'example.enemies:player-animation/boxer/hit-hook' }] }
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

		// An inherited enemy: playerReactionRef on an override of a Core attack, and on a new additional attack.
		private static string Variant(string i_inherited, string i_additional) => @"{
  'schemaVersion': 1, 'type': 'enemy', 'id': 'example.enemies:enemy/brawler', 'displayName': 'Brawler', 'extends': 'core:enemy/zombie-1',
  'spawn': { 'inheritTemplateSpawners': false },
  'attacks': [ { 'index': 0, 'damage': 6" + i_inherited + @" } ],
  'animationRefs': { 'slam': 'example.enemies:enemy-animation/slam' },
  'additionalAttacks': [ { 'id': 'slam', 'type': 'area', 'animation': 'slam', 'chance': 0.5, 'damage': 10, 'cooldownSeconds': 3,
    'initiateRange': 1.5, 'hitRange': 1.5, 'areaRadius': 1.2, 'durationSeconds': 1.2" + i_additional + @" } ] }";

		private const string Reaction = ", 'playerReactionRef': 'example.enemies:player-animation/boxer/hit-jab'";

		private static string PlayerClip(string i_path, float i_duration, bool i_loop, string i_events = "") => (@"{
  'schemaVersion': 1, 'type': 'playerAnimation', 'id': 'example.enemies:player-animation/" + i_path + @"', 'rig': 'core:player-rig/alex',
  'displayName': 'Hit', 'durationSeconds': " + i_duration.ToString(System.Globalization.CultureInfo.InvariantCulture) + @", 'frameRate': 60,
  'loop': " + (i_loop ? "true" : "false") + @",
  'tracks': [ { 'target': 'bone/head', 'property': 'rotation.z', 'keys': [ { 'time': 0, 'value': 0 }, { 'time': 0.1, 'value': -25 } ] } ],
  'events': [ " + i_events + @" ] }").Replace('\'', '"');

		[TearDown]
		public void TearDown()
		{
			NormalizedPlayerAnimationRegistry.Initialize(ModLoaderRuntime.PlayerAnimationDefinitions, new ValidationReport());
		}

		private static EnemyDefinitionLoadResult Parse(string i_json)
		{
			return EnemyDefinitionParser.Parse(i_json.Replace('\'', '"'), "example.enemies", "boxer.json");
		}

		private static string Issues(ValidationReport i_report)
		{
			return string.Join("\n", i_report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray());
		}

		// ModLoaderRuntime resolves each playerReactionRef against the loaded player animations.
		private static ValidationReport Resolve(EnemyDefinition i_enemy, params string[] i_playerClips)
		{
			ValidationReport report = new ValidationReport();
			List<NormalizedPlayerAnimationDefinition> clips = new List<NormalizedPlayerAnimationDefinition>();
			foreach (string json in i_playerClips)
			{
				NormalizedPlayerAnimationLoadResult clip = NormalizedPlayerAnimationParser.Parse(json, "example.enemies", "hit.player-animation.json");
				Assert.That(clip.Report.IsValid, Is.True, Issues(clip.Report));
				clips.Add(clip.Definition);
			}
			NormalizedPlayerAnimationRegistry.Initialize(clips, report);
			MethodInfo resolve = typeof(ModLoaderRuntime).GetMethod("ResolvePlayerAnimationReferences", BindingFlags.NonPublic | BindingFlags.Static);
			Assert.That(resolve, Is.Not.Null);
			resolve.Invoke(null, new object[] { new[] { i_enemy }, report });
			return report;
		}

		[Test]
		public void Parse_AcceptsAReactionOnAnAttackAndOnAStage()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy);
			Assert.That(result.Report.IsValid, Is.True, Issues(result.Report));
			Assert.That(result.Definition.Attacks.Single(i_attack => i_attack.Id == "jab").PlayerReactionReference,
				Is.EqualTo("example.enemies:player-animation/boxer/hit-jab"));
			EnemyAttackDefinition combo = result.Definition.Attacks.Single(i_attack => i_attack.Id == "combo");
			Assert.That(combo.PlayerReactionReference, Is.Null, "Only the attacks that name a reaction get one.");
			Assert.That(combo.Stages.Select(i_stage => i_stage.PlayerReactionReference),
				Is.EqualTo(new[] { null, "example.enemies:player-animation/boxer/hit-hook" }));
			Assert.That(combo.Stages[1].PlayerReaction, Is.Null, "The clip is resolved when the game loads its content, not by the parser.");
		}

		[TestCase("'example.enemies:player-animation/boxer/hit-jab'", "'hit-jab'")]
		[TestCase("'example.enemies:player-animation/boxer/hit-jab'", "'example.enemies:enemy-animation/boxer/hit-jab'")]
		[TestCase("'example.enemies:player-animation/boxer/hit-hook'", "'example.enemies:enemy/boxer'")]
		public void Parse_RejectsAReactionThatIsNotAPlayerAnimation(string i_find, string i_replace)
		{
			EnemyDefinitionLoadResult result = Parse(Enemy.Replace(i_find, i_replace));
			Assert.That(result.Report.Issues.Select(i_issue => i_issue.Code), Does.Contain("enemy.attacks.player-reaction-ref"), Issues(result.Report));
		}

		// An inherited attack hits through the Core enemy's own code; an additional attack lands through the same
		// delivery as an original enemy's.
		[Test]
		public void Parse_AcceptsAReactionOnAnAdditionalAttackButNotOnAnInheritedOne()
		{
			EnemyDefinitionLoadResult additional = Parse(Variant("", Reaction));
			Assert.That(additional.Report.IsValid, Is.True, Issues(additional.Report));
			Assert.That(additional.Definition.AdditionalAttacks.Single().PlayerReactionReference, Is.EqualTo("example.enemies:player-animation/boxer/hit-jab"));
			EnemyDefinitionLoadResult inherited = Parse(Variant(Reaction, ""));
			Assert.That(inherited.Report.Issues.Select(i_issue => i_issue.Code), Does.Contain("enemy.attacks.player-reaction-inherited"), Issues(inherited.Report));
		}

		[Test]
		public void Load_ResolvesTheReactionClips()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy);
			ValidationReport report = Resolve(result.Definition, PlayerClip("boxer/hit-jab", 0.4f, false), PlayerClip("boxer/hit-hook", 0.6f, false));
			Assert.That(report.IsValid, Is.True, Issues(report));
			EnemyAnimationClipDefinition jab = result.Definition.Attacks.Single(i_attack => i_attack.Id == "jab").PlayerReaction;
			Assert.That(jab, Is.Not.Null);
			Assert.That(jab.DurationSeconds, Is.EqualTo(0.4f));
			Assert.That(jab.Frames.Last().Bones["head"].Rotation, Is.EqualTo(-25f));
			Assert.That(result.Definition.Attacks.Single(i_attack => i_attack.Id == "combo").Stages[1].PlayerReaction.DurationSeconds, Is.EqualTo(0.6f));
		}

		[Test]
		public void Load_ResolvesAnAdditionalAttacksReaction()
		{
			EnemyDefinitionLoadResult result = Parse(Variant("", Reaction));
			ValidationReport report = Resolve(result.Definition, PlayerClip("boxer/hit-jab", 0.4f, false));
			Assert.That(report.IsValid, Is.True, Issues(report));
			Assert.That(result.Definition.AdditionalAttacks.Single().PlayerReaction, Is.Not.Null);
		}

		[Test]
		public void Load_RejectsAMissingReaction()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy);
			ValidationReport report = Resolve(result.Definition, PlayerClip("boxer/hit-jab", 0.4f, false));
			Assert.That(report.Issues.Select(i_issue => i_issue.Code), Does.Contain("enemy.player-animation-ref-missing"), Issues(report));
		}

		// It is layered on a free player for a moment: once, short, and without events that would fire on every hit.
		[TestCase(0.4f, true, "", "enemy.player-reaction-animation-length")]
		[TestCase(2.5f, false, "", "enemy.player-reaction-animation-length")]
		[TestCase(0.4f, false, "{ 'time': 0.1, 'name': 'pleasure', 'floatValue': 1 }", "enemy.player-reaction-animation-events")]
		public void Load_RejectsAReactionThatIsNotAShortOneShot(float i_duration, bool i_loop, string i_events, string i_code)
		{
			EnemyDefinitionLoadResult result = Parse(Enemy);
			ValidationReport report = Resolve(result.Definition, PlayerClip("boxer/hit-jab", i_duration, i_loop, i_events), PlayerClip("boxer/hit-hook", 0.6f, false));
			Assert.That(report.Issues.Select(i_issue => i_issue.Code), Does.Contain(i_code), Issues(report));
			Assert.That(result.Definition.Attacks.Single(i_attack => i_attack.Id == "jab").PlayerReaction, Is.Null);
		}

		// The layer fades in fast, so the hit reads on the frame it lands, and fades out so the Animator's pose comes
		// back without a pop; outside the clip it adds nothing.
		[Test]
		public void TheLayer_FadesInAndOut()
		{
			Type reaction = Type.GetType("ModularPlayerHitReaction, Assembly-CSharp");
			Assert.That(reaction, Is.Not.Null, "The reaction component is missing from the game assembly.");
			MethodInfo weight = reaction.GetMethod("Weight", BindingFlags.Public | BindingFlags.Static);
			Func<float, float> at = i_elapsed => (float)weight.Invoke(null, new object[] { i_elapsed, 0.5f });
			Assert.That(at(-0.01f), Is.Zero);
			Assert.That(at(0f), Is.Zero);
			Assert.That(at(0.02f), Is.GreaterThan(0f).And.LessThan(1f), "Half way through the fade in.");
			Assert.That(at(0.1f), Is.EqualTo(1f).Within(0.0001f));
			Assert.That(at(0.3f), Is.EqualTo(1f).Within(0.0001f));
			Assert.That(at(0.45f), Is.GreaterThan(0f).And.LessThan(1f), "Fading out.");
			Assert.That(at(0.45f), Is.GreaterThan(at(0.48f)));
			Assert.That(at(0.5f), Is.Zero);
			Assert.That(at(0.6f), Is.Zero);
		}
	}
}
