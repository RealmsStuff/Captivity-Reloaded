public class XAILitigant : XAIWalker
{
	private Litigant m_litigant;

	private int m_chanceClaw = 20;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_litigant = (Litigant)i_npc;
	}
}
