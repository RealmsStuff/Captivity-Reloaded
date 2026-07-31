using System.Collections.Generic;

public class AttackMusca : AttackNPC
{
	private List<StatModifier> m_modifiers = new List<StatModifier>();

	public override void HandleAttackStart()
	{
		base.HandleAttackStart();
		m_modifiers.Add(m_npc.AddStatModifier("SpeedMax", 3f));
		m_modifiers.Add(m_npc.AddStatModifier("SpeedAccel", 2f));
	}

	public override void HandleAttackPerform()
	{
		base.HandleAttackPerform();
		m_npc.RemoveStatModifier(m_modifiers);
		m_modifiers.Clear();
	}
}
