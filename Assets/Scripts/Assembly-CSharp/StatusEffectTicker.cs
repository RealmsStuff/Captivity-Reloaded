using UnityEngine;

public abstract class StatusEffectTicker : StatusEffect
{
	private float m_ticksPerSec;

	private Coroutine m_coroutineTick;

	public StatusEffectTicker(string i_name, string i_source, TypeStatusEffect i_type, float i_duration, bool i_isStackable, float i_ticksPerSec)
		: base(i_name, i_source, i_type, i_duration, i_isStackable)
	{
		m_ticksPerSec = i_ticksPerSec;
	}

	public override void Activate()
	{
		base.Activate();
	}

	public abstract void Tick();

	public float GetTicksPerSec()
	{
		return m_ticksPerSec;
	}
}
