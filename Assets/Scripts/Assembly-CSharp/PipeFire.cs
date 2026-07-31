public class PipeFire : Interactable
{
	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		i_initiator.Ignite("pipefire");
		if (i_initiator is Player && (bool)GetHeadHumperAttachedToPlayerHead())
		{
			HeadHumper headHumperAttachedToPlayerHead = GetHeadHumperAttachedToPlayerHead();
			headHumperAttachedToPlayerHead.Ignite("pipefire");
			headHumperAttachedToPlayerHead.LetGoOfHead();
			headHumperAttachedToPlayerHead.ScreechIgnite();
		}
	}

	private HeadHumper GetHeadHumperAttachedToPlayerHead()
	{
		return CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Head)
			.GetComponentInChildren<HeadHumper>();
	}
}
