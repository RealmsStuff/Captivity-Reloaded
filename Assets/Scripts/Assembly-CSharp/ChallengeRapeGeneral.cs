using UnityEngine;

public class ChallengeRapeGeneral : Challenge
{
	[SerializeField]
	private int m_timesToRape;

	private int m_timesRaped;

	protected override void HandleActivation()
	{
		CommonReferences.Instance.GetPlayer().OnBeingRaped += OnBeingRaped;
		m_timesRaped = ManagerDB.GetTimesRaped();
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetPlayer().OnBeingRaped -= OnBeingRaped;
	}

	protected override void TrackCompletion()
	{
	}

	private void OnBeingRaped()
	{
		m_timesRaped++;
		if (m_timesRaped >= m_timesToRape)
		{
			Complete();
		}
	}
}
