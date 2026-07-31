public class XAITrapper : XAIWalker
{
	private Trapper m_trapper;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_trapper = (Trapper)i_npc;
	}

	protected override void HandleStateChase()
	{
		base.HandleStateChase();
		if (m_trapper.IsCanPlaceTrap() && !m_player.IsDead() && m_trapper.GetStateActorCurrent() != StateActor.Climbing && m_trapper.GetIsGroundedRayCast())
		{
			m_trapper.TryPlaceTrap();
		}
	}
}
