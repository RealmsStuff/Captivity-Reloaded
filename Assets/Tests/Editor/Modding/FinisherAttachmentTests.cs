using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class FinisherAttachmentTests
	{
		private const string Enemy = @"{
  'schemaVersion': 1, 'type': 'enemy', 'id': 'example.enemies:enemy/climber', 'displayName': 'Climber',
  'stats': { 'healthMax': 10, 'speedAcceleration': 4, 'speedMax': 3, 'traction': 0.2 },
  'spawn': { 'inheritTemplateSpawners': false },
  'behavior': { 'modules': [
    { 'type': 'downedFinisher', 'triggerRange': 2, 'meterMax': 100, 'inputPower': 10,
      'attachment': { 'target': 'spine', 'offsetY': 0.25, 'rootX': -0.5, 'rootY': -0.6, 'followRotation': 0.8, 'settleSeconds': 0.4,
        'contacts': [{ 'bone': 'hand', 'x': -0.05, 'y': 0.06 }],
        'sortingOrders': { 'hand': 30 }, 'playerSortingOrders': { 'arm-right-upper': 28 } },
      'phases': [
        { 'id': 'cling', 'durationSeconds': 2, 'animation': 'idle' },
        { 'id': 'grip', 'durationSeconds': 2, 'animation': 'idle', 'blendSeconds': 0.2, 'contacts': [{ 'bone': 'hand', 'target': 'chest', 'x': 0.1, 'y': 0.02, 'angle': 15, 'bend': -1 }] },
        { 'id': 'free', 'durationSeconds': 2, 'animation': 'idle', 'contacts': [] },
        { 'id': 'drop', 'durationSeconds': 1, 'animation': 'idle', 'attached': false }
      ] }
  ] },
  'ai': { 'type': 'groundChase', 'preferredRange': 1, 'reactionSeconds': 0.1 },
  'attacks': [{ 'id': 'hit', 'type': 'melee', 'animation': 'idle', 'chance': 1, 'damage': 1,
    'cooldownSeconds': 1, 'initiateRange': 1, 'hitRange': 1, 'durationSeconds': 0.5, 'hitTimeSeconds': 0.2 }],
  'animation': { 'clips': {
    'idle': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }] },
    'move': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'x': 0 } } }] }
  } },
  'visual': { 'type': 'originalSkeletonAtlas', 'atlas': 'assets/climber.png', 'pixelsPerUnit': 32,
    'bodyWidth': 1, 'bodyHeight': 2, 'regions': { 'part': { 'x': 0, 'y': 0, 'width': 16, 'height': 16 } },
    'bones': [
      { 'id': 'hips', 'region': 'part' },
      { 'id': 'arm-upper', 'parent': 'hips', 'region': 'part', 'y': 0.3 },
      { 'id': 'arm-lower', 'parent': 'arm-upper', 'region': 'part', 'x': 0.2 },
      { 'id': 'hand', 'parent': 'arm-lower', 'region': 'part', 'x': 0.2 }
    ],
    'hitZones': [{ 'bone': 'hips', 'shape': 'box', 'width': 1, 'height': 1 }]
  }
}";

		private static EnemyDefinitionLoadResult Parse(string i_json)
		{
			return EnemyDefinitionParser.Parse(i_json, "example.enemies", "climber.json");
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
		public void Parse_AcceptsAttachmentAndPhaseContacts()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy);
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			EnemyBehaviorModuleDefinition finisher = Finisher(result);
			Assert.That(finisher.Attachment.Target, Is.EqualTo("spine"));
			Assert.That(finisher.Attachment.SettleSeconds, Is.EqualTo(0.4f));
			Assert.That(finisher.Attachment.Contacts.Single().Bone, Is.EqualTo("hand"));
			Assert.That(finisher.Phases[0].Contacts, Is.Null, "A phase without contacts keeps the attachment's.");
			EnemyFinisherContactDefinition grip = finisher.Phases[1].Contacts.Single();
			Assert.That(grip.Target, Is.EqualTo("chest"));
			Assert.That(grip.Bend, Is.EqualTo(-1));
			Assert.That(finisher.Phases[2].Contacts, Is.Empty, "An empty list lets every limb go.");
			Assert.That(finisher.Phases[3].Attached, Is.False);
		}

		[Test]
		public void PhaseBlendSeconds_OverridesAttachmentSettleTime()
		{
			EnemyDefinitionLoadResult result = Parse(Enemy);
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			EnemyBehaviorModuleDefinition finisher = Finisher(result);
			GameObject host = new GameObject("finisher-attachment-blend-test");
			try
			{
				ModularEnemyFinisher runtime = host.AddComponent<ModularEnemyFinisher>();
				System.Type type = typeof(ModularEnemyFinisher);
				type.GetField("m_definition", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(runtime, finisher);
				FieldInfo phaseIndex = type.GetField("m_phaseIndex", BindingFlags.Instance | BindingFlags.NonPublic);
				MethodInfo settleSeconds = type.GetMethod("GetSettleSeconds", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.That(phaseIndex, Is.Not.Null);
				Assert.That(settleSeconds, Is.Not.Null);

				phaseIndex.SetValue(runtime, 0);
				Assert.That((float)settleSeconds.Invoke(runtime, new object[] { finisher.Attachment }), Is.EqualTo(0.4f),
					"A phase without blendSeconds keeps attachment.settleSeconds.");
				phaseIndex.SetValue(runtime, 1);
				Assert.That((float)settleSeconds.Invoke(runtime, new object[] { finisher.Attachment }), Is.EqualTo(0.2f),
					"A phase blend controls its attachment and contact easing too.");
			}
			finally
			{
				Object.DestroyImmediate(host);
			}
		}

		[Test]
		public void Parse_RejectsPhaseContactsWithoutAttachment()
		{
			int start = Enemy.IndexOf("'attachment'"), end = Enemy.IndexOf("'phases'");
			EnemyDefinitionLoadResult result = Parse(Enemy.Remove(start, end - start));
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Count(i_issue => i_issue.Code == "enemy.behavior.finisher-attachment-phase"), Is.EqualTo(3), Issues(result));
		}

		[TestCase("'target': 'spine'", "'target': 'tail'", "enemy.behavior.finisher-attachment-target")]
		[TestCase("'settleSeconds': 0.4", "'settleSeconds': 9", "enemy.stats.finisher-attachment-settle")]
		[TestCase("'followRotation': 0.8", "'followRotation': 2", "enemy.stats.finisher-attachment-follow-rotation")]
		[TestCase("'rootY': -0.6", "'rootY': -30", "enemy.stats.finisher-attachment-offset")]
		[TestCase("[{ 'bone': 'hand', 'x': -0.05", "[{ 'bone': 'arm-upper', 'x': -0.05", "enemy.behavior.finisher-contact-bone")]
		[TestCase("'target': 'chest'", "'target': 'tail'", "enemy.behavior.finisher-contact-target")]
		[TestCase("'bend': -1", "'bend': 2", "enemy.behavior.finisher-contact-value")]
		[TestCase("'sortingOrders': { 'hand': 30 }", "'sortingOrders': { 'tail': 30 }", "enemy.behavior.finisher-attachment-sorting")]
		[TestCase("'playerSortingOrders': { 'arm-right-upper': 28 }", "'playerSortingOrders': { 'tail': 28 }", "enemy.behavior.finisher-attachment-player-sorting")]
		public void Parse_RejectsInvalidAttachmentValues(string i_find, string i_replace, string i_code)
		{
			Assert.That(Enemy.Contains(i_find), Is.True, "Fixture no longer contains: " + i_find);
			EnemyDefinitionLoadResult result = Parse(Enemy.Replace(i_find, i_replace));
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == i_code), Is.True, Issues(result));
		}
	}
}
