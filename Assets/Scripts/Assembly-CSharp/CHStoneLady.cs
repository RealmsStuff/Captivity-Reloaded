public class CHStoneLady : Challenge
{
	protected override void HandleActivation()
	{
		CommonReferences.Instance.GetPlayer().OnBirth += OnBirth;
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetPlayer().OnBirth -= OnBirth;
	}

	protected override void TrackCompletion()
	{
	}

	private void OnBirth(Actor i_actorChild)
	{
		if (i_actorChild.GetName() == "Altar Spirit")
		{
			Complete();
		}
	}
}
