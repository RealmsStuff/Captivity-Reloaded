using NUnit.Framework;

namespace CaptivityReloaded.Modding.Tests
{
	// Spawner.Spawn picks through SpawnSelection: Core entries weigh 1 from wave 0, joined mod enemies carry
	// their own weight and minimum wave.
	public sealed class SpawnSelectionTests
	{
		[Test]
		public void Pick_SharesByWeightAmongUnlockedEntries()
		{
			float[] weights = { 1f, 1f, 1f };
			int[] waves = { 0, 0, 4 };
			// Before wave 4 the third entry takes no share: the roll splits the first two.
			Assert.That(SpawnSelection.Pick(weights, waves, 1, 0.1f), Is.EqualTo(0));
			Assert.That(SpawnSelection.Pick(weights, waves, 1, 0.6f), Is.EqualTo(1));
			Assert.That(SpawnSelection.Pick(weights, waves, 1, 0.99f), Is.EqualTo(1));
			// From wave 4 it takes a third.
			Assert.That(SpawnSelection.Pick(weights, waves, 4, 0.2f), Is.EqualTo(0));
			Assert.That(SpawnSelection.Pick(weights, waves, 4, 0.5f), Is.EqualTo(1));
			Assert.That(SpawnSelection.Pick(weights, waves, 4, 0.8f), Is.EqualTo(2));
		}

		[Test]
		public void Pick_WeightsSkewTheShare()
		{
			// A joined enemy at weight 0.25 beside one Core choice has a 20% share.
			float[] weights = { 1f, 0.25f };
			Assert.That(SpawnSelection.Pick(weights, new[] { 0, 0 }, 0, 0.79f), Is.EqualTo(0));
			Assert.That(SpawnSelection.Pick(weights, new[] { 0, 0 }, 0, 0.81f), Is.EqualTo(1));
		}

		[Test]
		public void Pick_FallsBackToEveryEntryWhenNoneIsUnlocked()
		{
			float[] weights = { 1f, 2f };
			int[] waves = { 5, 5 };
			Assert.That(SpawnSelection.Pick(weights, waves, 1, 0.2f), Is.EqualTo(0));
			Assert.That(SpawnSelection.Pick(weights, waves, 1, 0.5f), Is.EqualTo(1));
			// Missing wave entries count as wave 0; an empty list picks the first entry.
			Assert.That(SpawnSelection.Pick(weights, null, 0, 0.9f), Is.EqualTo(1));
			Assert.That(SpawnSelection.Pick(new float[0], null, 0, 0.5f), Is.EqualTo(0));
		}
	}
}
