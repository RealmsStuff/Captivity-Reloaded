using UnityEngine;

public class ChallengeReachWave : Challenge
{
	[SerializeField]
	private int m_numWaveToReach;

	protected override void HandleActivation()
	{
		CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.OnWaveStart += OnWaveStart;
	}

	protected override void TrackCompletion()
	{
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.OnWaveStart -= OnWaveStart;
	}

	protected virtual void OnWaveStart()
	{
		if (IsReachedWave())
		{
			Complete();
		}
	}

	protected bool IsReachedWave()
	{
		if (CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.GetNumWaveCurrent() >= m_numWaveToReach)
		{
			return true;
		}
		return false;
	}
}
