using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public class MuzzleShotTests
	{
		private readonly List<GameObject> m_objects = new List<GameObject>();
		// Far from anything else that may be loaded in the editor's scene.
		private static readonly Vector2 Base = new Vector2(5000f, 5000f);

		[TearDown]
		public void RemoveObjects()
		{
			foreach (GameObject created in m_objects) if (created != null) UnityEngine.Object.DestroyImmediate(created);
			m_objects.Clear();
		}

		private Collider2D Box(string name, float left, float right, Transform parent = null, float bottom = -1f, float top = 1f)
		{
			var created = new GameObject(name);
			if (parent != null) created.transform.SetParent(parent, false);
			else m_objects.Add(created);
			created.transform.position = Base + new Vector2((left + right) * 0.5f, (bottom + top) * 0.5f);
			BoxCollider2D box = created.AddComponent<BoxCollider2D>();
			box.size = new Vector2(right - left, top - bottom);
			return box;
		}

		private Transform Body(string name)
		{
			var created = new GameObject(name);
			m_objects.Add(created);
			return created.transform;
		}

		// A collider belongs to a body when it sits under an object named "enemy ...".
		private static object OwnerOf(Collider2D collider)
		{
			Transform root = collider.transform.root;
			return root.name.StartsWith("enemy", StringComparison.Ordinal) ? root : null;
		}

		private static RaycastHit2D[] Ray(float fromX, float toX)
		{
			Physics2D.SyncTransforms();
			return Physics2D.RaycastAll(Base + new Vector2(fromX, 0f), Vector2.right, toX - fromX)
				.Where(hit => Vector2.Distance(hit.collider.transform.position, Base) < 100f).ToArray();
		}

		private static Collider2D[] At(float x)
		{
			Physics2D.SyncTransforms();
			return Physics2D.OverlapCircleAll(Base + new Vector2(x, 0f), MuzzleShot.CoverRadius);
		}

		[Test]
		public void ShotFliesToAFarCursorAndAlongTheBarrelToANearOne()
		{
			Vector2 muzzle = new Vector2(1.5f, 1f), barrel = Vector2.right;
			Vector2 far = MuzzleShot.Direction(muzzle, barrel, new Vector2(6f, 4f));
			Assert.That(Vector2.Angle(far, new Vector2(4.5f, 3f)), Is.LessThan(0.01f), "at the cursor");
			Assert.That(MuzzleShot.Direction(muzzle, barrel, new Vector2(1.5f, 3f)), Is.EqualTo(barrel), "cursor level with the muzzle");
			Assert.That(MuzzleShot.Direction(muzzle, barrel, new Vector2(0.4f, 0.2f)), Is.EqualTo(barrel), "cursor behind the muzzle");
			Assert.That(MuzzleShot.Direction(muzzle, new Vector2(0f, 3f), new Vector2(1.5f, 0f)).y, Is.EqualTo(1f).Within(1e-5f), "barrel is normalised");
			Assert.That(MuzzleShot.Direction(muzzle, Vector2.zero, muzzle), Is.EqualTo(Vector2.right), "nothing to go on");
		}

		// Bringing the cursor in past the muzzle turns the shot from the cursor onto the barrel without a jump.
		[Test]
		public void DirectionNeverSnaps()
		{
			Vector2 muzzle = new Vector2(1.5f, 1f), barrel = PlayerAimSolver.Direction(20f);
			Vector2 side = PlayerAimSolver.Direction(110f) * 0.4f;
			Vector2? last = null;
			for (float ahead = 3f; ahead >= -1f; ahead -= 0.005f)
			{
				Vector2 direction = MuzzleShot.Direction(muzzle, barrel, muzzle + barrel * ahead + side);
				Assert.That(direction.magnitude, Is.EqualTo(1f).Within(1e-4f));
				if (last.HasValue) Assert.That(Vector2.Angle(last.Value, direction), Is.LessThan(2f), $"ahead {ahead}");
				last = direction;
			}
		}

		[Test]
		public void EnemyOnTheMuzzleIsNotHitButTheOneBehindIs()
		{
			Transform near = Body("enemy near");
			Box("torso", 1.2f, 1.8f, near);
			Box("far arm", 2.0f, 2.2f, near);
			Transform beyond = Body("enemy beyond");
			Collider2D beyondTorso = Box("torso", 3f, 3.5f, beyond);
			Collider2D wall = Box("wall", 6f, 6.3f);

			RaycastHit2D[] all = Ray(1.5f, 20f);
			Assert.That(all.Length, Is.EqualTo(4), "the ray itself sees both bodies and the wall (physics queries work in this test)");
			RaycastHit2D[] kept = MuzzleShot.HitsPastTheMuzzle(all, At(1.5f), OwnerOf);
			Assert.That(kept.Select(hit => hit.collider).ToArray(), Is.EqualTo(new[] { beyondTorso, wall }), "the whole near enemy is skipped, arm included");
		}

		[Test]
		public void EnemyAheadOfTheMuzzleIsHit()
		{
			Transform enemy = Body("enemy ahead");
			Collider2D torso = Box("torso", 1.7f, 2.3f, enemy);
			RaycastHit2D[] kept = MuzzleShot.HitsPastTheMuzzle(Ray(1.5f, 20f), At(1.5f), OwnerOf);
			Assert.That(kept.Select(hit => hit.collider).ToArray(), Is.EqualTo(new[] { torso }));
			Assert.That(kept[0].distance, Is.EqualTo(0.2f).Within(0.01f));
		}

		[Test]
		public void ThingsThatAreNotBodiesOnTheMuzzleStillStopTheShot()
		{
			Collider2D crate = Box("crate", 1.2f, 1.8f);
			RaycastHit2D[] kept = MuzzleShot.HitsPastTheMuzzle(Ray(1.5f, 20f), At(1.5f), OwnerOf);
			Assert.That(kept.Select(hit => hit.collider).ToArray(), Is.EqualTo(new[] { crate }));
			Assert.That(MuzzleShot.HitsPastTheMuzzle(new RaycastHit2D[0], At(1.5f), OwnerOf), Is.Empty);
			Assert.That(MuzzleShot.HitsPastTheMuzzle(null, null, OwnerOf), Is.Empty);
		}

		[Test]
		public void WallBetweenShoulderAndMuzzleStopsTheShot()
		{
			Transform enemy = Body("enemy between");
			Box("torso", 0.2f, 0.5f, enemy);
			Collider2D lever = Box("lever", 0.6f, 0.7f);
			Collider2D wall = Box("wall solid", 1f, 1.2f);
			Box("crate solid behind", 1.3f, 1.4f);
			Func<RaycastHit2D, bool> isSolid = hit => hit.collider.name.Contains("solid");

			Assert.That(MuzzleShot.TryFindBlock(Ray(0f, 1.5f), isSolid, OwnerOf, out RaycastHit2D[] hits), Is.True);
			Assert.That(hits.Select(hit => hit.collider).ToArray(), Is.EqualTo(new[] { lever, wall }), "what is not solid on the way is still shot, bodies are not, and it ends at the wall");
			Assert.That(MuzzleShot.TryFindBlock(Ray(0f, 0.9f), isSolid, OwnerOf, out hits), Is.False, "muzzle short of the wall: the shot leaves it");
			Assert.That(hits, Is.Null);
			Assert.That(MuzzleShot.TryFindBlock(null, isSolid, OwnerOf, out hits), Is.False);
		}

		// The option is on until switched off, and the switch is what the game then reads.
		[Test]
		public void RuleIsASavedSwitchThatDefaultsToOn()
		{
			Type rule = Type.GetType("GunMuzzleRule, Assembly-CSharp");
			Assert.That(rule, Is.Not.Null);
			string key = (string)rule.GetField("PreferenceKey").GetValue(null);
			var enabled = rule.GetProperty("Enabled");
			bool had = PlayerPrefs.HasKey(key);
			int before = PlayerPrefs.GetInt(key, 1);
			try
			{
				enabled.SetValue(null, false);
				Assert.That((bool)enabled.GetValue(null), Is.False);
				Assert.That(PlayerPrefs.GetInt(key, 1), Is.EqualTo(0));
				enabled.SetValue(null, true);
				Assert.That((bool)enabled.GetValue(null), Is.True);
				PlayerPrefs.DeleteKey(key);
				rule.GetField("s_isRead", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).SetValue(null, false);
				Assert.That((bool)enabled.GetValue(null), Is.True, "never set: on");
			}
			finally
			{
				if (had) PlayerPrefs.SetInt(key, before); else PlayerPrefs.DeleteKey(key);
				rule.GetField("s_isRead", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).SetValue(null, false);
			}
		}
	}
}
