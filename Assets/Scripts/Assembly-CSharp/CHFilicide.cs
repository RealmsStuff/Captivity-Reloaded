using UnityEngine;

public class CHFilicide : Challenge
{
	[SerializeField]
	private int m_timesToKillOwnChildren;

	private int m_timesKilledOwnChildren;

	protected override void HandleActivation()
	{
		CommonReferences.Instance.GetPlayer().OnKill += OnKill;
		m_timesKilledOwnChildren = 0;
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetPlayer().OnKill -= OnKill;
	}

	protected override void TrackCompletion()
	{
	}

	private void OnKill(NPC i_npcKilled)
	{
		if (i_npcKilled.IsPlayerParent())
		{
			m_timesKilledOwnChildren++;
			if (m_timesKilledOwnChildren >= m_timesToKillOwnChildren)
			{
				Complete();
			}
		}
	}
}
