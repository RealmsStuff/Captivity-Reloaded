using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class RuleProfileStateRegressionTests
	{
		private static Type RuntimeType(string i_name)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = assembly.GetType(i_name, false);
				if (type != null) return type;
			}
			Assert.Fail("Runtime type was not loaded: " + i_name);
			return null;
		}

		private static FieldInfo Field(Type i_type, string i_name)
		{
			for (Type type = i_type; type != null; type = type.BaseType)
			{
				FieldInfo field = type.GetField(i_name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null) return field;
			}
			Assert.Fail("Field was not found: " + i_type.FullName + "." + i_name);
			return null;
		}

		private static object Call(object i_target, string i_name, BindingFlags i_flags, params object[] i_arguments)
		{
			foreach (MethodInfo method in i_target.GetType().GetMethods(i_flags))
			{
				if (method.Name != i_name) continue;
				ParameterInfo[] parameters = method.GetParameters();
				if (parameters.Length != i_arguments.Length) continue;
				bool matches = true;
				for (int index = 0; index < parameters.Length; index++)
				{
					object argument = i_arguments[index];
					if (argument != null && !parameters[index].ParameterType.IsInstanceOfType(argument)) { matches = false; break; }
				}
				if (matches) return method.Invoke(i_target, i_arguments);
			}
			Assert.Fail("Method was not found: " + i_target.GetType().FullName + "." + i_name);
			return null;
		}

		private static object PublicCall(object i_target, string i_name, params object[] i_arguments)
		{
			return Call(i_target, i_name, BindingFlags.Instance | BindingFlags.Public, i_arguments);
		}

		private static object NonPublicCall(object i_target, string i_name, params object[] i_arguments)
		{
			return Call(i_target, i_name, BindingFlags.Instance | BindingFlags.NonPublic, i_arguments);
		}

		private static object CreatePlayerFixture(out GameObject o_gameObject)
		{
			o_gameObject = new GameObject("Rule profile player regression");
			o_gameObject.SetActive(false);
			Component player = o_gameObject.AddComponent(RuntimeType("Player"));
			Field(player.GetType(), "m_healthMax").SetValue(player, 100f);
			NonPublicCall(player, "InitializeStats");
			return player;
		}

		[UnityTest]
		public IEnumerator StandardProfile_RestoresAuthoredPlayerLimitsWithoutHealing()
		{
			yield return null;
			object player = CreatePlayerFixture(out GameObject playerObject);
			string oldProfile = RuleProfileRegistry.CurrentId;
			try
			{
				RuleProfileRegistry.SetCurrent(string.Empty);
				Field(player.GetType(), "m_numOfHeartsBase").SetValue(player, 3);
				Field(player.GetType(), "m_numOfHeartsMax").SetValue(player, 5);
				Field(player.GetType(), "m_numOfHeartsCurrent").SetValue(player, 2);
				Field(player.GetType(), "m_pleasureMaxBase").SetValue(player, 100f);
				Field(player.GetType(), "m_pleasureMax").SetValue(player, 500f);
				Field(player.GetType(), "m_pleasureCurrent").SetValue(player, 80f);
				Field(player.GetType(), "m_libidoMaxBase").SetValue(player, 100f);
				Field(player.GetType(), "m_libidoMax").SetValue(player, 500f);
				Field(player.GetType(), "m_libidoCurrent").SetValue(player, 80f);
				Field(player.GetType(), "m_healthMaxRuleBase").SetValue(player, 100f);
				Field(player.GetType(), "m_healthMax").SetValue(player, 200f);
				Field(player.GetType(), "m_healthCurrent").SetValue(player, 45f);
				Field(player.GetType(), "m_staminaCurrent").SetValue(player, 35f);

				PublicCall(player, "ApplyRuleProfileLimits", false);

				Assert.That(Field(player.GetType(), "m_numOfHeartsMax").GetValue(player), Is.EqualTo(3));
				Assert.That(Field(player.GetType(), "m_numOfHeartsCurrent").GetValue(player), Is.EqualTo(2));
				Assert.That(Field(player.GetType(), "m_pleasureMax").GetValue(player), Is.EqualTo(100f));
				Assert.That(Field(player.GetType(), "m_libidoMax").GetValue(player), Is.EqualTo(100f));
				Assert.That(Field(player.GetType(), "m_healthMax").GetValue(player), Is.EqualTo(100f));
				Assert.That((float)Field(player.GetType(), "m_healthCurrent").GetValue(player),
					Is.LessThanOrEqualTo(45f), "Changing profiles must not heal the player.");
				Assert.That((float)Field(player.GetType(), "m_staminaCurrent").GetValue(player),
					Is.LessThanOrEqualTo(35f), "Changing profiles must not refill stamina.");
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(playerObject);
				RuleProfileRegistry.SetCurrent(oldProfile);
			}
		}

		[Test]
		public void StandardProfile_RestoresAuthoredCameraZoom()
		{
			string oldProfile = RuleProfileRegistry.CurrentId;
			GameObject cameraObject = new GameObject("Rule profile camera regression");
			try
			{
				RuleProfileRegistry.SetCurrent(string.Empty);
				Camera camera = cameraObject.AddComponent<Camera>();
				camera.fieldOfView = 60f;
				camera.orthographicSize = 5f;
				Component cameraX = cameraObject.AddComponent(RuntimeType("CameraXGame"));

				camera.fieldOfView = 84f;
				camera.orthographicSize = 7f;
				PublicCall(cameraX, "ApplyRuleProfileZoom");

				Assert.That(camera.fieldOfView, Is.EqualTo(60f));
				Assert.That(camera.orthographicSize, Is.EqualTo(5f));
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(cameraObject);
				RuleProfileRegistry.SetCurrent(oldProfile);
			}
		}

		[Test]
		public void WeaponRange_IsCalculatedFromTheAuthoredValueWithoutMutation()
		{
			string oldProfile = RuleProfileRegistry.CurrentId;
			GameObject weaponObject = new GameObject("Rule profile weapon regression");
			weaponObject.SetActive(false);
			try
			{
				RuleProfileRegistry.SetCurrent(string.Empty);
				Component gun = weaponObject.AddComponent(RuntimeType("Gun"));
				FieldInfo range = Field(gun.GetType(), "m_distanceShootMax");
				range.SetValue(gun, 12.5f);

				Assert.That((float)NonPublicCall(gun, "GetRuleProfileRange"), Is.EqualTo(12.5f));
				Assert.That((float)NonPublicCall(gun, "GetRuleProfileRange"), Is.EqualTo(12.5f));
				Assert.That(range.GetValue(gun), Is.EqualTo(12.5f));
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(weaponObject);
				RuleProfileRegistry.SetCurrent(oldProfile);
			}
		}

		[UnityTest]
		public IEnumerator RefreshSelectedProfile_RemovesTrackedExperimentModifiers()
		{
			yield return null;
			object player = CreatePlayerFixture(out GameObject playerObject);
			object speedStat = PublicCall(player, "GetStat", "SpeedSprint");
			string oldProfile = RuleProfileRegistry.CurrentId;
			GameObject controllerObject = new GameObject("Rule profile modifier regression");
			Component controller = null;
			try
			{
				RuleProfileRegistry.SetCurrent(string.Empty);
				PublicCall(speedStat, "SetValueBase", 100f);
				float baseline = (float)PublicCall(speedStat, "GetValueTotal");
				controller = controllerObject.AddComponent(RuntimeType("ExternalRuleProfileController"));
				((Behaviour)controller).enabled = false;
				Field(controller.GetType(), "m_player").SetValue(controller, player);
				Field(controller.GetType(), "m_experimentModifierPlayer").SetValue(controller, player);
				Type statName = RuntimeType("StatNamePlayer");
				object speedSprint = Enum.Parse(statName, "SpeedSprint");
				NonPublicCall(controller, "AddPercentModifier", speedSprint, 25f);
				Assert.That((float)PublicCall(speedStat, "GetValueTotal"), Is.GreaterThan(baseline));

				PublicCall(controller, "RefreshSelectedProfile");
				Assert.That((float)PublicCall(speedStat, "GetValueTotal"), Is.EqualTo(baseline).Within(0.0001f));
			}
			finally
			{
				if (controller != null) NonPublicCall(controller, "ClearExperimentScaling");
				UnityEngine.Object.DestroyImmediate(controllerObject);
				UnityEngine.Object.DestroyImmediate(playerObject);
				RuleProfileRegistry.SetCurrent(oldProfile);
			}
		}
	}
}
