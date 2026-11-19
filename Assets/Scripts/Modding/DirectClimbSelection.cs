using System.Collections.Generic;

namespace CaptivityReloaded.Modding
{
	// A ledge a ground chaser could pull itself onto: where it is and which platform it belongs to.
	public struct ClimbLedge
	{
		public float X;
		public float Y;
		public int Platform;

		public ClimbLedge(float i_x, float i_y, int i_platform)
		{
			X = i_x;
			Y = i_y;
			Platform = i_platform;
		}
	}

	// Which ledge an original ground chaser climbs straight to when the player stands higher than it does.
	// The ledge has to belong to the platform the player is on. The first version took the nearest ledge of any
	// platform at the lowest height above the enemy: in F.E.R.'s theatre that was the weapon-case platform when the
	// player stood on the dining platform next to it, so the enemy climbed the wrong platform, walked off it towards
	// the right one, and climbed it again for as long as the player stayed up there.
	public static class DirectClimbSelection
	{
		// Lower than this is a step, not a climb.
		public const float MinimumRise = 0.5f;
		// One storey. Core's authored climb links rise 5.5 to 7.5; a platform further up is reached over the
		// stage's own route instead.
		public const float MaximumRise = 8.5f;

		// The nearest ledge of the player's platform that is one climb above the enemy's feet, or -1.
		public static int PickOnPlayerPlatform(IReadOnlyList<ClimbLedge> i_ledges, float i_feetX, float i_feetY, int i_currentPlatform, int i_playerPlatform)
		{
			if (i_ledges == null || i_playerPlatform == 0 || i_playerPlatform == i_currentPlatform) return -1;
			int best = -1;
			float bestHorizontal = float.PositiveInfinity;
			for (int index = 0; index < i_ledges.Count; index++)
			{
				ClimbLedge ledge = i_ledges[index];
				if (ledge.Platform != i_playerPlatform) continue;
				float rise = ledge.Y - i_feetY;
				if (rise < MinimumRise || rise > MaximumRise) continue;
				float horizontal = Abs(ledge.X - i_feetX);
				if (horizontal < bestHorizontal)
				{
					best = index;
					bestHorizontal = horizontal;
				}
			}
			return best;
		}

		// For a stage that has no route to the player: the lowest ledge above the enemy that is not above the
		// player, the nearest one among equals. A stepping stone that may or may not lead on.
		public static int PickSteppingStone(IReadOnlyList<ClimbLedge> i_ledges, float i_feetX, float i_feetY, int i_currentPlatform, float i_playerFeetY)
		{
			if (i_ledges == null) return -1;
			int best = -1;
			float bestRise = float.PositiveInfinity;
			float bestHorizontal = float.PositiveInfinity;
			for (int index = 0; index < i_ledges.Count; index++)
			{
				ClimbLedge ledge = i_ledges[index];
				if (ledge.Platform == 0 || ledge.Platform == i_currentPlatform) continue;
				float rise = ledge.Y - i_feetY;
				if (rise < MinimumRise || ledge.Y > i_playerFeetY + 0.75f) continue;
				float horizontal = Abs(ledge.X - i_feetX);
				if (rise < bestRise - 0.1f || (Abs(rise - bestRise) <= 0.1f && horizontal < bestHorizontal))
				{
					best = index;
					bestRise = rise;
					bestHorizontal = horizontal;
				}
			}
			return best;
		}

		// Whether the enemy can walk under a ledge without leaving the platform it stands on (between that
		// platform's ends, give or take its reach). A ledge of the player's platform can be one climb up and still
		// be across a gap: from the landing under a hatch a horse walked off the landing for the far edge of the
		// yard, fell, was led back up by the stage's route, and walked off again.
		public static bool IsOverOwnPlatform(float i_ledgeX, float i_platformMinX, float i_platformMaxX, float i_reach)
		{
			return i_ledgeX >= i_platformMinX - i_reach && i_ledgeX <= i_platformMaxX + i_reach;
		}

		private static float Abs(float i_value) { return i_value < 0f ? -i_value : i_value; }
	}
}
