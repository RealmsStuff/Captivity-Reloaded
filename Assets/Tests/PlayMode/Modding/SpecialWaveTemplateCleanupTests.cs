using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CaptivityReloaded.Modding.Tests
{
	// A special-wave enemy with an `appearance` spawns from a hidden copy of the enemy that ManagerWave makes at the
	// scene root. The wave end destroys those copies, but leaving the stage mid-wave destroys the stage first, and the
	// copies stayed in the scene for the rest of the session, one more set per run. ManagerWave lives in Assembly-CSharp.
	public sealed class SpecialWaveTemplateCleanupTests
	{
		private static Type RuntimeType(string i_name)
		{
			Type type = AppDomain.CurrentDomain.GetAssemblies().Select(i_assembly => i_assembly.GetType(i_name, false)).FirstOrDefault(i_type => i_type != null);
			Assert.That(type, Is.Not.Null, "Runtime type was not loaded: " + i_name);
			return type;
		}

		[UnityTest]
		public IEnumerator LeavingTheStageMidWave_DestroysTheAppearanceCopies()
		{
			Type waveType = RuntimeType("ManagerWave");
			GameObject stage = new GameObject("stage with a special wave running");
			Component manager = stage.AddComponent(waveType);
			// Its Update reads the game's input, which this test scene doesn't have.
			((Behaviour)manager).enabled = false;
			// Like CreateAppearanceVariant's copy: an inactive enemy at the scene root.
			GameObject copy = new GameObject("zombie [abby appearance]");
			copy.SetActive(false);
			Component npc = copy.AddComponent(RuntimeType("Zombie"));
			IList templates = (IList)waveType.GetField("m_specialWaveTemplates", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager);
			templates.Add(npc);
			try
			{
				Object.Destroy(stage);
				yield return null;
				yield return null;
				Assert.That(copy == null, Is.True, "The appearance copy must go with the stage that made it.");
			}
			finally
			{
				if (copy != null) Object.Destroy(copy);
				if (stage != null) Object.Destroy(stage);
			}
		}
	}
}
