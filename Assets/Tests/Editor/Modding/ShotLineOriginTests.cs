using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	// Where the line of a shot starts. Player report, 2.2.3: the Schockgewehr's beam came out of the player's neck.
	// Every Core gun traced its shot from the shoulder and drew the line from there too; the thick red beam of a gun
	// held at the hip made that plain. The line starts at the muzzle now. Gun lives in Assembly-CSharp, reached by
	// reflection.
	public sealed class ShotLineOriginTests
	{
		private static readonly Vector2 Shoulder = new Vector2(0f, 1.2f);
		// The Schockgewehr's muzzle point is 1.53 along the gun, which hangs below the shoulder in its two-handed hold.
		private static readonly Vector2 Muzzle = new Vector2(1.5f, 0.6f);

		private static Vector2 Pick(Vector2 i_shotOrigin, bool i_hasMuzzle, Vector2 i_muzzle, Vector2 i_posEnd)
		{
			Type gun = AppDomain.CurrentDomain.GetAssemblies().Select(i_assembly => i_assembly.GetType("Gun", false)).FirstOrDefault(i_type => i_type != null);
			Assert.That(gun, Is.Not.Null, "Runtime type was not loaded: Gun");
			MethodInfo method = gun.GetMethod("PickTracerOrigin", BindingFlags.Public | BindingFlags.Static);
			Assert.That(method, Is.Not.Null, "Gun.PickTracerOrigin is gone.");
			return (Vector2)method.Invoke(null, new object[] { i_shotOrigin, i_hasMuzzle, i_muzzle, i_posEnd });
		}

		[Test]
		public void TheLineStartsAtTheMuzzle_ForAHitAndForAShotThatHitNothing()
		{
			Assert.That(Pick(Shoulder, true, Muzzle, new Vector2(8f, 1f)), Is.EqualTo(Muzzle), "A hit down range.");
			// Bullet.HandleHits passes Vector2.zero when the shot ran its whole range.
			Assert.That(Pick(Shoulder, true, Muzzle, Vector2.zero), Is.EqualTo(Muzzle), "A shot into the air.");
		}

		// The shot is still traced from the shoulder, so it can stop at a wall the muzzle pokes through. The line
		// must not then run backwards from the muzzle to the wall.
		[Test]
		public void AShotStoppedBeforeTheMuzzle_KeepsTheShoulder()
		{
			Assert.That(Pick(Shoulder, true, Muzzle, new Vector2(0.8f, 1f)), Is.EqualTo(Shoulder));
		}

		[Test]
		public void AGunWithoutAMuzzlePoint_KeepsTheShoulder()
		{
			Assert.That(Pick(Shoulder, false, Shoulder, new Vector2(8f, 1f)), Is.EqualTo(Shoulder));
		}
	}
}
