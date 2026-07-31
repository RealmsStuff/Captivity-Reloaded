using System.Collections.Generic;

public class AttackSpiderStab : AttackNPC
{
	private List<StatModifier> m_modifiers = new List<StatModifier>();

	public override void HandleAttackStart()
	{
		base.HandleAttackStart();
		m_npc.AddStatModifier("SpeedAccel", 1f);
		m_modifiers.Add(m_npc.AddStatModifier("SpeedMax", 5.5f));
	}

	public override void HandleAttackEnd()
	{
		base.HandleAttackEnd();
		m_npc.RemoveStatModifier(m_modifiers);
		m_modifiers.Clear();
	}
}
