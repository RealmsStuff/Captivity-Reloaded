public class Wardrobe : Interactable
{
	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (CommonReferences.Instance.GetPlayer().GetStatePlayerCurrent() != StatePlayer.Dashing)
		{
			CommonReferences.Instance.GetManagerHud().ShowWardrobeHud();
		}
	}
}
