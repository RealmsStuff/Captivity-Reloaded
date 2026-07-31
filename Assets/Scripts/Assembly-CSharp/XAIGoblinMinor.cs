public class XAIGoblinMinor : XAIWalker
{
	private GoblinMinor m_goblinMinor;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_goblinMinor = (GoblinMinor)i_npc;
	}
}
