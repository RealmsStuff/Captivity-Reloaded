using UnityEngine;

public class ChallengeMindBrokenCountTotal : Challenge
{
	[SerializeField]
	private int m_timesToMindBreak;

	private int m_timesMindBroken;

	protected override void HandleActivation()
	{
		CommonReferences.Instance.GetPlayer().OnDie += OnDie;
		m_timesMindBroken = ManagerDB.GetTimesMindBroken();
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetPlayer().OnDie -= OnDie;
	}

	protected override void TrackCompletion()
	{
	}

	private void OnDie()
	{
		m_timesMindBroken++;
		if (m_timesMindBroken >= ExternalRuleProfileFactory.MindBreakTarget(m_timesToMindBreak))
		{
			Complete();
		}
	}
}
