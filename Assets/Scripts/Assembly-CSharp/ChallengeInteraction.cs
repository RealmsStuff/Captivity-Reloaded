using UnityEngine;

public class ChallengeInteraction : Challenge
{
	[SerializeField]
	private Interaction m_interaction;

	[SerializeField]
	private int m_timesToInteract;

	[SerializeField]
	private int m_numWaveToReach;

	private int m_timesInteracted;

	private bool m_isInteractionComplete;

	private bool m_isReachWaveComplete;

	protected override void HandleActivation()
	{
		ManagerDB.OnInteraction += OnInteraction;
		CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.OnWaveStart += OnWaveStart;
		m_timesInteracted = 0;
		m_isInteractionComplete = false;
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
		ManagerDB.OnInteraction -= OnInteraction;
		CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.OnWaveStart -= OnWaveStart;
	}

	protected override void TrackCompletion()
	{
		if (m_isInteractionComplete && m_isReachWaveComplete)
		{
			Complete();
		}
	}

	private void OnInteraction(int i_id, NPC i_npc)
	{
		if (!m_isInteractionComplete && i_id == m_interaction.GetId())
		{
			m_timesInteracted++;
			if (m_timesInteracted >= m_timesToInteract)
			{
				m_isInteractionComplete = true;
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
