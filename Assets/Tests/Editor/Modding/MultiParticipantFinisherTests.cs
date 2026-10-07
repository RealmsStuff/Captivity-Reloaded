using System.Linq;
using NUnit.Framework;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class MultiParticipantFinisherTests
	{
		private const string ValidEnemy = @"{
  'schemaVersion': 1, 'type': 'enemy', 'id': 'example.multi:enemy/owner', 'displayName': 'Owner',
  'stats': { 'healthMax': 10, 'speedAcceleration': 4, 'speedMax': 3, 'traction': 0.2 },
  'spawn': { 'inheritTemplateSpawners': false },
  'behavior': { 'modules': [{ 'type': 'downedFinisher', 'triggerRange': 2, 'meterMax': 100, 'inputPower': 10,
    'participants': [{ 'id': 'assistant', 'enemy': 'example.multi:enemy/assistant', 'joinPolicy': 'phaseBoundary',
      'joinRange': 3, 'approachRange': 9, 'offsetX': 1.25, 'offsetY': 0, 'facing': 'left', 'animation': 'assist-idle' }],
    'phases': [
      { 'id': 'hold', 'durationSeconds': 2, 'animation': 'hold', 'participantAnimations': { 'assistant': 'assist-hold' } },
      { 'id': 'finish', 'durationSeconds': 2, 'animation': 'finish', 'participantAnimations': { 'assistant': 'assist-finish' } }
    ]
  }] },
  'ai': { 'type': 'groundChase', 'preferredRange': 1, 'reactionSeconds': 0.1 },
  'attacks': [{ 'id': 'hit', 'type': 'melee', 'animation': 'idle', 'chance': 1, 'damage': 1,
    'cooldownSeconds': 1, 'initiateRange': 1, 'hitRange': 1, 'durationSeconds': 0.5, 'hitTimeSeconds': 0.2 }],
  'animation': { 'clips': {
    'idle': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }] },
    'move': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'x': 0 } } }] },
    'hold': { 'durationSeconds': 2, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }] },
    'finish': { 'durationSeconds': 2, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }] }
  } },
  'visual': { 'type': 'originalSkeletonAtlas', 'atlas': 'assets/owner.png', 'pixelsPerUnit': 32,
    'bodyWidth': 1, 'bodyHeight': 2, 'regions': { 'hips': { 'x': 0, 'y': 0, 'width': 16, 'height': 16 } },
    'bones': [{ 'id': 'hips', 'region': 'hips' }], 'hitZones': [{ 'bone': 'hips', 'shape': 'box', 'width': 1, 'height': 1 }]
  }
}";

		[Test]
		public void Parse_AcceptsPhaseBoundaryParticipant()
		{
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(ValidEnemy, "example.multi", "owner.json");
			Assert.That(result.Report.IsValid, Is.True,
				string.Join("\n", result.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message)));
			EnemyBehaviorModuleDefinition finisher = result.Definition.Behavior.Modules.Single();
			Assert.That(finisher.Participants.Single().Id, Is.EqualTo("assistant"));
			Assert.That(finisher.Phases[1].ParticipantAnimations["assistant"], Is.EqualTo("assist-finish"));
		}

		[Test]
		public void Parse_RejectsRequiredLateJoinParticipant()
		{
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(
				ValidEnemy.Replace("'joinPolicy': 'phaseBoundary'", "'joinPolicy': 'phaseBoundary', 'required': true"),
				"example.multi", "owner.json");
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "enemy.behavior.finisher-participant-required"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnknownParticipantAnimationSlot()
		{
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(
				ValidEnemy.Replace("'assistant': 'assist-finish'", "'missing': 'assist-finish'"),
				"example.multi", "owner.json");
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "enemy.behavior.finisher-participant-animation-id"), Is.True);
		}

		[Test]
		public void Parse_AcceptsModuleAndPhaseInputPatterns()
		{
			string json = ValidEnemy
				.Replace("'meterMax': 100, 'inputPower': 10", "'meterMax': 100, 'inputPower': 10, 'inputPattern': 'tap'")
				.Replace("'id': 'hold', 'durationSeconds': 2", "'id': 'hold', 'durationSeconds': 2, 'inputPattern': 'rotate'");
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(json, "example.multi", "owner.json");

			Assert.That(result.Report.IsValid, Is.True,
				string.Join("\n", result.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message)));
			EnemyBehaviorModuleDefinition finisher = result.Definition.Behavior.Modules.Single();
			Assert.That(finisher.InputPattern, Is.EqualTo("tap"));
			Assert.That(finisher.Phases[0].InputPattern, Is.EqualTo("rotate"));
		}
	}
}
