using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class FinisherStartFreezeTests
	{
		private const string Enemy = @"{
  'schemaVersion': 1, 'type': 'enemy', 'id': 'example.enemies:enemy/grabber', 'displayName': 'Grabber',
  'stats': { 'healthMax': 10, 'speedAcceleration': 4, 'speedMax': 3, 'traction': 0.2 },
  'spawn': { 'inheritTemplateSpawners': false },
  'behavior': { 'modules': [
    { 'type': 'downedFinisher', 'triggerRange': 2, 'meterMax': 100, 'inputPower': 10, 'startFreezeSeconds': 0.5,
      'phases': [{ 'id': 'pin', 'durationSeconds': 2, 'animation': 'idle' }, { 'id': 'end', 'durationSeconds': 1, 'animation': 'idle' }] }
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
		public void Parse_ReadsStartFreezeSeconds()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy);
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			Assert.That(Finisher(result).StartFreezeSeconds, Is.EqualTo(0.5f));
		}

		[Test]
		public void Parse_LeavesStartFreezeUnsetByDefault()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy.Replace(" 'startFreezeSeconds': 0.5,", string.Empty));
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			Assert.That(Finisher(result).StartFreezeSeconds, Is.Null);
		}

		[TestCase("1.5")]
		[TestCase("-0.1")]
		public void Parse_RejectsStartFreezeOutsideZeroToOne(string i_value)
		{
			EnemyDefinitionLoadResult result = Parse(Enemy.Replace("'startFreezeSeconds': 0.5", "'startFreezeSeconds': " + i_value));
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "enemy.stats.finisher-start-freeze"), Is.True, Issues(result));
		}

		[Test]
		public void Runtime_StopsTimeUntilTheFreezeEnds()
		{
			EnemyBehaviorModuleDefinition finisher = Finisher(Parse(Enemy));
			// ModularEnemyFinisher is in Assembly-CSharp, which this test assembly cannot reference.
			System.Type type = System.Type.GetType("ModularEnemyFinisher, Assembly-CSharp");
			Assert.That(type, Is.Not.Null);
			GameObject host = new GameObject("finisher-start-freeze-test");
			try
			{
				Component runtime = host.AddComponent(type);
				type.GetField("m_definition", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(runtime, finisher);
				MethodInfo begin = type.GetMethod("BeginStartFreeze", BindingFlags.Instance | BindingFlags.NonPublic);
				MethodInfo disable = type.GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.That(begin, Is.Not.Null);
				Assert.That(disable, Is.Not.Null);

				begin.Invoke(runtime, null);
				Assert.That(Time.timeScale, Is.EqualTo(0f), "The grab holds the game still as it lands.");
				disable.Invoke(runtime, null);
				Assert.That(Time.timeScale, Is.EqualTo(1f), "A finisher torn down mid-freeze lets the game run again.");

				finisher.StartFreezeSeconds = null;
				begin.Invoke(runtime, null);
				Assert.That(Time.timeScale, Is.EqualTo(1f), "Without startFreezeSeconds the grab does not freeze.");
			}
			finally
			{
				Time.timeScale = 1f;
				Object.DestroyImmediate(host);
			}
		}
	}
}
