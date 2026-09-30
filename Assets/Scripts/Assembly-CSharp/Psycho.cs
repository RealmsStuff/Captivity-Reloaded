public class Psycho : Usable
{
	protected override bool HandleUse(bool i_isAltFire)
	{
		base.HandleUse(i_isAltFire);
		float duration = ExternalRuleProfileFactory.TryGetConsumableRule("psycho", out CaptivityReloaded.Modding.ConsumableRule rule)
			? rule.EffectDurationSeconds : 9999f;
		StatusEffectStatModifier statusEffectStatModifier = new StatusEffectStatModifier("Psycho", GetName() + GetInstanceID(), TypeStatusEffect.Positive, duration, i_isStackable: false);
		statusEffectStatModifier.AddStatModification(StatNamePlayer.DamageMultiplierGun, 0.5f);
		statusEffectStatModifier.AddPlayerStatusHudItem("Psycho", "+50% damage", StatusPlayerHudItemColor.Buff);
		bool num = CommonReferences.Instance.GetPlayer().ApplyStatusEffect(statusEffectStatModifier);
		if (num)
		{
			CommonReferences.Instance.GetManagerPostProcessing().PlayEffectPsycho();
		}
		return num;
	}
}
