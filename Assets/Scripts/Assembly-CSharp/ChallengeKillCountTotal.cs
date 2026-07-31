using UnityEngine;

public class ChallengeKillCountTotal : Challenge
{
	[SerializeField]
	private int m_timesToKill;

	private int m_timesKilled;

	protected override void HandleActivation()
	{
		CommonReferences.Instance.GetPlayer().OnKill += OnKill;
		m_timesKilled = ManagerDB.GetKillsTotal();
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetPlayer().OnKill -= OnKill;
	}

	protected override void TrackCompletion()
	{
	}

	private void OnKill(Actor i_actor)
	{
		m_timesKilled++;
		if (m_timesKilled >= m_timesToKill)
		{
			Complete();
		}
	}
}
