using UnityEngine;

public class ChallengeRape : Challenge
{
	[SerializeField]
	private NPC m_npcRaper;

	[SerializeField]
	private int m_timesToRape;

	[SerializeField]
	private int m_numWaveToReach;

	private int m_timesRaped;

	private bool m_isReachWaveComplete;

	private bool m_isRapeComplete;

	protected override void HandleActivation()
	{
		CommonReferences.Instance.GetPlayer().OnBeingRaped += OnBeingRaped;
		CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.OnWaveStart += OnWaveStart;
		m_isRapeComplete = false;
		m_timesRaped = 0;
		if (m_numWaveToReach == 0)
		{
			m_isReachWaveComplete = true;
		}
		else
		{
			m_isReachWaveComplete = false;
		}
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetPlayer().OnBeingRaped -= OnBeingRaped;
		CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.OnWaveStart -= OnWaveStart;
	}

	protected override void TrackCompletion()
	{
		if (m_isRapeComplete && m_isReachWaveComplete)
		{
			Complete();
		}
	}

	private void OnBeingRaped()
	{
		if (!m_isRapeComplete && (bool)CommonReferences.Instance.GetPlayer().GetRaperCurrent() && CommonReferences.Instance.GetPlayer().GetRaperCurrent().GetNPC()
			.GetId() == m_npcRaper.GetId())
		{
			m_timesRaped++;
			if (m_timesRaped >= m_timesToRape)
			{
				m_isRapeComplete = true;
			}
		}
	}

	private void OnWaveStart()
	{
		if (!m_isReachWaveComplete && CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.GetNumWaveCurrent() >= m_numWaveToReach)
		{
			m_isReachWaveComplete = true;
		}
	}
}
