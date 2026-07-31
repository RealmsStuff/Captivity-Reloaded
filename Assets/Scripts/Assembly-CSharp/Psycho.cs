public class Psycho : Usable
{
	protected override bool HandleUse(bool i_isAltFire)
	{
		base.HandleUse(i_isAltFire);
		StatusEffectStatModifier statusEffectStatModifier = new StatusEffectStatModifier("Psycho", GetName() + GetInstanceID(), TypeStatusEffect.Positive, 9999f, i_isStackable: false);
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
