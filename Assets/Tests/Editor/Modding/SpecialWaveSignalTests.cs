using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaptivityReloaded.Modding.Tests
{
	// Two triggerSignal special waves armed at once: the wave start picks one of them. The other one's signal was not
	// spent, so it stays armed and gets its turn at a later wave. ManagerWave lives in Assembly-CSharp, reached by reflection.
	public sealed class SpecialWaveSignalTests
	{
		private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private static StageSpecialWaveDocument Wave(string i_id, string i_signal, float i_chance) => new StageSpecialWaveDocument
		{
			Id = i_id, DisplayName = i_id, MinimumWave = 1, TriggerSignal = i_signal, Chance = i_chance,
			// Not loaded in an EditMode test: the picked wave is skipped after the pick, which is all this test needs.
			Enemies = new List<StageSpecialWaveEnemyDocument> { new StageSpecialWaveEnemyDocument { Enemy = "example.waves:enemy/ghost", Weight = 1f } }
		};

		private static void Run(Action<Component, Type> i_test, params StageSpecialWaveDocument[] i_waves)
		{
			ValidationReport report = new ValidationReport();
			List<StageSpecialWaveDefinition> waves = StageSpecialWaveParser.Validate(i_waves.ToList(), report, "test", "stage.special-wave");
			Assert.That(report.IsValid, Is.True, string.Join("\n", report.Issues.Select(i_issue => i_issue.ToString())));
			Type waveType = Type.GetType("ManagerWave, Assembly-CSharp");
			Assert.That(waveType, Is.Not.Null);
			GameObject stage = new GameObject("special-wave signals");
			try
			{
				Component manager = stage.AddComponent(waveType);
				waveType.GetMethod("ConfigureModWave").Invoke(manager, new object[] { 4, waves });
				waveType.GetField("m_numWaveCurrent", Instance).SetValue(manager, 3);
				i_test(manager, waveType);
			}
			finally { Object.DestroyImmediate(stage); }
		}

		private static bool Armed(Component i_manager, Type i_type, string i_id) => (bool)i_type.GetMethod("IsSpecialWaveArmed").Invoke(i_manager, new object[] { i_id });
		private static void Notify(Component i_manager, Type i_type, string i_signal) => Assert.That((bool)i_type.GetMethod("NotifyStageSignal").Invoke(i_manager, new object[] { i_signal }), Is.True);
		private static void StartWave(Component i_manager, Type i_type) => i_type.GetMethod("SelectSpecialWave", Instance).Invoke(i_manager, null);

		[Test]
		public void TwoArmedWaves_ThePickedOneSpendsItsSignal_TheOtherStaysArmed()
		{
			Run((i_manager, i_type) =>
			{
				Notify(i_manager, i_type, "red-button");
				Notify(i_manager, i_type, "blue-button");
				StartWave(i_manager, i_type);
				bool red = Armed(i_manager, i_type, "red"), blue = Armed(i_manager, i_type, "blue");
				Assert.That(red ^ blue, Is.True, "One wave was picked and spent its signal; the other must still be armed (red " + red + ", blue " + blue + ").");
				StartWave(i_manager, i_type);
				Assert.That(Armed(i_manager, i_type, "red") || Armed(i_manager, i_type, "blue"), Is.False, "The next wave start picks the other one.");
			}, Wave("red", "red-button", 1f), Wave("blue", "blue-button", 1f));
		}

		[Test]
		public void AFailedChanceRoll_StillSpendsTheSignal()
		{
			// As documented: "the signal is consumed when its chance is evaluated".
			Run((i_manager, i_type) =>
			{
				Notify(i_manager, i_type, "never-button");
				StartWave(i_manager, i_type);
				Assert.That(Armed(i_manager, i_type, "never"), Is.False);
			}, Wave("never", "never-button", 0.000001f));
		}
	}
}
