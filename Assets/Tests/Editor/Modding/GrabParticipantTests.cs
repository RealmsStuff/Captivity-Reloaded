using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	// The extra enemies in a grab (downedFinisher participants).
	public sealed class GrabParticipantTests
	{
		private static readonly System.Type EnemyType = System.Type.GetType("ModularEnemy, Assembly-CSharp");
		private static readonly System.Type ControllerType = System.Type.GetType("ModularEnemyAnimationController, Assembly-CSharp");
		private static readonly System.Type FinisherType = System.Type.GetType("ModularEnemyFinisher, Assembly-CSharp");

		// ModularEnemy.UpdateAnim played idle or move every frame on a participant, so its phase clip showed for one frame
		// at the phase change and then idle again. It stands aside while a grab holds the enemy, as for its own grab.
		[Test]
		public void AParticipant_KeepsPlayingItsPhaseClip()
		{
			Assert.That(EnemyType, Is.Not.Null);
			Assert.That(ControllerType, Is.Not.Null);
			Assert.That(FinisherType, Is.Not.Null);
			GameObject host = new GameObject("grab-participant-clip-test");
			object reserved = FinisherType.GetField("ReservedParticipants", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
			MethodInfo reserve = reserved.GetType().GetMethod("Add"), release = reserved.GetType().GetMethod("Remove");
			try
			{
				host.AddComponent<Rigidbody2D>();
				Component enemy = host.AddComponent(EnemyType);
				Component controller = host.AddComponent(ControllerType);
				List<EnemyAnimationFrameDefinition> frames = new List<EnemyAnimationFrameDefinition> { new EnemyAnimationFrameDefinition { Time = 0f } };
				EnemyAnimationDefinition animation = new EnemyAnimationDefinition();
				foreach (string clip in new[] { "idle", "move", "assist-hold" })
					animation.Clips[clip] = new EnemyAnimationClipDefinition { DurationSeconds = 1f, Loop = true, Frames = frames };
				Set(controller, "m_animation", animation);
				Set(enemy, "m_modAnimation", controller);
				MethodInfo play = ControllerType.GetMethod("Play", new[] { typeof(string), typeof(bool) });
				MethodInfo updateAnim = EnemyType.GetMethod("UpdateAnim");

				play.Invoke(controller, new object[] { "assist-hold", true });
				updateAnim.Invoke(enemy, null);
				Assert.That(Get(controller, "m_clipName"), Is.EqualTo("idle"), "Fixture: an enemy no grab holds stands idle.");

				play.Invoke(controller, new object[] { "assist-hold", true });
				reserve.Invoke(reserved, new object[] { enemy });
				Assert.That(FinisherType.GetMethod("IsReserved").Invoke(null, new object[] { enemy }), Is.True);
				updateAnim.Invoke(enemy, null);
				Assert.That(Get(controller, "m_clipName"), Is.EqualTo("assist-hold"), "The grab's phase clip must keep playing on its participant.");
			}
			finally
			{
				foreach (Component enemy in host.GetComponents(EnemyType)) release.Invoke(reserved, new object[] { enemy });
				Object.DestroyImmediate(host);
			}
		}

		private static void Set(object i_target, string i_field, object i_value)
		{
			for (System.Type type = i_target.GetType(); type != null; type = type.BaseType)
			{
				FieldInfo field = type.GetField(i_field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
				if (field == null) continue;
				field.SetValue(i_target, i_value);
				return;
			}
			Assert.Fail("No field " + i_field + " on " + i_target.GetType().Name);
		}

		private static object Get(object i_target, string i_field)
		{
			for (System.Type type = i_target.GetType(); type != null; type = type.BaseType)
			{
				FieldInfo field = type.GetField(i_field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
				if (field != null) return field.GetValue(i_target);
			}
			Assert.Fail("No field " + i_field + " on " + i_target.GetType().Name);
			return null;
		}
	}
}
