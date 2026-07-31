using UnityEngine;

public class LockActivator : Interactable
{
	[SerializeField]
	private PickUpable m_keyNeeded;

	[SerializeField]
	private Interactable m_interactableToActivate;

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (!m_isActivatedOnceAlready)
		{
			if (CommonReferences.Instance.GetPlayer().GetIsHasPickUpable(m_keyNeeded))
			{
				CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification("Activated digital lock with " + m_keyNeeded.GetName(), ColorTextNotification.UnlockDoor, i_isContinues: false);
				m_isActivatedOnceAlready = true;
				m_interactableToActivate.Activate(i_initiator, InteractableActivationType.Use);
			}
			else
			{
				CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification("Requires " + m_keyNeeded.GetName(), ColorTextNotification.Other, i_isContinues: false);
			}
		}
	}
}
