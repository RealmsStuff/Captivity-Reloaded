using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class AnimationPresentationTests
	{
		[Test]
		public void EnemyAnimation_AcceptsAllPublishedPresentationKinds()
		{
			JObject document = Document("assets/vfx/sheet.png", 3);
			NormalizedEnemyAnimationLoadResult result = NormalizedEnemyAnimationParser.Parse(document.ToString(), "example.presentation", "test.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(i_issue => i_issue.ToString()).ToArray()));
		}

		[Test]
		public void EnemyAnimation_RejectsEscapingTextureAndOutOfRangeRegion()
		{
			JObject document = Document("../outside.png", 4);
			NormalizedEnemyAnimationLoadResult result = NormalizedEnemyAnimationParser.Parse(document.ToString(), "example.presentation", "test.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "enemy-animation.effect-texture"), Is.True);
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "enemy-animation.effect-texture-sheet"), Is.True);
		}

		[Test]
		public void EnemyAnimation_AcceptsPortableAdvancedParticleModules()
		{
			JObject document = Document("assets/vfx/sheet.png", 1);
			JObject effect = (JObject)((JArray)document["effects"])[0];
			effect["bursts"] = new JArray(new JObject { ["time"] = .1f, ["countMin"] = 2, ["countMax"] = 5, ["cycleCount"] = 2, ["repeatInterval"] = .2f, ["probability"] = .75f });
			effect["sizeOverLifetime"] = new JObject { ["enabled"] = true, ["separateAxes"] = false, ["x"] = Curve() };
			effect["colorOverLifetime"] = new JObject { ["mode"] = "Gradient", ["gradientMax"] = new JArray(
				new JObject { ["time"] = 0, ["color"] = Color() }, new JObject { ["time"] = 1, ["color"] = new JObject { ["r"] = 1, ["g"] = 1, ["b"] = 1, ["a"] = 0 } }) };
			NormalizedEnemyAnimationLoadResult result = NormalizedEnemyAnimationParser.Parse(document.ToString(), "example.presentation", "test.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(i_issue => i_issue.ToString()).ToArray()));
		}

		[Test]
		public void EnemyAnimation_RejectsInvalidAdvancedParticleModules()
		{
			JObject document = Document("assets/vfx/sheet.png", 1);
			((JObject)((JArray)document["effects"])[0])["bursts"] = new JArray(new JObject
				{ ["time"] = 0, ["countMin"] = 8, ["countMax"] = 2, ["cycleCount"] = 0, ["repeatInterval"] = -1, ["probability"] = 2 });
			NormalizedEnemyAnimationLoadResult result = NormalizedEnemyAnimationParser.Parse(document.ToString(), "example.presentation", "test.json");
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "enemy-animation.effect-particle-modules"), Is.True);
		}

		private static JObject Document(string i_texture, int i_frame)
		{
			JArray effects = new JArray(
				Effect("particle", "particle", i_texture, i_frame), Effect("light", "light", i_texture, i_frame),
				Effect("trail", "trail", i_texture, i_frame), Effect("decal", "decal", i_texture, i_frame));
			return new JObject
			{
				["schemaVersion"] = 1, ["type"] = "enemyAnimation", ["id"] = "example.presentation:enemy-animation/test/attack",
				["enemy"] = "example.presentation:enemy/test", ["displayName"] = "Presentation", ["durationSeconds"] = 1,
				["frameRate"] = 60, ["loop"] = false,
				["tracks"] = new JArray(new JObject { ["target"] = "bone/hips", ["property"] = "position.x", ["keys"] = new JArray(new JObject { ["time"] = 0, ["value"] = 0 }) }),
				["effects"] = effects,
				["effectTriggers"] = new JArray(effects.Values<JObject>().Select((i_effect, i_index) => new JObject { ["time"] = i_index * .1f, ["source"] = "author", ["effects"] = new JArray((string)i_effect["id"]) }))
			};
		}

		private static JObject Effect(string i_id, string i_kind, string i_texture, int i_frame)
		{
			return new JObject
			{
				["id"] = i_id, ["kind"] = i_kind, ["trigger"] = "author", ["index"] = 0, ["target"] = "enemy-bone/hips",
				["durationSeconds"] = .25f, ["loop"] = false, ["maxParticles"] = 16,
				["startLifetimeMin"] = .1f, ["startLifetimeMax"] = .2f, ["startSpeedMin"] = 0, ["startSpeedMax"] = 1,
				["startSizeMin"] = .1f, ["startSizeMax"] = .2f, ["gravityMin"] = 0, ["gravityMax"] = 0,
				["scaleX"] = 1, ["scaleY"] = 1, ["scaleZ"] = 1, ["simulationSpace"] = "Local",
				["startColorMin"] = Color(), ["startColorMax"] = Color(), ["emissionRateMin"] = 0, ["emissionRateMax"] = 12,
				["shapeEnabled"] = true, ["shape"] = "Cone", ["shapeRadius"] = .1f, ["shapeAngle"] = 25,
				["textureSheetTilesX"] = 2, ["textureSheetTilesY"] = 2, ["textureSheetFrame"] = i_frame,
				["textureSheetSprites"] = new JArray(), ["sortingOrder"] = 10, ["texture"] = i_texture,
				["lightIntensity"] = 1, ["lightRadius"] = 1, ["trailWidth"] = .1f, ["trailTime"] = .2f, ["decalPixelsPerUnit"] = 32,
				["source"] = new JObject { ["kind"] = "test" }
			};
		}

		private static JObject Color() { return new JObject { ["r"] = 1, ["g"] = 1, ["b"] = 1, ["a"] = 1 }; }
		private static JObject Curve()
		{
			return new JObject { ["mode"] = "Curve", ["multiplier"] = 1, ["constantMin"] = 0, ["constantMax"] = 1,
				["curveMin"] = new JArray(), ["curveMax"] = new JArray(new JObject { ["time"] = 0, ["value"] = 1, ["inTangent"] = 0, ["outTangent"] = 0 }, new JObject { ["time"] = 1, ["value"] = 0, ["inTangent"] = 0, ["outTangent"] = 0 }) };
		}
	}
}
