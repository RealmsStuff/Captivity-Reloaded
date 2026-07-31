using UnityEngine;

public class CHNimbleLegs : ChallengeReachWave
{
	[SerializeField]
	private Interaction m_interactionBearTrap;

	private bool m_isSteppedOnTrap;

	protected override void HandleActivation()
	{
		base.HandleActivation();
		ManagerDB.OnInteraction += OnInteraction;
		m_isSteppedOnTrap = false;
	}

	protected override void HandleDeActivation()
	{
		base.HandleDeActivation();
		ManagerDB.OnInteraction -= OnInteraction;
	}

	protected override void TrackCompletion()
	{
	}

	private void OnInteraction(int i_idInteraction, NPC i_npc)
	{
		if (i_idInteraction == m_interactionBearTrap.GetId())
		{
			m_isSteppedOnTrap = true;
			DeActivate();
		}
	}

	protected override void OnWaveStart()
	{
		if (!m_isSteppedOnTrap)
		{
			base.OnWaveStart();
		}
	}
}
