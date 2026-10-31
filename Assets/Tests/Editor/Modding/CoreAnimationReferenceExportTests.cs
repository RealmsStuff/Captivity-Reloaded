using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class CoreAnimationReferenceExportTests
	{
		private string m_root;

		[SetUp]
		public void SetUp()
		{
			m_root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/CoreAnimationReferenceExportTests/CoreAnimationReference"));
			if (Directory.Exists(Path.GetDirectoryName(m_root))) Directory.Delete(Path.GetDirectoryName(m_root), true);
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(Path.GetDirectoryName(m_root))) Directory.Delete(Path.GetDirectoryName(m_root), true);
		}

		// The editor assembly is not referenced by the test assembly, so the exporter is reached by name.
		private static Type EditorType(string i_name)
		{
			return AppDomain.CurrentDomain.GetAssemblies().Select(i_assembly => i_assembly.GetType("CaptivityReloaded.Editor.Modding." + i_name, false))
				.First(i_type => i_type != null);
		}

		[Test]
		public void Export_WritesEveryCoreRigWithItsClipsAndEverySpriteItDraws()
		{
			object summary = EditorType("CoreAnimationReferenceExporter").GetMethod("Export").Invoke(null, new object[] { m_root });
			Assert.That(summary.GetType().GetField("Missing").GetValue(summary), Is.Empty, summary.ToString());

			JObject index = JObject.Parse(File.ReadAllText(Path.Combine(m_root, "index.json")));
			Assert.That(index["type"].Value<string>(), Is.EqualTo("coreAnimationReference"));
			JArray enemies = (JArray)index["enemies"];
			Assert.That(enemies.Count, Is.EqualTo(21));
			string clipRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "Resources/Modding/Core/Content/Animations"));
			foreach (JObject enemy in enemies.Cast<JObject>().Append((JObject)index["player"]))
			{
				string folder = Path.Combine(m_root, enemy["folder"].Value<string>());
				JObject rig = JObject.Parse(File.ReadAllText(Path.Combine(folder, "rig.json")));
				string[] clips = Directory.GetFiles(Path.Combine(folder, "clips"), "*.json");
				Assert.That(clips.Length, Is.EqualTo(enemy["clips"].Value<int>()).And.GreaterThan(0), folder);
				if (rig["type"].Value<string>() == "enemyRigReference")
					Assert.That(clips.Length, Is.EqualTo(Directory.GetFiles(Path.Combine(clipRoot, Path.GetFileName(folder)), "*.json").Length), folder);

				JObject atlas = JObject.Parse(File.ReadAllText(Path.Combine(folder, "sprites.json")));
				Texture2D image = new Texture2D(2, 2);
				try
				{
					Assert.That(image.LoadImage(File.ReadAllBytes(Path.Combine(folder, "sprites.png"))), Is.True, folder);
					Assert.That(image.width, Is.EqualTo(atlas["width"].Value<int>()));
					Assert.That(image.height, Is.EqualTo(atlas["height"].Value<int>()));
				}
				finally { UnityEngine.Object.DestroyImmediate(image); }
				var regions = atlas["sprites"].Cast<JObject>().ToDictionary(i_sprite => i_sprite["asset"] + "#" + i_sprite["name"]);
				foreach (JObject region in regions.Values)
				{
					Assert.That(region["x"].Value<int>() + region["width"].Value<int>(), Is.LessThanOrEqualTo(atlas["width"].Value<int>()));
					Assert.That(region["y"].Value<int>() + region["height"].Value<int>(), Is.LessThanOrEqualTo(atlas["height"].Value<int>()));
				}
				// Every sprite the rig names is in the atlas.
				foreach (JObject bone in rig["bones"].Cast<JObject>())
					foreach (JObject sprite in (bone["sprites"] as JArray ?? new JArray()).Cast<JObject>())
						Assert.That(regions.ContainsKey(sprite["asset"] + "#" + sprite["name"]), Is.True, folder + ": " + sprite["asset"]);
				if (rig["type"].Value<string>() == "playerRig")
					Assert.That(((JObject)atlas["bones"]).Properties().Select(i_bone => i_bone.Name), Is.SupersetOf(new[] { "hips", "chest", "head", "foot-left", "foot-right" }));
			}
		}

		[Test]
		public void BuildStep_FindsStreamingAssetsBesideEachStandalonePlayer()
		{
			MethodInfo folder = EditorType("CoreAnimationReferenceBuildStep").GetMethod("StreamingAssetsFolder");
			string windows = (string)folder.Invoke(null, new object[] { UnityEditor.BuildTarget.StandaloneWindows64, Path.Combine("Builds", "Windows", "Captivity Reloaded.exe") });
			Assert.That(windows, Is.EqualTo(Path.Combine("Builds", "Windows", "Captivity Reloaded_Data", "StreamingAssets")));
			string mac = (string)folder.Invoke(null, new object[] { UnityEditor.BuildTarget.StandaloneOSX, Path.Combine("Builds", "Captivity Reloaded.app") });
			Assert.That(mac, Is.EqualTo(Path.Combine("Builds", "Captivity Reloaded.app", "Contents", "Resources", "Data", "StreamingAssets")));
			Assert.That(folder.Invoke(null, new object[] { UnityEditor.BuildTarget.WebGL, "Builds/Web" }), Is.Null);
		}
	}
}
