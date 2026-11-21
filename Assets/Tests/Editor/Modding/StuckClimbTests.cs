using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	// An enemy whose path climbs higher than its jump reaches never gets its head near the ledge, so every climb try
	// fails. 2.2.3 cancels the try and lets the pathfinder retry, which keeps climbs from skipping ledges, but on such
	// a climb it retries forever: the enemy stays stuck below and the wave can't end. After a few failed tries at the
	// same climb it is now placed on the ledge. XAIWalker and ClimbRetryCounter live in Assembly-CSharp.
	public sealed class StuckClimbTests
	{
		private static Type RuntimeType(string i_name)
		{
			return AppDomain.CurrentDomain.GetAssemblies().Select(i_assembly => i_assembly.GetType(i_name, false)).FirstOrDefault(i_type => i_type != null);
		}

		private static object NewCounter()
		{
			Type counter = RuntimeType("ClimbRetryCounter");
			Assert.That(counter, Is.Not.Null, "XAIWalker needs a way to give up on a climb that keeps failing.");
			return Activator.CreateInstance(counter);
		}

		private static bool Fail(object i_counter, Vector2 i_target) => (bool)i_counter.GetType().GetMethod("RecordFailure").Invoke(i_counter, new object[] { i_target });

		[Test]
		public void AClimbThatKeepsFailing_EndsWithTheEnemyPlacedOnTheLedge()
		{
			object counter = NewCounter();
			Vector2 ledge = new Vector2(4f, 9f);
			Assert.That(Fail(counter, ledge), Is.False, "The first failed try is retried, so climbs still don't skip ledges.");
			Assert.That(Fail(counter, ledge + new Vector2(0.1f, 0f)), Is.False);
			Assert.That(Fail(counter, ledge), Is.True, "The third failed try at the same climb places the enemy.");
			Assert.That(Fail(counter, ledge), Is.False, "The count starts over after placing.");
		}

		[Test]
		public void ADifferentClimbOrASuccess_StartsTheCountOver()
		{
			object counter = NewCounter();
			Vector2 first = new Vector2(4f, 9f), second = new Vector2(12f, 6f);
			Fail(counter, first);
			Fail(counter, first);
			Assert.That(Fail(counter, second), Is.False, "Another climb has its own tries.");
			counter.GetType().GetMethod("Reset").Invoke(counter, null);
			Assert.That(Fail(counter, second), Is.False);
			Assert.That(Fail(counter, second), Is.False, "A climb that started reset the count.");
			Assert.That(Fail(counter, second), Is.True);
		}

		[Test]
		public void XAIWalker_UsesTheCounterForItsClimbs()
		{
			Type walker = RuntimeType("XAIWalker");
			Assert.That(walker, Is.Not.Null);
			Assert.That(walker.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Any(i_field => i_field.FieldType.Name == "ClimbRetryCounter"), Is.True);
		}
	}
}
