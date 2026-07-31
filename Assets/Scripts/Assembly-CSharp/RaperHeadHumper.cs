public class RaperHeadHumper : RaperSmasher
{
	protected override void EndRape()
	{
		base.EndRape();
		if (!m_isLose)
		{
			HugHead();
		}
	}

	private void HugHead()
	{
		((HeadHumper)m_npc).HugHead();
	}
}
