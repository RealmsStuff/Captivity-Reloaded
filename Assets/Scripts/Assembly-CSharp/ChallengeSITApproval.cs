public class ChallengeSITApproval : Challenge
{
	protected override void HandleActivation()
	{
		CommonReferences.Instance.GetManagerChallenge().OnChallengeComplete += OnChallengeComplete;
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetManagerChallenge().OnChallengeComplete -= OnChallengeComplete;
	}

	protected override void TrackCompletion()
	{
	}

	private void OnChallengeComplete(Challenge i_challenge)
	{
		bool flag = true;
		foreach (Challenge allChallenge in CommonReferences.Instance.GetManagerChallenge().GetAllChallenges())
		{
			if (allChallenge != this && allChallenge.GetState() == 0)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			Complete();
		}
	}

	protected override void Complete()
	{
		base.Complete();
	}
}
