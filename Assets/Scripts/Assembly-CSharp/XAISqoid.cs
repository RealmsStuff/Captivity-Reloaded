public class XAISqoid : XAIFlier
{
	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_radiusCompleteDestination = 0.5f;
	}

	protected override void HandleCombat()
	{
		if (m_player.GetStateActorCurrent() != StateActor.Ragdoll && !m_player.IsExposing())
		{
			base.HandleCombat();
		}
	}

	protected override void HandleStateChase()
	{
		base.HandleStateChase();
		if ((m_player.GetStateActorCurrent() == StateActor.Ragdoll || m_player.IsExposing()) && CheckIfCloseEnoughToRape() && m_player.GetIsCanBeRaped())
		{
			m_npc.StartRape();
		}
	}

	private bool CheckIfCloseEnoughToRape()
	{
		if (m_npc.GetDistanceBetweenPlayerHips() < 1f)
		{
			return true;
		}
		return false;
	}
}
