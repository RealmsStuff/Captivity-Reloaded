using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class SimpleRetexturePackBuilderTests
	{
		[Test]
		public void MatchEnemyPngs_AssignsDragonBonesDuplicateFilesToMirroredSlots()
		{
			CoreEnemyAssetSlotEnemy enemy = new CoreEnemyAssetSlotEnemy
			{
				Id = "core:enemy/test",
				Slots = new List<CoreEnemyAssetSlotRecord>
				{
					new CoreEnemyAssetSlotRecord { Slot = "body/arm-left", SpriteName = "ArmUpper_5" },
					new CoreEnemyAssetSlotRecord { Slot = "body/arm-right", SpriteName = "ArmUpper_5" },
					new CoreEnemyAssetSlotRecord { Slot = "body/head", SpriteName = "Head_11" }
				}
			};

			SimpleRetextureMatchResult result = SimpleRetexturePackBuilder.MatchEnemyPngs(enemy, new[]
			{
				@"C:\art\armupper_5-2.png", @"C:\art\head_11.png", @"C:\art\armupper_5.png", @"C:\art\notes.png"
			});

			Assert.That(result.Matches.Select(i_match => i_match.Slot), Is.EquivalentTo(new[] { "body/arm-left", "body/arm-right", "body/head" }));
			Assert.That(result.Matches.Single(i_match => i_match.Slot == "body/arm-left").SourcePng, Does.EndWith("armupper_5.png"));
			Assert.That(result.Matches.Single(i_match => i_match.Slot == "body/arm-right").SourcePng, Does.EndWith("armupper_5-2.png"));
			Assert.That(result.UnmatchedPngs.Single(), Does.EndWith("notes.png"));
		}

		[Test]
		public void MatchEnemyPngs_PreservesNumericSuffixThatBelongsToOriginalSpriteName()
		{
			CoreEnemyAssetSlotEnemy enemy = new CoreEnemyAssetSlotEnemy
			{
				Id = "core:enemy/zombie-3",
				Slots = new List<CoreEnemyAssetSlotRecord>
				{
					new CoreEnemyAssetSlotRecord { Slot = "body/leg-left-upper", SpriteName = "LegUpper_19 1" },
					new CoreEnemyAssetSlotRecord { Slot = "body/leg-right-upper", SpriteName = "LegUpper_19 1" }
				}
			};

			SimpleRetextureMatchResult result = SimpleRetexturePackBuilder.MatchEnemyPngs(enemy,
				new[] { @"C:\art\legupper_19-1-2.png", @"C:\art\legupper_19-1.png" });

			Assert.That(result.Matches.Count, Is.EqualTo(2));
			Assert.That(result.Matches.Single(i_match => i_match.Slot == "body/leg-left-upper").SourcePng, Does.EndWith("legupper_19-1.png"));
			Assert.That(result.Matches.Single(i_match => i_match.Slot == "body/leg-right-upper").SourcePng, Does.EndWith("legupper_19-1-2.png"));
		}

		[Test]
		public void Build_WritesPatchOverridesAndDownscalesDragonBonesArtwork()
		{
			string root = Path.Combine(Path.GetTempPath(), "captivity-retexture-test-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(root);
			Texture2D texture = null;
			Texture2D decoded = null;
			try
			{
				File.WriteAllText(Path.Combine(root, "manifest.json"), @"{
  'schemaVersion': 1,
  'id': 'test.easy-retexture',
  'displayName': 'Easy Retexture',
  'version': '1.0.0',
  'modApiVersion': 1,
  'contentRoots': ['content']
}".Replace('\'', '"'));
				string source = Path.Combine(root, "edited.png");
				texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
				Color32[] pixels = Enumerable.Repeat(new Color32(20, 40, 60, 255), 64).ToArray();
				texture.SetPixels32(pixels);
				texture.Apply();
				File.WriteAllBytes(source, texture.EncodeToPNG());

				SimpleRetextureBuildResult result = SimpleRetexturePackBuilder.Build(root, "jenny-blue", "core:enemy/jenny",
					new[] { new SimpleRetextureInput { Slot = "body/head", SourcePng = source } }, 4, false);

				Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(i_issue => i_issue.Message).ToArray()));
				JObject patch = JObject.Parse(File.ReadAllText(result.PatchPath));
				Assert.That((string)patch["target"], Is.EqualTo("core:enemy/jenny"));
				Assert.That((string)patch["replacements"]["body/head"], Is.EqualTo("assets/retextures/jenny-blue/body-head.png"));
				JObject manifest = JObject.Parse(File.ReadAllText(Path.Combine(root, "manifest.json")));
				Assert.That(manifest["overrides"].Values<string>(), Does.Contain("core:enemy/jenny/body/head"));
				Assert.That(manifest["dependencies"].Values<JObject>().Any(i_dependency => (string)i_dependency["id"] == "core"), Is.True);

				byte[] output = File.ReadAllBytes(Path.Combine(root, "assets", "retextures", "jenny-blue", "body-head.png"));
				decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
				Assert.That(decoded.LoadImage(output), Is.True);
				Assert.That(decoded.width, Is.EqualTo(2));
				Assert.That(decoded.height, Is.EqualTo(2));
			}
			finally
			{
				if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
				if (decoded != null) UnityEngine.Object.DestroyImmediate(decoded);
				if (Directory.Exists(root)) Directory.Delete(root, true);
			}
		}
	}
}
