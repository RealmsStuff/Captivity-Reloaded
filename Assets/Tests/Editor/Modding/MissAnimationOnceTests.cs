using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	// A missAnimation may hold attackHit events of its own, or be the attack's own clip. Those hits miss too, and each
	// miss started the miss clip again from that hit's time, so the attack kept replaying its landing (forever, when the
	// miss clip is the attack clip) and never ended. The miss now plays once per attack.
	public sealed class MissAnimationOnceTests
	{
		[Test]
		public void AHitInsideTheMissClip_DoesNotStartTheMissClipAgain()
		{
			System.Type enemyType = System.Type.GetType("ModularEnemy, Assembly-CSharp");
			System.Type controllerType = System.Type.GetType("ModularEnemyAnimationController, Assembly-CSharp");
			Assert.That(enemyType, Is.Not.Null);
			Assert.That(controllerType, Is.Not.Null);
			GameObject host = new GameObject("miss-once-test");
			try
			{
				Component enemy = host.AddComponent(enemyType);
				Component controller = host.AddComponent(controllerType);
				List<EnemyAnimationFrameDefinition> frames = new List<EnemyAnimationFrameDefinition> { new EnemyAnimationFrameDefinition { Time = 0f } };
				EnemyAnimationDefinition animation = new EnemyAnimationDefinition();
				animation.Clips["attack"] = new EnemyAnimationClipDefinition
				{
					DurationSeconds = 1f, Frames = frames,
					Events = new List<EnemyAnimationEventDefinition> { new EnemyAnimationEventDefinition { Time = 0.1f, Type = "attackHit" } }
				};
				// The landing clip has a hit of its own at 0.5 s; there is no target, so it misses as well.
				animation.Clips["miss"] = new EnemyAnimationClipDefinition
				{
					DurationSeconds = 1f, Frames = frames,
					Events = new List<EnemyAnimationEventDefinition> { new EnemyAnimationEventDefinition { Time = 0.5f, Type = "attackHit" } }
				};
				Set(controller, "m_animation", animation);
				Set(enemy, "m_modAnimation", controller);
				Set(enemy, "m_isAttacking", true);
				Set(enemy, "m_animationAttackCooldownStarted", true);
				Set(enemy, "m_animationAttackHitsRemaining", 1);
				Set(enemy, "m_animationAttackDefinition", new EnemyAttackDefinition { Id = "hit", Type = "melee", Animation = "attack", MissAnimation = "miss" });
				controllerType.GetMethod("Play", new[] { typeof(string), typeof(bool) }).Invoke(controller, new object[] { "attack", true });
				MethodInfo apply = controllerType.GetMethod("ApplyEvents", BindingFlags.Instance | BindingFlags.NonPublic);

				apply.Invoke(controller, new object[] { animation.Clips["attack"], 0.25f, enemy });
				Assert.That(Get(controller, "m_clipName"), Is.EqualTo("miss"), "Fixture: the attack's hit missed and the landing took over.");
				float started = (float)Get(controller, "m_started");
				float endsAt = (float)Get(enemy, "m_animationAttackEndsAt");

				// The landing plays on to 0.6 s of its own time and passes its own hit.
				apply.Invoke(controller, new object[] { animation.Clips["miss"], 0.6f, enemy });
				Assert.That((float)Get(controller, "m_started"), Is.EqualTo(started), "The landing must play on, not start again from its own hit.");
				Assert.That((float)Get(enemy, "m_animationAttackEndsAt"), Is.EqualTo(endsAt), "The attack still ends when the landing ends.");
			}
			finally
			{
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
