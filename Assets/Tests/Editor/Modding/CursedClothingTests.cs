using System.Linq;
using NUnit.Framework;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class CursedClothingTests
	{
		private const string ValidClothing = @"{
  'schemaVersion': 1, 'type': 'clothing', 'id': 'example.curse:clothing/hex-band',
  'displayName': 'Hex Band', 'category': 'Other',
  'visual': { 'type': 'originalClothingSprites', 'sprites': {
    'icon': 'assets/hex.png', 'piece/band': 'assets/hex.png'
  }, 'attachments': { 'piece/band': { 'bone': 'Spine' } } },
  'effects': { 'escapePowerMultiplier': 0.65, 'bountyMultiplier': 0.5,
    'statModifiers': { 'DamageMultiplierGun': 0.4 } }
}";

		private const string ValidEnemy = @"{
  'schemaVersion': 1, 'type': 'enemy', 'id': 'example.curse:enemy/hexer', 'displayName': 'Hexer',
  'stats': { 'healthMax': 10, 'speedAcceleration': 4, 'speedMax': 3, 'traction': 0.2 },
  'spawn': { 'inheritTemplateSpawners': false },
  'behavior': { 'modules': [
    { 'type': 'onHitEquipClothing', 'clothing': 'example.curse:clothing/hex-band', 'chance': 0.5, 'cooldownSeconds': 4 },
    { 'type': 'downedFinisher', 'triggerRange': 2, 'durationSeconds': 4, 'meterMax': 100, 'inputPower': 10,
      'animation': 'idle', 'failureOutcome': { 'equipClothing': ['example.curse:clothing/hex-band'] } }
  ] },
  'ai': { 'type': 'groundChase', 'preferredRange': 1, 'reactionSeconds': 0.1 },
  'attacks': [{ 'id': 'hit', 'type': 'melee', 'animation': 'idle', 'chance': 1, 'damage': 1,
    'cooldownSeconds': 1, 'initiateRange': 1, 'hitRange': 1, 'durationSeconds': 0.5, 'hitTimeSeconds': 0.2 }],
  'animation': { 'clips': {
    'idle': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }] },
    'move': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'x': 0 } } }] }
  } },
  'visual': { 'type': 'originalSkeletonAtlas', 'atlas': 'assets/hexer.png', 'pixelsPerUnit': 32,
    'bodyWidth': 1, 'bodyHeight': 2, 'regions': { 'hips': { 'x': 0, 'y': 0, 'width': 16, 'height': 16 } },
    'bones': [{ 'id': 'hips', 'region': 'hips' }], 'hitZones': [{ 'bone': 'hips', 'shape': 'box', 'width': 1, 'height': 1 }]
  }
}";

		[Test]
		public void ClothingParser_AcceptsBoundedCurseMultipliers()
		{
			ClothingDefinitionLoadResult result = ClothingDefinitionParser.Parse(ValidClothing, "example.curse", "hex-band.json");
			Assert.That(result.Report.IsValid, Is.True,
				string.Join("\n", result.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message)));
			Assert.That(result.Definition.Effects.EscapePowerMultiplier, Is.EqualTo(0.65f));
			Assert.That(result.Definition.Effects.BountyMultiplier, Is.EqualTo(0.5f));
		}

		[Test]
		public void ClothingParser_RejectsUnsafeCurseMultipliers()
		{
			string json = ValidClothing.Replace("'escapePowerMultiplier': 0.65", "'escapePowerMultiplier': 0.01")
				.Replace("'bountyMultiplier': 0.5", "'bountyMultiplier': 4");
			ClothingDefinitionLoadResult result = ClothingDefinitionParser.Parse(json, "example.curse", "hex-band.json");
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "clothing.effects.escape-power"), Is.True);
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "clothing.effects.bounty"), Is.True);
		}

		[Test]
		public void EnemyParser_AcceptsOnHitAndFinisherClothing()
		{
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(ValidEnemy, "example.curse", "hexer.json");
			Assert.That(result.Report.IsValid, Is.True,
				string.Join("\n", result.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message)));
			Assert.That(result.Definition.Behavior.Modules[0].Clothing, Is.EqualTo("example.curse:clothing/hex-band"));
			Assert.That(result.Definition.Behavior.Modules[1].FailureOutcome.EquipClothing.Single(),
				Is.EqualTo("example.curse:clothing/hex-band"));
		}

	}
}
