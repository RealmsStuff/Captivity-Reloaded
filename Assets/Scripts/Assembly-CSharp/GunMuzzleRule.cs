using UnityEngine;

// Whether the player's shots start at the muzzle (MuzzleShot) or, as the game always had it, at the shoulder.
// A saved option, on unless the player switched it off in the options.
public static class GunMuzzleRule
{
	public const string PreferenceKey = "MuzzleRule";

	private static bool s_isRead;
	private static bool s_isEnabled = true;

	public static bool Enabled
	{
		get
		{
			if (!s_isRead)
			{
				s_isRead = true;
				s_isEnabled = PlayerPrefs.GetInt(PreferenceKey, 1) == 1;
			}
			return s_isEnabled;
		}
		set
		{
			s_isRead = true;
			s_isEnabled = value;
			PlayerPrefs.SetInt(PreferenceKey, value ? 1 : 0);
		}
	}
}
