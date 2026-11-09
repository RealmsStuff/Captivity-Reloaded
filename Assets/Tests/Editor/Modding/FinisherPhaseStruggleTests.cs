using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class FinisherPhaseStruggleTests
	{
		private const string Enemy = @"{
  'schemaVersion': 1, 'type': 'enemy', 'id': 'example.enemies:enemy/grabber', 'displayName': 'Grabber',
  'stats': { 'healthMax': 10, 'speedAcceleration': 4, 'speedMax': 3, 'traction': 0.2 },
  'spawn': { 'inheritTemplateSpawners': false },
  'behavior': { 'modules': [
    { 'type': 'downedFinisher', 'triggerRange': 2, 'meterMax': 100, 'inputPower': 10,
      'phases': [{ 'id': 'pin', 'durationSeconds': 2, 'animation': 'idle' }, { 'id': 'ride', 'durationSeconds': 3, 'animation': 'idle' }, { 'id': 'end', 'durationSeconds': 1, 'animation': 'idle', 'struggle': false }] }
  ] },
  'ai': { 'type': 'groundChase', 'preferredRange': 1, 'reactionSeconds': 0.1 },
  'attacks': [{ 'id': 'hit', 'type': 'melee', 'animation': 'idle', 'chance': 1, 'damage': 1,
    'cooldownSeconds': 1, 'initiateRange': 1, 'hitRange': 1, 'durationSeconds': 0.5, 'hitTimeSeconds': 0.2 }],
  'animation': { 'clips': {
    'idle': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }] },
    'move': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'x': 0 } } }] }
  } },
  'visual': { 'type': 'originalSkeletonAtlas', 'atlas': 'assets/grabber.png', 'pixelsPerUnit': 32,
    'bodyWidth': 1, 'bodyHeight': 2, 'regions': { 'part': { 'x': 0, 'y': 0, 'width': 16, 'height': 16 } },
    'bones': [{ 'id': 'hips', 'region': 'part' }],
    'hitZones': [{ 'bone': 'hips', 'shape': 'box', 'width': 1, 'height': 1 }]
  }
}";

		private static EnemyDefinitionLoadResult Parse(string i_json)
		{
			return EnemyDefinitionParser.Parse(i_json, "example.enemies", "grabber.json");
		}

		private static string Issues(EnemyDefinitionLoadResult i_result)
		{
			return string.Join("\n", i_result.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray());
		}

		private static EnemyBehaviorModuleDefinition Finisher(EnemyDefinitionLoadResult i_result)
		{
			return i_result.Definition.Behavior.Modules.Single(i_module => i_module.Type == "downedFinisher");
		}

		[Test]
		public void Parse_ReadsStruggleFalseOnTrailingPhases()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy);
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			EnemyBehaviorModuleDefinition finisher = Finisher(result);
			Assert.That(finisher.Phases[0].Struggle, Is.Null);
			Assert.That(finisher.Phases[2].Struggle, Is.False);
		}

		[Test]
		public void Parse_RejectsAStrugglingPhaseAfterTheEscapeWindow()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy.Replace("'id': 'ride', 'durationSeconds': 3, 'animation': 'idle' }", "'id': 'ride', 'durationSeconds': 3, 'animation': 'idle', 'struggle': false }")
				.Replace("'animation': 'idle', 'struggle': false }] }", "'animation': 'idle', 'struggle': true }] }"));
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "enemy.behavior.finisher-phase-struggle"), Is.True, Issues(result));
		}

		[Test]
		public void Parse_RejectsAFirstPhaseWithoutStruggle()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy.Replace("'id': 'pin', 'durationSeconds': 2, 'animation': 'idle' }", "'id': 'pin', 'durationSeconds': 2, 'animation': 'idle', 'struggle': false }"));
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "enemy.behavior.finisher-phase-struggle-first"), Is.True, Issues(result));
		}

		[Test]
		public void Runtime_TimerCountsOnlyTheStrugglingPhases()
		{
			EnemyBehaviorModuleDefinition finisher = Finisher(Parse(Enemy));
			// ModularEnemyFinisher is in Assembly-CSharp, which this test assembly cannot reference.
			System.Type type = System.Type.GetType("ModularEnemyFinisher, Assembly-CSharp");
			Assert.That(type, Is.Not.Null);
			GameObject host = new GameObject("finisher-phase-struggle-test");
			try
			{
				Component runtime = host.AddComponent(type);
				type.GetField("m_definition", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(runtime, finisher);
				MethodInfo timeMax = type.GetMethod("GetTimeMax", BindingFlags.Instance | BindingFlags.Public);
				Assert.That((float)timeMax.Invoke(runtime, null), Is.EqualTo(5f), "pin (2 s) + ride (3 s); end has struggle: false.");
				finisher.Phases[2].Struggle = null;
				Assert.That((float)timeMax.Invoke(runtime, null), Is.EqualTo(6f), "Without the field every phase counts.");
			}
			finally
			{
				Object.DestroyImmediate(host);
			}
		}
	}
}
