using System;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

public enum BaseGameMode
{
	Standard = 0,
	Randomizer = 1
}

/// <summary>Native game modes. These are available without, and do not depend on, the mod loader.</summary>
public static class BaseGameModeRegistry
{
	public const string StandardId = "base:standard";
	public const string RandomizerId = "base:randomizer";
	private const string PreferenceKey = "BaseGameMode";

	public static BaseGameMode Current
	{
		get
		{
			int saved = PlayerPrefs.GetInt(PreferenceKey, (int)BaseGameMode.Standard);
			return Enum.IsDefined(typeof(BaseGameMode), saved) ? (BaseGameMode)saved : BaseGameMode.Standard;
		}
	}

	public static bool IsRandomizer => Current == BaseGameMode.Randomizer;

	public static void SetCurrent(BaseGameMode i_mode)
	{
		PlayerPrefs.SetInt(PreferenceKey, (int)i_mode);
		PlayerPrefs.Save();
	}
}

/// <summary>Owns Randomizer's constrained loadout roll and its weapon-lock policy.</summary>
public static class RandomizerGameMode
{
	public const int MaximumLoadoutWeight = 10;
	private static bool s_isGrantingLoadout;
	private static StatusPlayerHud s_statusOwner;
	private static StatusPlayerHudItem s_statusItem;

	public sealed class Candidate
	{
		public string Name;
		public WeaponType Type;
		public int Weight;
		public Gun Template;
	}

	public static bool IsWeaponLockActive
	{
		get
		{
			if (!BaseGameModeRegistry.IsRandomizer || CommonReferences.Instance == null) return false;
			ManagerStages stages = CommonReferences.Instance.GetManagerStages();
			return stages != null && stages.GetStageCurrent() != null && !(stages.GetStageCurrent() is StageHub);
		}
	}

	public static bool ShouldBlockWeaponAcquisition(PickUpable i_item)
	{
		return IsWeaponLockActive && !s_isGrantingLoadout && i_item is Gun;
	}

	public static bool ShouldBlockWeaponRemoval(PickUpable i_item)
	{
		return IsWeaponLockActive && i_item is Gun;
	}

	public static void NotifyLocked()
	{
		if (CommonReferences.Instance != null && CommonReferences.Instance.GetManagerHud() != null)
			CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification(
				"Randomizer loadout is locked", ColorTextNotification.Other, i_isContinues: false);
	}

	public static void ClearRunStatus()
	{
		if (s_statusOwner != null && s_statusItem != null) s_statusOwner.DestroyStatusItem(s_statusItem);
		s_statusOwner = null;
		s_statusItem = null;
	}

	public static bool TryGrantLoadout(Player i_player)
	{
		if (!BaseGameModeRegistry.IsRandomizer || i_player == null || Library.Instance == null || Library.Instance.Guns == null)
			return false;

		List<Candidate> loadout = SelectLoadout(BuildCoreCandidates(Library.Instance.Guns.GetAllGuns()), Environment.TickCount);
		if (loadout.Count == 0) return false;

		s_isGrantingLoadout = true;
		try
		{
			Gun first = null;
			foreach (Candidate choice in loadout)
			{
				Gun gun = UnityEngine.Object.Instantiate(choice.Template, i_player.transform.parent);
				gun.FillEntireGun();
				i_player.PickUp(gun, i_isDuplicate: false);
				if (first == null) first = gun;
			}
			if (first != null) i_player.EquipWeapon(first);
		}
		finally
		{
			s_isGrantingLoadout = false;
		}

		if (CommonReferences.Instance.GetManagerHud() != null)
		{
			List<string> names = new List<string>();
			foreach (Candidate choice in loadout) names.Add(choice.Name);
			string description = string.Join(" + ", names.ToArray()) + " | Loadout locked";
			CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification(
				"Randomizer: " + description, ColorTextNotification.Other, i_isContinues: false);
			ClearRunStatus();
			s_statusOwner = CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud();
			s_statusItem = s_statusOwner.CreateAndAddStatus("Randomizer", description, StatusPlayerHudItemColor.Special);
		}
		return true;
	}

	public static List<Candidate> BuildCoreCandidates(IEnumerable<Gun> i_guns)
	{
		List<Candidate> result = new List<Candidate>();
		if (i_guns == null) return result;
		HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (Gun gun in i_guns)
		{
			if (gun == null || string.IsNullOrWhiteSpace(gun.GetName()) || gun.GetWeaponType() == WeaponType.Special) continue;
			if (!gun.IsMarketable() && !string.Equals(gun.GetName(), "Pistol", StringComparison.OrdinalIgnoreCase)) continue;
			if (gun.GetName().IndexOf("dev", StringComparison.OrdinalIgnoreCase) >= 0 ||
				string.Equals(gun.GetName(), "Schockgewehr", StringComparison.OrdinalIgnoreCase)) continue;
			if (RuntimeContentIdentity.TryResolve(gun, out ContentId id, out ContentCategory _) && id.Namespace != "core") continue;
			if (!names.Add(gun.GetName())) continue;
			result.Add(new Candidate { Name = gun.GetName(), Type = gun.GetWeaponType(), Weight = gun.GetWeight(), Template = gun });
		}
		return result;
	}

	public static List<Candidate> SelectLoadout(IList<Candidate> i_candidates, int i_seed)
	{
		List<Candidate> empty = new List<Candidate>();
		if (i_candidates == null || i_candidates.Count == 0) return empty;
		System.Random random = new System.Random(i_seed);
		for (int attempt = 0; attempt < 80; attempt++)
		{
			int roll = random.Next(100);
			bool requiresHeavy = roll >= 60 && roll < 78;
			WeaponType[][] pattern = roll < 35
				? new[] { new[] { WeaponType.Pistol }, new[] { WeaponType.Smg, WeaponType.Shotgun, WeaponType.Rifle }, new[] { WeaponType.Pistol, WeaponType.Smg, WeaponType.Shotgun, WeaponType.Rifle } }
				: roll < 60
					? new[] { new[] { WeaponType.Shotgun, WeaponType.Rifle }, new[] { WeaponType.Shotgun, WeaponType.Rifle }, new[] { WeaponType.Pistol } }
					: roll < 78
						? new[] { new[] { WeaponType.Rifle }, new[] { WeaponType.Pistol, WeaponType.Smg } }
						: roll < 90
							? new[] { new[] { WeaponType.Shotgun }, new[] { WeaponType.Smg }, new[] { WeaponType.Pistol } }
							: new[] { new[] { WeaponType.Pistol, WeaponType.Smg }, new[] { WeaponType.Pistol, WeaponType.Smg } };

			List<Candidate> selected = TryPattern(i_candidates, pattern, random);
			if (selected.Count == pattern.Length && TotalWeight(selected) <= MaximumLoadoutWeight &&
				(!requiresHeavy || selected[0].Weight >= 4)) return selected;
		}

		List<Candidate> fallback = new List<Candidate>();
		Candidate pistol = FindRandom(i_candidates, new[] { WeaponType.Pistol }, fallback, random);
		if (pistol != null) fallback.Add(pistol);
		for (int i = 0; i < i_candidates.Count && fallback.Count < 3; i++)
		{
			Candidate candidate = i_candidates[(i + random.Next(i_candidates.Count)) % i_candidates.Count];
			if (!fallback.Contains(candidate) && TotalWeight(fallback) + candidate.Weight <= MaximumLoadoutWeight) fallback.Add(candidate);
		}
		return fallback;
	}

	private static List<Candidate> TryPattern(IList<Candidate> i_candidates, WeaponType[][] i_pattern, System.Random i_random)
	{
		List<Candidate> result = new List<Candidate>();
		foreach (WeaponType[] accepted in i_pattern)
		{
			Candidate candidate = FindRandom(i_candidates, accepted, result, i_random);
			if (candidate == null) return new List<Candidate>();
			result.Add(candidate);
		}
		return result;
	}

	private static Candidate FindRandom(IList<Candidate> i_candidates, WeaponType[] i_types, List<Candidate> i_excluded, System.Random i_random)
	{
		List<Candidate> pool = new List<Candidate>();
		foreach (Candidate candidate in i_candidates)
		{
			if (candidate == null || i_excluded.Contains(candidate)) continue;
			for (int i = 0; i < i_types.Length; i++)
				if (candidate.Type == i_types[i]) { pool.Add(candidate); break; }
		}
		return pool.Count == 0 ? null : pool[i_random.Next(pool.Count)];
	}

	private static int TotalWeight(IEnumerable<Candidate> i_candidates)
	{
		int total = 0;
		foreach (Candidate candidate in i_candidates) total += Mathf.Max(0, candidate.Weight);
		return total;
	}
}
