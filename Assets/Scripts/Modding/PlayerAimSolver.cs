using System;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	// The player's arm as the aim solver sees it. Everything here is in body space: the player faces +x, a bone at
	// rotation 0 hangs straight down, and a positive rotation turns it anticlockwise (forward and up for an arm).
	// The offsets are the child's local position in its parent bone, read from the rig, so the solver follows the
	// real joints instead of assuming straight bones.
	[Serializable]
	public struct AimArmRig
	{
		public Vector2 Elbow;
		public Vector2 Wrist;
		public Vector2 Hold;
		public float HoldRotation;

		// Alex.prefab: rArmLower in rArmUpper, rHand in rArmLower, PointHoldWeapon in rHand.
		public static AimArmRig Core => new AimArmRig
		{
			Elbow = new Vector2(-0.01f, -0.443f), Wrist = new Vector2(0.006f, -0.365f),
			Hold = new Vector2(-0.012f, -0.136f), HoldRotation = -90f
		};
	}

	// A hold as the game has always described one: each bone's rotation as an offset from the aim angle.
	[Serializable]
	public struct AimHoldPose
	{
		public float RightUpper, RightLower, RightHand;
		public bool UseLeftHand;
		public float LeftUpper, LeftLower, LeftHand;
		// A pose a pack authored (holdPose) is kept as written: the gun is not turned onto the cursor and never pulled in.
		public bool Authored;

		public static AimHoldPose OneHanded => new AimHoldPose { RightUpper = 60f, RightLower = 90f, RightHand = 90f };
		public static AimHoldPose TwoHanded1 => TwoHanded(-45f, 65f, 90f, 60f, 75f, 80f);
		public static AimHoldPose TwoHanded2 => TwoHanded(-65f, 60f, 90f, 60f, 75f, 80f);
		public static AimHoldPose TwoHanded3 => TwoHanded(-65f, 100f, 90f, 65f, 100f, 90f);

		private static AimHoldPose TwoHanded(float i_ru, float i_rl, float i_rh, float i_lu, float i_ll, float i_lh)
		{
			return new AimHoldPose { RightUpper = i_ru, RightLower = i_rl, RightHand = i_rh, UseLeftHand = true, LeftUpper = i_lu, LeftLower = i_ll, LeftHand = i_lh };
		}

		public static AimHoldPose FromDefinition(WeaponHoldPoseDefinition i_pose)
		{
			return new AimHoldPose
			{
				RightUpper = i_pose.RightUpper, RightLower = i_pose.RightLower, RightHand = i_pose.RightHand, UseLeftHand = i_pose.UseLeftHand,
				LeftUpper = i_pose.LeftUpper, LeftLower = i_pose.LeftLower, LeftHand = i_pose.LeftHand, Authored = true
			};
		}
	}

	public struct AimArmPose
	{
		public float Upper, Lower, Hand;
	}

	public struct AimSolution
	{
		public AimArmPose Right;
		public bool UseLeft;
		public AimArmPose Left;
		// The barrel's direction, and where the grip and the muzzle are from the shoulder.
		public float GunAngle;
		public Vector2 HoldPoint;
		public Vector2 Muzzle;
		public float Pull;
	}

	// Poses the player's arms on a gun. A hold is the pose Core always used at full reach; from there the gun can be
	// pulled back along its own barrel toward the shoulder and both arms follow by two-bone IK, and the whole hold is
	// turned so the barrel, not the shoulder, points at the cursor.
	public static class PlayerAimSolver
	{
		// Inside this distance of the shoulder the cursor says nothing about a direction: the old aim is kept.
		public const float DeadZone = 0.3f;
		// How far from straight ahead the gun turns. Facing follows the cursor, so more is only asked for while the
		// facing is locked, and then the gun stops here instead of pointing through the player's own back.
		public const float MaxPitch = 110f;
		// Close to the shoulder and past the limit the raw angle jumps; there the gun turns at this rate instead.
		public const float NearTurnRate = 720f;
		// The wrist never comes closer to the shoulder than this (an elbow folded to about 145 degrees).
		public const float MinReach = 0.27f;
		// A pulled-in gun stops this far short of the cursor.
		public const float MuzzleMargin = 0.15f;
		// A pulled-in arm's elbow goes back, but not up: the upper arm stays this far round from pointing ahead
		// (-90 hangs straight down, -180 points straight back). Aiming down, the gun pulls in less to keep to it.
		// A two-handed hold already has the grip hand under the shoulder, so its elbow may go nearly straight back.
		public const float ElbowBackLimit = -140f, TwoHandedElbowBackLimit = -165f;
		// The furthest a two-handed gun without a stock slides back: the grip hand passes under the shoulder, which
		// no reach stops, so it is stopped here, with the gun tucked under the arm.
		public const float TwoHandedPull = 0.35f;
		// No hold slides back further than this, whatever its wrists allow.
		public const float OneHandedPull = 0.6f;
		// Shots from a gun pulled all the way in spread this many times wider than from one at full reach.
		public const float PulledInSpread = 2f;
		private const float ReachEpsilon = 0.002f;

		public static Vector2 Direction(float i_degrees)
		{
			float radians = i_degrees * Mathf.Deg2Rad;
			return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
		}

		public static Vector2 Rotate(Vector2 i_vector, float i_degrees)
		{
			float radians = i_degrees * Mathf.Deg2Rad, cos = Mathf.Cos(radians), sin = Mathf.Sin(radians);
			return new Vector2(i_vector.x * cos - i_vector.y * sin, i_vector.x * sin + i_vector.y * cos);
		}

		public static float Angle(Vector2 i_vector)
		{
			return Mathf.Atan2(i_vector.y, i_vector.x) * Mathf.Rad2Deg;
		}

		// The aim angle for a cursor at i_toAim from the shoulder: 0 straight ahead, positive up.
		public static float AimAngle(Vector2 i_toAim, float i_previous, float i_deltaTime)
		{
			float distance = i_toAim.magnitude;
			if (distance < DeadZone) return Mathf.Clamp(i_previous, -MaxPitch, MaxPitch);
			float raw = Angle(i_toAim);
			bool pastLimit = Mathf.Abs(raw) > MaxPitch;
			float target = Mathf.Clamp(raw, -MaxPitch, MaxPitch);
			if (!pastLimit && distance >= DeadZone * 2f) return target;
			// Not MoveTowardsAngle: between the two limits the short way round is through the player's back.
			return Mathf.MoveTowards(Mathf.Clamp(i_previous, -MaxPitch, MaxPitch), target, NearTurnRate * Mathf.Max(0f, i_deltaTime));
		}

		// Where the wrist ends up for a bone pose, from the shoulder.
		public static Vector2 WristPosition(AimArmRig i_rig, float i_upper, float i_lower)
		{
			return Rotate(i_rig.Elbow, i_upper) + Rotate(i_rig.Wrist, i_lower);
		}

		public static Vector2 HoldPosition(AimArmRig i_rig, AimArmPose i_pose)
		{
			return WristPosition(i_rig, i_pose.Upper, i_pose.Lower) + Rotate(i_rig.Hold, i_pose.Hand);
		}

		// Two-bone IK: the upper and lower arm rotations that put the wrist on i_wrist. Of the two answers only the
		// one with the forearm folded forward of the upper arm is taken, the way an elbow bends. A target out of reach
		// is met as far along its direction as the arm goes.
		public static AimArmPose SolveArm(AimArmRig i_rig, Vector2 i_wrist, float i_hand)
		{
			float upperLength = i_rig.Elbow.magnitude, lowerLength = i_rig.Wrist.magnitude;
			float distance = Mathf.Clamp(i_wrist.magnitude, Mathf.Abs(upperLength - lowerLength) + ReachEpsilon, upperLength + lowerLength - ReachEpsilon);
			float toTarget = i_wrist.sqrMagnitude > 1e-10f ? Angle(i_wrist) : 0f;
			float cosine = (upperLength * upperLength + distance * distance - lowerLength * lowerLength) / (2f * upperLength * distance);
			float upperDirection = toTarget - Mathf.Acos(Mathf.Clamp(cosine, -1f, 1f)) * Mathf.Rad2Deg;
			Vector2 elbow = Direction(upperDirection) * upperLength;
			float lowerDirection = Angle(Direction(toTarget) * distance - elbow);
			return new AimArmPose
			{
				Upper = upperDirection - Angle(i_rig.Elbow),
				// Kept within half a turn of the upper arm so the forearm never takes the long way round.
				Lower = upperDirection - Angle(i_rig.Elbow) + Mathf.DeltaAngle(upperDirection - Angle(i_rig.Elbow), lowerDirection - Angle(i_rig.Wrist)),
				Hand = i_hand
			};
		}

		// How far the forearm is folded forward of the upper arm; negative would be an elbow bent backward.
		public static float ElbowBend(AimArmRig i_rig, AimArmPose i_pose)
		{
			return Mathf.DeltaAngle(i_pose.Upper + Angle(i_rig.Elbow), i_pose.Lower + Angle(i_rig.Wrist));
		}

		private static float GunOffset(AimArmRig i_rig, AimHoldPose i_pose)
		{
			return i_pose.RightHand + i_rig.HoldRotation;
		}

		// How far this hold can be pulled back along the barrel before a wrist is at MinReach.
		public static float MaxPull(AimArmRig i_rig, AimHoldPose i_pose)
		{
			if (i_pose.Authored) return 0f;
			Vector2 along = Direction(GunOffset(i_rig, i_pose));
			float pull = WristPull(WristPosition(i_rig, i_pose.RightUpper, i_pose.RightLower), along);
			if (i_pose.UseLeftHand) pull = Mathf.Min(TwoHandedPull, pull, WristPull(WristPosition(i_rig, i_pose.LeftUpper, i_pose.LeftLower), along));
			return Mathf.Clamp(pull, 0f, OneHandedPull);
		}

		// How far a wrist can slide back along the barrel before it is MinReach from the shoulder. A wrist whose
		// line passes the shoulder further off than that is never stopped by it.
		private static float WristPull(Vector2 i_wrist, Vector2 i_along)
		{
			float forward = Vector2.Dot(i_wrist, i_along);
			float side = i_wrist.x * i_along.y - i_wrist.y * i_along.x;
			if (Mathf.Abs(side) >= MinReach) return float.PositiveInfinity;
			return forward - Mathf.Sqrt(MinReach * MinReach - side * side);
		}

		// How much wider shots spread for a gun pulled in by i_pull of the i_maxPull it could be.
		public static float SpreadMultiplier(float i_pull, float i_maxPull)
		{
			if (i_maxPull <= 0f) return 1f;
			return Mathf.Lerp(1f, PulledInSpread, Mathf.Clamp01(i_pull / i_maxPull));
		}

		public static float ElbowLimit(AimHoldPose i_pose)
		{
			return i_pose.UseLeftHand ? TwoHandedElbowBackLimit : ElbowBackLimit;
		}

		// The pull that leaves the muzzle MuzzleMargin short of a cursor i_aimDistance from the shoulder.
		public static float DesiredPull(AimArmRig i_rig, AimHoldPose i_pose, Vector2 i_muzzle, float i_aimDistance)
		{
			float limit = MaxPull(i_rig, i_pose);
			if (limit <= 0f) return 0f;
			float offset = GunOffset(i_rig, i_pose);
			AimArmPose rest = new AimArmPose { Upper = i_pose.RightUpper, Lower = i_pose.RightLower, Hand = i_pose.RightHand };
			float muzzleForward = Vector2.Dot(HoldPosition(i_rig, rest) + Rotate(i_muzzle, offset), Direction(offset));
			return Mathf.Clamp(muzzleForward + MuzzleMargin - i_aimDistance, 0f, limit);
		}

		// The arm pose for a hold aimed at i_aimAngle with the cursor i_aimDistance away and the gun pulled back by
		// i_pull. i_muzzle is the muzzle in the gun's own space (x along the barrel); without one the hold is only
		// turned to the aim angle, as it always was.
		public static AimSolution Solve(AimArmRig i_rig, AimHoldPose i_pose, bool i_hasMuzzle, Vector2 i_muzzle, float i_aimAngle, float i_aimDistance, float i_pull)
		{
			float offset = GunOffset(i_rig, i_pose);
			Vector2 along = Direction(offset);
			float pull = Mathf.Clamp(i_pull, 0f, MaxPull(i_rig, i_pose));
			AimArmPose right = new AimArmPose { Upper = i_pose.RightUpper, Lower = i_pose.RightLower, Hand = i_pose.RightHand };
			AimArmPose left = new AimArmPose { Upper = i_pose.LeftUpper, Lower = i_pose.LeftLower, Hand = i_pose.LeftHand };
			if (pull > 0f)
			{
				// At no pull the hold is used exactly as written; pulled in, each wrist slides back with the gun.
				Vector2 rightWrist = WristPosition(i_rig, right.Upper, right.Lower);
				AimArmPose rest = right;
				right = SolveArm(i_rig, rightWrist - along * pull, rest.Hand);
				float elbowLimit = ElbowLimit(i_pose);
				if (UpperArmDirection(i_rig, rest, right, i_aimAngle) < elbowLimit)
				{
					// The elbow swings further back the more the gun comes in: find the pull that stops it at the
					// limit. A hold that is past the limit as written is simply not pulled in.
					float low = 0f, high = UpperArmDirection(i_rig, rest, rest, i_aimAngle) < elbowLimit ? 0f : pull;
					for (int step = 0; step < 14; step++)
					{
						float middle = (low + high) * 0.5f;
						if (UpperArmDirection(i_rig, rest, SolveArm(i_rig, rightWrist - along * middle, rest.Hand), i_aimAngle) < elbowLimit) high = middle;
						else low = middle;
					}
					pull = low;
					right = pull > 0f ? SolveArm(i_rig, rightWrist - along * pull, rest.Hand) : rest;
				}
				if (i_pose.UseLeftHand && pull > 0f) left = SolveArm(i_rig, WristPosition(i_rig, left.Upper, left.Lower) - along * pull, left.Hand);
			}
			Vector2 hold = HoldPosition(i_rig, right);
			Vector2 muzzle = hold + Rotate(i_muzzle, offset);
			float turn = i_aimAngle;
			if (i_hasMuzzle && !i_pose.Authored)
			{
				// The barrel runs beside the shoulder, not through it. Turn the hold so the barrel's line, not the
				// shoulder's, passes through the cursor. A cursor nearer than the muzzle cannot be pointed at this
				// way, so the correction is taken as if it were just past the muzzle.
				float side = along.x * muzzle.y - along.y * muzzle.x;
				float reach = Mathf.Max(i_aimDistance, Vector2.Dot(muzzle, along) + DeadZone);
				turn -= Mathf.Asin(Mathf.Clamp(side / Mathf.Max(reach, 0.01f), -0.7f, 0.7f)) * Mathf.Rad2Deg;
			}
			return new AimSolution
			{
				Right = Turned(right, turn), UseLeft = i_pose.UseLeftHand, Left = Turned(left, turn),
				GunAngle = offset + turn, HoldPoint = Rotate(hold, turn), Muzzle = Rotate(muzzle, turn), Pull = pull
			};
		}

		// Which way the upper arm points in body space once the hold is turned to the aim: 0 ahead, -90 down,
		// -180 straight back. Counted on from the hold as written (i_rest) without wrapping round, so it changes
		// smoothly however far the hold is turned and however far the elbow has gone back from there.
		public static float UpperArmDirection(AimArmRig i_rig, AimArmPose i_rest, AimArmPose i_pose, float i_turn)
		{
			return i_rest.Upper + Angle(i_rig.Elbow) + i_turn + Mathf.DeltaAngle(i_rest.Upper, i_pose.Upper);
		}

		private static AimArmPose Turned(AimArmPose i_pose, float i_turn)
		{
			return new AimArmPose { Upper = i_pose.Upper + i_turn, Lower = i_pose.Lower + i_turn, Hand = i_pose.Hand + i_turn };
		}
	}
}
