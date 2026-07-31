using UnityEngine;

public class ChallengeLitreCumTakenTotal : Challenge
{
	[SerializeField]
	private int m_litreCumToTake;

	protected override void HandleActivation()
	{
		CommonReferences.Instance.GetPlayer().OnRapeEnd += CheckLitreCumTaken;
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetPlayer().OnRapeEnd -= CheckLitreCumTaken;
	}

	protected override void TrackCompletion()
	{
	}

	private async void CheckLitreCumTaken()
	{
		if (await ManagerDB.GetLitreCumTaken() >= (decimal)m_litreCumToTake)
		{
			Complete();
		}
	}
}
