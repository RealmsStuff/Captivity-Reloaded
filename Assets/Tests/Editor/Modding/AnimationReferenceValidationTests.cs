using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class AnimationReferenceValidationTests
	{
		[Test]
		public void Validator_ReportsMissingBonesRegionsAssetsAndEffects_AndRepairsSafeReferences()
		{
			string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/AnimationReferenceValidationTests"));
			Directory.CreateDirectory(root);
			try
			{
				NormalizedEnemyAnimationDocument document = DocumentWithBrokenReferences();
				ValidationReport report = EnemyAnimationReferenceValidator.Validate(document,
					new[] { "hips" }, new[] { "body" }, root, "broken-reference.json");
				string[] codes = report.Issues.Select(i_issue => i_issue.Code).ToArray();
				Assert.That(codes, Does.Contain("enemy-animation.reference-bone"));
				Assert.That(codes, Does.Contain("enemy-animation.reference-region"));
				Assert.That(codes, Does.Contain("enemy-animation.reference-audio-file"));
				Assert.That(codes, Does.Contain("enemy-animation.reference-effect-texture"));
				Assert.That(codes, Does.Contain("enemy-animation.reference-effect"));

				int changes = EnemyAnimationReferenceValidator.RepairSafeReferences(document, new[] { "hips" }, new[] { "body" });
				Assert.That(changes, Is.GreaterThanOrEqualTo(5));
				ValidationReport repaired = EnemyAnimationReferenceValidator.Validate(document,
					new[] { "hips" }, new[] { "body" }, root, "repaired-reference.json");
				Assert.That(repaired.Issues.Any(i_issue => i_issue.Code == "enemy-animation.reference-bone"), Is.False);
				Assert.That(repaired.Issues.Any(i_issue => i_issue.Code == "enemy-animation.reference-region"), Is.False);
				Assert.That(repaired.Issues.Any(i_issue => i_issue.Code == "enemy-animation.reference-effect"), Is.False);
				Assert.That(repaired.Issues.Any(i_issue => i_issue.Code == "enemy-animation.reference-audio-file"), Is.True);
				Assert.That(repaired.Issues.Any(i_issue => i_issue.Code == "enemy-animation.reference-effect-texture"), Is.True);
			}
			finally
			{
				if (Directory.Exists(root)) Directory.Delete(root, true);
			}
		}

		[Test]
		public void AnimationReferenceGallery_AllSixEnemyFamiliesBuildEveryReferencedClipAgainstTheirPublishedRigs()
		{
			string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods/animation-reference-gallery"));
			JObject manifest = JObject.Parse(File.ReadAllText(Path.Combine(root, "manifest.json")));
			string packId = manifest["id"].Value<string>();
			string animationRoot = Path.Combine(root, "content", "animations");
			string rigRoot = Path.Combine(root, "reference", "rigs");
			string[] families = { "zombie", "death-hound", "fly", "maggot", "musca", "gremlin" };
			int clipCount = 0;
			foreach (string family in families)
			{
				JObject rig = JObject.Parse(File.ReadAllText(Path.Combine(rigRoot, family, "rig.json")));
				HashSet<string> bones = new HashSet<string>(((JArray)rig["bones"]).Values<JObject>().Select(i_bone => i_bone["name"].Value<string>()));
				Assert.That(bones.Count, Is.GreaterThan(0), family);
				foreach (string file in Directory.GetFiles(Path.Combine(animationRoot, family), "*.enemy-animation.json"))
				{
					string json = File.ReadAllText(file);
					NormalizedEnemyAnimationLoadResult parsed = NormalizedEnemyAnimationParser.Parse(json, packId, file, root);
					Assert.That(parsed.Report.IsValid, Is.True, file + "\n" + string.Join("\n", parsed.Report.Issues.Select(i_issue => i_issue.ToString()).ToArray()));
					NormalizedEnemyAnimationDocument document = JsonConvert.DeserializeObject<NormalizedEnemyAnimationDocument>(json);
					ValidationReport references = EnemyAnimationReferenceValidator.Validate(document, bones, null, root, file);
					Assert.That(references.IsValid, Is.True, file + "\n" + string.Join("\n", references.Issues.Select(i_issue => i_issue.ToString()).ToArray()));
					ValidationReport runtime = new ValidationReport();
					EnemyAnimationClipDefinition clip = parsed.Definition.CreateClip(bones, new HashSet<string>(), runtime);
					Assert.That(runtime.IsValid, Is.True, file + "\n" + string.Join("\n", runtime.Issues.Select(i_issue => i_issue.ToString()).ToArray()));
					Assert.That(clip.Frames, Is.Not.Empty, file);
					Assert.That(clip.DurationSeconds, Is.GreaterThan(0f), file);
					clipCount++;
				}
			}
			Assert.That(clipCount, Is.EqualTo(72));
		}

		private static NormalizedEnemyAnimationDocument DocumentWithBrokenReferences()
		{
			return new NormalizedEnemyAnimationDocument
			{
				SchemaVersion = 1, Type = "enemyAnimation", Id = "example.reference:enemy-animation/test",
				Enemy = "example.reference:enemy/test", DisplayName = "Broken references", DurationSeconds = 1f, FrameRate = 60f,
				Tracks = new List<NormalizedNumericTrack> { new NormalizedNumericTrack { Target = "bone/missing", Property = "position.x", Keys = new List<NormalizedNumericKey> { new NormalizedNumericKey { Time = 0f, Value = 0f } } } },
				ObjectTracks = new List<NormalizedObjectTrack> { new NormalizedObjectTrack { Target = "sprite/missing", Property = "sprite", Keys = new List<NormalizedObjectKey> { new NormalizedObjectKey { Time = 0f, Name = "missing-region" } } } },
				Events = new List<EnemyAnimationEventDefinition>
				{
					new EnemyAnimationEventDefinition { Time = .1f, Type = "sound", File = "assets/audio/missing.wav", Volume = 1f },
					new EnemyAnimationEventDefinition { Time = .2f, Type = "spriteEffect", Region = "missing-region", Bone = "missing", DurationSeconds = .1f, Scale = 1f }
				},
				Effects = new List<NormalizedEffectDefinition> { new NormalizedEffectDefinition { Id = "impact", Target = "enemy-bone/missing", Texture = "assets/vfx/missing.png" } },
				EffectTriggers = new List<NormalizedEffectTrigger> { new NormalizedEffectTrigger { Time = .3f, Source = "author", Effects = new List<string> { "impact", "missing-effect" } } }
			};
		}
	}
}
