public class XAIMarksman : XAIWalker
{
	private Marksman m_marksman;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_marksman = (Marksman)i_npc;
	}

	protected override void HandleStateChase()
	{
		if (m_marksman.IsHasLineOfSightToPlayer() && m_player.GetStateActorCurrent() != StateActor.Ragdoll && !m_marksman.IsShooting() && m_marksman.IsCanShoot() && m_npc.GetDistanceBetweenPlayer() <= m_marksman.GetDistanceToShootFrom())
		{
			m_marksman.Shoot();
		}
		base.HandleStateChase();
	}
}
