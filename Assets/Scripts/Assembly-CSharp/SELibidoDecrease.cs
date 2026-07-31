public class SELibidoDecrease : StatusEffectTicker
{
	private float m_libidoDecreaseAmount;

	public SELibidoDecrease(string i_name, string i_source, TypeStatusEffect i_type, float i_duration, bool i_isStackable, float i_ticksPerSec, float i_amountLibidoDecreasePerTick)
		: base(i_name, i_source, i_type, i_duration, i_isStackable, i_ticksPerSec)
	{
		m_libidoDecreaseAmount = i_amountLibidoDecreasePerTick;
	}

	public override void Tick()
	{
		((Player)m_actor).LoseLibido(m_libidoDecreaseAmount);
	}
}
