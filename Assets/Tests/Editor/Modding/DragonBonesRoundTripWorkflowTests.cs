using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class DragonBonesRoundTripWorkflowTests
	{
		private string m_root;
		private string m_backup;

		[SetUp]
		public void SetUp()
		{
			m_root = Path.Combine(Path.GetTempPath(), "captivity-dragonbones-roundtrip-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(Path.Combine(m_root, "mod", "content", "animations", "test-enemy", "player"));
			Directory.CreateDirectory(Path.Combine(m_root, "mod", "assets", "enemies", "test-enemy"));
			File.WriteAllText(Path.Combine(m_root, "mod", "manifest.json"),
				"{\"schemaVersion\":1,\"id\":\"example.roundtrip-test\",\"displayName\":\"Round Trip Test\",\"version\":\"1.0.0\",\"modApiVersion\":1,\"contentRoots\":[\"content\"]}");
			WriteAtlas(Path.Combine(m_root, "mod", "assets", "enemies", "test-enemy", "atlas.png"));
			WriteEnemy();
			WriteEnemyAnimation("idle", true, false);
			WriteEnemyAnimation("move", true, false);
			WriteEnemyAnimation("attack", false, true);
			WriteEnemyAnimation("paired-draft", true, false);
			File.WriteAllText(Path.Combine(m_root, "mod", "content", "animations", "test-enemy", "player", "paired-draft.player-animation.json"),
				new JObject { ["schemaVersion"] = 1, ["type"] = "playerAnimation", ["id"] = "example.roundtrip-test:player-animation/test-enemy/paired-draft",
					["rig"] = "core:player-rig/alex", ["displayName"] = "Paired Draft Player", ["durationSeconds"] = 1, ["frameRate"] = 60, ["loop"] = true,
					["tracks"] = new JArray(Track("bone/hips", "position.y", false)) }.ToString());
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(m_root)) Directory.Delete(m_root, true);
			if (!string.IsNullOrWhiteSpace(m_backup) && Directory.Exists(m_backup)) Directory.Delete(m_backup, true);
		}

		[Test]
		public void OriginalEnemy_PairedDragonBonesRoundTrip_PreservesAndValidatesMod()
		{
			Type bridge = FindEditorType("CaptivityReloaded.Editor.Modding.DragonBonesAnimationBridge");
			string mod = Path.Combine(m_root, "mod"), enemy = Path.Combine(mod, "content", "test-enemy.enemy.json");
			string animations = Path.Combine(mod, "content", "animations", "test-enemy"), output = Path.Combine(m_root, "dragonbones");
			Invoke(bridge, "ExportOriginalEnemy", enemy, animations, output, 4, true);

			string enemyOnlyPath = Path.Combine(output, "enemy-only", "test-enemy_ske.json");
			string skeletonPath = Path.Combine(output, "test-enemy-paired_ske.json");
			string sidecarPath = Path.Combine(output, "captivity-paired-roundtrip.json");
			Assert.That(File.Exists(enemyOnlyPath), Is.True); Assert.That(File.Exists(skeletonPath), Is.True); Assert.That(File.Exists(sidecarPath), Is.True);
			JObject enemyOnly = JObject.Parse(File.ReadAllText(enemyOnlyPath));
			JObject enemyArmature = ((JArray)enemyOnly["armature"])[0] as JObject;
			CollectionAssert.AreEquivalent(new[] { "attack", "idle", "move", "paired-draft" },
				((JArray)enemyArmature["animation"]).Values<JObject>().Select(i_animation => (string)i_animation["name"]).ToArray());
			JObject enemyHips = ((JArray)enemyArmature["bone"]).Values<JObject>().Single(i_bone => (string)i_bone["name"] == "hips");
			Assert.That(enemyHips["parent"], Is.Null, "The root-bone hierarchy changed during export.");
			JObject enemySlot = ((JArray)enemyArmature["slot"]).Values<JObject>().Single(i_slot => (string)i_slot["parent"] == "hips");
			Assert.That((int)enemySlot["userData"]["captivitySortingOrder"], Is.EqualTo(3));
			JObject enemyDisplay = ((JArray)((JObject)((JArray)enemyArmature["skin"])[0])["slot"])[0]["display"][0] as JObject;
			Assert.That((float)enemyDisplay["userData"]["captivityPivotX"], Is.EqualTo(.25f).Within(.0001f));
			Assert.That((float)enemyDisplay["userData"]["captivityPivotY"], Is.EqualTo(.75f).Within(.0001f));
			Assert.That((float)enemyDisplay["transform"]["x"], Is.EqualTo(8f).Within(.0001f));
			Assert.That((float)enemyDisplay["transform"]["y"], Is.EqualTo(8f).Within(.0001f));

			JObject skeleton = JObject.Parse(File.ReadAllText(skeletonPath));
			JObject armature = ((JArray)skeleton["armature"])[0] as JObject;
			Assert.That(((JArray)armature["bone"]).Values<JObject>().Any(i_bone => (string)i_bone["name"] == "enemy-hips"), Is.True);
			Assert.That(((JArray)armature["bone"]).Values<JObject>().Any(i_bone => (string)i_bone["name"] == "player-hips"), Is.True);
			JObject animation = ((JArray)armature["animation"]).Values<JObject>().Single(i_animation => (string)i_animation["name"] == "paired-draft");
			JObject hips = ((JArray)animation["bone"]).Values<JObject>().Single(i_timeline => (string)i_timeline["name"] == "enemy-hips");
			JArray translate = hips["translateFrame"] as JArray ?? new JArray();
			if (translate.Count == 0) { translate.Add(new JObject { ["duration"] = 60, ["x"] = 0, ["y"] = 0 }); hips["translateFrame"] = translate; }
			((JObject)translate[0])["x"] = 32f;
			File.WriteAllText(skeletonPath, skeleton.ToString());

			object result = Invoke(bridge, "SafeImport", skeletonPath, sidecarPath, mod, true);
			ValidationReport report = (ValidationReport)result.GetType().GetProperty("Report").GetValue(result);
			Assert.That(report.IsValid, Is.True, string.Join("\n", report.Issues.Select(i_issue => i_issue.ToString()).ToArray()));
			Assert.That((int)result.GetType().GetProperty("ReplacedAnimationCount").GetValue(result), Is.EqualTo(2));
			m_backup = (string)result.GetType().GetProperty("BackupDirectory").GetValue(result);
			Assert.That(Directory.Exists(m_backup), Is.True);
			Assert.That(CapmodPackageBuilder.ValidateSource(mod).Report.IsValid, Is.True);
			string capmod = Path.Combine(m_root, "roundtrip-test.capmod");
			CapmodPackageResult package = CapmodPackageBuilder.Build(mod, capmod);
			Assert.That(package.Report.IsValid, Is.True, string.Join("\n", package.Report.Issues.Select(i_issue => i_issue.ToString()).ToArray()));
			Assert.That(File.Exists(capmod), Is.True);

			JObject imported = JObject.Parse(File.ReadAllText(Path.Combine(animations, "paired-draft.enemy-animation.json")));
			JObject xTrack = ((JArray)imported["tracks"]).Values<JObject>().Single(i_track => (string)i_track["target"] == "bone/hips" && (string)i_track["property"] == "position.x");
			Assert.That((float)xTrack["keys"][0]["value"], Is.EqualTo(.25f).Within(.0001f));
		}

		[Test]
		public void CoreEnemyBatchExport_ProducesProjectForEveryCatalogEnemy()
		{
			Type bridge = FindEditorType("CaptivityReloaded.Editor.Modding.DragonBonesAnimationBridge");
			string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
			string output = Path.Combine(m_root, "all-core-enemies");
			object result = Invoke(bridge, "ExportAllCoreEnemies", projectRoot, output, 32f, 1);

			JObject catalog = JObject.Parse(File.ReadAllText(Path.Combine(projectRoot, "ModSDK", "AnimationReference", "core-animation-catalog.json")));
			int expected = ((JArray)catalog["enemies"]).Count;
			Assert.That((int)result.GetType().GetProperty("EnemyCount").GetValue(result), Is.EqualTo(expected));
			Assert.That((int)result.GetType().GetProperty("AnimationCount").GetValue(result), Is.GreaterThan(expected));
			Assert.That((int)result.GetType().GetProperty("ImageCount").GetValue(result), Is.GreaterThan(0));

			string indexPath = (string)result.GetType().GetProperty("IndexPath").GetValue(result);
			Assert.That(File.Exists(indexPath), Is.True);
			JObject index = JObject.Parse(File.ReadAllText(indexPath));
			Assert.That((string)index["type"], Is.EqualTo("captivityDragonBonesCoreEnemyExportIndex"));
			Assert.That((int)index["enemyCount"], Is.EqualTo(expected));
			foreach (JObject enemy in ((JArray)index["enemies"]).OfType<JObject>())
			{
				string directory = Path.Combine(output, (string)enemy["directory"]);
				Assert.That(File.Exists(Path.Combine(output, ((string)enemy["skeleton"]).Replace('/', Path.DirectorySeparatorChar))), Is.True, (string)enemy["enemy"]);
				Assert.That(File.Exists(Path.Combine(directory, "captivity-roundtrip.json")), Is.True, (string)enemy["enemy"]);
			}
		}

		private void WriteEnemy()
		{
			JObject enemy = new JObject { ["schemaVersion"] = 1, ["type"] = "enemy", ["id"] = "example.roundtrip-test:enemy/test-enemy", ["displayName"] = "Test Enemy",
				["stats"] = new JObject { ["healthMax"] = 50, ["speedAcceleration"] = 5, ["speedMax"] = 3, ["traction"] = .15f, ["bounty"] = 1, ["healthIncreasePerWave"] = 1 },
				["spawn"] = new JObject { ["inheritTemplateSpawners"] = false, ["selectionWeight"] = 1 },
				["ai"] = new JObject { ["type"] = "groundChase", ["preferredRange"] = 1, ["retreatRange"] = .5f, ["reactionSeconds"] = .1f },
				["attacks"] = new JArray(new JObject { ["id"] = "primary", ["type"] = "melee", ["animation"] = "attack", ["hitTimeSeconds"] = .5f, ["chance"] = 1,
					["damage"] = 1, ["knockbackX"] = 1, ["knockbackY"] = 1, ["cooldownSeconds"] = 1, ["initiateRange"] = 1.5f, ["hitRange"] = 1.2f, ["durationSeconds"] = 1 }),
				["behavior"] = new JObject { ["visionRange"] = 20, ["ignoreWave"] = false, ["modules"] = new JArray() },
				["animationRefs"] = new JObject { ["idle"] = AnimationId("idle"), ["move"] = AnimationId("move"), ["attack"] = AnimationId("attack"), ["paired-draft"] = AnimationId("paired-draft") },
				["drops"] = new JObject { ["chance"] = 0, ["items"] = new JArray() },
				["visual"] = new JObject { ["type"] = "originalSkeletonAtlas", ["atlas"] = "assets/enemies/test-enemy/atlas.png", ["pixelsPerUnit"] = 32,
					["bodyWidth"] = .75f, ["bodyHeight"] = 1, ["regions"] = new JObject { ["hips"] = new JObject { ["x"] = 0, ["y"] = 0, ["width"] = 8, ["height"] = 8 } },
					["bones"] = new JArray(new JObject { ["id"] = "hips", ["region"] = "hips", ["x"] = 0, ["y"] = 0, ["rotation"] = 0, ["pivotX"] = .25f, ["pivotY"] = .75f, ["sortingOrder"] = 3 }),
					["hitZones"] = new JArray(new JObject { ["bone"] = "hips", ["shape"] = "box", ["width"] = .5f, ["height"] = .8f, ["damageMultiplier"] = "normal" }) } };
			File.WriteAllText(Path.Combine(m_root, "mod", "content", "test-enemy.enemy.json"), enemy.ToString());
		}

		private void WriteEnemyAnimation(string i_name, bool i_loop, bool i_attack)
		{
			JObject animation = new JObject { ["schemaVersion"] = 1, ["type"] = "enemyAnimation", ["id"] = AnimationId(i_name), ["enemy"] = "example.roundtrip-test:enemy/test-enemy",
				["displayName"] = i_name, ["durationSeconds"] = 1, ["frameRate"] = 60, ["loop"] = i_loop, ["tracks"] = new JArray(Track("bone/hips", "position.y", false)) };
			if (i_attack) animation["events"] = new JArray(new JObject { ["time"] = .5f, ["type"] = "attackHit" });
			File.WriteAllText(Path.Combine(m_root, "mod", "content", "animations", "test-enemy", i_name + ".enemy-animation.json"), animation.ToString());
		}

		private static JObject Track(string i_target, string i_property, bool i_offset)
		{
			return new JObject { ["target"] = i_target, ["property"] = i_property, ["keys"] = new JArray(
				new JObject { ["time"] = 0, ["value"] = i_offset ? .1f : 0f, ["inTangent"] = null, ["outTangent"] = null },
				new JObject { ["time"] = 1, ["value"] = i_offset ? .1f : 0f, ["inTangent"] = null, ["outTangent"] = null }) };
		}

		private static string AnimationId(string i_name) { return "example.roundtrip-test:enemy-animation/test-enemy/" + i_name; }

		private static void WriteAtlas(string i_path)
		{
			Texture2D texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
			try { texture.SetPixels(Enumerable.Repeat(Color.green, 64).ToArray()); texture.Apply(); File.WriteAllBytes(i_path, texture.EncodeToPNG()); }
			finally { UnityEngine.Object.DestroyImmediate(texture); }
		}

		private static Type FindEditorType(string i_name)
		{
			Type type = AppDomain.CurrentDomain.GetAssemblies().Select(i_assembly => i_assembly.GetType(i_name, false)).FirstOrDefault(i_type => i_type != null);
			Assert.That(type, Is.Not.Null, "Editor bridge type was not loaded."); return type;
		}

		private static object Invoke(Type i_type, string i_method, params object[] i_arguments)
		{
			try { return i_type.GetMethod(i_method, BindingFlags.Public | BindingFlags.Static).Invoke(null, i_arguments); }
			catch (TargetInvocationException exception) { throw exception.InnerException ?? exception; }
		}
	}
}
