public class StatModifier
{
	private string m_nameStat;

	private float m_valueModification;

	public StatModifier(string i_nameStat, float i_valueModification)
	{
		m_nameStat = i_nameStat;
		m_valueModification = i_valueModification;
	}

	public StatModifier(StatNameActor i_nameStat, float i_valueModification)
	{
		m_nameStat = i_nameStat.ToString();
		m_valueModification = i_valueModification;
	}

	public StatModifier(StatNamePlayer i_nameStat, float i_valueModification)
	{
		m_nameStat = i_nameStat.ToString();
		m_valueModification = i_valueModification;
	}

	public string GetNameStat()
	{
		return m_nameStat;
	}

	public float GetValueModification()
	{
		return m_valueModification;
	}
}
