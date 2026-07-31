public class RaperDeathHound : RaperSmasher
{
	private StatusPlayerHudItem m_statusKnot;

	private void Knot()
	{
		m_statusKnot = CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().CreateAndAddStatus("Knot", m_npc.GetName() + " has forced his bulbus glandis inside of you", StatusPlayerHudItemColor.Rape);
		m_npc.GetAllInteractions()[1].Trigger(m_npc);
	}

	private void UnKnot()
	{
		CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().DestroyStatusItem(m_statusKnot);
	}
}
