using UnityEngine;

public class ChallengeBirthTotal : Challenge
{
	[SerializeField]
	private int m_timesToBirth;

	protected override void HandleActivation()
	{
		CommonReferences.Instance.GetPlayer().OnBirthEnd += OnPlayerBirth;
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetPlayer().OnBirthEnd -= OnPlayerBirth;
	}

	protected override void TrackCompletion()
	{
	}

	private async void OnPlayerBirth()
	{
		if (await ManagerDB.GetTimesBirth() >= m_timesToBirth)
		{
			Complete();
		}
	}
}
