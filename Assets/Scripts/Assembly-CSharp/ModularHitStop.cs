using UnityEngine;

// The enemy animation event "hitStop": the game holds still for a few frames on a big hit (a slam, a haymaker), so
// the hit reads as heavy. It is Raper.CoroutineFreezeBeforeStartRape's freeze, far shorter and placed by the clip.
public sealed class ModularHitStop : MonoBehaviour
{
	// Longer than this stops reading as an impact and starts reading as a stutter.
	public const float MaximumSeconds = 0.25f;

	private static ModularHitStop s_instance;
	private float m_until;
	private bool m_holding;

	// Holds the game for i_seconds of real time. A hold already running is extended, never stacked. Nothing happens
	// while something else has the game stopped (the pause screen, a grab's opening freeze).
	public static void Hold(float i_seconds)
	{
		if (i_seconds <= 0f || !Application.isPlaying) return;
		bool holding = s_instance != null && s_instance.m_holding;
		if (!holding && Time.timeScale <= 0f) return;
		if (s_instance == null) s_instance = new GameObject("ModularHitStop").AddComponent<ModularHitStop>();
		s_instance.m_until = Mathf.Max(s_instance.m_until, Time.unscaledTime + Mathf.Min(i_seconds, MaximumSeconds));
		if (holding) return;
		s_instance.m_holding = true;
		Time.timeScale = 0f;
	}

	private void Update()
	{
		if (!m_holding || Time.unscaledTime < m_until) return;
		Release();
	}

	private void Release()
	{
		m_holding = false;
		// The pause screen owns the time scale while it is open.
		ScreenGame screen = CommonReferences.Instance?.GetManagerScreens()?.GetScreenGame();
		if (screen == null || !screen.IsPaused()) Time.timeScale = 1f;
	}

	private void OnDestroy()
	{
		if (m_holding) Release();
		if (s_instance == this) s_instance = null;
	}
}
