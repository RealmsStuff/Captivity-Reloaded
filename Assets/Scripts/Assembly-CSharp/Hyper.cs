public class Hyper : Usable
{
	protected override bool HandleUse(bool i_isAltFire)
	{
		base.HandleUse(i_isAltFire);
		StatusEffectStatModifier statusEffectStatModifier = new StatusEffectStatModifier("Hyper", GetName() + base.gameObject.GetInstanceID(), TypeStatusEffect.Positive, 30f, i_isStackable: false);
		statusEffectStatModifier.AddStatModification(StatNameActor.SpeedAccel, 8f);
		statusEffectStatModifier.AddStatModification(StatNamePlayer.SpeedSprint, 8f);
		statusEffectStatModifier.AddStatModification(StatNamePlayer.PowerDash, 20f);
		return CommonReferences.Instance.GetPlayer().ApplyStatusEffect(statusEffectStatModifier);
	}
}
