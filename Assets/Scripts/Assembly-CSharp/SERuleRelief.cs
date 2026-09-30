public sealed class SERuleRelief : StatusEffectTicker
{
	private readonly float m_libidoPerTick;
	private readonly float m_pleasurePerTick;

	public SERuleRelief(string i_name, string i_source, float i_duration, float i_libidoTotal, float i_pleasureTotal)
		: base(i_name, i_source, TypeStatusEffect.Positive, i_duration, i_isStackable: true, 1f)
	{
		float ticks = UnityEngine.Mathf.Max(1f, i_duration);
		m_libidoPerTick = i_libidoTotal / ticks;
		m_pleasurePerTick = i_pleasureTotal / ticks;
	}

	public override void Tick()
	{
		Player player = (Player)m_actor;
		if (m_libidoPerTick > 0f) player.LoseLibido(m_libidoPerTick);
		if (m_pleasurePerTick > 0f) player.LosePleasure(m_pleasurePerTick);
	}
}
