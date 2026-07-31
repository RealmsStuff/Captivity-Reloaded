public class CHInsectsest : Challenge
{
	protected override void HandleActivation()
	{
		CommonReferences.Instance.GetPlayer().OnFetusInsert += OnFetusInsert;
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetPlayer().OnFetusInsert -= OnFetusInsert;
	}

	protected override void TrackCompletion()
	{
	}

	private void OnFetusInsert(Fetus i_fetusInserted)
	{
		if (!(CommonReferences.Instance.GetPlayer().GetRaperCurrent() == null) && CommonReferences.Instance.GetPlayer().GetRaperCurrent().GetNPC()
			.IsPlayerParent())
		{
			Complete();
		}
	}
}
