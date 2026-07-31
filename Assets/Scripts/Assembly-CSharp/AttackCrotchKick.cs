using System.Collections.Generic;

public class AttackCrotchKick : AttackNPC
{
	private List<StatModifier> m_statModifiers = new List<StatModifier>();

	public override void HandleAttackStart()
	{
		base.HandleAttackStart();
		m_statModifiers.Add(m_npc.AddStatModifier("SpeedMax", 5f));
		m_statModifiers.Add(m_npc.AddStatModifier("Traction", 1f));
	}

	public override void HandleAttackPerform()
	{
		base.HandleAttackPerform();
		m_npc.RemoveStatModifier(m_statModifiers);
		m_statModifiers.Clear();
	}

	public override void HandleAttackEnd()
	{
		base.HandleAttackEnd();
		m_npc.RemoveStatModifier(m_statModifiers);
		m_statModifiers.Clear();
	}
}
