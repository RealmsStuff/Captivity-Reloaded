public class SELibidoIncrease : StatusEffectTicker
{
	private float m_libidoIncreaseAmount;

	public SELibidoIncrease(string i_name, string i_source, TypeStatusEffect i_type, float i_duration, bool i_isStackable, float i_ticksPerSec, float i_amountLibidoIncreasePerTick)
		: base(i_name, i_source, i_type, i_duration, i_isStackable, i_ticksPerSec)
	{
		m_libidoIncreaseAmount = i_amountLibidoIncreasePerTick;
	}

	public override void Tick()
	{
		((Player)m_actor).GainLibido(m_libidoIncreaseAmount);
	}
}
