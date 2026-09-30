using UnityEngine;

public class SIID21 : Usable
{
	protected override bool HandleUse(bool i_isAltFire)
	{
		base.HandleUse(i_isAltFire);
		Player player = CommonReferences.Instance.GetPlayer();
		if (!ExternalRuleProfileFactory.TryGetConsumableRule("siid-21", out CaptivityReloaded.Modding.ConsumableRule rule))
		{
			player.Ragdoll(3f);
			player.BecomeInvulnerable("SIID21 Invulnerability", "SIID21", 20f, i_isAffectSkeleton: true);
			CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().CreateAndAddStatus("SIID-21 Invulnerability", "Invulnerable for 20 secs", StatusPlayerHudItemColor.Buff, 20f);
			return true;
		}
		float duration = rule.InvulnerabilitySeconds;
		player.Ragdoll(Mathf.Min(1f, duration));
		if (duration > 0f) player.BecomeInvulnerable("SIID21 Invulnerability", "SIID21", duration, i_isAffectSkeleton: true);
		StatusEffectStatModifier boost = new StatusEffectStatModifier("SIID-21 Serum", GetName() + GetInstanceID(), TypeStatusEffect.Positive, Mathf.Max(0.1f, rule.EffectDurationSeconds), i_isStackable: false);
		if (rule.GunDamageBonus != 0f) boost.AddStatModification(StatNamePlayer.DamageMultiplierGun, rule.GunDamageBonus);
		if (rule.AccelerationBonus != 0f) boost.AddStatModification(StatNameActor.SpeedAccel, rule.AccelerationBonus);
		if (rule.SprintBonus != 0f) boost.AddStatModification(StatNamePlayer.SpeedSprint, rule.SprintBonus);
		if (rule.DashBonus != 0f) boost.AddStatModification(StatNamePlayer.PowerDash, rule.DashBonus);
		player.ApplyStatusEffect(boost);
		return true;
	}
}
