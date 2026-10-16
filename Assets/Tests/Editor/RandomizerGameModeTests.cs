using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public sealed class RandomizerGameModeTests
{
	private static List<RandomizerGameMode.Candidate> Candidates()
	{
		return new List<RandomizerGameMode.Candidate>
		{
			new RandomizerGameMode.Candidate { Name = "Pistol", Type = WeaponType.Pistol, Weight = 0 },
			new RandomizerGameMode.Candidate { Name = "Revolver", Type = WeaponType.Pistol, Weight = 2 },
			new RandomizerGameMode.Candidate { Name = "SMG", Type = WeaponType.Smg, Weight = 3 },
			new RandomizerGameMode.Candidate { Name = "NP40", Type = WeaponType.Smg, Weight = 2 },
			new RandomizerGameMode.Candidate { Name = "Tenelli", Type = WeaponType.Shotgun, Weight = 4 },
			new RandomizerGameMode.Candidate { Name = "Sawed-Off", Type = WeaponType.Shotgun, Weight = 3 },
			new RandomizerGameMode.Candidate { Name = "M4B1", Type = WeaponType.Rifle, Weight = 4 },
			new RandomizerGameMode.Candidate { Name = "MG32", Type = WeaponType.Rifle, Weight = 5 }
		};
	}

	[Test]
	public void SelectLoadout_IsDeterministicForSeed()
	{
		string[] first = RandomizerGameMode.SelectLoadout(Candidates(), 742).Select(item => item.Name).ToArray();
		string[] second = RandomizerGameMode.SelectLoadout(Candidates(), 742).Select(item => item.Name).ToArray();
		Assert.That(second, Is.EqualTo(first));
	}

	[Test]
	public void SelectLoadout_AlwaysFitsInventoryAndHasNoDuplicates()
	{
		for (int seed = 0; seed < 250; seed++)
		{
			List<RandomizerGameMode.Candidate> loadout = RandomizerGameMode.SelectLoadout(Candidates(), seed);
			Assert.That(loadout, Is.Not.Empty, "Seed " + seed);
			Assert.That(loadout.Count, Is.LessThanOrEqualTo(3), "Seed " + seed);
			Assert.That(loadout.Sum(item => item.Weight), Is.LessThanOrEqualTo(RandomizerGameMode.MaximumLoadoutWeight), "Seed " + seed);
			Assert.That(loadout.Select(item => item.Name).Distinct().Count(), Is.EqualTo(loadout.Count), "Seed " + seed);
		}
	}

	[Test]
	public void SelectLoadout_FallsBackWhenArchetypesCannotBeCompleted()
	{
		List<RandomizerGameMode.Candidate> candidates = new List<RandomizerGameMode.Candidate>
		{
			new RandomizerGameMode.Candidate { Name = "Pistol", Type = WeaponType.Pistol, Weight = 0 }
		};
		List<RandomizerGameMode.Candidate> loadout = RandomizerGameMode.SelectLoadout(candidates, 1);
		Assert.That(loadout.Select(item => item.Name), Is.EqualTo(new[] { "Pistol" }));
	}
}
