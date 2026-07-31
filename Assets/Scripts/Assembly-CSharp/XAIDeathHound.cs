public class XAIDeathHound : XAIWalker
{
	private DeathHound m_deathHound;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_deathHound = (DeathHound)i_npc;
	}

	protected override void HandleCombat()
	{
		if (!m_deathHound.GetIsCharging())
		{
			base.HandleCombat();
		}
	}

	protected override void HandleStateChase()
	{
		base.HandleStateChase();
		if (m_deathHound.GetIsCanCharge() && !m_deathHound.GetIsCharging() && !m_deathHound.GetIsPreparingCharge() && m_deathHound.GetIsCloseEnoughToPrepareCharge())
		{
			m_deathHound.TryCharge();
		}
		else if (m_deathHound.GetIsCharging())
		{
			HandleCharge();
		}
	}

	private void HandleCharge()
	{
		if (m_deathHound.GetIsCloseEnoughToLeap())
		{
			m_deathHound.Leap();
		}
	}
}
