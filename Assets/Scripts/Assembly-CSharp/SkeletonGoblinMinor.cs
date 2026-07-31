public class SkeletonGoblinMinor : SkeletonActor
{
	private void AnimEventFlyAttack()
	{
		GetComponentInParent<GoblinMinor>().FlyAttackRagdoll();
	}
}
