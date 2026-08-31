using System.Collections.Generic;
using UnityEngine;

public class Stat
{
	private string m_name;

	private float m_valueBase;

	private List<StatModifier> m_modifiers = new List<StatModifier>();

	public Stat(string i_nameStat, float i_valueBase)
	{
		m_name = i_nameStat;
		m_valueBase = i_valueBase;
	}

	public Stat(StatNameActor i_nameStat, float i_valueBase)
	{
		m_name = i_nameStat.ToString();
		m_valueBase = i_valueBase;
	}

	public Stat(StatNamePlayer i_nameStat, float i_valueBase)
	{
		m_name = i_nameStat.ToString();
		m_valueBase = i_valueBase;
	}

	public float GetValueTotal()
	{
		float num = m_valueBase;
		foreach (StatModifier modifier in m_modifiers)
		{
			num += modifier.GetValueModification();
		}
		return num;
	}

	public float GetValueBase()
	{
		return m_valueBase;
	}

	public void SetValueBase(float i_valueBase)
	{
		m_valueBase = i_valueBase;
	}

	public StatModifier AddModifier(float i_valueModification)
	{
		StatModifier statModifier = new StatModifier(m_name, i_valueModification);
		m_modifiers.Add(statModifier);
		return statModifier;
	}

	public void RemoveModifier(StatModifier i_modifierToRemove)
	{
		foreach (StatModifier modifier in m_modifiers)
		{
			if (modifier == i_modifierToRemove)
			{
				m_modifiers.Remove(i_modifierToRemove);
				return;
			}
		}
		Debug.Log("Couldn't find statmodifier to remove: " + i_modifierToRemove);
	}

	public string GetName()
	{
		return m_name;
	}
}
