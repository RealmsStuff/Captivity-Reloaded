public class Morphine : Usable
{
	protected override bool HandleUse(bool i_isAltFire)
	{
		Player player = CommonReferences.Instance.GetPlayer();
		if (!ExternalRuleProfileFactory.TryGetConsumableRule("morphine", out CaptivityReloaded.Modding.ConsumableRule rule))
		{
			player.RestoreHealth(100f);
			return true;
		}
		player.RestoreHealth(rule.HealthRestore ?? 100f);
		Aspirin.ApplyRelief(player, rule, GetName() + GetInstanceID());
		if (rule.RestoreHeart) player.RestoreAHeart();
		return true;
	}
}
