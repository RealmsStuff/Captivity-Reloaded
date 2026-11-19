using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CaptivityReloaded.Modding.Tests
{
	public class WeaponHoldPoseTests
	{
		private static JObject Weapon()
		{
			return JObject.Parse(@"{'schemaVersion':1,'type':'weapon','id':'test.pose:item/weapon/test','displayName':'Test',
				'extends':'core:item/weapon/pistol','stats':{'holdType':'TwoHanded1'},
				'visual':{'type':'coreWeaponSprites','sprites':{'base':'assets/base.png'}},
				'holdPose':{'rightUpper':-45,'rightLower':65,'rightHand':90,'useLeftHand':true,'leftUpper':60,'leftLower':75,'leftHand':80}}");
		}

		[Test]
		public void OptionalPoseAndLegacyPresetBothLoad()
		{
			JObject doc = Weapon();
			var custom = WeaponDefinitionParser.Parse(doc.ToString(), "test.pose", "test.json");
			Assert.That(custom.Report.IsValid, Is.True);
			Assert.That(custom.Definition.HoldPose.RightUpper, Is.EqualTo(-45f));
			Assert.That(custom.Definition.HoldPose.UseLeftHand, Is.True);
			doc.Remove("holdPose");
			var legacy = WeaponDefinitionParser.Parse(doc.ToString(), "test.pose", "test.json");
			Assert.That(legacy.Report.IsValid, Is.True);
			Assert.That(legacy.Definition.HoldPose, Is.Null);
		}

		[TestCase("rightUpper")][TestCase("rightLower")][TestCase("rightHand")]
		[TestCase("leftUpper")][TestCase("leftLower")][TestCase("leftHand")]
		public void EveryAngleIsRequiredAndBounded(string field)
		{
			foreach (double value in new[] { -361d, 361d, double.NaN, double.PositiveInfinity })
			{
				JObject doc = Weapon(); doc["holdPose"][field] = value;
				Assert.That(WeaponDefinitionParser.Parse(doc.ToString(), "test.pose", "test.json").Report.IsValid, Is.False);
			}
			JObject missing = Weapon(); ((JObject)missing["holdPose"]).Remove(field);
			Assert.That(WeaponDefinitionParser.Parse(missing.ToString(), "test.pose", "test.json").Report.IsValid, Is.False);
		}

		[Test]
		public void PoseRequiresFallbackAndRejectsUnknownFields()
		{
			JObject doc = Weapon(); doc.Remove("stats");
			Assert.That(WeaponDefinitionParser.Parse(doc.ToString(), "test.pose", "test.json").Report.IsValid, Is.False);
			doc = Weapon(); doc["holdPose"]["typo"] = 1;
			Assert.That(WeaponDefinitionParser.Parse(doc.ToString(), "test.pose", "test.json").Report.IsValid, Is.False);
		}

		[Test]
		public void ConfiguredPoseSurvivesUnityPrefabClone()
		{
			var type = Type.GetType("Gun, Assembly-CSharp");
			Assert.That(type, Is.Not.Null);
			var source = new UnityEngine.GameObject("pose-test");
			source.SetActive(false);
			var component = source.AddComponent(type);
			var pose = new WeaponHoldPoseDefinition { RightUpper = -45, RightLower = 65, RightHand = 90, UseLeftHand = true, LeftUpper = 60, LeftLower = 75, LeftHand = 80 };
			type.GetMethod("ConfigureModHoldPose").Invoke(component, new object[] { pose });
			UnityEngine.GameObject clone = null;
			try
			{
				clone = UnityEngine.Object.Instantiate(source);
				var copied = (WeaponHoldPoseDefinition)type.GetMethod("GetModHoldPose").Invoke(clone.GetComponent(type), null);
				Assert.That(copied, Is.Not.Null);
				Assert.That(copied.RightUpper, Is.EqualTo(-45));
				Assert.That(copied.UseLeftHand, Is.True);
				Assert.That(copied.LeftHand, Is.EqualTo(80));
			}
			finally { if (clone != null) UnityEngine.Object.DestroyImmediate(clone); UnityEngine.Object.DestroyImmediate(source); }
		}
	}
}
