public class AttackSyringe : AttackNPC
{
	public override void HandleAttackHit()
	{
		base.HandleAttackHit();
		GetComponentInParent<GoblinMinor>().StickNeedle();
	}
}
