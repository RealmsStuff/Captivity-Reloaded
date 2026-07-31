public class XAIHunter : XAIWalker
{
	private Hunter m_hunter;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_hunter = (Hunter)i_npc;
	}
}
