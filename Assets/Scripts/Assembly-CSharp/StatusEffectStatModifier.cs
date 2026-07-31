using System.Collections.Generic;

public class StatusEffectStatModifier : StatusEffect
{
	protected List<StatModifier> m_statModifiers = new List<StatModifier>();

	protected List<StatModifier> m_statModifiersAddedToPlayer = new List<StatModifier>();

	public StatusEffectStatModifier(string i_name, string i_source, TypeStatusEffect i_type, float i_duration, bool i_isStackable)
		: base(i_name, i_source, i_type, i_duration, i_isStackable)
	{
	}

	public override void Activate()
	{
		base.Activate();
		foreach (StatModifier statModifier in m_statModifiers)
		{
			m_statModifiersAddedToPlayer.Add(CommonReferences.Instance.GetPlayer().AddStatModifier(statModifier.GetNameStat(), statModifier.GetValueModification()));
		}
	}

	public override void End()
	{
		base.End();
		CommonReferences.Instance.GetPlayer().RemoveStatModifier(m_statModifiersAddedToPlayer);
		m_statModifiersAddedToPlayer.Clear();
	}

	public void AddStatModification(string i_nameStat, float i_valueModification)
	{
		m_statModifiers.Add(new StatModifier(i_nameStat, i_valueModification));
	}

	public void AddStatModification(StatNameActor i_nameStat, float i_valueModification)
	{
		m_statModifiers.Add(new StatModifier(i_nameStat, i_valueModification));
	}

	public void AddStatModification(StatNamePlayer i_nameStat, float i_valueModification)
	{
		m_statModifiers.Add(new StatModifier(i_nameStat, i_valueModification));
	}

	public List<StatModifier> GetStatModifiers()
	{
		return m_statModifiers;
	}
}
