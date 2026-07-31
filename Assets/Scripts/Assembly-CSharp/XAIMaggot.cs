public class XAIMaggot : XAIWalker
{
	private Maggot m_maggot;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_maggot = (Maggot)i_npc;
		m_radiusCompleteDestination = 2f;
	}
}
