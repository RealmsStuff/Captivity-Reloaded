using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	// Several downedFinisher modules per original enemy: ids, weights, a grab naming its finisher, and the weighted pick.
	public sealed class MultipleFinisherTests
	{
		private const string Enemy = @"{
  'schemaVersion': 1, 'type': 'enemy', 'id': 'example.enemies:enemy/twin', 'displayName': 'Twin',
  'stats': { 'healthMax': 10, 'speedAcceleration': 4, 'speedMax': 3, 'traction': 0.2 },
  'spawn': { 'inheritTemplateSpawners': false },
  'behavior': { 'modules': [
    { 'type': 'downedFinisher', 'id': 'pin', 'weight': 3, 'triggerRange': 2, 'durationSeconds': 4, 'meterMax': 100, 'inputPower': 10, 'animation': 'pin' },
    { 'type': 'downedFinisher', 'id': 'carry', 'triggerRange': 1, 'durationSeconds': 6, 'meterMax': 100, 'inputPower': 10, 'animation': 'carry', 'cooldownSeconds': 9 }
  ] },
  'ai': { 'type': 'groundChase', 'preferredRange': 1, 'reactionSeconds': 0.1 },
  'attacks': [{ 'id': 'tackle', 'type': 'grab', 'finisher': 'carry', 'animation': 'idle', 'chance': 1, 'damage': 0,
    'cooldownSeconds': 1, 'initiateRange': 1, 'hitRange': 1, 'durationSeconds': 0.5, 'hitTimeSeconds': 0.2 }],
  'animation': { 'clips': {
    'idle': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }] },
    'move': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'x': 0 } } }] },
    'pin': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'rotation': 10 } } }] },
    'carry': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'rotation': -80 } } }] }
  } },
  'visual': { 'type': 'originalSkeletonAtlas', 'atlas': 'assets/twin.png', 'pixelsPerUnit': 32,
    'bodyWidth': 1, 'bodyHeight': 2, 'regions': { 'hips': { 'x': 0, 'y': 0, 'width': 16, 'height': 16 } },
    'bones': [{ 'id': 'hips', 'region': 'hips' }], 'hitZones': [{ 'bone': 'hips', 'shape': 'box', 'width': 1, 'height': 1 }]
  }
}";

		private static EnemyDefinitionLoadResult Parse(string i_json) => EnemyDefinitionParser.Parse(i_json, "example.enemies", "twin.json");

		private static string Issues(EnemyDefinitionLoadResult i_result) =>
			string.Join("\n", i_result.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray());

		private static string With(string i_find, string i_replace)
		{
			Assert.That(Enemy.Contains(i_find), Is.True, i_find);
			return Enemy.Replace(i_find, i_replace);
		}

		private static bool Has(EnemyDefinitionLoadResult i_result, string i_code) => i_result.Report.Issues.Any(i_issue => i_issue.Code == i_code);

		[Test]
		public void Parse_AcceptsSeveralFinishersWithIdsWeightsAndANamedGrab()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy);
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			List<EnemyBehaviorModuleDefinition> finishers = result.Definition.Behavior.Modules.Where(i_module => i_module.Type == "downedFinisher").ToList();
			Assert.That(finishers.Select(i_finisher => i_finisher.Id), Is.EqualTo(new[] { "pin", "carry" }));
			Assert.That(finishers.Select(i_finisher => i_finisher.Weight), Is.EqualTo(new float?[] { 3f, null }));
			Assert.That(result.Definition.Attacks.Single().Finisher, Is.EqualTo("carry"));
		}

		[Test]
		public void Parse_KeepsOneFinisherWithoutAnIdValid()
		{
			Newtonsoft.Json.Linq.JObject enemy = Newtonsoft.Json.Linq.JObject.Parse(With("'id': 'pin', 'weight': 3, ", string.Empty).Replace("'finisher': 'carry', ", string.Empty));
			((Newtonsoft.Json.Linq.JArray)enemy["behavior"]["modules"])[1].Remove();
			EnemyDefinitionLoadResult result = Parse(enemy.ToString());
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			Assert.That(result.Definition.Behavior.Modules.Single().Id, Is.Null);
		}

		[Test]
		public void Parse_RequiresUniqueIdsWhenThereAreSeveralFinishers()
		{
			Assert.That(Has(Parse(With("'id': 'carry', 'triggerRange'", "'triggerRange'").Replace("'finisher': 'carry', ", string.Empty)), "enemy.behavior.finisher-id"), Is.True);
			Assert.That(Has(Parse(With("'id': 'carry', 'triggerRange'", "'id': 'pin', 'triggerRange'").Replace("'finisher': 'carry', ", "'finisher': 'pin', ")), "enemy.behavior.finisher-id"), Is.True);
		}

		[Test]
		public void Parse_RejectsBadWeightsAndIdsOnOtherModules()
		{
			Assert.That(Has(Parse(With("'weight': 3", "'weight': 0")), "enemy.stats.finisher-weight"), Is.True);
			EnemyDefinitionLoadResult regeneration = Parse(With("'behavior': { 'modules': [", "'behavior': { 'modules': [ { 'type': 'regeneration', 'id': 'heal', 'amount': 1, 'intervalSeconds': 1 },"));
			Assert.That(Has(regeneration, "enemy.behavior.module-id"), Is.True, Issues(regeneration));
			EnemyDefinitionLoadResult twice = Parse(With("'behavior': { 'modules': [", "'behavior': { 'modules': [ { 'type': 'regeneration', 'amount': 1, 'intervalSeconds': 1 }, { 'type': 'regeneration', 'amount': 2, 'intervalSeconds': 1 },"));
			Assert.That(Has(twice, "enemy.behavior.module"), Is.True, "Only downedFinisher may repeat.");
		}

		[Test]
		public void Parse_RejectsAGrabNamingAMissingFinisherAndANonGrabNamingOne()
		{
			Assert.That(Has(Parse(With("'finisher': 'carry'", "'finisher': 'hug'")), "enemy.attacks.finisher-missing"), Is.True);
			Assert.That(Has(Parse(With("'type': 'grab'", "'type': 'melee'")), "enemy.attacks.finisher"), Is.True);
		}

		[Test]
		public void PickByWeight_FollowsTheWeightsAndDefaultsToOne()
		{
			Type finisherType = Type.GetType("ModularEnemyFinisher, Assembly-CSharp");
			Assert.That(finisherType, Is.Not.Null);
			MethodInfo pick = finisherType.GetMethod("PickByWeight", BindingFlags.Public | BindingFlags.Static);
			List<EnemyBehaviorModuleDefinition> finishers = Parse(Enemy).Definition.Behavior.Modules.Where(i_module => i_module.Type == "downedFinisher").ToList();
			// pin weighs 3, carry 1 (default): rolls below 0.75 pick pin.
			Func<float, string> roll = i_roll => ((EnemyBehaviorModuleDefinition)pick.Invoke(null, new object[] { finishers, (float?)i_roll })).Id;
			Assert.That(roll(0f), Is.EqualTo("pin"));
			Assert.That(roll(0.74f), Is.EqualTo("pin"));
			Assert.That(roll(0.76f), Is.EqualTo("carry"));
			Assert.That(roll(0.999f), Is.EqualTo("carry"));
			Assert.That(pick.Invoke(null, new object[] { finishers.Take(1).ToList(), (float?)0.9f }), Is.SameAs(finishers[0]), "One finisher is always the one.");
		}

		[Test]
		public void TryBeginFromAttack_StartsTheNamedFinisherAndSharesTheCooldown()
		{
			Type finisherType = Type.GetType("ModularEnemyFinisher, Assembly-CSharp");
			GameObject host = new GameObject("twin");
			try
			{
				Component finisher = host.AddComponent(finisherType);
				List<EnemyBehaviorModuleDefinition> finishers = Parse(Enemy).Definition.Behavior.Modules.Where(i_module => i_module.Type == "downedFinisher").ToList();
				FieldInfo list = finisherType.GetField("m_definitions", BindingFlags.NonPublic | BindingFlags.Instance);
				((IList)list.GetValue(finisher)).Clear();
				foreach (EnemyBehaviorModuleDefinition item in finishers) ((IList)list.GetValue(finisher)).Add(item);
				IReadOnlyList<EnemyBehaviorModuleDefinition> listed = (IReadOnlyList<EnemyBehaviorModuleDefinition>)finisherType.GetMethod("GetFinishers").Invoke(finisher, null);
				Assert.That(listed.Select(i_item => i_item.Id), Is.EqualTo(new[] { "pin", "carry" }));
				// No NPC on this object: TryBeginFromAttack refuses before choosing, so nothing starts.
				MethodInfo begin = finisherType.GetMethod("TryBeginFromAttack", new[] { Type.GetType("Player, Assembly-CSharp"), typeof(string) });
				Assert.That(begin, Is.Not.Null, "TryBeginFromAttack(Player, string) takes the attack's finisher id.");
				Assert.That((bool)begin.Invoke(finisher, new object[] { null, "carry" }), Is.False);
			}
			finally { UnityEngine.Object.DestroyImmediate(host); }
		}
	}
}
