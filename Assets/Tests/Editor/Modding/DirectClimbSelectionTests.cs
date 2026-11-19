using System.Collections.Generic;
using NUnit.Framework;

namespace CaptivityReloaded.Modding.Tests
{
	// Which ledge an original ground chaser climbs to when the player is above it, on the ledges of Core F.E.R.
	// (measured from stage_fer: every ledge sits 0.25 in from its platform's end, at the platform's top).
	// Player report, 2.2.3: from the theatre, standing on the dining platform that leads back to the start, every
	// Samantha kept teleporting onto the weapon-case platform beside it and dropping off again.
	public sealed class DirectClimbSelectionTests
	{
		private const int Floor = 1, Dining = 2, WeaponCase = 3, AtticStepLeft1 = 4, AtticStepLeft2 = 5, Attic = 6;
		private const float FloorY = -7.5f, FirstY = -0.75f, SecondY = 6.5f, AtticY = 14f;

		private static readonly ClimbLedge[] Fer =
		{
			new ClimbLedge(-75.75f, FloorY, Floor), new ClimbLedge(71.75f, FloorY, Floor),
			new ClimbLedge(-23.75f, FirstY, Dining), new ClimbLedge(23.75f, FirstY, Dining),
			new ClimbLedge(-38.25f, FirstY, WeaponCase), new ClimbLedge(-30.75f, FirstY, WeaponCase),
			new ClimbLedge(-73.75f, FirstY, AtticStepLeft1), new ClimbLedge(-67.25f, FirstY, AtticStepLeft1),
			new ClimbLedge(-68.75f, SecondY, AtticStepLeft2), new ClimbLedge(-62.25f, SecondY, AtticStepLeft2),
			new ClimbLedge(-62.75f, AtticY, Attic), new ClimbLedge(32.75f, AtticY, Attic)
		};

		private static int Platform(int i_index) { return i_index < 0 ? 0 : Fer[i_index].Platform; }

		// The report: she is anywhere on the theatre floor (x -61 to -30.5), the player on the dining platform.
		[TestCase(-55f)]
		[TestCase(-45f)]
		[TestCase(-36f)]
		[TestCase(-31f)]
		public void FromTheTheatre_SheClimbsThePlayersPlatform_NotTheWeaponCasePlatformBesideIt(float i_x)
		{
			int pick = DirectClimbSelection.PickOnPlayerPlatform(Fer, i_x, FloorY, Floor, Dining);
			Assert.That(Platform(pick), Is.EqualTo(Dining));
			Assert.That(Fer[pick].X, Is.EqualTo(-23.75f), "The dining platform's near ledge.");
		}

		// What she did before: the nearest ledge at the lowest height, whichever platform it was on. From the theatre
		// that is the weapon-case platform; from there the stage route walks her off it again, and round it goes.
		[Test]
		public void TheOldNearestLedgeRule_PicksTheWrongPlatformFromTheTheatre()
		{
			int pick = DirectClimbSelection.PickSteppingStone(Fer, -45f, FloorY, Floor, FirstY);
			Assert.That(Platform(pick), Is.EqualTo(WeaponCase), "This is the loop the players saw; the rule is now only the fallback for a stage with no route.");
		}

		// The attic is three climbs up. No ledge of it is in reach from the floor, so nothing is picked and the
		// stage's own route takes her up the steps.
		[Test]
		public void APlatformMoreThanOneClimbUp_IsLeftToTheStageRoute()
		{
			Assert.That(DirectClimbSelection.PickOnPlayerPlatform(Fer, -50f, FloorY, Floor, Attic), Is.EqualTo(-1));
			// From the second step the attic's left ledge is one climb away.
			int pick = DirectClimbSelection.PickOnPlayerPlatform(Fer, -66f, SecondY, AtticStepLeft2, Attic);
			Assert.That(pick, Is.GreaterThanOrEqualTo(0));
			Assert.That(Fer[pick].X, Is.EqualTo(-62.75f));
		}

		[Test]
		public void NothingIsPicked_OnThePlayersPlatform_OrWithoutOne_OrForALowerPlayer()
		{
			Assert.That(DirectClimbSelection.PickOnPlayerPlatform(Fer, 0f, FirstY, Dining, Dining), Is.EqualTo(-1));
			Assert.That(DirectClimbSelection.PickOnPlayerPlatform(Fer, 0f, FloorY, Floor, 0), Is.EqualTo(-1), "A player in the air has no platform.");
			Assert.That(DirectClimbSelection.PickOnPlayerPlatform(Fer, 0f, FirstY, Dining, Floor), Is.EqualTo(-1), "Down is a fall, not a climb.");
			Assert.That(DirectClimbSelection.PickOnPlayerPlatform(null, 0f, FloorY, Floor, Dining), Is.EqualTo(-1));
		}

		// The Barn, first build: a horse on the landing under the hatch (x 64.25 to 67, 4.5 up) with the player in
		// the yard. The yard's nearest ledge was one climb up but 28 units west, across the stable: she walked off
		// the landing for it, fell, was led back up by the stage's route, and walked off again. A ledge is only a
		// straight climb when she can get under it without leaving what she stands on.
		[Test]
		public void ALedgeAcrossAGap_IsNotAStraightClimb()
		{
			const float reach = 0.375f;
			Assert.That(DirectClimbSelection.IsOverOwnPlatform(36f, 64.25f, 67f, reach), Is.False, "The yard's edge from the landing.");
			Assert.That(DirectClimbSelection.IsOverOwnPlatform(64.25f, 62f, 64.25f, reach), Is.True, "The hatch's own edge, straight above the landing's end.");
			Assert.That(DirectClimbSelection.IsOverOwnPlatform(64.5f, 62f, 64.25f, reach), Is.True, "Within her reach of the end.");
			Assert.That(DirectClimbSelection.IsOverOwnPlatform(64.7f, 62f, 64.25f, reach), Is.False);
			// F.E.R.'s theatre floor runs under the dining platform's ledge: that climb stays a straight one.
			Assert.That(DirectClimbSelection.IsOverOwnPlatform(-23.75f, -76f, 72f, reach), Is.True);
		}

		[Test]
		public void TheFallback_TakesTheLowestLedgeAbove_ThatIsNotAboveThePlayer()
		{
			// Player in the attic, she on the floor by the left steps: the first step, not a higher ledge.
			int pick = DirectClimbSelection.PickSteppingStone(Fer, -66f, FloorY, Floor, AtticY);
			Assert.That(Platform(pick), Is.EqualTo(AtticStepLeft1));
			// Player on the first level: the second step and the attic are above her, so they are out.
			pick = DirectClimbSelection.PickSteppingStone(new List<ClimbLedge> { Fer[8], Fer[10] }, -66f, FloorY, Floor, FirstY);
			Assert.That(pick, Is.EqualTo(-1));
		}
	}
}
