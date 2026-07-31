public class RaperMusca : RaperSmasher
{
	protected override void EndRape()
	{
		if (!m_isLose)
		{
			Carry();
		}
		else
		{
			base.EndRape();
		}
	}

	private void Carry()
	{
		((Musca)m_npc).CarryPlayer();
	}

	public void StopCarrying()
	{
		base.EndRape();
	}
}
