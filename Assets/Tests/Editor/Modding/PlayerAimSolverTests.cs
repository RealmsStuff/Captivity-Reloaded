using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public class PlayerAimSolverTests
	{
		private static readonly AimArmRig Rig = AimArmRig.Core;
		private static readonly Vector2 PistolMuzzle = new Vector2(0.539f, 0.159f);
		private static readonly Vector2 RifleMuzzle = new Vector2(1.313f, 0.204f);

		private static IEnumerable<TestCaseData> CoreHolds()
		{
			yield return new TestCaseData(AimHoldPose.OneHanded).SetName("OneHanded");
			yield return new TestCaseData(AimHoldPose.TwoHanded1).SetName("TwoHanded1");
			yield return new TestCaseData(AimHoldPose.TwoHanded2).SetName("TwoHanded2");
			yield return new TestCaseData(AimHoldPose.TwoHanded3).SetName("TwoHanded3");
		}

		// With nothing to correct for and no pull, a hold is the fixed angles the game always used plus the aim.
		[TestCaseSource(nameof(CoreHolds))]
		public void HoldAtFullReachIsTheCorePose(AimHoldPose pose)
		{
			foreach (float aim in new[] { -110f, -45f, 0f, 30f, 90f })
			{
				AimSolution solved = PlayerAimSolver.Solve(Rig, pose, false, Vector2.zero, aim, 5f, 0f);
				Assert.That(solved.Right.Upper, Is.EqualTo(pose.RightUpper + aim).Within(1e-4f));
				Assert.That(solved.Right.Lower, Is.EqualTo(pose.RightLower + aim).Within(1e-4f));
				Assert.That(solved.Right.Hand, Is.EqualTo(pose.RightHand + aim).Within(1e-4f));
				Assert.That(solved.UseLeft, Is.EqualTo(pose.UseLeftHand));
				if (!pose.UseLeftHand) continue;
				Assert.That(solved.Left.Upper, Is.EqualTo(pose.LeftUpper + aim).Within(1e-4f));
				Assert.That(solved.Left.Lower, Is.EqualTo(pose.LeftLower + aim).Within(1e-4f));
				Assert.That(solved.Left.Hand, Is.EqualTo(pose.LeftHand + aim).Within(1e-4f));
			}
		}

		// The IK answers the same pose the hold was written in, so there is no step where pulling in begins.
		[TestCaseSource(nameof(CoreHolds))]
		public void SolvingAHoldsOwnWristGivesTheHoldBack(AimHoldPose pose)
		{
			Vector2 wrist = PlayerAimSolver.WristPosition(Rig, pose.RightUpper, pose.RightLower);
			AimArmPose solved = PlayerAimSolver.SolveArm(Rig, wrist, pose.RightHand);
			Assert.That(Mathf.DeltaAngle(solved.Upper, pose.RightUpper), Is.EqualTo(0f).Within(0.05f));
			Assert.That(Mathf.DeltaAngle(solved.Lower, pose.RightLower), Is.EqualTo(0f).Within(0.05f));
		}

		[TestCase(0.539f, 0.159f, false)]
		[TestCase(1.313f, 0.204f, true)]
		public void BarrelPointsAtTheCursor(float muzzleX, float muzzleY, bool twoHanded)
		{
			AimHoldPose pose = twoHanded ? AimHoldPose.TwoHanded1 : AimHoldPose.OneHanded;
			Vector2 muzzle = new Vector2(muzzleX, muzzleY);
			foreach (float distance in new[] { 2.5f, 4f, 9f })
			{
				for (float aim = -110f; aim <= 110f; aim += 10f)
				{
					AimSolution solved = PlayerAimSolver.Solve(Rig, pose, true, muzzle, aim, distance, 0f);
					Vector2 cursor = PlayerAimSolver.Direction(aim) * distance;
					Vector2 barrel = PlayerAimSolver.Direction(solved.GunAngle);
					Vector2 toCursor = cursor - solved.Muzzle;
					Assert.That(Vector2.Dot(toCursor, barrel), Is.GreaterThan(0f), "cursor ahead of the muzzle");
					Assert.That(barrel.x * toCursor.y - barrel.y * toCursor.x, Is.EqualTo(0f).Within(0.002f), $"aim {aim} distance {distance}");
				}
			}
		}

		// Pulled in, the gun has only slid back along its own barrel, and the hand is still on the grip.
		[Test]
		public void PulledInGunSlidesBackAlongItsBarrel()
		{
			AimHoldPose pose = AimHoldPose.OneHanded;
			float limit = PlayerAimSolver.MaxPull(Rig, pose);
			Assert.That(limit, Is.GreaterThan(0.4f).And.LessThan(0.7f));
			AimSolution rest = PlayerAimSolver.Solve(Rig, pose, false, Vector2.zero, 25f, 5f, 0f);
			for (float pull = 0.01f; pull <= limit; pull += 0.01f)
			{
				AimSolution solved = PlayerAimSolver.Solve(Rig, pose, false, Vector2.zero, 25f, 5f, pull);
				Assert.That(solved.GunAngle, Is.EqualTo(25f).Within(1e-3f));
				Vector2 moved = solved.HoldPoint - rest.HoldPoint;
				Assert.That(Vector2.Distance(moved, -PlayerAimSolver.Direction(25f) * pull), Is.LessThan(0.002f), $"pull {pull}");
				Assert.That(Vector2.Distance(PlayerAimSolver.HoldPosition(Rig, solved.Right), solved.HoldPoint), Is.LessThan(1e-4f));
				Assert.That(solved.Right.Hand, Is.EqualTo(pose.RightHand + 25f).Within(1e-3f));
			}
		}

		[TestCaseSource(nameof(CoreHolds))]
		public void ElbowsOnlyFoldTheirOwnWay(AimHoldPose pose)
		{
			float limit = PlayerAimSolver.MaxPull(Rig, pose);
			for (int step = 0; step <= 40; step++)
			{
				float pull = limit * step / 40f;
				for (float aim = -110f; aim <= 110f; aim += 5f)
				{
					AimSolution solved = PlayerAimSolver.Solve(Rig, pose, true, PistolMuzzle, aim, 1f + step * 0.1f, pull);
					Assert.That(PlayerAimSolver.ElbowBend(Rig, solved.Right), Is.InRange(0f, 170f), $"right, aim {aim} pull {pull}");
					if (solved.UseLeft) Assert.That(PlayerAimSolver.ElbowBend(Rig, solved.Left), Is.InRange(0f, 170f), $"left, aim {aim} pull {pull}");
				}
			}
		}

		// No pop anywhere: a small move of the cursor or of the pull is a small move of every bone.
		[TestCaseSource(nameof(CoreHolds))]
		public void PoseChangesSmoothlyWithAimDistanceAndPull(AimHoldPose pose)
		{
			Vector2 muzzle = pose.UseLeftHand ? RifleMuzzle : PistolMuzzle;
			float limit = PlayerAimSolver.MaxPull(Rig, pose);
			AimSolution? last = null;
			for (float aim = -110f; aim <= 110f; aim += 0.5f)
			{
				AimSolution solved = PlayerAimSolver.Solve(Rig, pose, true, muzzle, aim, 3f, limit * 0.5f);
				// A left arm at 0.99 of its reach turns two degrees for a centimetre of pull; a pop would be tens of degrees.
				if (last.HasValue) AssertClose(last.Value, solved, 3f, $"aim {aim}");
				last = solved;
			}
			last = null;
			// Fine steps: a left arm that starts at 0.99 of its reach turns steeply as the gun first comes in.
			for (float distance = 0.05f; distance <= 6f; distance += 0.004f)
			{
				float pull = PlayerAimSolver.DesiredPull(Rig, pose, muzzle, distance);
				AimSolution solved = PlayerAimSolver.Solve(Rig, pose, true, muzzle, 20f, distance, pull);
				if (last.HasValue) AssertClose(last.Value, solved, 4f, $"distance {distance}");
				last = solved;
			}
		}

		private static void AssertClose(AimSolution a, AimSolution b, float degrees, string where)
		{
			Assert.That(Mathf.Abs(Mathf.DeltaAngle(a.Right.Upper, b.Right.Upper)), Is.LessThan(degrees), "right upper, " + where);
			Assert.That(Mathf.Abs(Mathf.DeltaAngle(a.Right.Lower, b.Right.Lower)), Is.LessThan(degrees), "right lower, " + where);
			Assert.That(Mathf.Abs(Mathf.DeltaAngle(a.Right.Hand, b.Right.Hand)), Is.LessThan(degrees), "right hand, " + where);
			if (!a.UseLeft) return;
			Assert.That(Mathf.Abs(Mathf.DeltaAngle(a.Left.Upper, b.Left.Upper)), Is.LessThan(degrees), "left upper, " + where);
			Assert.That(Mathf.Abs(Mathf.DeltaAngle(a.Left.Lower, b.Left.Lower)), Is.LessThan(degrees), "left lower, " + where);
		}

		[Test]
		public void PullLeavesTheMuzzleJustShortOfANearCursor()
		{
			AimHoldPose pose = AimHoldPose.OneHanded;
			float limit = PlayerAimSolver.MaxPull(Rig, pose);
			Assert.That(PlayerAimSolver.DesiredPull(Rig, pose, PistolMuzzle, 6f), Is.EqualTo(0f), "far cursor: full reach");
			Assert.That(PlayerAimSolver.DesiredPull(Rig, pose, PistolMuzzle, 0.4f), Is.EqualTo(limit).Within(1e-5f), "cursor at the body: all the way in");
			float pull = PlayerAimSolver.DesiredPull(Rig, pose, PistolMuzzle, 1.2f);
			Assert.That(pull, Is.GreaterThan(0f).And.LessThan(limit));
			AimSolution solved = PlayerAimSolver.Solve(Rig, pose, false, PistolMuzzle, 0f, 1.2f, pull);
			Assert.That(solved.Muzzle.x, Is.EqualTo(1.2f - PlayerAimSolver.MuzzleMargin).Within(0.002f));
		}

		// Pulled in while aiming down, the elbow would swing up behind the shoulder: the gun comes in less instead.
		[Test]
		public void PulledInElbowGoesBackButNotUp()
		{
			foreach (AimHoldPose twoHanded in new[] { AimHoldPose.TwoHanded1, AimHoldPose.TwoHanded2 })
			{
				for (float aim = 110f; aim >= -110f; aim -= 1f)
				{
					AimSolution solved = PlayerAimSolver.Solve(Rig, twoHanded, false, Vector2.zero, aim, 0.5f, PlayerAimSolver.TwoHandedPull);
					AimArmPose restOfTwoHanded = new AimArmPose { Upper = twoHanded.RightUpper, Lower = twoHanded.RightLower, Hand = twoHanded.RightHand };
					AimArmPose unturned = new AimArmPose { Upper = solved.Right.Upper - aim, Lower = solved.Right.Lower - aim, Hand = solved.Right.Hand - aim };
					float allowedTwoHanded = Mathf.Min(PlayerAimSolver.TwoHandedElbowBackLimit, PlayerAimSolver.UpperArmDirection(Rig, restOfTwoHanded, restOfTwoHanded, aim));
					Assert.That(PlayerAimSolver.UpperArmDirection(Rig, restOfTwoHanded, unturned, aim), Is.GreaterThanOrEqualTo(allowedTwoHanded - 0.05f), $"two-handed, aim {aim}");
				}
			}
			AimHoldPose pose = AimHoldPose.OneHanded;
			float limit = PlayerAimSolver.MaxPull(Rig, pose);
			float lastPull = -1f;
			AimSolution? last = null;
			for (float aim = 110f; aim >= -110f; aim -= 0.5f)
			{
				AimSolution solved = PlayerAimSolver.Solve(Rig, pose, false, Vector2.zero, aim, 0.5f, limit);
				Vector2 elbow = PlayerAimSolver.Rotate(Rig.Elbow, solved.Right.Upper);
				float direction = Mathf.DeltaAngle(0f, PlayerAimSolver.Angle(elbow) + 90f) - 90f;
				// At the very bottom of the aim the hold as written is already a little past the limit: there it is left alone.
				Vector2 restElbow = PlayerAimSolver.Rotate(Rig.Elbow, pose.RightUpper + aim);
				float allowed = Mathf.Min(PlayerAimSolver.ElbowBackLimit, Mathf.DeltaAngle(0f, PlayerAimSolver.Angle(restElbow) + 90f) - 90f);
				Assert.That(direction, Is.GreaterThanOrEqualTo(allowed - 0.05f), $"aim {aim}");
				if (aim >= 0f) Assert.That(solved.Pull, Is.EqualTo(limit).Within(1e-5f), $"level and above the gun comes all the way in, aim {aim}");
				if (lastPull >= 0f) Assert.That(Mathf.Abs(solved.Pull - lastPull), Is.LessThan(0.02f), $"aim {aim}");
				if (last.HasValue) AssertClose(last.Value, solved, 3f, $"aim {aim}");
				lastPull = solved.Pull;
				last = solved;
			}
			Assert.That(lastPull, Is.LessThan(limit * 0.6f), "aiming far down it is held well out");
		}

		// A two-handed gun without a stock tucks back under the arm by a fixed amount; the hold with the grip hand
		// up at the shoulder has nowhere to go.
		[Test]
		public void TwoHandedHoldsPullInByAFixedAmount()
		{
			Assert.That(PlayerAimSolver.MaxPull(Rig, AimHoldPose.TwoHanded1), Is.EqualTo(PlayerAimSolver.TwoHandedPull));
			Assert.That(PlayerAimSolver.MaxPull(Rig, AimHoldPose.TwoHanded2), Is.EqualTo(PlayerAimSolver.TwoHandedPull));
			Assert.That(PlayerAimSolver.MaxPull(Rig, AimHoldPose.TwoHanded3), Is.EqualTo(0f));
			Assert.That(PlayerAimSolver.MaxPull(Rig, AimHoldPose.OneHanded), Is.LessThan(PlayerAimSolver.OneHandedPull));
		}

		// Pulled in, both hands stay on the gun: each wrist has moved back along the barrel by the same amount.
		[Test]
		public void TwoHandedPullKeepsBothHandsOnTheGun()
		{
			AimHoldPose pose = AimHoldPose.TwoHanded1;
			AimSolution rest = PlayerAimSolver.Solve(Rig, pose, false, Vector2.zero, 10f, 5f, 0f);
			AimSolution solved = PlayerAimSolver.Solve(Rig, pose, false, Vector2.zero, 10f, 5f, 0.3f);
			Assert.That(solved.Pull, Is.EqualTo(0.3f).Within(1e-5f));
			Vector2 back = -PlayerAimSolver.Direction(10f) * 0.3f;
			Vector2 rightMoved = PlayerAimSolver.WristPosition(Rig, solved.Right.Upper, solved.Right.Lower) - PlayerAimSolver.WristPosition(Rig, rest.Right.Upper, rest.Right.Lower);
			Vector2 leftMoved = PlayerAimSolver.WristPosition(Rig, solved.Left.Upper, solved.Left.Lower) - PlayerAimSolver.WristPosition(Rig, rest.Left.Upper, rest.Left.Lower);
			Assert.That(Vector2.Distance(rightMoved, back), Is.LessThan(0.002f));
			Assert.That(Vector2.Distance(leftMoved, back), Is.LessThan(0.002f));
			Assert.That(solved.Left.Hand, Is.EqualTo(rest.Left.Hand).Within(1e-4f));
			Assert.That(solved.GunAngle, Is.EqualTo(rest.GunAngle).Within(1e-4f));
		}

		[Test]
		public void SpreadWidensAsTheGunComesIn()
		{
			Assert.That(PlayerAimSolver.SpreadMultiplier(0f, 0.5f), Is.EqualTo(1f));
			Assert.That(PlayerAimSolver.SpreadMultiplier(0.25f, 0.5f), Is.EqualTo(1f + (PlayerAimSolver.PulledInSpread - 1f) * 0.5f).Within(1e-5f));
			Assert.That(PlayerAimSolver.SpreadMultiplier(0.5f, 0.5f), Is.EqualTo(PlayerAimSolver.PulledInSpread));
			Assert.That(PlayerAimSolver.SpreadMultiplier(0.9f, 0.5f), Is.EqualTo(PlayerAimSolver.PulledInSpread), "never more than all the way in");
			Assert.That(PlayerAimSolver.SpreadMultiplier(0.3f, 0f), Is.EqualTo(1f), "a gun that cannot pull in keeps its spread");
		}

		[Test]
		public void AuthoredPoseIsKeptAsWritten()
		{
			var definition = new WeaponHoldPoseDefinition { RightUpper = 40, RightLower = 70, RightHand = 100, UseLeftHand = true, LeftUpper = 10, LeftLower = 20, LeftHand = 30 };
			AimHoldPose pose = AimHoldPose.FromDefinition(definition);
			Assert.That(PlayerAimSolver.MaxPull(Rig, pose), Is.EqualTo(0f));
			Assert.That(PlayerAimSolver.DesiredPull(Rig, pose, PistolMuzzle, 0.5f), Is.EqualTo(0f));
			AimSolution solved = PlayerAimSolver.Solve(Rig, pose, true, PistolMuzzle, 35f, 1f, 0.3f);
			Assert.That(solved.Right.Upper, Is.EqualTo(75f).Within(1e-4f));
			Assert.That(solved.Right.Lower, Is.EqualTo(105f).Within(1e-4f));
			Assert.That(solved.Right.Hand, Is.EqualTo(135f).Within(1e-4f));
			Assert.That(solved.Left.Upper, Is.EqualTo(45f).Within(1e-4f));
			Assert.That(solved.Left.Lower, Is.EqualTo(55f).Within(1e-4f));
			Assert.That(solved.Left.Hand, Is.EqualTo(65f).Within(1e-4f));
		}

		[Test]
		public void CursorOnTheShoulderKeepsTheAim()
		{
			Assert.That(PlayerAimSolver.AimAngle(new Vector2(0.05f, -0.1f), 37f, 0.016f), Is.EqualTo(37f));
			Assert.That(PlayerAimSolver.AimAngle(new Vector2(-0.2f, 0.1f), -80f, 0.016f), Is.EqualTo(-80f));
			Assert.That(PlayerAimSolver.AimAngle(new Vector2(3f, 3f), 37f, 0.016f), Is.EqualTo(45f).Within(1e-4f), "a far cursor is followed at once");
		}

		[Test]
		public void GunStopsAtTheLimitAndNeverSwingsThroughTheBack()
		{
			Assert.That(PlayerAimSolver.AimAngle(new Vector2(-3f, 0.5f), PlayerAimSolver.MaxPitch, 0.016f), Is.EqualTo(PlayerAimSolver.MaxPitch));
			// The cursor crosses behind the player from above to below: the gun comes down the front, a little a frame.
			float angle = PlayerAimSolver.MaxPitch;
			float largestStep = 0f;
			for (int frame = 0; frame < 60; frame++)
			{
				float next = PlayerAimSolver.AimAngle(new Vector2(-3f, -0.5f), angle, 1f / 60f);
				Assert.That(next, Is.InRange(-PlayerAimSolver.MaxPitch, PlayerAimSolver.MaxPitch));
				largestStep = Mathf.Max(largestStep, Mathf.Abs(next - angle));
				angle = next;
			}
			Assert.That(angle, Is.EqualTo(-PlayerAimSolver.MaxPitch));
			Assert.That(largestStep, Is.LessThanOrEqualTo(PlayerAimSolver.NearTurnRate / 60f + 1e-3f));
		}

		[Test]
		public void JustOutsideTheDeadZoneTheGunTurnsAtALimitedRate()
		{
			float next = PlayerAimSolver.AimAngle(new Vector2(0f, PlayerAimSolver.DeadZone * 1.5f), 0f, 1f / 60f);
			Assert.That(next, Is.EqualTo(PlayerAimSolver.NearTurnRate / 60f).Within(1e-3f));
		}

		[Test]
		public void TargetsOutOfReachAreMetWithoutNonsense()
		{
			foreach (Vector2 target in new[] { Vector2.zero, new Vector2(0.01f, 0f), new Vector2(5f, 2f), new Vector2(-4f, -4f), new Vector2(0f, 0.808f) })
			{
				AimArmPose pose = PlayerAimSolver.SolveArm(Rig, target, 90f);
				Assert.That(float.IsNaN(pose.Upper) || float.IsNaN(pose.Lower), Is.False, target.ToString());
				Assert.That(PlayerAimSolver.ElbowBend(Rig, pose), Is.InRange(0f, 180f), target.ToString());
			}
			Vector2 reachable = new Vector2(0.5f, -0.2f);
			AimArmPose solved = PlayerAimSolver.SolveArm(Rig, reachable, 90f);
			Assert.That(Vector2.Distance(PlayerAimSolver.WristPosition(Rig, solved.Upper, solved.Lower), reachable), Is.LessThan(1e-4f));
		}

		// The game's side of it: which hold a gun gets, and that only a one-handed gun without an authored pose pulls in.
		[Test]
		public void GunsMapToTheirHolds()
		{
			Type gunType = Type.GetType("Gun, Assembly-CSharp");
			Type manager = Type.GetType("ManagerArmWeaponPlayer, Assembly-CSharp");
			Type holdType = Type.GetType("GunHoldType, Assembly-CSharp");
			Assert.That(gunType, Is.Not.Null);
			Assert.That(manager, Is.Not.Null);
			var gunObject = new GameObject("aim-hold-test");
			gunObject.SetActive(false);
			try
			{
				Component gun = gunObject.AddComponent(gunType);
				var field = gunType.GetField("m_gunHoldType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				var getPose = manager.GetMethod("GetHoldPose");
				var canPullIn = manager.GetMethod("CanPullIn");
				var expected = new Dictionary<string, AimHoldPose>
				{
					{ "OneHanded", AimHoldPose.OneHanded }, { "TwoHanded1", AimHoldPose.TwoHanded1 },
					{ "TwoHanded2", AimHoldPose.TwoHanded2 }, { "TwoHanded3", AimHoldPose.TwoHanded3 }
				};
				foreach (KeyValuePair<string, AimHoldPose> pair in expected)
				{
					field.SetValue(gun, Enum.Parse(holdType, pair.Key));
					Assert.That((AimHoldPose)getPose.Invoke(null, new object[] { gun }), Is.EqualTo(pair.Value), pair.Key);
					Assert.That((bool)canPullIn.Invoke(null, new object[] { gun }), Is.EqualTo(pair.Key == "OneHanded"), pair.Key);
				}
				// stats.stock decides it either way round, and a copy of the gun (how the game hands guns out) keeps it.
				var hasStock = gunType.GetMethod("HasStock");
				var configure = gunType.GetMethod("ConfigureModWeaponStats");
				field.SetValue(gun, Enum.Parse(holdType, "TwoHanded1"));
				Assert.That((bool)hasStock.Invoke(gun, null), Is.True);
				configure.Invoke(gun, new object[] { new WeaponStatsDefinition { Stock = false } });
				Assert.That((bool)hasStock.Invoke(gun, null), Is.False);
				Assert.That((bool)canPullIn.Invoke(null, new object[] { gun }), Is.True, "a two-handed gun a pack declares stockless pulls in");
				GameObject copy = UnityEngine.Object.Instantiate(gunObject);
				try { Assert.That((bool)hasStock.Invoke(copy.GetComponent(gunType), null), Is.False, "survives a copy"); }
				finally { UnityEngine.Object.DestroyImmediate(copy); }
				field.SetValue(gun, Enum.Parse(holdType, "OneHanded"));
				configure.Invoke(gun, new object[] { new WeaponStatsDefinition { Stock = true } });
				Assert.That((bool)hasStock.Invoke(gun, null), Is.True);
				Assert.That((bool)canPullIn.Invoke(null, new object[] { gun }), Is.False, "a one-handed gun a pack gives a stock does not");
				configure.Invoke(gun, new object[] { new WeaponStatsDefinition { Stock = false } });
				var authored = new WeaponHoldPoseDefinition { RightUpper = 1, RightLower = 2, RightHand = 3 };
				gunType.GetMethod("ConfigureModHoldPose").Invoke(gun, new object[] { authored });
				AimHoldPose pose = (AimHoldPose)getPose.Invoke(null, new object[] { gun });
				Assert.That(pose.Authored, Is.True);
				Assert.That(pose.RightLower, Is.EqualTo(2f));
				Assert.That((bool)canPullIn.Invoke(null, new object[] { gun }), Is.False);
			}
			finally { UnityEngine.Object.DestroyImmediate(gunObject); }
		}
	}
}
