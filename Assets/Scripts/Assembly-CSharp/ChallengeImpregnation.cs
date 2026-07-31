using UnityEngine;

public class ChallengeImpregnation : ChallengeReachWave
{
	[SerializeField]
	private int m_timesToGetImpregnated;

	private int m_timesImpregnated;

	protected override void HandleActivation()
	{
		base.HandleActivation();
		CommonReferences.Instance.GetPlayer().OnFetusInsert += OnFetusInsert;
		m_timesImpregnated = 0;
	}

	protected override void HandleDeActivation()
	{
		base.HandleDeActivation();
		CommonReferences.Instance.GetPlayer().OnFetusInsert -= OnFetusInsert;
	}

	protected override void TrackCompletion()
	{
		base.TrackCompletion();
	}

	private void OnFetusInsert(Fetus i_fetusInserted)
	{
		m_timesImpregnated++;
		if (m_timesImpregnated >= m_timesToGetImpregnated && IsReachedWave())
		{
			Complete();
		}
	}

	protected override void OnWaveStart()
	{
		if (m_timesImpregnated >= m_timesToGetImpregnated)
		{
			base.OnWaveStart();
		}
	}
}
