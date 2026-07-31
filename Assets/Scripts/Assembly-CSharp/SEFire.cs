public class SEFire : StatusEffectTicker
{
	private float m_damagePerTick;

	public SEFire(string i_name, string i_source, TypeStatusEffect i_type, float i_duration, bool i_isStackable, float i_ticksPerSec, float i_damagePerTick)
		: base(i_name, i_source, i_type, i_duration, i_isStackable, i_ticksPerSec)
	{
		m_damagePerTick = i_damagePerTick;
	}

	public override void Tick()
	{
		if (!m_actor.IsDead())
		{
			m_actor.TakeDamage(m_damagePerTick);
		}
	}

	public float GetDamagePerTick()
	{
		return m_damagePerTick;
	}
}
