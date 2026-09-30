public class Aspirin : Usable
{
	protected override bool HandleUse(bool i_isAltFire)
	{
		base.HandleUse(i_isAltFire);
		Player player = CommonReferences.Instance.GetPlayer();
		if (!ExternalRuleProfileFactory.TryGetConsumableRule("aspirin", out CaptivityReloaded.Modding.ConsumableRule rule))
		{
			player.RestoreHealth(35f);
			return true;
		}
		player.RestoreHealth(rule.HealthRestore ?? 35f);
		ApplyRelief(player, rule, GetName() + GetInstanceID());
		return true;
	}

	internal static void ApplyRelief(Player i_player, CaptivityReloaded.Modding.ConsumableRule i_rule, string i_source)
	{
		if (i_rule.EffectDurationSeconds > 0f && (i_rule.LibidoReduction > 0f || i_rule.PleasureReduction > 0f))
			i_player.ApplyStatusEffect(new SERuleRelief("Relief", i_source, i_rule.EffectDurationSeconds, i_rule.LibidoReduction, i_rule.PleasureReduction));
		else
		{
			if (i_rule.LibidoReduction > 0f) i_player.LoseLibido(i_rule.LibidoReduction);
			if (i_rule.PleasureReduction > 0f) i_player.LosePleasure(i_rule.PleasureReduction);
		}
	}
}
