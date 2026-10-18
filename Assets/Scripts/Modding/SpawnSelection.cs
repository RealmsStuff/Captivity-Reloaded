using System.Collections.Generic;

namespace CaptivityReloaded.Modding
{
	// Weighted choice among a spawner's enemies, where mod entries may also wait for a minimum wave. Core
	// entries carry weight 1 and wave 0. Entries not yet unlocked take no share; if nothing is unlocked
	// (a spawner made only of waiting variants), every entry is eligible so the spawner still spawns.
	public static class SpawnSelection
	{
		public static int Pick(IReadOnlyList<float> i_weights, IReadOnlyList<int> i_minimumWaves, int i_wave, float i_roll01)
		{
			int count = i_weights == null ? 0 : i_weights.Count;
			if (count == 0) return 0;
			float total = 0f;
			for (int index = 0; index < count; index++) if (Unlocked(i_minimumWaves, index, i_wave)) total += Weight(i_weights, index);
			bool anyUnlocked = total > 0f;
			if (!anyUnlocked) for (int index = 0; index < count; index++) total += Weight(i_weights, index);
			if (total <= 0f) return 0;
			float roll = total * (i_roll01 < 0f ? 0f : i_roll01 > 1f ? 1f : i_roll01);
			int last = 0;
			for (int index = 0; index < count; index++)
			{
				if (anyUnlocked && !Unlocked(i_minimumWaves, index, i_wave)) continue;
				last = index;
				roll -= Weight(i_weights, index);
				if (roll <= 0f) return index;
			}
			return last;
		}

		private static float Weight(IReadOnlyList<float> i_weights, int i_index) { return i_weights[i_index] > 0f ? i_weights[i_index] : 0f; }

		private static bool Unlocked(IReadOnlyList<int> i_minimumWaves, int i_index, int i_wave)
		{
			return i_minimumWaves == null || i_index >= i_minimumWaves.Count || i_minimumWaves[i_index] <= i_wave;
		}
	}
}
