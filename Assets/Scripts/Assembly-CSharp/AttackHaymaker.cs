public class AttackHaymaker : AttackNPC
{
	public override void HandleAttackPerform()
	{
		base.HandleAttackPerform();
		((Abby)m_npc).RunAway();
	}

	public override void HandleAttackEnd()
	{
		base.HandleAttackEnd();
	}
}
