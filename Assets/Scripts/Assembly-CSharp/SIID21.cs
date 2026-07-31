public class SIID21 : Usable
{
	protected override bool HandleUse(bool i_isAltFire)
	{
		base.HandleUse(i_isAltFire);
		CommonReferences.Instance.GetPlayer().Ragdoll(3f);
		CommonReferences.Instance.GetPlayer().BecomeInvulnerable("SIID21 Invulnerability", "SIID21", 20f, i_isAffectSkeleton: true);
		CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().CreateAndAddStatus("SIID-21 Invulnerability", "Invulnerable for 20 secs", StatusPlayerHudItemColor.Buff, 20f);
		return true;
	}
}
