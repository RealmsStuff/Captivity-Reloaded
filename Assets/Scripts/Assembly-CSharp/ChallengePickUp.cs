using UnityEngine;

public class ChallengePickUp : Challenge
{
	[SerializeField]
	private PickUpable m_pickUpable;

	protected override void HandleActivation()
	{
		CommonReferences.Instance.GetPlayer().OnPickUp += OnPickUp;
	}

	protected override void TrackCompletion()
	{
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetPlayer().OnPickUp -= OnPickUp;
	}

	private void OnPickUp(PickUpable i_pickUpable)
	{
		if (i_pickUpable.GetName() == m_pickUpable.GetName())
		{
			CommonReferences.Instance.GetPlayer().OnPickUp -= OnPickUp;
			Complete();
		}
	}
}
