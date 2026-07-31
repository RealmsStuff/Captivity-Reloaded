public class XAIJacky : XAIAndroid
{
	private Jacky m_jacky;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_jacky = (Jacky)i_npc;
	}

	protected override void HandleCombat()
	{
		if (m_player.IsDead() || m_player.GetStateActorCurrent() == StateActor.Ragdoll || m_jacky.IsHasScaredAlready())
		{
			base.HandleCombat();
		}
	}

	public override void HandleIntelligence()
	{
		if (m_jacky.IsScaring())
		{
			return;
		}
		base.HandleIntelligence();
		if (m_jacky.IsHasScaredAlready() || m_jacky.GetStateActorCurrent() == StateActor.Climbing)
		{
			return;
		}
		if (m_player.GetIsBeingRaped())
		{
			m_jacky.StopAudioReverberation();
			return;
		}
		m_jacky.StartAudioReverberation();
		if (m_player.GetStateActorCurrent() != StateActor.Ragdoll && (!m_player.GetIsInvulnerable() || m_player.GetStatePlayerCurrent() == StatePlayer.Dashing))
		{
			TryScare();
		}
	}

	private void TryScare()
	{
		if (m_jacky.GetDistanceBetweenPlayerHips() <= m_jacky.GetDistanceCanScare() && m_jacky.GetPlatformCurrent() == m_player.GetPlatformCurrent())
		{
			m_jacky.Scare();
		}
	}
}
