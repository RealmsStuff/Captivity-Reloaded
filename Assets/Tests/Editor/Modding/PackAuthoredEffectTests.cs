using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class PackAuthoredEffectTests
	{
		private static JObject Document(string i_trigger, JObject i_source, string i_triggerSource)
		{
			return new JObject
			{
				["schemaVersion"] = 1, ["type"] = "playerAnimation", ["id"] = "example.fx:player-animation/drip",
				["rig"] = "core:player-rig/alex", ["displayName"] = "Drip", ["durationSeconds"] = 1, ["frameRate"] = 30, ["loop"] = true,
				["tracks"] = new JArray(new JObject { ["target"] = "bone/hips", ["property"] = "position.y", ["keys"] = new JArray(new JObject { ["time"] = 0, ["value"] = 0 }) }),
				["effects"] = new JArray(Effect("vfx/drip", i_trigger, i_source)),
				["effectTriggers"] = new JArray(new JObject { ["time"] = 0.5f, ["source"] = i_triggerSource, ["effects"] = new JArray("vfx/drip") })
			};
		}

		private static JObject Effect(string i_id, string i_trigger, JObject i_source)
		{
			return new JObject
			{
				["id"] = i_id, ["kind"] = "particle", ["trigger"] = i_trigger, ["index"] = 0, ["target"] = "bone/hips",
				["durationSeconds"] = .25f, ["loop"] = false, ["maxParticles"] = 16,
				["startLifetimeMin"] = .1f, ["startLifetimeMax"] = .2f, ["startSpeedMin"] = 0, ["startSpeedMax"] = 1,
				["startSizeMin"] = .1f, ["startSizeMax"] = .2f, ["gravityMin"] = 0, ["gravityMax"] = 0,
				["scaleX"] = 1, ["scaleY"] = 1, ["scaleZ"] = 1, ["simulationSpace"] = "Local",
				["startColorMin"] = Color(), ["startColorMax"] = Color(), ["emissionRateMin"] = 0, ["emissionRateMax"] = 12,
				["shapeEnabled"] = true, ["shape"] = "Cone", ["shapeRadius"] = .1f, ["shapeAngle"] = 25,
				["textureSheetTilesX"] = 1, ["textureSheetTilesY"] = 1, ["textureSheetFrame"] = 0,
				["textureSheetSprites"] = new JArray(), ["sortingOrder"] = 10,
				["lightIntensity"] = 1, ["lightRadius"] = 1, ["trailWidth"] = .1f, ["trailTime"] = .2f, ["decalPixelsPerUnit"] = 32,
				["source"] = i_source
			};
		}

		private static JObject Color() { return new JObject { ["r"] = 1, ["g"] = 1, ["b"] = 1, ["a"] = 1 }; }
		private static JObject Authoring() { return new JObject { ["authoring"] = "tools/drip-vfx.py" }; }
		private static JObject CorePrefab() { return new JObject { ["unityPrefab"] = "Assets/Prefab/Squoid.prefab", ["hierarchyPath"] = "Squoid/Particles" }; }

		private static NormalizedPlayerAnimationLoadResult Parse(JObject i_document)
		{
			return NormalizedPlayerAnimationParser.Parse(i_document.ToString(), "example.fx", "drip.player-animation.json");
		}

		private static string Issues(NormalizedPlayerAnimationLoadResult i_result)
		{
			return string.Join("\n", i_result.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray());
		}

		[TestCase("drip", "pip:slam+cum+overflow")]
		[TestCase("sweat", "pip:sweat")]
		[TestCase("cumThrust", "example.fx:tip-drip")]
		public void Parse_AcceptsPackAuthoredEffects(string i_trigger, string i_triggerSource)
		{
			NormalizedPlayerAnimationLoadResult result = Parse(Document(i_trigger, Authoring(), i_triggerSource));
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			Assert.That(result.Definition.CreateFinisherClip().NormalizedPresentation.EffectTriggers.Single().Source, Is.EqualTo(i_triggerSource));
		}

		[TestCase("thrust", "enemy:Thrust")]
		[TestCase("cumThrust", "enemy:CumThrust")]
		[TestCase("unique", "enemy:PlayParticleUnique")]
		public void Parse_StillAcceptsCoreExports(string i_trigger, string i_triggerSource)
		{
			NormalizedPlayerAnimationLoadResult result = Parse(Document(i_trigger, CorePrefab(), i_triggerSource));
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
		}

		[TestCase("Drip")]
		[TestCase("drip drop")]
		[TestCase("")]
		[TestCase("a-very-long-trigger-name-that-runs-past-forty")]
		public void Parse_RejectsMalformedEffectTriggerNames(string i_trigger)
		{
			NormalizedPlayerAnimationLoadResult result = Parse(Document(i_trigger, Authoring(), "pip:drip"));
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "player-animation.effect-trigger-name"), Is.True, Issues(result));
		}

		[TestCase("pip")]
		[TestCase("pip:")]
		[TestCase("Pip:Slam")]
		[TestCase("pip:slam+")]
		[TestCase("enemy:Bogus")]
		[TestCase("pip:a+b+c+d+e+f+g+h+i")]
		public void Parse_RejectsMalformedTriggerSources(string i_triggerSource)
		{
			NormalizedPlayerAnimationLoadResult result = Parse(Document("drip", Authoring(), i_triggerSource));
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "player-animation.effect-trigger-source"), Is.True, Issues(result));
		}

		[Test]
		public void Parse_RejectsEffectSourcesOfNeitherShape()
		{
			JObject[] sources =
			{
				new JObject(),
				new JObject { ["authoring"] = "" },
				new JObject { ["authoring"] = 3 },
				new JObject { ["unityPrefab"] = "Assets/Prefab/Squoid.prefab" },
				new JObject { ["authoring"] = "tools/drip-vfx.py", ["unityPrefab"] = "Assets/Prefab/Squoid.prefab" },
				new JObject { ["authoring"] = new string('a', 261) }
			};
			foreach (JObject source in sources)
			{
				NormalizedPlayerAnimationLoadResult result = Parse(Document("drip", source, "pip:drip"));
				Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "player-animation.effect-source"), Is.True, source + "\n" + Issues(result));
			}
		}
	}
}
