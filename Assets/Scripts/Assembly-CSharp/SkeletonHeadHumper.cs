using UnityEngine;

public class SkeletonHeadHumper : SkeletonActor
{
	[SerializeField]
	private AttackLeap m_attackLeap;

	public void AnimEventLeap()
	{
		m_attackLeap.Leap();
	}

	public void AnimEventEmptySack()
	{
		if ((bool)GetComponentInParent<HeadHumper>())
		{
			GetComponentInParent<HeadHumper>().EmptySack();
		}
	}
}
